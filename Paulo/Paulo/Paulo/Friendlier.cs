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
            initialiseStates();
        }

        /// <summary>
        /// Initialise Friendlier
        /// </summary>
        /// <param name="state"></param>
        public override void initialise(State state)
        {
            // Set the initial state
            //
            setInitialState(state);

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
            connect("TextEditing", new KeyAction(Keys.O, KeyboardModifier.Alt), "OpenFile");
            connect("TextEditing", new KeyAction(Keys.S, KeyboardModifier.Alt), "SaveFile");

            // Connect some standard editor keys to states
            //
            connectEditorKeys("TextEditing");
            connectEditorKeys("FileSaveAs");
            connectEditorKeys("FileOpen");
            connectEditorKeys("PositionScreenOpen");
            connectEditorKeys("PositionScreenNew");

            // Move the selects bufferview
            //
            //viewSpace.connect(State.TextEditing, new KeyAction(Keys.Left, KeyboardModifier.Alt), Target.
            //viewSpace.connectArrowKeys(State.
        }

        /// <summary>
        /// Initialise the states - might find a way to genericise this
        /// </summary>
        protected void initialiseStates()
        {
            // States of the application - where are we in the navigation around the app.  States will affect what 
            // components are showing and how we interact with them.
            //
            string[] states = { "TextEditing", "DemoExpired", "FileSaveAs", "FileOpen", "Configuration", "PositionScreenOpen", "PositionScreenNew", "PositionScreenCopy", "SplashScreen", "Information", "Help", "DiffPicker", "ManageProject", "FindText", "GotoLine", "FileSave" };
            foreach (string state in states)
            {
                addState(state);
            }

            // Targets are actions we can perform in the application - a default target will usually mean the object
            // in focus within the current State.  This should be defined by the component.
            //
            string[] targets = { "None", "Default", "CurrentBufferView", "OpenFile", "SaveFile", "CursorUp", "CursorDown", "CursorRight", "CursorLeft" };
            foreach (string target in targets)
            {
                addTarget(target);
            }

            // Confirm states are substates used when confirming actions or movements between states.  These
            // give greater control over movement between States.
            //
            string[] confirmStates = { "None", "ConfirmQuit", "FileSaveCancel", "CancelBuild", "FileSave" };
            foreach (string confirmState in confirmStates)
            {
                addConfirmState(confirmState);
            }
        }
    }
}
