using System;

namespace SharpMod.Player
{
	
	public class EnvPr
	{
        private short _flg; /* envelope flag */

        public short Flg
        {
            get { return _flg; }
            set { _flg = value; }
        }
        private short _pts; /* number of envelope points */

        public short Pts
        {
            get { return _pts; }
            set { _pts = value; }
        }
        private short _sus; /* envelope sustain index */

        public short Sus
        {
            get { return _sus; }
            set { _sus = value; }
        }
        private short _beg; /* envelope loop begin */

        public short Beg
        {
            get { return _beg; }
            set { _beg = value; }
        }
        private short _end; /* envelope loop end */

        public short End
        {
            get { return _end; }
            set { _end = value; }
        }
        private short _currentCounter; /* current envelope counter */

        public short CurrentCounter
        {
            get { return _currentCounter; }
            set { _currentCounter = value; }
        }
        private short _envIdxA; /* envelope index a */

        public short EnvIdxA
        {
            get { return _envIdxA; }
            set { _envIdxA = value; }
        }
        private short _envIdxB; /* envelope index b */

        public short EnvIdxB
        {
            get { return _envIdxB; }
            set { _envIdxB = value; }
        }
        private EnvPt[] _envPoints;	/* envelope points */

        public EnvPt[] EnvPoints
        {
            get { return _envPoints; }
            set { _envPoints = value; }
        }

        public EnvPr()
		{
		}
	}
	
}