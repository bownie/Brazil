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
    public class XygloXNA : Game, IBrazilApp
    {
        /////////////////////////////// CONSTRUCTORS ////////////////////////////
        /// <summary>
        /// Default constructor
        /// </summary>
        public XygloXNA(ActionMap actionMap, List<Component> componentList, BrazilWorld world, List<State> states, List<Target> targets)
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

            // Setup the XygloMouse
            //
            m_mouse = new XygloMouse(m_context, m_brazilContext);

            // Link signals up to the handlers
            //
            m_mouse.ChangePositionEvent += new PositionChangeEventHandler(handleFlyToPosition);
            m_mouse.BufferViewChangeEvent += new BufferViewChangeEventHandler(handleBufferViewChange);
            m_mouse.EyeChangeEvent += new EyeChangeEventHandler(handleEyeChange);
            m_mouse.NewBufferViewEvent += new NewBufferViewEventHandler(handleNewBufferView);
            m_mouse.TemporaryMessageEvent += new TemporaryMessageEventHandler(handleTemporaryMessage);

            // Keyboard wrapper class
            //
            m_keyboard = new XygloKeyboard(m_context, world.getKeyAutoRepeatHoldTime(), world.getKeyAutoRepeatInterval());

            // Keyboard handling class - performs interpretation of the key commands into
            // whatever we want to do.
            //
            m_keyboardHandler = new XygloKeyboardHandler(m_context, m_brazilContext, m_graphics, m_keyboard);
            m_keyboardHandler.TemporaryMessageEvent += new TemporaryMessageEventHandler(handleTemporaryMessage);
            m_keyboardHandler.BufferViewChangeEvent += new BufferViewChangeEventHandler(handleBufferViewChange);
            m_keyboardHandler.ChangePositionEvent += new PositionChangeEventHandler(handleFlyToPosition);
            m_keyboardHandler.CleanExitEvent += new CleanExitEventHandler(handleCleanExit);
            // FontManager
            //
            m_context.m_fontManager = new FontManager();

            // Graphics helper
            //
            m_graphics = new XygloGraphics(m_context);

            // Initialise
            //
            initialise();
        }

         /////////////////////////////// METHODS //////////////////////////////////////
        /// <summary>
        /// Set the project
        /// </summary>
        /// <param name="project"></param>
        public void setProject(Project project)
        {
            m_context.m_project = project;

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
        /// Check a State exists and throw up if not
        /// </summary>
        /// <param name="state"></param>
        protected State confirmState(string stateName)
        {
            foreach (State state in m_brazilContext.m_states)
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

            // Set physical memory
            //
            Microsoft.VisualBasic.Devices.ComputerInfo ci = new Microsoft.VisualBasic.Devices.ComputerInfo();
            m_physicalMemory = (float)(ci.TotalPhysicalMemory / (1024 * 1024));

            // Set windowed mode as default
            //
            m_graphics.windowedMode(this);

            // Check the demo status and set as necessary
            //
            if (m_context.m_project != null && !m_context.m_project.getLicenced())
            {
                m_brazilContext.m_state = State.Test("DemoExpired");
            }
        }

        /// <summary>
        /// Get the FileBuffer id of the active view
        /// </summary>
        /// <returns></returns>
        protected int getActiveBufferIndex()
        {
            return m_context.m_project.getFileBuffers().IndexOf(m_context.m_project.getSelectedBufferView().getFileBuffer());
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
            m_context.m_project.connectFloatingWorld();

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

            // Setup the sprite font
            //
            setSpriteFont();

            // Load all the files - if we have nothing in this project then create a BufferView
            // and a FileBuffer.
            //
            if (m_context.m_project.getFileBuffers().Count == 0)
            {
                addNewFileBuffer();
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
            //m_activeBufferView = m_context.m_project.getSelectedBufferView();

            // Ensure that we are in the correct position to view this buffer so there's no initial movement
            //
            m_eye = m_context.m_project.getEyePosition();
            m_target = m_context.m_project.getTargetPosition();
            m_context.m_zoomLevel = m_eye.Z;

            // Set the active buffer view
            //
            setActiveBuffer();

            // Set-up the single FileSystemView we have
            //
            if (m_context.m_project.getOpenDirectory() == "")
            {
                m_context.m_project.setOpenDirectory(@"C:\");  // set Default
            }

            m_context.m_fileSystemView = new FileSystemView(m_context.m_project.getOpenDirectory(), new Vector3(-800.0f, 0f, 0f), m_context.m_project, m_context.m_fontManager);

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
        /// Set the current main display SpriteFont to something in keeping with the resolution and reset some important variables.
        /// </summary>
        protected void setSpriteFont()
        {
            // Font loading - set our text size a bit fluffily at the moment
            //
            if (m_context.m_graphics.GraphicsDevice.Viewport.Width < 960)
            {
                m_context.m_fontManager.setFontState(FontManager.FontType.Small);
                Logger.logMsg("XygloXNA:setSpriteFont() - using Small Window font");
            }
            else if (m_context.m_graphics.GraphicsDevice.Viewport.Width < 1024)
            {
                m_context.m_fontManager.setFontState(FontManager.FontType.Medium);
                Logger.logMsg("XygloXNA:setSpriteFont() - using Window font");
            }
            else
            {
                Logger.logMsg("XygloXNA:setSpriteFont() - using Full font");
                m_context.m_fontManager.setFontState(FontManager.FontType.Large);
            }

            // to handle tabs for the moment convert them to single spaces
            //
            Logger.logMsg("XygloXNA:setSpriteFont() - you must get these three variables correct for each position to avoid nasty looking fonts:");
            Logger.logMsg("XygloXNA:setSpriteFont() - zoom level = " + m_context.m_zoomLevel);
            Logger.logMsg("XygloXNA:setSpriteFont() - recalculating relative positions");

            // Now recalculate positions
            //
            foreach (BufferView bv in m_context.m_project.getBufferViews())
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
            m_context.m_viewMatrix = Matrix.CreateLookAt(m_eye, m_target, Vector3.Up);
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

            // Create a new SpriteBatch, which can be used to draw textures.
            //
            m_context.m_spriteBatch = new SpriteBatch(m_context.m_graphics.GraphicsDevice);

            // Panner spritebatch
            //
            m_context.m_pannerSpriteBatch = new SpriteBatch(m_context.m_graphics.GraphicsDevice);
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
            m_context.m_basicEffect.World = Matrix.Identity;
            m_context.m_basicEffect.DiffuseColor = Vector3.One;

            // Create and initialize our effect
            //
            m_context.m_lineEffect = new BasicEffect(m_context.m_graphics.GraphicsDevice);
            m_context.m_lineEffect.VertexColorEnabled = true;
            m_context.m_lineEffect.TextureEnabled = false;
            m_context.m_lineEffect.DiffuseColor = Vector3.One;
            m_context.m_lineEffect.World = Matrix.Identity;

            // Create the overlay SpriteBatch
            //
            m_context.m_overlaySpriteBatch = new SpriteBatch(m_context.m_graphics.GraphicsDevice);

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
            m_context.m_drawingHelper = new DrawingHelper(m_context);
        }

        /// <summary>
        /// Specific FriendlierContent to load
        /// </summary>
        protected void loadFriendlierContent()
        {
            m_context.m_splashScreen = Content.Load<Texture2D>("splash");

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
            if (Window.ClientBounds.Width != m_lastWindowSize.X)
            {
                m_context.m_graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                m_context.m_graphics.PreferredBackBufferHeight = (int)(Window.ClientBounds.Width / m_context.m_fontManager.getAspectRatio());
            }
            else if (Window.ClientBounds.Height != m_lastWindowSize.Y)
            {
                m_context.m_graphics.PreferredBackBufferWidth = (int)(Window.ClientBounds.Height * m_context.m_fontManager.getAspectRatio());
                m_context.m_graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            }

            m_context.m_graphics.ApplyChanges();

            // Set up the Sprite font according to new size
            //
            setSpriteFont();

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
        /// This is the generic method for setting an active view - for the moment this
        /// sets an additional parameter.
        /// </summary>
        /// <param name="view"></param>
        protected void setActiveBuffer(XygloView view)
        {
            // All the maths is done in the Buffer View
            //
            Vector3 eyePos = view.getEyePosition(m_context.m_zoomLevel);
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
                    m_context.m_project.setSelectedBufferView(item);
                }
                else if (m_context.m_project.getBufferViews().Count == 0) // Or if we have none then create one
                {
                    BufferView bv = new BufferView(m_context.m_fontManager);
                    using (FileBuffer fb = new FileBuffer())
                    {
                        m_context.m_project.addBufferView(bv);
                        m_context.m_project.addFileBuffer(fb);
                        bv.setFileBuffer(fb);
                    }
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

            Logger.logMsg("XygloXNA:setActiveBuffer() - active buffer view is " + m_context.m_project.getSelectedBufferViewId());

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
            Vector3 eyePos = m_context.m_project.getSelectedBufferView().getEyePosition(m_context.m_zoomLevel);

            flyToPosition(eyePos);

            // Set title to include current filename
            // (this is not thread safe - we need to synchronise)
            //this.Window.Title = "Friendlier v" + VersionInformation.getProductVersion() + " - " + m_context.m_project.getSelectedBufferView().getFileBuffer().getShortFileName();
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
                m_context.m_project.setEyePosition(m_eye);
                m_context.m_project.setTargetPosition(m_target);
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
                    setTemporaryMessage("Cancel build? (Y/N)", 0, m_context.m_gameTime);
                    m_brazilContext.m_confirmState.set("CancelBuild");
                    return true;
                }

                if (m_brazilContext.m_confirmState.equals("ConfirmQuit"))
                {
                    m_brazilContext.m_confirmState.set("None");
                    setTemporaryMessage("Cancelled quit.", 1.0, gameTime);
                    m_brazilContext.m_state = State.Test("TextEditing");
                    return true;
                }

                // Depends where we are in the process here - check state
                //
                Vector3 newPosition = m_eye;

                switch (m_brazilContext.m_state.m_name)
                {
                    // These are FRIENDLIER states
                    //
                    case "TextEditing":
                        checkExit(gameTime);
                        break;

                    case "FileSaveAs":
                        setTemporaryMessage("Cancelled quit.", 0.5, gameTime);
                        m_brazilContext.m_confirmState.set("None");
                        m_brazilContext.m_state = State.Test("TextEditing");
                        m_keyboardHandler.setSaveAsExit(false);
                        //m_filesToWrite = null;
                        m_keyboardHandler.setFilesToWrite(null);
                        break;

                    case "ManageProject":
                        newPosition = m_context.m_project.getSelectedBufferView().getLookPosition();
                        newPosition.Z = 500.0f;
                        m_brazilContext.m_state = State.Test("TextEditing");
                        m_keyboardHandler.setEditConfigurationItem(false);
                        break;

                    case "DiffPicker":
                        m_brazilContext.m_state = State.Test("TextEditing");

                        // Before we clear the differ we want to translate the current viewed differ
                        // position back to the Bufferviews we originally generated them from.
                        //
                        BufferView bv1 = m_keyboardHandler.getDiffer().getSourceBufferViewLhs();
                        BufferView bv2 = m_keyboardHandler.getDiffer().getSourceBufferViewRhs();


                        // Ensure that these are valid
                        //
                        if (bv1 != null && bv2 != null)
                        {

                            bv1.setBufferShowStartY(m_keyboardHandler.getDiffer().diffPositionLhsToOriginalPosition(m_keyboardHandler.getDiffPosition()));
                            bv2.setBufferShowStartY(m_keyboardHandler.getDiffer().diffPositionRhsToOriginalPosition(m_keyboardHandler.getDiffPosition()));

                            ScreenPosition sp1 = new ScreenPosition();
                            ScreenPosition sp2 = new ScreenPosition();

                            sp1.X = 0;
                            sp1.Y = m_keyboardHandler.getDiffer().diffPositionLhsToOriginalPosition(m_keyboardHandler.getDiffPosition());

                            sp2.X = 0;
                            sp2.Y = m_keyboardHandler.getDiffer().diffPositionRhsToOriginalPosition(m_keyboardHandler.getDiffPosition());

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
                        if (m_keyboardHandler.getDiffer() != null)
                        {
                            m_keyboardHandler.getDiffer().clear();
                        }
                        break;

                    // Two stage exit from the Configuration edit
                    //
                    case "Configuration":
                        if (m_keyboardHandler.getEditConfigurationItem() == true)
                        {
                            m_keyboardHandler.setEditConfigurationItem(false);
                        }
                        else
                        {
                            m_brazilContext.m_state = State.Test("TextEditing");
                            m_keyboardHandler.setEditConfigurationItem(false);
                        }
                        break;

                    case "FileOpen":
                    case "Information":
                    case "PositionScreenOpen":
                    case "PositionScreenNew":
                    case "PositionScreenCopy":
                    case "SplashScreen":
                    case "Help":
                        m_brazilContext.m_state = State.Test("TextEditing");
                        m_keyboardHandler.setEditConfigurationItem(false);
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
            if ((m_brazilContext.m_state.equals("Information") || m_brazilContext.m_state.equals("Help") /* || m_state == State.ManageProject */ ) && m_changingEyePosition == false)
            {
                if (keyList.Contains(Keys.PageDown))
                {
                    m_keyboardHandler.textScreenPageDown(m_textScreenLength);
                }
                else if (keyList.Contains(Keys.PageUp))
                {
                    m_keyboardHandler.textScreenPageUp();
                }
                return true;
            }

            // This helps us count through our lists of file to save if we're trying to exit
            //
            if (m_keyboardHandler.getFilesToWrite() != null && m_keyboardHandler.getFilesToWrite().Count > 0)
            {
                m_context.m_project.setSelectedBufferView(m_keyboardHandler.getFilesToWrite()[0]);
                m_eye = m_context.m_project.getSelectedBufferView().getEyePosition();
                m_keyboardHandler.selectSaveFile(gameTime);
            }

            // For PositionScreen state we want not handle events here other than direction keys - this section
            // decides where to place a new, opened or copied BufferView.
            //
            if (m_brazilContext.m_state.equals("PositionScreenOpen") || m_brazilContext.m_state.equals("PositionScreenNew") || m_brazilContext.m_state.equals("PositionScreenCopy"))
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
                    if (m_brazilContext.m_state.equals("PositionScreenOpen"))
                    {
                        // Open the file 
                        //
                        BufferView newBV = addNewFileBuffer(m_keyboardHandler.getSelectedFile(), m_keyboardHandler.getFileIsReadOnly(), m_keyboardHandler.getFileIsTailing());
                        setActiveBuffer(newBV);
                        m_brazilContext.m_state = State.Test("TextEditing");
                    }
                    else if (m_brazilContext.m_state.equals("PositionScreenNew"))
                    {
                        // Use the convenience function
                        //
                        BufferView newBV = addNewFileBuffer();
                        setActiveBuffer(newBV);
                        m_brazilContext.m_state = State.Test("TextEditing");
                    }
                    else if (m_brazilContext.m_state.equals("PositionScreenCopy"))
                    {
                        // Use the copy constructor
                        //
                        BufferView newBV = new BufferView(m_context.m_fontManager, m_context.m_project.getSelectedBufferView(), m_newPosition);
                        m_context.m_project.addBufferView(newBV);
                        setActiveBuffer(newBV);
                        m_brazilContext.m_state = State.Test("TextEditing");
                    }
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Ensure that all components are reactivated before changing to that state
        /// </summary>
        /// <param name="newState"></param>
        protected void setState(string newState)
        {
            foreach (Component component in m_context.m_componentList)
            {
                component.setDestroyed(false);
            }

            m_brazilContext.m_state = State.Test(newState);
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
            if (m_frameCounter.getElapsedTime() > TimeSpan.FromSeconds(1))
                m_frameCounter.setFrameRate();

            // Update the frustrum matrix
            //
            if (m_context.m_frustrum != null)
            {
                m_context.m_frustrum.Matrix = m_context.m_viewMatrix * m_context.m_projection;
            }

            // Check for end states that require no further processing
            //
            if (m_brazilContext.m_state.equals("RestartLevel"))
            {
                m_context.m_drawableComponents.Clear();
                setState("PlayingGame");
            }

            if (m_brazilContext.m_state.equals("GameOver"))
            {
                m_context.m_drawableComponents.Clear();
            }

            // getAllKeyActions also works out the modifiers and applies them
            // to the KeyActions in the list.  This also sets the relevant shift,
            // alt, ctrl, windows flags.
            //
            List<KeyAction> keyActionList = m_keyboard.getAllKeyActions();

            // Do we consume a key?  Has it been used in a Metacommand?
            //
            //bool consume = false;

            foreach(KeyAction keyAction in keyActionList)
            {
                // We check and discard key events that aren't within the repeat or press
                // window at this point.  So we apply the same delays for all keys currently.
                //
                if (!m_keyboard.checkKeyRepeat(gameTime, keyAction))
                    continue;

                // Process action keys
                //
                if (m_context.m_project != null)
                {
                    //processActionKey(gameTime, keyAction);
                    m_keyboardHandler.processActionKey(gameTime, this, m_eye, keyAction);

                    // do any key combinations
                    //
                    //if (processCombinationsCommands(gameTime, keyActionList))
                        //continue;
                    if (m_keyboardHandler.processCombinationsCommands(gameTime, keyActionList))
                        continue;
                }

                // Get a target for this (potential) combination of keys
                //
                Target target = m_brazilContext.m_actionMap.getTargetForKey(m_brazilContext.m_state, keyAction);

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
                        m_keyboardHandler.processKey(gameTime, keyAction);
                        /* consume = */ processMetaCommand(gameTime, keyAction);
                        break;
                    
                        // For OpenFile all we need to do is change state (for the moment)
                        //
                    //case Target.OpenFile:
                    case "OpenFile":
                        m_brazilContext.m_state = State.Test("FileOpen");
                        break;

                    case "NewBufferView":
                        m_brazilContext.m_state = State.Test("PositionScreenNew");
                        break;

                    //case Target.SaveFile:
                    case "SaveFile":
                        m_keyboardHandler.selectSaveFile(gameTime);
                        break;

                    case "ShowInformation":
                        m_brazilContext.m_state = State.Test("Information");
                        break;

                    //case Target.CursorUp:
                    case "CursorUp":
                        switch (m_brazilContext.m_state.m_name)
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
                        m_keyboardHandler.processKey(gameTime, keyAction);
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
                        m_context.m_drawableComponents.Clear();
                        m_brazilContext.m_state = State.Test("Menu");

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
                            m_context.m_drawableComponents[m_interloper].moveLeft(1);
                        }
                        else // we're in free flight
                        {
                            m_context.m_drawableComponents[m_interloper].accelerate(leftVector);
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
                            m_context.m_drawableComponents[m_interloper].moveRight(1);
                        }
                        else // we're in free flight
                        {
                            m_context.m_drawableComponents[m_interloper].accelerate(rightVector);
                        }
                        break;

                        // Jump the interloper
                        //
                    case "Jump":
                        m_context.m_drawableComponents[m_interloper].jump(new Vector3(0, -4, 0));
                        break;

                    case "MoveForward":
                    case "MoveBack":
                        break;

                    default:
                        // In the default state just try to change state to the passed target
                        // i.e. Target = New State
                        //
                        // This will throw an exception if the target state isn't found
                        m_brazilContext.m_state = confirmState(target.m_name);
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
            if (m_context.m_project != null && !m_context.m_project.getLicenced())
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
            //System.Windows.Forms.Cursor.Hide();

            // Set the startup banner on the first pass through
            //
            if (m_context.m_project != null & m_context.m_gameTime == null)
            {
                m_context.m_drawingHelper.startBanner(gameTime, VersionInformation.getProductName() + "\n" + VersionInformation.getProductVersion(), 5);
            }

            // Store gameTime for use in helper functions
            //
            m_context.m_gameTime = gameTime;

            // Check for any mouse actions here
            //
            m_mouse.checkMouse(this, gameTime, m_keyboard, m_eye, m_target);

            // Check for this change as necessary
            //
            if (m_changingEyePosition)
            {
                // Restore the original eye position before moving anywhere
                //
                //if (m_eyePerturber != null)
                //{
                    //m_eye = XygloConvert.getVector3(m_eyePerturber.getInitialPosition());
                    //m_eyePerturber = null;
                //}

                changeEyePosition(gameTime);
            }
                /*
            else
            {
                // Perform some humanising/vomitising of the view depending on the effect..
                //
                if (m_eyePerturber == null)
                {
                    m_eyePerturber = new EyePerturber(XygloConvert.getBrazilVector3(m_eye), 5.0f, 5.0f, 10.0, gameTime.TotalGameTime.TotalSeconds);
                }

                m_eye = XygloConvert.getVector3(m_eyePerturber.getPerturbedPosition(gameTime.TotalGameTime.TotalSeconds));
            }*/

            // Process components for MOVEMENT or creation depending on key context
            //
            foreach (Component component in m_context.m_componentList)
            {
                // Has this component already been added to the drawableComponent dictionary?
                //
                if (m_context.m_drawableComponents.ContainsKey(component)) // && m_drawableComponents[component].getVelocity() != Vector3.Zero)
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
                            m_context.m_drawableComponents[component].accelerate(XygloConvert.getVector3(m_brazilContext.m_world.getGravity()));
                        }

                        // Move any update any buffers
                        //
                        //m_drawableComponents[component].move(XygloConvert.getVector3(fb.getVelocity()));
                        m_context.m_drawableComponents[component].moveDefault();

                        // Apply any rotation if we have one
                        if (fb.getRotation() != 0)
                        {
                            m_context.m_drawableComponents[component].incrementRotation(fb.getRotation());
                        }

                        m_context.m_drawableComponents[component].buildBuffers(m_context.m_graphics.GraphicsDevice);
                    }
                    else if (component.GetType() == typeof(Xyglo.Brazil.BrazilInterloper))
                    {
                        BrazilInterloper il = (Xyglo.Brazil.BrazilInterloper)component;

                        // Accelerate this object if there is gravity
                        //
                        if (il.isAffectedByGravity())
                        {
                            m_context.m_drawableComponents[component].accelerate(XygloConvert.getVector3(m_brazilContext.m_world.getGravity()));
                        }

                        // Check for collisions and adjust the position and velocity accordingly before drawing this
                        //
                        computeCollisions();
                        //{
                            // Move any update any buffers
                            //
                            //m_drawableComponents[component].move(XygloConvert.getVector3(il.getVelocity()));
                            m_context.m_drawableComponents[component].moveDefault();
                        //}

                        // Apply any rotation if we have one
                        if (il.getRotation() != 0)
                        {
                            m_context.m_drawableComponents[component].incrementRotation(il.getRotation());  // this is initial rotation only
                        }

                        m_context.m_drawableComponents[component].buildBuffers(m_context.m_graphics.GraphicsDevice);

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
                                m_context.m_drawableComponents[component].incrementRotation(bg.getRotation());
                                m_context.m_drawableComponents[component].buildBuffers(m_context.m_graphics.GraphicsDevice);
                            }
                        }
                        else
                        {
                            throw new XygloException("Update", "Unsupported Goody Type");
                        }


                        if (!m_context.m_drawableComponents[component].shouldBeDestroyed())
                        {
                            m_context.m_drawableComponents[component].draw(m_context.m_graphics.GraphicsDevice);
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
            Dictionary<Component, XygloXnaDrawable> destroyDict = m_context.m_drawableComponents.Where(item => item.Value.shouldBeDestroyed() == true).ToDictionary(p => p.Key, p => p.Value);
            foreach (Component destroyKey in destroyDict.Keys)
            {
                XygloXnaDrawable drawable = m_context.m_drawableComponents[destroyKey];
                m_context.m_drawableComponents.Remove(destroyKey);
                drawable = null;
                
                // Now set the Component to be destroyed so it's not recreated by the next event loop
                //
                destroyKey.setDestroyed(true);
            }

            // Check for world boundary escape
            //
            if (m_interloper != null && !XygloConvert.getBoundingBox(m_brazilContext.m_world.getBounds()).Intersects(m_context.m_drawableComponents[m_interloper].getBoundingBox()))
            {
                Logger.logMsg("Interloper has left the world");
                m_brazilContext.m_world.setLives(m_brazilContext.m_world.getLives() - 1);

                if (m_brazilContext.m_world.getLives() < 0)
                {
                    setState("GameOver");
                }
                else
                {
                    // We've got one less life - this effectively restarts the level from scratch
                    m_interloper = null;
                    m_context.m_drawableComponents.Clear();
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
        /// Check for collisions in m_context.m_drawableComponentss that have some form (hardness != 0).   Return true
        /// if we have a collision and we're also modifying the drawables to have correct velocity
        /// changes.
        /// </summary>
        protected bool computeCollisions()
        {
            // We'll have to iterate the realDict twice but here we filter in any goodies or baddies
            // and also anything that has a hardness (isCorporeal).
            //
            Dictionary<Component, XygloXnaDrawable> realDict = m_context.m_drawableComponents.Where(item => item.Key.isCorporeal() || item.Key.getBehaviour() != Behaviour.Goody).ToDictionary(p => p.Key, p => p.Value);

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
                newFB.loadFile(m_context.m_project.getSyntaxManager());
            }

            // Add the FileBuffer and keep the index for our BufferView
            //
            int fileIndex = m_context.m_project.addFileBuffer(newFB);

            // Always assign a new bufferview to the right if we have one - else default position
            //
            Vector3 newPos = Vector3.Zero;
            if (m_context.m_project.getSelectedBufferView() != null)
            {
                newPos = getFreeBufferViewPosition(m_newPosition); // use the m_newPosition for the direction
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
            Vector3 newPos = m_context.m_project.getSelectedBufferView().calculateRelativePositionVector(position);
            do
            {
                occupied = false;

                foreach (BufferView cur in m_context.m_project.getBufferViews())
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
        /// Hook for flying to new position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void handleFlyToPosition(object sender, PositionEventArgs e)
        {
            flyToPosition(e.getPosition());
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
        protected void handleBufferViewChange(object sender, BufferViewEventArgs e)
        {
            setActiveBuffer(e.getBufferView());

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
            m_eye = eye.getPosition();
            m_target = target.getPosition();
        }

        /// <summary>
        /// Handle new BufferView event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void handleNewBufferView(object sender, NewBufferViewEventArgs e)
        {
            BufferView newBv = addNewFileBuffer(e.getFileName());
            setHighlightAndCenter(newBv, e.getScreenPosition());
        }

        /// <summary>
        /// Move the eye to a new position - store the original one so we can tell how far along the path we've moved
        /// </summary>
        /// <param name="newPosition"></param>
        protected void flyToPosition(Vector3 newPosition)
        {
            // If we're already changing eye position then change the new eye position only
            //
            if (m_changingEyePosition)
            {
                // Set new destination
                //
                m_newEyePosition = newPosition;

                // Modify the vectors we need to aim to the new target
                //
                m_vFly = (m_newEyePosition - m_eye) / m_flySteps;

                // Also set up the target modification vector (where the eye is looking)
                //
                Vector3 tempTarget = m_newEyePosition;
                tempTarget.Z = 0.0f;
                m_vFlyTarget = (tempTarget - m_target) / m_flySteps;
            }
            else
            {
                // If we're currently stationary then we start moving like this
                //
                m_originalEyePosition = m_eye;
                m_newEyePosition = newPosition;
                m_changingPositionLastGameTime = TimeSpan.Zero;
                m_changingEyePosition = true;
            }
        }

        /// <summary>
        /// An eye perturber indeed
        /// </summary>
        protected EyePerturber m_eyePerturber = null;

        /// <summary>
        /// Transform current eye position to an intended eye position over time.  We use the orignal eye position to enable us
        /// to accelerate and deccelerate.  We have to modify both the eye position and the target that the eye is looking at
        /// by the same amount to keep our orientation constant.
        /// </summary>
        /// <param name="delta"></param>
        protected void changeEyePosition(GameTime gameTime)
        {
            if (!m_changingEyePosition) return;

            // Start of 
            if (m_changingPositionLastGameTime == TimeSpan.Zero)
            {
                m_vFly = (m_newEyePosition - m_eye) / m_flySteps;

                // Also set up the target modification vector (where the eye is looking)
                //
                Vector3 tempTarget = m_newEyePosition;
                tempTarget.Z = 0.0f;
                m_vFlyTarget = (tempTarget - m_target) / m_flySteps;

                // Want to enforce a minimum vector size here
                //
                while (m_vFlyTarget.Length() != 0 && m_vFlyTarget.Length() < 10.0f)
                {
                    m_vFlyTarget *= 2;
                }

                m_changingPositionLastGameTime = gameTime.TotalGameTime;
            }

            // At this point we have the m_vFly and m_vFlyTarget vectors loaded
            // with a fraction of the distance from source to target.  To begin
            // with we need to start slowly and accelerate smoothly.  We use an
            // acceleration (acc) which is based on the distance from source to 
            // target.  This is used as a multiplier on the movement vector to
            // provide the acceleration within bounds set by Max and Min.
            //
            float acc = 1.0f;
            float percTrack = (m_eye - m_originalEyePosition).Length() / (m_newEyePosition - m_originalEyePosition).Length();

            bool enableAcceleration = false;

            if (enableAcceleration)
            {
                // Need a notion of distance for the next movement
                //
                if (m_eye != m_originalEyePosition)
                {
                    if (percTrack < 0.5)
                    {
                        acc = percTrack;
                    }
                    else
                    {
                        acc = 1.0f - percTrack;
                    }

                    // Set absolute limits on acceleration
                    //
                    acc = Math.Max(acc, 0.12f);
                    acc = Math.Min(acc, 1.0f);
                }
            }

            // Perform movement of the eye by the movement vector and acceleration
            //
            if (gameTime.TotalGameTime - m_changingPositionLastGameTime > m_movementPause)
            {
                m_eye += m_vFly * acc;

                // modify target by the other vector (this is to keep our eye level constan
                //
                m_target.X += m_vFlyTarget.X * acc;
                m_target.Y += m_vFlyTarget.Y * acc;

                m_changingPositionLastGameTime = gameTime.TotalGameTime;
            }

            // Font scaling
            //
            m_keyboardHandler.doFontScaling(acc);

            // Test arrival of the eye at destination position
            //
            m_testArrived.Center = m_newEyePosition;
            m_testArrived.Radius = 5.0f;
            m_testArrived.Contains(ref m_eye, out m_testResult);

            if (m_testResult == ContainmentType.Contains)
            {
                m_eye = m_newEyePosition;
                m_target.X = m_newEyePosition.X;
                m_target.Y = m_newEyePosition.Y;
                m_changingEyePosition = false;
                m_keyboardHandler.setCurrentFontScale(1.0f);
            }
                /*
            else
            {
                float distanceToTarget = (m_newEyePosition - m_eye).Length();
                float distanceToTargetNext = (m_newEyePosition - m_eye + (m_vFly * acc)).Length();
                // Check for overshoots
                //
                //if (((m_newEyePosition - m_eye + (m_vFly * acc)).Length() > (m_newEyePosition - m_eye).Length()))
                if (distanceToTargetNext > distanceToTarget)
                {
                    Logger.logMsg("OVERSHOT");
                }
            }*/
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
            m_context.m_viewMatrix = Matrix.CreateLookAt(m_eye, m_target, Vector3.Up);
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
        }


        /// <summary>
        /// Draw the Xyglo Components
        /// </summary>
        /// <param name="gameTime"></param>
        protected void drawXyglo(GameTime gameTime, BrazilApp app = null)
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
            if (app != null)
            {
                componentList = app.getComponents();
                state = app.getState();
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
                if ((!component.getStates().Contains(state) && !m_brazilContext.m_state.equals(compState)) || component.isDestroyed())
                    continue;

                // Has this component already been added to the drawableComponent dictionary?  If it hasn't then
                // we haven't drawn it yet.
                //
                if (!m_context.m_drawableComponents.ContainsKey(component))
                {
                    // If not then is it a drawable type? 
                    //
                    if (component.GetType() == typeof(Xyglo.Brazil.BrazilFlyingBlock))
                    {
                        // Found a FlyingBlock - initialise it and add it to the dictionary
                        //
                        BrazilFlyingBlock fb = (Xyglo.Brazil.BrazilFlyingBlock)component;
                        XygloFlyingBlock drawBlock = new XygloFlyingBlock(XygloConvert.getColour(fb.getColour()), m_context.m_lineEffect, fb.getPosition(), fb.getSize());
                        drawBlock.setVelocity(XygloConvert.getVector3(fb.getVelocity()));

                        // Naming is useful for tracking these blocks
                        drawBlock.setName(fb.getName());

                        // Set any rotation amount
                        drawBlock.setRotation(fb.getInitialAngle());

                        // Initial build and draw
                        //
                        drawBlock.buildBuffers(m_context.m_graphics.GraphicsDevice);
                        drawBlock.draw(m_context.m_graphics.GraphicsDevice);

                        // Push to dictionary
                        //
                        m_context.m_drawableComponents[component] = drawBlock;
                        
                    } else if (component.GetType() == typeof(Xyglo.Brazil.BrazilInterloper))
                    {
                        BrazilInterloper il = (Xyglo.Brazil.BrazilInterloper)component;
#if ATTEMPT_ONE
                        XygloSphere drawSphere = new XygloSphere(XygloConvert.getColour(il.getColour()), m_lineEffect, il.getPosition(), 10.0f);
                        drawSphere.setRotation(il.getRotation());
#else
                        XygloComponentGroup group = new XygloComponentGroup(m_context.m_lineEffect, Vector3.Zero);
                        XygloFlyingBlock drawBlock = new XygloFlyingBlock(XygloConvert.getColour(il.getColour()), m_context.m_lineEffect, il.getPosition(), il.getSize());
                        group.addComponent(drawBlock);

                        // Set the name of the component group from the interloper
                        //
                        group.setName(il.getName());

                        XygloSphere drawSphere = new XygloSphere(XygloConvert.getColour(il.getColour()), m_context.m_lineEffect, il.getPosition(), il.getSize().X);
                        drawSphere.setRotation(il.getRotation());
                        group.addComponentRelative(drawSphere, new Vector3(0, - (float)il.getSize().X, 0));

                        group.buildBuffers(m_context.m_graphics.GraphicsDevice);
                        group.draw(m_context.m_graphics.GraphicsDevice);

                        //group.setVelocity(new Vector3(0.01f, 0, 0));

                        group.setVelocity(XygloConvert.getVector3(il.getVelocity()));
                        m_context.m_drawableComponents[component] = group;
#endif
                    }
                    else if (component.GetType() == typeof(Xyglo.Brazil.BrazilBannerText))
                    {
                        BrazilBannerText bt = (Xyglo.Brazil.BrazilBannerText)component;

                        // The helper method does all the hard work in getting this position
                        //
                        Vector3 position = XygloConvert.getTextPosition(bt, m_context.m_fontManager, m_context.m_graphics.GraphicsDevice.Viewport.Width, m_context.m_graphics.GraphicsDevice.Viewport.Height);
                        XygloBannerText bannerText = new XygloBannerText(m_context.m_overlaySpriteBatch, m_context.m_fontManager.getOverlayFont(), XygloConvert.getColour(bt.getColour()), position, bt.getSize(), bt.getText());
                        bannerText.draw(m_context.m_graphics.GraphicsDevice);
                    }
                    else if (component.GetType() == typeof(Xyglo.Brazil.BrazilHud))
                    {
                        BrazilHud bh = (Xyglo.Brazil.BrazilHud)component;
                        Vector3 position = XygloConvert.getVector3(bh.getPosition());

                        if (m_frameCounter.getFrameRate() > 0)
                        {
                            string fpsText = "FPS = " + m_frameCounter.getFrameRate();
                            XygloBannerText bannerText = new XygloBannerText(m_context.m_overlaySpriteBatch, m_context.m_fontManager.getOverlayFont(), XygloConvert.getColour(bh.getColour()), position, bh.getSize(), fpsText);
                            bannerText.draw(m_context.m_graphics.GraphicsDevice);
                        }

                        if (m_interloper != null)
                        {
                            // Interloper position
                            //
                            Vector3 ipPos = m_context.m_drawableComponents[m_interloper].getPosition();
                            string ipText = "Interloper Position X = " + ipPos.X + ", Y = " + ipPos.Y + ", Z = " + ipPos.Z;
                            XygloBannerText ipBanner = new XygloBannerText(m_context.m_overlaySpriteBatch, m_context.m_fontManager.getOverlayFont(), XygloConvert.getColour(BrazilColour.Blue), new Vector3(0, m_context.m_fontManager.getOverlayFont().LineSpacing, 0), 1.0f, ipText);
                            ipBanner.draw(m_context.m_graphics.GraphicsDevice);

                            // Interloper score
                            //
                            string ipScore = "Score = " + m_interloper.getScore();
                            XygloBannerText ipScoreText = new XygloBannerText(m_context.m_overlaySpriteBatch, m_context.m_fontManager.getOverlayFont(), XygloConvert.getColour(BrazilColour.Green), new Vector3(0, m_context.m_fontManager.getOverlayFont().LineSpacing * 2, 0), 1.0f, ipScore);
                            ipScoreText.draw(m_context.m_graphics.GraphicsDevice);

                            string ipLives = "Lives = " + m_brazilContext.m_world.getLives();
                            XygloBannerText ipLivesText = new XygloBannerText(m_context.m_overlaySpriteBatch, m_context.m_fontManager.getOverlayFont(), XygloConvert.getColour(BrazilColour.Green), new Vector3(0, m_context.m_fontManager.getOverlayFont().LineSpacing * 3, 0), 1.0f, ipLives);
                            ipLivesText.draw(m_context.m_graphics.GraphicsDevice);
                        }

                    } else if (component.GetType() == typeof(Xyglo.Brazil.BrazilGoody))
                    {
                        //Logger.logMsg("Draw Goody for the first time");
                        BrazilGoody bg = (BrazilGoody)component;

                        if (bg.m_type == BrazilGoodyType.Coin)
                        {
                            // Build a coin
                            //
                            XygloCoin coin = new XygloCoin(Color.Yellow, m_context.m_lineEffect, XygloConvert.getVector3(bg.getPosition()), bg.getSize().X);
                            coin.setRotation(bg.getRotation());
                            coin.buildBuffers(m_context.m_graphics.GraphicsDevice);
                            coin.draw(m_context.m_graphics.GraphicsDevice);

                            // And store in drawable component array
                            //
                            m_context.m_drawableComponents[component] = coin;
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
                        XygloMenu menu = new XygloMenu(m_context.m_fontManager, m_context.m_spriteBatch, Color.DarkGray, m_context.m_lineEffect, m_mouse.getLastClickWorldPosition(), m_mouse.geLastClickCursorOffset(), m_context.m_project.getSelectedBufferView().getViewSize());

                        foreach (BrazilMenuOption item in bMenu.getMenuOptions().Keys)
                        {
                            menu.addOption(item.m_optionName);
                        }

                        // Build the buffers and draw
                        //
                        menu.buildBuffers(m_context.m_graphics.GraphicsDevice);
                        menu.draw(m_context.m_graphics.GraphicsDevice);
                        m_context.m_drawableComponents[component] = menu;
                    }
                    else if (component.GetType() == typeof(Xyglo.Brazil.BrazilContainer))
                    {
                        BrazilContainer container = (BrazilContainer)component;
                        drawXyglo(gameTime, container.getApp());
                    }
                }
                else
                {
                    // If a component is currently hiding then update its position accordingly
                    //
                    if (!component.isHiding())
                    {
                        m_context.m_drawableComponents[component].draw(m_context.m_graphics.GraphicsDevice);
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

            foreach (BrazilTemporary temporary in m_context.m_temporaryDrawables.Keys)
            {
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
            m_context.m_drawingHelper.drawXnaDrawableOverview(m_context.m_graphics.GraphicsDevice, gameTime, overviewList);
        }

        /// <summary>
        /// Draw specific code for Friendlier special case
        /// </summary>
        /// <param name="gameTime"></param>
        protected void drawFriendlier(GameTime gameTime)
        {
            // If we're not licenced then render this
            //
            if (!m_context.m_project.getLicenced())
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
            if (m_brazilContext.m_state.equals("ManageProject"))
            {
                m_context.m_drawingHelper.drawManageProject(m_context.m_overlaySpriteBatch, gameTime, m_keyboardHandler.getModelBuilder(), m_context.m_graphics, m_keyboardHandler.getConfigPosition(), out m_textScreenLength);
                base.Draw(gameTime);
                return;
            }

            // Here we need to vary the parameters to the SpriteBatch - to the BasicEffect and also the font size.
            // For large fonts we need to be able to downscale them effectively so that they will still look good
            // at higher resolutions.
            //
            m_context.m_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, m_context.m_basicEffect);

            // Draw all the BufferViews for all remaining modes
            //
            for (int i = 0; i < m_context.m_project.getBufferViews().Count; i++)
            {
                if (m_keyboardHandler.getDiffer() != null && m_keyboardHandler.getDiffer().hasDiffs() &&
                    (m_keyboardHandler.getDiffer().getSourceBufferViewLhs() == m_context.m_project.getBufferViews()[i] ||
                        m_keyboardHandler.getDiffer().getSourceBufferViewRhs() == m_context.m_project.getBufferViews()[i]))
                {
                    m_context.m_drawingHelper.drawDiffBuffer(m_context.m_project.getBufferViews()[i], gameTime, m_keyboardHandler);
                }
                else
                {
                    // We have to invert the BoundingBox along the Y axis to ensure that
                    // it matches with the frustrum we're culling against.
                    //
                    BoundingBox bb = m_context.m_project.getBufferViews()[i].getBoundingBox();
                    bb.Min.Y = -bb.Min.Y;
                    bb.Max.Y = -bb.Max.Y;

                    // We only do frustrum culling for BufferViews for the moment
                    // - intersects might be too grabby but Disjoint didn't appear 
                    // to be grabby enough.
                    //
                    //if (m_context.m_frustrum.Intersects(bb))
                    if (m_context.m_frustrum.Contains(bb) != ContainmentType.Disjoint)
                    {
                        m_context.m_drawingHelper.drawFileBuffer(m_context.m_spriteBatch, m_context.m_project.getBufferViews()[i], gameTime, m_brazilContext.m_state, m_buildStdOutView, m_buildStdErrView, m_context.m_zoomLevel, m_keyboardHandler.getCurrentFontScale());
                    }

                    // Draw a background square for all buffer views if they are coloured
                    //
                    //if (m_context.m_project.getViewMode() == Project.ViewMode.Coloured)
                    //{
                        m_context.m_drawingHelper.renderQuad(m_context.m_project.getBufferViews()[i].getTopLeft(), m_context.m_project.getBufferViews()[i].getBottomRight(), m_context.m_project.getBufferViews()[i].getBackgroundColour(gameTime), m_context.m_spriteBatch);
                    //}
                }
            }

            // We only draw the scrollbar on the active view in the right mode
            //
            if (m_brazilContext.m_state.equals("TextEditing"))
            {
                drawScrollbar(m_context.m_project.getSelectedBufferView());
            }

            // Cursor and cursor highlight
            //
            if (m_brazilContext.m_state.equals("TextEditing"))
            {
                // Stop and use a different spritebatch for the highlighting and cursor
                //
                m_context.m_spriteBatch.End();
                m_context.m_spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, m_context.m_basicEffect);


                if (this.IsActive && m_brazilContext.m_confirmState.equals("None") && m_brazilContext.m_state.notEquals("FindText") && m_brazilContext.m_state.notEquals("GotoLine"))
                {
                    m_mouse.drawCursor(gameTime, m_context.m_spriteBatch);
                }
                
                m_context.m_drawingHelper.drawHighlight(gameTime, m_context.m_spriteBatch);
            }

            m_context.m_spriteBatch.End();

            // Draw our generic views
            //
            foreach (XygloView view in m_context.m_project.getGenericViews())
            {
                view.draw(m_context.m_project, m_brazilContext.m_state, gameTime, m_context.m_spriteBatch, m_context.m_basicEffect);
            }

            // If we're choosing a file then
            //
            if (m_brazilContext.m_state.equals("FileSaveAs") || m_brazilContext.m_state.equals("FileOpen") || m_brazilContext.m_state.equals("PositionScreenOpen") || m_brazilContext.m_state.equals("PositionScreenNew") || m_brazilContext.m_state.equals("PositionScreenCopy"))
            {
                drawDirectoryChooser(gameTime);

            }
            else if (m_brazilContext.m_state.equals("Help"))
            {
                // Get the text screen length back from the drawing method
                //
                m_textScreenLength = m_context.m_drawingHelper.drawHelpScreen(m_context.m_overlaySpriteBatch, gameTime, m_context.m_graphics, m_keyboardHandler.getTextScreenPositionY());
            }
            else if (m_brazilContext.m_state.equals("Information"))
            {
                m_context.m_drawingHelper.drawInformationScreen(m_context.m_overlaySpriteBatch, gameTime, m_context.m_graphics, out m_textScreenLength);
            }
            else if (m_brazilContext.m_state.equals("Configuration"))
            {
                m_context.m_drawingHelper.drawConfigurationScreen(gameTime, m_keyboardHandler);
            }
            else
            {
                // http://forums.create.msdn.com/forums/p/61995/381650.aspx
                //
                m_context.m_overlaySpriteBatch.Begin();

                // Draw the Overlay HUD
                //
                m_context.m_drawingHelper.drawOverlay(m_context.m_overlaySpriteBatch, gameTime, m_context.m_graphics, m_brazilContext.m_state, m_keyboardHandler.getGotoLine(), m_keyboard.isShiftDown(), m_keyboard.isCtrlDown(), m_keyboard.isAltDown(),
                                            m_eye, m_temporaryMessage, m_temporaryMessageStartTime, m_temporaryMessageEndTime);

                // Draw map of BufferViews - want to get rid of this way of doing things and
                // move it to the XnaDrawableOverview way.
                //
                m_context.m_drawingHelper.drawBufferViewMap(gameTime, m_context.m_overlaySpriteBatch);
                m_context.m_overlaySpriteBatch.End();

                // Draw any differ overlay
                //
                m_context.m_pannerSpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone /*, m_pannerEffect */ );

                // Draw the differ
                //
                m_context.m_drawingHelper.drawDiffer(gameTime, m_context.m_pannerSpriteBatch, m_brazilContext, m_keyboardHandler);

                // Draw system load
                //
                drawSystemLoad(gameTime, m_context.m_pannerSpriteBatch);

                m_context.m_pannerSpriteBatch.End();
            }

            // Draw the textures for generic views
            //
            //foreach (XygloView view in m_context.m_project.getGenericViews())
            //{
            //view.drawTextures(m_basicEffect);
            //}

            // Draw a welcome banner
            //
            if (m_context.m_drawingHelper.getBannerStartTime() != -1 && m_context.m_project.getViewMode() != Project.ViewMode.Formal)
            {
                m_context.m_drawingHelper.drawBanner(m_context.m_spriteBatch, gameTime, m_context.m_basicEffect, m_context.m_splashScreen);
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
        /// Draw the system CPU load and memory usage next to the FileBuffer
        /// </summary>
        /// <param name="gameTime"></param>
        protected void drawSystemLoad(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 startPosition = Vector2.Zero;
            int linesHigh = 6;

            // Bufferview
            BufferView bv = m_context.m_project.getSelectedBufferView();

            startPosition.X += m_context.m_graphics.GraphicsDevice.Viewport.Width - m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) * 3;
            startPosition.Y += (m_context.m_graphics.GraphicsDevice.Viewport.Height / 2) - m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay) * linesHigh / 2;

            float height = m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay) * linesHigh;
            float width = m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) / 2;

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
            m_context.m_drawingHelper.drawBox(spriteBatch, p1, p2, Color.DarkGray, 0.8f);

            // Draw CPU load over the top
            //
            p1 = startPosition;
            p2 = startPosition;

            p1.Y += height;
            p2.Y += height - (m_systemLoad * height / 100.0f);
            p1.X += 1;

            m_context.m_drawingHelper.drawBox(spriteBatch, p1, p2, Color.DarkGreen, 0.8f);

            // Draw background for Memory counter
            //
            startPosition.X += m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) * 0.5f;
            p1 = startPosition;
            p2 = startPosition;

            p1.Y += height;
            p1.X += 1;

            m_context.m_drawingHelper.drawBox(spriteBatch, p1, p2, Color.DarkGray, 0.8f);

            // Draw Memory over the top
            //
            p1 = startPosition;
            p2 = startPosition;

            p1.Y += height;
            p2.Y += height - (height * m_memoryAvailable / m_physicalMemory);
            p1.X += 1;

            m_context.m_drawingHelper.drawBox(spriteBatch, p1, p2, Color.DarkOrange, 0.8f);
            //m_pannerSpriteBatch.End();
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
            Vector3 startPosition = new Vector3((float)m_context.m_fontManager.getOverlayFont().MeasureString("X").X * 20,
                                                (float)m_context.m_fontManager.getOverlayFont().LineSpacing * 8,
                                                0.0f);


            if (m_brazilContext.m_state.equals("FileOpen"))
            {
                line = "Open file...";
            }
            else if (m_brazilContext.m_state.equals("FileSaveAs"))
            {
                line = "Save as...";
            }
            else if (m_brazilContext.m_state.equals("PositionScreenNew") || m_brazilContext.m_state.equals("PositionScreenOpen") || m_brazilContext.m_state.equals("PositionScreenCopy"))
            {
                line = "Choose a position...";
            }
            else
            {
                line = "Unknown State...";
            }

            // Overlay batch
            //
            m_context.m_overlaySpriteBatch.Begin();

            // Draw header line
            //
            m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), line, new Vector2((int)startPosition.X, (int)(startPosition.Y - m_context.m_project.getSelectedBufferView().getLineSpacing() * 3)), Color.White, 0, lineOrigin, 1.0f, 0, 0);

            // If we're using this method to position a new window only then don't show the directory chooser part..
            //
            if (m_brazilContext.m_state.equals("PositionScreenNew") || m_brazilContext.m_state.equals("PositionScreenCopy"))
            {
                m_context.m_overlaySpriteBatch.End();
                return;
            }

            Color dirColour = Color.White;

            startPosition.X += 50.0f;

            int lineNumber = 0;
            int dropStep = 6;

            // Page handling in the GUI
            //
            float showPage = 6.0f; // rows before stepping down
            int showOffset = (int)(((float)m_context.m_fileSystemView.getHighlightIndex()) / showPage);

            // This works out where the list that we're showing should end
            //
            int endShowing = (m_context.m_fileSystemView.getHighlightIndex() < dropStep ? dropStep : m_context.m_fileSystemView.getHighlightIndex()) + (int)showPage;


            // Draw the drives
            //
            if (m_context.m_fileSystemView.atDriveLevel())
            {
                DriveInfo[] driveInfo = m_context.m_fileSystemView.getDrives();
                //lineNumber = 0;

                foreach (DriveInfo d in driveInfo)
                {
                    if (!d.IsReady)
                    {
                        continue;
                    }

                    if (lineNumber > m_context.m_fileSystemView.getHighlightIndex() - dropStep
                        && lineNumber <= endShowing)
                    {
                        if (lineNumber < endShowing)
                        {
                            line = "[" + d.Name + "] " + d.VolumeLabel;
                        }
                        else
                        {
                            yPosition += m_context.m_fontManager.getOverlayFont().LineSpacing /* * 1.5f */;
                            line = "...";
                        }

                        m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(),
                             line,
                             new Vector2((int)startPosition.X, (int)(startPosition.Y + yPosition)),
                             (lineNumber == m_context.m_fileSystemView.getHighlightIndex() ? ColourScheme.getHighlightColour() : (lineNumber == endShowing ? Color.White : dirColour)),
                             0,
                             lineOrigin,
                             1.0f,
                             0, 0);

                        yPosition += m_context.m_fontManager.getOverlayFont().LineSpacing /* * 1.5f */;
                    }

                    lineNumber++;
                }
            }
            else // This is where we draw Directories and Files
            {
                if (!Directory.Exists(m_context.m_fileSystemView.getPath()))
                {
                    m_context.m_fileSystemView.setDirectory(@"C:\");
                }


                // For drives and directories we highlight item 1  - not zero
                //
                lineNumber = 1;
                FileInfo[] fileInfo = m_context.m_fileSystemView.getDirectoryInfo().GetFiles();
                DirectoryInfo[] dirInfo = m_context.m_fileSystemView.getDirectoryInfo().GetDirectories();

#if DIRECTORY_CHOOSER_DEBUG
                Logger.logMsg("showPage = " + showPage);
                Logger.logMsg("showOffset = " + showOffset);
                Logger.logMsg("m_directoryHighlight = " + m_directoryHighlight);
#endif

                line = m_context.m_fileSystemView.getPath() + m_keyboardHandler.getSaveFileName();
                m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), line, new Vector2((int)startPosition.X, (int)startPosition.Y), (m_context.m_fileSystemView.getHighlightIndex() == 0 ? ColourScheme.getHighlightColour() : dirColour), 0, lineOrigin, 1.0f, 0, 0);

                yPosition += m_context.m_fontManager.getOverlayFont().LineSpacing * 3.0f;

                foreach (DirectoryInfo d in dirInfo)
                {
                    if (lineNumber > m_context.m_fileSystemView.getHighlightIndex() - dropStep
                        && lineNumber <= endShowing)
                    {
                        if (lineNumber < endShowing)
                        {
                            line = "[" + d.Name + "]";
                        }
                        else
                        {
                            yPosition += m_context.m_fontManager.getOverlayFont().LineSpacing;
                            line = "...";
                        }

                        m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(),
                             line,
                             new Vector2(startPosition.X, startPosition.Y + yPosition),
                             (lineNumber == m_context.m_fileSystemView.getHighlightIndex() ? ColourScheme.getHighlightColour() : (lineNumber == endShowing ? Color.White : dirColour)),
                             0,
                             lineOrigin,
                             1.0f,
                             0, 0);

                        yPosition += m_context.m_fontManager.getOverlayFont().LineSpacing;
                    }

                    lineNumber++;
                }

                foreach (FileInfo f in fileInfo)
                {
                    if (lineNumber > m_context.m_fileSystemView.getHighlightIndex() - dropStep
                        && lineNumber <= endShowing)
                    {
                        if (lineNumber < endShowing)
                        {
                            line = f.Name;
                        }
                        else
                        {
                            yPosition += m_context.m_fontManager.getDefaultLineSpacing();
                            line = "...";
                        }

                        m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(),
                                                 line,
                                                 new Vector2((int)startPosition.X, (int)(startPosition.Y + yPosition)),
                                                 (lineNumber == m_context.m_fileSystemView.getHighlightIndex() ? ColourScheme.getHighlightColour() : (lineNumber == endShowing ? Color.White : ColourScheme.getItemColour())),
                                                 0,
                                                 lineOrigin,
                                                 1.0f,
                                                 0, 0);

                        yPosition += m_context.m_fontManager.getOverlayFont().LineSpacing/* * 1.5f */;
                    }
                    lineNumber++;
                }
            }

            if (m_temporaryMessageEndTime > gameTime.TotalGameTime.TotalSeconds && m_temporaryMessage != "")
            {
                // Add any temporary message on to the end of the message
                //
                m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(),
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
            m_context.m_overlaySpriteBatch.End();
        }


        /// <summary>
        /// Render some scrolling text to a texture.  This takes the current m_temporaryMessage and renders
        /// to a texture according to how much time has passed since the message was created.
        /// </summary>
        protected void renderTextScroller()
        {
            if (m_brazilContext.m_state.notEquals("TextEditing"))
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
            m_context.m_graphics.GraphicsDevice.SetRenderTarget(m_context.m_textScroller);
            m_context.m_graphics.GraphicsDevice.Clear(Color.Black);

            // Start with whole message showing and scroll it left
            //
            int newPosition = (int)((m_context.m_gameTime.TotalGameTime.TotalSeconds - m_temporaryMessageStartTime) * -speed);

            if ((newPosition + (int)(m_temporaryMessage.Length * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay))) < 0)
            {
                // Set the temporary message to start again and adjust position/time 
                // by width of the textScroller.
                //
                m_temporaryMessageStartTime = m_context.m_gameTime.TotalGameTime.TotalSeconds + m_context.m_textScroller.Width / speed;
            }

            // xPosition holds the scrolling position of the text in the temporary message window
            int xPosition = 0;
            float delayScroll = 0.7f; // delay the scrolling by this amount so we can read it before it starts moving

            if (m_context.m_gameTime.TotalGameTime.TotalSeconds - m_temporaryMessageStartTime > delayScroll)
            {
                xPosition = (int)((m_context.m_gameTime.TotalGameTime.TotalSeconds - delayScroll - m_temporaryMessageStartTime) * -120.0f);
            }

            // Draw to the render target
            //
            m_context.m_spriteBatch.Begin();
            m_context.m_spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), m_temporaryMessage, new Vector2((int)xPosition, 0), Color.Pink, 0, new Vector2(0, 0), 1.0f, 0, 0);
            m_context.m_spriteBatch.End();

            // Now reset the render target to the back buffer
            //
            m_context.m_graphics.GraphicsDevice.SetRenderTarget(null);
            m_context.m_textScrollTexture = (Texture2D)m_context.m_textScroller;
        }

        /// <summary>
        /// Draw a scroll bar for a BufferView
        /// </summary>
        /// <param name="view"></param>
        /// <param name="file"></param>
        protected void drawScrollbar(BufferView view)
        {
            Vector3 sbPos = view.getPosition();
            float height = view.getBufferShowLength() * m_context.m_fontManager.getLineSpacing(view.getViewSize());

            Rectangle sbBackGround = new Rectangle(Convert.ToInt16(sbPos.X - m_context.m_fontManager.getTextScale() * 30.0f),
                                                   Convert.ToInt16(sbPos.Y),
                                                   1,
                                                   Convert.ToInt16(height));

            // Draw scroll bar
            //
            m_context.m_spriteBatch.Draw(m_context.m_flatTexture, sbBackGround, Color.DarkCyan);

            // Draw viewing window
            //
            float start = view.getBufferShowStartY();

            // Override this for the diff view
            //
            if (m_keyboardHandler.getDiffer() != null && m_brazilContext.m_state.equals("DiffPicker"))
            {
                start = m_keyboardHandler.getDiffPosition();
            }

            float length = 0;

            // Get the line count
            //
            if (view.getFileBuffer() != null)
            {
                // Make this work for diff view as well as normal view
                //
                if (m_keyboardHandler.getDiffer() != null && m_brazilContext.m_state.equals("DiffPicker"))
                {
                    length = m_keyboardHandler.getDiffer().getMaxDiffLength();
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

                Rectangle sb = new Rectangle(Convert.ToInt16(sbPos.X - m_context.m_fontManager.getTextScale() * 30.0f),
                                             Convert.ToInt16(sbPos.Y + scrollStart),
                                             1,
                                             Convert.ToInt16(scrollLength));

                // Draw scroll bar window position
                //
                m_context.m_spriteBatch.Draw(m_context.m_flatTexture, sb, Color.LightGoldenrodYellow);
            }

            // Draw a highlight overview
            //
            if (view.gotHighlight() && length > 0)
            {
                //float hS = view.getHighlightStart().Y;

                float highlightStart = ((float)view.getHighlightStart().Y) / length * height;
                float highlightEnd = ((float)view.getHighlightEnd().Y) / length * height;

                Rectangle hl = new Rectangle(Convert.ToInt16(sbPos.X - m_context.m_fontManager.getTextScale() * 40.0f),
                                             Convert.ToInt16(sbPos.Y + highlightStart),
                                             1,
                                             Convert.ToInt16(highlightEnd - highlightStart));

                m_context.m_spriteBatch.Draw(m_context.m_flatTexture, hl, view.getHighlightColor());
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
                checkExit(m_context.m_gameTime, true);
            }
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
                            m_buildStdErrView = addNewFileBuffer(buildStdErrLog, true, true);
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
                            m_buildStdOutView = addNewFileBuffer(buildStdOutLog, true, true);
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
                    setTemporaryMessage(message, 5, m_context.m_gameTime);
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

        /// <summary>
        /// Start a Game within Friendlier
        /// </summary>
        /// <param name="gameTime"></param>
        protected void startGame(GameTime gameTime)
        {
            Logger.logMsg("Starting a Game");

            // Note that this uses the local BrazilPaulo which is a copy of the top-level Paulo
            // as we must avoid circular dependencies.  BrazilPaulo is of app type 'Hosted' so it
            // won't reinitialise XygloXna via the ViewSpace
            //
            BrazilApp app = new BrazilPaulo(new BrazilVector3(0, 0.1f, 0));

            // Now initialise a container with the BrazilApp inside it
            //
            BrazilContainer container = new BrazilContainer(app, new BrazilBoundingBox(new BrazilVector3(0, 0, 0), new BrazilVector3(600, 400, 10)));

            // Now attach the container to this application at the right state for Friendlier - this is
            // the context in which the app itself will be shown and not the state context for the app to
            // run in.  The app container has its own state held internally.
            //
            addComponent("TextEditing", container);

            // Initialise with default state
            //
            app.initialise(State.Test("PlayingGame"));
        }

        // The following methods are used to satisfy the IBrazilApp interface
        //

        /// <summary>
        /// Get a state
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public State getState(string stateName)
        {
            foreach (State state in m_brazilContext.m_states)
            {
                if (state.m_name == stateName)
                {
                    return state;
                }
            }

            throw new Exception("BrazilApp: state not defined " + stateName);
        }


        /// <summary>
        /// Add a Component with a given State - by state name
        /// </summary>
        /// <param name="component"></param>
        public void addComponent(string stateName, Component component)
        {
            State state = getState(stateName);
            addComponent(state, component);
        }

        /// <summary>
        /// Add a Component with a given State
        /// </summary>
        /// <param name="component"></param>
        public void addComponent(State state, Component component)
        {
            checkState(state);
            component.addState(state);
            m_context.m_componentList.Add(component);
        }

        /// <summary>
        /// Check a State exists
        /// </summary>
        /// <param name="state"></param>
        public void checkState(State state)
        {
            if (!m_brazilContext.m_states.Contains(state))
            {
                throw new Exception("Unrecognized state " + state.m_name);
            }
        }

        ///////////////// MEMBER VARIABLES //////////////////
        /// <summary>
        /// Eye/Camera location
        /// </summary>
        protected Vector3 m_eye = new Vector3(0f, 0f, 500f);  // 275 is good

        /// <summary>
        /// Camera target
        /// </summary>
        protected Vector3 m_target;

        /// <summary>
        /// Interloper object - our game 
        /// </summary>
        protected BrazilInterloper m_interloper = null;

        /// <summary>
        /// We can use this to communicate something to the user about the last command
        /// </summary>
        protected string m_temporaryMessage = "";

        /// <summary>
        /// Start time for the temporary message
        /// </summary>
        protected double m_temporaryMessageStartTime;

        /// <summary>
        /// End time for the temporary message
        /// </summary>
        protected double m_temporaryMessageEndTime;

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
        protected int m_flySteps = 10;

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
        /// Length of information screen - so we know if we can page up or down
        /// </summary>
        protected int m_textScreenLength = 0;

        /// <summary>
        /// Testing whether arrived in bounding sphere
        /// </summary>
        protected BoundingSphere m_testArrived = new BoundingSphere();

        /// <summary>
        /// Test result
        /// </summary>
        protected ContainmentType m_testResult;

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
    }
}
