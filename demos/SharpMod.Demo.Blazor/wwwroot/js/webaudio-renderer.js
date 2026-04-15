// ═══════════════════════════════════════════════════════════════
// SharpMod v12 — Vrais oscillos + VU par canal
// Tout encodé dans le header de FillBuffer
// UN SEUL appel interop
// ═══════════════════════════════════════════════════════════════

window.SharpModAudio = {
    audioContext: null,
    scriptNode: null,
    analyser: null,
    dotnetRef: null,
    isPlaying: false,
    sampleRate: 44100,
    bufferSize: 2048,
    fftData: null,

    songPosition: 0,
    patternNumber: 0,
    patternPosition: 0,

    // Vrais scope data par canal (remplis depuis le header C#)
    channelCount: 0,
    vuLevels: null,       // Float64Array — VU 0.0-1.0
    scopeData: null,      // Array de Float32Array — oscillos -1..+1

    SCOPE_SIZE: 128,

    initialize: function () {
        if (!this.audioContext) {
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)({
                sampleRate: this.sampleRate
            });
            this.analyser = this.audioContext.createAnalyser();
            this.analyser.fftSize = 256;
            this.analyser.smoothingTimeConstant = 0.8;
            this.fftData = new Uint8Array(this.analyser.frequencyBinCount);
        }
    },

    setDotNetReference: function (dotnetRef) {
        this.dotnetRef = dotnetRef;
    },

    play: function () {
        if (!this.audioContext || !this.dotnetRef) return;
        if (this.audioContext.state === 'suspended') {
            this.audioContext.resume();
        }

        // Toujours recréer le scriptNode (évite les rattrapages de buffer)
        if (this.scriptNode) {
            this.scriptNode.disconnect();
            this.scriptNode = null;
        }

        this.scriptNode = this.audioContext.createScriptProcessor(
            this.bufferSize, 0, 2
        );
        var self = this;

        this.scriptNode.onaudioprocess = function (event) {
            if (!self.isPlaying || !self.dotnetRef) {
                event.outputBuffer.getChannelData(0).fill(0);
                event.outputBuffer.getChannelData(1).fill(0);
                return;
            }

            var audioByteCount = self.bufferSize * 4;
            var rawBytes = self.dotnetRef.invokeMethod('FillBuffer', audioByteCount);

            var left = event.outputBuffer.getChannelData(0);
            var right = event.outputBuffer.getChannelData(1);

            if (!rawBytes || rawBytes.length < 16) {
                left.fill(0);
                right.fill(0);
                return;
            }

            var view = new DataView(new Uint8Array(rawBytes).buffer);

            // ── Header fixe (16 bytes) ──
            self.songPosition = view.getInt32(0, true);
            self.patternNumber = view.getInt32(4, true);
            self.patternPosition = view.getInt32(8, true);
            var chCount = view.getInt32(12, true);

            if (chCount !== self.channelCount) {
                self.channelCount = chCount;
                self.vuLevels = new Float64Array(chCount);
                self.scopeData = [];
                for (var i = 0; i < chCount; i++) {
                    self.scopeData.push(new Float32Array(self.SCOPE_SIZE));
                }
            }

            var headerSize = 16 + chCount + (chCount * self.SCOPE_SIZE);

            // ── VU levels ──
            var vuOffset = 16;
            for (var ch = 0; ch < chCount; ch++) {
                self.vuLevels[ch] = rawBytes[vuOffset + ch] / 255.0;
            }

            // ── Scope data ──
            var scopeOffset = vuOffset + chCount;
            for (var ch = 0; ch < chCount; ch++) {
                var buf = self.scopeData[ch];
                var off = scopeOffset + ch * self.SCOPE_SIZE;
                for (var i = 0; i < self.SCOPE_SIZE; i++) {
                    var b = rawBytes[off + i];
                    buf[i] = (b < 128 ? b : b - 256) / 128.0;
                }
            }

            // ── Audio PCM ──
            var bufSize = self.bufferSize;
            for (var i = 0; i < bufSize; i++) {
                var offset = headerSize + i * 4;
                if (offset + 3 < rawBytes.length) {
                    left[i] = view.getInt16(offset, true) / 32768.0;
                    right[i] = view.getInt16(offset + 2, true) / 32768.0;
                } else {
                    left[i] = 0;
                    right[i] = 0;
                }
            }
        };

        this.scriptNode.connect(this.analyser);
        this.analyser.connect(this.audioContext.destination);
        this.isPlaying = true;
    },

    stop: function () {
        this.isPlaying = false;
        if (this.scriptNode) {
            this.scriptNode.disconnect();
            this.scriptNode = null;
        }
    },

    pause: function () {
        if (this.isPlaying) {
            // PAUSE : couper le son proprement
            this.isPlaying = false;
            if (this.scriptNode) {
                this.scriptNode.disconnect();
                this.scriptNode = null;
            }
        } else {
            // RESUME : recréer le pipeline audio from scratch
            this.play();
        }
    },
   

    updateFFT: function () {
        if (!this.analyser || !this.isPlaying) return false;
        this.analyser.getByteFrequencyData(this.fftData);
        return true;
    }
};

// ═══════════════════════════════════
// VISUALS — boucle unique RAF
// ═══════════════════════════════════

var _ft2AnimId = null;
var _ft2ActiveRowEl = null;
var _ft2ScrollRow = 0;
var _ft2LastPatNum = -1;

var _ft2VuPeaksSmooth = null; // Pour le decay visuel
var _ft2VuDecay = 0.92;
var _ft2ScopeSmooth = null;

var _ft2FFTCanvas = null;
var _ft2FFTCtx = null;
var _ft2FFTw = 0;
var _ft2FFTh = 0;

var _ft2ScopeCanvas = null;
var _ft2ScopeCtx = null;
var _ft2ScopeW = 0;
var _ft2ScopeH = 0;

var _ft2PatternGrid = null;
var _ft2RowHeight = 14;
var _ft2TotalRows = 64;

// ft2InitScopes n'est plus nécessaire — les buffers sont auto-créés
// dans onaudioprocess quand channelCount change. On le garde pour compat.
window.ft2InitScopes = function (channelCount) {
    // No-op — auto-init dans onaudioprocess
};

window.ft2StartAll = function (fftCanvasId, scopeCanvasId, patternGridId,
    rowHeight, totalRows) {
    _ft2FFTCanvas = document.getElementById(fftCanvasId);
    if (_ft2FFTCanvas) {
        _ft2FFTCtx = _ft2FFTCanvas.getContext('2d');
        function resizeFFT() {
            _ft2FFTw = _ft2FFTCanvas.width = _ft2FFTCanvas.clientWidth;
            _ft2FFTh = _ft2FFTCanvas.height = _ft2FFTCanvas.clientHeight;
        }
        resizeFFT();
        new ResizeObserver(resizeFFT).observe(_ft2FFTCanvas);
    }

    _ft2ScopeCanvas = document.getElementById(scopeCanvasId);
    if (_ft2ScopeCanvas) {
        _ft2ScopeCtx = _ft2ScopeCanvas.getContext('2d');
        function resizeScope() {
            _ft2ScopeW = _ft2ScopeCanvas.width = _ft2ScopeCanvas.clientWidth;
            _ft2ScopeH = _ft2ScopeCanvas.height = _ft2ScopeCanvas.clientHeight;
        }
        resizeScope();
        new ResizeObserver(resizeScope).observe(_ft2ScopeCanvas);
    }

    _ft2PatternGrid = document.getElementById(patternGridId);
    _ft2RowHeight = rowHeight || 14;
    _ft2TotalRows = totalRows || 64;
    _ft2ScrollRow = 0;
    _ft2LastPatNum = -1;

    if (_ft2AnimId) cancelAnimationFrame(_ft2AnimId);

    function mainLoop() {
        _ft2AnimId = requestAnimationFrame(mainLoop);
        drawFFT();
        drawScopes();
        updatePattern();
    }
    mainLoop();
};

window.ft2StopAll = function () {
    if (_ft2AnimId) {
        cancelAnimationFrame(_ft2AnimId);
        _ft2AnimId = null;
    }
    if (_ft2ActiveRowEl) {
        _ft2ActiveRowEl.classList.remove('active');
        _ft2ActiveRowEl = null;
    }
};

window.ft2UpdatePatternGrid = function (patternGridId, totalRows) {
    _ft2PatternGrid = document.getElementById(patternGridId);
    _ft2TotalRows = totalRows || 64;
};

// ══════════════════════════════
// Scroll horizontal synchro
// Headers + Scopes suivent le pattern grid
// ══════════════════════════════
var _ft2ScrollSyncBound = false;

window.ft2SyncScrollInit = function (gridId, headersInnerId, scopeCanvasId) {
    var grid = document.getElementById(gridId);
    var headersInner = document.getElementById(headersInnerId);
    var scopeWrap = document.getElementById('scopeCanvasWrap');

    if (!grid) return;
    if (grid._ft2ScrollBound) return;
    grid._ft2ScrollBound = true;

    grid.addEventListener('scroll', function () {
        var scrollLeft = grid.scrollLeft;
        if (headersInner) {
            headersInner.style.transform = 'translateX(-' + scrollLeft + 'px)';
        }
        if (scopeWrap) {
            scopeWrap.style.transform = 'translateX(-' + scrollLeft + 'px)';
        }
    });
};

// ──────────────
// FFT
// ──────────────
function drawFFT() {
    if (!_ft2FFTCtx || _ft2FFTw === 0) return;

    var ctx = _ft2FFTCtx;
    var w = _ft2FFTw;
    var h = _ft2FFTh;

    if (!window.SharpModAudio.updateFFT()) {
        ctx.fillStyle = '#0B0E14';
        ctx.fillRect(0, 0, w, h);
        return;
    }

    var data = window.SharpModAudio.fftData;
    ctx.fillStyle = '#0B0E14';
    ctx.fillRect(0, 0, w, h);

    var barCount = Math.min(data.length, 64);
    // Utiliser la largeur RÉELLE (float) pour couvrir tout le canvas
    var barWidth = w / barCount;
    var gap = Math.max(1, barWidth * 0.15); // 15% de gap entre les barres

    for (var i = 0; i < barCount; i++) {
        var val = data[i] / 255.0;
        var barH = val * h;
        var x = i * barWidth;
        var r, g;
        if (val < 0.5) { r = (val * 384) | 0; g = 192; }
        else { r = 192; g = ((1 - (val - 0.5) * 2) * 192) | 0; }
        ctx.fillStyle = 'rgb(' + r + ',' + g + ',40)';
        ctx.fillRect(x + gap * 0.5, h - barH, barWidth - gap, barH);
    }
}

// ──────────────
// Scopes + VU — VRAIS DONNÉES PAR CANAL
// ──────────────
function drawScopes() {
    var audio = window.SharpModAudio;
    if (!_ft2ScopeCtx || !audio.scopeData || audio.channelCount === 0) return;
    if (_ft2ScopeW === 0 || _ft2ScopeH === 0) return;

    var ctx = _ft2ScopeCtx;
    var w = _ft2ScopeW;
    var h = _ft2ScopeH;
    var count = audio.channelCount;

    // ══════════════════════════════════════════════════
    // Lire les positions réelles des cellules du pattern
    // pour aligner les scopes exactement en face
    // ══════════════════════════════════════════════════
    var cellPositions = null; // [{x, w}, ...] en coordonnées canvas
    var scopeCanvas = document.getElementById('scopesCanvas');
    var patternGrid = document.getElementById('patternGrid');

    if (scopeCanvas && patternGrid) {
        var firstRow = patternGrid.querySelector('.ft2-row');
        if (firstRow) {
            var cells = firstRow.querySelectorAll('.ft2-cell');
            if (cells.length >= count) {
                var canvasRect = scopeCanvas.getBoundingClientRect();
                var scaleX = w / canvasRect.width; // ratio CSS pixels → canvas pixels

                cellPositions = [];
                for (var i = 0; i < count; i++) {
                    var cellRect = cells[i].getBoundingClientRect();
                    cellPositions.push({
                        x: (cellRect.left - canvasRect.left) * scaleX,
                        w: cellRect.width * scaleX
                    });
                }
            }
        }
    }

    // Fallback si pas de pattern chargé
    if (!cellPositions) {
        cellPositions = [];
        var cellW = w / count;
        for (var i = 0; i < count; i++) {
            cellPositions.push({ x: i * cellW, w: cellW });
        }
    }

    var vuW = 6;
    var halfH = h * 0.5;
    var ampH = halfH * 0.8;
    var segH = ((h - 2) / 8) | 0;

    // ── Init smooth buffers ──
    if (!_ft2VuPeaksSmooth || _ft2VuPeaksSmooth.length !== count) {
        _ft2VuPeaksSmooth = new Float64Array(count);
    }
    if (!_ft2ScopeSmooth || _ft2ScopeSmooth.length !== count) {
        _ft2ScopeSmooth = [];
        for (var i = 0; i < count; i++) {
            _ft2ScopeSmooth.push(new Float32Array(audio.SCOPE_SIZE));
        }
    }

    // ── Smooth scope data ──
    for (var ch = 0; ch < count; ch++) {
        var src = audio.scopeData[ch];
        var dst = _ft2ScopeSmooth[ch];
        if (!src || !dst) continue;

        var maxAbs = 0;
        for (var i = 0; i < src.length; i++) {
            var a = src[i]; if (a < 0) a = -a;
            if (a > maxAbs) maxAbs = a;
        }

        if (maxAbs > 0.001) {
            for (var i = 0; i < src.length; i++) dst[i] = src[i];
        } else {
            var allZero = true;
            for (var i = 0; i < dst.length; i++) {
                dst[i] *= 0.85;
                if (dst[i] > 0.001 || dst[i] < -0.001) allZero = false;
            }
            if (allZero) {
                for (var i = 0; i < dst.length; i++) dst[i] = 0;
            }
        }
    }

    // ── Smooth VU peaks ──
    for (var ch = 0; ch < count; ch++) {
        var raw = audio.vuLevels ? audio.vuLevels[ch] : 0;
        if (raw > _ft2VuPeaksSmooth[ch]) {
            _ft2VuPeaksSmooth[ch] = raw;
        } else {
            _ft2VuPeaksSmooth[ch] *= 0.96;
            if (_ft2VuPeaksSmooth[ch] < 0.01) _ft2VuPeaksSmooth[ch] = 0;
        }
    }

    // ── Background ──
    ctx.fillStyle = '#0B0E14';
    ctx.fillRect(0, 0, w, h);

    // ── Bordures entre canaux ──
    ctx.strokeStyle = '#1A1F2E';
    ctx.lineWidth = 1;
    ctx.beginPath();
    for (var ch = 1; ch < count; ch++) {
        var cp = cellPositions[ch];
        ctx.moveTo(cp.x, 0);
        ctx.lineTo(cp.x, h);
    }
    ctx.stroke();

    // ── Lignes centrales ──
    ctx.strokeStyle = 'rgba(48, 64, 96, 0.3)';
    ctx.lineWidth = 0.5;
    ctx.beginPath();
    for (var ch = 0; ch < count; ch++) {
        var cp = cellPositions[ch];
        var scopeW = cp.w - vuW - 3;
        if (scopeW < 4) scopeW = 4;
        ctx.moveTo(cp.x + 1, halfH);
        ctx.lineTo(cp.x + 1 + scopeW, halfH);
    }
    ctx.stroke();

    // ── Waveforms ──
    var scopeColors = ['#40B040', '#40A0D0', '#D0A040', '#D04080',
        '#8040D0', '#40D0A0', '#D06040', '#4080D0',
        '#40B040', '#40A0D0', '#D0A040', '#D04080',
        '#8040D0', '#40D0A0', '#D06040', '#4080D0'];

    for (var ch = 0; ch < count; ch++) {
        var data = _ft2ScopeSmooth[ch];
        if (!data) continue;

        var cp = cellPositions[ch];
        var scopeW = cp.w - vuW - 3;
        if (scopeW < 4) scopeW = 4;
        var x0 = cp.x + 1;
        var step = data.length / scopeW;

        ctx.strokeStyle = scopeColors[ch % scopeColors.length];
        ctx.lineWidth = 1;
        ctx.beginPath();
        for (var px = 0; px < scopeW; px++) {
            var val = data[(px * step) | 0];
            var y = halfH - val * ampH;
            if (px === 0) ctx.moveTo(x0 + px, y);
            else ctx.lineTo(x0 + px, y);
        }
        ctx.stroke();
    }

    // ── VU-meters ──
    var vuColors = ['#40C040', '#C0C040', '#C04040'];
    var vuRanges = [[0, 4], [4, 6], [6, 8]];

    for (var pass = 0; pass < 3; pass++) {
        ctx.fillStyle = vuColors[pass];
        var sMin = vuRanges[pass][0];
        var sMax = vuRanges[pass][1];
        for (var ch = 0; ch < count; ch++) {
            var peak = _ft2VuPeaksSmooth[ch];
            var segs = Math.min(8, (peak * 8 + 0.5) | 0);
            var cp = cellPositions[ch];
            var scopeW = cp.w - vuW - 3;
            if (scopeW < 4) scopeW = 4;
            var vuX = cp.x + scopeW + 2;
            for (var s = sMin; s < sMax && s < segs; s++) {
                ctx.fillRect(vuX, h - 1 - (s + 1) * segH, vuW, segH - 1);
            }
        }
    }

    ctx.fillStyle = '#0E1218';
    for (var ch = 0; ch < count; ch++) {
        var peak = _ft2VuPeaksSmooth[ch];
        var segs = Math.min(8, (peak * 8 + 0.5) | 0);
        var cp = cellPositions[ch];
        var scopeW = cp.w - vuW - 3;
        if (scopeW < 4) scopeW = 4;
        var vuX = cp.x + scopeW + 2;
        for (var s = segs; s < 8; s++) {
            ctx.fillRect(vuX, h - 1 - (s + 1) * segH, vuW, segH - 1);
        }
    }
}

// ──────────────
// Pattern
// ──────────────
function updatePattern() {
    if (!_ft2PatternGrid) return;

    var audio = window.SharpModAudio;
    var row = audio.patternPosition;
    var patNum = audio.patternNumber;

    // Changement de pattern ? Notifier Blazor (ASYNC, non bloquant)
    if (patNum !== _ft2LastPatNum && audio.dotnetRef) {
        _ft2LastPatNum = patNum;
        audio.dotnetRef.invokeMethodAsync('OnPatternChanged',
            audio.songPosition, patNum);
    }

    if (_ft2ActiveRowEl) {
        _ft2ActiveRowEl.classList.remove('active');
    }
    var el = document.getElementById('prow_' + row);
    if (el) {
        el.classList.add('active');
        _ft2ActiveRowEl = el;
    }

    var diff = row - _ft2ScrollRow;
    if (Math.abs(diff) < 0.5) _ft2ScrollRow = row;
    else _ft2ScrollRow += diff * 0.3;

    var gridH = _ft2PatternGrid.clientHeight;
    var center = ((gridH / _ft2RowHeight) | 0) >> 1;
    var scroll = (_ft2ScrollRow - center) * _ft2RowHeight;
    _ft2PatternGrid.scrollTop = Math.max(0,
        Math.min(scroll, _ft2TotalRows * _ft2RowHeight - gridH));
}

window.triggerFileInput = function (elementId) {
    document.getElementById(elementId).click();
};
