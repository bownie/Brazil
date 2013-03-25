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

using Xyglo.Brazil;
using Xyglo.Brazil.Xna.Physics;
using Xyglo.Gesture;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// XygloXNA is defined by a XNA Game class - the core of the XNA world.
    /// </summary>
    public class XygloXNA : Game, IBrazilTechnology
    {
        /////////////////////////////// CONSTRUCTORS ////////////////////////////
        /// <summary>
        /// Default constructor
        /// </summary>
        public XygloXNA(ActionMap actionMap, List<Component> componentList, BrazilWorld world, List<State> states, List<Target> targets, Dictionary<string, Resource> resources, string homePath)
        {
            // Setup context and handlers
            //
            m_context = new XygloContext();
            m_context.m_componentList = componentList;

            // Setup BrazilContext
            //
            m_brazilContext = new BrazilContext();
            m_brazilContext.m_actionMap = actionMap;
            m_brazilContext.m_world = world;
            m_brazilContext.m_states = states;
            m_brazilContext.m_targets = targets;
            m_brazilContext.m_resourceMap = resources;
            m_brazilContext.m_homePath = homePath;

            // Setup the XygloMouse
            //
            m_mouse = new XygloMouse(m_context, m_brazilContext);

            // Link signals up to the handlers
            //
            m_mouse.ChangePositionEvent += new PositionChangeEventHandler(handleFlyToPosition);
            m_mouse.XygloViewChangeEvent += new XygloViewChangeEventHandler(handleViewChange);
            m_mouse.EyeChangeEvent += new EyeChangeEventHandler(handleEyeChange);
            m_mouse.NewBufferViewEvent += new NewBufferViewEventHandler(handleNewBufferView);
            m_mouse.TemporaryMessageEvent += new TemporaryMessageEventHandler(handleTemporaryMessage);

            // Keyboard wrapper class
            //
            m_keyboard = new XygloKeyboard(m_context, world.getKeyAutoRepeatHoldTime(), world.getKeyAutoRepeatInterval());

            // FontManager
            //
            m_context.m_fontManager = new FontManager();

            // Graphics helper
            //
            m_graphics = new XygloGraphics(m_context);

            // Keyboard handling class - performs interpretation of the key commands into
            // whatever we want to do.
            //
            m_keyboardHandler = new XygloKeyboardHandler(m_context, m_brazilContext, m_graphics, m_keyboard);
            m_keyboardHandler.TemporaryMessageEvent += new TemporaryMessageEventHandler(handleTemporaryMessage);
            m_keyboardHandler.XygloViewChangeEvent += new XygloViewChangeEventHandler(handleViewChange);
            m_keyboardHandler.ChangePositionEvent += new PositionChangeEventHandler(handleFlyToPosition);
            m_keyboardHandler.CleanExitEvent += new CleanExitEventHandler(handleCleanExit);
            m_keyboardHandler.CommandEvent += new CommandEventHandler(handleCommand);

            // Temporary Messages
            //
            m_tempMessage = new TemporaryMessage(m_context);
            m_tempMessage.TemporaryMessageEvent += new TemporaryMessageEventHandler(handleTemporaryMessage);
            
            // The Eye position handler
            //
            m_eyeHandler = new EyeHandler(m_context, m_keyboardHandler);

            // Initialise including the physics handler
            //
            initialise();

            // Now we can generate the XygloFactory - the physics handler is only initialised above
            //
            m_factory = new XygloFactory(m_context, m_brazilContext, m_physicsHandler, m_eyeHandler, m_mouse, m_frameCounter);

            // And generate the XygloEngine object that does our state transitions
            //
            m_engine = new XygloEngine(m_context, m_brazilContext, m_physicsHandler, m_keyboard, m_keyboardHandler, m_eyeHandler);
            m_engine.CleanExitEvent += new CleanExitEventHandler(handleCleanExit);
            m_engine.TemporaryMessageEvent += new TemporaryMessageEventHandler(handleTemporaryMessage);
            m_engine.NewBufferViewEvent += new NewBufferViewEventHandler(handleNewBufferView);


            // Start the leap test
            //
            //testLeap();
        }

        /*
        protected void testLeap()
        {
            // Create a sample listener and controller
            m_context.m_leapListener = new LeapListener();
            m_context.m_leapController = new Leap.Controller(m_context.m_leapListener);
        }
*/
         /////////////////////////////// METHODS //////////////////////////////////////
        /// <summary>
        /// Set the project
        /// </summary>
        /// <param name="project"></param>
        public void setProject(Project project)
        {
            m_context.m_project = project;

            // System Analyser for performance stats
            //
            if (m_context.m_project != null)
                m_systemAnalyser = new SystemAnalyser();

            // Reset windowed mode
            //
            m_graphics.windowedMode(this);
        }

        /// <summary>
        /// Get the project
        /// </summary>
        /// <returns></returns>
        public Project getProject()
        {
            return m_context.m_project;
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
            m_keyboard.setRepeats(m_brazilContext.m_world.getKeyAutoRepeatHoldTime(), m_brazilContext.m_world.getKeyAutoRepeatInterval());
        }

        /// <summary>
        /// Check a Target exists
        /// </summary>
        /// <param name="target"></param>
        protected string confirmTarget(string targetName)
        {
            foreach (Target target in m_brazilContext.m_targets)
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

            m_context.m_graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            // Initialise the bloom component
            //
            m_context.m_bloom = new BloomComponent(this);
            m_context.m_bloom.Settings = BloomSettings.PresetSettings[5];
            Components.Add(m_context.m_bloom);

            // Antialiasing
            //
            m_context.m_graphics.PreferMultiSampling = true;

            // Set windowed mode as default
            //
            m_graphics.windowedMode(this);

            // Check the demo status and set as necessary
            //
            if (m_context.m_project != null && !m_context.m_project.getLicenced())
            {
                m_brazilContext.m_state = State.Test("DemoExpired");
            }

            // Create our physics handler
            //
            m_physicsHandler = new PhysicsHandler(this, m_context);
            //m_physicsHandler.addGround(this);
        }

        /// <summary>
        /// Get the current application State
        /// </summary>
        /// <returns></returns>
        public State getState()
        {
            return m_brazilContext.m_state;
        }

        /// <summary>
        /// Set the State - we usually only do this once per instantiation and then let the world
        /// just live.
        /// </summary>
        /// <param name="state"></param>
        public void setState(State state)
        {
            m_brazilContext.m_state = state;
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
            m_context.m_project.connectFloatingWorld(m_brazilContext, m_context);

            // Reconnect these views if they exist
            //
            m_buildStdOutView = m_context.m_project.getStdOutView();
            m_buildStdErrView = m_context.m_project.getStdErrView();

            // Set the tab space
            //
            m_context.m_project.setTab("  ");

            // Initialise the configuration item if it's null - this is in case we've persisted
            // a version of the project without a configuration item it will create it here.
            //
            m_context.m_project.buildInitialConfiguration();

            // Load all the files - if we have nothing in this project then create a BufferView
            // and a FileBuffer.
            //
            if (m_context.m_project.getFileBuffers().Count == 0)
            {
                addNewFileBuffer(BufferView.ViewPosition.Right);
            }
            else
            {
                m_context.m_project.loadFiles(m_smartHelpWorker);
            }

            // Now do some jiggery pokery to make sure positioning is correct and that
            // any cursors or highlights are within bounds.
            //
            foreach (BufferView bv in m_context.m_project.getBufferViews())
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
            //m_activeBufferView = m_context.m_project.getSelectedView();

            // Ensure that we are in the correct position to view this buffer so there's no initial movement
            //
            m_eyeHandler.setEyePosition(m_context.m_project.getEyePosition());
            m_eyeHandler.setTargetPosition(m_context.m_project.getTargetPosition());
            m_context.m_zoomLevel = m_eyeHandler.getEyePosition().Z;

            // Set the active buffer view
            //
            setActiveBuffer();

            // Set-up the single FileSystemView we have
            //
            if (m_context.m_project.getOpenDirectory() == "")
            {
                m_context.m_project.setOpenDirectory(@"C:\");  // set Default
            }

            m_context.m_fileSystemView = new FileSystemView(m_context, m_brazilContext, m_context.m_project.getOpenDirectory(), new Vector3(-800.0f, 0f, 0f), m_context.m_project, m_context.m_fontManager);

            // Tree builder and model builder
            //
            m_keyboardHandler.generateTreeModel();
        }

        /// <summary>
        /// A event handler for FileSystemWatcher.  Specify what is done when a file is changed, created, or deleted.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            Logger.logMsg("XygloXNA::OnFileChanged() - File: " + e.FullPath + " " + e.ChangeType);
            XygloXNA sourceObject = (XygloXNA)source;

            foreach (FileBuffer fb in sourceObject.m_context.m_project.getFileBuffers())
            {
                if (fb.getFilepath() == e.FullPath)
                {
                    fb.forceRefetchFile(sourceObject.m_context.m_project.getSyntaxManager());
                }
            }
        }

        /// <summary>
        /// Set up all of our one shot translations
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            initializeWorld();

            m_physicsHandler.initialise();
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
            m_context.m_viewMatrix = Matrix.CreateLookAt(m_eyeHandler.getEyePosition(), m_eyeHandler.getTargetPosition(), Vector3.Up);
            m_context.m_projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * Matrix.CreatePerspectiveFieldOfView(m_context.m_fov, GraphicsDevice.Viewport.AspectRatio, 0.1f, 10000f);

            // Generate frustrum
            //
            if (m_context.m_frustrum == null)
            {
                m_context.m_frustrum = new BoundingFrustum(m_context.m_viewMatrix * m_context.m_projection);
            }
            else
            {
                // You can also update frustrum matrix like this
                //
                m_context.m_frustrum.Matrix = m_context.m_viewMatrix * m_context.m_projection;
            }

            m_context.m_basicEffect.View = m_context.m_viewMatrix;
            m_context.m_basicEffect.Projection = m_context.m_projection;
            m_context.m_basicEffect.World = Matrix.CreateScale(1, -1, 1);

            m_context.m_lineEffect.View = m_context.m_viewMatrix;
            m_context.m_lineEffect.Projection = m_context.m_projection;
            m_context.m_lineEffect.World = Matrix.CreateScale(1, -1, 1);

            m_context.m_physicsEffect.View = m_context.m_viewMatrix;
            m_context.m_physicsEffect.Projection = m_context.m_projection;
            m_context.m_physicsEffect.World = Matrix.CreateScale(1, -1, 1);

            // Create a new SpriteBatch, which can be used to draw textures.
            //
            m_context.m_spriteBatch = new SpriteBatch(m_context.m_graphics.GraphicsDevice);

            // Panner spritebatch
            //
            m_context.m_pannerSpriteBatch = new SpriteBatch(m_context.m_graphics.GraphicsDevice);
        }

        /// <summary>
        /// Load resources from files into our xyglo resource map.  Possibly completely not necessary any more after
        /// sorting out serialisation problems.
        /// </summary>
        protected void loadResources(Dictionary<string, Resource> resources)
        {
            foreach (string key in resources.Keys)
            {
                Resource res = resources[key];
                switch (res.getType())
                {
                    case ResourceType.Image:
                        XygloImageResource xir = new XygloImageResource(key, m_brazilContext.m_homePath + res.getFilePath());
                        xir.loadResource(m_context.m_graphics.GraphicsDevice);
                        m_context.m_xygloResourceMap.Add(key, xir);
                        Logger.logMsg("Loaded Image resource \"" + key + "\" from " + res.getFilePath());
                        break;

                    case ResourceType.Audio:
                        Logger.logMsg("Didn't load Audio resource");
                        break;

                    case ResourceType.Midi:
                        Logger.logMsg("Didn't load MIDI resource");
                        break;

                    case ResourceType.Video:
                        Logger.logMsg("Didn't load video resource");
                        break;

                    default:
                        Logger.logMsg("Unknown resource type");
                        break;
                }
            }
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
            m_context.m_fontManager.initialise(Content, "Bitstream Vera Sans Mono", GraphicsDevice.Viewport.AspectRatio, "Nuclex");
            if (m_context.m_project != null)
            {
                m_context.m_project.setFontManager(m_context.m_fontManager);
                loadFriendlierContent();
            }

            // We have to initialise this as follows to work around CA2000 warning
            //
            m_context.m_basicEffect = new BasicEffect(m_context.m_graphics.GraphicsDevice);
            m_context.m_basicEffect.TextureEnabled = true;
            m_context.m_basicEffect.VertexColorEnabled = true;
            m_context.m_basicEffect.World = Matrix.CreateScale(1, -1, 1);
            m_context.m_basicEffect.DiffuseColor = Vector3.One;
            //m_context.m_basicEffect.EnableDefaultLighting();

            // Create and initialize our effect
            //
            m_context.m_lineEffect = new BasicEffect(m_context.m_graphics.GraphicsDevice);
            m_context.m_lineEffect.VertexColorEnabled = true;
            m_context.m_lineEffect.TextureEnabled = false;
            m_context.m_lineEffect.DiffuseColor = Vector3.One;
            m_context.m_lineEffect.World = Matrix.CreateScale(1, -1, 1);
            //m_context.m_lineEffect.EnableDefaultLighting();


            //m_texture = Content.Load<Texture2D>("checker");
            m_texture = Content.Load<Texture2D>("red");
            m_context.m_physicsEffect = new BasicEffect(m_context.m_graphics.GraphicsDevice);
            //m_context.m_physicsEffect.VertexColorEnabled = false;
            m_context.m_physicsEffect.EnableDefaultLighting();
            m_context.m_physicsEffect.PreferPerPixelLighting = true;
            m_context.m_physicsEffect.SpecularColor = new Vector3(0.01f, 0.01f, 0.01f);
            m_context.m_physicsEffect.Texture = m_texture;
            //m_context.m_physicsEffect.Texture = m_context.m_flatTexture;
            m_context.m_physicsEffect.TextureEnabled = true;
            m_context.m_physicsEffect.World = Matrix.CreateScale(1, -1, 1);
            

            //texture = this.Game.Content.Load<Texture2D>("checker");
            //effect = new BasicEffect(this.GraphicsDevice);
            //effect.EnableDefaultLighting();
            //effect.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);
            //effect.World = Matrix.Identity;
            //effect.TextureEnabled = true;
            //effect.Texture = texture;

            // Create the overlay SpriteBatch
            //
            m_context.m_overlaySpriteBatch = new SpriteBatch(m_context.m_graphics.GraphicsDevice);

            // Load all the resources from current resource map
            //
            //loadResources(m_brazilContext.m_resourceMap);

            // Create a flat texture for drawing rectangles etc
            //
            Color[] foregroundColors = new Color[1];
            foregroundColors[0] = Color.White;
            m_context.m_flatTexture = new Texture2D(m_context.m_graphics.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            m_context.m_flatTexture.SetData(foregroundColors);

            // Set up the text scroller width
            //
            if (m_context.m_project != null)
            {
                setTextScrollerWidth(Convert.ToInt16(m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) * 32));
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

            // Initialise the DrawingHelper with our context
            //
            m_context.m_drawingHelper = new DrawingHelper(m_context, m_brazilContext, m_frameCounter, m_eyeHandler);

            // Setup the sprite font
            //
            if (m_context.m_project != null)
                m_context.m_drawingHelper.setSpriteFont();
        }


        protected Texture2D m_texture = null;

        /// <summary>
        /// Specific FriendlierContent to load
        /// </summary>
        protected void loadFriendlierContent()
        {
            m_context.m_splashScreen = Content.Load<Texture2D>("splash");

            m_smartHelpWorker = new SmartHelpWorker();
            m_smartHelpWorkerThread = new Thread(m_smartHelpWorker.startWorking);
            m_smartHelpWorkerThread.Start();

            // Loop until worker thread activates.
            //
            while (!m_smartHelpWorkerThread.IsAlive) ;
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
            if (m_context.m_project != null)
            {
                initialiseProject();
            }

            // Send the smart help worker to the keyboard handler so it
            // can call for updates.
            //
            m_keyboardHandler.setSmartHelpWorker(m_smartHelpWorker);

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
            //if (m_isResizable)
            //{
                this.Window.AllowUserResizing = true;
                this.Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
            //}

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
                m_graphics.fullScreenMode(this);
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
                changeHeight = changeWidth / m_context.m_fontManager.getAspectRatio();
                m_graphics.PreferredBackBufferHeight = (int)changeHeight;
            }
            else
            {
                changeWidth = changeHeight * m_context.m_fontManager.getAspectRatio();
                m_graphics.PreferredBackBufferWidth = (int)changeWidth;
            }
            m_graphics.ApplyChanges();
*/

            // Calculate new window size and resize all BufferViews accordingly
            //
            if (Window.ClientBounds.Width != m_lastWindowSize.X && Window.ClientBounds.Width > 0)
            {
                m_context.m_graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                m_context.m_graphics.PreferredBackBufferHeight = (int)(Window.ClientBounds.Width / m_context.m_fontManager.getAspectRatio());
            }
            else if (Window.ClientBounds.Height != m_lastWindowSize.Y && Window.ClientBounds.Height > 0)
            {
                m_context.m_graphics.PreferredBackBufferWidth = (int)(Window.ClientBounds.Height * m_context.m_fontManager.getAspectRatio());
                m_context.m_graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            }

            m_context.m_graphics.ApplyChanges();

            // Set up the Sprite font according to new size
            //
            m_context.m_drawingHelper.setSpriteFont();

            // Save these values
            //
            m_lastWindowSize.X = Window.ClientBounds.Width;
            m_lastWindowSize.Y = Window.ClientBounds.Height;

            // Store it in the project too
            //
            m_context.m_project.setWindowSize(m_lastWindowSize.X, m_lastWindowSize.Y);

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
            m_context.m_textScroller = new RenderTarget2D(m_context.m_graphics.GraphicsDevice, width, Convert.ToInt16(m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay)));
        }

        /// <summary>
        /// At a zoom level where we want to rotate and reset the active buffer
        /// </summary>
        /// <param name="direction"></param>
        protected void setActiveBuffer(BufferView.ViewCycleDirection direction)
        {
            m_context.m_project.getSelectedBufferView().rotateQuadrant(direction);
            setActiveBuffer();
        }

        /// <summary>
        /// Use another method to set the active BufferView
        /// </summary>
        /// <param name="item"></param>
        protected void setActiveBuffer(int item)
        {
            if (item >= 0 && item < m_context.m_project.getBufferViews().Count)
            {
                Logger.logMsg("XygloXNA::setActiveBuffer() - setting active BufferView " + item);
                setActiveBuffer(m_context.m_project.getBufferViews()[item]);
            }
        }

        /// <summary>
        /// Set which BufferView is the active one with a cursor in it
        /// </summary>
        /// <param name="view"></param>
        protected void setActiveBuffer(XygloView item = null)
        {
            try
            {
                // Either set the BufferView 
                if (item != null)
                {
                    m_context.m_project.setSelectedView(item);
                }
                else if (m_context.m_project.getViews().Count == 0) // Or if we have none then create one
                {
                    BufferView bv = new BufferView(m_context.m_fontManager);
                    FileBuffer fb = new FileBuffer();
                    m_context.m_project.addBufferView(bv);
                    m_context.m_project.addFileBuffer(fb);
                    bv.setFileBuffer(fb);
                }

                // Unset the view selection
                //
                m_context.m_project.setSelectedView(null);
            }
            catch (Exception e)
            {
                Logger.logMsg("Cannot locate BufferView item in list " + e.ToString());
                return;
            }

            Logger.logMsg("XygloXNA:setActiveBuffer() - active buffer view is " + m_context.m_project.getSelectedViewId());

            // Set the font manager up with a zoom level
            //
            m_context.m_fontManager.setScreenState(m_context.m_zoomLevel, m_context.m_project.isFullScreen());

            // Now recalculate positions
            //
            foreach (BufferView bv in m_context.m_project.getBufferViews())
            {
                bv.calculateMyRelativePosition();
            }

            // All the maths is done in the Buffer View
            //
            Vector3 eyePos = m_context.m_project.getSelectedView().getEyePosition(m_context.m_zoomLevel);
            m_eyeHandler.flyToPosition(eyePos);

            // Ensure that the m_brazilContext.m_interloper variable is valid
            // Ensure that the m_brazilContext.m_interloper is valid
            //
            validateInterloper();

            // Set title to include current filename
            // (this is not thread safe - we need to synchronise)
            //this.Window.Title = "Friendlier v" + VersionInformation.getProductVersion() + " - " + m_context.m_project.getSelectedView().getFileBuffer().getShortFileName();
        }

        /// <summary>
        /// Ensure that the m_brazilContext.m_interloper object is valid in the new context.
        /// </summary>
        protected void validateInterloper()
        {
            // Ensure we have a project
            //
            if (m_context.m_project == null) return;

            if (m_context.m_project.getSelectedView().GetType() == typeof(BrazilView))
            {
                BrazilView bv = (BrazilView)m_context.m_project.getSelectedView();
                List<BrazilInterloper> bI = bv.getApp().getComponents().Where(item => item.GetType() == typeof(BrazilInterloper)).Cast<BrazilInterloper>().ToList();
                
                // Now we need to check if there is a key for this interloper already - if so we can
                // set m_brazilContext.m_interloper.
                //
                if (bI.Count() > 0 && m_context.m_drawableComponents.ContainsKey(bI[0]))
                {
                    m_brazilContext.m_interloper = bI[0];

                    if (bI.Count() > 1)
                    {
                        Logger.logMsg("validateInterloper:: got more than one interloper from an App");
                    }

                    return;
                }
            }

            // Otherwise invalidate the m_brazilContext.m_interloper
            //
            m_brazilContext.m_interloper = null;

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
            if (m_context.m_project == null)
            {
                this.Exit();
                return;
            }

            // Only check BufferViews status if we're not forcing an exit
            //
            if (!force && m_keyboardHandler.getSaveAsExit() == false && m_context.m_project.getConfigurationValue("CONFIRMQUIT").ToUpper() == "TRUE")
            {
                if (m_brazilContext.m_confirmState.notEquals("ConfirmQuit"))
                {
                    setTemporaryMessage("Confirm quit? Y/N", 0, gameTime);
                    m_brazilContext.m_confirmState.set("ConfirmQuit");
                }

                if (m_keyboardHandler.getConfirmQuit() == false)
                    return;
            }

            // Check for unsaved like this
            //
            if (!force)
            {
                unsaved = (m_context.m_project.getFileBuffers().Where(item => item.isModified() == true).Count() > 0);
            }

            // Likewise only save if we want to
            //
            if (unsaved && !force)
            {
                if (m_brazilContext.m_confirmState.equals("FileSaveCancel"))
                {
                    setTemporaryMessage("", 1, gameTime);
                    m_brazilContext.m_confirmState.set("None");
                    return;
                }
                else
                {
                    setTemporaryMessage("Unsaved Buffers.  Save?  Y/N/C", 0, gameTime);
                    m_brazilContext.m_confirmState.set("FileSaveCancel");
                    m_keyboardHandler.setSaveAsExit(true);
                    m_brazilContext.m_state = State.Test("FileSaveAs");
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

                // Explicitly stop the analyser
                //
                if (m_systemAnalyser != null)
                    m_systemAnalyser.stop();

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
                if (m_eyeHandler.getEyePosition().Z == 600.0f)
                    m_eyeHandler.setEyePosition(new Vector3(m_eyeHandler.getEyePosition().X, m_eyeHandler.getEyePosition().Y, 500.0f));

                // Store the eye and target positions to the project before serialising it.
                //
                m_context.m_project.setEyePosition(m_eyeHandler.getEyePosition());
                m_context.m_project.setTargetPosition(m_eyeHandler.getTargetPosition());
                m_context.m_project.setOpenDirectory(m_context.m_fileSystemView.getPath());

                // Do some file management to ensure we have some backup copies
                //
                m_context.m_project.manageSerialisations();

                // Save our project including any updated file statuses
                //
                m_context.m_project.dataContractSerialise();

                this.Exit();
            }
        }

         /// <summary>
        /// Ensure that all components are reactivated before changing to that state
        /// </summary>
        /// <param name="newState"></param>
        protected void setState(string newState)
        {
            foreach (Component component in m_context.m_componentList)
                component.setDestroyed(false);

            m_brazilContext.m_state = State.Test(newState);
        }


        /// <summary>
        /// Handle gesture controller
        /// </summary>
        protected void handleGestures(GameTime gameTime)
        {
            // Sanity check
            //
            if (m_kinectWorker == null)
                return;

            foreach (System.EventArgs args in m_kinectWorker.getAllEvents())
            {
                // Handle swipe
                //
                if (args.GetType() == typeof(SwipeEventArgs))
                {
                    SwipeEventArgs swipe = (SwipeEventArgs)args;


                    // MOVE
                    //
                    Vector3 eyeDestination = m_eyeHandler.getEyePosition() + swipe.getDirection() * swipe.getSpeed() / 10.0f;
                    m_eyeHandler.flyToPosition(eyeDestination);

                    // Determine a location for the swipe and move towards it
                    //
                    //m_eyeHandler.getEyePosition(), m_eyeHandler.getTargetPosition()

                }
                else if (args.GetType() == typeof(ScreenTapEventArgs))
                {
                    Logger.logMsg("Got screen tap");
                }
                else if (args.GetType() == typeof(ScreenPositionEventArgs))
                {
                    ScreenPositionEventArgs pos = (ScreenPositionEventArgs)args;
                    //Logger.logMsg("Set pointer ghost X = " + pos.X() + ", Y = " + pos.Y());

                    bool found = false;
                    // First search for an existing temporary with the same id
                    //
                    foreach(BrazilTemporary temp in m_context.m_temporaryDrawables.Keys)
                    {
                        if (temp.getIndex() == pos.getId() && temp.getType() == BrazilTemporaryType.FingerPointer)
                        {
                            Vector3 position = m_context.m_drawingHelper.getScreenPlaneIntersection(pos.getScreenPosition());
                            m_context.m_temporaryDrawables[temp].setPosition(position);
                            found = true;
                        }
                        else if (temp.getIndex() == pos.getId() && temp.getAltIndex() == pos.getHandIndex() && temp.getType() == BrazilTemporaryType.FingerBone)
                        {
                            Vector3 position = (pos.getFingerStartPosition() + pos.getFingerEndPosition()) / 2;
                            XygloXnaDrawable drw = m_context.m_temporaryDrawables[temp];
                            drw.setPosition(m_context.m_drawingHelper.getScreenPlaneIntersection(position));

                            
                            //drw.setOrientation();
                            
                            found = true;
                        }
                    }

                    // If we've not found a temporary then create one
                    //
                    if (!found)
                    {
                        XygloFingerPointer pointer = new XygloFingerPointer(pos.getId(), m_context.m_lineEffect, XygloConvert.getBrazilVector3(m_context.m_drawingHelper.getScreenPlaneIntersection(pos.getScreenPosition())));

                        BrazilTemporary temp = new BrazilTemporary(BrazilTemporaryType.FingerPointer, pos.getId());
                        // Set drop dead as five seconds into the future
                        temp.setDropDead(gameTime.TotalGameTime.TotalSeconds + 0.5);
                        m_context.m_temporaryDrawables[temp] = pointer;

                        XygloFingerBone bone = new XygloFingerBone(pos.getId(), pos.getHandIndex().ToString(), m_context.m_lineEffect, m_context.m_drawingHelper.getScreenPlaneIntersection(pos.getFingerStartPosition()), m_context.m_drawingHelper.getScreenPlaneIntersection(pos.getFingerEndPosition()));
                        BrazilTemporary boneTemp = new BrazilTemporary(BrazilTemporaryType.FingerBone, pos.getId());
                        boneTemp.setAltIndex(pos.getHandIndex().ToString());
                        boneTemp.setDropDead(gameTime.TotalGameTime.TotalSeconds + 0.5);
                        m_context.m_temporaryDrawables[boneTemp] = bone;

                        Logger.logMsg("Finger count is now " + m_context.m_temporaryDrawables.Keys.Select(item => item.getType() == BrazilTemporaryType.FingerPointer).Count());
                    }


                    // WIDTH                        
                    //Window.ClientBounds.Width
                    /*
                    if (pos.X() >= Window.ClientBounds.X && (Window.ClientBounds.X + Window.ClientBounds.Width) <= pos.X() &&
                        pos.Y() >= Window.ClientBounds.Y && (Window.ClientBounds.Y + Window.ClientBounds.Height) <= pos.Y())
                    {
                        Logger.logMsg("Pointing at friendlier window");
                    }*/
                }

            }
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world, checking for collisions, gathering input, and playing audio.
        /// Also handles all the keypresses and other movemements.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void Update(GameTime gameTime)
        {
            // Do some frame counting
            //
            m_frameCounter.incrementElapsedTime(gameTime.ElapsedGameTime);

            if (m_frameCounter.getElapsedTime() > TimeSpan.FromMilliseconds(50))
                handleGestures(gameTime);

            if (m_frameCounter.getElapsedTime() > TimeSpan.FromSeconds(1))
                m_frameCounter.setFrameRate();

            // Do the main interpretation of our actions and ensure that we return
            // here if required.
            if (m_engine.interpretActions(gameTime, this, m_buildProcess))
                return;

            // Set the cursor to something useful
            //
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.IBeam;
            //System.Windows.Forms.Cursor.Hide();

            // Set the startup banner on the first pass through
            //
            if (m_context.m_project != null & m_context.m_gameTime == null)
                m_context.m_drawingHelper.startBanner(gameTime, VersionInformation.getProductName() + "\n" + VersionInformation.getProductVersion(), 5);

            // Store gameTime for use in helper functions
            //
            m_context.m_gameTime = gameTime;

            // Check for any mouse actions here
            //
            m_mouse.checkMouse(this, gameTime, m_keyboard, m_eyeHandler.getEyePosition(), m_eyeHandler.getTargetPosition());

            // Look for interloper at the edge of the screen
            //
            m_engine.checkInterloperBoundaries(gameTime);

            // Eye movement performed here - including auto panning in a game context
            //
            m_engine.performEyeMovement(gameTime);

            // Update all Xyglo/Brazil Components whether they be embedded in Friendlier or
            // free in their own app.
            ///
            m_context.m_drawingHelper.updateAllComponents();

            // Check for out of scope drawables
            //
            m_context.m_drawingHelper.checkForDestroyedDrawables();

            // Check for world escape and set as necessary
            //
            m_engine.checkWorldEscape();

            // Update physics
            //
            m_physicsHandler.update(gameTime);

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
            Dictionary<Component, XygloXnaDrawable> testDict = m_context.m_drawableComponents.Where(item => item.Key.isCorporeal() && item.Key != checkComponent).ToDictionary(p => p.Key, p => p.Value);

            XygloXnaDrawable checkComp = m_context.m_drawableComponents[checkComponent];

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
        /// Public accessor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="seconds"></param>
        public void setTemporaryMessage(string message, double seconds)
        {
            setTemporaryMessage(message, seconds, null);
        }

        /// <summary>
        ///  Set a temporary message until a given end time (seconds into the future)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="seconds"></param>
        /// <param name="gameTime"></param>
        protected void setTemporaryMessage(string message, double seconds, GameTime gameTime = null)
        {
            m_tempMessage.setTemporaryMessage(message, seconds, gameTime);
        }


        /// <summary>
        /// Add a new FileBuffer and a new BufferView and set this as active
        /// </summary>
        protected BufferView addNewFileBuffer(BufferView.ViewPosition position, string filename = null, bool readOnly = false, bool tailFile = false)
        {
            BufferView newBV = null;
            FileBuffer newFB = (filename == null ? new FileBuffer() : new FileBuffer(filename, readOnly));

            if (filename != null)
            {
                newFB.loadFile(m_context.m_project.getSyntaxManager());
            }

            // Add the FileBuffer and keep the index for our BufferView
            //
            int fileIndex = m_context.m_project.addFileBuffer(newFB);

            // Always assign a new bufferview to the right if we have one - else default position
            //
            Vector3 newPos = Vector3.Zero;
            if (m_context.m_project.getSelectedView() != null)
            {
                newPos = getFreeBufferViewPosition(position); // use the m_newPosition for the direction
            }

            newBV = new BufferView(m_context.m_fontManager, newFB, newPos, 0, 20, fileIndex, readOnly);
            newBV.setTailing(tailFile);
            m_context.m_project.addBufferView(newBV);

            // Set the background colour
            //
            newBV.setBackgroundColour(m_context.m_project.getNewFileBufferColour());

            // Only do the following if tailing
            //
            if (!tailFile)
            {
                // We've add a new file so regenerate the model
                //
                m_keyboardHandler.generateTreeModel();

                // Now generate highlighting
                //
                if (m_context.m_project.getConfigurationValue("SYNTAXHIGHLIGHT").ToUpper() == "TRUE")
                {
                    //m_context.m_project.getSyntaxManager().generateAllHighlighting(newFB, true);
                    m_smartHelpWorker.updateSyntaxHighlighting(m_context.m_project.getSyntaxManager(), newFB);
                }
            }

            return newBV;
        }

        protected Vector3 getFreeBufferViewPosition(BufferView.ViewPosition position)
        {
            return getFreeBufferViewBoundingBox(position).Min;
        }

        /// <summary>
        /// Find a free position around the active view
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        protected BoundingBox getFreeBufferViewBoundingBox(BufferView.ViewPosition position)
        {
            bool occupied = false;

            // Initial new pos is here from active BufferView
            //
            //Vector3 newPos = m_context.m_project.getSelectedView().calculateRelativePositionVector(position);
            BoundingBox bb = m_context.m_project.getSelectedView().calculateRelativePositionBoundingBox(position, XygloView.getDefaultBufferShowWidth(), XygloView.getDefaultBufferShowLength());

            do
            {
                occupied = false;

                foreach (BufferView cur in m_context.m_project.getBufferViews())
                {
                    BoundingBox curBB = cur.getSpacedBoundingBox();
                    while (cur.getBoundingBox().Intersects(bb))
                    {
                        // We get the next available slot in the same direction away from original
                        //
                        //bb = cur.calculateRelativePositionBoundingBox(position, XygloView.getDefaultBufferShowWidth(), XygloView.getDefaultBufferShowLength());

                        // Move the new bounding box away by position
                        switch (position)
                        {
                            case XygloView.ViewPosition.Left:
                                bb.Min.X -= m_context.m_fontManager.getCharWidth(XygloView.getDefaultViewSize());
                                bb.Max.X -= m_context.m_fontManager.getCharWidth(XygloView.getDefaultViewSize());
                                break;

                            case XygloView.ViewPosition.Right:
                                bb.Min.X += m_context.m_fontManager.getCharWidth(XygloView.getDefaultViewSize());
                                bb.Max.X += m_context.m_fontManager.getCharWidth(XygloView.getDefaultViewSize());
                                break;

                            case XygloView.ViewPosition.Above:
                                bb.Min.Y -= m_context.m_fontManager.getCharHeight(XygloView.getDefaultViewSize());
                                bb.Max.Y -= m_context.m_fontManager.getCharHeight(XygloView.getDefaultViewSize());
                                break;

                            case XygloView.ViewPosition.Below:
                                bb.Min.Y += m_context.m_fontManager.getCharHeight(XygloView.getDefaultViewSize());
                                bb.Max.Y += m_context.m_fontManager.getCharHeight(XygloView.getDefaultViewSize());
                                break;

                        }

                        occupied = true;
                        break;
                    }
                }
            } while (occupied);

            return bb;
        }

        /// <summary>
        /// Find a good position for a new BufferView relative to the current active position
        /// </summary>
        /// <param name="position"></param>
        protected void addBufferView(BufferView.ViewPosition position)
        {
            Vector3 newPos = getFreeBufferViewPosition(position);

            BufferView newBufferView = new BufferView(m_context.m_fontManager, m_context.m_project.getSelectedBufferView(), newPos);
            //newBufferView.m_textColour = Color.LawnGreen;
            m_context.m_project.addBufferView(newBufferView);
            setActiveBuffer(newBufferView);
        }

        /// <summary>
        /// Hook for handling temporary messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void handleTemporaryMessage(object sender, TextEventArgs e)
        {
            setTemporaryMessage(e.getText(), e.getDuration(), e.getGameTime());
        }

        /// <summary>
        /// Handle commands
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void handleCommand(object sender, CommandEventArgs e)
        {
            switch (e.getCommand())
            {
                case XygloCommand.Build:
                    doBuildCommand(e.getGameTime());
                    break;

                case XygloCommand.AlternateBuild:
                    doBuildCommand(e.getGameTime(), e.getArguments());
                    break;

                case XygloCommand.XygloClient:
                    if (e.getArguments() == "Paulo")
                    {
                        invokeBrazil(e.getGameTime());
                    }
                    else
                    {
                        throw new XygloException("handleCommand", "Unknown XygloClient");
                    }
                    break;

                case XygloCommand.XygloComponent:
                    if (e.getArguments() == "Test")
                    {
                        launchComponent();
                    }
                    else
                    {
                        throw new XygloException("handleCommand", "Unknown XygloComponent");
                    }
                    break;

                default:
                    throw new XygloException("handleCommand", "Unrecognised command event");
            }
        }

        /// <summary>
        /// Hook for flying to new position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void handleFlyToPosition(object sender, PositionEventArgs e)
        {
            m_eyeHandler.flyToPosition(e.getPosition());
        }

        /// <summary>
        /// Hook for Clean Exit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void handleCleanExit(object sender, CleanExitEventArgs e)
        {
            checkExit(e.getGameTime(), e.getForceExit());
        }

        /// <summary>
        /// Signal hook for changing BufferView and setting cursor position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void handleViewChange(object sender, XygloViewEventArgs e)
        {
            setActiveBuffer(e.getView());

            // If not "set active only" then also set position
            //
            if (!e.setActiveOnly())
            {
                m_context.m_project.getSelectedBufferView().mouseCursorTo(e.isExtendingHighlight(), e.getScreenPosition());
            }
        }

        /// <summary>
        /// Handle eye change event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eye"></param>
        /// <param name="target"></param>
        protected void handleEyeChange(object sender, PositionEventArgs eye, PositionEventArgs target)
        {
            m_eyeHandler.setEyePosition(eye.getPosition());
            m_eyeHandler.setTargetPosition(target.getPosition());
        }

        /// <summary>
        /// Handle new BufferView event.  This can happen in a variety of ways which we switch out
        /// here depending on required behaviour.   Some are new buffers, some copy, some existing files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void handleNewBufferView(object sender, NewBufferViewEventArgs e)
        {
            BufferView newBV;

            switch(e.getMode())
            {
                case NewBufferViewMode.ScreenPosition:
                    newBV = addNewFileBuffer(e.getViewPosition(), e.getFileName(), e.isReadOnly(), e.isTailing());
                    setHighlightAndCenter(newBV, e.getScreenPosition());
                    break;

                case NewBufferViewMode.Relative:
                    newBV = addNewFileBuffer(e.getViewPosition(), e.getFileName(), e.isReadOnly(), e.isTailing());
                    setActiveBuffer(newBV);
                    break;

                case NewBufferViewMode.NewBuffer:
                    newBV = addNewFileBuffer(e.getViewPosition());
                    setActiveBuffer(newBV);
                    break;

                case NewBufferViewMode.Copy:
                    newBV = new BufferView(e.getFontManager(), e.getSourceBufferView(), e.getViewPosition());
                    m_context.m_project.addBufferView(newBV);
                    setActiveBuffer(newBV);
                    break;

                default:
                    throw new XygloException("handleNewBufferView", "Don't recognise this add type for BufferViews");
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
        /// The XNA Draw override used to draw the game
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Increment the frame counter
            m_frameCounter.incrementFrameCounter();

            // Draw onto the Bloom component
            //
            m_context.m_bloom.BeginDraw();

            // Call setup for the projection matrix and frustrum etc.
            //
            setupDrawWorld(gameTime);

            // Are we drawing Friendlier - we cheat a bit here
            if (m_context.m_project != null)
                drawFriendlier(gameTime);

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
            m_context.m_graphics.GraphicsDevice.Clear(Color.Black);

            // If we are resizing then do nothing
            //
            //if (m_isResizing)
            //{
                //base.Draw(gameTime);
                //return;
            //}

            // Duplicate of what we have in the Initialize()
            //
            m_context.m_viewMatrix = Matrix.CreateLookAt(m_eyeHandler.getEyePosition(), m_eyeHandler.getTargetPosition(), Vector3.Up);
            m_context.m_projection = Matrix.CreateTranslation(-0.5f, -0.5f, 0) * Matrix.CreatePerspectiveFieldOfView(m_context.m_fov, GraphicsDevice.Viewport.AspectRatio, 0.1f, 10000f);

            // Generate frustrum
            //
            if (m_context.m_frustrum == null)
            {
                m_context.m_frustrum = new BoundingFrustum(m_context.m_viewMatrix * m_context.m_projection);
            }
            else
            {
                // You can also update frustrum matrix like this
                //
                m_context.m_frustrum.Matrix = m_context.m_viewMatrix * m_context.m_projection;
            }

            m_context.m_basicEffect.View = m_context.m_viewMatrix;
            m_context.m_basicEffect.Projection = m_context.m_projection;
            m_context.m_basicEffect.World = Matrix.CreateScale(1, -1, 1);

            m_context.m_lineEffect.View = m_context.m_viewMatrix;
            m_context.m_lineEffect.Projection = m_context.m_projection;
            m_context.m_lineEffect.World = Matrix.CreateScale(1, -1, 1);

            m_context.m_physicsEffect.View = m_context.m_viewMatrix;
            m_context.m_physicsEffect.Projection = m_context.m_projection;
            m_context.m_physicsEffect.World = Matrix.CreateScale(1, -1, 1);

        }

        /// <summary>
        /// Ensure that the correct texture is loaded into the BasicEffect
        /// </summary>
        /// <param name="component"></param>
        /// <param name="effect"></param>
        protected void loadComponentTexture(Component component, BasicEffect effect)
        {
            if (component.getResourceByType(ResourceType.Image).Count == 0)
                return;

            // Load the resource if it's not already available
            //
            //if (!m_context.m_xygloResourceMap.ContainsKey(component.getResources()[0].getResource().getName()))
            //{
                //loadResources(m_brazilContext.m_resourceMap);
            //}

            // Get the XygloResource using the unique name
            //
            XygloImageResource xir = (XygloImageResource)m_context.m_xygloResourceMap[component.getResources()[0].getResource().getName()];
            effect.Texture = xir.getTexture();
        }


        /// <summary>
        /// Draw the Xyglo Components
        /// </summary>
        /// <param name="gameTime"></param>
        protected void drawXyglo(GameTime gameTime, BrazilView view = null)
        {
            // Create/draw the components - note that we have two components called the same in different
            // areas of the framework - we should disambiguate to make sure this distinction between
            // API levels is clear.
            //
            // This loop generates the XNA level component to match the framework/Brazil level component
            // only if it doesn't already exist in the m_drawableComponents dictionary. If it does already
            // exist it just calls redraw on it.
            //
            List<Component> componentList = null;
            State state = null;
            if (view != null)
            {
                componentList = view.getApp().getComponents();
                state = view.getApp().getState();
            }
            else
            {
                componentList = m_context.m_componentList;
                state = m_brazilContext.m_state;
            }

            //m_spriteBatch.Begin();
            //m_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, m_basicEffect);

            foreach (Component component in componentList)
            {
                string compState = "";
                if (component.getStateActions().Count > 0)
                    compState = component.getStateActions().First().getState().m_name;

                // Check that this component should be showing for this State and that's it's not been destroyed
                //
                if ((!component.getStates().Contains(state) && !m_brazilContext.m_state.equals(compState)) || component.isDestroyed() || component.isHiding())
                    continue;

                // Has this component already been added to the drawableComponent dictionary?  If it hasn't then
                // we haven't drawn it yet.
                //
                if (!m_context.m_drawableComponents.ContainsKey(component))
                    m_factory.createInitialXygloDrawable(view, component);

                // If a component is not hiding then draw it
                //
                if (!(component is BrazilInvisibleBlock) && !(component is BrazilHud))
                {
                    loadComponentTexture(component, m_context.m_physicsEffect);
                    m_context.m_drawableComponents[component].draw(m_context.m_graphics.GraphicsDevice);
                }
            }

            // Now we can draw any temporary drawables:
            // List to remove
            //
            List<BrazilTemporary> deleteTemps = new List<BrazilTemporary>();

            foreach (BrazilTemporary temporary in m_context.m_temporaryDrawables.Keys)
            {
                // Update world matrix on physics affect
                //
                //m_context.m_physicsEffect.World = ;

                // Rebuild any buffers and draw
                //
                m_context.m_temporaryDrawables[temporary].buildBuffers(m_context.m_graphics.GraphicsDevice);
                m_context.m_temporaryDrawables[temporary].draw(m_context.m_graphics.GraphicsDevice);

                // Drop any temporaries that have exceeded their lifespan
                if (temporary.getDropDead() < gameTime.TotalGameTime.TotalSeconds)
                    deleteTemps.Add(temporary);
            }

            // Remove items marked for deletion
            //
            foreach (BrazilTemporary temporary in deleteTemps)
            {
                foreach (BrazilTemporary testTemp in m_context.m_temporaryDrawables.Keys)
                {
                    if (testTemp == temporary)
                    {
                        m_context.m_temporaryDrawables[testTemp] = null;
                        m_context.m_temporaryDrawables.Remove(testTemp);
                        break;
                    }
                }
            }

            // Now draw any previews in the panner.
            //
            // Draw overview of the all the XnaDrawables - compress all into a list and bung
            // over to work out the preview.
            //
            List<XygloXnaDrawable> overviewList = m_context.m_drawableComponents.Values.ToList();
            overviewList.AddRange(m_context.m_temporaryDrawables.Values.ToList());

            // Draw previews of temporary objects and the like
            //
            if (m_context.m_project != null)
            {
                m_context.m_overlaySpriteBatch.Begin();
                m_context.m_drawingHelper.drawXnaDrawableOverview(m_context.m_graphics.GraphicsDevice, gameTime, overviewList, m_context.m_overlaySpriteBatch);
                m_context.m_overlaySpriteBatch.End();
            }
        }

        /// <summary>
        /// Draw specific code for Friendlier special case
        /// </summary>
        /// <param name="gameTime"></param>
        protected void drawFriendlier(GameTime gameTime)
        {
            // Send any welcome message or temporary message if there are licencing issues
            //
            m_tempMessage.sendWelcomeMessage(gameTime, m_context.m_project.getLicenced());

            // In the manage project mode we zoom off into the distance
            //
            if (m_brazilContext.m_state.equals("ManageProject"))
            {
                m_context.m_drawingHelper.drawManageProject(m_context.m_overlaySpriteBatch, gameTime, m_keyboardHandler.getModelBuilder(), m_context.m_graphics, m_keyboardHandler.getConfigPosition());
                base.Draw(gameTime);
                return;
            }

            // Draw the FileBuffers
            //
            m_context.m_drawingHelper.drawFileBuffers(gameTime, this.IsActive, m_keyboardHandler, m_mouse, m_buildStdOutView, m_buildStdErrView);

            // Draw any Brazil views
            //
            foreach (BrazilView view in m_context.m_project.getBrazilViews())
                drawXyglo(gameTime, view);

            // Draw everything else - differs, panners, banners, system load, view map, config screens, help screens etc.
            //
            m_context.m_drawingHelper.drawScreenFluff(gameTime, m_keyboardHandler, m_keyboard, m_tempMessage, m_eyeHandler, m_systemAnalyser);

            // Any Kinect information to share
            //
            drawKinectInformation();
        }


        /// <summary>
        /// Do something with a Kinect controller if we have one
        /// </summary>
        protected void drawKinectInformation()
        {
#if GOT_KINECT
            if (m_kinectManager.gotUser())
            {
                string skeletonPosition = m_kinectManager.getSkeletonDetails();

                m_overlaySpriteBatch.Begin();

                // hardcode the font size to 1.0f so it looks nice
                //
                m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), skeletonPosition, new Vector2(50.0f, 50.0f), Color.White, 0, Vector2.Zero, 1.0f, 0, 0);
                //m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), eyePosition, new Vector2(0.0f, 0.0f), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
                //m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), modeString, new Vector2((int)modeStringXPos, 0.0f), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
                //m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), positionString, new Vector2((int)positionStringXPos, (int)yPos), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
                //m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), filePercentString, new Vector2((int)filePercentStringXPos, (int)yPos), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
                m_overlaySpriteBatch.End();
            }
#endif // GOT_KINECT
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
            if (m_kinectWorker != null || (m_systemAnalyser != null && m_systemAnalyser.isWorking()))
                checkExit(m_context.m_gameTime, true);

            //m_context.m_leapController.RemoveListener(m_context.m_leapListener);
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
                setTemporaryMessage("Checking build status", 3, m_context.m_gameTime);
                return;
            }

            Logger.logMsg("XygloXNA::doBuildCommand() - attempting to run build command");

            // Check that we can find the build command
            //
            try
            {
                //string[] commandList = m_context.m_project.getBuildCommand().Split(' ');
                string[] commandList = m_context.m_project.getConfigurationValue("BUILDCOMMAND").Split(' ');

                // Override the default build command
                //
                if (overrideString != "")
                {
                    commandList = overrideString.Split();
                }

                if (commandList.Length == 0)
                {
                    setTemporaryMessage("Build command not defined", 2, gameTime);
                    return;
                }
                

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

                    string buildCommand = commandList[0];

                    // We ensure that full path is given to build command at this time
                    //
                    if (!File.Exists(commandList[0]))
                    {
                        // Try again expanding the path
                        //
                        buildCommand = FileSystemHelper.FindExePath(commandList[0]);
                        if (!(File.Exists(buildCommand)))
                        {
                            setTemporaryMessage("Build command not found : \"" + commandList[0] + "\"", 5, gameTime);
                            return;
                        }
                    }

                    string buildDir = m_context.m_project.getConfigurationValue("BUILDDIRECTORY");
                    string buildStdOutLog = m_context.m_project.getConfigurationValue("BUILDSTDOUTLOG");
                    string buildStdErrLog = m_context.m_project.getConfigurationValue("BUILDSTDERRLOG");

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

                    m_buildStdErrView = m_context.m_project.findBufferView(buildStdErrLog);

                    if (m_buildStdErrView == null)
                    {
                        m_buildStdErrView = addNewFileBuffer(BufferView.ViewPosition.Right, buildStdErrLog, true, true);
                    }
                    m_buildStdErrView.setTailColour(Color.Orange);
                    m_buildStdErrView.noHighlight();

                    //m_buildStdErrView.setReadOnlyColour(Color.DarkRed);

                    // Store the line length of the existing file
                    //
                    m_context.m_project.setStdErrLastLine(m_buildStdErrView.getFileBuffer().getLineCount());

                    // If the build log doesn't exist then create it
                    //
                    if (!File.Exists(buildStdOutLog))
                    {
                        StreamWriter newStdOut = File.CreateText(buildStdOutLog);
                        newStdOut.Close();
                    }

                    // Now ensure that the build log is visible on the screen somewhere
                    //
                    m_buildStdOutView = m_context.m_project.findBufferView(buildStdOutLog);

                    if (m_buildStdOutView == null)
                    {
                        m_buildStdOutView = addNewFileBuffer(BufferView.ViewPosition.Right, buildStdOutLog, true, true);
                    }
                    m_buildStdOutView.noHighlight();

                    // Store the line length of the existing file
                    //
                    m_context.m_project.setStdOutLastLine(m_buildStdOutView.getFileBuffer().getLineCount());

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
                    info.FileName = m_context.m_project.getCommand(commandList);
                    info.WindowStyle = ProcessWindowStyle.Hidden;
                    info.CreateNoWindow = true;
                    //info.Arguments = m_context.m_project.getArguments() + (options == "" ? "" : " " + options);
                    info.Arguments = m_context.m_project.getArguments(commandList);
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
                    m_context.m_drawingHelper.startBanner(m_context.m_gameTime, "Build started", 5);

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
            catch (Exception e)
            {
                Logger.logMsg("Can't run command " + e.Message);

                // Disconnect the file handlers and the exit handler
                //
                if (m_buildProcess != null)
                {
                    m_buildProcess.OutputDataReceived -= new DataReceivedEventHandler(logBuildStdOut);
                    m_buildProcess.ErrorDataReceived -= new DataReceivedEventHandler(logBuildStdErr);
                    m_buildProcess.Exited -= new EventHandler(buildCompleted);
                }

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

            System.IO.TextWriter logFile = new StreamWriter(m_context.m_project.getConfigurationValue("BUILDLOG"), true);
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
            System.IO.TextWriter logFile = new StreamWriter(m_context.m_project.getConfigurationValue("BUILDLOG"), true);
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
                setTemporaryMessage("Build failed with exit code " + m_buildProcess.ExitCode, 5, m_context.m_gameTime);
                m_buildStdErrView.setTailColour(Color.Red);

                m_context.m_drawingHelper.startBanner(m_context.m_gameTime, "Build failed", 5);
            }
            else
            {
                setTemporaryMessage("Build completed successfully.", 3, m_context.m_gameTime);

                // Also colour the error log green
                //
                m_buildStdErrView.setTailColour(Color.Green);

                m_context.m_drawingHelper.startBanner(m_context.m_gameTime, "Build completed", 5);
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
                        newView = addNewFileBuffer(BufferView.ViewPosition.Right, newFile);
                        filesAdded.Add(newFile);
                    }
                }

                // Always set to the last added BufferView
                //
                if (newView != null)
                    setActiveBuffer(newView);

                // Build an intelligible temporary message after we've done this work
                //
                string message = "";

                if (filesAdded.Count > 0)
                {
                    message = filesAdded.Count + " file";

                    if (filesAdded.Count > 1)
                        message += "s";

                    message += " added ";

                    foreach (string fi in filesAdded)
                        message += " " + fi;
                }

                if (dirsAdded.Count > 0)
                {
                    if (message != "")
                        message += ", ";

                    message += dirsAdded.Count + " ";

                    if (dirsAdded.Count == 1)
                        message += "directory";
                    else
                        message += "directories";

                    message += " added";

                    foreach (string di in dirsAdded)
                        message += " " + di;
                }

                // Set the temporary message if we've generated one
                //
                if (message != "")
                    setTemporaryMessage(message, 5, m_context.m_gameTime);
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

        /// <summary>
        /// Go Brazil within Friendlier
        /// </summary>
        /// <param name="gameTime"></param>
        protected void invokeBrazil(GameTime gameTime)
        {
            setTemporaryMessage("Launching BrazilApp...", 2, gameTime);

            // Note that this uses the local BrazilPaulo which is a copy of the top-level Paulo
            // as we must avoid circular dependencies.  BrazilPaulo is of app type 'Hosted' so it
            // won't reinitialise XygloXna via the ViewSpace
            //
            BrazilApp app = new BrazilPaulo(new BrazilVector3(0, 0.1f, 0), @"..\..\..\..\..\..\projects\testproject\");

            // Now attach the container to this application at the right state for Friendlier - this is
            // the context in which the app itself will be shown and not the state context for the app to
            // run in.  The app container has its own state held internally.
            //
            //addComponent("TextEditing", container);

            // Initialise the app with a default state
            //
            app.initialise(State.Test("PlayingGame"));

            // Now we need to instantiate any resources and ensure that they are added to our current resource list
            //
            loadResources(app.getResources());

            // Now insert the app, inside the container into a BrazilView 
            //
            Vector3 position = m_context.m_project.getSelectedView().calculateRelativePositionVector(XygloView.ViewPosition.Below);
            BrazilView brazilView = new BrazilView(app, position);

            // Insert it into the project
            //
            int id = m_context.m_project.addBrazilView(brazilView);
            m_context.m_project.setSelectedViewId(id);
            app.getWorld().setWorldScale(XygloConvert.getBrazilBoundingBox(brazilView.getBoundingBox()));
            setActiveBuffer();
        }

        /// <summary>
        /// Launch an individual component - for the moment this is a test stub
        /// </summary>
        /// <param name="component"></param>
        protected void launchComponent()
        {
            BrazilTestBlock block = new BrazilTestBlock(BrazilColour.Pink, new BrazilVector3(0, 0, 0), new BrazilVector3(20, 20, 20));
            block.addState(m_brazilContext.m_state);
            m_context.m_componentList.Add(block);

            block = new BrazilTestBlock(BrazilColour.Pink, new BrazilVector3(-10, 100, 0), new BrazilVector3(20, 20, 20));
            block.addState(m_brazilContext.m_state);
            block.setAffectedByGravity(false);
            block.setMoveable(false);
            m_context.m_componentList.Add(block);
        }

        ///////////////// MEMBER VARIABLES //////////////////
        /// <summary>
        /// Temporary Message handler
        /// </summary>
        protected TemporaryMessage m_tempMessage;

        /// <summary>
        /// File system watcher
        /// </summary>
        protected List<FileSystemWatcher> m_watcherList = new List<FileSystemWatcher>();

        /// <summary>
        /// For system analysis we run PerformanceCounters in their own thread which
        /// is wrapped by this class.
        /// </summary>
        protected SystemAnalyser m_systemAnalyser;

        /// <summary>
        /// Handle all things to do with moving our eye position
        /// </summary>
        protected EyeHandler m_eyeHandler;

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
        protected Process m_buildProcess = null;

        /// <summary>
        /// Store the last window size in case we're resizing
        /// </summary>
        protected Vector2 m_lastWindowSize = Vector2.Zero;

        /// <summary>
        /// FrameCounter for counting frames
        /// </summary>
        protected FrameCounter m_frameCounter = new FrameCounter();

        /// <summary>
        /// Context holds our graphics and state context
        /// </summary>
        protected XygloContext m_context;

        /// <summary>
        /// The BrazilContext
        /// </summary>
        protected BrazilContext m_brazilContext;

        /// <summary>
        /// Holds our mouse handling code
        /// </summary>
        protected XygloMouse m_mouse;

        /// <summary>
        /// Keyboard manager
        /// </summary>
        protected XygloKeyboard m_keyboard;

        /// <summary>
        /// Keyboard handler
        /// </summary>
        protected XygloKeyboardHandler m_keyboardHandler;

        /// <summary>
        /// XygloGraphics helper
        /// </summary>
        protected XygloGraphics m_graphics;

        /// <summary>
        /// Physics handler
        /// </summary>
        protected PhysicsHandler m_physicsHandler = null;

        /// <summary>
        /// An eye perturber indeed
        /// </summary>
        protected EyePerturber m_eyePerturber = null;

        /// <summary>
        /// Handle for generating all of our Drawables and Physics related goods
        /// </summary>
        protected XygloFactory m_factory = null;

        /// <summary>
        /// Engine handles state transitions and actions
        /// </summary>
        protected XygloEngine m_engine = null;
    }
}
