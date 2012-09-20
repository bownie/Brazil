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

        public void initialise()
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
            ViewSpace viewSpace = new ViewSpace();
            viewSpace.initialise(m_actionMap, project, m_componentList);

            // Connect up a key
            //
            //viewSpace.connectKey(State.TextEditing, Keys.A, Target.CurrentBufferView);
            //viewSpace.connectKey(State.TextEditing, Keys.B, Target.CurrentBufferView);
            //viewSpace.connectKey(State.TextEditing, Keys.Escape, Target.Default);

            // Connect ALT+O to the open file mode
            //
            connect(State.TextEditing, new KeyAction(Keys.O, KeyboardModifier.Alt), Target.OpenFile);
            connect(State.TextEditing, new KeyAction(Keys.S, KeyboardModifier.Alt), Target.SaveFile);

            //List<Xyglo.Brazil.Action> actionList = new List<Xyglo.Brazil.Action>();

            connectEditorKeys(State.TextEditing);
            connectEditorKeys(State.FileSaveAs);
            connectEditorKeys(State.FileOpen);
            connectEditorKeys(State.PositionScreenOpen);
            connectEditorKeys(State.PositionScreenNew);

            // Move the selects bufferview
            //
            //viewSpace.connect(State.TextEditing, new KeyAction(Keys.Left, KeyboardModifier.Alt), Target.
            //viewSpace.connectArrowKeys(State.
        }
    }
}
