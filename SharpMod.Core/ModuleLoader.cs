using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SharpMod.Song;
using System.IO;
using SharpMod.Exceptions;
using SharpMod.IO;
using SharpMod.UniTracker;

namespace SharpMod
{
    ///<summary>
    ///</summary>
    public class ModuleLoader
    {
        private ILoader _currentLoader;
        private ModBinaryReader _reader;
        private SampleLoader _sampleLoader;
        private List<byte[]> _samples;
        private UniTrk _uniTrack;

        ///<summary>
        ///</summary>
        public List<ILoader> Loaders { get; private set; }

        #region Singleton definition
        private static ModuleLoader _instance;
        /// <summary>
        /// Singleton definition of the ModuleLoader
        /// </summary>
        public static ModuleLoader Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ModuleLoader();

                return _instance;
            }
        }
        #endregion

        /// <summary>
        /// Private constructor
        /// </summary>
        private ModuleLoader()
        {
            Loaders = new List<ILoader>();
            _sampleLoader = new SampleLoader();
            LoadInternalsLoaders();
        }

        /// <summary>
        /// Allow to add custom loader to the internal list
        /// </summary>
        /// <param name="loader">Loader to add</param>
        public void AddLoader(ILoader loader)
        {
            loader.AllocInstruments += AllocInstruments;
            loader.AllocPatterns += AllocPatterns;
            loader.AllocSamples += AllocSamples;
            loader.AllocTracks += AllocTracks;
            
            Loaders.Add(loader);
        }

        /// <summary>
        /// Load all internals loaders
        /// </summary>
        private void LoadInternalsLoaders()
        {
            // Retrieve types that implements ILoader interface
// ReSharper disable AssignNullToNotNullAttribute
            var loaders = Assembly.GetExecutingAssembly().GetExportedTypes().Where(x => x.GetInterface(typeof(ILoader).FullName,false) != null).ToList();
// ReSharper restore AssignNullToNotNullAttribute

            loaders.ForEach(new Action<Type>(x =>
                {
                    var loader = (ILoader)Activator.CreateInstance(x);
                    loader.AllocInstruments += AllocInstruments;
                    loader.AllocPatterns += AllocPatterns;
                    loader.AllocSamples += AllocSamples;
                    loader.AllocTracks += AllocTracks;
                    Loaders.Add(loader);
                }));
        }

        /// <summary>
        /// Initialize internals variables
        /// </summary>
        private void Init()
        {
            _uniTrack = new UniTrk();
            _samples = new List<byte[]>();
        }

        /// <summary>
        /// Load Module from file
        /// </summary>
        /// <param name="fileName">Path of the file to load from</param>
        /// <returns>SongModule</returns>
        public SongModule LoadModule(string fileName)
        {
            Stream stream;

            try
            {
                if ((stream = new FileStream(fileName, FileMode.Open, FileAccess.Read)) == null)
                {
                    throw new SharpModException(SharpModExceptionResources.ERROR_OPENING_FILE, fileName);
                }

                var toReturn = LoadModule(stream);

                stream.Close();
                stream.Dispose();

                return toReturn;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Load Module from stream
        /// </summary>
        /// <param name="inputStream">stream to load from</param>
        /// <returns>SongModule</returns>
        public SongModule LoadModule(Stream inputStream)
        {
            var toReturn = new SongModule();

            Init();

            _reader = new ModBinaryReader(inputStream);

            // init panning array 
            for (var t = 0; t < 32; t++)
            {
                toReturn.Panning[t] = (short)((((t + 1) & 2) != 0) ? 255 : 0);
            }

            if (!LoadHeader(toReturn))
            {
                toReturn = null;
                throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_HEADER);
            }

            if (!LoadSamples(toReturn))
            {
                toReturn = null;
                throw new SharpModException(SharpModExceptionResources.ERROR_LOADING_SAMPLEINFO);
            }

            return toReturn;
        }

        /// <summary>
        /// Load header of the module with the best loader
        /// </summary>
        /// <param name="module">Module to fill</param>
        /// <returns>true if success</returns>
        private bool LoadHeader(SongModule module)
        {
            var toReturn = false;

            _currentLoader = null;
            foreach (var loader in Loaders)
            {
                //Reset the reader
                _reader.Rewind();
                loader.Reader = _reader;
                if (loader.Test())
                {
                    _currentLoader = loader;
                    break;
                }
            }

            // Loader not found...
            if (null == _currentLoader)
            {
                throw new SharpModException(SharpModExceptionResources.ERROR_NOT_A_MODULE);
            }

            if (!_uniTrack.UniInit())
                return false;

            _currentLoader.UniTrack = _uniTrack;     

            // init module loader
            if (_currentLoader.Init(module))
            {
                _reader.Rewind();
                toReturn = _currentLoader.Load();
            }

            // free unitrk allocations 
            _uniTrack.UniCleanup();

            return toReturn;
        }

        /// <summary>
        /// Load samples of the module
        /// </summary>
        /// <param name="module">Module to fill</param>
        /// <returns>True if success</returns>
        private bool LoadSamples(SongModule module)
        {
            foreach (var t1 in module.Instruments)
            {
                for (var u = 0; u < t1.NumSmp; u++)
                {
                    // sample has to be loaded ? -> increase
                    // number of samples and allocate memory and
                    // load sample 
                    if (t1.Samples[u].Length != 0)
                    {
                        if (t1.Samples[u].SeekPos != 0)
                        {
                            _reader.Seek(t1.Samples[u].SeekPos, SeekOrigin.Begin);
                        }

                        // Call the sample load routine of the driver module.
                        // It has to return a 'handle' (>=0) that identifies
                        // the sample
                        var smp = t1.Samples[u];
                        int handle = SampleLoad(smp.Length, smp.LoopStart, smp.LoopEnd, smp.Flags);
                        t1.Samples[u].Handle = handle;


                        if (t1.Samples[u].Handle < 0)
                            return false;

                        t1.Samples[u].SampleBytes = _samples[handle];
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Load Sample from the Module Stream
        /// </summary>
        /// <param name="length">Length in bytes of the sample</param>
        /// <param name="reppos">Repeat begin position</param>
        /// <param name="repend">Repeat end position</param>
        /// <param name="flags">Sample format flags</param>
        /// <returns>Handle of the sample</returns>
        public short SampleLoad(int length, int reppos, int repend, SampleFormatFlags flags)
        {
            // Find empty slot to put sample address in
            var handle = _samples.Count;

            _sampleLoader.Init(_reader, flags, ((flags | (SampleFormatFlags.SF_SIGNED)) & ~(SampleFormatFlags.SF_16BITS)));

            // create the new byte array entry
            _samples.Add(new byte[length + 17]);

            // read sample into buffer. 
            LargeRead(_samples[handle], length);

            // Unclick samples: 
            if ((flags & (SampleFormatFlags.SF_LOOP)) != 0)
            {
                if ((flags & (SampleFormatFlags.SF_BIDI)) != 0)
                    for (var t = 0; t < 16; t++)
                        _samples[handle][repend + t] = _samples[handle][(repend - t) - 1];
                else
                    for (var t = 0; t < 16; t++)
                        _samples[handle][repend + t] = _samples[handle][t + reppos];
            }
            else
            {
                for (int t = 0; t < 16; t++)
                    _samples[handle][t + length] = 0;
            }

            return (short)handle;
        }

        ///<summary>
        ///</summary>
        ///<param name="buffer"></param>
        ///<param name="size"></param>
        public void LargeRead(byte[] buffer, int size)
        {
            var bufOffset = 0;

            while (size != 0)
            {
                // how many bytes to load (in chunks of 8000) ?
                int todo = (size > 8000) ? 8000 : size;

                // read data
                _sampleLoader.Load(buffer, bufOffset, todo);

                // and update pointers..
                size -= todo;
                bufOffset += todo;
            }
        }

        /// <summary>
        /// Allocate Instruments
        /// </summary>
        /// <param name="module"></param>
        /// <param name="nbInstruments"></param>
        /// <returns></returns>
        public bool AllocInstruments(SongModule module, int nbInstruments)
        {
            module.Instruments = new List<Instrument>(nbInstruments);
            for (var i = 0; i < nbInstruments; i++)
            {
                module.Instruments.Add(new Instrument());
            }
            for (var i = 0; i < nbInstruments; i++)
            {
                module.Instruments[i].NumSmp = 0;
                module.Instruments[i].VolFlg = 0;
                module.Instruments[i].VolPts = 0;
                module.Instruments[i].VolSus = 0;
                module.Instruments[i].VolBeg = 0;
                module.Instruments[i].VolEnd = 0;
                module.Instruments[i].PanFlg = 0;
                module.Instruments[i].PanSus = 0;
                module.Instruments[i].PanEnd = 0;
                module.Instruments[i].VibType = 0;
                module.Instruments[i].VibSweep = 0;
                module.Instruments[i].VibDepth = 0;
                module.Instruments[i].VibRate = 0;
                module.Instruments[i].VolFade = 0;

                module.Instruments[i].InsName = null;
                module.Instruments[i].Samples = new List<Sample>();

                for (var j = 0; j < 96; j++)
                    module.Instruments[i].SampleNumber[j] = 0;

                for (var j = 0; j < 12; j++)
                {
                    module.Instruments[i].VolEnv[j].Pos = 0;
                    module.Instruments[i].VolEnv[j].Val = 0;
                    module.Instruments[i].PanEnv[j].Pos = 0;
                    module.Instruments[i].PanEnv[j].Val = 0;
                }
            }
            return true;
        }

        /// <summary>
        /// Allocate samples for instrument
        /// </summary>
        /// <param name="ins"></param>
        /// <returns></returns>
        public bool AllocSamples(Instrument ins)
        {
            int n;

            if ((n = ins.NumSmp) != 0)
            {                
                ins.Samples = new List<Sample>(n);// new Sample[n];
                for (var u = 0; u < n; u++)
                {
                    ins.Samples.Add(new Sample());
                }
                for (var u = 0; u < n; u++)
                {
                    ins.Samples[u].C2Spd = 0;
                    ins.Samples[u].Length = 0;
                    ins.Samples[u].LoopStart = 0;
                    ins.Samples[u].LoopEnd = 0;
                    ins.Samples[u].Flags = 0;
                    ins.Samples[u].SeekPos = 0;
                    ins.Samples[u].Handle = 0;
                    ins.Samples[u].Transpose = 0;
                    ins.Samples[u].Volume = 0;
                    ins.Samples[u].Panning = 0;
                    ins.Samples[u].SampleName = null;
                    ins.Samples[u].Panning = 128;
                    ins.Samples[u].Handle = -1;
                }
              
            }
            return true;
        }

        ///<summary>
        ///</summary>
        ///<param name="module"></param>
        ///<param name="numPat"></param>
        ///<param name="rowsCount"></param>
        ///<returns></returns>
        public bool AllocPatterns(SongModule module, int numPat, int rowsCount)
        {
            // Allocate track sequencing array
            //module.Patterns = new List<Pattern>(numPat);
            if (module.Patterns == null)
                module.Patterns = new List<Pattern>();

            if (module.Patterns.Count <= numPat)
                module.Patterns.Add(new Pattern(rowsCount));
            else
                module.Patterns[numPat] = new Pattern(rowsCount);
            //for (int t = 0; t < numPat; t++)
                

            return true;
        }

        ///<summary>
        ///</summary>
        ///<param name="pat"></param>
        ///<param name="channelCount"></param>
        ///<returns></returns>
        public bool AllocTracks(Pattern pat,int channelCount)
        {
           // foreach (Pattern pat in module.Patterns)
           // {
            pat.Tracks = new List<Track>(channelCount/*module.ChannelsCount*/);
            for (int t = 0; t < channelCount/*module.ChannelsCount*/; t++)
                {
                    var trk = new Track();
                    trk.UniTrack = new short[] { };
                    pat.Tracks.Add(trk);
                }
          //  }

            return true;
        }
    }
}
