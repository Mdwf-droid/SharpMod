using System;

namespace SharpMod.Player
{	
	public class EnvPt
	{
        private short _pos;

        public short Pos
        {
            get { return _pos; }
            set { _pos = value; }
        }

        private short _val;

        public short Val
        {
            get { return _val; }
            set { _val = value; }
        }
	}
}