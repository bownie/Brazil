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
    /// We will use these to define the default states for a ViewSpace. ?????  Probably won't be usedd.
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
        public void initialise(ActionMap actionMap, List<Component> componentList, BrazilWorld world, List<State> states, List<Target> targets)
        {
            m_xna = new XygloXNA(actionMap, componentList, world, states, targets);
        }

       // Run the main loop
        //
        public void run()
        {
            m_xna.Run();
        }

        /// <summary>
        /// Set the State of the app
        /// </summary>
        /// <param name="state"></param>
        public void setState(State state)
        {
            
            m_xna.setState(state);
        }

        public State getState()
        {
            return m_xna.getState();
        }

        /// <summary>
        /// Tell the XNA layer to re-read world data
        /// </summary>
        public void pushWorld()
        {
            m_xna.pushWorld();
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
