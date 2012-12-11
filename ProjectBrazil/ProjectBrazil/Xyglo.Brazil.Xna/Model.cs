using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Currently not used for anything
    /// </summary>
    public class Model
    {
        /*
        /// Projection Matrix
        /// </summary>
        protected Matrix m_projection;

        /// <summary>
        /// Our view matrix
        /// </summary>
        protected Matrix m_viewMatrix = new Matrix();

        /// <summary>
        /// A bounding frustrum to allow us to cull objects not visible
        /// </summary>
        protected BoundingFrustum m_frustrum;

        /// <summary>
        /// Frame rate
        /// </summary>
        protected int m_frameRate = 0;

        /// <summary>
        /// Frame counter
        /// </summary>
        protected int m_frameCounter = 0;

        /// <summary>
        /// The state of our application - what we're doing at the moment
        /// </summary>
        protected State m_state;

        /// <summary>
        /// Elapse time to help calculate frame rate
        /// </summary>
        TimeSpan m_elapsedTime = TimeSpan.Zero;

        /// <summary>
        /// A drawable, keyed component dictionary
        /// </summary>
        protected Dictionary<Component, XygloXnaDrawable> m_drawableComponents = new Dictionary<Component, XygloXnaDrawable>();

        /// <summary>
        /// A list of temporary Drawables - everything on here must have a time to live set for them
        /// or a scope defined to get rid of them from this list.
        /// </summary>
        protected Dictionary<BrazilTemporary, XygloXnaDrawable> m_temporaryDrawables = new Dictionary<BrazilTemporary, XygloXnaDrawable>();

        public Model()
        {
        }
        */

        /*
        public void update(GameTime gameTime)
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

            foreach (KeyAction keyAction in keyActionList)
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
                        // consume =
                        processMetaCommand(gameTime, keyAction);
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
                        // consume =
                        processMetaCommand(gameTime, keyAction);
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
        }
        */
    }
}
