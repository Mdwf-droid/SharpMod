using System;
using SharpMod.Player;
using System.Collections.Generic;

namespace SharpMod.Song
{	
	public class Instrument
	{
        private short _numSmp;

        public short NumSmp
        {
            get { return _numSmp; }
            set { _numSmp = value; }
        }
        private short[] _sampleNumber;

        public short[] SampleNumber
        {
            get { return _sampleNumber; }
            set { _sampleNumber = value; }
        }

        private short _volFlg; /* bit 0: on 1: sustain 2: loop */

        public short VolFlg
        {
            get { return _volFlg; }
            set { _volFlg = value; }
        }
        private short _volPts;

        public short VolPts
        {
            get { return _volPts; }
            set { _volPts = value; }
        }
        private short _volSus;

        public short VolSus
        {
            get { return _volSus; }
            set { _volSus = value; }
        }
        private short _volBeg;

        public short VolBeg
        {
            get { return _volBeg; }
            set { _volBeg = value; }
        }
        private short _volEnd;

        public short VolEnd
        {
            get { return _volEnd; }
            set { _volEnd = value; }
        }
        private EnvPt[] _volEnv;

        public EnvPt[] VolEnv
        {
            get { return _volEnv; }
            set { _volEnv = value; }
        }

        private short _panFlg; /* bit 0: on 1: sustain 2: loop */

        public short PanFlg
        {
            get { return _panFlg; }
            set { _panFlg = value; }
        }
        private short _panPts;

        public short PanPts
        {
            get { return _panPts; }
            set { _panPts = value; }
        }
        private short _panSus;

        public short PanSus
        {
            get { return _panSus; }
            set { _panSus = value; }
        }
        private short _panBeg;

        public short PanBeg
        {
            get { return _panBeg; }
            set { _panBeg = value; }
        }
        private short _panEnd;

        public short PanEnd
        {
            get { return _panEnd; }
            set { _panEnd = value; }
        }
        private EnvPt[] _panEnv;

        public EnvPt[] PanEnv
        {
            get { return _panEnv; }
            set { _panEnv = value; }
        }

        private short _vibType;

        public short VibType
        {
            get { return _vibType; }
            set { _vibType = value; }
        }
        private short _vibSweep;

        public short VibSweep
        {
            get { return _vibSweep; }
            set { _vibSweep = value; }
        }
        private short _vibDepth;

        public short VibDepth
        {
            get { return _vibDepth; }
            set { _vibDepth = value; }
        }
        private short _vibRate;

        public short VibRate
        {
            get { return _vibRate; }
            set { _vibRate = value; }
        }

        private int _volFade;

        public int VolFade
        {
            get { return _volFade; }
            set { _volFade = value; }
        }
        private String _insName;

        public String InsName
        {
            get { return _insName; }
            set { _insName = value; }
        }

        public List<Sample> Samples { get; set; }

        public Instrument()
		{
			SampleNumber = new short[96];
			VolEnv = new EnvPt[12];
			PanEnv = new EnvPt[12];
			int i;
			for (i = 0; i < 12; i++)
			{
				VolEnv[i] = new EnvPt();
				PanEnv[i] = new EnvPt();
			}
		}
	}
}