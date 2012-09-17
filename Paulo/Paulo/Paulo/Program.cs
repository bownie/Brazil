using System;
using System.IO;
using System.Collections.Generic;
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

            // Create and initialise viewspace
            //
            ViewSpace viewSpace = new ViewSpace(project);
            viewSpace.initialise();

            // Connect up a key
            //
            //viewSpace.connectKey(State.TextEditing, Keys.A, Target.CurrentBufferView);
            //viewSpace.connectKey(State.TextEditing, Keys.B, Target.CurrentBufferView);
            //viewSpace.connectKey(State.TextEditing, Keys.Escape, Target.Default);

            // Connect ALT+O to the open file mode
            //
            viewSpace.connect(State.TextEditing, new KeyAction(Keys.O, KeyboardModifier.Alt), Target.OpenFile);
            viewSpace.connect(State.TextEditing, new KeyAction(Keys.S, KeyboardModifier.Alt), Target.SaveFile);

            //List<Xyglo.Brazil.Action> actionList = new List<Xyglo.Brazil.Action>();

            viewSpace.connectEditorKeys(State.TextEditing);
            viewSpace.connectEditorKeys(State.FileSaveAs);
            viewSpace.connectEditorKeys(State.FileOpen);
            viewSpace.connectEditorKeys(State.PositionScreenOpen);
            viewSpace.connectEditorKeys(State.PositionScreenNew);

            // Move the selects bufferview
            //
            //viewSpace.connect(State.TextEditing, new KeyAction(Keys.Left, KeyboardModifier.Alt), Target.

            //viewSpace.connectArrowKeys(State.

            // Run the viewspace
            //
            viewSpace.run();

        }
    }
#endif
}

