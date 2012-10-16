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
            bool friendlier = true;
            BrazilApp app = null;

            if (friendlier)
            {
                // We can create a Friendlier
                //
                app = new Friendlier();

                // And initiliase with default state of TextEditing
                //
                app.initialise(State.Test("TextEditing"));
            }
            else
            {
                // Or we create a Paulo with an initial gravity vector
                //
                app = new Paulo(new BrazilVector3(0, 0.1f, 0));

                // Initialise with default state
                //
                app.initialise(State.Test("Menu"));
            }

            // If we have something then run it
            //
            if (app != null)
            {
                app.run();
            }
        }
    }
#endif
}