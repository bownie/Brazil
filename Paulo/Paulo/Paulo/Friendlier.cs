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

            // Set up the keyboard behaviour
            //
            m_world.setKeyAutoRepeatHoldTime(0.4); // seconds
            m_world.setKeyAutoRepeatInterval(0.05); // seconds

            // Have to re-push these changes as the above key repeats have already
            // been loaded into the XNA world.
            //
            pushWorldChanges();
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
            connect("PositionScreenOpen", new KeyAction(Keys.Left), "Default");
            connect("PositionScreenOpen", new KeyAction(Keys.Right), "Default");
            connect("PositionScreenOpen", new KeyAction(Keys.Up), "Default");
            connect("PositionScreenOpen", new KeyAction(Keys.Down), "Default");

            // File save connection
            //
            connect("TextEditing", new KeyAction(Keys.S, KeyboardModifier.Alt), "SaveFile");

            // New BufferView with state keys for selecting direction - we don't need a lot of states
            // defining as our XygloXNA for Friendlier cheats a bit as it predates these methods.
            //
            //connect("TextEditing", new KeyAction(Keys.N, KeyboardModifier.Alt), "NewBufferView");
            //connect("TextEditing", new KeyAction(Keys.C, KeyboardModifier.Alt), "CloseBufferView");
            connect("PositionScreenNew", new KeyAction(Keys.Left), "Default");
            connect("PositionScreenNew", new KeyAction(Keys.Right), "Default");
            connect("PositionScreenNew", new KeyAction(Keys.Up), "Default");
            connect("PositionScreenNew", new KeyAction(Keys.Down), "Default");

            // Connect up the information screen and escape to get out of it
            //
            connect("TextEditing", new KeyAction(Keys.I, KeyboardModifier.Alt), "ShowInformation");
            connect("Information", Keys.Escape, "Default");

            // Connect up manage project
            //
            connect("TextEditing", new KeyAction(Keys.M, KeyboardModifier.Alt), "ManageProject");
            connect("ManageProject", Keys.Escape, "Default");

            // Ensure the escape key works in various modes
            //
            connect("Configuration", Keys.Escape, "Default");
            connect("Help", Keys.Escape, "Default");
            connect("Help", Keys.PageUp, "Default");
            connect("Help", Keys.PageDown, "Default");
            connect("FileOpen", Keys.Escape, "Default");
            connect("DiffPicker", Keys.Escape, "Default");

            // Save as
            connectEditorKeys("FileSaveAs");
            connect("FileSaveAs", Keys.Escape, "Default");

            // Connect some standard editor keys to states
            //
            connectEditorKeys("TextEditing");

            // Undo/redo
            //
            connect("TextEditing", new KeyAction(Keys.Z, KeyboardModifier.Control), "Default");
            connect("TextEditing", new KeyAction(Keys.Y, KeyboardModifier.Control), "Default");

            // CTRL + A
            //
            connect("TextEditing", new KeyAction(Keys.A, KeyboardModifier.Control), "Default");

            // Help screen
            //
            connect("TextEditing", new KeyAction(Keys.H, KeyboardModifier.Alt), "Help");

            // Find command
            //
            connect("TextEditing", new KeyAction(Keys.F, KeyboardModifier.Alt), "FindText");
            connectEditorKeys("FindText");
            connect("FindText", Keys.Escape, "Default");


            // Goto line
            connect("TextEditing", new KeyAction(Keys.L, KeyboardModifier.Alt), "GotoLine");
            connectEditorKeys("GotoLine");
            connect("GotoLine", Keys.Escape, "Default");

            // Information
            //
            connect("TextEditing", new KeyAction(Keys.I, KeyboardModifier.Alt), "Information");

            //connectEditorKeys("FileSaveAs");
            //connectEditorKeys("FileOpen");
            //connectEditorKeys("PositionScreenOpen");
            //connectEditorKeys("PositionScreenNew");

            // and the F keys
            initialiseFKeys();

            // Setup context menus
            //
            contextMenus();
        }

        /// <summary>
        /// Initialise the Function Keys
        /// </summary>
        protected void initialiseFKeys()
        {
            Keys[] keys = { Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8, Keys.F9, Keys.F10 };
            foreach (Keys key in keys)
            {
                connect("TextEditing", key);
            }
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
            string[] targets = { "None", "Default", "CurrentBufferView", "OpenFile", "SaveFile", "NewBufferView", "CursorUp", "CursorDown", "CursorRight", "CursorLeft", "ShowInformation" , "ManageProject", "Help", "Information", "FindText", "GotoLine"};
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

        protected void contextMenus()
        {
            // Set a context menu on right button press in the TextEditing state
            //
            BrazilMenu rightClickEditMenu = new BrazilMenu(BrazilMenuType.ContextMenu, new MouseAction(Mouse.RightButtonPress), "Right Click Edit Menu");
            rightClickEditMenu.addMenuOption("Copy", new KeyAction(Keys.C), "Copy");
            rightClickEditMenu.addMenuOption("Paste", new KeyAction(Keys.C), "Paste");
            StateAction sA = new StateAction(getState("TextEditing"), Mouse.RightButtonPress);
            addComponent(sA, rightClickEditMenu);
        }

    }
}
