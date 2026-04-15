// ═══════════════════════════════════════════════
// FT2 Visuals: FFT Spectrum Analyzer + Scroll
// ═══════════════════════════════════════════════

var _ft2FFTAnimId = null;

// ── FFT Spectrum Analyzer ──
window.ft2StartFFT = function (canvasId) {
    var canvas = document.getElementById(canvasId);
    if (!canvas) return;
    var ctx = canvas.getContext('2d');

    function draw() {
        _ft2FFTAnimId = requestAnimationFrame(draw);

        var data = window.SharpModAudio.getFFTData();
        var w = canvas.width = canvas.clientWidth;
        var h = canvas.height = canvas.clientHeight;

        // Fond
        ctx.fillStyle = '#0B0E14';
        ctx.fillRect(0, 0, w, h);

        if (!data) return;

        var barCount = Math.min(data.length, 64);
        var barWidth = Math.floor(w / barCount);
        var gap = 1;

        for (var i = 0; i < barCount; i++) {
            var val = data[i] / 255.0;
            var barH = val * h;
            var x = i * barWidth;

            // Couleur dégradée vert → jaune → rouge
            var r, g, b;
            if (val < 0.5) {
                r = Math.round(val * 2 * 192);
                g = 192;
                b = 40;
            } else {
                r = 192;
                g = Math.round((1 - (val - 0.5) * 2) * 192);
                b = 40;
            }

            ctx.fillStyle = 'rgb(' + r + ',' + g + ',' + b + ')';
            ctx.fillRect(x + gap, h - barH, barWidth - gap * 2, barH);

            // Reflet subtil en haut de la barre
            var grad = ctx.createLinearGradient(x, h - barH, x, h - barH + 4);
            grad.addColorStop(0, 'rgba(255,255,255,0.2)');
            grad.addColorStop(1, 'rgba(255,255,255,0)');
            ctx.fillStyle = grad;
            ctx.fillRect(x + gap, h - barH, barWidth - gap * 2, Math.min(4, barH));
        }

        // Ligne de grille horizontale subtile
        ctx.strokeStyle = 'rgba(48, 64, 96, 0.3)';
        ctx.lineWidth = 0.5;
        for (var y = 0; y < h; y += Math.round(h / 4)) {
            ctx.beginPath();
            ctx.moveTo(0, y);
            ctx.lineTo(w, y);
            ctx.stroke();
        }
    }

    draw();
};

window.ft2StopFFT = function () {
    if (_ft2FFTAnimId) {
        cancelAnimationFrame(_ft2FFTAnimId);
        _ft2FFTAnimId = null;
    }
};

// ── Auto-scroll pattern ──
window.ft2ScrollToActiveRow = function (containerId, rowIndex, rowHeight, totalRows) {
    var container = document.getElementById(containerId);
    if (!container) return;

    var containerHeight = container.clientHeight;
    var visibleRows = Math.floor(containerHeight / rowHeight);
    var centerOffset = Math.floor(visibleRows / 2);

    var targetScroll = (rowIndex - centerOffset) * rowHeight;
    targetScroll = Math.max(0, Math.min(targetScroll, (totalRows * rowHeight) - containerHeight));

    container.scrollTop = targetScroll;
};

// ── VU level getter (appelé depuis Blazor) ──
window.ft2GetVuLevel = function () {
    return window.SharpModAudio.getVuLevel();
};
