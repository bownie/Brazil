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
            bool friendlier = false;
            BrazilApp app = null;

            if (friendlier)
            {
                // We can create a Friendlier
                //
                app = new Friendlier();
                app.initialise(State.Test("TextEditing"));
            }
            else
            {
                // Or we create a Paulo
                //
                app = new Paulo();
                app.initialise(State.Test("Menu"));
            }

            if (app != null)
            {
                app.run();
            }
        }
    }
#endif
}