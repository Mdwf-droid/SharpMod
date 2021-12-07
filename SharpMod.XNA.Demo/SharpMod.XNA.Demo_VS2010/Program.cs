using System;

namespace SharpMod.XNA.WP7.Demo_VS2010
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (SharpModApp game = new SharpModApp())
            {
                game.Run();
            }
        }
    }
#endif
}

