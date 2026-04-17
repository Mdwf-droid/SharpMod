// ═══════════════════════════════════════════════════════
// SharpMod AudioWorklet Processor
// Thread audio séparé — jamais throttlé en background
// Reçoit des Float32Array via port.postMessage
// ═══════════════════════════════════════════════════════

class SharpModProcessor extends AudioWorkletProcessor {
    constructor() {
        super();

        // FIFO : tableau de { left: Float32Array, right: Float32Array }
        this._fifo = [];
        this._fifoOffset = 0;  // position de lecture dans le chunk courant
        this._playing = false;

        this.port.onmessage = (e) => {
            var msg = e.data;

            if (msg.type === 'start') {
                this._playing = true;
                this._fifo = [];
                this._fifoOffset = 0;
            }
            else if (msg.type === 'stop') {
                this._playing = false;
                this._fifo = [];
                this._fifoOffset = 0;
            }
            else if (msg.type === 'audio') {
                // Recevoir un chunk PCM pré-décodé
                this._fifo.push({
                    left: msg.left,
                    right: msg.right
                });
            }
        };
    }

    process(inputs, outputs, parameters) {
        var output = outputs[0];
        if (!output || output.length < 2) return true;

        var outLeft = output[0];
        var outRight = output[1];
        var frameCount = outLeft.length; // 128 samples par appel

        if (!this._playing || this._fifo.length === 0) {
            // Silence
            for (var i = 0; i < frameCount; i++) {
                outLeft[i] = 0;
                outRight[i] = 0;
            }
            return true;
        }

        var written = 0;

        while (written < frameCount && this._fifo.length > 0) {
            var chunk = this._fifo[0];
            var chunkLen = chunk.left.length;
            var available = chunkLen - this._fifoOffset;
            var needed = frameCount - written;
            var toCopy = Math.min(available, needed);

            for (var i = 0; i < toCopy; i++) {
                outLeft[written + i] = chunk.left[this._fifoOffset + i];
                outRight[written + i] = chunk.right[this._fifoOffset + i];
            }

            written += toCopy;
            this._fifoOffset += toCopy;

            if (this._fifoOffset >= chunkLen) {
                // Chunk épuisé, passer au suivant
                this._fifo.shift();
                this._fifoOffset = 0;
            }
        }

        // Remplir le reste de silence si FIFO vide (underrun)
        for (var i = written; i < frameCount; i++) {
            outLeft[i] = 0;
            outRight[i] = 0;
        }

        // Signaler le niveau de remplissage au main thread
        // pour qu'il puisse ajuster le rythme de production
        if (currentFrame % 512 === 0) {
            this.port.postMessage({ type: 'fifoLevel', count: this._fifo.length });
        }

        return true;
    }
}

registerProcessor('sharpmod-processor', SharpModProcessor);
