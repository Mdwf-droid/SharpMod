using SharpMod.IO;
using SharpMod.Song;
using SharpMod.UniTracker;
using System;

namespace SharpMod
{
    ///<summary>
    ///</summary>
    ///<param name="module"></param>
    ///<param name="numPat"></param>
    ///<param name="rowsCount"></param>
    public delegate bool AllocPatternsHandler(SongModule module, int numPat, int rowsCount);

    ///<summary>
    ///</summary>
    ///<param name="pat"></param>
    ///<param name="channelCount"></param>
    public delegate bool AllocTracksHandler(Pattern pat, int channelCount);

    ///<summary>
    ///</summary>
    ///<param name="module"></param>
    ///<param name="nbInstruments"></param>
    public delegate bool AllocInstrumentsHandler(SongModule module, int nbInstruments);

    ///<summary>
    ///</summary>
    ///<param name="ins"></param>
    public delegate bool AllocSamplesHandler(Instrument ins);

    ///<summary>
    ///</summary>
    public interface ILoader
    {
        ///<summary>
        ///</summary>
        event AllocPatternsHandler AllocPatterns;
        ///<summary>
        ///</summary>
        event AllocTracksHandler AllocTracks;
        ///<summary>
        ///</summary>
        event AllocInstrumentsHandler AllocInstruments;
        ///<summary>
        ///</summary>
        event AllocSamplesHandler AllocSamples;

        ///<summary>
        ///</summary>
        ModBinaryReader Reader { get; set; }

        ///<summary>
        ///</summary>
        String LoaderType
        {
            get;
        }

        ///<summary>
        ///</summary>
        String LoaderVersion
        {
            get;
        }

        ///<summary>
        ///</summary>
        UniTrk UniTrack
        {
            get;
            set;
        }

        ///<summary>
        ///</summary>
        ///<param name="module"></param>
        ///<returns></returns>
        bool Init(SongModule module);

        ///<summary>
        ///</summary>
        ///<returns></returns>
        bool Load();

        ///<summary>
        ///</summary>
        ///<returns></returns>
        bool Test();
    }
}
