using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Xyglo.Brazil
{
    using Xyglo.Brazil.Xna;

    /// <summary>
    /// A client should check the Input features that the world returns and use this
    /// to determine what it can connect and what it can't.  If it tries to connect an
    /// interface that isn't supported then an exception should be thrown.
    /// </summary>
    public enum InputFeatures
    {
        Dumb,
        Keyboard,
        Mouse,
        Gesture
    };

    /// <summary>
    /// Targets are things that the World implements and can be connected to actions
    /// from InputFeatures
    /// </summary>
    public enum Target
    {
        None,       // set if there is nothing to return for an action
        Default,
        CurrentBufferView,
        CreateNewBufferView,
        CompileStandard,
        CompileAlternate,
        OpenFile,
        SaveFile,
        CloseBufferView,
        LockMove,
        CursorDown,
        CursorUp,
        CursorLeft,
        CursorRight,
        Select
    };

    /// <summary>
    /// State is where the World can find itself either as the direct Action of an 
    /// Input or through some other aspect of interaction with the system or outside
    /// influence.
    /// </summary>
    public enum State
    {
        TextEditing,        // default mode
        FileOpen,           // opening a file
        FileSaveAs,         // saving a file as
        Information,        // show an information pane
        Help,               // show a help pane
        Configuration,      // configuration mode
        PositionScreenOpen, // where to position a screen when opening a file
        PositionScreenNew,  // where to position a new screen
        PositionScreenCopy, // where to position a copied FileBuffer/BufferView
        FindText,           // Enter some text to find
        ManageProject,      // View and edit the files in our project
        SplashScreen,       // What we see when we're arriving in the application
        DiffPicker,         // Mode for picking two files for differences checking
        WindowsRearranging, // When windows are flying between positions themselves
        GotoLine,           // Go to a line
        DemoExpired         // Demo period has expired
    };

    /// <summary>
    /// Confirmation state 
    /// </summary>
    public enum ConfirmState
    {
        None,
        FileSave,
        FileSaveCancel,
        CancelBuild,
        ConfirmQuit
    }

    /// <summary>
    /// We will use these to define the default states for a ViewSpace. ?????
    /// </summary>
    public enum DefaultStates
    {
        FreeSpaceStill,
        FreeSpaceWobbling
    }

    /// <summary>
    /// What mode is the ViewSpace in?
    /// </summary>
    public enum ViewSpaceMode
    {
        Window,
        FullScreen
    }

    /// <summary>
    /// ViewSpace is the interface to the graphical world that our Brazil objects live in.  The ViewSpace itself resides within a
    /// BrazilApp and allows the componentList generated at the BrazilApp level to be passed down to the world below where it 
    /// can be realised (XygloXNA for example).
    /// 
    /// 
    /// 
    /// </summary>
    public class ViewSpace : IDisposable
    {
        /// <summary>
        /// Default ViewSpace with nothing attached
        /// </summary>
        public ViewSpace()
        {
        }

        /// <summary>
        /// Initialise with a Friendlier Project
        /// </summary>
        /// <param name="actionMap"></param>
        /// <param name="project"></param>
        //public void initialise(ActionMap actionMap, Project project, List<Component> componentList, BrazilWorld  world)
        //{
            //m_xna = new XygloXNA(actionMap, project, componentList, world);
        //}

        /// <summary>
        /// Set the project in the XNA 
        /// </summary>
        /// <param name="project"></param>
        public void setProject(Project project)
        {
            m_xna.setProject(project);
        }

        /// <summary>
        /// Default initialise with just an ActionMap
        /// </summary>
        /// <param name="actionMap"></param>
        public void initialise(ActionMap actionMap, List<Component> componentList, BrazilWorld world)
        {
            m_xna = new XygloXNA(actionMap, componentList, world);

            /*
            // Get the features of the input
            //
            if (m_xna.getInputFeatures().Contains("Keyboard"))
            {
                m_inputFeatures |= InputFeatures.Keyboard;
            }

            if (m_xna.getInputFeatures().Contains("Mouse"))
            {
                m_inputFeatures |= InputFeatures.Mouse;
            }

            if (m_xna.getInputFeatures().Contains("Gesture"))
            {
                m_inputFeatures |= InputFeatures.Gesture;
            }

            Logger.logMsg("Input features = " + m_inputFeatures);
             * */
        }


        /// <summary>
        /// Return the input features
        /// </summary>
        /// <returns></returns>
        public List<string> getInputFeatures()
        {
            return m_xna.getInputFeatures();
        }

        /// <summary>
        /// Return the State
        /// </summary>
        /// <returns></returns>
        public List<String> getStates()
        {
            return m_xna.getStates();
        }

        /// <summary>
        /// Return the Targets
        /// </summary>
        /// <returns></returns>
        public List<string> getTargets()
        {
            return m_xna.getTargets();
        }

        // Run the main loop
        //
        public void run()
        {
            m_xna.Run();
        }

        /// <summary>
        /// Implement the Dispose method
        /// </summary>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// An XNA handle to display stuff
        /// </summary>
        protected XygloXNA m_xna = null;

        /// <summary>
        /// Object which has current focus
        /// </summary>
        protected XygloObject m_focusObject = null;

        /// <summary>
        /// Window mode
        /// </summary>
        protected ViewSpaceMode m_windowMode = ViewSpaceMode.Window;

    }
}
