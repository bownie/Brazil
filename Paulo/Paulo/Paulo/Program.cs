using System;
using Xyglo.Brazil;

namespace Paulo
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]// Setting this as single threaded for cut and paste of all things to work
        static void Main(string[] args)
        {
            bool friendler = false;
            BrazilApp app = null;

            if (friendler)
            {
                // We can create a Friendlier
                //
                app = new Friendlier();
            }
            else
            {
                // Or we create a Paulo
                //
                app = new Paulo();
            }

            if (app != null)
            {
                app.initialise();
                app.run();
            }
        }
    }
#endif
}