// ═══════════════════════════════════════════════════════════════
// SharpMod v19 — Audio engine (AudioWorklet + latency compensation)
// ═══════════════════════════════════════════════════════════════

window.SharpModAudio = {
    audioContext: null,
    workletNode: null,
    analyser: null,
    dotnetRef: null,
    isPlaying: false,
    sampleRate: 44100,
    bufferSize: 2048,
    fftData: null,
    _feedInterval: null,
    _fifoLevel: 0,

    // ── Positions BRUTES (en avance sur l'audio) ──
    songPosition: 0,
    patternNumber: 0,
    patternPosition: 0,

    // ── ★ Positions RETARDÉES (synchronisées avec l'audio entendu) ──
    displaySongPosition: 0,
    displayPatternNumber: 0,
    displayPatternPosition: 0,

    // ── ★ Ring buffer de positions horodatées ──
    _positionRing: [],
    _audioLatencyMs: 280,  // valeur initiale, ajustée dynamiquement
    _smoothLatencyMs: 280,

    // ── Données partagées avec visuals-renderer.js ──
    channelCount: 0,
    vuLevels: null,
    scopeData: null,
    SCOPE_SIZE: 128,

    // ═══════════════════
    // Init
    // ═══════════════════

    initialize: async function () {
        if (this.audioContext) return;

        this.audioContext = new (window.AudioContext || window.webkitAudioContext)({
            sampleRate: this.sampleRate
        });

        await this.audioContext.audioWorklet.addModule('js/audio-worklet-processor.js');

        this.analyser = this.audioContext.createAnalyser();
        this.analyser.fftSize = 256;
        this.analyser.smoothingTimeConstant = 0.8;
        this.fftData = new Uint8Array(this.analyser.frequencyBinCount);
    },

    setDotNetReference: function (dotnetRef) {
        this.dotnetRef = dotnetRef;
    },

    // ═══════════════════
    // Playback
    // ═══════════════════

    play: function () {
        if (!this.audioContext || !this.dotnetRef) return;
        if (this.audioContext.state === 'suspended') {
            this.audioContext.resume();
        }

        this.stop();
        this.isPlaying = true;

        // ★ Reset ring buffer
        this._positionRing = [];
        this.displaySongPosition = 0;
        this.displayPatternNumber = 0;
        this.displayPatternPosition = 0;

        // Créer le worklet node
        this.workletNode = new AudioWorkletNode(
            this.audioContext, 'sharpmod-processor', {
            outputChannelCount: [2]
        }
        );

        var self = this;

        // Recevoir le niveau FIFO depuis le worklet
        this.workletNode.port.onmessage = function (e) {
            if (e.data.type === 'fifoLevel') {
                self._fifoLevel = e.data.count;
                var rawLatency = Math.round(
                    (e.data.count * self.bufferSize / self.sampleRate) * 1000
                );
                // ★ Convergence lente — ne jamais sauter
                self._smoothLatencyMs += (rawLatency - self._smoothLatencyMs) * 0.05;
            }
        };


        this.workletNode.port.postMessage({ type: 'start' });

        // Connecter : worklet → analyser → speakers
        this.workletNode.connect(this.analyser);
        this.analyser.connect(this.audioContext.destination);

        // Pré-remplir la FIFO
        for (var i = 0; i < 6; i++) {
            this._produceChunk();
        }

        // Feed loop
        this._feedInterval = setInterval(function () {
            if (!self.isPlaying) return;

            var produced = 0;
            while (self._fifoLevel < 8 && produced < 3) {
                self._produceChunk();
                produced++;
                self._fifoLevel++;
            }
        }, 15);
    },

    // ═══════════════════
    // ★ Produce chunk + ring buffer
    // ═══════════════════

    _produceChunk: function () {
        if (!this.dotnetRef || !this.workletNode) return;

        var audioByteCount = this.bufferSize * 4;
        var rawBytes = this.dotnetRef.invokeMethod('FillBuffer', audioByteCount);

        if (!rawBytes || rawBytes.length < 16) return;

        var view = new DataView(new Uint8Array(rawBytes).buffer);

        // Décoder le header
        var songPos = view.getInt32(0, true);
        var patNum = view.getInt32(4, true);
        var patPos = view.getInt32(8, true);
        var chCount = view.getInt32(12, true);

        // Stocker les positions brutes (pour compatibilité)
        this.songPosition = songPos;
        this.patternNumber = patNum;
        this.patternPosition = patPos;

        // ★ Stocker dans le ring buffer avec timestamp
        this._positionRing.push({
            time: performance.now(),
            songPosition: songPos,
            patternNumber: patNum,
            patternPosition: patPos
        });

        // Nettoyer les entrées trop vieilles (> 2s)
        var cutoff = performance.now() - 2000;
        while (this._positionRing.length > 0 && this._positionRing[0].time < cutoff) {
            this._positionRing.shift();
        }

        // Channels
        if (chCount !== this.channelCount) {
            this.channelCount = chCount;
            this.vuLevels = new Float64Array(chCount);
            this.scopeData = [];
            for (var i = 0; i < chCount; i++) {
                this.scopeData.push(new Float32Array(this.SCOPE_SIZE));
            }
        }

        var headerSize = 16 + chCount + (chCount * this.SCOPE_SIZE);

        // VU levels
        var vuOffset = 16;
        for (var ch = 0; ch < chCount; ch++) {
            this.vuLevels[ch] = rawBytes[vuOffset + ch] / 255.0;
        }

        // Scope data
        var scopeOffset = vuOffset + chCount;
        for (var ch = 0; ch < chCount; ch++) {
            var buf = this.scopeData[ch];
            var off = scopeOffset + ch * this.SCOPE_SIZE;
            for (var i = 0; i < this.SCOPE_SIZE; i++) {
                var b = rawBytes[off + i];
                buf[i] = (b < 128 ? b : b - 256) / 128.0;
            }
        }

        // PCM → Float32
        var bufSize = this.bufferSize;
        var left = new Float32Array(bufSize);
        var right = new Float32Array(bufSize);

        for (var i = 0; i < bufSize; i++) {
            var offset = headerSize + i * 4;
            if (offset + 3 < rawBytes.length) {
                left[i] = view.getInt16(offset, true) / 32768.0;
                right[i] = view.getInt16(offset + 2, true) / 32768.0;
            }
        }

        // Envoyer au worklet (zero-copy)
        this.workletNode.port.postMessage(
            { type: 'audio', left: left, right: right },
            [left.buffer, right.buffer]
        );
    },

    // ═══════════════════
    // ★ Positions retardées
    // ═══════════════════
    
    updateDisplayPositions: function () {
        var now = performance.now();
        var targetTime = now - this._smoothLatencyMs;

        var best = null;
        for (var i = this._positionRing.length - 1; i >= 0; i--) {
            if (this._positionRing[i].time <= targetTime) {
                best = this._positionRing[i];
                break;
            }
        }

        if (!best) return;

        var oldPat = this.displayPatternNumber;
        var oldRow = this.displayPatternPosition;

        if (best.patternNumber === oldPat) {
            // Même pattern : la row ne peut qu'avancer de 1 max par frame
            if (best.patternPosition > oldRow) {
                this.displayPatternPosition = oldRow + 1;
            }
            // Si best.patternPosition <= oldRow → on ne bouge pas (anti-yoyo)
        } else {
            // Changement de pattern → accepter directement
            this.displayPatternNumber = best.patternNumber;
            this.displayPatternPosition = best.patternPosition;
        }

        this.displaySongPosition = best.songPosition;
    },

    // ★ Appelé par PlayerService.cs via JSInterop
    getDisplayPositions: function () {
        this.updateDisplayPositions();
        return [
            this.displaySongPosition,
            this.displayPatternNumber,
            this.displayPatternPosition
        ];
    },

    // ═══════════════════
    // Stop / Pause
    // ═══════════════════

    stop: function () {
        this.isPlaying = false;

        if (this._feedInterval) {
            clearInterval(this._feedInterval);
            this._feedInterval = null;
        }

        if (this.workletNode) {
            this.workletNode.port.postMessage({ type: 'stop' });
            this.workletNode.disconnect();
            this.workletNode = null;
        }

        this._fifoLevel = 0;
        this._positionRing = [];
    },

    pause: function () {
        if (this.isPlaying) {
            this.isPlaying = false;
            if (this._feedInterval) {
                clearInterval(this._feedInterval);
                this._feedInterval = null;
            }
            if (this.workletNode) {
                this.workletNode.port.postMessage({ type: 'stop' });
                this.workletNode.disconnect();
                this.workletNode = null;
            }
        } else {
            this.play();
        }
    },

    updateFFT: function () {
        if (!this.analyser || !this.isPlaying) return false;
        this.analyser.getByteFrequencyData(this.fftData);
        return true;
    }
};

window.triggerFileInput = function (elementId) {
    document.getElementById(elementId).click();
};
