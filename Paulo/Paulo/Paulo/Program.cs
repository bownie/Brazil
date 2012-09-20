using System;

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

            if (friendler)
            {
                // We can run our Friendlier
                //
                Friendlier friendlier = new Friendlier();
                friendlier.initialise();
                friendlier.run();
            }
            else
            {
                // Or we run our Paulo
                //
                Paulo paulo = new Paulo();
                paulo.initialise();
                paulo.run();
            }
        }
    }
#endif
}