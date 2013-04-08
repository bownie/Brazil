using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xyglo.Brazil.Xna.Physics;
using Microsoft.Xna.Framework;
using System.Threading;
using System.Diagnostics;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// The XygloEngine defines what happens when events interact with the physics/graphics.  The engine
    /// provides state transitions and also sends events to objects.
    /// 
    /// Emits:
    /// 
    /// - OnNewBufferViewEvent
    /// - OnCleanExitEvent
    /// - OnTemporaryMessage
    /// 
    /// </summary>
    public class XygloEngine : XygloEventEmitter
    {

        public XygloEngine(XygloContext context, BrazilContext brazilContext, 
                           XygloKeyboard keyboard, XygloKeyboardHandler keyboardHandler, EyeHandler eyeHandler)
        {
            m_context = context;
            m_brazilContext = brazilContext;
            m_keyboard = keyboard;
            m_keyboardHandler = keyboardHandler;
            m_eyeHandler = eyeHandler;
        }

        /// <summary>
        /// This is the main method for fetching actions and interpreting them in our state model.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="game"></param>
        /// <param name="buildProcess"></param>
        public bool interpretActions(GameTime gameTime, Game game, Process buildProcess)
        {
            // Update the frustrum matrix
            //
            if (m_context.m_frustrum != null)
                m_context.m_frustrum.Matrix = m_context.m_viewMatrix * m_context.m_projection;

            // Check for end states that require no further processing
            //
            if (m_context.m_project.getSelectedView().GetType() == typeof(BrazilView))
            {
                BrazilView brazilView = (BrazilView)m_context.m_project.getSelectedView();

                if (m_brazilContext.m_state.equals("RestartLevel"))
                {
                    // Clear drawables and any physics simulations
                    //
                    //m_context.m_physicsHandler.clearAppComponents(brazilView.getApp().getComponents());
                    m_context.m_physicsHandler.clearAll();

                    // Remove drawables
                    //
                    foreach (Component component in brazilView.getApp().getComponents())
                    {
                        m_context.m_drawableComponents[component] = null;
                        m_context.m_drawableComponents.Remove(component);
                    }

                    setState("PlayingGame");
                }

                if (m_brazilContext.m_state.equals("GameOver"))
                    m_context.m_drawableComponents.Clear();
            }

            // getAllKeyActions also works out the modifiers and applies them
            // to the KeyActions in the list.  This also sets the relevant shift,
            // alt, ctrl, windows flags.
            //
            List<KeyAction> keyActionList = m_keyboard.getAllKeyActions();

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
                    // Check and continue if consumed
                    //
                    if (m_keyboardHandler.processActionKey(gameTime, game, m_eyeHandler.getEyePosition(), keyAction))
                        continue;

                    // Check and continue if consumed
                    //
                    if (m_keyboardHandler.processCombinationsCommands(gameTime, keyActionList, m_eyeHandler))
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
                        if (processMetaCommand(gameTime, keyAction, buildProcess))
                            continue;

                        if (m_context.m_project != null && m_context.m_project.getSelectedView().GetType() == typeof(BrazilView))
                        {
                            if (m_keyboardHandler.processBrazilViewKey(gameTime, keyAction))
                                continue;
                        }
                        else
                        {
                            if (m_keyboardHandler.processBufferViewKey(gameTime, keyAction))
                                continue;
                        }

                        
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

                        // The default target will process meta key commands
                        //
                    case "Exit":
                        if (m_context.m_project != null && m_context.m_project.getSelectedView().GetType() == typeof(BrazilView))
                            m_keyboardHandler.processBrazilViewKey(gameTime, keyAction);
                        else
                            m_keyboardHandler.processBufferViewKey(gameTime, keyAction);

                        processMetaCommand(gameTime, keyAction, buildProcess);
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
                        m_brazilContext.m_interloper = null;
                        break;

                    case "MoveLeft":
                        // accelerate will accelerate in mid air or move
                        if (m_brazilContext.m_interloper != null)
                            m_context.m_physicsHandler.accelerate(m_context.m_drawableComponents[m_brazilContext.m_interloper], new Vector3(-10, 0, 0));

                        break;

                    case "MoveRight":
                        // accelerate will accelerate in mid air or move
                        if (m_brazilContext.m_interloper != null)
                            m_context.m_physicsHandler.accelerate(m_context.m_drawableComponents[m_brazilContext.m_interloper], new Vector3(10, 0, 0));
                        break;

                        // Jump the interloper
                        //
                    case "Jump":
                        if (m_brazilContext.m_interloper != null)
                            m_context.m_physicsHandler.accelerate(m_context.m_drawableComponents[m_brazilContext.m_interloper], new Vector3(0, -200, 0));
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
                keyList.Add(keyAction.m_key);

            // Return after these commands have been processed for the demo version
            //
            if (m_context.m_project != null && !m_context.m_project.getLicenced())
            {
                // Allow the game to exit
                //
                if (keyList.Contains(Keys.Escape))
                {
                    //checkExit(gameTime, true);
                    OnCleanExitEvent(new CleanExitEventArgs(gameTime, true));
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Process some meta commands as part of our update statement
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        protected bool processMetaCommand(GameTime gameTime, KeyAction keyAction, Process buildProcess)
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
                if (buildProcess != null)
                {
                    //setTemporaryMessage("Cancel build? (Y/N)", 0, m_context.m_gameTime);
                    OnTemporaryMessage(new TextEventArgs("Cancel build? (Y/N)", 0, gameTime));
                    m_brazilContext.m_confirmState.set("CancelBuild");
                    return true;
                }

                if (m_brazilContext.m_confirmState.equals("ConfirmQuit"))
                {
                    m_brazilContext.m_confirmState.set("None");
                    //setTemporaryMessage("Cancelled quit.", 1.0, gameTime);
                    OnTemporaryMessage(new TextEventArgs("Cancelled quit.", 1.0, gameTime));
                    m_brazilContext.m_state = State.Test("TextEditing");
                    return true;
                }

                // Depends where we are in the process here - check state
                //
                Vector3 newPosition = m_eyeHandler.getEyePosition(); ;

                switch (m_brazilContext.m_state.m_name)
                {
                    // These are FRIENDLIER states
                    //
                    case "TextEditing":
                        //checkExit(gameTime);
                        OnCleanExitEvent(new CleanExitEventArgs(gameTime));
                        break;

                    case "FileSaveAs":
                        //setTemporaryMessage("Cancelled quit.", 0.5, gameTime);
                        OnTemporaryMessage(new TextEventArgs("Cancelled quit.", 0.5, gameTime));
                        m_brazilContext.m_confirmState.set("None");
                        m_brazilContext.m_state = State.Test("TextEditing");
                        m_keyboardHandler.setSaveAsExit(false);
                        //m_filesToWrite = null;
                        m_keyboardHandler.setFilesToWrite(null);
                        break;

                    case "ManageProject":
                        newPosition = m_context.m_project.getSelectedView().getEyePosition();
                        //newPosition.Z = 500.0f;
                        //newPosition = m_eyeHandler.getEyePosition();
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
                    case "ProjectOpen":
                    case "Help":
                        m_brazilContext.m_state = State.Test("TextEditing");
                        m_keyboardHandler.setEditConfigurationItem(false);
                        break;

                    // These are PAULO states
                    // 
                    case "Menu":
                        //checkExit(gameTime);
                        OnCleanExitEvent(new CleanExitEventArgs(gameTime));
                        break;

                    default:
                        // Ummmm??
                        //
                        m_brazilContext.m_state = State.Test("TextEditing");
                        break;
                }

                // Cancel any temporary message
                //
                //m_temporaryMessageEndTime = gameTime.TotalGameTime.TotalSeconds;

                // Fly back to correct position
                //
                m_eyeHandler.flyToPosition(newPosition);
            }

            // If we're viewing some information then only escape can get us out
            // of this mode.  Note that we also have to mind any animations so we
            // also want to ensure that m_changingEyePosition is not true.
            //
            if ((m_brazilContext.m_state.equals("Information") || m_brazilContext.m_state.equals("Help") /* || m_state == State.ManageProject */ ) && m_eyeHandler.isChangingPosition() == false)
            {
                if (keyList.Contains(Keys.PageDown))
                    m_keyboardHandler.textScreenPageDown(m_context.m_drawingHelper.getLastDrawTextScreenLength());
                else if (keyList.Contains(Keys.PageUp))
                    m_keyboardHandler.textScreenPageUp();

                return true;
            }

            // This helps us count through our lists of file to save if we're trying to exit
            //
            if (m_keyboardHandler.getFilesToWrite() != null && m_keyboardHandler.getFilesToWrite().Count > 0)
            {
                m_context.m_project.setSelectedViewByFileBuffer(m_keyboardHandler.getFilesToWrite()[0]);
                m_eyeHandler.setEyePosition(m_context.m_project.getSelectedView().getEyePosition());
                m_keyboardHandler.selectSaveFile(gameTime);
            }

            // For PositionScreen state we want not handle events here other than direction keys - this section
            // decides where to place a new, opened or copied BufferView.
            //
            BufferView.ViewPosition position = XygloView.ViewPosition.Above;

            if (m_brazilContext.m_state.equals("PositionScreenOpen") || m_brazilContext.m_state.equals("PositionScreenNew") || m_brazilContext.m_state.equals("PositionScreenCopy"))
            {
                bool gotSelection = false;

                if (keyList.Contains(Keys.Left))
                {
                    position = BufferView.ViewPosition.Left;
                    gotSelection = true;
                }
                else if (keyList.Contains(Keys.Right))
                {
                    position = BufferView.ViewPosition.Right;
                    //return true;
                    gotSelection = true;
                }
                else if (keyList.Contains(Keys.Up))
                {
                    position = BufferView.ViewPosition.Above;
                    gotSelection = true;
                }
                else if (keyList.Contains(Keys.Down))
                {
                    position = BufferView.ViewPosition.Below;
                    gotSelection = true;
                }

                // If we have discovered a position for our pending new window
                //
                if (gotSelection)
                {
                    if (m_brazilContext.m_state.equals("PositionScreenOpen"))
                    {
                        // Open the file 
                        //
                        //BufferView newBV = addNewFileBuffer(position, m_keyboardHandler.getSelectedFile(), m_keyboardHandler.getFileIsReadOnly(), m_keyboardHandler.getFileIsTailing());
                        //setActiveBuffer(newBV);
                        OnNewBufferViewEvent(new NewViewEventArgs(m_keyboardHandler.getSelectedFile(), position, m_keyboardHandler.getFileIsReadOnly(), m_keyboardHandler.getFileIsTailing()));
                        m_brazilContext.m_state = State.Test("TextEditing");
                    }
                    else if (m_brazilContext.m_state.equals("PositionScreenNew"))
                    {
                        // Use the convenience function
                        //
                        //BufferView newBV = addNewFileBuffer(position);
                        //setActiveBuffer(newBV);
                        OnNewBufferViewEvent(new NewViewEventArgs(m_context.m_fontManager, m_context.m_project.getSelectedBufferView(), position, NewViewMode.NewBuffer));
                        m_brazilContext.m_state = State.Test("TextEditing");
                    }
                    else if (m_brazilContext.m_state.equals("PositionScreenCopy"))
                    {
                        // Use the copy constructor
                        //
                        //BufferView newBV = new BufferView(m_context.m_fontManager, m_context.m_project.getSelectedBufferView(), position);
                        //m_context.m_project.addBufferView(newBV);
                        //setActiveBuffer(newBV);
                        OnNewBufferViewEvent(new NewViewEventArgs(m_context.m_fontManager, m_context.m_project.getSelectedBufferView(), position, NewViewMode.Copy));
                        m_brazilContext.m_state = State.Test("TextEditing");
                    }
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check to see if we have an interloper and if it needs some screen movement to keep it visible.  We draw a bounding box
        /// around the interloper and see if this intersects completely with the frustrum.  If it does then we're within the scope
        /// of the view - if not, work out which direction we're short in and pan the eye accordingly to keep the interloper
        /// in sight.
        ///
        /// </summary>
        /// <param name="gameTime"></param>
        public void checkInterloperBoundaries(GameTime gameTime)
        {
            if (m_brazilContext.m_interloper == null || m_eyeHandler.isChangingPosition() || !m_context.m_drawableComponents.ContainsKey(m_brazilContext.m_interloper))
                return;

            // Position within screen
            //
            Rectangle rect = m_context.m_graphics.GraphicsDevice.Viewport.Bounds;

            Vector3 pos = m_context.m_drawableComponents[m_brazilContext.m_interloper].getPosition();
            Vector3 boxSize = new Vector3(100.0f, 20.0f, 10.0f);
            BoundingBox playerBox = new BoundingBox(pos - boxSize, pos + boxSize);

            // If the player box is within the frustrum then we have nothing to do
            //

            Plane leftPlane = m_context.m_frustrum.Left;
            if (m_context.m_frustrum.Contains(playerBox) != ContainmentType.Intersects) /// ???
                return;

            Logger.logMsg("Escaping the frustrum");

            // Need to test which side has exited - we create new boundingboxen and retest these
            //
            BoundingBox leftBox = new BoundingBox(pos - boxSize, pos - boxSize + new Vector3(1, boxSize.Y, 1));
            BoundingBox rightBox = new BoundingBox(pos + boxSize, pos + boxSize - new Vector3(1, boxSize.Y, 1));
            BoundingBox bottomBox = new BoundingBox(pos - boxSize, pos - boxSize + new Vector3(boxSize.X, 1, 1));
            BoundingBox topBox = new BoundingBox(pos + boxSize, pos + boxSize - new Vector3(boxSize.X, 1, 1));

            //float leftLength = (m_eyeHandler.getEyePosition() + pos - new Vector3(boxSize.X, 0, 0)).Length();
            //float rightLength = (m_eyeHandler.getEyePosition() + pos + new Vector3(boxSize.X, 0, 0)).Length();

            // Start with a new eye position equalling old and affect this according to the results
            //
            Vector3 newEyePosition = m_eyeHandler.getEyePosition();

            // Adjust new eye position accordindly
            //
            if (m_context.m_frustrum.Contains(bottomBox) != ContainmentType.Contains)
                newEyePosition += new Vector3(0.0f, 100.0f, 0);

            if (m_context.m_frustrum.Contains(topBox) != ContainmentType.Contains)
                newEyePosition += new Vector3(0.0f, -100.0f, 0);

            if (m_context.m_frustrum.Contains(leftBox) != ContainmentType.Contains)
                newEyePosition += new Vector3(-2 * boxSize.X, 0, 0);

            if (m_context.m_frustrum.Contains(rightBox) != ContainmentType.Contains)
                newEyePosition += new Vector3(2 * boxSize.X, 0, 0);

            // And perform movement if necessary - if we don't do any movement here
            // then something strange is going on with our logic.
            //
            if (newEyePosition != m_eyeHandler.getEyePosition())
                m_eyeHandler.flyToPosition(newEyePosition);
            else
            {
                //Logger.logMsg("checkInterloperBoundaries - should have moved somewhere here");
                Thread.Sleep(1);
            }
        }


        /// <summary>
        /// Do some eye movement here as necessary.  Put the auto movement code in here for games too.
        /// </summary>
        /// <param name="gameTime"></param>
        public void performEyeMovement(GameTime gameTime)
        {
            // Check for this change as necessary
            //
            if (m_eyeHandler.isChangingPosition())
            {
                // Restore the original eye position before moving anywhere
                //
                //if (m_eyePerturber != null)
                //{
                //m_eye = XygloConvert.getVector3(m_eyePerturber.getInitialPosition());
                //m_eyePerturber = null;
                //}

                m_eyeHandler.changeEyePosition(gameTime);
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

        }

        /// <summary>
        /// Check for the world escape and return the state we should be in now.  Whether we being this level or whether
        /// we being an app within a BrazilView.  If we're doing this in an app then we need to find the interloper and
        /// remove all drawables for this app if we've died.  This restarts the level.
        /// </summary>
        /// <returns></returns>
        public void checkWorldEscape()
        {
            // Process any apps
            //
            if (m_context.m_project != null)
            {
                foreach (BrazilView bV in m_context.m_project.getBrazilViews())
                {
                    // Ignore once game over has happened
                    //
                    if (bV.getApp().getState() == State.Test("GameOver"))
                        continue;

                    // List from removal of objects
                    //
                    List<Component> removeList = new List<Component>();

                    // Fetch the interloper
                    //
                    foreach(Component intComp in bV.getApp().getComponents().Where(item => item.GetType() == typeof(BrazilInterloper)).ToList())
                    {
                        // Ensure that the interloper drawable is available
                        //
                        if (!m_context.m_drawableComponents.ContainsKey(intComp))
                            continue;

                        // Get the rendered drawable for each component
                        //
                        XygloComponentGroup group = (XygloComponentGroup)m_context.m_drawableComponents[intComp];

                        // Test for the interloper leaving its own bounding box
                        //
                        if (XygloConvert.getBoundingBox(bV.getApp().getWorldBounds()).Contains(group.getBoundingBox()) == ContainmentType.Disjoint)
                        {
                            // Decrement lives
                            //
                            bV.getApp().getWorld().setLives(bV.getApp().getWorld().getLives() - 1);

                            if (bV.getApp().getWorld().getLives() < 0)
                            {
                                bV.getApp().setState("GameOver");
                            }
                            else
                            {
                                // We've got one less life - this effectively restarts the level from scratch - we want to
                                // remove all the drawables from this current state and then they will get recreated 
                                // with the next draw run.
                                //
                                removeList.AddRange(bV.getApp().getComponents());
                            }
                        }
                    }

                    // Remove all the flagged defunct components
                    //
                    foreach (Component remove in removeList)
                    {
                        m_context.m_drawableComponents[remove] = null;
                        m_context.m_drawableComponents.Remove(remove);
                    }

                    // Clear the removal list ready for the next app
                    //
                    removeList.Clear();
                }
            }
            else
            {
                // Checking for the top level (this is a game)
                //
                if (m_brazilContext.m_interloper == null || !m_context.m_drawableComponents.ContainsKey(m_brazilContext.m_interloper))
                    return;

                // Check for world boundary escape
                //
                if (!XygloConvert.getBoundingBox(m_brazilContext.m_world.getBounds()).Intersects(m_context.m_drawableComponents[m_brazilContext.m_interloper].getBoundingBox()))
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
                        m_brazilContext.m_interloper = null;
                        m_context.m_drawableComponents.Clear();
                    }
                }
            }
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
        /// Set the State - we usually only do this once per instantiation and then let the world
        /// just live.
        /// </summary>
        /// <param name="state"></param>
        public void setState(State state)
        {
            m_brazilContext.m_state = state;
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
        /// The xyglo context
        /// </summary>
        protected XygloContext m_context;

        protected BrazilContext m_brazilContext;

        protected XygloKeyboard m_keyboard;

        protected XygloKeyboardHandler m_keyboardHandler;

        protected EyeHandler m_eyeHandler;

    }
}
