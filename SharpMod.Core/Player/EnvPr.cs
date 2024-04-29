namespace SharpMod.Player
{

    public class EnvPr
    {
        /* envelope flag */
        public short Flg { get; set; }

        /* number of envelope points */
        public short Pts { get; set; }

        /* envelope sustain index */
        public short Sus { get; set; }

        /* envelope loop begin */
        public short Beg { get; set; }

        /* envelope loop end */
        public short End { get; set; }

        /* current envelope counter */
        public short CurrentCounter { get; set; }

        /* envelope index a */
        public short EnvIdxA { get; set; }

        /* envelope index b */
        public short EnvIdxB { get; set; }

        /* envelope points */
        public EnvPt[] EnvPoints { get; set; }

        public EnvPr()
        {
        }
    }

}