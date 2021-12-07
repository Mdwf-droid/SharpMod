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
using System.Collections.Generic;

namespace SharpMod.SoundRenderer
{
    public class SilverlightDriver : IRenderer
    {
        private int bufferLength;
        private DynamicMediaStreamSource _synth;
        private MediaElement _mediaElement;
        private object _lock = new object();
        private Queue<byte[]> _doubleBuffer;
        private Queue<byte[]> DoubleBuffer
        {
            get
            {
                lock (_lock)
                {
                    return _doubleBuffer;
                }
            }
            set
            {
                lock (_lock)
                {
                    _doubleBuffer = value;
                }
            }
        }

        private object _lock2 = new object();
        private bool _doubleBufferFill;
        private bool DoubleBufferFill
        {
            get
            {
                lock (_lock)
                {
                    return _doubleBufferFill;
                }
            }
            set
            {
                lock (_lock)
                {
                    _doubleBufferFill = value;
                }
            }
        }

        private int DblBufferReaded = 0;


        private System.Threading.Thread th;

        public ModulePlayer Player
        {
            get;
            set;
        }

        public SilverlightDriver(MediaElement mediaElement)
        {
            _mediaElement = mediaElement;
            _synth = new DynamicMediaStreamSource();
            _synth.OnByteStreamRequired += new ByteStreamRequiredHandler(_synth_OnByteStreamRequired);
            DoubleBuffer = new Queue<byte[]>();
            th = new System.Threading.Thread(new System.Threading.ThreadStart(_fillBuffer));
            th.IsBackground = true;
            th.Start();
        }

        private void _fillBuffer()
        {
            while (true)
            {

                while (DoubleBuffer.Count < 6 && bufferLength >0)
                {
                    byte[] tmp = new byte[bufferLength];
                    this.Player.GetBytes(tmp, bufferLength);
                    DoubleBuffer.Enqueue(tmp);
                    tmp = null;
                }


                System.Threading.Thread.Sleep(5);

            }
        }

        int _synth_OnByteStreamRequired(byte[] buffer, int count)
        {
            if (bufferLength == 0)
                bufferLength = count;
            if (DoubleBuffer.Count >= 2)
            {
                byte[] bufferToPlay = DoubleBuffer.Dequeue();
                

                Buffer.BlockCopy(bufferToPlay, 0, buffer, 0, count);               
                bufferToPlay = null;
            }
            
            return count;
        }

        #region IRenderer Membres

        void IRenderer.Init()
        {
            _mediaElement.SetSource(_synth);
        }

        void IRenderer.PlayStart()
        {
            _mediaElement.Play();
        }

        void IRenderer.PlayStop()
        {
            _mediaElement.Stop();
        }



        #endregion


    }
}
