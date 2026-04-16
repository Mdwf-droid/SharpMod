// ═══════════════════════════════════════════════════════════════
// SharpMod v18 — Audio engine (AudioWorklet + postMessage)
// Pas de graphique ici — voir visuals-renderer.js
// ═══════════════════════════════════════════════════════════════

window.SharpModAudio = {
    audioContext: null,
    workletNode: null,
    analyser: null,
    dotnetRef: null,
    isPlaying: false,
    sampleRate: 44100,
    bufferSize: 2048,      // taille d'un chunk FillBuffer
    fftData: null,
    _feedInterval: null,
    _fifoLevel: 0,

    // Données partagées avec visuals-renderer.js (lecture seule côté visuals)
    songPosition: 0,
    patternNumber: 0,
    patternPosition: 0,
    channelCount: 0,
    vuLevels: null,
    scopeData: null,
    SCOPE_SIZE: 128,

    initialize: async function () {
        if (this.audioContext) return;

        this.audioContext = new (window.AudioContext || window.webkitAudioContext)({
            sampleRate: this.sampleRate
        });

        // Charger le worklet processor
        await this.audioContext.audioWorklet.addModule('js/audio-worklet-processor.js');

        // Analyser pour la FFT (utilisé par visuals-renderer.js)
        this.analyser = this.audioContext.createAnalyser();
        this.analyser.fftSize = 256;
        this.analyser.smoothingTimeConstant = 0.8;
        this.fftData = new Uint8Array(this.analyser.frequencyBinCount);
    },

    setDotNetReference: function (dotnetRef) {
        this.dotnetRef = dotnetRef;
    },

    play: function () {
        if (!this.audioContext || !this.dotnetRef) return;
        if (this.audioContext.state === 'suspended') {
            this.audioContext.resume();
        }

        this.stop();

        this.isPlaying = true;

        // Créer le worklet node
        this.workletNode = new AudioWorkletNode(
            this.audioContext, 'sharpmod-processor', {
            outputChannelCount: [2]
        }
        );

        var self = this;

        // Recevoir le niveau de FIFO depuis le worklet
        this.workletNode.port.onmessage = function (e) {
            if (e.data.type === 'fifoLevel') {
                self._fifoLevel = e.data.count;
            }
        };

        // Dire au worklet de démarrer
        this.workletNode.port.postMessage({ type: 'start' });

        // Connecter : worklet → analyser → speakers
        this.workletNode.connect(this.analyser);
        this.analyser.connect(this.audioContext.destination);

        // ── Pré-remplir la FIFO (6 chunks d'avance) ──
        for (var i = 0; i < 6; i++) {
            this._produceChunk();
        }

        // ── Feed loop : setInterval (pas throttlé comme RAF) ──
        // Produit des chunks à ~60fps pour garder la FIFO pleine
        // setInterval dans un Worker serait encore mieux, mais
        // on a besoin de l'interop Blazor sur le main thread
        this._feedInterval = setInterval(function () {
            if (!self.isPlaying) return;
            // Toujours produire au moins 1 chunk (pour les visuels)
            // + jusqu'à 2 de plus si la FIFO est basse
            self._produceChunk();
            if (self._fifoLevel < 6) self._produceChunk();
            if (self._fifoLevel < 4) self._produceChunk();
        }, 15); // ~66fps
    },

    // Produire un chunk : FillBuffer (C# interop) → décode → postMessage au worklet
    _produceChunk: function () {
        if (!this.dotnetRef || !this.workletNode) return;

        var audioByteCount = this.bufferSize * 4;
        var rawBytes = this.dotnetRef.invokeMethod('FillBuffer', audioByteCount);

        if (!rawBytes || rawBytes.length < 16) return;

        var view = new DataView(new Uint8Array(rawBytes).buffer);

        // ── Décoder le header (pour les visuels) ──
        this.songPosition = view.getInt32(0, true);
        this.patternNumber = view.getInt32(4, true);
        this.patternPosition = view.getInt32(8, true);
        var chCount = view.getInt32(12, true);

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

        // ── Décoder le PCM en Float32 ──
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

        // ── Envoyer au worklet via Transferable (zero-copy) ──
        this.workletNode.port.postMessage(
            { type: 'audio', left: left, right: right },
            [left.buffer, right.buffer]
        );
    },

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
