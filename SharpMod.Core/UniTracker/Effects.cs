namespace SharpMod.UniTracker
{
    /// <summary>
    ///	all known effects:
    /// </summary>
    public enum Effects : short
    {
        /// <summary>
        /// Note to play
        /// </summary>
        UNI_NOTE = 1,

        /// <summary>
        /// Instrument to use
        /// </summary>
        UNI_INSTRUMENT = 2,

        /// <summary>
        /// Arpeggio 
        /// </summary>
        UNI_PTEFFECT0 = 3,

        /// <summary>
        /// Portamento up
        /// </summary>
        UNI_PTEFFECT1 = 4,

        /// <summary>
        /// Portamento down
        /// </summary>
        UNI_PTEFFECT2 = 5,

        /// <summary>
        /// Tone-portamento
        /// </summary>
        UNI_PTEFFECT3 = 6,

        /// <summary>
        /// Vibrato 
        /// </summary>
        UNI_PTEFFECT4 = 7,

        /// <summary>
        /// ToneP + Volsl 
        /// </summary>
        UNI_PTEFFECT5 = 8,

        /// <summary>
        /// Vibra + Volsl
        /// </summary>
        UNI_PTEFFECT6 = 9,

        /// <summary>
        /// Tremolo 
        /// </summary>
        UNI_PTEFFECT7 = 10,

        /// <summary>
        /// Panning
        /// </summary>
        UNI_PTEFFECT8 = 11,

        /// <summary>
        /// Set SampleOffset
        /// </summary>
        UNI_PTEFFECT9 = 12,

        /// <summary>
        /// Volume Slide
        /// </summary>
        UNI_PTEFFECTA = 13,

        /// <summary>
        /// Position Jump
        /// </summary>
        UNI_PTEFFECTB = 14,

        /// <summary>
        /// Set Volume
        /// </summary>
        UNI_PTEFFECTC = 15,

        /// <summary>
        /// Pattern break
        /// </summary>       
        UNI_PTEFFECTD = 16,

        /// <summary>
        /// E0 : Set filter
        /// E1 : Fineslide up
        /// E2 : Fineslide down
        /// E3 : Glissando Ctrl
        /// E4 : Set vibrato waveform
        /// E5 : Set finetune
        /// E6 : PatternLoop 
        /// E7 : Set tremolo waveform
        /// E8 : Fine panning
        /// E9 : Retrig note
        /// EA : FineVolsl up
        /// EB : FineVolsl down
        /// EC : Cut note 
        /// ED : NoteDelay 
        /// EE : PatternDelay 
        /// EF : Invert Loop
        /// </summary>
        UNI_PTEFFECTE = 17,

        /// <summary>
        /// Set speed 
        /// </summary>
        UNI_PTEFFECTF = 18,

        /// <summary>
        /// ScreamTracker Set Speed
        /// </summary>
        UNI_S3MEFFECTA = 19,

        /// <summary>
        /// ScreamTracker Volume Slide
        /// </summary>
        UNI_S3MEFFECTD = 20,

        /// <summary>
        /// ScreamTracker Portamento Down
        /// </summary>
        UNI_S3MEFFECTE = 21,

        /// <summary>
        /// ScreamTracker Portamento Up
        /// </summary>
        UNI_S3MEFFECTF = 22,

        /// <summary>
        /// ScreamTracker Tremor
        /// </summary>
        UNI_S3MEFFECTI = 23,

        /// <summary>
        /// ScreamTracker Retrig
        /// </summary>
        UNI_S3MEFFECTQ = 24,

        /// <summary>
        /// ScreamTracker Set Tempo
        /// </summary>
        UNI_S3MEFFECTT = 25,

        /// <summary>
        /// Fasttracker II Volume Slide
        /// </summary>
        UNI_XMEFFECTA = 26,

        /// <summary>
        /// Fasttracker II Global Volume
        /// </summary>
        UNI_XMEFFECTG = 27,

        /// <summary>
        /// Fasttracker II Global Volume Slide
        /// </summary>
        UNI_XMEFFECTH = 28,

        /// <summary>
        /// Fasttracker II Panning Slide
        /// </summary>
        UNI_XMEFFECTP = 29
    }
}
