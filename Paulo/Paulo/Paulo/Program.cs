using System;
using System.IO;
using Xyglo.Brazil;
using Xyglo.Brazil.Xna;

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
            // Create a FontManager
            //
            FontManager fontManager = new FontManager();

            Project project;
            string projectFile = Project.getUserDataPath() + "default_project.xml";

            if (File.Exists(projectFile))
            {
                project = Project.dataContractDeserialise(fontManager, projectFile);
            }
            else
            {
                project = new Project(fontManager, "New Project", projectFile);
            }

            project.setLicenced(true);
            project.setViewMode(Project.ViewMode.Formal);

            ViewSpace viewSpace = new ViewSpace(project);
            viewSpace.initialise();


            // Connect up a key
            //
            viewSpace.connectKey(State.TextEditing, Keys.A, Target.CurrentBufferView);
            viewSpace.connectKey(State.TextEditing, Keys.B, Target.CurrentBufferView);

            viewSpace.run();
        }
    }
#endif
}

