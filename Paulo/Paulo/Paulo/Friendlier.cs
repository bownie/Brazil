using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xyglo.Brazil;
using Xyglo.Brazil.Xna;

namespace Paulo
{
    /// <summary>
    /// Top level wrapper class for Friendlier functionality
    /// </summary>
    class Friendlier : BrazilApp
    {
        public Friendlier() 
        {
        }

        public override void initialise()
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

            // Set the project
            //
            m_viewSpace.setProject(project);

            // Connect ALT+O to the open file mode
            //
            //connect(State.Test("TextEditing"), new KeyAction(Keys.O, KeyboardModifier.Alt), Target.Test("OpenFile"));
            //connect(State.Test("TextEditing"), new KeyAction(Keys.S, KeyboardModifier.Alt), Target.Test("SaveFile"));

            // Connect some standard editor keys to states
            //
            //connectEditorKeys(State.TextEditing);
            //connectEditorKeys(State.FileSaveAs);
            //connectEditorKeys(State.FileOpen);
            //connectEditorKeys(State.PositionScreenOpen);
            //connectEditorKeys(State.PositionScreenNew);

            // Move the selects bufferview
            //
            //viewSpace.connect(State.TextEditing, new KeyAction(Keys.Left, KeyboardModifier.Alt), Target.
            //viewSpace.connectArrowKeys(State.
        }
    }
}
