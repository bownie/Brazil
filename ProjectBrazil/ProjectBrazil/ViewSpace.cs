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
        Default,
        CurrentBufferView,
        CreateNewBufferView,
        CompileStandard,
        CompileAlternate,
        OpenFile,
        CloseBufferView,
        LockMove
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
    /// We will use these to define the default states for a ViewSpace. ?????
    /// </summary>
    public enum DefaultStates
    {
        FreeSpaceStill,
        FreeSpaceWobbling
    }

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
    /// What mode is the ViewSpace in?
    /// </summary>
    public enum ViewSpaceMode
    {
        Window,
        FullScreen
    }

    /// <summary>
    /// ViewSpace is the world that our Xyglot objects live in.  The ViewSpace wraps any of the platform specific functionality that we want to implement
    /// therefore it contains any technology specific objects we need to initialise to provide a presentation.  Think of Xyglot as part game engine, part
    /// window manager - we are providing a framework in which to present items as well as some of the building blocks (widgets if you like) that can be
    /// used to create an application.  These widgets aim to be as consistent as possible across platforms hence the building of this abstration - once
    /// you learn to use Xyglot once you can use it on other platforms with similar results.
    ///
    /// Additional benefit to seperating the device specific graphics from the core engine of the application is for clarity.  Xyglot will only be 
    /// responsible for its interfaces and not responsible for game or application logic aside from that.
    /// 
    /// </summary>
    public class ViewSpace
    {
        public ViewSpace(Project project)
        {
            m_project = project;
        }

        public void initialise()
        {
            m_xna = new XygloXNA(m_project, m_actionMap);

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
        /// Connect a Key to a Target in a State
        /// </summary>
        /// <param name="state"></param>
        /// <param name="key"></param>
        /// <param name="target"></param>
        public void connectKey(State state, Keys key, Target target = Target.Default)
        {
            m_actionMap.setAction(state, new KeyAction(key), target);
        }

        /// <summary>
        /// Connect up States, generic Action and Targets
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actions"></param>
        /// <param name="target"></param>
        public void connect(State state, Action action, Target target = Target.Default)
        {
            m_actionMap.setAction(state, action, target);
        }

        /// <summary>
        /// We use connect to define states, actions and targets for those actions
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actions"></param>
        /// <param name="target"></param>
        public void connect(State state, List<Action> actions, Target target = Target.Default)
        {
            // Roll through all the actions 
            foreach (Action action in actions)
            {
                m_actionMap.setAction(state, action, target);
            }
        }

        /// <summary>
        /// Connect all the normal editor keys to a target - if the target is
        /// not specified then we assume the default target is the focus object.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="target"></param>
        public void connectEditorKeys(State state, Target target = Target.Default)
        {
            if (target == Target.Default)
            {
                // do something with the default target
            }

            //m_actionMap.setAction(state, 
        }

        /// <summary>
        /// Default to no input available from our world
        /// </summary>
        //protected InputFeatures m_inputFeatures = InputFeatures.Dumb;

        /// <summary>
        /// A project containing stuff
        /// </summary>
        protected Project m_project = null;

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

        /// <summary>
        /// Action map
        /// </summary>
        protected ActionMap m_actionMap = new ActionMap();

    }

}
