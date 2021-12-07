using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;

namespace SharpMod.SoundRenderer
{
    public delegate int ByteStreamRequiredHandler(byte[] buffer, int count);

    public partial class DynamicMediaStreamSource : MediaStreamSource, INotifyPropertyChanged
    {
        public event ByteStreamRequiredHandler OnByteStreamRequired;

        private MemoryStream _stream;
        private SampleRate _sampleRate = new SampleRate(48000);

        public SampleRate SampleRate
        {
            get { return _sampleRate; }
            set { _sampleRate = value; }
        }

        public Stream AudioStream
        {
            get { return _stream; }
        }

        public uint ByteRate
        {
            get { return (uint)_sampleRate.SamplesPerSecond * ChannelCount * BitsPerSample / 8; }
        }


        public double Volume
        {
            get { return _volume; }
            set { _volume = value; NotifyChanged("Volume"); }
        }

        private double _volume = .5;     // 0 to 1, but can go from 0 to infinity since it is just a multiplier. My amp goes to 11!

        public const int BitsPerSample = 16;    // change this and you need to change code that writes the sample too
        public const int ChannelCount = 2;      // 2 for Stereo

        private WaveFormatEx _waveFormat;
        private MediaStreamDescription _audioDesc;
        private long _currentPosition;
        private long _startPosition;
        private long _currentTimeStamp;

        private uint _bufferByteCount;
        private long _mediaTimeStampIncrement;

        // you only need sample attributes for video
        private Dictionary<MediaSampleAttributeKeys, string> _emptySampleDict = new Dictionary<MediaSampleAttributeKeys, string>();

        public DynamicMediaStreamSource()
        {            
            InitializeMediaStreamSource();
        }


        private void InitializeMediaStreamSource()
        {
            AudioBufferLength = 30;

            _waveFormat = new WaveFormatEx();
            _waveFormat.BitsPerSample = BitsPerSample;
            _waveFormat.AvgBytesPerSec = (int)ByteRate;
            _waveFormat.Channels = ChannelCount;
            _waveFormat.BlockAlign = ChannelCount * (BitsPerSample / 8);
            _waveFormat.ext = null; // ??
            _waveFormat.FormatTag = WaveFormatEx.FormatPCM;
            _waveFormat.SamplesPerSec = _sampleRate.SamplesPerSecond;
            _waveFormat.Size = 0; // must be zero

            _waveFormat.ValidateWaveFormat();


            //_stream = new System.IO.MemoryStream();

            _bufferByteCount = 4096;//uint)_oscillatorMonoBufferSampleCount * ChannelCount * (BitsPerSample / 8);
            _mediaTimeStampIncrement = _waveFormat.AudioDurationFromBufferSize(_bufferByteCount);
            _currentPosition = 0;
        }


        protected override void OpenMediaAsync()
        {
            System.Diagnostics.Debug.WriteLine("MediaStreamSource.OpenMediaAsync");

            _startPosition = _currentPosition = 0;
            _currentTimeStamp = 0;


            // Init
            Dictionary<MediaStreamAttributeKeys, string> streamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();
            Dictionary<MediaSourceAttributesKeys, string> sourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            List<MediaStreamDescription> availableStreams = new List<MediaStreamDescription>();

            // Stream Description 
            streamAttributes[MediaStreamAttributeKeys.CodecPrivateData] = _waveFormat.ToHexString(); // wfx
            MediaStreamDescription msd = new MediaStreamDescription(MediaStreamType.Audio, streamAttributes);
            _audioDesc = msd;
            availableStreams.Add(_audioDesc);

            //sourceAttributes[MediaSourceAttributesKeys.Duration] = _queue.Duration.ToString();
            sourceAttributes[MediaSourceAttributesKeys.Duration] = TimeSpan.FromMinutes(0).Ticks.ToString(CultureInfo.InvariantCulture);
            sourceAttributes[MediaSourceAttributesKeys.CanSeek] = false.ToString();

            ReportOpenMediaCompleted(sourceAttributes, availableStreams);

            //System.Diagnostics.Debug.WriteLine("Completed OpenMediaAsync");

        }

        protected override void CloseMedia()
        {
            System.Diagnostics.Debug.WriteLine("MediaStreamSource.CloseMedia");
            // Close the stream
            _startPosition = _currentPosition = 0;
            _audioDesc = null;
        }

        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            //if (diagnosticKind == MediaStreamSourceDiagnosticKind.BufferLevelInBytes)

            //System.Diagnostics.Debug.WriteLine("MediaStreamSource.GetDiagnosticAsync " + diagnosticKind);
            //ReportGetDiagnosticCompleted(diagnosticKind, 0);

            throw new NotImplementedException();

        }

        //private int AlignUp(int a, int b)
        //{
        //    int tmp = a + b - 1;
        //    return tmp - (tmp % b);
        //}

        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            //System.Diagnostics.Debug.WriteLine("MediaStreamSource.GetSampleAsync sample start:" + _currentTimeStamp);
            _stream = new System.IO.MemoryStream();
            Output();

            // Send out the next sample
            MediaStreamSample msSamp = new MediaStreamSample(
                _audioDesc,
                _stream,
                0/*_currentPosition*/,
                _bufferByteCount,
                _currentTimeStamp,
                _emptySampleDict);

            // Move our timestamp and position forward
            _currentTimeStamp += _mediaTimeStampIncrement;
            _currentPosition += _bufferByteCount;


            //// wrap the stream
            //if (_currentPosition >= long.MaxValue - _bufferByteCount || _currentPosition + _bufferByteCount >= _stream.Capacity)
            //{
            //    _currentPosition = 0;
            //    //TODO: Find out when I'll need to wrap the timeStamp and how the stream responds
            //}


            ReportGetSampleCompleted(msSamp);

            //_stream.Dispose();
            
            //           System.Diagnostics.Debug.WriteLine("Finished MediaStreamSource.GetSampleAsync " + DateTime.Now.Millisecond);

        }





        protected override void SeekAsync(long seekToTime)
        {
            //throw new NotImplementedException();
            System.Diagnostics.Debug.WriteLine("MediaStreamSource.SeekAsync " + seekToTime);

            _currentPosition = _waveFormat.BufferSizeFromAudioDuration(seekToTime);           
            _currentTimeStamp = seekToTime;

            ReportSeekCompleted(seekToTime);
        }

        public void Output()
        {
            StereoSample peakValues = new StereoSample();

            byte[] buffer = new byte[_bufferByteCount];
            
            int len = 0;
            if (OnByteStreamRequired != null)
            {
                len = OnByteStreamRequired(buffer, buffer.Length);
            }
            _stream.Write(buffer, 0, len);
            _stream.Seek(0, SeekOrigin.Begin);
        }


        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotImplementedException();
        }

        #region INotifyPropertyChanged Members

        private void NotifyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public event PropertyChangedEventHandler PropertyChanged;


        #endregion
    }
}
