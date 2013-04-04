using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

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
    [DataContract(Namespace = "http://www.xyglo.com")]
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
        public void initialise(ActionMap actionMap, List<Component> componentList, BrazilWorld world, List<State> states, List<Target> targets, Dictionary<string, Resource> resources, string homePath)
        {
            m_xna = new XygloXNA(actionMap, componentList, world, states, targets, resources, homePath);
        }

        // Run the main loop
        //
        public void run(BrazilAppMode mode = BrazilAppMode.App)
        {
            // If we have XNA directly attached to this ViewSpace then we kick off the main loop.
            //
            if (m_xna != null && mode == BrazilAppMode.App)
            {
                m_xna.Run();
            }
            else
            {
                /*
                // If we're not then we have to work according to the hosting mode
                //
                if (mode == BrazilAppMode.Hosted)
                {
                    // do something
                }
                else if (mode == BrazilAppMode.Shared)
                {
                    // do something
                }
                else
                {*/
                    throw new XygloException("BrazilApp::run", "Unsupported app mode for a run");
                //}
            }
        }

        /// <summary>
        /// Get the Xna time in BrazilGameTime
        /// </summary>
        /// <returns></returns>
 /*
        public BrazilGameTime getAppTime()
        {
            if (m_xna != null)
                return XygloConvert.getBrazilGameTime(m_xna.getGameTime());

//            else
  //          {
    //            BrazilGameTime gameTime = new BrazilGameTime();
      //          gameTime.TotalGameTime = new TimeSpan(m_localTransport.getAppTime().Ticks);
        //        return gameTime;
          //  }
        }
        */

        /// <summary>
        /// Set the local app time
        /// </summary>
        /// <param name="gameTime"></param>
        public void setLocalAppTime(BrazilGameTime gameTime)
        {
            m_localTransport.setAppTime(new DateTime(gameTime.TotalGameTime.Ticks));
        }


        /// <summary>
        /// Reach through to set the transport object
        /// </summary>
        /// <param name="transport"></param>
        public void setTransport(BrazilTransport transport)
        {
            if (m_xna != null)
                m_xna.setTransport(transport);
            else
                m_localTransport = transport;
        }

        /// <summary>
        /// Set the State of the app
        /// </summary>
        /// <param name="state"></param>
        public void setState(State state)
        {
            if (m_xna != null)
                m_xna.setState(state);
            else
                m_localState = state;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public State getState()
        {
            if (m_xna != null)
                return m_xna.getState();
            else
                return m_localState;
        }

        /// <summary>
        /// Tell the XNA layer to re-read world data
        /// </summary>
        public void pushWorld()
        {
            m_xna.pushWorld();
        }

        /// <summary>
        /// Reach through a send a message to the GUI
        /// </summary>
        /// <param name="message"></param>
        /// <param name="time"></param>
        public void sendMessage(string message, double time)
        {
            m_xna.setTemporaryMessage(message, time);
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
        [NonSerialized]
        protected XygloXNA m_xna = null;

        /// <summary>
        /// Object which has current focus
        /// </summary>
        [NonSerialized]
        protected XygloObject m_focusObject = null;

        /// <summary>
        /// Window mode
        /// </summary>
        [DataMember]
        protected ViewSpaceMode m_windowMode = ViewSpaceMode.Window;

        /// <summary>
        /// We use a local state here at the ViewSpace level IFF there is no XNA to connect to -
        /// this can occur when we're working in the Hosted App mode and we're already running 
        /// within say an XNA instance.  Then we are running this ViewSpace and World within another
        /// and want to store our own state locally here.
        /// </summary>
        [DataMember]
        protected State m_localState;

        /// <summary>
        /// Simialr to above we may have no XNA to connect to within the Viewspace - if you're running
        /// in  Hosted mode we want to store the transport state at this level instead.
        /// </summary>
        [DataMember]
        protected BrazilTransport m_localTransport;
    }
}
