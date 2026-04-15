// ═══════════════════════════════════════════════
// SharpMod WebAudio Renderer  v5
// + AnalyserNode pour FFT spectrum + VU-meters
// ═══════════════════════════════════════════════

window.SharpModAudio = {
    audioContext: null,
    scriptNode: null,
    analyser: null,
    dotnetRef: null,
    isPlaying: false,
    sampleRate: 48000,
    bufferSize: 4096,
    fftData: null,
    timeDomainData: null,

    initialize: function () {
        if (!this.audioContext) {
            this.audioContext = new (window.AudioContext || window.webkitAudioContext)({
                sampleRate: this.sampleRate
            });

            // Créer l'AnalyserNode pour FFT et VU
            this.analyser = this.audioContext.createAnalyser();
            this.analyser.fftSize = 256;
            this.analyser.smoothingTimeConstant = 0.8;

            this.fftData = new Uint8Array(this.analyser.frequencyBinCount);
            this.timeDomainData = new Uint8Array(this.analyser.fftSize);
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

            var byteCount = self.bufferSize * 4;
            var rawBytes = self.dotnetRef.invokeMethod('FillBuffer', byteCount);

            if (!rawBytes || rawBytes.length === 0) {
                event.outputBuffer.getChannelData(0).fill(0);
                event.outputBuffer.getChannelData(1).fill(0);
                return;
            }

            var left = event.outputBuffer.getChannelData(0);
            var right = event.outputBuffer.getChannelData(1);
            var view = new DataView(new Uint8Array(rawBytes).buffer);

            for (var i = 0; i < self.bufferSize; i++) {
                var offset = i * 4;
                if (offset + 3 < rawBytes.length) {
                    left[i] = view.getInt16(offset, true) / 32768.0;
                    right[i] = view.getInt16(offset + 2, true) / 32768.0;
                } else {
                    left[i] = 0;
                    right[i] = 0;
                }
            }
        };

        // Connecter: scriptNode → analyser → destination
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
        this.isPlaying = !this.isPlaying;
    },

    // Retourne les données FFT (0-255 pour chaque bande de fréquence)
    getFFTData: function () {
        if (!this.analyser || !this.isPlaying) return null;
        this.analyser.getByteFrequencyData(this.fftData);
        return Array.from(this.fftData);
    },

    // Retourne le niveau RMS global (0-100) pour les VU-meters
    getVuLevel: function () {
        if (!this.analyser || !this.isPlaying) return 0;
        this.analyser.getByteTimeDomainData(this.timeDomainData);
        var sum = 0;
        for (var i = 0; i < this.timeDomainData.length; i++) {
            var v = (this.timeDomainData[i] - 128) / 128.0;
            sum += v * v;
        }
        var rms = Math.sqrt(sum / this.timeDomainData.length);
        return Math.min(100, Math.round(rms * 300));
    }
};

window.triggerFileInput = function (elementId) {
    document.getElementById(elementId).click();
};
