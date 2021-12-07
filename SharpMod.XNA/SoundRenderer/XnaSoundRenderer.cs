using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using SharpMod;

namespace SharpMod.SoundRenderer
{
    public class XnaSoundRenderer: IRenderer
    {
        byte[] buf = new byte[8192];

        DynamicSoundEffectInstance _dsei;

        public XnaSoundRenderer(DynamicSoundEffectInstance dsei)
        {
            _dsei = dsei;
            _dsei.BufferNeeded += new EventHandler<EventArgs>(_dsei_BufferNeeded);
        }

        void _dsei_BufferNeeded(object sender, EventArgs e)
        {
           
            if (Player != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    this.Player.GetBytes(buf, 8192);
                    _dsei.SubmitBuffer(buf);
                }
            }
            else
            _dsei.SubmitBuffer(buf);
            
        }

        #region IRenderer Members

        public void Init()
        {
            
        }

        public void PlayStart()
        {
            _dsei.Play();
        }

        public void PlayStop()
        {
            _dsei.Stop();
        }

        public ModulePlayer Player
        {
            get;
            set;
        }

        #endregion
    }
}
