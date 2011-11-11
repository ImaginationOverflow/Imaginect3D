using System;
using Microsoft.Research.Kinect.Nui;

namespace UI
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            try
            {

            
            using (Imaginect3D game = new Imaginect3D())
            {
                game.Run();
            }
            }
            catch (Exception e)
            {
                
                Runtime.Kinects[0].Uninitialize();
                throw e;
            }
        }
    }
#endif
}

