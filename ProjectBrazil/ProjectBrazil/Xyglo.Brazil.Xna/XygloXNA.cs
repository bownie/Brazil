#region File Description
//-----------------------------------------------------------------------------
// Friendlier.cs
//
// Copyright (C) Xyglo. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using BloomPostprocess;
using System.Security.Permissions;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// XygloXNA is defined by a XNA Game class - the core of the XNA world.
    /// </summary>
    public class XygloXNA : Game
    {
        ///////////////// MEMBER VARIABLES //////////////////

        /// <summary>
        /// Store a map of all keys that are being pressed or held - bool is true for the first
        /// pass through for the 'initial' hold.  Double is for the GameTime in TotalSeconds.
        /// </summary>
        protected Dictionary<Keys, Pair<bool, double>> m_keyMap = new Dictionary<Keys, Pair<bool, double>>();

        /// <summary>
        /// Source BufferView when doing a drag of text etc
        /// </summary>
        protected BufferView m_sourceBufferView = null;

        /// <summary>
        /// Component list is passed in from the BrazilApp
        /// </summary>
        List<Component> m_componentList = null;

        /// <summary>
        /// Get the list of States from the BrazilApp
        /// </summary>
        List<State> m_states = null;

        /// <summary>
        /// Get the list of Targets from the BrazilApp
        /// </summary>
        List<Target> m_targets = null;

        /// <summary>
        /// BrazilWorld holds some Worldly information for us
        /// </summary>
        BrazilWorld m_world = null;

        /// <summary>
        /// A drawable, keyed component dictionary
        /// </summary>
        Dictionary<Component, XygloXnaDrawable> m_drawableComponents = new Dictionary<Component, XygloXnaDrawable>();

        /// <summary>
        /// A list of temporary Drawables - everything on here must have a time to live set for them
        /// or a scope defined to get rid of them from this list.
        /// </summary>
        Dictionary<BrazilTemporary, XygloXnaDrawable> m_temporaryDrawables = new Dictionary<BrazilTemporary, XygloXnaDrawable>();

        // XNA stuff
        //
        GraphicsDeviceManager m_graphics;

        /// <summary>
        /// One SpriteBatch
        /// </summary>
        SpriteBatch m_spriteBatch;

        /// <summary>
        /// Another SpriteBatch for the overlay
        /// </summary>
        SpriteBatch m_overlaySpriteBatch;

        /// <summary>
        /// A third SpriteBatch for panners/differs etc utilising alpha
        /// </summary>
        SpriteBatch m_pannerSpriteBatch;

        /// <summary>
        /// The state of our application - what we're doing at the moment
        /// </summary>
        State m_state;

        /// <summary>
        /// Basic effect has with textures for fonts
        /// </summary>
        BasicEffect m_basicEffect;

        /// <summary>
        /// Line effect has no textures
        /// </summary>
        BasicEffect m_lineEffect;

        /// <summary>
        /// Projection Matrix
        /// </summary>
        Matrix m_projection;

        /// <summary>
        /// Eye/Camera location
        /// </summary>
        Vector3 m_eye = new Vector3(0f, 0f, 500f);  // 275 is good

        /// <summary>
        /// Camera target
        /// </summary>
        Vector3 m_target;

        /// <summary>
        /// Are we spinning?
        /// </summary>
        bool m_spinning = false;

        /// <summary>
        /// The bloom component
        /// </summary>
        BloomComponent m_bloom;

        // Current bloom settings index
        //
        int m_bloomSettingsIndex = 0;

        /// <summary>
        /// A local Differ object
        /// </summary>
        protected Differ m_differ = null;

        /// <summary>
        /// Interloper object - our game 
        /// </summary>
        BrazilInterloper m_interloper = null;

        /// <summary>
        /// Position we are in the diff
        /// </summary>
        protected int m_diffPosition = 0;

        /// <summary>
        /// Current project
        /// </summary>
        static protected Project m_project;

        /// <summary>
        /// We need a FontManager at this level now (not project)
        /// </summary>
        protected FontManager m_fontManager = new FontManager();

        /// <summary>
        /// Last keyboard state so that we can compare with current
        /// </summary>
        protected KeyboardState m_lastKeyboardState;

        /// <summary>
        /// Is either Shift key being held?
        /// </summary>
        protected bool m_shiftDown = false;

        /// <summary>
        /// Is either Control key being held down?
        /// </summary>
        protected bool m_ctrlDown = false;

        /// <summary>
        /// Is either Alt key being held down?
        /// </summary>
        protected bool m_altDown = false;

        /// <summary>
        /// Is either Windows key held down?
        /// </summary>
        protected bool m_windowsDown = false;

        /// <summary>
        /// Use this to store number when we've got ALT down - to select a new BufferView
        /// </summary>
        protected string m_gotoBufferView = "";

        // Stopwatch
        //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        /// <summary>
        /// The position where the project model will be viewable
        /// </summary>
        protected Vector3 m_projectPosition = Vector3.Zero;

        /// <summary>
        /// Goto line string holder
        /// </summary>
        protected string m_gotoLine = "";

        /// <summary>
        /// Flag used to confirm quit
        /// </summary>
        protected bool m_confirmQuit = false;

        /// <summary>
        /// The index of the last directory we went into so we can save it
        /// </summary>
        protected int m_lastHighlightIndex = 0;

        /// <summary>
        /// Turn on and off file save confirmation
        /// </summary>
        protected bool m_confirmFileSave = false;

        /// <summary>
        /// Confirmation state - expecting Y/N
        /// </summary>
        public ConfirmState m_confirmState = ConfirmState.Test("None");

        /// <summary>
        /// A flat texture we use for drawing coloured blobs like highlighting and cursors
        /// </summary>
        protected Texture2D m_flatTexture;

        /// <summary>
        /// A rendertarget for the text scroller
        /// </summary>
        protected RenderTarget2D m_textScroller;

        /// <summary>
        /// A texture we can render a text string to and scroll
        /// </summary>
        protected Texture2D m_textScrollTexture;

        /// <summary>
        /// Our view matrix
        /// </summary>
        Matrix m_viewMatrix = new Matrix();

        /// <summary>
        /// A bounding frustrum to allow us to cull objects not visible
        /// </summary>
        protected BoundingFrustum m_frustrum;

        /// <summary>
        /// We can use this to communicate something to the user about the last command
        /// </summary>
        string m_temporaryMessage = "";

        /// <summary>
        /// Start time for the temporary message
        /// </summary>
        double m_temporaryMessageStartTime;

        /// <summary>
        /// End time for the temporary message
        /// </summary>
        double m_temporaryMessageEndTime;

        /// <summary>
        /// Used when displaying licence messages
        /// </summary>
        private bool m_flipFlop = true;

        /// <summary>
        /// Something to do with licence messages
        /// </summary>
        private double m_nextLicenceMessage = 0.0f;

        /// <summary>
        /// File system watcher
        /// </summary>
        protected List<FileSystemWatcher> m_watcherList = new List<FileSystemWatcher>();

        /// <summary>
        /// Position in which we should open or create a new screen
        /// </summary>
        protected BufferView.ViewPosition m_newPosition;

        /// <summary>
        /// Store the last performance counter for CPU
        /// </summary>
        protected CounterSample m_lastCPUSample;

        /// <summary>
        /// Store the last performance counter for CPU
        /// </summary>
        protected CounterSample m_lastMemSample;

        /// <summary>
        /// Number of milliseconds between system status fetches
        /// </summary>
        protected TimeSpan m_systemFetchSpan = new TimeSpan(0, 0, 0, 1, 0);

        /// <summary>
        /// When we last fetched the system status
        /// </summary>
        protected TimeSpan m_lastSystemFetch = new TimeSpan(0, 0, 0, 0, 0);

        /// <summary>
        /// Are we allowed to process keyboard events?
        /// </summary>
        protected TimeSpan m_processKeyboardAllowed = TimeSpan.Zero;

        /// <summary>
        /// Percentage of system load
        /// </summary>
        protected float m_systemLoad = 0.0f;

        /// <summary>
        /// Percentage of system load
        /// </summary>
        protected float m_memoryAvailable = 0.0f;

        /// <summary>
        /// Physical Memory 
        /// </summary>
        protected float m_physicalMemory;

        /// <summary>
        /// List of files that need writing
        /// </summary>
        protected List<FileBuffer> m_filesToWrite;

        /// <summary>
        /// File selected in Open state - to be opened
        /// </summary>
        protected string m_selectedFile;

        /// <summary>
        /// Read only status of file to be opened (m_selectedFile)
        /// </summary>
        protected bool m_fileIsReadOnly = false;

        /// <summary>
        /// Tailing status of file to be opened (m_selectedFile)
        /// </summary>
        protected bool m_fileIsTailing = false;

        /// <summary>
        /// Current Z position - we call it m_zoomLevel
        /// </summary>
        protected float m_zoomLevel = 500.0f;

        /// <summary>
        /// Step for zooming
        /// </summary>
        protected float m_zoomStep = 50.0f;

        /// <summary>
        /// The new destination for our Eye position
        /// </summary>
        protected Vector3 m_newEyePosition;

        /// <summary>
        /// Original eye position - we know where we came from
        /// </summary>
        protected Vector3 m_originalEyePosition;

        /// <summary>
        /// Eye acceleration vector
        /// </summary>
        protected Vector3 m_eyeAcc = Vector3.Zero;

        /// <summary>
        /// Eye velocity vector
        /// </summary>
        protected Vector3 m_eyeVely = Vector3.Zero;

        /// <summary>
        /// Used to hold initial fractional value of a target font size when changing font sizes
        /// </summary>
        protected double m_fontScaleOriginal;

        /// <summary>
        /// Holds current font scale whilst scaling current BufferView
        /// </summary>
        protected double m_currentFontScale;

        /// <summary>
        /// Are we changing eye position?
        /// </summary>
        protected bool m_changingEyePosition = false;

        /// <summary>
        /// Used when changing the eye position - movement timer
        /// </summary>
        protected TimeSpan m_changingPositionLastGameTime;

        /// <summary>
        /// Frame rate of animation when moving between eye positions
        /// </summary>
        protected TimeSpan m_movementPause = new TimeSpan(0, 0, 0, 0, 10);

        /// <summary>
        /// This is the vector we're flying in - used to increment position each frame when
        /// moving between eye positions.
        /// </summary>
        protected Vector3 m_vFly;

        /// <summary>
        /// If our target position is not centred below our eye then we also have a vector here we need to
        /// modify.
        /// </summary>
        protected Vector3 m_vFlyTarget;

        /// <summary>
        /// How many steps between eye start and eye end fly position
        /// </summary>
        protected int m_flySteps = 15;

        /// <summary>
        /// An object that wraps our view of the file system
        /// </summary>
        protected FileSystemView m_fileSystemView;

        /// <summary>
        /// A variable we use to store our save filename as we edit it (we have no forms)
        /// </summary>
        protected string m_saveFileName;

        /// <summary>
        /// Position in configuration list when selecting something
        /// </summary>
        protected int m_configPosition;

        /// <summary>
        /// Item colour for a list of things
        /// </summary>
        protected Color m_itemColour = Color.DarkOrange;

        /// <summary>
        /// Highlight color for an element in a list
        /// </summary>
        protected Color m_highlightColour = Color.LightGreen;

        /// <summary>
        /// If we're in the Configuration state then look at this variable
        /// </summary>
        protected bool m_editConfigurationItem = false;

        /// <summary>
        /// The new value of the configuration item
        /// </summary>
        protected string m_editConfigurationItemValue;

        /// <summary>
        /// Worker thread for the PerformanceCounters
        /// </summary>
        protected PerformanceWorker m_counterWorker;

        /// <summary>
        /// The thread that is used for the counter
        /// </summary>
        protected Thread m_counterWorkerThread;

        /// <summary>
        /// SmartHelp worker thread
        /// </summary>
        protected SmartHelpWorker m_smartHelpWorker;

        /// <summary>
        /// The thread that is used for the counter
        /// </summary>
        protected Thread m_smartHelpWorkerThread;

        /// <summary>
        /// Worker thread for the Kinect management
        /// </summary>
        protected KinectWorker m_kinectWorker;

        /// <summary>
        /// The thread that is used for Kinect
        /// </summary>
        protected Thread m_kinectWorkerThread;

        /// <summary>
        /// Store GameTime somewhere central
        /// </summary>
        protected GameTime m_gameTime;

        /// <summary>
        /// View for the Standard Output of a build command
        /// </summary>
        protected BufferView m_buildStdOutView;

        /// <summary>
        /// View for the Standard Error of a build command
        /// </summary>
        protected BufferView m_buildStdErrView;

        /// <summary>
        /// Process for running builds
        /// </summary>
        Process m_buildProcess = null;

        /// <summary>
        /// Exit after save as
        /// </summary>
        protected bool m_saveAsExit = false;

        /// <summary>
        /// Generate a tree from a Friendlier structure
        /// </summary>
        protected TreeBuilder m_treeBuilder = new TreeBuilder();

        /// <summary>
        /// Model builder realises a model from a tree
        /// </summary>
        protected ModelBuilder m_modelBuilder;

        /// <summary>
        /// Text information screen y offset for page up and page down purposes
        /// </summary>
        protected int m_textScreenPositionY = 0;

        /// <summary>
        /// Length of information screen - so we know if we can page up or down
        /// </summary>
        protected int m_textScreenLength = 0;

        /// <summary>
        /// Config screen x direction
        /// </summary>
        protected int m_configXOffset = 0;

        /// <summary>
        /// Last mouse state
        /// </summary>
        protected MouseState m_lastMouseState;

        /// <summary>
        /// Time of last click
        /// </summary>
        protected TimeSpan m_lastClickTime = TimeSpan.Zero;

        /// <summary>
        /// Last position of the click
        /// </summary>
        protected Vector3 m_lastClickEyePosition = Vector3.Zero;

        /// <summary>
        /// Use this for highlighting a selected BufferView temporarily
        /// </summary>
        protected Pair<BufferView, Highlight> m_clickHighlight = new Pair<BufferView, Highlight>();

        /// <summary>
        /// Time for key auto-repeat to start - defaults to zero
        /// </summary>
        double m_repeatHoldTime = 0; // seconds

        // Time between autorepeats
        //
        double m_repeatInterval = 0.05; // second

        /// <summary>
        /// Testing whether arrived in bounding sphere
        /// </summary>
        protected BoundingSphere m_testArrived = new BoundingSphere();

        /// <summary>
        /// Test result
        /// </summary>
        protected ContainmentType m_testResult;

        /// <summary>
        /// Set field of view of the camera in radians
        /// </summary>
        protected float m_fov = MathHelper.PiOver4;

        /// <summary>
        /// Is this Window resizable - for the moment it isn't
        /// </summary>
        protected bool m_isResizable = false;

        /// <summary>
        /// Are we resizing the main window?
        /// </summary>
        protected bool m_isResizing = false;

        /// <summary>
        /// Store the last window size in case we're resizing
        /// </summary>
        protected Vector2 m_lastWindowSize = Vector2.Zero;

        /// <summary>
        /// Spalsh screen texture
        /// </summary>
        protected Texture2D m_splashScreen;

        /// <summary>
        /// Mouse wheel value
        /// </summary>
        protected int m_lastMouseWheelValue = 0;

        /// <summary>
        /// Position of last mouse click in screen coordinations
        /// </summary>
        protected Vector3 m_lastClickPosition = Vector3.Zero;

        /// <summary>
        /// Position in the World of the last click
        /// </summary>
        protected Vector3 m_lastClickWorldPosition = Vector3.Zero;

        /// <summary>
        /// Where the cursor was clicked within a BufferView
        /// </summary>
        protected Vector2 m_lastClickCursorOffsetPosition = Vector2.Zero;

        /// <summary>
        /// Vector resulting from last mouse click
        /// </summary>
        protected Vector3 m_lastClickVector = Vector3.Zero;

        /// <summary>
        /// A helper class for drawing things
        /// </summary>
        protected DrawingHelper m_drawingHelper;

        /// <summary>
        /// ActionMap is a Project Brazil reference gets passed from the constructor
        /// </summary>
        protected ActionMap m_actionMap = null;

        /// <summary>
        /// Part of our botched keyboard managament routines
        /// </summary>
        //protected Keys m_currentKeyDown;

        /// <summary>
        /// Frame rate
        /// </summary>
        int m_frameRate = 0;

        /// <summary>
        /// Frame counter
        /// </summary>
        int m_frameCounter = 0;

        /// <summary>
        /// Elapse time to help calculate frame rate
        /// </summary>
        TimeSpan m_elapsedTime = TimeSpan.Zero;

        /////////////////////////////// CONSTRUCTORS ////////////////////////////
        /// <summary>
        /// Default constructor
        /// </summary>
        public XygloXNA(ActionMap actionMap, List<Component> componentList, BrazilWorld world, List<State> states, List<Target> targets)
        {
            m_actionMap = actionMap;
            m_componentList = componentList;
            m_world = world;
            m_states = states;
            m_targets = targets;

            // Get these values from the World
            //
            m_repeatHoldTime = world.getKeyAutoRepeatHoldTime();
            m_repeatInterval = world.getKeyAutoRepeatInterval();

            initialise();
        }

        /// <summary>
        /// Construct with Project and ActionMap (reference)
        /// </summary>
        /// <param name="project"></param>
        /// <param name="actionMap"></param>
        public XygloXNA(ActionMap actionMap, Project project, List<Component> componentList, BrazilWorld world, List<State> states, List<Target> targets)
        {
            // Store project and actionmap
            //
            m_project = project;
            m_actionMap = actionMap;
            m_componentList = componentList;
            m_world = world;
            m_states = states;
            m_targets = targets;

            // Get these values from the World
            //
            m_repeatHoldTime = world.getKeyAutoRepeatHoldTime();
            m_repeatInterval = world.getKeyAutoRepeatInterval();

            // init
            initialise();
        }

        /////////////////////////////// METHODS //////////////////////////////////////
        /// <summary>
        /// Set the project
        /// </summary>
        /// <param name="project"></param>
        public void setProject(Project project)
        {
            m_project = project;
        }

        /// <summary>
        /// Get the project
        /// </summary>
        /// <returns></returns>
        public Project getProject()
        {
            return m_project;
        }

        /// <summary>
        /// Allow some world parameters to be re-read
        /// </summary>
        /// <param name="keyHold"></param>
        /// <param name="keyRepeatInterval"></param>
        public void pushWorld()
        {
            // Get these values from the World
            //
            m_repeatHoldTime = m_world.getKeyAutoRepeatHoldTime();
            m_repeatInterval = m_world.getKeyAutoRepeatInterval();
        }

        /// <summary>
        /// Check a State exists and throw up if not
        /// </summary>
        /// <param name="state"></param>
        protected State confirmState(string stateName)
        {
            foreach (State state in m_states)
            {
                if (state.m_name == stateName)
                {
                    return state;
                }
            }
            throw new Exception("Unrecognized state " + stateName);
        }

        /// <summary>
        /// Check a Target exists
        /// </summary>
        /// <param name="target"></param>
        protected string confirmTarget(string targetName)
        {
            foreach (Target target in m_targets)
            {
                if (target.m_name == targetName)
                {
                    return targetName;
                }
            }
            throw new Exception("Unrecognised target " + targetName);
        }

        /// <summary>
        /// Initialise some stuff in the constructor
        /// </summary>
        protected void initialise()
        {
            Logger.logMsg("XygloXNA::initialise() - loading components");

            m_graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Initialise the bloom component
            //
            m_bloom = new BloomComponent(this);
            m_bloom.Settings = BloomSettings.PresetSettings[5];
            Components.Add(m_bloom);

            // Antialiasing
            //
            m_graphics.PreferMultiSampling = true;

            // Set physical memory
            //
            Microsoft.VisualBasic.Devices.ComputerInfo ci = new Microsoft.VisualBasic.Devices.ComputerInfo();
            m_physicalMemory = (float)(ci.TotalPhysicalMemory / (1024 * 1024));

            // Set windowed mode as default
            //
            windowedMode();

            // Check the demo status and set as necessary
            //
            if (m_project != null && !m_project.getLicenced())
            {
                m_state = State.Test("DemoExpired");
            }
        }

        /// <summary>
        /// Get the FileBuffer id of the active view
        /// </summary>
        /// <returns></returns>
        protected int getActiveBufferIndex()
        {
            return m_project.getFileBuffers().IndexOf(m_project.getSelectedBufferView().getFileBuffer());
        }

        /// <summary>
        /// Get the current application State
        /// </summary>
        /// <returns></returns>
        public State getState()
        {
            return m_state;
        }

        /// <summary>
        /// Set the State - we usually only do this once per instantiation and then let the world
        /// just live.
        /// </summary>
        /// <param name="state"></param>
        public void setState(State state)
        {
            m_state = state;
        }

        /// <summary>
        /// Enable windowed mode
        /// </summary>
        protected void windowedMode()
        {
            // Some of the modes we've used
            //
            //InitGraphicsMode(640, 480, false);
            //InitGraphicsMode(720, 576, false);
            //InitGraphicsMode(800, 500, false);
            //InitGraphicsMode(960, 768, false);
            //InitGraphicsMode(1920, 1080, true);

            int maxWidth = 0;
            int maxHeight = 0;

            foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                // Set both when either is greater
                if (dm.Width > maxWidth || dm.Height > maxHeight)
                {
                    maxWidth = dm.Width;
                    maxHeight = dm.Height;
                }
            }

            // Defaults
            //
            if (m_project != null) m_project.getFontManager().setSmallScreen(true);
            int windowWidth = 640;
            int windowHeight = 480;

            if (maxWidth >= 1920)
            {
                windowWidth = 960;
                windowHeight = 768;
                if (m_project != null) m_project.getFontManager().setSmallScreen(false);
            }
            else if (maxWidth >= 1280)
            {
                windowWidth = 800;
                windowHeight = 500;
            }
            else if (maxWidth >= 1024)
            {
                windowWidth = 720;
                windowHeight = 576;
            }

            // Set this for storage
            //
            if (m_project != null)
            {
                m_project.setWindowSize(windowWidth, windowHeight);
                m_project.setFullScreen(false);
            }

            // Set the graphics modes
            initGraphicsMode(windowWidth, windowHeight, false);

            // Update this to ensure scanner appears in the right place
            //
            if (m_drawingHelper != null)
            {
                m_drawingHelper.setPreviewBoundingBox(m_graphics.GraphicsDevice.Viewport.Width, m_graphics.GraphicsDevice.Viewport.Height);
            }
        }

        /// <summary>
        /// Enable full screen mode
        /// </summary>
        protected void fullScreenMode()
        {
            int maxWidth = 0;
            int maxHeight = 0;

            foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                // Set both when either is greater
                if (dm.Width > maxWidth || dm.Height > maxHeight)
                {
                    maxWidth = dm.Width;
                    maxHeight = dm.Height;
                }
            }

            m_project.setFullScreen(true);

            // Set the graphics modes
            initGraphicsMode(maxWidth, maxHeight, true);

            // Update this to ensure scanner appears in the right place
            //
            if (m_drawingHelper != null)
            {
                m_drawingHelper.setPreviewBoundingBox(m_graphics.GraphicsDevice.Viewport.Width, m_graphics.GraphicsDevice.Viewport.Height);
            }
        }

        /// <summary>
        /// Initialise the project - load all the FileBuffers and select the correct BufferView
        /// </summary>
        /// <param name="project"></param>
        protected void initialiseProject()
        {
            Logger.logMsg("XygloXNA::initialiseProject() - initialising fonts");

            // We need to do this to connect up all the BufferViews, FileBuffers and the other components
            // such as FontManager etc.
            //
            m_project.connectFloatingWorld();

            // Reconnect these views if they exist
            //
            m_buildStdOutView = m_project.getStdOutView();
            m_buildStdErrView = m_project.getStdErrView();

            // Set the tab space
            //
            m_project.setTab("  ");

            // Initialise the configuration item if it's null - this is in case we've persisted
            // a version of the project without a configuration item it will create it here.
            //
            m_project.buildInitialConfiguration();

            // Setup the sprite font
            //
            setSpriteFont();

            // Load all the files - if we have nothing in this project then create a BufferView
            // and a FileBuffer.
            //
            if (m_project.getFileBuffers().Count == 0)
            {
                addNewFileBuffer();
            }
            else
            {
                m_project.loadFiles(m_smartHelpWorker);
            }

            // Now do some jiggery pokery to make sure positioning is correct and that
            // any cursors or highlights are within bounds.
            //
            foreach (BufferView bv in m_project.getBufferViews())
            {
                // check boundaries for cursor and highlighting
                //
                bv.verifyBoundaries();

                // Set any defaults that we haven't persisted - this is a version upgrade
                // catch all method.
                //
                bv.setDefaults();
            }

            // Get the BufferView id we've selected and set the BufferView
            //
            //m_activeBufferView = m_project.getSelectedBufferView();

            // Ensure that we are in the correct position to view this buffer so there's no initial movement
            //
            m_eye = m_project.getEyePosition();
            m_target = m_project.getTargetPosition();
            m_zoomLevel = m_eye.Z;

            // Set the active buffer view
            //
            setActiveBuffer();

            // Set-up the single FileSystemView we have
            //
            if (m_project.getOpenDirectory() == "")
            {
                m_project.setOpenDirectory(@"C:\");  // set Default
            }

            m_fileSystemView = new FileSystemView(m_project.getOpenDirectory(), new Vector3(-800.0f, 0f, 0f), m_project);

            // Tree builder and model builder
            //
            generateTreeModel();
        }

        /// <summary>
        /// Generate a model from the Project
        /// </summary>
        private void generateTreeModel()
        {
            Logger.logMsg("XygloXNA::generateTreeModel() - starting");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            // Firstly get a root directory for the FileBuffer tree
            //
            string fileRoot = m_project.getFileBufferRoot();

            TreeBuilderGraph rG = m_treeBuilder.buildTreeFromFiles(fileRoot, m_project.getNonNullFileBuffers());

            // Build a model and store it if we don't have one
            //
            if (m_modelBuilder == null)
            {
                m_modelBuilder = new ModelBuilder();
            }

            // Rebuild it in a given position
            //
            m_modelBuilder.build(rG, m_projectPosition);

            sw.Stop();
            Logger.logMsg("XygloXNA::generateTreeModel() - completed in " + sw.ElapsedMilliseconds + " ms");
        }


        /// <summary>
        /// A event handler for FileSystemWatcher
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Logger.logMsg("XygloXNA::OnFileChanged() - File: " + e.FullPath + " " + e.ChangeType);

            foreach (FileBuffer fb in m_project.getFileBuffers())
            {
                if (fb.getFilepath() == e.FullPath)
                {
                    fb.forceRefetchFile(m_project.getSyntaxManager());
                }
            }
        }

        /// <summary>
        /// Attempt to set the display mode to the desired resolution.  Itterates through the display
        /// capabilities of the default graphics adapter to determine if the graphics adapter supports the
        /// requested resolution.  If so, the resolution is set and the function returns true.  If not,
        /// no change is made and the function returns false.
        /// </summary>
        /// <param name="iWidth">Desired screen width.</param>
        /// <param name="iHeight">Desired screen height.</param>
        /// <param name="bFullScreen">True if you wish to go to Full Screen, false for Windowed Mode.</param>
        private bool initGraphicsMode(int iWidth, int iHeight, bool bFullScreen)
        {
            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            if (bFullScreen == false)
            {
                if ((iWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                    && (iHeight <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height))
                {
                    m_graphics.PreferredBackBufferWidth = iWidth;
                    m_graphics.PreferredBackBufferHeight = iHeight;
                    m_graphics.IsFullScreen = bFullScreen;
                    m_graphics.ApplyChanges();

                    // Reload the bloom component
                    //
                    if (m_bloom != null)
                    {
                        Components.Remove(m_bloom);
                        //m_bloom.Dispose();
                        m_bloom = new BloomComponent(this);
                        Components.Add(m_bloom);
                    }

                    Logger.logMsg("XygloXNA::initGraphicsMode() - width = " + iWidth + ", height = " + iHeight + ", fullscreen = " + bFullScreen.ToString());
                    return true;
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set.  To do this, we will
                // iterate thorugh the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check the width and height of each mode against the passed values
                    if ((dm.Width == iWidth) && (dm.Height == iHeight))
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        m_graphics.PreferredBackBufferWidth = iWidth;
                        m_graphics.PreferredBackBufferHeight = iHeight;
                        m_graphics.IsFullScreen = bFullScreen;
                        m_graphics.ApplyChanges();

                        // Reload the bloom component
                        //
                        if (m_bloom != null)
                        {
                            Components.Remove(m_bloom);
                            //m_bloom.Dispose();
                            m_bloom = new BloomComponent(this);
                            Components.Add(m_bloom);
                        }

                        Logger.logMsg("XygloXNA::initGraphicsMode() - width = " + iWidth + ", height = " + iHeight + ", fullscreen = " + bFullScreen.ToString());
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Set the current main display SpriteFont to something in keeping with the resolution and reset some important variables.
        /// </summary>
        protected void setSpriteFont()
        {
            // Font loading - set our text size a bit fluffily at the moment
            //
            if (m_graphics.GraphicsDevice.Viewport.Width < 960)
            {
                m_project.getFontManager().setFontState(FontManager.FontType.Small);
                Logger.logMsg("XygloXNA:setSpriteFont() - using Small Window font");
            }
            else if (m_graphics.GraphicsDevice.Viewport.Width < 1024)
            {
                m_project.getFontManager().setFontState(FontManager.FontType.Medium);
                Logger.logMsg("XygloXNA:setSpriteFont() - using Window font");
            }
            else
            {
                Logger.logMsg("XygloXNA:setSpriteFont() - using Full font");
                m_project.getFontManager().setFontState(FontManager.FontType.Large);
            }

            // to handle tabs for the moment convert them to single spaces
            //
            Logger.logMsg("XygloXNA:setSpriteFont() - you must get these three variables correct for each position to avoid nasty looking fonts:");
            Logger.logMsg("XygloXNA:setSpriteFont() - zoom level = " + m_zoomLevel);
            Logger.logMsg("XygloXNA:setSpriteFont() - recalculating relative positions");

            // Now recalculate positions
            //
            foreach (BufferView bv in m_project.getBufferViews())
            {
                bv.calculateMyRelativePosition();
            }
        }

        /// <summary>
        /// Set up all of our one shot translations
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            initializeWorld();
        }

        /// <summary>
        /// Initialise our world
        /// </summary>
        public void initializeWorld()
        {
            // Construct our view and projection matrices.  See here for alternatives:
            // 
            // http://www.toymaker.info/Games/XNA/html/xna_camera.html
            // 
            m_viewMatrix = Matrix.CreateLookAt(m_eye, m_target, Vector3.Up);
            m_projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * Matrix.CreatePerspectiveFieldOfView(m_fov, GraphicsDevice.Viewport.AspectRatio, 0.1f, 10000f);

            // Generate frustrum
            //
            if (m_frustrum == null)
            {
                m_frustrum = new BoundingFrustum(m_viewMatrix * m_projection);
            }
            else
            {
                // You can also update frustrum matrix like this
                //
                m_frustrum.Matrix = m_viewMatrix * m_projection;
            }

            m_basicEffect.View = m_viewMatrix;
            m_basicEffect.Projection = m_projection;
            m_basicEffect.World = Matrix.CreateScale(1, -1, 1);

            m_lineEffect.View = m_viewMatrix;
            m_lineEffect.Projection = m_projection;
            m_lineEffect.World = Matrix.CreateScale(1, -1, 1);

            // Create a new SpriteBatch, which can be used to draw textures.
            //
            m_spriteBatch = new SpriteBatch(m_graphics.GraphicsDevice);

            // Panner spritebatch
            //
            m_pannerSpriteBatch = new SpriteBatch(m_graphics.GraphicsDevice);
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Logger.logMsg("XygloXNA::LoadContent() - loading resources");

            // Initialise font Manager
            //
            m_fontManager.initialise(Content, "Bitstream Vera Sans Mono", GraphicsDevice.Viewport.AspectRatio, "Nuclex");
            if (m_project != null)
            {
                m_project.setFontManager(m_fontManager);
                loadFriendlierContent();
            }

            // We have to initialise this as follows to work around CA2000 warning
            //
            m_basicEffect = new BasicEffect(m_graphics.GraphicsDevice);
            m_basicEffect.TextureEnabled = true;
            m_basicEffect.VertexColorEnabled = true;
            m_basicEffect.World = Matrix.Identity;
            m_basicEffect.DiffuseColor = Vector3.One;

            // Create and initialize our effect
            //
            m_lineEffect = new BasicEffect(m_graphics.GraphicsDevice);
            m_lineEffect.VertexColorEnabled = true;
            m_lineEffect.TextureEnabled = false;
            m_lineEffect.DiffuseColor = Vector3.One;
            m_lineEffect.World = Matrix.Identity;

            // Create the overlay SpriteBatch
            //
            m_overlaySpriteBatch = new SpriteBatch(m_graphics.GraphicsDevice);

            // Create a flat texture for drawing rectangles etc
            //
            Color[] foregroundColors = new Color[1];
            foregroundColors[0] = Color.White;
            m_flatTexture = new Texture2D(m_graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            m_flatTexture.SetData(foregroundColors);

            // Set up the text scroller width
            //
            if (m_project != null)
            {
                setTextScrollerWidth(Convert.ToInt16(m_project.getFontManager().getCharWidth(FontManager.FontType.Overlay) * 32));
            }

            // Hook up the drag and drop
            //
            System.Windows.Forms.Form gameForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Window.Handle);
            gameForm.AllowDrop = true;
            gameForm.DragEnter += new System.Windows.Forms.DragEventHandler(friendlierDragEnter);
            gameForm.DragDrop += new System.Windows.Forms.DragEventHandler(friendlierDragDrop);

            // Store the last window size
            //
            m_lastWindowSize.X = Window.ClientBounds.Width;
            m_lastWindowSize.Y = Window.ClientBounds.Height;

            // Initialise the DrawingHelper with this bounding box and some other stuff
            //
            if (m_project != null)
            {
                m_drawingHelper = new DrawingHelper(m_project, m_flatTexture, m_graphics.GraphicsDevice.Viewport.Width, m_graphics.GraphicsDevice.Viewport.Height);
            }
        }

        /// <summary>
        /// Specific FriendlierContent to load
        /// </summary>
        protected void loadFriendlierContent()
        {
            m_splashScreen = Content.Load<Texture2D>("splash");

            // Start up the worker thread for the performance counters
            //
            m_counterWorker = new PerformanceWorker();
            m_counterWorkerThread = new Thread(m_counterWorker.startWorking);
            m_counterWorkerThread.Start();

            m_smartHelpWorker = new SmartHelpWorker();
            m_smartHelpWorkerThread = new Thread(m_smartHelpWorker.startWorking);
            m_smartHelpWorkerThread.Start();

            // Loop until worker thread activates.
            //
            while (!m_counterWorkerThread.IsAlive && !m_smartHelpWorkerThread.IsAlive) ;
            Thread.Sleep(1);

            // Start up the worker thread for Kinect integration
            //
            m_kinectWorker = new KinectWorker();
            m_kinectWorkerThread = new Thread(m_kinectWorker.startWorking);
            m_kinectWorkerThread.Start();

            // Loop until worker thread activates.
            //
            while (!m_kinectWorkerThread.IsAlive) ;
            Thread.Sleep(1);

            // Initialise the project - do this only once and after the font maan
            //
            if (m_project != null)
            {
                initialiseProject();
            }

            // Make mouse invisible
            //
            IsMouseVisible = true;

            // Ensure that the maximise box is shown and hook up the callback
            //
            System.Windows.Forms.Form f = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(this.Window.Handle);
            f.MaximizeBox = true;
            f.Resize += Window_ResizeEvent;

            // Allow user resizing if we want this
            //
            if (m_isResizable)
            {
                this.Window.AllowUserResizing = true;
                this.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
            }

            this.Window.Title = "Friendlier v" + VersionInformation.getProductVersion();
        }

        /// <summary>
        /// Windows resize event is captured here so that we can go to full screen mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Window_ResizeEvent(object sender, System.EventArgs e)
        {
            System.Windows.Forms.Form f = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(this.Window.Handle);

            if (f.WindowState == System.Windows.Forms.FormWindowState.Maximized)
            {
                fullScreenMode();
            }
        }

        /// <summary>
        /// Client is changing size event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            // Make changes to handle the new window size.            
            Logger.logMsg("XygloXNA::Window_ClientSizeChanged() - got client resized event");

            // Disable the callback to this method for the moment
            this.Window.ClientSizeChanged -= new EventHandler<EventArgs>(Window_ClientSizeChanged);

            /*
            float changeWidth = Window.ClientBounds.Width - m_lastWindowSize.X;
            float changeHeight = Window.ClientBounds.Height - m_lastWindowSize.Y;
            
            if (changeWidth > changeHeight) // enforce aspect ratio on height
            {
                changeHeight = changeWidth / m_project.getFontManager().getAspectRatio();
                m_graphics.PreferredBackBufferHeight = (int)changeHeight;
            }
            else
            {
                changeWidth = changeHeight * m_project.getFontManager().getAspectRatio();
                m_graphics.PreferredBackBufferWidth = (int)changeWidth;
            }
            m_graphics.ApplyChanges();
*/

            // Calculate new window size and resize all BufferViews accordingly
            //
            if (Window.ClientBounds.Width != m_lastWindowSize.X)
            {
                m_graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                m_graphics.PreferredBackBufferHeight = (int)(Window.ClientBounds.Width / m_project.getFontManager().getAspectRatio());
            }
            else if (Window.ClientBounds.Height != m_lastWindowSize.Y)
            {
                m_graphics.PreferredBackBufferWidth = (int)(Window.ClientBounds.Height * m_project.getFontManager().getAspectRatio());
                m_graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            }

            m_graphics.ApplyChanges();

            // Set up the Sprite font according to new size
            //
            setSpriteFont();

            // Save these values
            //
            m_lastWindowSize.X = Window.ClientBounds.Width;
            m_lastWindowSize.Y = Window.ClientBounds.Height;

            // Store it in the project too
            //
            m_project.setWindowSize(m_lastWindowSize.X, m_lastWindowSize.Y);

            // Reenable the callback
            //
            this.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
        }


        /// <summary>
        /// Creates a new text scroller of given width
        /// </summary>
        protected void setTextScrollerWidth(int width)
        {
            // Dispose
            //
            //if (m_textScroller != null)
            //{
            //m_textScroller.Dispose();
            //}

            // Set up the text scrolling texture
            //
            m_textScroller = new RenderTarget2D(m_graphics.GraphicsDevice, width, Convert.ToInt16(m_project.getFontManager().getLineSpacing(FontManager.FontType.Overlay)));
        }

        /// <summary>
        /// At a zoom level where we want to rotate and reset the active buffer
        /// </summary>
        /// <param name="direction"></param>
        protected void setActiveBuffer(BufferView.ViewCycleDirection direction)
        {
            m_project.getSelectedBufferView().rotateQuadrant(direction);
            setActiveBuffer();
        }

        /// <summary>
        /// Use another method to set the active BufferView
        /// </summary>
        /// <param name="item"></param>
        protected void setActiveBuffer(int item)
        {
            if (item >= 0 && item < m_project.getBufferViews().Count)
            {
                Logger.logMsg("XygloXNA::setActiveBuffer() - setting active BufferView " + item);
                setActiveBuffer(m_project.getBufferViews()[item]);
            }
        }

        /// <summary>
        /// This is the generic method for setting an active view - for the moment this
        /// sets an additional parameter.
        /// </summary>
        /// <param name="view"></param>
        protected void setActiveBuffer(XygloView view)
        {
            // All the maths is done in the Buffer View
            //
            Vector3 eyePos = view.getEyePosition(m_zoomLevel);
            flyToPosition(eyePos);
        }

        /// <summary>
        /// Set which BufferView is the active one with a cursor in it
        /// </summary>
        /// <param name="view"></param>
        protected void setActiveBuffer(BufferView item = null)
        {
            try
            {
                // Either set the BufferView 
                if (item != null)
                {
                    m_project.setSelectedBufferView(item);
                }
                else if (m_project.getBufferViews().Count == 0) // Or if we have none then create one
                {
                    BufferView bv = new BufferView(m_project.getFontManager());
                    using (FileBuffer fb = new FileBuffer())
                    {
                        m_project.addBufferView(bv);
                        m_project.addFileBuffer(fb);
                        bv.setFileBuffer(fb);
                    }
                }

                // Unset the view selection
                //
                m_project.setSelectedView(null);
            }
            catch (Exception e)
            {
                Logger.logMsg("Cannot locate BufferView item in list " + e.ToString());
                return;
            }

            Logger.logMsg("XygloXNA:setActiveBuffer() - active buffer view is " + m_project.getSelectedBufferViewId());

            // Set the font manager up with a zoom level
            //
            m_project.getFontManager().setScreenState(m_zoomLevel, m_project.isFullScreen());

            // Now recalculate positions
            //
            foreach (BufferView bv in m_project.getBufferViews())
            {
                bv.calculateMyRelativePosition();
            }

            // All the maths is done in the Buffer View
            //
            Vector3 eyePos = m_project.getSelectedBufferView().getEyePosition(m_zoomLevel);

            flyToPosition(eyePos);

            // Set title to include current filename
            // (this is not thread safe - we need to synchronise)
            //this.Window.Title = "Friendlier v" + VersionInformation.getProductVersion() + " - " + m_project.getSelectedBufferView().getFileBuffer().getShortFileName();
        }

        // Set up the file save mode
        //
        protected void selectSaveFile()
        {
            // Enter this mode and clear and existing message
            //
            m_state = State.Test("FileSaveAs");
            m_temporaryMessage = "";

            // Clear the filename
            //
            m_saveFileName = "";
        }

        /// <summary>
        /// Got to the FileOpen mode in the overall application state.  This will zoom out from the
        /// bufferview and grey it out and allow us to select a file
        /// </summary>
        protected void selectOpenFile()
        {
            // Enter this mode and clear and existing message
            //
            m_state = State.Test("FileOpen");
            m_temporaryMessage = "";
        }

        /// <summary>
        /// Switch to the Configuration mode
        /// </summary>
        protected void showConfigurationScreen()
        {
            m_state = State.Test("Configuration");
            m_temporaryMessage = "";
        }

        /// <summary>
        /// Close the active buffer view
        /// </summary>
        protected void closeActiveBuffer(GameTime gameTime)
        {
            if (m_project.getBufferViews().Count > 1)
            {
                int index = m_project.getBufferViews().IndexOf(m_project.getSelectedBufferView());
                m_project.removeBufferView(m_project.getSelectedBufferView());

                // Ensure that the index is not greater than number of bufferviews
                //
                if (index > m_project.getBufferViews().Count - 1)
                {
                    index = m_project.getBufferViews().Count - 1;
                }

                //m_project.setSelectedBufferViewId(index);

                setActiveBuffer(index);

                setTemporaryMessage("Removed BufferView.", 2, gameTime);
            }
            else
            {
                setTemporaryMessage("Can't remove the last BufferView.", 2, gameTime);
            }
        }

        /// <summary>
        /// Traverse a directory and allow opening/saving at that point according to state
        /// </summary>
        protected void traverseDirectory(GameTime gameTime, bool readOnly = false, bool tailFile = false)
        {
            //string fileToOpen = m_fileSystemView.getHighlightedFile();
            if (m_fileSystemView.atDriveLevel())
            {
                // First extract the drive letter and set the path
                //
                m_fileSystemView.setHighlightedDrive();
            }

            // If we're not at the root directory
            //
            if (m_fileSystemView.getHighlightIndex() > 0)
            {
                string subDirectory = "";

                // Set the directory to the sub directory and reset the highlighter
                //
                try
                {
                    if (m_fileSystemView.getHighlightIndex() - 1 < m_fileSystemView.getDirectoryInfo().GetDirectories().Length)
                    {
                        // Set error directory in case of failure to test access
                        //
                        DirectoryInfo directoryToAccess = m_fileSystemView.getDirectoryInfo().GetDirectories()[m_fileSystemView.getHighlightIndex() - 1];
                        subDirectory = directoryToAccess.Name;

                        // Test access
                        //
                        DirectoryInfo[] testAccess = directoryToAccess.GetDirectories();


                        FileInfo[] testFiles = directoryToAccess.GetFiles();

                        m_fileSystemView.setDirectory(directoryToAccess.FullName);
                        m_fileSystemView.setHighlightIndex(0);
                        m_lastHighlightIndex = m_fileSystemView.getHighlightIndex();
                    }
                    else
                    {
                        int fileIndex = m_fileSystemView.getHighlightIndex() - 1 - m_fileSystemView.getDirectoryInfo().GetDirectories().Length;
                        FileInfo fileInfo = m_fileSystemView.getDirectoryInfo().GetFiles()[fileIndex];

                        Logger.logMsg("Friendler::traverseDirectory() - selected a file " + fileInfo.Name);

                        // Set these values and the status
                        //
                        m_fileIsReadOnly = readOnly;
                        m_fileIsTailing = tailFile;
                        m_selectedFile = fileInfo.FullName;

                        if (m_state.equals("FileOpen"))
                        {
                            // Now we need to choose a position for the new file we're opening
                            //
                            m_state = State.Test("PositionScreenOpen");
                        }
                        else if (m_state.equals("FileSaveAs"))
                        {
                            // Set the FileBuffer path
                            //
                            m_project.getSelectedBufferView().getFileBuffer().setFilepath(m_selectedFile);

                            if (checkFileSave())
                            {
                                if (m_filesToWrite != null)
                                {
                                    // Check if we need to remove this FileBuffer from the todo list - it's not important if we can't
                                    // remove it here but we should try to anyway.
                                    //
                                    m_filesToWrite.RemoveAt(0);
                                    Logger.logMsg("XygloXNA::traverseDirectory() - total files left to write is now " + m_filesToWrite.Count);

                                    // If we have finished saving all of our files then we can exit (although we check once again)
                                    //
                                    if (m_filesToWrite.Count == 0)
                                    {
                                        if (m_saveAsExit == true)
                                        {
                                            checkExit(gameTime);
                                        }
                                        else
                                        {
                                            setActiveBuffer();
                                        }
                                    }
                                }
                                else
                                {
                                    m_state = State.Test("TextEditing");
                                    setActiveBuffer();
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    setTemporaryMessage("XygloXNA::traverseDirectory() - Cannot access \"" + subDirectory + "\"", 2, gameTime);
                }
            }
        }

        /// <summary>
        /// Checks to see if we are licenced before saving
        /// </summary>
        /// <returns></returns>
        protected bool checkFileSave()
        {
            if (m_project.getLicenced())
            {
                m_project.getSelectedBufferView().getFileBuffer().save();
                return true;
            }

            setTemporaryMessage("Can't save due to licence issue.", 10, m_gameTime);

            return false;
        }


        /// <summary>
        /// Completing a File->Save operation
        /// </summary>
        /// <param name="gameTime"></param>
        protected void completeSaveFile(GameTime gameTime)
        {
            try
            {
                checkFileSave();

                if (m_filesToWrite != null && m_filesToWrite.Count > 0)
                {
                    m_filesToWrite.RemoveAt(0);
                    Logger.logMsg("XygloXNA::completeSaveFile() - files remaining to be written " + m_filesToWrite.Count);
                }

                Vector3 newPosition = m_eye;
                newPosition.Z = 500.0f;

                flyToPosition(newPosition);
                m_state = State.Test("TextEditing");

                setTemporaryMessage("Saved.", 2, gameTime);
            }
            catch (Exception)
            {
                setTemporaryMessage("Failed to save to " + m_project.getSelectedBufferView().getFileBuffer().getFilepath(), 2, gameTime);
            }
        }


        /// <summary>
        /// Exit but ensuring that buffers are saved
        /// </summary>
        protected void checkExit(GameTime gameTime, bool force = false)
        {
            Logger.logMsg("XygloXNA::checkExit() - checking exit with force = " + force.ToString());

            // Firstly check for any unsaved buffers and warn
            //
            bool unsaved = false;

            // Immediate exit for no project (not Friendlier)
            //
            if (m_project == null)
            {
                this.Exit();
                return;
            }

            // Use this somewhere probably
            //
            // _project.getLicenced() 

            // Only check BufferViews status if we're not forcing an exit
            //
            if (!force && m_saveAsExit == false && m_project.getConfigurationValue("CONFIRMQUIT").ToUpper() == "TRUE")
            {
                if (m_confirmState.notEquals("ConfirmQuit"))
                {
                    setTemporaryMessage("Confirm quit? Y/N", 0, gameTime);
                    m_confirmState.set("ConfirmQuit");
                }

                if (m_confirmQuit == false)
                {
                    return;
                }
            }

            if (!force)
            {
                foreach (FileBuffer fb in m_project.getFileBuffers())
                {
                    if (fb.isModified())
                    {
                        unsaved = true;
                        break;
                    }
                }
            }

            // Likewise only save if we want to
            //
            if (unsaved && !force)
            {
                if (m_confirmState.equals("FileSaveCancel"))
                {
                    setTemporaryMessage("", 1, gameTime);
                    m_confirmState.set("None");
                    return;
                }
                else
                {
                    setTemporaryMessage("Unsaved Buffers.  Save?  Y/N/C", 0, gameTime);
                    m_confirmState.set("FileSaveCancel");
                    m_saveAsExit = true;
                    //m_state = State.FileSaveAs;
                }
            }
            else
            {
                // If these are not null then we're completed
                if (m_kinectWorker != null)
                {
                    // Close the kinect thread
                    //
                    m_kinectWorker.requestStop();
                    m_kinectWorkerThread.Join();
                    m_kinectWorker = null;
                }

                if (m_counterWorker != null)
                {
                    // Clear the worker thread and exit
                    //
                    m_counterWorker.requestStop();
                    m_counterWorkerThread.Join();
                    m_counterWorker = null;
                }

                // Join the smart help worker 
                //
                if (m_smartHelpWorker != null)
                {
                    m_smartHelpWorker.requestStop();
                    m_smartHelpWorkerThread.Join();
                    m_smartHelpWorker = null;
                }

                // Modify Z if we're in the file selector height of 600.0f
                //
                if (m_eye.Z == 600.0f)
                {
                    m_eye.Z = 500.0f;
                }

                // Store the eye and target positions to the project before serialising it.
                //
                m_project.setEyePosition(m_eye);
                m_project.setTargetPosition(m_target);
                m_project.setOpenDirectory(m_fileSystemView.getPath());

                // Do some file management to ensure we have some backup copies
                //
                m_project.manageSerialisations();

                // Save our project including any updated file statuses
                //
                m_project.dataContractSerialise();

                this.Exit();
            }
        }

        /// <summary>
        /// Set a current zoom level
        /// </summary>
        /// <param name="zoomLevel"></param>
        protected void setZoomLevel(float zoomLevel)
        {
            m_zoomLevel = zoomLevel;

            if (m_zoomLevel < 500.0f)
            {
                m_zoomLevel = 500.0f;
            }

            Vector3 eyePos = m_eye;
            eyePos.Z = m_zoomLevel;
            flyToPosition(eyePos);

            // Don't always centre on a BufferView as we don't always want this
            //setActiveBuffer();
        }

        /// <summary>
        /// Process some meta commands as part of our update statement
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        protected bool processMetaCommand(GameTime gameTime, KeyAction keyAction)
        {
            List<Keys> keyList = new List<Keys>();
            //foreach(KeyAction keyAction in keyActionList)
            //{
                keyList.Add(keyAction.m_key);
            //}

            // Allow the game to exit
            //
            if (keyList.Contains(Keys.Escape))
            {
                // Check to see if we are building something
                //
                if (m_buildProcess != null)
                {
                    setTemporaryMessage("Cancel build? (Y/N)", 0, m_gameTime);
                    m_confirmState.set("CancelBuild");
                    return true;
                }

                if (m_confirmState.equals("ConfirmQuit"))
                {
                    m_confirmState.set("None");
                    setTemporaryMessage("Cancelled quit.", 1.0, gameTime);
                    m_state = State.Test("TextEditing");
                    return true;
                }

                // Depends where we are in the process here - check state
                //
                Vector3 newPosition = m_eye;

                switch (m_state.m_name)
                {
                    // These are FRIENDLIER states
                    //
                    case "TextEditing":
                        checkExit(gameTime);
                        break;

                    case "FileSaveAs":
                        setTemporaryMessage("Cancelled quit.", 0.5, gameTime);
                        m_confirmState.set("None");
                        m_state = State.Test("TextEditing");
                        m_saveAsExit = false;
                        m_filesToWrite = null;
                        break;

                    case "ManageProject":
                        newPosition = m_project.getSelectedBufferView().getLookPosition();
                        newPosition.Z = 500.0f;
                        m_state = State.Test("TextEditing");
                        m_editConfigurationItem = false;
                        break;

                    case "DiffPicker":
                        m_state = State.Test("TextEditing");

                        // Before we clear the differ we want to translate the current viewed differ
                        // position back to the Bufferviews we originally generated them from.
                        //
                        BufferView bv1 = m_differ.getSourceBufferViewLhs();
                        BufferView bv2 = m_differ.getSourceBufferViewRhs();


                        // Ensure that these are valid
                        //
                        if (bv1 != null && bv2 != null)
                        {

                            bv1.setBufferShowStartY(m_differ.diffPositionLhsToOriginalPosition(m_diffPosition));
                            bv2.setBufferShowStartY(m_differ.diffPositionRhsToOriginalPosition(m_diffPosition));

                            ScreenPosition sp1 = new ScreenPosition();
                            ScreenPosition sp2 = new ScreenPosition();

                            sp1.X = 0;
                            sp1.Y = m_differ.diffPositionLhsToOriginalPosition(m_diffPosition);

                            sp2.X = 0;
                            sp2.Y = m_differ.diffPositionRhsToOriginalPosition(m_diffPosition);

                            // Limit Y in case it overruns file length
                            //
                            if (sp1.Y >= bv1.getFileBuffer().getLineCount())
                            {
                                sp1.Y = bv1.getFileBuffer().getLineCount() - 1;
                            }
                            if (sp2.Y >= bv2.getFileBuffer().getLineCount())
                            {
                                sp2.Y = bv2.getFileBuffer().getLineCount() - 1;
                            }

                            bv1.setCursorPosition(sp1);
                            bv2.setCursorPosition(sp2);
                        }

                        // Clear the differ object if it exists
                        //
                        if (m_differ != null)
                        {
                            m_differ.clear();
                        }
                        break;

                    // Two stage exit from the Configuration edit
                    //
                    case "Configuration":
                        if (m_editConfigurationItem == true)
                        {
                            m_editConfigurationItem = false;
                        }
                        else
                        {
                            m_state = State.Test("TextEditing");
                            m_editConfigurationItem = false;
                        }
                        break;

                    case "FileOpen":
                    case "Information":
                    case "PositionScreenOpen":
                    case "PositionScreenNew":
                    case "PositionScreenCopy":
                    case "SplashScreen":
                    case "Help":
                        m_state = State.Test("TextEditing");
                        m_editConfigurationItem = false;
                        break;

                    // These are PAULO states
                    // 
                    case "Menu":
                        checkExit(gameTime);
                        break;

                    default:
                        // Ummmm??
                        //
                        break;
                }

                // Cancel any temporary message
                //
                //m_temporaryMessageEndTime = gameTime.TotalGameTime.TotalSeconds;

                // Fly back to correct position
                //
                flyToPosition(newPosition);
            }

            // If we're viewing some information then only escape can get us out
            // of this mode.  Note that we also have to mind any animations so we
            // also want to ensure that m_changingEyePosition is not true.
            //
            if ((m_state.equals("Information") || m_state.equals("Help") /* || m_state == State.ManageProject */ ) && m_changingEyePosition == false)
            {
                if (keyList.Contains(Keys.PageDown))
                {
                    if (m_textScreenPositionY + m_project.getSelectedBufferView().getBufferShowLength() < m_textScreenLength)
                    {
                        m_textScreenPositionY += m_project.getSelectedBufferView().getBufferShowLength();
                    }
                }
                else if (keyList.Contains(Keys.PageUp))
                {
                    if (m_textScreenPositionY > 0)
                    {
                        m_textScreenPositionY = m_textScreenPositionY - Math.Min(m_project.getSelectedBufferView().getBufferShowLength(), m_textScreenPositionY);
                    }
                }
                return true;
            }

            // This helps us count through our lists of file to save if we're trying to exit
            //
            if (m_filesToWrite != null && m_filesToWrite.Count > 0)
            {
                m_project.setSelectedBufferView(m_filesToWrite[0]);
                m_eye = m_project.getSelectedBufferView().getEyePosition();
                selectSaveFile();
            }

            // For PositionScreen state we want not handle events here other than direction keys - this section
            // decides where to place a new, opened or copied BufferView.
            //
            if (m_state.equals("PositionScreenOpen") || m_state.equals("PositionScreenNew") || m_state.equals("PositionScreenCopy"))
            {
                bool gotSelection = false;

                if (keyList.Contains(Keys.Left))
                {
                    Logger.logMsg("XygloXNA::processMetaCommands() - position screen left");
                    m_newPosition = BufferView.ViewPosition.Left;
                    gotSelection = true;
                }
                else if (keyList.Contains(Keys.Right))
                {
                    m_newPosition = BufferView.ViewPosition.Right;
                    gotSelection = true;
                    Logger.logMsg("XygloXNA::processMetaCommands() - position screen right");
                }
                else if (keyList.Contains(Keys.Up))
                {
                    m_newPosition = BufferView.ViewPosition.Above;
                    gotSelection = true;
                    Logger.logMsg("XygloXNA::processMetaCommands() - position screen up");
                }
                else if (keyList.Contains(Keys.Down))
                {
                    m_newPosition = BufferView.ViewPosition.Below;
                    gotSelection = true;
                    Logger.logMsg("XygloXNA::processMetaCommands() - position screen down");
                }

                // If we have discovered a position for our pending new window
                //
                if (gotSelection)
                {
                    if (m_state.equals("PositionScreenOpen"))
                    {
                        // Open the file 
                        //
                        BufferView newBV = addNewFileBuffer(m_selectedFile, m_fileIsReadOnly, m_fileIsTailing);
                        setActiveBuffer(newBV);
                        m_state = State.Test("TextEditing");
                    }
                    else if (m_state.equals("PositionScreenNew"))
                    {
                        // Use the convenience function
                        //
                        BufferView newBV = addNewFileBuffer();
                        setActiveBuffer(newBV);
                        m_state = State.Test("TextEditing");
                    }
                    else if (m_state.equals("PositionScreenCopy"))
                    {
                        // Use the copy constructor
                        //
                        BufferView newBV = new BufferView(m_project.getFontManager(), m_project.getSelectedBufferView(), m_newPosition);
                        m_project.addBufferView(newBV);
                        setActiveBuffer(newBV);
                        m_state = State.Test("TextEditing");
                    }
                }


                return true;
            }

            return false;
        }

        /// <summary>
        /// Process action keys for commands
        /// </summary>
        /// <param name="gameTime"></param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected void processActionKey(GameTime gameTime, KeyAction keyAction)
        {
            List<Keys> keyList = new List<Keys>();
            keyList.Add(keyAction.m_key);

            // Main key handling statement
            //
            if (keyList.Contains(Keys.Up))
            {
                if (m_state.equals("FileSaveAs") || m_state.equals("FileOpen"))
                {
                    if (m_fileSystemView.getHighlightIndex() > 0)
                    {
                        m_fileSystemView.incrementHighlightIndex(-1);
                    }
                }
                else if (m_state.equals("ManageProject") || (m_state.equals("Configuration") && m_editConfigurationItem == false)) // Configuration changes
                {
                    if (m_configPosition > 0)
                    {
                        m_configPosition--;
                    }
                }
                else if (m_state.equals("DiffPicker"))
                {
                    if (m_diffPosition > 0)
                    {
                        m_diffPosition--;
                    }
                }
                else
                {
                    if (m_altDown && m_shiftDown) // Do zoom
                    {
                        setZoomLevel(m_zoomLevel - m_zoomStep);
                    }
                    else if (m_altDown)
                    {
                        // Attempt to move right if there's a BufferView there
                        //
                        detectMove(BufferView.ViewPosition.Above, gameTime);
                    }
                    else
                    {
                        m_project.getSelectedBufferView().moveCursorUp(m_project, false, m_shiftDown);

                        if (m_shiftDown)
                        {
                            m_project.getSelectedBufferView().extendHighlight();  // Extend 
                        }
                        else
                        {
                            m_project.getSelectedBufferView().noHighlight(); // Disable
                        }
                    }
                }
            }
            //else if (keyActionList.Find(item => item.m_key == Keys.Down && item.m_modifier == KeyboardModifier.None).ToString() != "")
            else if (keyList.Contains(Keys.Down))
            {
                if (m_state.equals("FileSaveAs") || m_state.equals("FileOpen"))
                {
                    if (m_fileSystemView.atDriveLevel())
                    {
                        // Drives are highlighted slightly differently to directories as the zero index is 
                        // counted for drives (1 for directories) hence the adjustment in the RH term
                        //
                        if (m_fileSystemView.getHighlightIndex() < m_fileSystemView.countActiveDrives() - 1)
                        {
                            m_fileSystemView.incrementHighlightIndex(1);
                        }
                    }
                    else if (m_fileSystemView.getHighlightIndex() < m_fileSystemView.getDirectoryLength())
                    {
                        m_fileSystemView.incrementHighlightIndex(1);
                    }
                }
                else if (m_state.equals("Configuration") && m_editConfigurationItem == false) // Configuration changes
                {
                    if (m_configPosition < m_project.getConfigurationListLength() - 1)
                    {
                        m_configPosition++;
                    }
                }
                else if (m_state.equals("ManageProject"))
                {
                    if (m_configPosition < m_modelBuilder.getLeafNodesPlaces() - 1)
                    {
                        m_configPosition++;
                    }
                }
                else if (m_state.equals("DiffPicker"))
                {
                    if (m_differ != null && m_diffPosition < m_differ.getMaxDiffLength())
                    {
                        m_diffPosition++;
                    }
                }
                else
                {
                    if (m_altDown && m_shiftDown) // Do zoom
                    {
                        m_zoomLevel += m_zoomStep;
                        setActiveBuffer();
                    }
                    else if (m_altDown)
                    {
                        // Attempt to move right if there's a BufferView there
                        //
                        detectMove(BufferView.ViewPosition.Below, gameTime);
                    }
                    else
                    {
                        m_project.getSelectedBufferView().moveCursorDown(false, m_shiftDown);

                        if (m_shiftDown)
                        {
                            m_project.getSelectedBufferView().extendHighlight();
                        }
                        else
                        {
                            m_project.getSelectedBufferView().noHighlight(); // Disable
                        }
                    }
                }
            }
            else if (keyList.Contains(Keys.Left))
            {
                if (m_state.equals("FileSaveAs") || m_state.equals("FileOpen"))
                {
                    string parDirectory = "";

                    // Set the directory to the sub directory and reset the highlighter
                    //
                    try
                    {
                        DirectoryInfo parentTest = m_fileSystemView.getParent();

                        if (parentTest == null)
                        {
                            Logger.logMsg("Check devices");
                            m_fileSystemView.setDirectory(null);
                        }
                        else
                        {
                            parDirectory = m_fileSystemView.getParent().Name;
                            DirectoryInfo[] testAccess = m_fileSystemView.getParent().GetDirectories();
                            FileInfo[] testFiles = m_fileSystemView.getParent().GetFiles();

                            m_fileSystemView.setDirectory(m_fileSystemView.getParent().FullName);
                            m_fileSystemView.setHighlightIndex(m_lastHighlightIndex);
                        }
                    }
                    catch (Exception /*e*/)
                    {
                        setTemporaryMessage("Cannot access " + parDirectory.ToString(), 2, gameTime);
                    }
                }
                else
                {
                    if (m_ctrlDown)
                    {
                        m_project.getSelectedBufferView().wordJumpCursorLeft();
                    }
                    else if (m_altDown)
                    {
                        // Attempt to move right if there's a BufferView there
                        //
                        detectMove(BufferView.ViewPosition.Left, gameTime);
                    }
                    else
                    {
                        m_project.getSelectedBufferView().moveCursorLeft(m_project, m_shiftDown);
                    }

                    if (m_shiftDown)
                    {
                        m_project.getSelectedBufferView().extendHighlight();  // Extend
                    }
                    else
                    {
                        m_project.getSelectedBufferView().noHighlight(); // Disable
                    }
                }
            }
            else if (keyList.Contains(Keys.Right))
            {
                if (m_state.equals("FileSaveAs") || m_state.equals("FileOpen"))
                {
                    traverseDirectory(gameTime);
                }
                else
                {
                    if (m_ctrlDown)
                    {
                        m_project.getSelectedBufferView().wordJumpCursorRight();
                    }
                    else if (m_altDown)
                    {
                        // Attempt to move right if there's a BufferView there
                        //
                        detectMove(BufferView.ViewPosition.Right, gameTime);
                    }
                    else
                    {
                        m_project.getSelectedBufferView().moveCursorRight(m_project, m_shiftDown);
                    }

                    if (m_shiftDown)
                    {
                        m_project.getSelectedBufferView().extendHighlight(); // Extend
                    }
                    else
                    {
                        m_project.getSelectedBufferView().noHighlight(); // Disable
                    }
                }
            }
            else if (keyList.Contains(Keys.End))
            {
                ScreenPosition fp = m_project.getSelectedBufferView().getCursorPosition();
                ScreenPosition originalFp = fp;

                // Set X and allow for tabs
                //
                if (fp.Y < m_project.getSelectedBufferView().getFileBuffer().getLineCount())
                {
                    fp.X = m_project.getSelectedBufferView().getFileBuffer().getLine(fp.Y).Replace("\t", m_project.getTab()).Length;
                }
                m_project.getSelectedBufferView().setCursorPosition(fp);

                // Change the X offset if the row is longer than the visible width
                //
                if (fp.X > m_project.getSelectedBufferView().getBufferShowWidth())
                {
                    int bufferX = fp.X - m_project.getSelectedBufferView().getBufferShowWidth();
                    m_project.getSelectedBufferView().setBufferShowStartX(bufferX);
                }

                if (m_shiftDown)
                {
                    m_project.getSelectedBufferView().extendHighlight(); // Extend
                }
                else
                {
                    m_project.getSelectedBufferView().noHighlight(); // Disable
                }

            }
            else if (keyList.Contains(Keys.Home))
            {
                // Reset the cursor to zero
                //
                ScreenPosition fp = m_project.getSelectedBufferView().getFirstNonSpace(m_project);

                m_project.getSelectedBufferView().setCursorPosition(fp);

                // Reset any X offset to zero
                //
                m_project.getSelectedBufferView().setBufferShowStartX(0);

                if (m_shiftDown)
                {
                    m_project.getSelectedBufferView().extendHighlight(); // Extend
                }
                else
                {
                    m_project.getSelectedBufferView().noHighlight(); // Disable
                }
            }
            else if (keyList.Contains(Keys.F9)) // Spin anticlockwise though BVs
            {
                m_zoomLevel = 1000.0f;
                setActiveBuffer(BufferView.ViewCycleDirection.Anticlockwise);
            }
            else if (keyList.Contains(Keys.F10)) // Spin clockwise through BVs
            {
                m_zoomLevel = 1000.0f;
                setActiveBuffer(BufferView.ViewCycleDirection.Clockwise);
            }
            else if (keyList.Contains(Keys.F3))
            {
                doSearch(gameTime);
            }
            else if (keyList.Contains(Keys.F4))
            {
                m_project.setViewMode(Project.ViewMode.Fun);
                m_drawingHelper.startBanner(gameTime, "Friendlier\nv1.0", 5);
            }
            else if (keyList.Contains(Keys.F6))
            {
                doBuildCommand(gameTime);
            }
            else if (keyList.Contains(Keys.F7))
            {
                string command = m_project.getConfigurationValue("ALTERNATEBUILDCOMMAND");
                doBuildCommand(gameTime, command);
            }
            else if (keyList.Contains(Keys.F11)) // Toggle full screen
            {
                if (m_project.isFullScreen())
                {
                    windowedMode();
                }
                else
                {
                    fullScreenMode();
                }
                setSpriteFont();
            }
            else if (keyList.Contains(Keys.F1))  // Cycle down through BufferViews
            {
                int newValue = m_project.getSelectedBufferViewId() - 1;
                if (newValue < 0)
                {
                    newValue += m_project.getBufferViews().Count;
                }

                m_project.setSelectedBufferViewId(newValue);
                setActiveBuffer();
            }
            else if (keyList.Contains(Keys.F2)) // Cycle up through BufferViews
            {
                int newValue = (m_project.getSelectedBufferViewId() + 1) % m_project.getBufferViews().Count;
                m_project.setSelectedBufferViewId(newValue);
                setActiveBuffer();
            }
            else if (keyList.Contains(Keys.PageDown))
            {
                if (m_state.equals("TextEditing"))
                {
                    m_project.getSelectedBufferView().pageDown(m_project);

                    if (m_shiftDown)
                    {
                        m_project.getSelectedBufferView().extendHighlight(); // Extend
                    }
                    else
                    {
                        m_project.getSelectedBufferView().noHighlight(); // Disable
                    }
                }
                else if (m_state.equals("DiffPicker"))
                {
                    if (m_differ != null && m_diffPosition < m_differ.getMaxDiffLength())
                    {
                        m_diffPosition += m_project.getSelectedBufferView().getBufferShowLength();

                        if (m_diffPosition >= m_differ.getMaxDiffLength())
                        {
                            m_diffPosition = m_differ.getMaxDiffLength() - 1;
                        }
                    }
                }
            }
            else if (keyList.Contains(Keys.PageUp))
            {
                if (m_state.equals("TextEditing"))
                {
                    m_project.getSelectedBufferView().pageUp(m_project);

                    if (m_shiftDown)
                    {
                        m_project.getSelectedBufferView().extendHighlight(); // Extend
                    }
                    else
                    {
                        m_project.getSelectedBufferView().noHighlight(); // Disable
                    }
                }
                else if (m_state.equals("DiffPicker"))
                {
                    if (m_diffPosition > 0)
                    {
                        m_diffPosition -= m_project.getSelectedBufferView().getBufferShowLength();

                        if (m_diffPosition < 0)
                        {
                            m_diffPosition = 0;
                        }
                    }
                }
            }
            else if (keyList.Contains(Keys.Scroll))
            {
                if (m_project.getSelectedBufferView().isLocked())
                {
                    m_project.getSelectedBufferView().setLock(false, 0);
                }
                else
                {
                    m_project.getSelectedBufferView().setLock(true, m_project.getSelectedBufferView().getCursorPosition().Y);
                }
            }
            else if (keyList.Contains(Keys.Tab)) // Insert a tab space
            {
                m_project.getSelectedBufferView().insertText(m_project, "\t");
                updateSmartHelp();
            }
            else if (keyList.Contains(Keys.Insert))
            {
                if (m_state.equals("ManageProject"))
                {
                    if (m_configPosition >= 0 && m_configPosition < m_modelBuilder.getLeafNodesPlaces())
                    {
                        string fileToEdit = m_modelBuilder.getSelectedModelString(m_configPosition);

                        BufferView bv = m_project.getBufferView(fileToEdit);

                        if (bv != null)
                        {
                            setActiveBuffer(bv);
                        }
                        else // create and activate
                        {
                            try
                            {
                                FileBuffer fb = m_project.getFileBuffer(fileToEdit);
                                bv = new BufferView(m_project.getFontManager(), m_project.getBufferViews()[0], BufferView.ViewPosition.Left);
                                bv.setFileBuffer(fb);
                                int bvIndex = m_project.addBufferView(bv);
                                setActiveBuffer(bvIndex);

                                Vector3 rootPosition = m_project.getBufferViews()[0].getPosition();
                                Vector3 newPosition2 = bv.getPosition();

                                Logger.logMsg(rootPosition.ToString() + newPosition2.ToString());
                                //bv.setFileBufferIndex(
                                fb.loadFile(m_project.getSyntaxManager());

                                if (m_project.getConfigurationValue("SYNTAXHIGHLIGHT").ToUpper() == "TRUE")
                                {
                                    //m_project.getSyntaxManager().generateAllHighlighting(fb, true);
                                    m_smartHelpWorker.updateSyntaxHighlighting(m_project.getSyntaxManager(), fb);
                                }

                                // Break out of Manage mode and back to editing
                                //
                                Vector3 newPosition = m_project.getSelectedBufferView().getLookPosition();
                                newPosition.Z = 500.0f;
                                m_state = State.Test("TextEditing");
                                m_editConfigurationItem = false;
                            }
                            catch (Exception e)
                            {
                                setTemporaryMessage("Failed to load file " + e.Message, 2, gameTime);
                            }
                        }
                    }
                }

            }
            else if (keyList.Contains(Keys.Delete) || keyList.Contains(Keys.Back))
            {

                if (m_state.equals("FileSaveAs") && keyList.Contains(Keys.Back))
                {
                    // Delete charcters from the file name if we have one
                    //
                    if (m_saveFileName.Length > 0)
                    {
                        m_saveFileName = m_saveFileName.Substring(0, m_saveFileName.Length - 1);
                    }
                }
                else if (m_state.equals("FindText") && keyList.Contains(Keys.Back))
                {
                    string searchText = m_project.getSelectedBufferView().getSearchText();
                    // Delete charcters from the file name if we have one
                    //
                    if (searchText.Length > 0)
                    {
                        m_project.getSelectedBufferView().setSearchText(searchText.Substring(0, searchText.Length - 1));
                    }
                }
                else if (m_state.equals("GotoLine") && keyList.Contains(Keys.Back))
                {
                    if (m_gotoLine.Length > 0)
                    {
                        m_gotoLine = m_gotoLine.Substring(0, m_gotoLine.Length - 1);
                    }
                }
                else if (m_state.equals("Configuration") && m_editConfigurationItem && keyList.Contains(Keys.Back))
                {
                    if (m_editConfigurationItemValue.Length > 0)
                    {
                        m_editConfigurationItemValue = m_editConfigurationItemValue.Substring(0, m_editConfigurationItemValue.Length - 1);
                    }
                }
                else if (m_state.equals("ManageProject"))
                {
                    if (m_configPosition >= 0 && m_configPosition < m_modelBuilder.getLeafNodesPlaces())
                    {
                        string fileToRemove = m_modelBuilder.getSelectedModelString(m_configPosition);
                        if (m_project.removeFileBuffer(fileToRemove))
                        {
                            Logger.logMsg("XygloXNA::processActionKeys() - removed FileBuffer for " + fileToRemove);

                            // Update Active Buffer as necessary
                            //
                            setActiveBuffer();

                            // Rebuild the file model
                            //
                            generateTreeModel();

                            setTemporaryMessage("Removed " + fileToRemove + " from project", 5, m_gameTime);
                        }
                        else
                        {
                            Logger.logMsg("XygloXNA::processActionKeys() - failed to remove FileBuffer for " + fileToRemove);
                        }
                    }
                }
                else if (m_project.getSelectedBufferView().gotHighlight()) // If we have a valid highlighted selection then delete it (normal editing)
                {
                    // All the clever stuff with the cursor is done at the BufferView level and it also
                    // calls the command in the FileBuffer.
                    //
                    m_project.getSelectedBufferView().deleteCurrentSelection(m_project);
                    updateSmartHelp();
                }
                else // delete at cursor
                {
                    if (keyList.Contains(Keys.Delete))
                    {
                        m_project.getSelectedBufferView().deleteSingle(m_project);
                    }
                    else if (keyList.Contains(Keys.Back))
                    {
                        // Start with a file position from the screen position
                        //
                        FilePosition fp = m_project.getSelectedBufferView().screenToFilePosition(m_project);

                        // Get the character before the current one and backspace accordingly 

                        if (fp.X > 0)
                        {
                            string fetchLine = m_project.getSelectedBufferView().getCurrentLine();

                            // Decrement and set X
                            //
                            fp.X--;

                            // Now convert back to a screen position
                            fp.X = fetchLine.Substring(0, fp.X).Replace("\t", m_project.getTab()).Length;

                        }
                        else if (fp.Y > 0)
                        {
                            fp.Y -= 1;

                            // Don't forget to do tab conversions here too
                            //
                            fp.X = m_project.getSelectedBufferView().getFileBuffer().getLine(Convert.ToInt16(fp.Y)).Replace("\t", m_project.getTab()).Length;
                        }

                        m_project.getSelectedBufferView().setCursorPosition(new ScreenPosition(fp));

                        m_project.getSelectedBufferView().deleteSingle(m_project);
                    }
                    updateSmartHelp();
                }
            }
            else if (keyList.Contains(Keys.Enter))
            {
                //ScreenPosition fp = m_project.getSelectedBufferView().getCursorPosition();

                if (m_state.equals("FileSaveAs"))
                {
                    // Check that the filename is valid
                    //
                    if (m_saveFileName != "" && m_saveFileName != null)
                    {
                        m_project.getSelectedBufferView().getFileBuffer().setFilepath(m_fileSystemView.getPath() + m_saveFileName);

                        Logger.logMsg("XygloXNA::processActionKeys() - file name = " + m_project.getSelectedBufferView().getFileBuffer().getFilepath());

                        completeSaveFile(gameTime);

                        // Now if we have remaining files to write then we need to carry on saving files
                        //
                        if (m_filesToWrite != null)
                        {
                            //m_filesToWrite.Remove(m_project.getSelectedBufferView().getFileBuffer());

                            // If we have remaining files to edit then set the active BufferView to one that
                            // looks over this file - then fly to it and choose and file location.
                            //
                            if (m_filesToWrite.Count > 0)
                            {
                                m_project.setSelectedBufferView(m_filesToWrite[0]);
                                m_eye = m_project.getSelectedBufferView().getEyePosition();
                                selectSaveFile();
                            }
                            else // We're done 
                            {
                                m_filesToWrite = null;
                                Logger.logMsg("XygloXNA::processActionKeys() - saved some files.  Quitting.");

                                // Exit nicely and ensure we serialise
                                //
                                checkExit(gameTime);
                            }
                        }
                        else
                        {
                            // Exit nicely and ensure we serialise
                            //
                            if (m_saveAsExit)
                            {
                                checkExit(gameTime);
                            }
                        }
                    }
                }
                else if (m_state.equals("FileOpen"))
                {
                    traverseDirectory(gameTime);
                }
                else if (m_state.equals("Configuration"))
                {
                    // Set this status so that we edit the item
                    //
                    if (m_editConfigurationItem == false)
                    {
                        // Go into item edit mode and copy across the current value
                        m_editConfigurationItem = true;
                        m_editConfigurationItemValue = m_project.getConfigurationItem(m_configPosition).Value;
                    }
                    else
                    {
                        // Completed editing the item - now set it
                        //
                        m_editConfigurationItem = false;
                        m_project.updateConfigurationItem(m_project.getConfigurationItem(m_configPosition).Name, m_editConfigurationItemValue);
                    }
                }
                else if (m_state.equals("FindText"))
                {
                    doSearch(gameTime);
                }
                else if (m_state.equals("GotoLine"))
                {
                    try
                    {
                        int gotoLine = Convert.ToInt16(m_gotoLine);

                        if (gotoLine > 0)
                        {
                            if (gotoLine < m_project.getSelectedBufferView().getFileBuffer().getLineCount() - 1)
                            {
                                ScreenPosition sp = new ScreenPosition(0, gotoLine);
                                m_project.getSelectedBufferView().setCursorPosition(sp);
                            }
                            else
                            {
                                setTemporaryMessage("Attempted to go beyond end of file.", 2, gameTime);
                            }
                        }
                    }
                    catch (Exception /* e */)
                    {
                        Logger.logMsg("Probably got junk in the goto line dialog");
                        setTemporaryMessage("Lines are identified by numbers.", 2, gameTime);
                    }

                    m_gotoLine = "";
                    m_state = State.Test("TextEditing");
                }
                else
                {
                    // Insert a line into the editor
                    //
                    string indent = "";

                    try
                    {
                        indent = m_project.getConfigurationValue("AUTOINDENT");
                    }
                    catch (Exception e)
                    {
                        Logger.logMsg("XygloXNA::processActionKeys) - couldn't get AUTOINDENT from config - " + e.Message);
                    }

                    if (m_project.getSelectedBufferView().gotHighlight())
                    {
                        m_project.getSelectedBufferView().replaceCurrentSelection(m_project, "\n");
                    }
                    else
                    {
                        m_project.getSelectedBufferView().insertNewLine(m_project, indent);
                    }
                    updateSmartHelp();
                }
            }
        }


        /// <summary>
        /// Process key combinations and commands from the keyboard - return true if we've
        /// captured a command so we don't print that character
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected bool processCombinationsCommands(GameTime gameTime, List<KeyAction> keyActionList)
        {
            bool rC = false;
            List<Keys> keyList = new List<Keys>();
            foreach (KeyAction keyAction in keyActionList)
            {
                keyList.Add(keyAction.m_key);
            }

            if (m_confirmState.equals("ConfirmQuit") && keyList.Contains(Keys.Y))
            {
                m_confirmQuit = true;
                checkExit(gameTime, true);
                return true;
            }

            // Check confirm state - this works out the complicated statuses of open file buffers
            //
            if (!m_confirmState.equals("None"))
            {
                if (keyList.Contains(Keys.Y))
                {
                    Logger.logMsg("XygloXNA::processCombinationsCommands() - confirm y/n");
                    try
                    {
                        if (m_confirmState.equals("FileSave"))
                        {
                            // Select a file path if we need one
                            //
                            if (m_project.getSelectedBufferView().getFileBuffer().getFilepath() == "")
                            {
                                selectSaveFile();
                            }
                            else
                            {
                                // Attempt save
                                //
                                if (checkFileSave())
                                {
                                    // Save has completed without error
                                    //
                                    setTemporaryMessage("Saved.", 2, gameTime);
                                }

                                m_state = State.Test("TextEditing");
                                rC = true;
                            }
                        }
                        else if (m_confirmState.equals("FileSaveCancel"))
                        {
                            // First of all save all open buffers we can write and save
                            // a list of all those we can't
                            //
                            m_filesToWrite = new List<FileBuffer>();

                            foreach (FileBuffer fb in m_project.getFileBuffers())
                            {
                                if (fb.isModified())
                                {
                                    if (fb.isWriteable())
                                    {
                                        fb.save();
                                    }
                                    else
                                    {
                                        // Only add a filebuffer if it's not the same physical file
                                        //
                                        bool addFileBuffer = true;
                                        foreach (FileBuffer fb2 in m_filesToWrite)
                                        {
                                            if (fb2.getFilepath() == fb.getFilepath())
                                            {
                                                addFileBuffer = false;
                                                break;
                                            }
                                        }

                                        if (addFileBuffer)
                                        {
                                            m_filesToWrite.Add(fb);
                                        }
                                    }
                                }
                            }

                            // All files saved then exit
                            //
                            if (m_filesToWrite.Count == 0)
                            {
                                checkExit(gameTime);
                            }
                        }
                        else if (m_confirmState.equals("CancelBuild"))
                        {
                            Logger.logMsg("XygloXNA::processCombinationsCommands() - cancel build");
                            m_buildProcess.Close();
                            m_buildProcess = null;
                        }
                        else if (m_confirmState.equals("ConfirmQuit"))
                        {
                            m_confirmQuit = true;
                            checkExit(gameTime, true);
                        }
                        rC = true; // consume this letter
                    }
                    catch (Exception e)
                    {
                        setTemporaryMessage("Save failed with \"" + e.Message + "\".", 5, gameTime);
                    }

                    m_confirmState.set("None");
                }
                else if (keyList.Contains(Keys.N))
                {
                    // If no for single file save then continue - if no for FileSaveCancel then quit
                    //
                    if (m_confirmState.equals("FileSave"))
                    {
                        m_temporaryMessage = "";
                        m_confirmState.set("None");
                    }
                    else if (m_confirmState.equals("FileSaveCancel"))
                    {
                        // Exit nicely
                        //
                        checkExit(gameTime, true);
                    }
                    else if (m_confirmState.equals("CancelBuild"))
                    {
                        setTemporaryMessage("Continuing build..", 2, gameTime);
                        m_confirmState.set("None");
                    }
                    else if (m_confirmState.equals("ConfirmQuit"))
                    {
                        setTemporaryMessage("Cancelled quit", 2, gameTime);
                        m_confirmState.set("None");
                    }
                    rC = true; // consume this letter
                }
                else if (keyList.Contains(Keys.C) && m_confirmState.equals("FileSaveCancel"))
                {
                    setTemporaryMessage("Cancelled Quit.", 0.5, gameTime);
                    m_confirmState.set("None");
                    rC = true;
                }
            }
            else if (m_ctrlDown && !m_altDown)  // CTRL down and no ALT
            {
                if (keyList.Contains(Keys.C)) // Copy
                {
                    if (m_state.equals("Configuration") && m_editConfigurationItem)
                    {
                        Logger.logMsg("XygloXNA::processCombinationsCommands() - copying from configuration");
                        System.Windows.Forms.Clipboard.SetText(m_editConfigurationItemValue);
                    }
                    else
                    {
                        Logger.logMsg("XygloXNA::processCombinationsCommands() - copying to clipboard");
                        string text = m_project.getSelectedBufferView().getSelection(m_project).getClipboardString();

                        // We can only set this is the text is not empty
                        if (text != "")
                        {
                            System.Windows.Forms.Clipboard.SetText(text);
                        }
                        rC = true;
                    }
                }
                else if (keyList.Contains(Keys.X)) // Cut
                {
                    if (m_state.equals("Configuration") && m_editConfigurationItem)
                    {
                        Logger.logMsg("XygloXNA::processCombinationsCommands() - cutting from configuration");
                        System.Windows.Forms.Clipboard.SetText(m_editConfigurationItemValue);
                        m_editConfigurationItemValue = "";
                    }
                    else
                    {
                        Logger.logMsg("XygloXNA::processCombinationsCommands() - cut");

                        System.Windows.Forms.Clipboard.SetText(m_project.getSelectedBufferView().getSelection(m_project).getClipboardString());
                        m_project.getSelectedBufferView().deleteCurrentSelection(m_project);
                        rC = true;
                    }
                }
                else if (keyList.Contains(Keys.V)) // Paste
                {
                    if (System.Windows.Forms.Clipboard.ContainsText())
                    {
                        if (m_state.equals("Configuration") && m_editConfigurationItem)
                        {
                            Logger.logMsg("XygloXNA::processCombinationsCommands() - pasting into configuration");

                            // Ensure that we only get one line out of the clipboard and make sure
                            // it's the last meaningful one.
                            //
                            string lastPasteText = "";
                            foreach (string text in System.Windows.Forms.Clipboard.GetText().Split('\n'))
                            {
                                if (text != "")
                                {
                                    lastPasteText = text;
                                }
                            }

                            m_editConfigurationItemValue = lastPasteText;
                        }
                        else
                        {
                            Logger.logMsg("XygloXNA::processCombinationsCommands() - pasting text");
                            // If we have a selection then replace it - else insert
                            //
                            if (m_project.getSelectedBufferView().gotHighlight())
                            {
                                m_project.getSelectedBufferView().replaceCurrentSelection(m_project, System.Windows.Forms.Clipboard.GetText());
                            }
                            else
                            {
                                m_project.getSelectedBufferView().insertText(m_project, System.Windows.Forms.Clipboard.GetText());
                            }
                            updateSmartHelp();
                            rC = true;
                        }
                    }
                }
                else if (keyList.Contains(Keys.Z))  // Undo
                {
                    // Undo a certain number of steps
                    //
                    try
                    {
                        // We call the undo against the FileBuffer and this returns the cursor position
                        // resulting from this action.
                        //
                        if (m_project.getSelectedBufferView().getFileBuffer().getUndoPosition() > 0)
                        {
                            m_project.getSelectedBufferView().undo(m_project, 1);
                            updateSmartHelp();
                        }
                        else
                        {
                            setTemporaryMessage("Nothing to undo.", 1.0, gameTime);
                        }
                    }
                    catch (Exception e)
                    {
                        //System.Windows.Forms.MessageBox.Show("Undo stack is empty - " + e.Message);
                        Logger.logMsg("XygloXNA::processCombinationsCommands() - got exception " + e.Message);
                        setTemporaryMessage("Nothing to undo with exception.", 2, gameTime);
                    }

                    // Always return true
                    //
                    rC = true;
                }
                else if (keyList.Contains(Keys.Y))  // Redo
                {
                    // Undo a certain number of steps
                    //
                    try
                    {
                        // We call the undo against the FileBuffer and this returns the cursor position
                        // resulting from this action.
                        //
                        if (m_project.getSelectedBufferView().getFileBuffer().getUndoPosition() <
                            m_project.getSelectedBufferView().getFileBuffer().getCommandStackLength())
                        {
                            m_project.getSelectedBufferView().redo(m_project, 1);
                            updateSmartHelp();
                        }
                        else
                        {
                            setTemporaryMessage("Nothing to redo.", 1.0, gameTime);
                        }
                    }
                    catch (Exception e)
                    {
                        //System.Windows.Forms.MessageBox.Show("Undo stack is empty - " + e.Message);
                        Logger.logMsg("XygloXNA::processCombinationsCommands() - got exception " + e.Message);
                        setTemporaryMessage("Nothing to redo.", 2, gameTime);
                    }

                    // Always return true as we've captured the event
                    //
                    rC = true;
                }
                else if (keyList.Contains(Keys.A))  // Select all
                {
                    m_project.getSelectedBufferView().selectAll();
                    rC = true;
                }
                else if (keyList.Contains(Keys.OemPlus)) // increment bloom state
                {
                    if (m_shiftDown)
                    {
                        m_fontScaleOriginal = m_project.getSelectedBufferView().incrementViewSize(m_graphics.GraphicsDevice.Viewport.Width, m_graphics.GraphicsDevice.Viewport.Height, m_project.getFontManager());
                        m_currentFontScale = 0.0f;
                        setActiveBuffer();
                    }
                    else
                    {
                        m_bloomSettingsIndex = (m_bloomSettingsIndex + 1) % BloomSettings.PresetSettings.Length;
                        m_bloom.Settings = BloomSettings.PresetSettings[m_bloomSettingsIndex];
                        m_bloom.Visible = true;

                        setTemporaryMessage("Bloom set to " + BloomSettings.PresetSettings[m_bloomSettingsIndex].Name, 3, gameTime);
                    }
                }
                else if (keyList.Contains(Keys.OemMinus)) // decrement bloom state
                {
                    if (m_shiftDown)
                    {
                        m_fontScaleOriginal = m_project.getSelectedBufferView().decrementViewSize(m_graphics.GraphicsDevice.Viewport.Width, m_graphics.GraphicsDevice.Viewport.Height, m_project.getFontManager());
                        m_currentFontScale = 0.0f;
                        setActiveBuffer();
                    }
                    else
                    {
                        m_bloomSettingsIndex = (m_bloomSettingsIndex - 1);

                        if (m_bloomSettingsIndex < 0)
                        {
                            m_bloomSettingsIndex += BloomSettings.PresetSettings.Length;
                        }

                        m_bloom.Settings = BloomSettings.PresetSettings[m_bloomSettingsIndex];
                        m_bloom.Visible = true;
                        setTemporaryMessage("Bloom set to " + BloomSettings.PresetSettings[m_bloomSettingsIndex].Name, 3, gameTime);
                    }
                }
                else if (keyList.Contains(Keys.B)) // Toggle bloom
                {
                    m_bloom.Visible = !m_bloom.Visible;
                    setTemporaryMessage("Bloom " + (m_bloom.Visible ? "on" : "off"), 3, gameTime);
                }

            }
            else if (m_altDown && !m_ctrlDown) // ALT down action and no CTRL down
            {
                if (keyList.Contains(Keys.S) && m_project.getSelectedBufferView().getFileBuffer().isModified())
                {
                    // If we want to confirm save then ask
                    //
                    if (m_confirmFileSave)
                    {
                        setTemporaryMessage("Confirm Save? Y/N", 0, gameTime);
                        m_confirmState.set("FileSave");
                    }
                    else  // just save
                    {
                        // Select a file path if we need one
                        //
                        if (m_project.getSelectedBufferView().getFileBuffer().getFilepath() == "")
                        {
                            m_saveAsExit = false;
                            selectSaveFile();
                        }
                        else
                        {
                            // Attempt save
                            //
                            if (checkFileSave())
                            {
                                // Save has completed without error
                                //
                                setTemporaryMessage("Saved.", 2, gameTime);
                            }

                            m_state = State.Test("TextEditing");
                        }
                        rC = true;
                    }
                }
                else if (keyList.Contains(Keys.A)) // Explicit save as
                {
                    m_saveAsExit = false;
                    selectSaveFile();
                    rC = true;
                }
                else if (keyList.Contains(Keys.N)) // New BufferView on new FileBuffer
                {
                    m_state = State.Test("PositionScreenNew");
                    rC = true;
                }
                else if (keyList.Contains(Keys.B)) // New BufferView on same FileBuffer (copy the existing BufferView)
                {
                    m_state = State.Test("PositionScreenCopy");
                    rC = true;
                }
                else if (keyList.Contains(Keys.O)) // Open a file
                {
                    selectOpenFile();
                    rC = true;
                }
                else if (keyList.Contains(Keys.H)) // Show the help screen
                {
                    // Reset page position and set information mode
                    //
                    m_textScreenPositionY = 0;
                    m_state = State.Test("Help");
                    rC = true;
                }
                else if (keyList.Contains(Keys.I)) // Show the information screen
                {
                    // Reset page position and set information mode
                    //
                    m_textScreenPositionY = 0;
                    m_state = State.Test("Information");
                    rC = true;
                }
                else if (keyList.Contains(Keys.G)) // Show the config screen
                {
                    // Reset page position and set information mode
                    //
                    m_textScreenPositionY = 0;
                    showConfigurationScreen();
                    rC = true;
                }
                else if (keyList.Contains(Keys.C)) // Close current BufferView
                {
                    closeActiveBuffer(gameTime);
                    rC = true;
                }
                else if (keyList.Contains(Keys.D))
                {
                    m_state = State.Test("DiffPicker");
                    setTemporaryMessage("Pick a BufferView to diff against", 5, gameTime);

                    // Set up the differ
                    //
                    if (m_differ == null)
                    {
                        m_differ = new Differ();
                    }
                    else
                    {
                        m_differ.clear();
                    }

                    rC = true;
                }
                else if (keyList.Contains(Keys.M))
                {
                    // Set the config position - we (re)use this to hold menu position in the manage
                    // project screen for removing file items.
                    //
                    m_configPosition = 0;

                    m_state = State.Test("ManageProject"); // Manage the files in the project

                    // Copy current position to m_projectPosition - then rebuild model
                    //
                    m_projectPosition = m_project.getSelectedBufferView().getPosition();
                    m_projectPosition.X = -1000.0f;
                    m_projectPosition.Y = -1000.0f;

                    generateTreeModel();

                    // Fly to a new position in this mode to view the model
                    //
                    Vector3 newPos = m_projectPosition;
                    newPos.Z = 800.0f;
                    flyToPosition(newPos);
                    rC = true;
                }
                else if (keyList.Contains(Keys.D0) ||
                            keyList.Contains(Keys.D1) ||
                            keyList.Contains(Keys.D2) ||
                            keyList.Contains(Keys.D3) ||
                            keyList.Contains(Keys.D4) ||
                            keyList.Contains(Keys.D5) ||
                            keyList.Contains(Keys.D6) ||
                            keyList.Contains(Keys.D7) ||
                            keyList.Contains(Keys.D8) ||
                            keyList.Contains(Keys.D9))
                {
                    m_gotoBufferView += getNumberKey();
                    rC = true;
                }


                /*
                // Don't do any state transitions in this class now
                //  
                else if (keyList.Contains(Keys.F)) // Find text
                {
                    Logger.logMsg("XygloXNA::processCombinationsCommands() - find");
                    m_state = State.Test("FindText");
                }
                else if (keyList.Contains(Keys.L)) // go to line
                {
                    Logger.logMsg("XygloXNA::processCombinationsCommands() - goto line");
                    m_state = State.Test("GotoLine");
                }*/ 


            }
            else if (m_windowsDown) // Windows keys
            {
                // Initialially tried CTRL and ALT combinations but ran up against this:
                //
                // http://forums.create.msdn.com/forums/t/41522.aspx
                // 
                // and then this solution which I ignored:
                //
                // http://bnoerj.codeplex.com/wikipage?title=Bnoerj.Winshoked&referringTitle=Home
                //
                //
                if (keyList.Contains(Keys.A))
                {
                    Logger.logMsg("RELAYOUT AND FLY");  //???
                }
            }

            return rC;
        }


        /// <summary>
        /// Get a list of mouse actions since the last time we had a mouse action - create an
        /// event based interface for the mouse.
        /// </summary>
        /// <returns></returns>
        protected List<MouseAction> getAllMouseActions()
        {
            List<MouseAction> lMA = new List<MouseAction>();
            MouseState mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

            // This conversion process diffs the states and returns the results
            //
            List<Mouse> currentMouseList = XygloConvert.convertMouseMappings(mouseState, m_lastMouseState);
            
            // So now we have all the differences between current mouse list and last one.
            // Generate some MouseActions for these.
            //
            foreach (Mouse mouse in currentMouseList)
            {
                MouseAction mouseAction = new MouseAction(mouse);
                mouseAction.m_position.X = mouseState.X;
                mouseAction.m_position.Y = mouseState.Y;
                mouseAction.m_scrollWheel = mouseState.ScrollWheelValue;
                lMA.Add(mouseAction);
            }

            // If nothing has changed then at least update the positional information
            //
            /*
            if (lMA.Count == 0)
            {
                // No mouse action but we might have movement
                //
                MouseAction mouseAction = new MouseAction();
                if (mouseState.X != m_lastMouseState.X)
                {
                    mouseAction.m_position.X = mouseState.X;
                }

                if (mouseState.Y != m_lastMouseState.Y)
                {
                    mouseAction.m_position.Y = mouseState.Y;
                }

                if (mouseState.ScrollWheelValue != m_lastMouseState.ScrollWheelValue)
                {
                    mouseAction.m_scrollWheel = mouseState.ScrollWheelValue;
                }
                lMA.Add(mouseAction);
            }
            */

            /*
            // Now get the inverse state
            //
            foreach (Mouse lastMouse in lastMouseList)
            {
                if (!currentMouseList.Contains(lastMouse))
                {
                    MouseAction mouseAction = new MouseAction(lastMouse);
                    mouseAction.m_position.X = mouseState.X;
                    mouseAction.m_position.Y = mouseState.Y;
                    mouseAction.m_scrollWheel = mouseState.ScrollWheelValue;
                    lMA.Add(mouseAction);
                }
            }*/

            return lMA;
        }

        /// <summary>
        /// Remove keyboard modifiers from a list of XNA Keys
        /// </summary>
        /// <param name="inList"></param>
        /// <returns></returns>
        protected List<Keys> removeModifiers(List<Keys> inList)
        {
            List<Keys> outList = new List<Keys>();
            List<Keys> modifierList = new List<Keys>();
            modifierList.Add(Keys.LeftShift);
            modifierList.Add(Keys.RightShift);
            modifierList.Add(Keys.LeftControl);
            modifierList.Add(Keys.RightControl);
            modifierList.Add(Keys.LeftAlt);
            modifierList.Add(Keys.RightAlt);
            modifierList.Add(Keys.LeftWindows);
            modifierList.Add(Keys.RightWindows);

            foreach (Keys key in inList)
            {
                if (!modifierList.Contains(key))
                {
                    outList.Add(key);
                }
            }

            return outList;
        }

        /// <summary>
        /// Get all the KeyActions that are currently in progress - whether keys be newly down 
        /// or held down or released.  We can use this method to define repeat timings for individual
        /// keys as well.
        /// </summary>
        /// <returns></returns>
        protected List<KeyAction> getAllKeyActions()
        {
            List<KeyAction> lKA = new List<KeyAction>();
            List<Keys> newKeys = XygloConvert.keyMappings(Keyboard.GetState().GetPressedKeys());
            List<Keys> lastKeys = XygloConvert.keyMappings(m_lastKeyboardState.GetPressedKeys());
            KeyboardModifier modifier = KeyboardModifier.None;

            // Check for modifiers - flag and remove
            //
            if (newKeys.Contains(Keys.LeftShift))
            {
                modifier |= KeyboardModifier.Shift;
                //newKeys.Remove(Keys.LeftShift);
            }

            if (newKeys.Contains(Keys.RightShift))
            {
                modifier |= KeyboardModifier.Shift;
                //newKeys.Remove(Keys.RightShift);
            }

            if (newKeys.Contains(Keys.LeftControl))
            {
                modifier |= KeyboardModifier.Control;
                //newKeys.Remove(Keys.LeftControl);
            }

            if (newKeys.Contains(Keys.RightControl))
            {
                modifier |= KeyboardModifier.Control;
                //newKeys.Remove(Keys.RightControl);
            }

            if (newKeys.Contains(Keys.LeftAlt))
            {
                modifier |= KeyboardModifier.Alt;
                //newKeys.Remove(Keys.LeftAlt);
            }

            if (newKeys.Contains(Keys.RightAlt))
            {
                modifier |= KeyboardModifier.Alt;
                //newKeys.Remove(Keys.RightAlt);
            }

            if (newKeys.Contains(Keys.LeftWindows))
            {
                modifier |= KeyboardModifier.Windows;
                //newKeys.Remove(Keys.LeftWindows);
            }

            if (newKeys.Contains(Keys.RightWindows))
            {
                modifier |= KeyboardModifier.Windows;
                //newKeys.Remove(Keys.RightWindows);
            }

            // At this point we can work out if we have any new keys pressed or any held
            // 
            foreach (Keys key in removeModifiers(newKeys))
            {
                bool pressed = true;

                foreach (Keys lastKey in XygloConvert.keyMappings(m_lastKeyboardState.GetPressedKeys()))
                {
                    if (lastKey == key) // was down last time so hasn't been pressed - is held
                    {
                        pressed = false;  // set flag
                        lastKeys.Remove(lastKey); // and remove from lastKeys
                        break;
                    }
                }

                KeyAction keyAction = new KeyAction(key, modifier);

                if (pressed)
                {
                    keyAction.m_state = KeyButtonState.Pressed;
                }
                else
                {
                    keyAction.m_state = KeyButtonState.Held;
                }

                lKA.Add(keyAction);
            }

            // Now any keys that are were and have been released.
            //
            foreach (Keys key in removeModifiers(lastKeys))
            {
                KeyAction keyAction = new KeyAction(key, modifier);
                keyAction.m_state = KeyButtonState.Released;
                lKA.Add(keyAction);
            }

            // Finally set the convenience flags for the modifiers already worked out
            //
            m_shiftDown = ((modifier & KeyboardModifier.Shift ) == KeyboardModifier.Shift);
            m_ctrlDown = ((modifier & KeyboardModifier.Control) == KeyboardModifier.Control);
            m_altDown = ((modifier & KeyboardModifier.Alt) == KeyboardModifier.Alt);
            m_windowsDown = ((modifier & KeyboardModifier.Windows) == KeyboardModifier.Windows);

            return lKA;
        }

        /// <summary>
        /// Ensure that all components are reactivated before changing to that state
        /// </summary>
        /// <param name="newState"></param>
        protected void setState(string newState)
        {
            foreach (Component component in m_componentList)
            {
                component.setDestroyed(false);
            }

            m_state = State.Test(newState);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world, checking for collisions, gathering input, and playing audio.
        /// Also handles all the keypresses and other movemements.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void Update(GameTime gameTime)
        {
            m_elapsedTime += gameTime.ElapsedGameTime;

            if (m_elapsedTime > TimeSpan.FromSeconds(1))
            {
                m_elapsedTime -= TimeSpan.FromSeconds(1);
                m_frameRate = m_frameCounter;
                m_frameCounter = 0;
            }

            // Update the frustrum matrix
            //
            if (m_frustrum != null)
            {
                m_frustrum.Matrix = m_viewMatrix * m_projection;
            }

            // Check for end states that require no further processing
            //
            if (m_state.equals("RestartLevel"))
            {                        
                m_drawableComponents.Clear();
                setState("PlayingGame");
            }

            if (m_state.equals("GameOver"))
            {
                m_drawableComponents.Clear();
            }

            // getAllKeyActions also works out the modifiers and applies them
            // to the KeyActions in the list.  This also sets the relevant shift,
            // alt, ctrl, windows flags.
            //
            List<KeyAction> keyActionList = getAllKeyActions();

            // Do we consume a key?  Has it been used in a Metacommand?
            //
            //bool consume = false;

            foreach(KeyAction keyAction in keyActionList)
            {
                // We check and discard key events that aren't within the repeat or press
                // window at this point.  So we apply the same delays for all keys currently.
                //
                if (!checkKeyRepeat(gameTime, keyAction))
                    continue;

                // Process action keys
                //
                if (m_project != null)
                {
                    processActionKey(gameTime, keyAction);

                    // do any key combinations
                    //
                    if (processCombinationsCommands(gameTime, keyActionList))
                        continue;
                }

                // Get a target for this (potential) combination of keys
                //
                Target target = m_actionMap.getTargetForKey(m_state, keyAction);

                // Now fire off the keys according to the Target
                switch (target.m_name)
                {
                    // --- FRIENDLIER cases ---
                    //
                    case "None":
                        // do nothing;
                        break;

                    //case Target.Default:
                    //case Target.CurrentBufferView:
                    case "Default":
                    case "CurrentBufferView":
                        // The default target will process meta key commands
                        //
                        processKey(gameTime, keyAction);
                        /* consume = */ processMetaCommand(gameTime, keyAction);
                        break;
                    
                        // For OpenFile all we need to do is change state (for the moment)
                        //
                    //case Target.OpenFile:
                    case "OpenFile":
                        m_state = State.Test("FileOpen");
                        break;

                    case "NewBufferView":
                        m_state = State.Test("PositionScreenNew");
                        break;

                    //case Target.SaveFile:
                    case "SaveFile":
                        selectSaveFile();
                        break;

                    case "ShowInformation":
                        m_state = State.Test("Information");
                        break;

                    //case Target.CursorUp:
                    case "CursorUp":
                        switch (m_state.m_name)
                        {
                            //case State.Test("FileOpen"):
                            case "FileOpen":
                                
                            default:
                                break;
                        }
                        break;

                    case "CursorDown":
                        break;

                    case "CursorLeft":
                        break;

                    case "CursorRight":
                        break;


                        // --- PAULO cases ---
                        //
                    case "Exit":
                        // The default target will process meta key commands
                        //
                        processKey(gameTime, keyAction);
                        /* consume = */ processMetaCommand(gameTime, keyAction);
                        break;

                        // If we hit this target then transition to PlayingGame
                    case "StartPlaying":
                        // Before we start the state ensure that all components are reset to being active
                        //
                        setState("PlayingGame");
                        break;

                    case "QuitToMenu":
                        // Before we quit to menu we want to remove all of our drawing shapes
                        //
                        m_drawableComponents.Clear();
                        m_state = State.Test("Menu");

                        // Clear this global
                        m_interloper = null;
                        break;

                    case "MoveLeft":
                        Vector3 leftVector = new Vector3(-1, 0, 0);
                        // accelerate will accelerate in mid air or move
                        Pair<XygloXnaDrawable, Vector3> coll = checkCollisions(m_interloper);

                        // If there is an X component to the checkCollisions call then we're on an object
                        // or so we guess at the moment until proven otherwise.
                        //
                        if (coll.Second.X != 0)
                        {
                            m_drawableComponents[m_interloper].moveLeft(1);
                        }
                        else // we're in free flight
                        {
                            m_drawableComponents[m_interloper].accelerate(leftVector);
                        }
                        break;

                    case "MoveRight":
                        Vector3 rightVector = new Vector3(1, 0, 0);
                        // accelerate will accelerate in mid air or move

                        Pair<XygloXnaDrawable, Vector3> colr = checkCollisions(m_interloper);

                        // If there is an X component to the checkCollisions call then we're on an object
                        // or so we guess at the moment until proven otherwise.
                        //
                        if (colr.Second.X != 0)
                        {
                            m_drawableComponents[m_interloper].moveRight(1);
                        }
                        else // we're in free flight
                        {
                            m_drawableComponents[m_interloper].accelerate(rightVector);
                        }
                        break;

                        // Jump the interloper
                        //
                    case "Jump":
                        m_drawableComponents[m_interloper].jump(new Vector3(0, -4, 0));
                        break;

                    case "MoveForward":
                    case "MoveBack":
                        break;

                    default:
                        // In the default state just try to change state to the passed target
                        // i.e. Target = New State
                        //
                        // This will throw an exception if the target state isn't found
                        m_state = confirmState(target.m_name);
                        //throw new XygloException("Update", "Unhandled Target encountered");
                        break;
                }
            }

            // Turn KeyAction list to key list
            //
            List<Keys> keyList = new List<Keys>();
            foreach (KeyAction keyAction in keyActionList)
            {
                keyList.Add(keyAction.m_key);
            }

            // Return after these commands have been processed for the demo version
            //
            if (m_project != null && !m_project.getLicenced())
            {
                // Allow the game to exit
                //
                if (keyList.Contains(Keys.Escape))
                {
                    checkExit(gameTime, true);
                }

                return;
            }
            
            // Set the cursor to something useful
            //
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.IBeam;

            // Set the startup banner on the first pass through
            //
            if (m_project != null & m_gameTime == null)
            {
                m_drawingHelper.startBanner(gameTime, VersionInformation.getProductName() + "\n" + VersionInformation.getProductVersion(), 5);
            }

            // Store gameTime for use in helper functions
            //
            m_gameTime = gameTime;

            // Fetch all the mouse actions too
            //
            List<MouseAction> mouseActionList = getAllMouseActions();

            // Check for any mouse actions here
            //
            checkMouse(gameTime, mouseActionList);

            // limit number of keys
            //
            m_processKeyboardAllowed = gameTime.TotalGameTime + new TimeSpan(0, 0, 0, 0, 100);

            // Check for this change as necessary
            //
            changeEyePosition(gameTime);

            // Save the last state if it has changed and clear any temporary message
            //
            if (m_lastKeyboardState != Keyboard.GetState())
            {
                m_lastKeyboardState = Keyboard.GetState();
            }

            // Save this to ensure we can keep processing
            //
            if (m_processKeyboardAllowed < gameTime.TotalGameTime)
            {
                m_processKeyboardAllowed = gameTime.TotalGameTime;
            }

            // We can add gravity or other stuff to this component as required 
            //
            //BrazilVector3 acceleration = BrazilVector3.Zero;

            // Process components for MOVEMENT or creation depending on key context
            //
            foreach (Component component in m_componentList)
            {
                // Has this component already been added to the drawableComponent dictionary?
                //
                if (m_drawableComponents.ContainsKey(component)) // && m_drawableComponents[component].getVelocity() != Vector3.Zero)
                {
                    // If so then process it for any movement
                    //
                    if (component.GetType() == typeof(Xyglo.Brazil.BrazilFlyingBlock))
                    {
                        // Found a FlyingBlock - initialise it and add it to the dictionary
                        //
                        BrazilFlyingBlock fb = (Xyglo.Brazil.BrazilFlyingBlock)component;

                        // Check and accelerate the drawable as needed
                        //
                        if (fb.isAffectedByGravity())
                        {
                            m_drawableComponents[component].accelerate(XygloConvert.getVector3(m_world.getGravity()));
                        }

                        // Move any update any buffers
                        //
                        //m_drawableComponents[component].move(XygloConvert.getVector3(fb.getVelocity()));
                        m_drawableComponents[component].moveDefault();

                        // Apply any rotation if we have one
                        if (fb.getRotation() != 0)
                        {
                            m_drawableComponents[component].incrementRotation(fb.getRotation());
                        }

                        m_drawableComponents[component].buildBuffers(m_graphics.GraphicsDevice);
                    }
                    else if (component.GetType() == typeof(Xyglo.Brazil.BrazilInterloper))
                    {
                        BrazilInterloper il = (Xyglo.Brazil.BrazilInterloper)component;

                        // Accelerate this object if there is gravity
                        //
                        if (il.isAffectedByGravity())
                        {
                            m_drawableComponents[component].accelerate(XygloConvert.getVector3(m_world.getGravity()));
                        }

                        // Check for collisions and adjust the position and velocity accordingly before drawing this
                        //
                        computeCollisions();
                        //{
                            // Move any update any buffers
                            //
                            //m_drawableComponents[component].move(XygloConvert.getVector3(il.getVelocity()));
                            m_drawableComponents[component].moveDefault();
                        //}

                        // Apply any rotation if we have one
                        if (il.getRotation() != 0)
                        {
                            m_drawableComponents[component].incrementRotation(il.getRotation());  // this is initial rotation only
                        }

                        m_drawableComponents[component].buildBuffers(m_graphics.GraphicsDevice);

                        // Store our interloper object
                        //
                        if (m_interloper == null)
                        {
                            m_interloper = il;
                        }
                    }
                    else if (component.GetType() == typeof(Xyglo.Brazil.BrazilGoody))
                    {
                        //Logger.logMsg("Draw Goody for the first time");
                        BrazilGoody bg = (BrazilGoody)component;

                        if (bg.m_type == BrazilGoodyType.Coin)
                        {
                            // For the moment the only movement is a rotation
                            //
                            if (bg.getRotation() != 0)
                            {
                                m_drawableComponents[component].incrementRotation(bg.getRotation());
                                m_drawableComponents[component].buildBuffers(m_graphics.GraphicsDevice);
                            }
                        }
                        else
                        {
                            throw new XygloException("Update", "Unsupported Goody Type");
                        }


                        if (!m_drawableComponents[component].shouldBeDestroyed())
                        {
                            m_drawableComponents[component].draw(m_graphics.GraphicsDevice);
                        }
                    }
                    //else if (component.GetType() == typeof(Xyglo.Brazil.BrazilMenu))
                    //{
                        //BrazilMenu menu = (BrazilMenu)component;
                    //}
                }
            }

            // Check for any drawables which need removing and get rid of them
            //
            Dictionary<Component, XygloXnaDrawable> destroyDict = m_drawableComponents.Where(item => item.Value.shouldBeDestroyed() == true).ToDictionary(p => p.Key, p => p.Value);
            foreach (Component destroyKey in destroyDict.Keys)
            {
                XygloXnaDrawable drawable = m_drawableComponents[destroyKey];
                m_drawableComponents.Remove(destroyKey);
                drawable = null;
                
                // Now set the Component to be destroyed so it's not recreated by the next event loop
                //
                destroyKey.setDestroyed(true);
            }

            // Check for world boundary escape
            //
            if (m_interloper != null && !XygloConvert.getBoundingBox(m_world.getBounds()).Intersects(m_drawableComponents[m_interloper].getBoundingBox()))
            {
                Logger.logMsg("Interloper has left the world");
                m_world.setLives(m_world.getLives() - 1);

                if (m_world.getLives() < 0)
                {
                    setState("GameOver");
                }
                else
                {
                    // We've got one less life - this effectively restarts the level from scratch
                    m_interloper = null;
                    m_drawableComponents.Clear();
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Given two components that have collided - play back the velocities of these components until
        /// we discover the collision point
        /// </summary>
        /// <param name="comp1"></param>
        /// <param name="comp2"></param>
        protected Vector3 getCollisionPoint(XygloXnaDrawable comp1, XygloXnaDrawable comp2)
        {
            if (!comp1.getBoundingBox().Intersects(comp2.getBoundingBox()))
            {
                throw new Exception("getCollisionPoint - Testing for a collision that doesn't appear to have happened.");
            }

            BoundingBox b1 = comp1.getBoundingBox();
            BoundingBox b2 = comp2.getBoundingBox();

            Vector3 v1 = comp1.getVelocity();
            Vector3 v2 = comp2.getVelocity();

            if (comp1.getPosition().Y == comp2.getPosition().Y)
            {
                return comp1.getPosition();
            }

            // Draw ray from last last to current position (from where we're not intersecting to where we are intersecting)
            //
            Ray ray = new Ray(comp1.getPosition() - comp1.getVelocity(), comp1.getPosition());

            // This defines the plane we're trying to penetrate with our ray
            //
            Plane intPlane = new Plane(0, -1, 0, comp2.getBoundingBox().Min.Y);

            float? planeIntersection = ray.Intersects(intPlane);

            if (planeIntersection != null)
            {
                //Logger.logMsg("Got PLANE intersect");
                return ray.Position + ray.Direction * planeIntersection.Value;
            }

            // Normalise both vectors and we start playing the boundingbox backwards
            //
            if (v1 != Vector3.Zero)
            {
                v1.Normalize();
            }

            if (v2 != Vector3.Zero)
            {
                v2.Normalize();
            }

            // We are playing these unit vectors backwards against the intersecting positions until
            // they separate.
            //
            while(b1.Intersects(b2))
            {
                b1.Min -= v1;
                b1.Max -= v1;
                b2.Min -= v2;
                b2.Max -= v2;
            }


            // At this point we need to work out _where_ they are seperating from - try all the options
            //
            b1.Min += v1;

            if (b1.Intersects(b2))
            {
                return b1.Min;
            }
            
            b1.Max += v1;

            if (b1.Intersects(b2))
            {
                return b1.Max;
            }

            b2.Min += v2;

            if (b1.Intersects(b2))
            {
                return b2.Max;
            }

            b2.Max += v2;

            if (b1.Intersects(b2))
            {
                return b2.Max;
            }

            throw new Exception("Failed to find the collision boundary");
        }

        /// <summary>
        /// Return a collision point and a component that we've collided with
        /// </summary>
        /// <param name="checkComponent"></param>
        /// <returns></returns>
        protected Pair<XygloXnaDrawable, Vector3> checkCollisions(Component checkComponent)
        {
            // We'll have to iterate the real dictionary for all items that aren't passed in and have something to them
            //
            Dictionary<Component, XygloXnaDrawable> testDict = m_drawableComponents.Where(item => item.Key.isCorporeal() && item.Key != checkComponent).ToDictionary(p => p.Key, p => p.Value);

            XygloXnaDrawable checkComp = m_drawableComponents[checkComponent];

            //realComp.
            BoundingBox bb1 = checkComp.getBoundingBox();

            // Keep a list of all elements we've applied collisions to
            //
            List<XygloXnaDrawable> collisionList = new List<XygloXnaDrawable>();

            foreach (Component testKey in testDict.Keys)
            {
                XygloXnaDrawable testComp = testDict[testKey];
                BoundingBox bb2 = testComp.getBoundingBox();

                if (testComp.getBoundingBox().Intersects(checkComp.getBoundingBox()) && !collisionList.Contains(testComp))
                {
                    // Get the point at which they collided
                    //
                    Vector3 collisionPoint = getCollisionPoint(checkComp, testComp);

                    // Only reset the position if the X axis is unmoved
                    // otherwise we've arrived.
                    //
                    if (collisionPoint.X == checkComp.getPosition().X)
                    {
                        checkComp.setPosition(collisionPoint);
                    }

                    collisionList.Add(checkComp);

                    return new Pair<XygloXnaDrawable, Vector3>(testComp, collisionPoint);
                }
            }

            return new Pair<XygloXnaDrawable, Vector3>(null, Vector3.Zero);

        }

        /// <summary>
        /// Check for collisions in m_drawableComponentss that have some form (hardness != 0).   Return true
        /// if we have a collision and we're also modifying the drawables to have correct velocity
        /// changes.
        /// </summary>
        protected bool computeCollisions()
        {
            // We'll have to iterate the realDict twice but here we filter in any goodies or baddies
            // and also anything that has a hardness (isCorporeal).
            //
            Dictionary<Component, XygloXnaDrawable> realDict = m_drawableComponents.Where(item => item.Key.isCorporeal() || item.Key.getBehaviour() != Behaviour.Goody).ToDictionary(p => p.Key, p => p.Value);

            // Keep a list of all elements we've applied collisions to
            //
            List<XygloXnaDrawable> collisionList = new List<XygloXnaDrawable>();

            // Everything we're interating over has hardness or is a prize/baddy - so they could potentially interact 
            //
            foreach (Component realKey in realDict.Keys)
            {
                // Skip all but Interloper for the moment
                //
                if (realKey.GetType() != typeof(Xyglo.Brazil.BrazilInterloper))
                    continue;

                BrazilInterloper il = (Xyglo.Brazil.BrazilInterloper)realKey;
                
                // For the moment we only 
                foreach (Component testKey in realDict.Keys)
                {
                    // Ignore ourself
                    if (testKey == realKey)
                        continue;

                    XygloXnaDrawable realComp = realDict[realKey];
                    XygloXnaDrawable testComp = realDict[testKey];

                    // If we've already processed this element
                    //
                    if (collisionList.Contains(testComp))
                        continue;

                    // Deal with Goodies
                    //
                    if (testKey.GetType() == typeof(Xyglo.Brazil.BrazilGoody))
                    {
                        BoundingBox bb1 = realComp.getBoundingBox();
                        BoundingSphere bs1 = ((XygloCoin)testComp).getBoundingSphere();

                        // Check for a collision
                        //
                        if (bb1.Intersects(bs1))
                        {
                            BrazilGoody goody = (BrazilGoody)testKey;
                            il.incrementScore(goody.m_worth);

                            // Set this item for destruction
                            //
                            testComp.setDestroy(true);

                            // Add collision item
                            //
                            collisionList.Add(testComp);
                        }
                    }
                    else
                    {
                        // Now we have all the non ourself, non Interloper components
                        //
                        BoundingBox bb1 = realComp.getBoundingBox();
                        BoundingBox bb2 = testComp.getBoundingBox();

                        if (realComp.getBoundingBox().Intersects(testComp.getBoundingBox()))
                        {
                            // Work out the vectors and the rebound angle
                            //
                            //Logger.logMsg("computeCollisions - Got a collision");

                            // Then testKey has mass and realKey doesn't - give testKey a bounce
                            //
                            Vector3 testVely = testComp.getVelocity();
                            Vector3 realVely = realComp.getVelocity();

                            // If they both have mass then do an elastic collision.
                            //
                            if (testKey.getMass() > 0 && realKey.getMass() > 0)
                            {
                                // Elastic collision undefined
                                //
                                //Logger.logMsg("computeCollisions- Elastic collision");
                            }
                            else
                            {
                                if (testKey.getHardness() > 0)
                                {


                                    Vector3 newVely = Vector3.Zero;
                                    //newVely.Y = -realVely.Y * 0.6f;
                                    newVely.Y = -realVely.Y * (float)(realKey.getHardness() / testKey.getHardness());
                                    newVely.X = realVely.X * (float)(realKey.getHardness() / testKey.getHardness());

                                    newVely = XygloConvert.roundVector(newVely, 1);

                                    if (newVely.Length() < 0.5f)
                                    {
                                        newVely = Vector3.Zero;
                                    }
                                    realComp.setVelocity(newVely);


                                    //realComp.accelerate(
                                    collisionList.Add(realComp);

                                    /*
                                    //if (collisionList.Contains(testComp))
                                    //{
                                        // Get the point at which they collided
                                        //
                                        Vector3 collisionPoint = getCollisionPoint(testComp, realComp);

                                        // Get the gravity vector, normalize and use to invert
                                        //
                                        Vector3 unitGravity = XygloConvert.getVector3(m_world.getGravity());
                                        unitGravity.Normalize();

                                        // Invert our velocity by multiplying each element by gravity
                                        if (unitGravity.X > 0) testVely.X *= -unitGravity.X;
                                        if (unitGravity.Y > 0) testVely.Y *= -unitGravity.Y;
                                        if (unitGravity.Z > 0) testVely.Z *= -unitGravity.Z;

                                        // Invert the velocity and modify by a factor
                                        //
                                        //testComp.setVelocity(testVely * (float)testKey.getHardness());

                                        //testComp.se

                                        // Move the testComp by the difference from the surface to the new position below the 
                                        // surface.
                                        //
                                        
                                        Vector3 depthDiff = realComp.getBoundingBox().Max;// - testComp.getPosition().Y;
                                        depthDiff.X = 0;
                                        depthDiff.Z = 0;
                                        depthDiff.Y += testComp.getPosition().Y;
                                        testComp.move(depthDiff);
                                        

                                        //try
                                        //{
                                            //BrazilFlyingBlock xfb = (BrazilFlyingBlock)testKey;

                                            //if (testKey != null) //.GetType() == typeof(XygloFlyingBlock))
                                            //{
                                                //Logger.logMsg("Trying to do somethign weird");
                                            //}
                                        //}
                                        //catch (Exception)
                                        //{
                                        //}

                                        // Only reset the position if the X axis is unmoved
                                        // otherwise we've arrived.
                                        //
                                        if (collisionPoint.X == testComp.getPosition().X)
                                        {
                                            Logger.logMsg("computeCollisions - moving position of interloper");

                                            testComp.setPosition(collisionPoint);
                                        }

                                        collisionList.Add(testComp);
                                    //}*/
                                }
                                /*
                            else // realKey.getMass() > 0
                            {
                                if (!collisionList.Contains(realComp))
                                {
                                    realComp.setVelocity(-realVely);
                                    collisionList.Add(realComp);
                                }
                            }*/
                            }
                        }
                    }
                }
            }

            return (collisionList.Count > 0);
        }

        /// <summary>
        /// Check to see whether a key is available for repeat yet and ensure that the m_keyMap is
        /// updated with the latest status.  Returns true if we can do something with this key.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="keyAction"></param>
        /// <returns></returns>
        protected bool checkKeyRepeat(GameTime gameTime, KeyAction keyAction)
        {
            // Skip all repeats except the ones we want
            //
            if (keyAction.m_state == KeyButtonState.Released)
            {
                // Release the value of the key pressed
                m_keyMap[keyAction.m_key] = null;
                m_keyMap.Remove(keyAction.m_key);
                return false;
            }

            // Create new key storage if it's not available yet - might want to
            // optimise this later.
            //
            if (!m_keyMap.ContainsKey(keyAction.m_key))
            {
                m_keyMap[keyAction.m_key] = new Pair<bool, double>();
            }

            // For the held state we have to check to see if it's ready to repeat this key yet
            //
            if (keyAction.m_state == KeyButtonState.Held)
            {
                if (m_keyMap[keyAction.m_key].First) // Within the first hold interval
                {
                    if ((gameTime.TotalGameTime.TotalSeconds - m_keyMap[keyAction.m_key].Second) < m_repeatHoldTime)
                    {
                        return false;
                    }
                }
                else
                {
                    if ((gameTime.TotalGameTime.TotalSeconds - m_keyMap[keyAction.m_key].Second) < m_repeatInterval)
                    {
                        //Logger.logMsg("LS = " + gameTime.TotalGameTime.TotalSeconds + ", MS = " + m_keyMap[keyAction.m_key].Second);
                        return false;
                    }
                }
            }

            // For pressed and held store the last time this event was issued
            //
            m_keyMap[keyAction.m_key].First = (keyAction.m_state == KeyButtonState.Pressed);
            m_keyMap[keyAction.m_key].Second = gameTime.TotalGameTime.TotalSeconds;

            return true;
        }

        /// <summary>
        /// Process any keys that need to be printed
        /// </summary>
        /// <param name="gameTime"></param>
        protected void processKey(GameTime gameTime, KeyAction keyAction)
        {
            // Do nothing if no keys are pressed except check for auto repeat and clear if necessary.
            // We have to adjust for any held down modifier keys here and also clear the variable as 
            // necessary.

            // Ok, let's see if we can translate a key
            //
            string key = "";

            // Iterate the list and provide some output according to timing conditions on keys and repeats
            //
            //foreach (KeyAction keyAction in keyActionList)
            //{
                // Check for key repeats and handle as necessary
                //
                //if (!checkKeyRepeat(gameTime, keyAction))
                    //continue;

                switch (keyAction.m_key)
                {
                    case Keys.LeftShift:
                    case Keys.RightShift:
                    case Keys.LeftControl:
                    case Keys.RightControl:
                    case Keys.LeftAlt:
                    case Keys.RightAlt:
                        break;

                    case Keys.OemPipe:
                        if (m_shiftDown)
                        {
                            key += "|";
                        }
                        else
                        {
                            key += "\\";
                        }
                        break;

                    case Keys.OemQuestion:
                        if (m_shiftDown)
                        {
                            key += "?";
                        }
                        else
                        {
                            key += "/";
                        }
                        break;

                    case Keys.OemSemicolon:
                        if (m_shiftDown)
                        {
                            key += ":";
                        }
                        else
                        {
                            key += ";";
                        }
                        break;

                    case Keys.OemQuotes:
                        if (m_shiftDown)
                        {
                            key += "\"";
                        }
                        else
                        {
                            key += "'";
                        }
                        break;

                    case Keys.OemTilde:
                        if (m_shiftDown)
                        {
                            key += "@";
                        }
                        else
                        {
                            key += "'";
                        }
                        break;

                    case Keys.OemOpenBrackets:
                        if (m_shiftDown)
                        {
                            key += "{";
                        }
                        else
                        {
                            key += "[";
                        }
                        break;

                    case Keys.OemCloseBrackets:
                        if (m_shiftDown)
                        {
                            key += "}";
                        }
                        else
                        {
                            key += "]";
                        }
                        break;

                    case Keys.D0:
                        if (m_shiftDown)
                        {
                            key += ")";
                        }
                        else
                        {
                            key += "0";
                        }
                        break;

                    case Keys.D1:
                        if (m_shiftDown)
                        {
                            key += "!";
                        }
                        else
                        {
                            key += "1";
                        }
                        break;

                    case Keys.D2:
                        if (m_shiftDown)
                        {
                            key += "@";
                        }
                        else
                        {
                            key += "2";
                        }
                        break;

                    case Keys.D3:
                        if (m_shiftDown)
                        {
                            key += "#";
                        }
                        else
                        {
                            key += "3";
                        }
                        break;

                    case Keys.D4:
                        if (m_shiftDown)
                        {
                            key += "$";
                        }
                        else
                        {
                            key += "4";
                        }
                        break;

                    case Keys.D5:
                        if (m_shiftDown)
                        {
                            key += "%";
                        }
                        else
                        {
                            key += "5";
                        }
                        break;

                    case Keys.D6:
                        if (m_shiftDown)
                        {
                            key += "^";
                        }
                        else
                        {
                            key += "6";
                        }
                        break;

                    case Keys.D7:
                        if (m_shiftDown)
                        {
                            key += "&";
                        }
                        else
                        {
                            key += "7";
                        }
                        break;

                    case Keys.D8:
                        if (m_shiftDown)
                        {
                            key += "*";
                        }
                        else
                        {
                            key += "8";
                        }
                        break;

                    case Keys.D9:
                        if (m_shiftDown)
                        {
                            key += "(";
                        }
                        else
                        {
                            key += "9";
                        }
                        break;


                    case Keys.Space:
                        key += " ";
                        break;

                    case Keys.OemPlus:
                        if (m_shiftDown)
                        {
                            key += "+";
                        }
                        else
                        {
                            key += "=";
                        }
                        break;

                    case Keys.OemMinus:
                        if (m_shiftDown)
                        {
                            key += "_";
                        }
                        else
                        {
                            key += "-";
                        }
                        break;

                    case Keys.OemPeriod:
                        if (m_shiftDown)
                        {
                            key += ">";
                        }
                        else
                        {
                            key += ".";
                        }
                        break;

                    case Keys.OemComma:
                        if (m_shiftDown)
                        {
                            key += "<";
                        }
                        else
                        {
                            key += ",";
                        }
                        break;

                    case Keys.A:
                    case Keys.B:
                    case Keys.C:
                    case Keys.D:
                    case Keys.E:
                    case Keys.F:
                    case Keys.G:
                    case Keys.H:
                    case Keys.I:
                    case Keys.J:
                    case Keys.K:
                    case Keys.L:
                    case Keys.M:
                    case Keys.N:
                    case Keys.O:
                    case Keys.P:
                    case Keys.Q:
                    case Keys.R:
                    case Keys.S:
                    case Keys.U:
                    case Keys.V:
                    case Keys.W:
                    case Keys.X:
                    case Keys.Y:
                    case Keys.Z:
                        if (m_shiftDown)
                        {
                            key += keyAction.m_key.ToString().ToUpper();
                        }
                        else
                        {
                            key += keyAction.m_key.ToString().ToLower();
                        }
                        break;

                    case Keys.T:
                        if (m_state.equals("FileOpen"))
                        {
                            // Open a file as read only and tail it
                            //
                            traverseDirectory(gameTime, true, true);
                        }
                        else
                        {
                            if (m_shiftDown)
                            {
                                key += keyAction.m_key.ToString().ToUpper();
                            }
                            else
                            {
                                key += keyAction.m_key.ToString().ToLower();
                            }
                        }
                        break;


                    // Do nothing as default
                    //
                    default:
                        key += "";
                        break;
                }
            //}


            if (key != "")
            {
                //Logger.logMsg("XygloXNA::processKeys() - processing key " + key);

                if (m_state.equals("FileSaveAs")) // File name
                {
                    //Logger.logMsg("Writing letter " + key);
                    m_saveFileName += key;
                }
                else if (m_state.equals("Configuration") && m_editConfigurationItem) // Configuration item
                {
                    m_editConfigurationItemValue += key;
                }
                else if (m_state.equals("FindText"))
                {
                    m_project.getSelectedBufferView().appendToSearchText(key);
                }
                else if (m_state.equals("GotoLine"))
                {
                    m_gotoLine += key;
                }
                else if (m_state.equals("TextEditing"))
                {
                    // Do we need to do some deletion or replacing?
                    //
                    if (m_project.getSelectedBufferView().gotHighlight())
                    {
                        m_project.getSelectedBufferView().replaceCurrentSelection(m_project, key);
                    }
                    else
                    {
                        m_project.getSelectedBufferView().insertText(m_project, key);
                    }
                    updateSmartHelp();
                }
            }
        }

        /// <summary>
        /// Update syntax highlighting if we need to 
        /// </summary>
        protected void updateSmartHelp()
        {
            // Update the syntax highlighting
            //
            if (m_project.getConfigurationValue("SYNTAXHIGHLIGHT").ToUpper() == "TRUE")
            {
                FileBuffer fb = m_project.getSelectedBufferView().getFileBuffer();
                int startLine = m_project.getSelectedBufferView().getBufferShowStartY();
                int endLine = m_project.getSelectedBufferView().getBufferShowStartY() + m_project.getSelectedBufferView().getBufferShowLength();

                // Limit end line as necessary
                if (endLine >= fb.getLineCount())
                {
                    endLine = Math.Max(fb.getLineCount() - 1, 0);
                }

                FilePosition startPosition = new FilePosition(0, startLine);
                FilePosition endPosition = new FilePosition(fb.getLine(endLine).Length, endLine);

                // Process immediately the current visible buffer
                //
                m_project.getSyntaxManager().generateHighlighting(fb, startPosition, endPosition, false);

                // Ensure that the syntax manager isn't processing highlights at the time of the next request.
                //
                m_project.getSyntaxManager().interruptProcessing();

                // We process all highlighting in the SmartHelpWorker thread.  Note that you have to do this
                // all in the same thread or the main GUI gets locked out.  Although it would make sense to
                // do just the on-screen bit in main thread we can minimise latency by keeping the highlight
                // thread sleep
                //
                m_smartHelpWorker.updateSyntaxHighlighting(m_project.getSyntaxManager(), m_project.getSelectedBufferView().getFileBuffer(),
                    startLine, endLine);
            }
        }


        /// <summary>
        /// Run a search on the current BufferView
        /// </summary>
        /// <returns></returns>
        protected void doSearch(GameTime gameTime)
        {
            m_state = State.Test("TextEditing");

            // Don't search for nothing
            //
            if (m_project.getSelectedBufferView().getSearchText() == "")
            {
                return;
            }

            // If we find something from cursor we're finished here
            //
            if (m_project.getSelectedBufferView().findFromCursor(false))
            {
                return;
            }

            // Now try to find from the top of the file
            //
            if (m_project.getSelectedBufferView().getCursorPosition().Y > 0)
            {
                // Try find from the top - if it finds something then let user know we've
                // wrapped around.
                //
                if (m_project.getSelectedBufferView().find(new ScreenPosition(0, 0), false))
                {
                    setTemporaryMessage("Search wrapped around end of file", 1.5f, gameTime);
                    return;
                }
            }

            setTemporaryMessage("\"" + m_project.getSelectedBufferView().getSearchText() + "\" not found", 3, gameTime);
        }


        /// <summary>
        /// Are we pressing on a number key?
        /// </summary>
        /// <returns></returns>
        protected string getNumberKey()
        {
            string key = "";

            foreach (Keys keyDown in Keyboard.GetState().GetPressedKeys())
            {
                switch (keyDown)
                {
                    case Keys.D0:
                        key = "0";
                        break;

                    case Keys.D1:
                        key = "1";
                        break;

                    case Keys.D2:
                        key = "2";
                        break;

                    case Keys.D3:
                        key = "3";
                        break;

                    case Keys.D4:
                        key = "4";
                        break;

                    case Keys.D5:
                        key = "5";
                        break;

                    case Keys.D6:
                        key = "6";
                        break;

                    case Keys.D7:
                        key = "7";
                        break;

                    case Keys.D8:
                        key = "8";
                        break;

                    case Keys.D9:
                        key = "9";
                        break;

                    default:
                        break;
                }
            }

            return key;

        }

        /// <summary>
        /// Set a temporary message until a given end time (seconds into the future)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="gameTime"></param>
        protected void setTemporaryMessage(string message, double seconds, GameTime gameTime)
        {
            m_temporaryMessage = message;

            if (seconds == 0)
            {
                seconds = 604800; // a week should be long enough to signal infinity
            }

            // Store the start and end time for this message - start time is used for
            // scrolling.
            //
            m_temporaryMessageStartTime = gameTime.TotalGameTime.TotalSeconds;
            m_temporaryMessageEndTime = m_temporaryMessageStartTime + seconds;
        }


        /// <summary>
        /// Add a new FileBuffer and a new BufferView and set this as active
        /// </summary>
        protected BufferView addNewFileBuffer(string filename = null, bool readOnly = false, bool tailFile = false)
        {
            BufferView newBV = null;
            FileBuffer newFB = (filename == null ? new FileBuffer() : new FileBuffer(filename, readOnly));

            if (filename != null)
            {
                newFB.loadFile(m_project.getSyntaxManager());
            }

            // Add the FileBuffer and keep the index for our BufferView
            //
            int fileIndex = m_project.addFileBuffer(newFB);

            // Always assign a new bufferview to the right if we have one - else default position
            //
            Vector3 newPos = Vector3.Zero;
            if (m_project.getSelectedBufferView() != null)
            {
                newPos = getFreeBufferViewPosition(m_newPosition); // use the m_newPosition for the direction
            }

            newBV = new BufferView(m_project.getFontManager(), newFB, newPos, 0, 20, fileIndex, readOnly);
            newBV.setTailing(tailFile);
            m_project.addBufferView(newBV);

            // Set the background colour
            //
            newBV.setBackgroundColour(m_project.getNewFileBufferColour());

            // Only do the following if tailing
            //
            if (!tailFile)
            {
                // We've add a new file so regenerate the model
                //
                generateTreeModel();

                // Now generate highlighting
                //
                if (m_project.getConfigurationValue("SYNTAXHIGHLIGHT").ToUpper() == "TRUE")
                {
                    //m_project.getSyntaxManager().generateAllHighlighting(newFB, true);
                    m_smartHelpWorker.updateSyntaxHighlighting(m_project.getSyntaxManager(), newFB);
                }
            }

            return newBV;
        }

        /// <summary>
        /// Find a free position around the active view
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        protected Vector3 getFreeBufferViewPosition(BufferView.ViewPosition position)
        {
            bool occupied = false;

            // Initial new pos is here from active BufferView
            //
            Vector3 newPos = m_project.getSelectedBufferView().calculateRelativePositionVector(position);
            do
            {
                occupied = false;

                foreach (BufferView cur in m_project.getBufferViews())
                {
                    if (cur.getPosition() == newPos)
                    {
                        // We get the next available slot in the same direction away from original
                        //
                        newPos = cur.calculateRelativePositionVector(position);
                        occupied = true;
                        break;
                    }
                }
            } while (occupied);

            return newPos;
        }

        /// <summary>
        /// Find a good position for a new BufferView relative to the current active position
        /// </summary>
        /// <param name="position"></param>
        protected void addBufferView(BufferView.ViewPosition position)
        {
            Vector3 newPos = getFreeBufferViewPosition(position);

            BufferView newBufferView = new BufferView(m_project.getFontManager(), m_project.getSelectedBufferView(), newPos);
            //newBufferView.m_textColour = Color.LawnGreen;
            m_project.addBufferView(newBufferView);
            setActiveBuffer(newBufferView);
        }


        /// <summary>
        /// Locate a BufferView located in a specified direction - if we find one then
        /// we set that as the active buffer view.
        /// </summary>
        /// <param name="position"></param>
        protected void detectMove(BufferView.ViewPosition position, GameTime gameTime)
        {
            // First get the position of a potential BufferView
            //
            BoundingBox searchBox = m_project.getSelectedBufferView().calculateRelativePositionBoundingBox(position);

            // Store the id of the current view
            //
            int fromView = m_project.getSelectedBufferViewId();

            // Search by index
            //
            for (int i = 0; i < m_project.getBufferViews().Count; i++)
            {
                if (m_project.getBufferViews()[i].getBoundingBox().Intersects(searchBox))
                {
                    m_project.setSelectedBufferViewId(i);
                    break;
                }
            }

            // Now set the active buffer if we need to - if not give a warning
            //
            if (fromView != m_project.getSelectedBufferViewId())
            {
                setActiveBuffer();
            }
            else
            {
                setTemporaryMessage("No BufferView.", 2.0, gameTime);
            }
        }


        /// <summary>
        /// Move the eye to a new position - store the original one so we can tell how far along the path we've moved
        /// </summary>
        /// <param name="newPosition"></param>
        protected void flyToPosition(Vector3 newPosition)
        {
            m_originalEyePosition = m_eye;
            m_newEyePosition = newPosition;
            m_changingPositionLastGameTime = TimeSpan.Zero;
            m_changingEyePosition = true;
        }

        /// <summary>
        /// Transform current eye position to an intended eye position over time.  We use the orignal eye position to enable us
        /// to accelerate and deccelerate.  We have to modify both the eye position and the target that the eye is looking at
        /// by the same amount to keep our orientation constant.
        /// </summary>
        /// <param name="delta"></param>
        protected void changeEyePosition(GameTime gameTime)
        {
            if (m_changingEyePosition)
            {
                // If more than twenty ms has elapsed
                //
                try
                {
                    // Result of any of our bounding checks
                    //

                    // Set up the flying vector for the first iteration
                    //
                    if (m_changingPositionLastGameTime == TimeSpan.Zero)
                    {
                        m_vFly = (m_newEyePosition - m_eye) / m_flySteps;

                        // Also set up the target modification vector thus
                        //
                        Vector3 tempTarget = m_newEyePosition;
                        tempTarget.Z = 0.0f;
                        m_vFlyTarget = (tempTarget - m_target) / m_flySteps;

                        //m_vFly.Normalize();
                        m_changingPositionLastGameTime = gameTime.TotalGameTime;
                    }

                    // As we close in on target ensure that we smoothly stop
                    //
                    /*
                    m_testArrived.Center = m_newEyePosition;
                    m_testArrived.Radius = 80.0f;

                    m_testArrived.Contains(ref m_eye, out m_testResult);
                    if (m_testResult == ContainmentType.Contains)
                    {
                        m_vFly *= 0.9f;
                    }
                    */

                    // Add an acceleration and decceleration component
                    //
                    float acc = 0.2f;

                    // Percentage position along way of track
                    //
                    float percTrack = (m_eye - m_originalEyePosition).Length() - (m_newEyePosition - m_originalEyePosition).Length();
                    //float deltaPos = 
                    //Logger.logMsg("Position = " + percTrack);

                    if (percTrack < 0.5f)
                    {
                        acc = percTrack; // *percTrack;
                    }
                    else
                    {
                        acc = 1.0f - (percTrack); // * percTrack);
                    }

                    acc = Math.Min(acc, 0.01f);
                    //Logger.logMsg("ACC = " + acc);
                    acc = 1.0f;


                    if (m_currentFontScale == 0.0f)
                    {
                        m_currentFontScale = m_fontScaleOriginal;
                    }
                    else if (m_currentFontScale != 1.0f)
                    {
                        if (m_fontScaleOriginal < 1.0f)
                        {
                            m_currentFontScale = m_fontScaleOriginal + ((1.0f - m_fontScaleOriginal) * percTrack);
                        }
                        else
                        {
                            m_currentFontScale = m_fontScaleOriginal - ((m_fontScaleOriginal - 1.0f) * percTrack);
                        }
                    }


                    // Perform movement of the eye by the movement vector
                    //
                    if (gameTime.TotalGameTime - m_changingPositionLastGameTime > m_movementPause)
                    {
                        m_eye += m_vFly * acc;

                        // modify target by the other vector (this is to keep our eye level constan
                        //
                        m_target.X += m_vFlyTarget.X * acc;
                        m_target.Y += m_vFlyTarget.Y * acc;
                        m_changingPositionLastGameTime = gameTime.TotalGameTime;
                        //m_view = Matrix.CreateLookAt(m_eye, Vector3.Zero, Vector3.Up);
#if DEBUG_FLYING
                        Logger.logMsg("XygloXNA::changeEyePosition() - eye is now at " + m_eye.ToString());
                        Logger.logMsg("XygloXNA::changeEyePosition() - final position is " + m_newEyePosition.ToString());
#endif
                    }

                    // Test arrival of the eye at destination position
                    //
                    m_testArrived.Center = m_newEyePosition;
                    m_testArrived.Radius = 1.0f;
                    m_testArrived.Contains(ref m_eye, out m_testResult);

                    if (m_testResult == ContainmentType.Contains)
                    {
                        m_eye = m_newEyePosition;
                        m_target.X = m_newEyePosition.X;
                        m_target.Y = m_newEyePosition.Y;
                        m_changingEyePosition = false;
                        m_currentFontScale = 1.0f;
                    }

                }
                catch (Exception e)
                {
                    Console.Write("Got timing exception " + e.Message);
                }
            }
        }

        /// <summary>
        /// Get some rays to help us work out where the user is clicking
        /// </summary>
        /// <returns></returns>
        public Ray getPickRay()
        {
            // Get the mouse state
            //
            MouseState mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

            int mouseX = mouseState.X;
            int mouseY = mouseState.Y;

            Vector3 nearsource = new Vector3((float)mouseX, (float)mouseY, m_zoomLevel);
            Vector3 farsource = new Vector3((float)mouseX, (float)mouseY, 0);

            Matrix world = Matrix.CreateScale(1, -1, 1); //Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = m_graphics.GraphicsDevice.Viewport.Unproject(nearsource, m_projection, m_viewMatrix, world);
            Vector3 farPoint = m_graphics.GraphicsDevice.Viewport.Unproject(farsource, m_projection, m_viewMatrix, world);

            //farPoint.X = nearPoint.X;
            //farPoint.Y = nearPoint.Y;

            // Create a ray from the near clip plane to the far clip plane.
            //
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            Ray pickRay = new Ray(nearPoint, direction);
            return pickRay;
        }

        /// <summary>
        /// Double click handler
        /// </summary>
        protected void handleDoubleClick()
        {
            Pair<BufferView, Pair<ScreenPosition, ScreenPosition>> testFind = m_project.testRayIntersection(getPickRay());
            BufferView bv = (BufferView)testFind.First;
            ScreenPosition fp = (ScreenPosition)testFind.Second.First;
            ScreenPosition screenRelativePosition = (ScreenPosition)testFind.Second.Second;

            // Check for validity of bv and position here
            //
            if (bv == null || fp.Y < 0 || fp.Y > bv.getFileBuffer().getLineCount() || fp.X < 0 || fp.X >= bv.getFileBuffer().getLine(fp.Y).Length) // do nothing
                return;

            if (bv.isTailing())
            {
                handleTailingDoubleClick(bv, fp, screenRelativePosition);
            }
            else
            {
                handleStandardDoubleClick(bv, fp, screenRelativePosition);
            }
        }

        /// <summary>
        /// Return a null BufferView if there is no intersection
        /// </summary>
        /// <returns></returns>
        protected Pair<BufferView, ScreenPosition> getBufferViewIntersection()
        {
            Pair<BufferView, Pair<ScreenPosition, ScreenPosition>> testFind = m_project.testRayIntersection(getPickRay());
            BufferView bv = (BufferView)testFind.First;
            ScreenPosition fp = (ScreenPosition)testFind.Second.First;
            ScreenPosition screenRelativePosition = (ScreenPosition)testFind.Second.Second;

            // Check for validity of bv and position here
            //
            if (bv == null /* || fp.Y < 0 || fp.Y > bv.getFileBuffer().getLineCount() */ || fp.X < 0 || fp.X >= bv.getBufferShowWidth()) 
            {
                bv = null;
            }

            return new Pair<BufferView, ScreenPosition>(bv, fp);
        }

        /// <summary>
        /// At this point we should have two BufferView - one selected and one we've clicked on.
        /// Now we can run a diff on the two and present some results.  Somehow.
        /// </summary>
        /// <param name="gameTime"></param>
        protected void handleDiffPick(GameTime gameTime)
        {
            Pair<BufferView, Pair<ScreenPosition, ScreenPosition>> testFind = m_project.testRayIntersection(getPickRay());

            Logger.logMsg("XygloXNA::handleDiffPick() - got diff pick request");

            // We're only really interesed in the BufferView
            //
            if (testFind.First != null)
            {
                BufferView bv1 = m_project.getSelectedBufferView();
                BufferView bv2 = testFind.First;

                if (bv1 != bv2)
                {
                    // Just set the views we're comparing and generate a diff.
                    // This is then drawn later on in this file.
                    //

                    // If the x positions are the wrong way around then convert them
                    //
                    if (bv1.getEyePosition().X > bv2.getEyePosition().X)
                    {
                        BufferView swap = bv1;
                        bv1 = bv2;
                        bv2 = swap;
                    }

                    // Set the two sides of the diff
                    //
                    m_project.setLHSDiff(bv1);
                    m_project.setRHSDiff(bv2);

#if USING_DIFFVIEW
                    DiffView diffView = new DiffView(bv1, bv2);
                    diffView.initialise(m_graphics, m_project);
                    if (diffView.process())
                    {
                        m_project.addGenericView(diffView);

                        // Show the differences and break out a new diff view
                        //
                        setActiveBuffer(diffView);
                    }
                    else
                    {
                        setTemporaryMessage("No differences found.", 3, gameTime);
                    }
#endif
                    // Set the BufferViews
                    //
                    m_differ.setBufferViews(bv1, bv2);


                    Logger.logMsg("XygloXNA::handleDiffPick() - starting diff pick process");
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();

                    // Set wait default
                    //
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

                    if (!m_differ.process())
                    {
                        setTemporaryMessage("No differences found.", 3, gameTime);

                        // Set state back to default
                        //
                        m_state = State.Test("TextEditing");
                    }
                    else
                    {
                        setTemporaryMessage("Diff selected", 1.5f, gameTime);

                        // Now set up the position of the diff previews and make sure we've
                        // pregenerated the lines
                        //
                        Vector2 leftBox = Vector2.Zero;
                        leftBox.X = (int)((m_graphics.GraphicsDevice.Viewport.Width / 2) - m_project.getFontManager().getCharWidth(FontManager.FontType.Overlay) * 20);
                        leftBox.Y = (int)(m_project.getFontManager().getLineSpacing(FontManager.FontType.Overlay) * 3);

                        Vector2 leftBoxEnd = leftBox;
                        leftBoxEnd.X += (int)(m_project.getFontManager().getCharWidth(bv1.getViewSize()) * 16);
                        leftBoxEnd.Y += (int)(m_project.getFontManager().getLineSpacing(bv1.getViewSize()) * 10);

                        Vector2 rightBox = Vector2.Zero;
                        rightBox.X = (int)((m_graphics.GraphicsDevice.Viewport.Width / 2) + m_project.getFontManager().getCharWidth(FontManager.FontType.Overlay) * 4);
                        rightBox.Y = leftBox.Y;

                        Vector2 rightBoxEnd = rightBox;
                        rightBoxEnd.X += (int)(m_project.getFontManager().getCharWidth(bv2.getViewSize()) * 16);
                        rightBoxEnd.Y += (int)(m_project.getFontManager().getLineSpacing(bv2.getViewSize()) * 10);

                        // Now generate some previews and store these positions
                        //
                        m_differ.generateDiffPreviews(leftBox, leftBoxEnd, rightBox, rightBoxEnd);

                        // Store the diff position as the left hand current cursor position - still need
                        // to check that this is valid and correct when it is expanded into the 'real' diff
                        // position.
                        //
                        m_diffPosition = m_differ.originalLhsFileToDiffPosition(m_project.getSelectedBufferView().getBufferShowStartY());

                        // Now we want to fly to the mid position between the two views - we have to assume they are
                        // next to each other for this to make sense.  Only do this if it's configured as such.
                        //
                        if (m_project.getConfigurationValue("DIFFCENTRE").ToUpper() == "TRUE")
                        {
                            Vector3 look1 = (bv1.getEyePosition() + bv2.getEyePosition()) / 2;
                            look1.Z = bv1.getEyePosition().Z * 1.8f;
                            flyToPosition(look1);
                        }
                    }

                    sw.Stop();

                    // Set wait default
                    //
                    System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.IBeam;

                    Logger.logMsg("XygloXNA::handleDiffPick() - diff pick took " + sw.ElapsedMilliseconds + " ms to run");
                }
                else
                {
                    setTemporaryMessage("Can't diff a BufferView with itself.", 3, gameTime);

                    // Set state back to default
                    //
                    m_state = State.Test("TextEditing");
                }
            }
        }

        /// <summary>
        /// Handle a single left button mouse click
        /// </summary>
        /// <param name="gameTime"></param>
        protected void handleSingleClick(GameTime gameTime)
        {
            if (m_project == null)
            {
                return;
            }

            Pair<BufferView, Pair<ScreenPosition, ScreenPosition>> testFind = m_project.testRayIntersection(getPickRay());

            // Have we got a valid intersection?
            //
            if (testFind.First != null && testFind.Second.First != null)
            {
                BufferView bv = (BufferView)testFind.First;
                ScreenPosition fp = (ScreenPosition)testFind.Second.First;

                if (m_state.equals("DiffPicker"))
                {
                    ScreenPosition newSP = bv.testCursorPosition(new FilePosition(fp));

                    // Do something with it like a highlight or something?
                    //
                }
                else
                {
                    ScreenPosition sp = bv.testCursorPosition(new FilePosition(fp));

                    if (sp.X != -1 && sp.Y != -1)
                    {
                        setActiveBuffer(bv);
                        bv.mouseCursorTo(m_shiftDown, sp);
                    }
                }
            }
        }

        /// <summary>
        /// How we handle a double click on a normal canvas when we're editing - accept the BufferView
        /// we've clicked on, the ScreenPosition (FilePosition with expanded tabs) and also the screen
        /// relative position.
        /// </summary>
        /// <param name="bv"></param>
        /// <param name="fp"></param>
        /// <param name="screenRelativePosition"></param>
        protected void handleStandardDoubleClick(BufferView bv, ScreenPosition fp, ScreenPosition screenRelativePosition)
        {
            // We need to identify a valid line
            //
            if (fp.Y >= bv.getFileBuffer().getLineCount())
                return;

            // All we need to do here is find the line, see if we're clicking in a word and
            // highlight it if so.
            //
            string line = bv.getFileBuffer().getLine(fp.Y);

            string splitChars = " \t:,(){}[]\"\'<>-";

            // Are we on a space or tab?  If not highlight the word
            //
            if (fp.X < line.Length && line[fp.X] != ' ' && line[fp.X] != '\t')
            {
                // Find first and last occurences of spaces after splitting the string effectively
                //
                int startWord = -1; // line.Substring(0, fp.X).LastIndexOf(' ') + 1;
                int endWord = -1; //line.Substring(fp.X, line.Length - fp.X).IndexOf(' ');

                // Find first occurence backwards
                //
                for (int i = fp.X; i > 0; i--)
                {
                    if (splitChars.Contains(line[i]))
                    {
                        // Step past this character as it's a delimiter
                        //
                        startWord = (i < line.Length - 1 ) ? i + 1 : line.Length - 1;
                        break;
                    }
                }

                // First first occurence forwards
                //
                for (int i = fp.X; i < line.Length; i++)
                {
                    if (splitChars.Contains(line[i]))
                    {
                        endWord = i;
                        break;
                    }
                }

                // Adjust for no space to end of line
                //
                if (endWord == -1) endWord = line.Length;
                if (startWord == -1) startWord = 0;

                // Convert file to screen positions allowing for tabs
                //
                ScreenPosition sp1 = new ScreenPosition(m_project.fileToScreen(line, startWord), fp.Y);
                ScreenPosition sp2 = new ScreenPosition(m_project.fileToScreen(line, endWord), fp.Y);
                bv.setHighlight(sp1, sp2);
            }
        }

        /// <summary>
        /// Help to activate, set cursor and center a highlighted row on the active BufferView
        /// </summary>
        /// <param name="bv"></param>
        /// <param name="sp"></param>
        protected void setHighlightAndCenter(BufferView bv, ScreenPosition sp)
        {
            string line = bv.getFileBuffer().getLine(sp.Y);

            setActiveBuffer(bv);
            bv.setCursorPosition(sp);
            bv.setBufferShowStartY(sp.Y - bv.getBufferShowLength() / 2);

            ScreenPosition sp1 = new ScreenPosition(0, sp.Y);
            ScreenPosition sp2 = new ScreenPosition(line.Length, sp.Y);
            bv.setHighlight(sp1, sp2);
        }

        /// <summary>
        /// How we handle a double click on a tailing view
        /// </summary>
        /// <param name="bv"></param>
        /// <param name="fp"></param>
        /// <param name="screenRelativePosition"></param>
        protected void handleTailingDoubleClick(BufferView bv, ScreenPosition fp, ScreenPosition screenRelativePosition)
        {
            Logger.logMsg("XygloXNA::handleTailingDoubleClick()");
            ScreenPosition testSp = bv.testCursorPosition(new FilePosition(fp));

            if (testSp.X == -1 && testSp.Y == -1)
            {
                Logger.logMsg("XygloXNA::handleTailingDoubleClick() - failed in testCursorPosition");
            }
            else
            {
                // Fetch the line indicated from the file into the line variable
                //
                string line = "";
                try
                {
                    line = bv.getFileBuffer().getLine(fp.Y);
                    Logger.logMsg("XygloXNA::handleTailingDoubleClick() - got a line = " + line);
                }
                catch (Exception)
                {
                    Logger.logMsg("XygloXNA::handleTailingDoubleClick() - couldn't fetch line " + fp.Y);
                }


                // Look for a FileBuffer indicated in this line - we look up the filename from
                // the text and seek this filename in the FileBuffers.  If we find one then we
                // zap to it
                //
                Pair<FileBuffer, ScreenPosition> found = m_project.findFileBufferFromText(line, m_modelBuilder);

                if (found.First != null)  // Found one.
                {
                    FileBuffer fb = (FileBuffer)found.First;
                    ScreenPosition sp = (ScreenPosition)found.Second;

                    // Try to find a BufferView for this FileBuffer
                    //
                    bv = m_project.getBufferView(fb.getFilepath());

                    // Adjust line by 1 - hardcode this for QtCreator for the moment
                    //
                    if (sp.Y > 0)
                    {
                        sp.Y--;
                    }

                    // If we have one then zap to it
                    //
                    if (bv != null)
                    {
                        try
                        {
                            Logger.logMsg("XygloXNA::handleTailingDoubleClick() - trying to active BufferView and zap to line");
                            setHighlightAndCenter(bv, sp);
                        }
                        catch (Exception)
                        {
                            Logger.logMsg("XygloXNA::handleTailingDoubleClick() - couldn't activate and zap to line in file");
                        }
                    }
                    else
                    {
                        // Create a new FileBuffer at calculated position and add it to the project
                        //
                        Vector3 newPos = m_project.getBestBufferViewPosition(m_project.getSelectedBufferView());
                        BufferView newBV = new BufferView(m_project.getFontManager(), fb, newPos, 0, 20, m_project.getFileIndex(fb), false);
                        int index = m_project.addBufferView(newBV);

                        int fileIndex = m_project.getFileIndex(fb);
                        newBV.setFileBufferIndex(fileIndex);

                        // Load the file
                        //
                        fb.loadFile(m_project.getSyntaxManager());

                        // Activate and centre
                        //
                        setHighlightAndCenter(newBV, sp);

                        return;
                    }
                }
                else // not found anything - look on the filesystem
                {
                    Logger.logMsg("XygloXNA::handleTailingDoubleClick() - inspecting filesystem");

                    // By default get the build directory - we'll probably want to change ths
                    //
                    string baseDir = m_project.getConfigurationValue("BUILDDIRECTORY");

                    // If we have retrieved a line to test
                    //
                    if (line != "")
                    {
                        // The getFileNamesFromText tries to find a filename and a FilePosition as well as
                        // an adjusted relative Scr

                        List<Pair<string, Pair<int, int>>> filePositionList = m_project.getFileNamesFromText(line);

                        foreach (Pair<string, Pair<int, int>> fpEntry in filePositionList)
                        {
                            // Clear the storage out first
                            //
                            m_fileSystemView.clearSearchDirectories();

                            // Do a search
                            //
                            m_fileSystemView.directorySearch(baseDir, fpEntry.First);

                            // Get results
                            //
                            List<string> rL = m_fileSystemView.getSearchDirectories();

                            if (rL.Count > 0)
                            {
                                Logger.logMsg("XygloXNA::handleTailingDoubleClick() - got " + rL.Count + " matches for file " + fpEntry.First);

                                // Set a highlight on the current BufferView
                                //
                                int xPosition = line.IndexOf(fpEntry.First);

                                // Set up the m_clickHighlight
                                //
                                m_clickHighlight.First = m_project.getSelectedBufferView();
                                m_clickHighlight.Second = new Highlight(screenRelativePosition.Y, xPosition, xPosition + fpEntry.First.Length, fpEntry.First, HighlightType.UserHighlight);

                                // Open file and zap to it
                                //
                                BufferView newBv = addNewFileBuffer(rL[0]);
                                ScreenPosition sp = new ScreenPosition(fpEntry.Second.Second, fpEntry.Second.First);
                                setHighlightAndCenter(newBv, sp);
                                break;
                            }
                        }
                    }
                }

                // Do something with the result
                //
                return;
            }
        }

        /// <summary>
        /// Have we recently got a double click - next release clears it
        /// </summary>
        bool m_gotDoubleClick = false;

        /// <summary>
        /// Have we left mouse clicked in a highlighted piece of text?
        /// </summary>
        bool m_clickInHighlight = false;

        /// <summary>
        /// Is the mouse hovering within a highlight boundary?
        /// </summary>
        /// <returns></returns>
        protected bool mouseWithinHighlight()
        {
            // Get intersection and check
            //
            Pair <BufferView, ScreenPosition> bS = getBufferViewIntersection();

            if (bS.First == m_project.getSelectedBufferView() && (bS.Second.X != -1 || bS.Second.Y != -1))
            {
                ScreenPosition sP = bS.Second;
                string line = m_project.getSelectedBufferView().getFileBuffer().getLine(bS.Second.Y);
                int maxX = Math.Max(m_project.screenToFile(line, line.Length - 1), 0);
                if (sP.X > maxX)
                {
                    sP.X = maxX;
                }
                else
                {
                    sP.X = m_project.screenToFile(line, sP.X);
                }

                if (sP.X == -1)
                {
                    Logger.logMsg("GOT -1");
                }

                ScreenPosition highlightStart = bS.First.getHighlightStart();
                ScreenPosition highlightEnd = bS.First.getHighlightEnd();
                
                // Are we within the current highlight?
                //
                return (sP >= highlightStart && sP <= highlightEnd);
            }

            return false;
        }

        /// <summary>
        /// Zoom the view out to accomodate a given set of BufferViews
        /// </summary>
        /// <param name="accmodateThese"></param>
        protected void zoomToAccomodate(BufferView accomodateThis)
        {
            // See:
            //
            // http://msdn.microsoft.com/en-us/library/bb197900(v=xnagamestudio.10).aspx
            //
            List<BufferView> accomodateThese = new List<BufferView>();
            accomodateThese.Add(m_project.getSelectedBufferView());
            accomodateThese.Add(accomodateThis);

            // Get the overall bounding box
            //
            BoundingBox containingBB = m_project.getBoundingBox(accomodateThese);

            Vector3 newEyePosition = Vector3.Zero;
            
            // Work out width and find out widest or highest part
            //
            float widthX = ( containingBB.Max.X + containingBB.Min.X ) / 2;
            float widthY = ( containingBB.Max.Y + containingBB.Min.Y ) / 2;
            float largest = Math.Max(widthX, widthY);

            // Position eye containing both bufferviews
            //
            newEyePosition.X = containingBB.Min.X + widthX;
            newEyePosition.Y = containingBB.Min.Y + widthY;
            newEyePosition.Z = largest / (float) Math.Sin(m_fov / 2);

            flyToPosition(newEyePosition);
        }

        /// <summary>
        /// Handle mouse click and double clicks and farm out the responsibility to other
        /// helper methods.
        /// </summary>
        /// <param name="gameTime"></param>
        public void checkMouse(GameTime gameTime, List<MouseAction> mouseActionList)
        {
            // Ignore an empty project for the moment
            //
            if (m_project == null)
                return;

            // If our main XNA window is inactive then ignore mouse clicks
            //
            if (IsActive == false) return;

            // If we are flying somewhere then ignore mouse clicks
            //
            if (m_changingEyePosition) return;

            foreach (MouseAction mouseAction in mouseActionList)
            {
                // Left button
                //
                if (mouseAction.m_mouse == Mouse.LeftButtonPress)
                {
#if SAM_MOUSE_TEST
                    if (sw.IsRunning)
                    {
                        sw.Stop();
                        //setTemporaryMessage("Time since last mouse click was " + sw.ElapsedMilliseconds + "ms", 2, gameTime);
                        Logger.logMsg("Time since last mouse click was " + sw.ElapsedMilliseconds + "ms");
                    }
                    else
                    {
                        sw.Reset();
                        sw.Start();
                    }
#endif
                    // Check to see if we're clicking within a highlight
                    //
                    m_clickInHighlight = mouseWithinHighlight();

                    // Get the pick ray
                    //
                    Ray pickRay = getPickRay();
                    int mouseX = (int)mouseAction.m_position.X;
                    int mouseY = (int)mouseAction.m_position.Y;

                    // Handle double clicks
                    //
                    m_lastClickPosition.X = mouseX;
                    m_lastClickPosition.Y = mouseY;
                    m_lastClickPosition.Z = 0;

                    // Double click fired
                    //
                    if ((gameTime.TotalGameTime - m_lastClickTime).TotalSeconds < 0.25f)
                    {
                        handleDoubleClick();

                        // set a double click
                        //
                        m_gotDoubleClick = true;

                        // If we return early then make sure we set m_lastMousState
                        //
                        m_lastMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

                        return;
                    }

                    m_lastClickVector = pickRay.Direction;
                    m_lastClickTime = gameTime.TotalGameTime;

                    m_lastClickEyePosition = m_eye;// m_project.getSelectedBufferView().getEyePosition(m_zoomLevel);

                    Logger.logMsg("Friender::checkMouse() - mouse clicked");
                    
                }
                else if (mouseAction.m_mouse == Mouse.LeftButtonHeld)
                {
                    if (m_clickInHighlight)
                    {
                        Logger.logMsg("Dragging in a click highlight");

                        // Define a BrazilTemporary as a placeholder for this temporary piece of text
                        //
                        //

                        // Fetch the temporary drawable by type and index
                        //
                        List<BrazilTemporary> tempList = m_temporaryDrawables.Keys.ToList().Where(item => item.getType() == BrazilTemporaryType.CopyText && item.getIndex() == 0).ToList();

                        if (tempList.Count == 0)
                        {
                            // Generate a simulcrum of the highlighted text and make a moving ghost of it.
                            //
                            //Vector3 position = m_project.getActualTestRayIntersection(getPickRay());
                            Vector3 ?position = m_project.getZeroPlaneIntersection(getPickRay());

                            // There is some interesting stuff on setData here:
                            //
                            // http://blogs.msdn.com/b/shawnhar/archive/2008/04/15/stalls-part-two-beware-of-setdata.aspx
                            //
                            //

                            if (position != null)
                            {
                                Vector3 foundPosition = (Vector3)position;

                                string text = m_project.getSelectedBufferView().getHighlightText(m_project);
                                XygloText highlightText = new XygloText(m_fontManager, m_spriteBatch, m_project.getSelectedBufferView().getTextColour(), m_lineEffect, foundPosition, m_project.getSelectedBufferView().getViewSize(), text, m_project.getSelectedBufferView().getHighlightStart().X);
                                highlightText.setPickupOffset(m_project.getSelectedBufferView().getHighlightOffset(foundPosition));
                                highlightText.setVelocity(new Vector3(1, 0, 0));

                                BrazilTemporary temp = new BrazilTemporary(BrazilTemporaryType.CopyText);
                                // Set drop dead as five seconds into the future
                                temp.setDropDead(gameTime.TotalGameTime.TotalSeconds + 5);
                                m_temporaryDrawables[temp] = highlightText;

                                // Store the source bufferview
                                //
                                m_sourceBufferView = m_project.getSelectedBufferView();
                            }
                        }
                        else
                        {
                            // Move the ghost using this code
                            //

                            //Vector3 position = m_project.getActualTestRayIntersection(getPickRay());
                            Vector3? position = m_project.getZeroPlaneIntersection(getPickRay());

                            if (position != null)
                            {
                                Vector3 foundPosition = (Vector3)position;
                                foreach (BrazilTemporary temp in m_temporaryDrawables.Keys)
                                {
                                    if (temp.getType() == BrazilTemporaryType.CopyText && temp.getIndex() == 0)
                                    {
                                        m_temporaryDrawables[temp].setPosition(foundPosition);
                                        temp.setDropDead(gameTime.TotalGameTime.TotalSeconds + 5);
                                    }
                                }
                            }

                            // If we're over a buffer view then insert a cursor where we might like to drop the text
                            //
                            Pair<BufferView, Pair<ScreenPosition, ScreenPosition>> testFind = m_project.testRayIntersection(getPickRay());
                            if (testFind.First != null && testFind.First != m_sourceBufferView)
                            {
                                // Firstly want to fit target on the screen
                                //
                                //testFind.First.isvi
                                //Logger.logMsg("WIBBLE");

                                // Dete
                                // http://stackoverflow.com/questions/10998288/xna-camera-scene-size
                                //
                                if (m_frustrum.Contains(testFind.First.getBoundingBox()) == ContainmentType.Disjoint)
                                {
                                    Logger.logMsg("Target BV outside view - accomodate it by moving the eye back and out and across");
                                    zoomToAccomodate(testFind.First);
                                }
                            }
                        }
                    }
                    else
                    if (m_gotDoubleClick)
                    {
                        // Reset cursor to current position
                        //
                        Pair <BufferView, ScreenPosition> bS = getBufferViewIntersection();

                        if (bS.First == m_project.getSelectedBufferView() && (bS.Second.X != -1 || bS.Second.Y != -1))
                        {
                            ScreenPosition sP = bS.Second;
                            string line = m_project.getSelectedBufferView().getFileBuffer().getLine(bS.Second.Y);
                            int maxX = Math.Max(m_project.screenToFile(line, line.Length - 1), 0);
                            if (sP.X > maxX)
                            {
                                sP.X = maxX;
                            }
                            else
                            {
                                sP.X = m_project.screenToFile(line, sP.X);
                            }

                            if (sP.X == -1)
                            {
                                Logger.logMsg("GOT -1");
                            }


                            // If we're outside of the current highlight then extend it - if we don't
                            // do this check then the hold after the double click cuts the newly 
                            // highlighted 'word' in half at this new point within it.
                            //
                            if (sP.Y != m_project.getSelectedBufferView().getHighlightStart().Y ||
                                sP.Y != m_project.getSelectedBufferView().getHighlightEnd().Y ||
                                sP.X < m_project.getSelectedBufferView().getHighlightStart().X ||
                                sP.X > m_project.getSelectedBufferView().getHighlightEnd().X)
                            {
                                // Move cursor to the new position and ensure that line is tab safe with the conversion
                                //
                                //Logger.logMsg("SET CURSOR POSTITION X = " + sP.X + ",Y = " + sP.Y);
                                m_project.getSelectedBufferView().setCursorPosition(sP);

                                // Sweep out a selection to the cursor
                                //
                                m_project.getSelectedBufferView().extendHighlight();
                            }
                        }

                        // If we're at the extremities of the buffer view then try scrolling it
                        //
                        // etc.
                        //
                        // Actually this does it already to a certain extent

                    }
                    else
                    {
                        // Get the pick ray
                        //
                        Ray pickRay = getPickRay();
                        int mouseX = (int)mouseAction.m_position.X;
                        int mouseY = (int)mouseAction.m_position.Y;

                        // We are dragging - work out the rate
                        //
                        double deltaX = pickRay.Position.X - m_lastClickPosition.X;
                        double deltaY = pickRay.Position.Y - m_lastClickPosition.Y;

                        // Vector and angle
                        //
                        double dragVector = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                        double dragAngle = Math.Atan2(deltaY, deltaX);

                        Vector3 nowPosition = Vector3.Zero;

                        nowPosition.X = mouseAction.m_position.X;
                        nowPosition.Y = mouseAction.m_position.Y;
                        //nowPosition.X = mouseState.X;
                        //nowPosition.Y = mouseState.Y;
                        nowPosition.Z = 0;

                        Vector3 diffPosition = (nowPosition - m_lastClickPosition);
                        diffPosition.Z = 0;

                        // Only set the cursor if we've started to drag
                        //
                        if (diffPosition.X != 0.0f && diffPosition.Y != 0.0f)
                        {
                            // Do panning - first set cursor to a hand
                            //
                            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Hand;
                        }

                        //Logger.logMsg("XygloXNA::checkMouse() - mouse dragged: X = " + diffPosition.X + ", Y = " + diffPosition.Y);

                        float multiplier = m_zoomLevel / m_zoomLevel;
                        m_eye.X = m_lastClickEyePosition.X - diffPosition.X * multiplier;
                        m_eye.Y = m_lastClickEyePosition.Y + diffPosition.Y * multiplier;

                        // If shift isn't down then we pan with the eye movement
                        //
                        if (!m_shiftDown)
                        {
                            m_target.X = m_eye.X;
                            m_target.Y = m_eye.Y;
                        }
                    }
                }
                else if (mouseAction.m_mouse == Mouse.LeftButtonRelease)
                {

                    // Clear down any temporaries
                    //
                    if (m_clickInHighlight)
                    {
                        List<BrazilTemporary> tempList = m_temporaryDrawables.Keys.ToList().Where(item => item.getType() == BrazilTemporaryType.CopyText && item.getIndex() == 0).ToList();

                        if (tempList.Count > 0)
                        {
                            foreach (BrazilTemporary temp in tempList)
                            {
                                m_temporaryDrawables[temp] = null;
                                m_temporaryDrawables.Remove(temp);
                            }
                        }
                    }

                    // Clear any double click or click in highlight flag
                    //
                    m_gotDoubleClick = false;
                    m_clickInHighlight = false;

                    if ((gameTime.TotalGameTime - m_lastClickTime).TotalSeconds < 0.15f)
                    {

                        if (m_state.equals("DiffPicker"))
                        {
                            if (m_differ != null && m_differ.hasDiffs())
                            {
                                // We have a diff pick - let's do something to the view
                                //
                                handleSingleClick(gameTime);
                            }
                            else
                            {
                                // Generate a diff pick
                                //
                                handleDiffPick(gameTime);
                            }
                        }
                        else
                        {
                            handleSingleClick(gameTime);
                        }
                    }
                    else // we've done a long click and release we're dragging - on the release handle the drag result
                    {
                        // At this point test to see which bufferview centre we're nearest and if we're near a
                        // different one then switch to that.
                        //
                        BufferView newView = m_project.testNearBufferView(m_eye);

                        if (newView != null)
                        {
                            //Logger.logMsg("XygloXNA::checkMouse() - switching to new buffer view");
                            setActiveBuffer(newView);
                        }
                        else
                        {
                            // Flyback to our existing view
                            //
                            //m_eye = m_lastClickEyePosition;
                            //m_target.X = m_lastClickEyePosition.X;
                            //m_target.Y = m_lastClickEyePosition.Y;
                            flyToPosition(m_lastClickEyePosition);
                        }

                        // Set default cursor back
                        //
                        System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.IBeam;
                    }
                }
                
                // Mouse scrollwheel
                //
                if (m_lastMouseWheelValue != mouseAction.m_scrollWheel)
                {
                    Logger.logMsg("XygloXNA::checkMouse() - mouse wheel value now = " + mouseAction.m_scrollWheel);

                    // If shift down then scroll current view - otherwise zoom in/out
                    //
                    if (m_shiftDown)
                    {
                        int linesDown = -(int)((m_lastMouseWheelValue - mouseAction.m_scrollWheel) / 120.0f);

                        if (linesDown < 0)
                        {
                            for (int i = 0; i < -linesDown; i++)
                            {
                                m_project.getSelectedBufferView().moveCursorDown(false, m_shiftDown);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < linesDown; i++)
                            {
                                m_project.getSelectedBufferView().moveCursorUp(m_project, false, m_shiftDown);
                            }
                        }
                    }
                    else
                    {
                        float newZoomLevel = m_zoomLevel + (m_zoomStep * ((m_lastMouseWheelValue - mouseAction.m_scrollWheel) / 120.0f));
                        setZoomLevel(newZoomLevel);
                    }

                    m_lastMouseWheelValue = mouseAction.m_scrollWheel;
                }
                

                // Check for the release of a sizing move
                //
                if (mouseAction.m_mouse == Mouse.LeftButtonRelease && m_isResizing)
                {
                    m_isResizing = false;
                }

                // Right mouse press
                //
                if (mouseAction.m_mouse == Mouse.RightButtonPress)
                {
                    StateAction sA = new StateAction(m_state, Mouse.RightButtonPress);

                    // scan components that could match
                    //
                    foreach (Component component in m_componentList)
                    {
                        string name = component.getStateActions().First().getState().m_name;
                        if (!m_state.equals(component.getStateActions().First().getState().m_name) || (!component.isDestroyed() && !component.isHiding()))
                            continue;

                        //Mouse mouse = (MouseAction)(component.getStateActions().First().getActions().First());
                        Logger.logMsg("Got a matching component " + component.getName());
                        Mouse mouse = ((MouseAction)component.getStateActions().First().getActions().First()).m_mouse;

                        if (mouse == Mouse.RightButtonPress)
                        {
                            // Reset cursor to current position
                            //
                            Pair<BufferView, ScreenPosition> bS = getBufferViewIntersection();

                            if (bS.First == m_project.getSelectedBufferView() && (bS.Second.X != -1 || bS.Second.Y != -1))
                            {
                                ScreenPosition sP = bS.Second;
                                // Remove the indents for these purposes
                                //
                                sP.X -= bS.First.getBufferShowStartX();
                                sP.Y -= bS.First.getBufferShowStartY();

                                // If we're outside of the current highlight then extend it - if we don't
                                // do this check then the hold after the double click cuts the newly 
                                // highlighted 'word' in half at this new point within it.
                                //
                                //if (sP.Y != m_project.getSelectedBufferView().getHighlightStart().Y ||
                                    //sP.Y != m_project.getSelectedBufferView().getHighlightEnd().Y ||
                                    //sP.X < m_project.getSelectedBufferView().getHighlightStart().X ||
                                    //sP.X > m_project.getSelectedBufferView().getHighlightEnd().X)
                                //{
                                    m_lastClickWorldPosition = bS.First.getPosition();
                                    m_lastClickCursorOffsetPosition = new Vector2(sP.X, sP.Y);

                                    // Make it appear
                                    //
                                    component.setDestroyed(false);
                                    component.setHiding(false);

                                    // Update the click position if this is already drawn but hiding
                                    //
                                    if (m_drawableComponents.ContainsKey(component))
                                    {
                                        XygloMenu menu = (XygloMenu)m_drawableComponents[component];
                                        menu.setPosition(m_lastClickWorldPosition);
                                        menu.setOffset(sP);
                                        menu.buildBuffers(m_graphics.GraphicsDevice);
                                    }
                                //}
                            }
                        }
                    }
                }
                else if (mouseAction.m_mouse == Mouse.RightButtonRelease)
                {
                    List<Component> destroyList = new List<Component>();

                    // scan components that could match
                    //
                    foreach (Component component in m_componentList)
                    {
                        string name = component.getStateActions().First().getState().m_name;
                        if (!m_state.equals(component.getStateActions().First().getState().m_name))
                            continue;

                        // Make it disappear
                        //
                        component.setHiding(true);
                    }

                    //m_lastClickPosition.X = mouseAction.m_position.X;
                    //m_lastClickPosition.Y = mouseAction.m_position.Y;

                }

                // Store the last mouse state
                //
                m_lastMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            }
        }

        /// <summary>
        /// The XNA Draw override used to draw the game
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Increment the frame counter
            //
            m_frameCounter++;

            // Draw onto the Bloom component
            //
            m_bloom.BeginDraw();

            // Call setup for the projection matrix and frustrum etc.
            //
            setupDrawWorld(gameTime);

            // Are we drawing Friendlier - we cheat a bit here
            if (m_project != null)
            {
                drawFriendlier(gameTime);
            }

            // Now draw any Xyglo components
            //
            drawXyglo(gameTime);

            base.Draw(gameTime);
        }

        /// <summary>
        /// Setup the world view and projection matrices
        /// </summary>
        /// <param name="gameTime"></param>
        protected void setupDrawWorld(GameTime gameTime)
        {
            // Set background colour
            //
            m_graphics.GraphicsDevice.Clear(Color.Black);

            // If we are resizing then do nothing
            //
            if (m_isResizing)
            {
                base.Draw(gameTime);
                return;
            }

            // If spinning then spin around current position based on time.
            //
            if (m_spinning)
            {
                float angle = (float)gameTime.TotalGameTime.TotalSeconds;
                m_eye.X = (float)Math.Cos(angle * .5f) * 12f;
                m_eye.Z = (float)Math.Sin(angle * .5f) * 12f;
            }

            // Duplicate of what we have in the Initialize()
            //
            m_viewMatrix = Matrix.CreateLookAt(m_eye, m_target, Vector3.Up);
            m_projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * Matrix.CreatePerspectiveFieldOfView(m_fov, GraphicsDevice.Viewport.AspectRatio, 0.1f, 10000f);

            // Generate frustrum
            //
            if (m_frustrum == null)
            {
                m_frustrum = new BoundingFrustum(m_viewMatrix * m_projection);
            }
            else
            {
                // You can also update frustrum matrix like this
                //
                m_frustrum.Matrix = m_viewMatrix * m_projection;
            }

            m_basicEffect.View = m_viewMatrix;
            m_basicEffect.Projection = m_projection;
            m_basicEffect.World = Matrix.CreateScale(1, -1, 1);

            m_lineEffect.View = m_viewMatrix;
            m_lineEffect.Projection = m_projection;
            m_lineEffect.World = Matrix.CreateScale(1, -1, 1);
        }


        /// <summary>
        /// Draw the Xyglo Components
        /// </summary>
        /// <param name="gameTime"></param>
        protected void drawXyglo(GameTime gameTime)
        {
            // Create/draw the components - note that we have two components called the same in different
            // areas of the framework - we should disambiguate to make sure this distinction between
            // API levels is clear.
            //
            // This loop generates the XNA level component to match the framework/Brazil level component
            // only if it doesn't already exist in the m_drawableComponents dictionary. If it does already
            // exist it just calls redraw on it.
            //

            //m_spriteBatch.Begin();
            //m_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, m_basicEffect);

            foreach (Component component in m_componentList)
            {
                string compState = "";
                if (component.getStateActions().Count > 0)
                    compState = component.getStateActions().First().getState().m_name;

                // Check that this component should be showing for this State and that's it's not been destroyed
                //
                if ((!component.getStates().Contains(m_state) && !m_state.equals(compState)) || component.isDestroyed())
                    continue;

                // Has this component already been added to the drawableComponent dictionary?  If it hasn't then
                // we haven't drawn it yet.
                //
                if (!m_drawableComponents.ContainsKey(component))
                {
                    // If not then is it a drawable type? 
                    //
                    if (component.GetType() == typeof(Xyglo.Brazil.BrazilFlyingBlock))
                    {
                        // Found a FlyingBlock - initialise it and add it to the dictionary
                        //
                        BrazilFlyingBlock fb = (Xyglo.Brazil.BrazilFlyingBlock)component;
                        XygloFlyingBlock drawBlock = new XygloFlyingBlock(XygloConvert.getColour(fb.getColour()), m_lineEffect, fb.getPosition(), fb.getSize());
                        drawBlock.setVelocity(XygloConvert.getVector3(fb.getVelocity()));

                        // Naming is useful for tracking these blocks
                        drawBlock.setName(fb.getName());

                        // Set any rotation amount
                        drawBlock.setRotation(fb.getInitialAngle());

                        // Initial build and draw
                        //
                        drawBlock.buildBuffers(m_graphics.GraphicsDevice);
                        drawBlock.draw(m_graphics.GraphicsDevice);

                        // Push to dictionary
                        //
                        m_drawableComponents[component] = drawBlock;
                        
                    } else if (component.GetType() == typeof(Xyglo.Brazil.BrazilInterloper))
                    {
                        BrazilInterloper il = (Xyglo.Brazil.BrazilInterloper)component;
#if ATTEMPT_ONE
                        XygloSphere drawSphere = new XygloSphere(XygloConvert.getColour(il.getColour()), m_lineEffect, il.getPosition(), 10.0f);
                        drawSphere.setRotation(il.getRotation());
#else
                        XygloComponentGroup group = new XygloComponentGroup(m_lineEffect, Vector3.Zero);
                        XygloFlyingBlock drawBlock = new XygloFlyingBlock(XygloConvert.getColour(il.getColour()), m_lineEffect, il.getPosition(), il.getSize());
                        group.addComponent(drawBlock);

                        // Set the name of the component group from the interloper
                        //
                        group.setName(il.getName());

                        XygloSphere drawSphere = new XygloSphere(XygloConvert.getColour(il.getColour()), m_lineEffect, il.getPosition(), il.getSize().X);
                        drawSphere.setRotation(il.getRotation());
                        group.addComponentRelative(drawSphere, new Vector3(0, - (float)il.getSize().X, 0));

                        group.buildBuffers(m_graphics.GraphicsDevice);
                        group.draw(m_graphics.GraphicsDevice);

                        //group.setVelocity(new Vector3(0.01f, 0, 0));

                        group.setVelocity(XygloConvert.getVector3(il.getVelocity()));
                        m_drawableComponents[component] = group;
#endif
                    }
                    else if (component.GetType() == typeof(Xyglo.Brazil.BrazilBannerText))
                    {
                        BrazilBannerText bt = (Xyglo.Brazil.BrazilBannerText)component;

                        // The helper method does all the hard work in getting this position
                        //
                        Vector3 position = XygloConvert.getTextPosition(bt, m_fontManager, m_graphics.GraphicsDevice.Viewport.Width, m_graphics.GraphicsDevice.Viewport.Height);
                        XygloBannerText bannerText = new XygloBannerText(m_overlaySpriteBatch, m_fontManager.getOverlayFont(), XygloConvert.getColour(bt.getColour()), position, bt.getSize(), bt.getText());
                        bannerText.draw(m_graphics.GraphicsDevice);
                    }
                    else if (component.GetType() == typeof(Xyglo.Brazil.BrazilHud))
                    {
                        BrazilHud bh = (Xyglo.Brazil.BrazilHud)component;
                        Vector3 position = XygloConvert.getVector3(bh.getPosition());

                        string fpsText = "FPS = " + m_frameRate;
                        XygloBannerText bannerText = new XygloBannerText(m_overlaySpriteBatch, m_fontManager.getOverlayFont(), XygloConvert.getColour(bh.getColour()), position, bh.getSize(), fpsText);
                        bannerText.draw(m_graphics.GraphicsDevice);

                        if (m_interloper != null)
                        {
                            // Interloper position
                            //
                            Vector3 ipPos = m_drawableComponents[m_interloper].getPosition();
                            string ipText = "Interloper Position X = " + ipPos.X + ", Y = " + ipPos.Y + ", Z = " + ipPos.Z;
                            XygloBannerText ipBanner = new XygloBannerText(m_overlaySpriteBatch, m_fontManager.getOverlayFont(), XygloConvert.getColour(BrazilColour.Blue), new Vector3(0, m_fontManager.getOverlayFont().LineSpacing, 0), 1.0f, ipText);
                            ipBanner.draw(m_graphics.GraphicsDevice);

                            // Interloper score
                            //
                            string ipScore = "Score = " + m_interloper.getScore();
                            XygloBannerText ipScoreText = new XygloBannerText(m_overlaySpriteBatch, m_fontManager.getOverlayFont(), XygloConvert.getColour(BrazilColour.Green), new Vector3(0, m_fontManager.getOverlayFont().LineSpacing * 2, 0), 1.0f, ipScore);
                            ipScoreText.draw(m_graphics.GraphicsDevice);

                            string ipLives = "Lives = " + m_world.getLives();
                            XygloBannerText ipLivesText = new XygloBannerText(m_overlaySpriteBatch, m_fontManager.getOverlayFont(), XygloConvert.getColour(BrazilColour.Green), new Vector3(0, m_fontManager.getOverlayFont().LineSpacing * 3, 0), 1.0f, ipLives);
                            ipLivesText.draw(m_graphics.GraphicsDevice);

                        }

                    } else if (component.GetType() == typeof(Xyglo.Brazil.BrazilGoody))
                    {
                        //Logger.logMsg("Draw Goody for the first time");
                        BrazilGoody bg = (BrazilGoody)component;

                        if (bg.m_type == BrazilGoodyType.Coin)
                        {
                            // Build a coin
                            //
                            XygloCoin coin = new XygloCoin(Color.Yellow, m_lineEffect, XygloConvert.getVector3(bg.getPosition()), bg.getSize().X);
                            coin.setRotation(bg.getRotation());
                            coin.buildBuffers(m_graphics.GraphicsDevice);
                            coin.draw(m_graphics.GraphicsDevice);

                            // And store in drawable component array
                            //
                            m_drawableComponents[component] = coin;
                        }
                        else
                        {
                            throw new XygloException("Update", "Unsupported Goody Type");
                        }


                    } else if (component.GetType() == typeof(Xyglo.Brazil.BrazilBaddy))
                    {
                        Logger.logMsg("Draw Baddy for the first time");
                    } else if (component.GetType() == typeof(Xyglo.Brazil.BrazilFinishBlock))
                    {
                        //Logger.logMsg("Draw Finish Block for the first time");
                    }
                    else if (component.GetType() == typeof(Xyglo.Brazil.BrazilMenu))
                    {
                        BrazilMenu bMenu = (BrazilMenu)component;

                        // Line effect or Basic effect here?
                        //
                        XygloMenu menu = new XygloMenu(m_fontManager, m_spriteBatch,  Color.DarkGray, m_lineEffect, m_lastClickWorldPosition, m_lastClickCursorOffsetPosition, m_project.getSelectedBufferView().getViewSize());

                        foreach (BrazilMenuOption item in bMenu.getMenuOptions().Keys)
                        {
                            menu.addOption(item.m_optionName);
                        }

                        // Build the buffers and draw
                        //
                        menu.buildBuffers(m_graphics.GraphicsDevice);
                        menu.draw(m_graphics.GraphicsDevice);
                        m_drawableComponents[component] = menu;
                    }
                }
                else
                {
                    // If a component is currently hiding then update its position accordingly
                    //
                    if (!component.isHiding())
                    {
                        m_drawableComponents[component].draw(m_graphics.GraphicsDevice);
                    }
                    /*
                    else
                    {
                        //m_drawableComponents[component].setPosition(m_lastClickPosition);
                        //if (component.GetType() == typeof(Xyglo.Brazil.BrazilMenu))
                        //{
                            //XygloMenu menu = (XygloMenu)m_drawableComponents[component];
                            //menu.setPosition(m_lastClickWorldPosition);
                            //menu.buildBuffers(m_graphics.GraphicsDevice);
                        //}
                    }*/
                    
                }
            }

            //m_spriteBatch.End();

            

            // Now we can draw any temporary drawables:
            // List to remove
            //
            List<BrazilTemporary> deleteTemps = new List<BrazilTemporary>();

            foreach (BrazilTemporary temporary in m_temporaryDrawables.Keys)
            {
                // Rebuild any buffers and draw
                //
                m_temporaryDrawables[temporary].buildBuffers(m_graphics.GraphicsDevice);
                m_temporaryDrawables[temporary].draw(m_graphics.GraphicsDevice);

                // Drop any temporaries that have exceeded their lifespan
                if (temporary.getDropDead() < gameTime.TotalGameTime.TotalSeconds)
                    deleteTemps.Add(temporary);
            }

            // Remove items marked for deletion
            //
            foreach (BrazilTemporary temporary in deleteTemps)
            {
                foreach (BrazilTemporary testTemp in m_temporaryDrawables.Keys)
                {
                    if (testTemp == temporary)
                    {
                        m_temporaryDrawables[testTemp] = null;
                        m_temporaryDrawables.Remove(testTemp);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Draw specific code for Friendlier special case
        /// </summary>
        /// <param name="gameTime"></param>
        protected void drawFriendlier(GameTime gameTime)
        {
            // If we're not licenced then render this
            //
            if (!m_project.getLicenced())
            {
                if (gameTime.TotalGameTime.TotalSeconds > m_nextLicenceMessage)
                {
                    if (m_flipFlop)
                    {
                        setTemporaryMessage("Friendlier demo period has expired.", 3, gameTime);
                    }
                    else
                    {
                        setTemporaryMessage("Please see www.xyglo.com for licencing details.", 3, gameTime);
                    }

                    m_flipFlop = !m_flipFlop;
                    m_nextLicenceMessage = gameTime.TotalGameTime.TotalSeconds + 5;
                }
                //renderTextScroller();
            }
            else
            {
                // Set the welcome message once
                //
                if (m_flipFlop)
                {
                    setTemporaryMessage(VersionInformation.getProductName() + " " + VersionInformation.getProductVersion(), 3, gameTime);
                    m_flipFlop = false;
                }
            }

            // In the manage project mode we zoom off into the distance
            //
            if (m_state.equals("ManageProject"))
            {
                m_drawingHelper.drawManageProject(m_overlaySpriteBatch, gameTime, m_modelBuilder, m_graphics, m_configPosition, out m_textScreenLength);
                base.Draw(gameTime);
                return;
            }

            // Here we need to vary the parameters to the SpriteBatch - to the BasicEffect and also the font size.
            // For large fonts we need to be able to downscale them effectively so that they will still look good
            // at higher reoslutions.
            //
            m_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, m_basicEffect);

            // Draw all the BufferViews for all remaining modes
            //
            for (int i = 0; i < m_project.getBufferViews().Count; i++)
            {
                if (m_differ != null && m_differ.hasDiffs() &&
                    (m_differ.getSourceBufferViewLhs() == m_project.getBufferViews()[i] ||
                        m_differ.getSourceBufferViewRhs() == m_project.getBufferViews()[i]))
                {
                    drawDiffBuffer(m_project.getBufferViews()[i], gameTime);
                }
                else
                {
                    // We have to invert the BoundingBox along the Y axis to ensure that
                    // it matches with the frustrum we're culling against.
                    //
                    BoundingBox bb = m_project.getBufferViews()[i].getBoundingBox();
                    bb.Min.Y = -bb.Min.Y;
                    bb.Max.Y = -bb.Max.Y;

                    // We only do frustrum culling for BufferViews for the moment
                    // - intersects might be too grabby but Disjoint didn't appear 
                    // to be grabby enough.
                    //
                    //if (m_frustrum.Contains(bb) != ContainmentType.Disjoint)
                    if (m_frustrum.Intersects(bb))
                    {
                        m_drawingHelper.drawFileBuffer(m_spriteBatch, m_project.getBufferViews()[i], gameTime, m_state, m_buildStdOutView, m_buildStdErrView, m_zoomLevel, m_currentFontScale);
                    }

                    // Draw a background square for all buffer views if they are coloured
                    //
                    if (m_project.getViewMode() == Project.ViewMode.Coloured)
                    {
                        m_drawingHelper.renderQuad(m_project.getBufferViews()[i].getTopLeft(), m_project.getBufferViews()[i].getBottomRight(), m_project.getBufferViews()[i].getBackgroundColour(), m_spriteBatch);
                    }
                }
            }

            // We only draw the scrollbar on the active view in the right mode
            //
            if (m_state.equals("TextEditing"))
            {
                drawScrollbar(m_project.getSelectedBufferView());
            }

            // Cursor and cursor highlight
            //
            if (m_state.equals("TextEditing"))
            {
                // Stop and use a different spritebatch for the highlighting and cursor
                //
                m_spriteBatch.End();
                m_spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, m_basicEffect);

                drawCursor(gameTime, m_spriteBatch);
                m_drawingHelper.drawHighlight(gameTime, m_spriteBatch);
            }

            m_spriteBatch.End();

            // Draw our generic views
            //
            foreach (XygloView view in m_project.getGenericViews())
            {
                view.draw(m_project, m_state, gameTime, m_spriteBatch, m_basicEffect);
            }

            // If we're choosing a file then
            //
            if (m_state.equals("FileSaveAs") || m_state.equals("FileOpen") || m_state.equals("PositionScreenOpen") || m_state.equals("PositionScreenNew") || m_state.equals("PositionScreenCopy"))
            {
                drawDirectoryChooser(gameTime);

            }
            else if (m_state.equals("Help"))
            {
                // Get the text screen length back from the drawing method
                //
                m_textScreenLength = m_drawingHelper.drawHelpScreen(m_overlaySpriteBatch, gameTime, m_graphics, m_textScreenPositionY);
            }
            else if (m_state.equals("Information"))
            {
                m_drawingHelper.drawInformationScreen(m_overlaySpriteBatch, gameTime, m_graphics, out m_textScreenLength);
            }
            else if (m_state.equals("Configuration"))
            {
                drawConfigurationScreen(gameTime);
            }
            else
            {
                // http://forums.create.msdn.com/forums/p/61995/381650.aspx
                //
                m_overlaySpriteBatch.Begin();

                // Draw the Overlay HUD
                //
                m_drawingHelper.drawOverlay(m_overlaySpriteBatch, gameTime, m_graphics, m_state, m_gotoLine, m_shiftDown, m_ctrlDown, m_altDown,
                                            m_eye, m_temporaryMessage, m_temporaryMessageStartTime, m_temporaryMessageEndTime);

                // Draw map of BufferViews - want to get rid of this way of doing things and
                // move it to the XnaDrawableOverview way.
                //
                m_drawingHelper.drawBufferViewMap(gameTime, m_overlaySpriteBatch);
                m_overlaySpriteBatch.End();

                // Draw overview of the all the XnaDrawables - compress all into a list and bung
                // over to work out the preview.
                //
                List<XygloXnaDrawable> overviewList = m_drawableComponents.Values.ToList();
                overviewList.AddRange(m_temporaryDrawables.Values.ToList());
                m_drawingHelper.drawXnaDrawableOverview(m_graphics.GraphicsDevice, gameTime, overviewList);


                // Draw any differ overlay
                //
                m_pannerSpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone /*, m_pannerEffect */ );

                // Draw the differ
                //
                drawDiffer(gameTime, m_pannerSpriteBatch);

                // Draw system load
                //
                drawSystemLoad(gameTime, m_pannerSpriteBatch);

                m_pannerSpriteBatch.End();
            }

            // Draw the textures for generic views
            //
            //foreach (XygloView view in m_project.getGenericViews())
            //{
            //view.drawTextures(m_basicEffect);
            //}

            // Draw a welcome banner
            //
            if (m_drawingHelper.getBannerStartTime() != -1 && m_project.getViewMode() != Project.ViewMode.Formal)
            {
                m_drawingHelper.drawBanner(m_spriteBatch, gameTime, m_basicEffect, m_splashScreen);
            }

            // Any Kinect information to share
            //
            drawKinectInformation();
        }


        protected void drawKinectInformation()
        {
#if GOT_KINECT
            if (m_kinectManager.gotUser())
            {
                string skeletonPosition = m_kinectManager.getSkeletonDetails();

                m_overlaySpriteBatch.Begin();

                // hardcode the font size to 1.0f so it looks nice
                //
                m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(), skeletonPosition, new Vector2(50.0f, 50.0f), Color.White, 0, Vector2.Zero, 1.0f, 0, 0);
                //m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(), eyePosition, new Vector2(0.0f, 0.0f), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
                //m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(), modeString, new Vector2((int)modeStringXPos, 0.0f), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
                //m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(), positionString, new Vector2((int)positionStringXPos, (int)yPos), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
                //m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(), filePercentString, new Vector2((int)filePercentStringXPos, (int)yPos), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
                m_overlaySpriteBatch.End();
            }
#endif // GOT_KINECT
        }


        /// <summary>
        /// Draw the system CPU load and memory usage next to the FileBuffer
        /// </summary>
        /// <param name="gameTime"></param>
        protected void drawSystemLoad(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 startPosition = Vector2.Zero;
            int linesHigh = 6;

            // Bufferview
            BufferView bv = m_project.getSelectedBufferView();

            startPosition.X += m_graphics.GraphicsDevice.Viewport.Width - m_project.getFontManager().getCharWidth(FontManager.FontType.Overlay) * 3;
            startPosition.Y += (m_graphics.GraphicsDevice.Viewport.Height / 2) - m_project.getFontManager().getLineSpacing(FontManager.FontType.Overlay) * linesHigh / 2;

            float height = m_project.getFontManager().getLineSpacing(FontManager.FontType.Overlay) * linesHigh;
            float width = m_project.getFontManager().getCharWidth(FontManager.FontType.Overlay) / 2;

            // Only fetch some new samples when this timespan has elapsed
            //
            TimeSpan mySpan = gameTime.TotalGameTime;

            if (mySpan - m_lastSystemFetch > m_systemFetchSpan)
            {
                if (m_counterWorker.m_cpuCounter != null && m_counterWorker.m_memCounter != null)
                {
                    CounterSample newCS = m_counterWorker.getCpuSample();
                    CounterSample newMem = m_counterWorker.getMemorySample();

                    // Calculate the percentages
                    //
                    m_systemLoad = CounterSample.Calculate(m_lastCPUSample, newCS);
                    m_memoryAvailable = CounterSample.Calculate(m_lastMemSample, newMem);

                    // Store the last samples
                    //
                    m_lastCPUSample = newCS;
                    m_lastMemSample = newMem;
                }

                m_lastSystemFetch = mySpan;

#if SYTEM_DEBUG
                Logger.logMsg("XygloXNA::drawSystemLoad() - load is now " + m_systemLoad);
                Logger.logMsg("XygloXNA::drawSystemLoad() - memory is now " + m_memoryAvailable);
                Logger.logMsg("XygloXNA::drawSystemLoad() - physical memory available is " + m_physicalMemory);
#endif
            }

            //m_pannerSpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone /*, m_pannerEffect */ );

            // Draw background for CPU counter
            //
            Vector2 p1 = startPosition;
            Vector2 p2 = startPosition;

            p1.Y += height;
            p1.X += 1;
            m_drawingHelper.drawBox(spriteBatch, p1, p2, Color.DarkGray, 0.8f);

            // Draw CPU load over the top
            //
            p1 = startPosition;
            p2 = startPosition;

            p1.Y += height;
            p2.Y += height - (m_systemLoad * height / 100.0f);
            p1.X += 1;

            m_drawingHelper.drawBox(spriteBatch, p1, p2, Color.DarkGreen, 0.8f);

            // Draw background for Memory counter
            //
            startPosition.X += m_project.getFontManager().getCharWidth(FontManager.FontType.Overlay) * 0.5f;
            p1 = startPosition;
            p2 = startPosition;

            p1.Y += height;
            p1.X += 1;

            m_drawingHelper.drawBox(spriteBatch, p1, p2, Color.DarkGray, 0.8f);

            // Draw Memory over the top
            //
            p1 = startPosition;
            p2 = startPosition;

            p1.Y += height;
            p2.Y += height - (height * m_memoryAvailable / m_physicalMemory);
            p1.X += 1;

            m_drawingHelper.drawBox(spriteBatch, p1, p2, Color.DarkOrange, 0.8f);
            //m_pannerSpriteBatch.End();
        }

        /// <summary>
        /// Draw the differ - it's two mini document overviews and we provide an overlay so that
        /// we know what position in the diff we're currently looking at.
        /// </summary>
        /// <param name="v"></param>
        protected void drawDiffer(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Don't draw the cursor if we're not the active window or if we're confirming 
            // something on the screen.
            //
            if (m_differ == null || m_state .notEquals("DiffPicker") || m_differ.hasDiffs() == false)
            {
                return;
            }

            Color myColour = Color.White;

            m_drawingHelper.drawBox(spriteBatch, m_differ.getLeftBox(), m_differ.getLeftBoxEnd(), myColour, 0.5f);
            m_drawingHelper.drawBox(spriteBatch, m_differ.getRightBox(), m_differ.getRightBoxEnd(), myColour, 0.5f);

            // Modify alpha according to the type of the line
            //
            float alpha = 1.0f;

            // Draw LHS preview
            //
            foreach (DiffPreview dp in m_differ.getLhsDiffPreview())
            {
                if (dp.m_colour == m_differ.m_unchangedColour)
                {
                    alpha = 0.5f;
                }
                else
                {
                    alpha = 0.8f;
                }

                m_drawingHelper.drawLine(spriteBatch, dp.m_startPos, dp.m_endPos, dp.m_colour, alpha);
            }

            // Draw RHS preview
            //
            foreach (DiffPreview dp in m_differ.getRhsDiffPreview())
            {
                if (dp.m_colour == m_differ.m_unchangedColour)
                {
                    alpha = 0.5f;
                }
                else
                {
                    alpha = 0.8f;
                }

                m_drawingHelper.drawLine(spriteBatch, dp.m_startPos, dp.m_endPos, dp.m_colour, alpha);
            }

            // Now we want to render a position viewer box overlay
            //
            float startY = Math.Min(m_differ.getLeftBox().Y + m_differ.getYMargin(), m_differ.getRightBox().Y + m_differ.getYMargin());
            float endY = Math.Min(m_differ.getLeftBoxEnd().Y - m_differ.getYMargin(), m_differ.getRightBoxEnd().Y - m_differ.getYMargin());

            double diffPercent = ((double)m_diffPosition) / ((double)m_differ.getMaxDiffLength());
            double height = ((double)m_project.getSelectedBufferView().getBufferShowLength()) / ((double)m_differ.getMaxDiffLength());

            Vector2 topLeft = new Vector2(m_differ.getLeftBox().X - 10.0f, startY + ((endY - startY) * ((float)diffPercent)));
            Vector2 topRight = new Vector2(m_differ.getRightBoxEnd().X + 10.0f, startY + ((endY - startY) * ((float)diffPercent)));
            Vector2 bottomRight = topRight;
            bottomRight.Y += Math.Max(((float)height * (endY - startY)), 3.0f);

            // Now render the quad
            //
            m_drawingHelper.drawQuad(spriteBatch, topLeft, bottomRight, Color.LightYellow, 0.3f);
        }

        /// <summary>
        /// Draw a cursor and make it blink in position on a FileBuffer
        /// </summary>
        /// <param name="v"></param>
        protected void drawCursor(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Don't draw the cursor if we're not the active window or if we're confirming 
            // something on the screen.
            //
            if (!this.IsActive || m_confirmState.notEquals("None") || m_state.equals("FindText") || m_state.equals("GotoLine"))
            {
                return;
            }

            // No cursor for tailing BufferViews
            //
            if (!m_project.getSelectedBufferView().isTailing())
            {
                double dTS = gameTime.TotalGameTime.TotalSeconds;
                int blinkRate = 4;

                // Test for when we're showing this
                //
                if (Convert.ToInt32(dTS * blinkRate) % 2 != 0)
                {
                    return;
                }

                // Blinks rate
                //
                Vector3 v1 = m_project.getSelectedBufferView().getCursorCoordinates();
                v1.Y += m_project.getSelectedBufferView().getLineSpacing();

                Vector3 v2 = m_project.getSelectedBufferView().getCursorCoordinates();
                v2.X += 1;

                m_drawingHelper.renderQuad(v1, v2, m_project.getSelectedBufferView().getHighlightColor(), spriteBatch);
            }
            // Draw any temporary highlight
            //
            if (m_clickHighlight.First != null &&
                ((BufferView)m_clickHighlight.First) == m_project.getSelectedBufferView())
            {
                Highlight h = (Highlight)m_clickHighlight.Second;
                Vector3 h1 = m_project.getSelectedBufferView().getSpaceCoordinates(h.m_startHighlight.asScreenPosition());
                Vector3 h2 = m_project.getSelectedBufferView().getSpaceCoordinates(h.m_endHighlight.asScreenPosition());

                // Add some height here so we can see the highlight
                //
                h2.Y += m_project.getFontManager().getLineSpacing(m_project.getSelectedBufferView().getViewSize());

                m_drawingHelper.renderQuad(h1, h2, h.getColour(), spriteBatch);
            }
        }

        /// <summary>
        /// This is a list of directories and files based on the current position of the FileSystemView
        /// </summary>
        /// <param name="gameTime"></param>
        protected void drawDirectoryChooser(GameTime gameTime)
        {
            // We only draw this if we've finished moving
            //
            if (m_eye != m_newEyePosition)
                return;

            // Draw header
            //
            string line;
            Vector2 lineOrigin = new Vector2();
            float yPosition = 0.0f;

            // We are showing this in the OverlayFont
            //
            Vector3 startPosition = new Vector3((float)m_project.getFontManager().getOverlayFont().MeasureString("X").X * 20,
                                                (float)m_project.getFontManager().getOverlayFont().LineSpacing * 8,
                                                0.0f);


            if (m_state.equals("FileOpen"))
            {
                line = "Open file...";
            }
            else if (m_state.equals("FileSaveAs"))
            {
                line = "Save as...";
            }
            else if (m_state.equals("PositionScreenNew") || m_state.equals("PositionScreenOpen") || m_state.equals("PositionScreenCopy"))
            {
                line = "Choose a position...";
            }
            else
            {
                line = "Unknown State...";
            }

            // Overlay batch
            //
            m_overlaySpriteBatch.Begin();

            // Draw header line
            //
            m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(), line, new Vector2((int)startPosition.X, (int)(startPosition.Y - m_project.getSelectedBufferView().getLineSpacing() * 3)), Color.White, 0, lineOrigin, 1.0f, 0, 0);

            // If we're using this method to position a new window only then don't show the directory chooser part..
            //
            if (m_state.equals("PositionScreenNew") || m_state.equals("PositionScreenCopy"))
            {
                m_overlaySpriteBatch.End();
                return;
            }

            Color dirColour = Color.White;

            startPosition.X += 50.0f;

            int lineNumber = 0;
            int dropStep = 6;

            // Page handling in the GUI
            //
            float showPage = 6.0f; // rows before stepping down
            int showOffset = (int)(((float)m_fileSystemView.getHighlightIndex()) / showPage);

            // This works out where the list that we're showing should end
            //
            int endShowing = (m_fileSystemView.getHighlightIndex() < dropStep ? dropStep : m_fileSystemView.getHighlightIndex()) + (int)showPage;


            // Draw the drives
            //
            if (m_fileSystemView.atDriveLevel())
            {
                DriveInfo[] driveInfo = m_fileSystemView.getDrives();
                //lineNumber = 0;

                foreach (DriveInfo d in driveInfo)
                {
                    if (!d.IsReady)
                    {
                        continue;
                    }

                    if (lineNumber > m_fileSystemView.getHighlightIndex() - dropStep
                        && lineNumber <= endShowing)
                    {
                        if (lineNumber < endShowing)
                        {
                            line = "[" + d.Name + "] " + d.VolumeLabel;
                        }
                        else
                        {
                            yPosition += m_project.getFontManager().getOverlayFont().LineSpacing /* * 1.5f */;
                            line = "...";
                        }

                        m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(),
                             line,
                             new Vector2((int)startPosition.X, (int)(startPosition.Y + yPosition)),
                             (lineNumber == m_fileSystemView.getHighlightIndex() ? m_highlightColour : (lineNumber == endShowing ? Color.White : dirColour)),
                             0,
                             lineOrigin,
                             1.0f,
                             0, 0);

                        yPosition += m_project.getFontManager().getOverlayFont().LineSpacing /* * 1.5f */;
                    }

                    lineNumber++;
                }
            }
            else // This is where we draw Directories and Files
            {
                if (!Directory.Exists(m_fileSystemView.getPath()))
                {
                    m_fileSystemView.setDirectory(@"C:\");
                }


                // For drives and directories we highlight item 1  - not zero
                //
                lineNumber = 1;
                FileInfo[] fileInfo = m_fileSystemView.getDirectoryInfo().GetFiles();
                DirectoryInfo[] dirInfo = m_fileSystemView.getDirectoryInfo().GetDirectories();

#if DIRECTORY_CHOOSER_DEBUG
                Logger.logMsg("showPage = " + showPage);
                Logger.logMsg("showOffset = " + showOffset);
                Logger.logMsg("m_directoryHighlight = " + m_directoryHighlight);
#endif

                line = m_fileSystemView.getPath() + m_saveFileName;
                m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(), line, new Vector2((int)startPosition.X, (int)startPosition.Y), (m_fileSystemView.getHighlightIndex() == 0 ? m_highlightColour : dirColour), 0, lineOrigin, 1.0f, 0, 0);

                yPosition += m_project.getFontManager().getOverlayFont().LineSpacing * 3.0f;

                foreach (DirectoryInfo d in dirInfo)
                {
                    if (lineNumber > m_fileSystemView.getHighlightIndex() - dropStep
                        && lineNumber <= endShowing)
                    {
                        if (lineNumber < endShowing)
                        {
                            line = "[" + d.Name + "]";
                        }
                        else
                        {
                            yPosition += m_project.getFontManager().getOverlayFont().LineSpacing;
                            line = "...";
                        }

                        m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(),
                             line,
                             new Vector2(startPosition.X, startPosition.Y + yPosition),
                             (lineNumber == m_fileSystemView.getHighlightIndex() ? m_highlightColour : (lineNumber == endShowing ? Color.White : dirColour)),
                             0,
                             lineOrigin,
                             1.0f,
                             0, 0);

                        yPosition += m_project.getFontManager().getOverlayFont().LineSpacing;
                    }

                    lineNumber++;
                }

                foreach (FileInfo f in fileInfo)
                {
                    if (lineNumber > m_fileSystemView.getHighlightIndex() - dropStep
                        && lineNumber <= endShowing)
                    {
                        if (lineNumber < endShowing)
                        {
                            line = f.Name;
                        }
                        else
                        {
                            yPosition += m_project.getFontManager().getDefaultLineSpacing();
                            line = "...";
                        }

                        m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(),
                                                 line,
                                                 new Vector2((int)startPosition.X, (int)(startPosition.Y + yPosition)),
                                                 (lineNumber == m_fileSystemView.getHighlightIndex() ? m_highlightColour : (lineNumber == endShowing ? Color.White : m_itemColour)),
                                                 0,
                                                 lineOrigin,
                                                 1.0f,
                                                 0, 0);

                        yPosition += m_project.getFontManager().getOverlayFont().LineSpacing/* * 1.5f */;
                    }
                    lineNumber++;
                }
            }

            if (m_temporaryMessageEndTime > gameTime.TotalGameTime.TotalSeconds && m_temporaryMessage != "")
            {
                // Add any temporary message on to the end of the message
                //
                m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(),
                                         m_temporaryMessage,
                                         new Vector2((int)startPosition.X, (int)(startPosition.Y - 30.0f)),
                                         Color.LightGoldenrodYellow,
                                         0,
                                         lineOrigin,
                                         1.0f,
                                         0,
                                         0);
            }

            // Close the SpriteBatch
            //
            m_overlaySpriteBatch.End();
        }

        /// <summary>
        /// How to draw a diff'd BufferView on the screen - we key on m_diffPosition rather
        /// than using the cursor.  Always start from the translated lhs window position.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="gameTime"></param>
        protected void drawDiffBuffer(BufferView view, GameTime gameTime)
        {
            // Only process for diff views
            //
            if (view != m_differ.getSourceBufferViewLhs() && view != m_differ.getSourceBufferViewRhs())
            {
                return;
            }

            int sourceLine = m_diffPosition;
            string line = "";
            Color colour = Color.White;
            Vector3 viewSpaceTextPosition = view.getPosition();
            float yPosition = 0;
            List<Pair<DiffResult, int>> diffList;

            // Get the diffList generated in the Differ object - this holds all the expanded
            // diff information which we'll need to adjust for (on the right hand side) if we're
            // to generate a meaningful side by side diff whilst we scroll through it.
            //
            if (view == m_differ.getSourceBufferViewLhs())
            {
                diffList = m_differ.getLhsDiff();
            }
            else
            {
                diffList = m_differ.getRhsDiff();
            }


            // Need to adjust the sourceLine by the number of padding lines in the diffList up to this
            // point - otherwise we lost alignment as we scroll through the document.
            //
            for (int j = 0; j < m_diffPosition; j++)
            {
                if (j < diffList.Count)
                {
                    if (diffList[j].First == DiffResult.Padding)
                    {
                        sourceLine--;
                    }
                }
            }

            // Now iterate down the view and pull in the lines as required
            //
            for (int i = 0; i < view.getBufferShowLength(); i++)
            {
                if ((i + m_diffPosition) < diffList.Count)
                {
                    switch (diffList[i + m_diffPosition].First)
                    {
                        case DiffResult.Unchanged:
                            colour = m_differ.m_unchangedColour;

                            if (sourceLine < view.getFileBuffer().getLineCount())
                            {
                                line = view.getFileBuffer().getLine(sourceLine++);
                            }
                            // print line
                            break;

                        case DiffResult.Deleted:
                            // print deleted line (colour change?)
                            colour = m_differ.m_deletedColour;

                            if (sourceLine < view.getFileBuffer().getLineCount())
                            {
                                line = view.getFileBuffer().getLine(sourceLine++);
                            }
                            break;

                        case DiffResult.Inserted:
                            // print inserted line (colour)
                            colour = m_differ.m_insertedColour;

                            if (sourceLine < view.getFileBuffer().getLineCount())
                            {
                                line = view.getFileBuffer().getLine(sourceLine++);
                            }
                            break;

                        case DiffResult.Padding:
                        default:
                            colour = m_differ.m_paddingColour;
                            line = "";
                            // add a padding line
                            break;
                    }

                }
                else
                {
                    // Do something to handle blank lines beyond end of list
                    //
                    colour = m_differ.m_paddingColour;
                    line = "";
                }

                // Truncate the line as necessary
                //
                string drawLine = line.Substring(view.getBufferShowStartX(), Math.Min(line.Length - view.getBufferShowStartX(), view.getBufferShowWidth()));
                if (view.getBufferShowStartX() + view.getBufferShowWidth() < line.Length)
                {
                    drawLine += " [>]";
                }

                m_spriteBatch.DrawString(
                                m_project.getFontManager().getViewFont(view.getViewSize()),
                                drawLine,
                                new Vector2((int)viewSpaceTextPosition.X /* + m_project.getFontManager().getCharWidth() * xPos */, (int)(viewSpaceTextPosition.Y + yPosition)),
                                colour,
                                0,
                                Vector2.Zero,
                                m_project.getFontManager().getTextScale(),
                                0,
                                0);

                //sourceLine++;

                yPosition += m_project.getFontManager().getLineSpacing(view.getViewSize());

            }
        }



        /// <summary>
        /// Render some scrolling text to a texture.  This takes the current m_temporaryMessage and renders
        /// to a texture according to how much time has passed since the message was created.
        /// </summary>
        protected void renderTextScroller()
        {
            if (m_state .notEquals("TextEditing"))
            {
                return;
            }
            if (m_temporaryMessage == "")
            {
                return;
            }

            // Speed - higher is faster
            //
            float speed = 120.0f;

            // Set the render target and clear the buffer
            //
            m_graphics.GraphicsDevice.SetRenderTarget(m_textScroller);
            m_graphics.GraphicsDevice.Clear(Color.Black);

            // Start with whole message showing and scroll it left
            //
            int newPosition = (int)((m_gameTime.TotalGameTime.TotalSeconds - m_temporaryMessageStartTime) * -speed);

            if ((newPosition + (int)(m_temporaryMessage.Length * m_project.getFontManager().getCharWidth(FontManager.FontType.Overlay))) < 0)
            {
                // Set the temporary message to start again and adjust position/time 
                // by width of the textScroller.
                //
                m_temporaryMessageStartTime = m_gameTime.TotalGameTime.TotalSeconds + m_textScroller.Width / speed;
            }

            // xPosition holds the scrolling position of the text in the temporary message window
            int xPosition = 0;
            float delayScroll = 0.7f; // delay the scrolling by this amount so we can read it before it starts moving

            if (m_gameTime.TotalGameTime.TotalSeconds - m_temporaryMessageStartTime > delayScroll)
            {
                xPosition = (int)((m_gameTime.TotalGameTime.TotalSeconds - delayScroll - m_temporaryMessageStartTime) * -120.0f);
            }

            // Draw to the render target
            //
            m_spriteBatch.Begin();
            m_spriteBatch.DrawString(m_project.getFontManager().getOverlayFont(), m_temporaryMessage, new Vector2((int)xPosition, 0), Color.Pink, 0, new Vector2(0, 0), 1.0f, 0, 0);
            m_spriteBatch.End();

            // Now reset the render target to the back buffer
            //
            m_graphics.GraphicsDevice.SetRenderTarget(null);
            m_textScrollTexture = (Texture2D)m_textScroller;
        }

        /// <summary>
        /// Draw a scroll bar for a BufferView
        /// </summary>
        /// <param name="view"></param>
        /// <param name="file"></param>
        protected void drawScrollbar(BufferView view)
        {
            Vector3 sbPos = view.getPosition();
            float height = view.getBufferShowLength() * m_project.getFontManager().getLineSpacing(view.getViewSize());

            Rectangle sbBackGround = new Rectangle(Convert.ToInt16(sbPos.X - m_project.getFontManager().getTextScale() * 30.0f),
                                                   Convert.ToInt16(sbPos.Y),
                                                   1,
                                                   Convert.ToInt16(height));

            // Draw scroll bar
            //
            m_spriteBatch.Draw(m_flatTexture, sbBackGround, Color.DarkCyan);

            // Draw viewing window
            //
            float start = view.getBufferShowStartY();

            // Override this for the diff view
            //
            if (m_differ != null && m_state.equals("DiffPicker"))
            {
                start = m_diffPosition;
            }

            float length = 0;

            // Get the line count
            //
            if (view.getFileBuffer() != null)
            {
                // Make this work for diff view as well as normal view
                //
                if (m_differ != null && m_state.equals("DiffPicker"))
                {
                    length = m_differ.getMaxDiffLength();
                }
                else
                {
                    length = view.getFileBuffer().getLineCount();
                }
            }

            // Check for length of FileBuffer in case it's empty
            //
            if (length > 0)
            {
                float scrollStart = start / length * height;
                float scrollLength = height; // full height unless we have anything to scroll

                if (length > view.getBufferShowLength())
                {
                    scrollLength = view.getBufferShowLength() / length * height;

                    // Ensure that scroll bar highlight is no longer than scroll bar
                    //
                    //if (scrollStart + scrollLength > height)
                    //{
                    //scrollLength = height - scrollStart;
                    //}
                }

                // Minimum scrollLength
                //
                if (scrollLength < 2)
                {
                    scrollLength = 2;
                }

                // Ensure that the highlight doens't jump over the end of the scrollbar
                //
                if (scrollStart + scrollLength > height)
                {
                    scrollStart = height - scrollLength;
                }

                Rectangle sb = new Rectangle(Convert.ToInt16(sbPos.X - m_project.getFontManager().getTextScale() * 30.0f),
                                             Convert.ToInt16(sbPos.Y + scrollStart),
                                             1,
                                             Convert.ToInt16(scrollLength));

                // Draw scroll bar window position
                //
                m_spriteBatch.Draw(m_flatTexture, sb, Color.LightGoldenrodYellow);
            }

            // Draw a highlight overview
            //
            if (view.gotHighlight() && length > 0)
            {
                //float hS = view.getHighlightStart().Y;

                float highlightStart = ((float)view.getHighlightStart().Y) / length * height;
                float highlightEnd = ((float)view.getHighlightEnd().Y) / length * height;

                Rectangle hl = new Rectangle(Convert.ToInt16(sbPos.X - m_project.getFontManager().getTextScale() * 40.0f),
                                             Convert.ToInt16(sbPos.Y + highlightStart),
                                             1,
                                             Convert.ToInt16(highlightEnd - highlightStart));

                m_spriteBatch.Draw(m_flatTexture, hl, view.getHighlightColor());
            }
        }

        /// <summary>
        /// This can be called from anywhere so let's ensure that we have a bit of locking
        /// around the checkExit code.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnExiting(Object sender, EventArgs args)
        {
            base.OnExiting(sender, args);

            // Stop the threads
            //
            if (m_kinectWorker != null || m_counterWorker != null)
            {
                checkExit(m_gameTime, true);
            }
        }


        /// <summary>
        /// Draw a screen which allows us to configure some settings
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="text"></param>
        protected void drawConfigurationScreen(GameTime gameTime)
        {
            Vector3 fp = m_project.getSelectedBufferView().getPosition();

            // Starting positions
            //
            float yPos = 5.5f * m_project.getFontManager().getLineSpacing(FontManager.FontType.Overlay);
            float xPos = 10 * m_project.getFontManager().getCharWidth(FontManager.FontType.Overlay);

            // Start the spritebatch
            //
            m_overlaySpriteBatch.Begin();

            if (m_editConfigurationItem) // Edit a single configuration item
            {
                string text = "Edit configuration item";

                m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(), text, new Vector2((int)xPos, (int)yPos), Color.White, 0, Vector2.Zero, 1.0f, 0, 0);
                yPos += m_project.getFontManager().getLineSpacing(FontManager.FontType.Overlay) * 2;

                m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(), m_project.getConfigurationItem(m_configPosition).Name, new Vector2((int)xPos, (int)yPos), m_itemColour, 0, Vector2.Zero, 1.0f, 0, 0);
                yPos += m_project.getFontManager().getLineSpacing(FontManager.FontType.Overlay);

                string configString = m_editConfigurationItemValue;
                if (configString.Length > m_project.getSelectedBufferView().getBufferShowWidth())
                {
                    configString = "[..]" + configString.Substring(configString.Length - m_project.getSelectedBufferView().getBufferShowWidth() + 4, m_project.getSelectedBufferView().getBufferShowWidth() - 4);
                }

                m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(), configString, new Vector2((int)xPos, (int)yPos), m_highlightColour, 0, Vector2.Zero, 1.0f, 0, 0);
            }
            else
            {
                string text = "Configuration Items";

                m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(), text, new Vector2((int)xPos, (int)yPos), Color.White, 0, Vector2.Zero, 1.0f, 0, 0);
                yPos += m_project.getFontManager().getLineSpacing(FontManager.FontType.Overlay) * 2;

                // Write all the configuration items out - if we're highlight one of them then change
                // the colour.
                //
                for (int i = 0; i < m_project.getConfigurationListLength(); i++)
                {
                    string configItem = m_project.estimateFileStringTruncation("", m_project.getConfigurationItem(i).Value, 60 - m_project.getConfigurationItem(i).Name.Length);
                    //string item = m_project.getConfigurationItem(i).Name + "  =  " + m_project.getConfigurationItem(i).Value;
                    string item = m_project.getConfigurationItem(i).Name + "  =  " + configItem;

                    item = m_project.estimateFileStringTruncation("", item, m_project.getSelectedBufferView().getBufferShowWidth());

                    /*
                    if (item.Length > m_project.getSelectedBufferView().getBufferShowWidth())
                    {
                        item = item.Substring(m_configXOffset, m_project.getSelectedBufferView().getBufferShowWidth());
                    }
                    */

                    m_overlaySpriteBatch.DrawString(m_project.getFontManager().getOverlayFont(), item, new Vector2((int)xPos, (int)yPos), (i == m_configPosition ? m_highlightColour : m_itemColour), 0, Vector2.Zero, 1.0f, 0, 0);
                    yPos += m_project.getFontManager().getLineSpacing(FontManager.FontType.Overlay);
                }
            }

            m_overlaySpriteBatch.End();
        }

        /// <summary>
        /// Perform an external build
        /// </summary>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected void doBuildCommand(GameTime gameTime, string overrideString = "")
        {

            if (m_buildProcess != null)
            {
                Logger.logMsg("XygloXNA::doBuildCommand() - build in progress");
                setActiveBuffer(m_buildStdOutView);
                setTemporaryMessage("Checking build status", 3, m_gameTime);
                return;
            }

            Logger.logMsg("XygloXNA::doBuildCommand() - attempting to run build command");

            // Check that we can find the build command
            //
            try
            {
                //string[] commandList = m_project.getBuildCommand().Split(' ');
                string[] commandList = m_project.getConfigurationValue("BUILDCOMMAND").Split(' ');

                // Override the default build command
                //
                if (overrideString != "")
                {
                    commandList = overrideString.Split();
                }

                if (commandList.Length == 0)
                {
                    setTemporaryMessage("Build command not defined", 2, gameTime);
                }
                else
                {
                    // If the end of the build command is no a .exe or a .bat then assume we've got a 
                    // space in the file name somewhere.  This code fixes spaces in file names for us
                    // in the command list for the first argument but could (and should) be extended
                    // to all arguments that don't make any sense.
                    //
                    if (!File.Exists(commandList[0]))
                    {
                        if (commandList[0].Length > 5)
                        {
                            int pos = 0;

                            while (pos < commandList.Length)
                            {
                                string endCommand = commandList[pos].Substring(commandList[pos].Length - 4, 4).ToUpper();

                                if (endCommand == ".EXE" || endCommand == ".BAT")
                                {
                                    // Create a new command list and combine the first 'pos' commands into
                                    // a single correct one.
                                    //
                                    string[] newCommandList = new string[commandList.Length - pos];

                                    for (int i = 0; i < commandList.Length; i++)
                                    {
                                        if (i <= pos)
                                        {
                                            newCommandList[0] += commandList[i];

                                            if (i < pos)
                                            {
                                                newCommandList[0] += " ";
                                            }
                                        }
                                        else
                                        {
                                            newCommandList[i - pos] = commandList[i];
                                        }
                                    }

                                    // Now assigned command list from newCommandList
                                    commandList = newCommandList;
                                    break;
                                }
                                else
                                {
                                    pos++;
                                }
                            }

                            //if (commandList[0].Substring(commandList[0].Length - 4, 3).ToUpper() != "EXE
                        }
                    }

                    // We ensure that full path is given to build command at this time
                    //
                    if (!File.Exists(commandList[0]))
                    {
                        setTemporaryMessage("Build command not found : \"" + commandList[0] + "\"", 5, gameTime);
                    }
                    else
                    {
                        string buildDir = m_project.getConfigurationValue("BUILDDIRECTORY");
                        string buildStdOutLog = m_project.getConfigurationValue("BUILDSTDOUTLOG");
                        string buildStdErrLog = m_project.getConfigurationValue("BUILDSTDERRLOG");

                        // Check the build directory
                        //
                        if (!Directory.Exists(buildDir))
                        {
                            setTemporaryMessage("Build directory doesn't exist : \"" + buildDir + "\"", 2, gameTime);
                            return;
                        }

                        // Add a standard error view
                        //
                        if (!File.Exists(buildStdErrLog))
                        {
                            StreamWriter newStdErr = File.CreateText(buildStdErrLog);
                            newStdErr.Close();
                        }

                        m_buildStdErrView = m_project.findBufferView(buildStdErrLog);

                        if (m_buildStdErrView == null)
                        {
                            m_buildStdErrView = addNewFileBuffer(buildStdErrLog, true, true);
                        }
                        m_buildStdErrView.setTailColour(Color.Orange);
                        m_buildStdErrView.noHighlight();

                        //m_buildStdErrView.setReadOnlyColour(Color.DarkRed);

                        // Store the line length of the existing file
                        //
                        m_project.setStdErrLastLine(m_buildStdErrView.getFileBuffer().getLineCount());

                        // If the build log doesn't exist then create it
                        //
                        if (!File.Exists(buildStdOutLog))
                        {
                            StreamWriter newStdOut = File.CreateText(buildStdOutLog);
                            newStdOut.Close();
                        }

                        // Now ensure that the build log is visible on the screen somewhere
                        //
                        m_buildStdOutView = m_project.findBufferView(buildStdOutLog);

                        if (m_buildStdOutView == null)
                        {
                            m_buildStdOutView = addNewFileBuffer(buildStdOutLog, true, true);
                        }
                        m_buildStdOutView.noHighlight();

                        // Store the line length of the existing file
                        //
                        m_project.setStdOutLastLine(m_buildStdOutView.getFileBuffer().getLineCount());

                        // Move to that BufferView
                        //
                        setActiveBuffer(m_buildStdOutView);

                        // Build the argument list
                        //
                        ProcessStartInfo info = new ProcessStartInfo();
                        info.WorkingDirectory = buildDir;
                        //info.WorkingDirectory = "C:\\Q\\mingw\\bin";
                        //info.EnvironmentVariables.Add("PATH", "C:\\Q\\mingw\\bin");
                        //info.EnvironmentVariables.Add("TempPath", "C:\\Temp");
                        info.UseShellExecute = false;
                        info.FileName = m_project.getCommand(commandList);
                        info.WindowStyle = ProcessWindowStyle.Hidden;
                        info.CreateNoWindow = true;
                        //info.Arguments = m_project.getArguments() + (options == "" ? "" : " " + options);
                        info.Arguments = m_project.getArguments(commandList);
                        info.RedirectStandardOutput = true;
                        info.RedirectStandardError = true;

                        // Append the command to the stdout file
                        //
                        m_buildStdOutView.getFileBuffer().appendLine("Running command: " + string.Join(" ", commandList));
                        m_buildStdOutView.getFileBuffer().save();

                        m_buildProcess = new Process();
                        m_buildProcess.StartInfo = info;
                        m_buildProcess.OutputDataReceived += new DataReceivedEventHandler(logBuildStdOut);
                        m_buildProcess.ErrorDataReceived += new DataReceivedEventHandler(logBuildStdErr);
                        m_buildProcess.Exited += new EventHandler(buildCompleted);

                        m_buildProcess.EnableRaisingEvents = true;

                        Logger.logMsg("XygloXNA::doBuildCommand() - working directory = " + info.WorkingDirectory);
                        Logger.logMsg("XygloXNA::doBuildCommand() - filename = " + info.FileName);
                        Logger.logMsg("XygloXNA::doBuildCommand() - arguments = " + info.Arguments);

                        // Start the external build command and check the logs
                        //
                        m_buildProcess.Start();
                        m_buildProcess.BeginOutputReadLine();
                        m_buildProcess.BeginErrorReadLine();

                        // Inform that we're starting the build
                        //
                        setTemporaryMessage("Starting build..", 4, gameTime);
                        m_drawingHelper.startBanner(m_gameTime, "Build started", 5);

                        /*
                        // Handle any immediate exit error code
                        //
                        if (m_buildProcess.ExitCode != 0)
                        {
                            Logger.logMsg("XygloXNA::doBuildCommand() - build process failed with code " + m_buildProcess.ExitCode);
                        }
                        else
                        {
                            Logger.logMsg("XygloXNA::doBuildCommand() - started build command succesfully");
                        }
                         * */
                    }
                }
            }
            catch (Exception e)
            {
                Logger.logMsg("Can't run command " + e.Message);

                // Disconnect the file handlers and the exit handler
                //
                m_buildProcess.OutputDataReceived -= new DataReceivedEventHandler(logBuildStdOut);
                m_buildProcess.ErrorDataReceived -= new DataReceivedEventHandler(logBuildStdErr);
                m_buildProcess.Exited -= new EventHandler(buildCompleted);

                // Set an error message
                //
                setTemporaryMessage("Problem when running command - " + e.Message, 5, gameTime);

                // Dispose of the build process object
                //
                m_buildProcess = null;
            }
        }

        /// <summary>
        /// Write the stdout from the build process to a log file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void logBuildStdOut(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                Logger.logMsg("XygloXNA::logBuildStdOut() - got null data");
                return;
            }

            string time = string.Format("{0:yyyyMMdd HH:mm:ss}", DateTime.Now);
            string logBody = "INF:" + time + ":" + e.Data;

            m_buildStdOutView.getFileBuffer().appendLine(logBody);

            // Save the log file
            //
            m_buildStdOutView.getFileBuffer().setReadOnly(false);
            m_buildStdOutView.getFileBuffer().save();
            m_buildStdOutView.getFileBuffer().setReadOnly(true);
#if WRITE_LOG_FILE

            System.IO.TextWriter logFile = new StreamWriter(m_project.getConfigurationValue("BUILDLOG"), true);
            logFile.WriteLine("INF:" + time + ":" + logBody);
            logFile.Flush();
            logFile.Close();
            logFile = null;
#endif

            // Ensure we're looking at the end of the file
            //
            m_buildStdOutView.setTailPosition();
        }

        /// <summary>
        /// Write stderr
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void logBuildStdErr(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                Logger.logMsg("XygloXNA::logBuildStdErr() - got null data");
                return;
            }

            string time = string.Format("{0:yyyyMMdd HH:mm:ss}", DateTime.Now);
            string logBody = "ERR:" + time + ":" + (string)e.Data;

            m_buildStdErrView.getFileBuffer().appendLine(logBody);

            // Save the log file
            //
            m_buildStdErrView.getFileBuffer().setReadOnly(false);
            m_buildStdErrView.getFileBuffer().save();
            m_buildStdErrView.getFileBuffer().setReadOnly(true);

#if WRITE_LOG_FILE
            System.IO.TextWriter logFile = new StreamWriter(m_project.getConfigurationValue("BUILDLOG"), true);
            logFile.WriteLine("ERR:" + time + ":" + logBody);
            logFile.Flush();
            logFile.Close();
            logFile = null;
#endif

            // Ensure we're looking at the end of the file
            //
            m_buildStdErrView.setTailPosition();
        }

        /// <summary>
        /// Build completed callback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buildCompleted(object sender, System.EventArgs e)
        {

            // If there was an issue with the build then move to the active buffer that holds the error logs
            //
            if (m_buildProcess.ExitCode != 0)
            {
                setActiveBuffer(m_buildStdErrView);
                setTemporaryMessage("Build failed with exit code " + m_buildProcess.ExitCode, 5, m_gameTime);
                m_buildStdErrView.setTailColour(Color.Red);

                m_drawingHelper.startBanner(m_gameTime, "Build failed", 5);
            }
            else
            {
                setTemporaryMessage("Build completed successfully.", 3, m_gameTime);

                // Also colour the error log green
                //
                m_buildStdErrView.setTailColour(Color.Green);

                m_drawingHelper.startBanner(m_gameTime, "Build completed", 5);
            }

            // Invalidate the build process
            //
            m_buildProcess = null;
        }

        /// <summary>
        /// Dragged files entering event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void friendlierDragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            Logger.logMsg("XygloXNA::friendlierDragEnter() - dragging entered " + e.Data.GetType().ToString());

            //if (!e.Data.GetDataPresent(typeof(System.Windows.Forms.DataObject)))
            if (e.Data.GetType() != typeof(System.Windows.Forms.DataObject))
            {
                e.Effect = System.Windows.Forms.DragDropEffects.None;
                return;
            }

            // Effect is to "link" to this project
            //
            e.Effect = System.Windows.Forms.DragDropEffects.Link;
        }

        /// <summary>
        /// Drag and drop target function - do some file and directory adding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void friendlierDragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            Logger.logMsg("XygloXNA::friendlierDragEnter() - drop event fired of type " + e.Data.ToString());

            if (e.Data.GetType() == typeof(System.Windows.Forms.DataObject))
            {
                System.Windows.Forms.DataObject obj = (System.Windows.Forms.DataObject)e.Data.GetData(typeof(System.Windows.Forms.DataObject));
                string[] formats = e.Data.GetFormats();
                string[] files = (string[])e.Data.GetData(System.Windows.Forms.DataFormats.FileDrop);

                //int filesAdded = 0;
                //int dirsAdded = 0;

                List<string> filesAdded = new List<string>();
                List<string> dirsAdded = new List<string>();

                BufferView newView = null;

                foreach (string newFile in files)
                {

                    // Is this a directory or a file?
                    //
                    if (Directory.Exists(newFile))
                    {
                        Logger.logMsg("XygloXNA::friendlierDragDrop() - adding directory = " + newFile);
                        addNewDirectory(newFile);
                        dirsAdded.Add(newFile);
                    }
                    else
                    {
                        Logger.logMsg("XygloXNA::friendlierDragDrop() - adding file " + newFile);
                        newView = addNewFileBuffer(newFile);
                        filesAdded.Add(newFile);
                    }
                }

                // Always set to the last added BufferView
                //
                if (newView != null)
                {
                    setActiveBuffer(newView);
                }

                // Build an intelligible temporary message after we've done this work
                //
                string message = "";

                if (filesAdded.Count > 0)
                {
                    message = filesAdded.Count + " file";

                    if (filesAdded.Count > 1)
                    {
                        message += "s";
                    }

                    message += " added ";

                    foreach (string fi in filesAdded)
                    {
                        message += " " + fi;
                    }
                }

                if (dirsAdded.Count > 0)
                {
                    if (message != "")
                    {
                        message += ", ";
                    }

                    message += dirsAdded.Count + " ";

                    if (dirsAdded.Count == 1)
                    {
                        message += "directory";
                    }
                    else
                    {
                        message += "directories";
                    }

                    message += " added";

                    foreach (string di in dirsAdded)
                    {
                        message += " " + di;
                    }
                }

                // Set the temporary message if we've generated one
                //
                if (message != "")
                {
                    setTemporaryMessage(message, 5, m_gameTime);
                }
            }
        }

        /// <summary>
        /// Add directory full of files recursively
        /// </summary>
        /// <param name="dirPath"></param>
        protected void addNewDirectory(string dirPath)
        {
            Logger.logMsg("XygloXNA::addNewDirectory() - adding directory " + dirPath);
        }
    }
}
