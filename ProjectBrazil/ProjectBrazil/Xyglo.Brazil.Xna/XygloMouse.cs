using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Xyglo.Friendlier;

namespace Xyglo.Brazil.Xna
{

    /// <summary>
    /// Collection of all the mouse methods we had hanging around in XygloXNA - probably need further
    /// teasing apart at some point.
    /// </summary>
    public class XygloMouse : XygloEventEmitter
    {
        /// <summary>
        /// Construct on a XygloContext
        /// </summary>
        /// <param name="context"></param>
        public XygloMouse(XygloContext context, BrazilContext brazilContext)
        {
            m_context = context;
            m_brazilContext = brazilContext;
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

            return lMA;
        }

        /// <summary>
        /// Handle mouse click and double clicks and farm out the responsibility to other
        /// helper methods.
        /// </summary>
        /// <param name="gameTime"></param>
        public void checkMouse(Game game, GameTime gameTime, XygloKeyboard keyboard, Vector3 eye, Vector3 target)
        {
            
            // Fetch all the mouse actions too
            //
            List<MouseAction> mouseActionList = getAllMouseActions();

            // Maybe we need a BufferView
            //
            BufferView bv = null;

            // If our main XNA window is inactive then ignore mouse clicks
            //
            if (game.IsActive == false || m_context.m_project == null) return;

            // Ignore an empty project for the moment
            //
            if (m_context.m_project != null)
                bv = m_context.m_project.getSelectedBufferView();

            // If we are flying somewhere then ignore mouse clicks
            //
            //if (m_changingEyePosition) return;

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

                    m_lastClickEyePosition = eye;// m_project.getSelectedView().getEyePosition(m_zoomLevel);

                    Logger.logMsg("Friender::checkMouse() - mouse clicked");


                    // If we're in a BrazilView then we need to see if the click has hit an object
                    //
                    if (m_context.m_project.getSelectedView().GetType() == typeof(BrazilView))
                    {
                        BrazilView brazilView = (BrazilView)m_context.m_project.getSelectedView();
                        bool gotComponent = false;
                        foreach (Component component in m_context.m_drawableComponents.Keys)
                        {
                            if (component.getApp() == null)
                                continue;

                            XygloXnaDrawable testDrawable = m_context.m_drawableComponents[component];

                            if (testDrawable.getBoundingBox().Intersects(pickRay) != null) // == ContainmentType.Intersects)
                            {
                                //Logger.logMsg("Clicked on an app object");
                                component.getApp().addToHighlight(component);
                                gotComponent = true;
                            }
                        }

                        // Clear the highlights if we've not added any (we've clicked outside them)
                        //
                        if (!gotComponent)
                            brazilView.getApp().clearHighlights();
                    }


                }
                else if (mouseAction.m_mouse == Mouse.LeftButtonHeld)
                {
                    // Have we clicked within a block of highlighted text?
                    //
                    if (m_clickInHighlight)
                    {
                        // Define a BrazilTemporary as a placeholder for this temporary piece of text
                        //
                        //

                        // Fetch the temporary drawable by type and index
                        //
                        List<BrazilTemporary> tempList = m_context.m_temporaryDrawables.Keys.ToList().Where(item => item.getType() == BrazilTemporaryType.CopyText && item.getIndex() == 0).ToList();

                        // Do we have a temporary piece of text yet?  (Not a definitive way of check for this)  If not we
                        // create one and put it on the m_temporaryDrawables list.
                        //
                        if (tempList.Count == 0)
                        {
                            // Generate a simulcrum of the highlighted text and make a moving ghost of it.
                            //
                            //Vector3 position = m_project.getActualTestRayIntersection(getPickRay());
                            Vector3? position = m_context.m_project.getZeroPlaneIntersection(getPickRay(), m_context.m_graphics.GraphicsDevice.Viewport.AspectRatio);

                            // There is some interesting stuff on setData here:
                            //
                            // http://blogs.msdn.com/b/shawnhar/archive/2008/04/15/stalls-part-two-beware-of-setdata.aspx
                            //
                            //
                            //if (bv.GetType() != typeof(BufferView))
                            //{
                                //BufferView bv = (BufferView)m_context.m_project.getSelectedView();

                                if (position != null && bv.gotHighlight())
                                {
                                    Vector3 foundPosition = (Vector3)position;

                                    string text = bv.getHighlightText(m_context.m_project);
                                    XygloText highlightText = new XygloText(m_context.m_fontManager, m_context.m_spriteBatch, bv.getTextColour(), m_context.m_lineEffect, foundPosition, bv.getViewSize(), text, bv.getHighlightStart().X);
                                    highlightText.setPickupOffset(foundPosition - bv.getHighlightStartPosition());
                                    highlightText.setVelocity(new Vector3(1, 0, 0));

                                    BrazilTemporary temp = new BrazilTemporary(BrazilTemporaryType.CopyText);
                                    // Set drop dead as five seconds into the future
                                    temp.setDropDead(gameTime.TotalGameTime.TotalSeconds + 5);
                                    m_context.m_temporaryDrawables[temp] = highlightText;

                                    // Store the source bufferview
                                    //
                                    m_context.m_sourceBufferView = bv;
                                }
                            //}
                        }
                        else
                        {
                            // We already have a ghost - move it
                            //
                            //Vector3 position = m_project.getActualTestRayIntersection(getPickRay());
                            Vector3? position = m_context.m_project.getZeroPlaneIntersection(getPickRay(), m_context.m_graphics.GraphicsDevice.Viewport.AspectRatio);

                            if (position != null)
                            {
                                Vector3 foundPosition = (Vector3)position;
                                foreach (BrazilTemporary temp in m_context.m_temporaryDrawables.Keys)
                                {
                                    if (temp.getType() == BrazilTemporaryType.CopyText && temp.getIndex() == 0)
                                    {
                                        m_context.m_temporaryDrawables[temp].setPosition(foundPosition);
                                        temp.setDropDead(gameTime.TotalGameTime.TotalSeconds + 5);

                                        // now test to see if the drawable is on the screen (or maybe near edge?)
                                        //
                                        if (m_context.m_frustrum.Contains(m_context.m_temporaryDrawables[temp].getBoundingBox()) != ContainmentType.Intersects)
                                        {
                                            Logger.logMsg("Moved ghost outside of screen position");
                                        }
                                    }
                                }
                            }

                            // If we're over a buffer view then insert a cursor where we might like to drop the text
                            //
                            Pair<BufferView, Pair<ScreenPosition, ScreenPosition>> testFind = m_context.m_project.testRayIntersection(getPickRay());
                            if (testFind.First != null && testFind.First != m_context.m_sourceBufferView)
                            {

                                // We want to fit target on the screen.  Use the following to determine the eye position:
                                //
                                // http://stackoverflow.com/questions/10998288/xna-camera-scene-size
                                //
                                if (m_context.m_frustrum.Contains(testFind.First.getBoundingBox()) == ContainmentType.Disjoint)
                                {
                                    Logger.logMsg("Target BV outside view - accomodate it by moving the eye back and out and across");
                                    zoomToAccomodate(testFind.First);
                                }
                            }
                        }
                    }
                    else if (m_gotDoubleClick)
                    {
                        // Reset cursor to current position
                        //
                        Pair<BufferView, ScreenPosition> bS = getBufferViewIntersection();

                        if (bS.First == m_context.m_project.getSelectedView() && bS.Second.X != -1 && bS.Second.Y != -1 && bS.Second.Y < bv.getFileBuffer().getLineCount())
                        {
                            ScreenPosition sP = bS.Second;
                            string line = bv.getFileBuffer().getLine(bS.Second.Y);
                            int maxX = Math.Max(m_context.m_project.screenToFile(line, line.Length - 1), 0);
                            if (sP.X > maxX)
                            {
                                sP.X = maxX;
                            }
                            else
                            {
                                sP.X = m_context.m_project.screenToFile(line, sP.X);
                            }

                            if (sP.X == -1)
                            {
                                Logger.logMsg("GOT -1");
                            }

                            // If we're outside of the current highlight then extend it - if we don't
                            // do this check then the hold after the double click cuts the newly 
                            // highlighted 'word' in half at this new point within it.
                            //
                            if (sP.Y != bv.getHighlightStart().Y ||
                                sP.Y != bv.getHighlightEnd().Y ||
                                sP.X < bv.getHighlightStart().X ||
                                sP.X > bv.getHighlightEnd().X)
                            {
                                // Store cursor position
                                //
                                ScreenPosition cP = bv.getCursorPosition();

                                // Move cursor to the new position and ensure that line is tab safe with the conversion
                                //
                                //Logger.logMsg("SET CURSOR POSTITION X = " + sP.X + ",Y = " + sP.Y);
                                bv.setCursorPosition(sP);

                                // Sweep out a selection to the cursor
                                //
                                bv.extendHighlight(cP);
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
                        //float multiplier = m_zoomLevel / m_zoomLevel;

                        Vector3 newEye = eye;
                        newEye.X = m_lastClickEyePosition.X - diffPosition.X; // *multiplier;
                        newEye.Y = m_lastClickEyePosition.Y + diffPosition.Y; // *multiplier;
                        Vector3 newTarget = target;

                        // If shift isn't down then we pan with the eye movement
                        //
                        if (!keyboard.isShiftDown())
                        {
                            newTarget.X = newEye.X;
                            newTarget.Y = newEye.Y;
                        }

                        OnEyeChangeEvent(new PositionEventArgs(newEye), new PositionEventArgs(newTarget));
                    }
                }
                else if (mouseAction.m_mouse == Mouse.LeftButtonRelease)
                {
                    // Clear down any temporaries
                    //
                    if (m_clickInHighlight)
                    {
                        List<BrazilTemporary> tempList = m_context.m_temporaryDrawables.Keys.ToList().Where(item => item.getType() == BrazilTemporaryType.CopyText && item.getIndex() == 0).ToList();

                        if (tempList.Count > 0)
                        {
                            foreach (BrazilTemporary temp in tempList)
                            {
                                m_context.m_temporaryDrawables[temp] = null;
                                m_context.m_temporaryDrawables.Remove(temp);
                            }
                        }
                    }

                    // Clear any double click or click in highlight flag
                    //
                    m_gotDoubleClick = false;
                    m_clickInHighlight = false;

                    if ((gameTime.TotalGameTime - m_lastClickTime).TotalSeconds < 0.15f)
                    {

                        if (m_brazilContext.m_state.equals("DiffPicker"))
                        {
                            if (m_differ != null && m_differ.hasDiffs())
                            {
                                // We have a diff pick - let's do something to the view
                                //
                                handleSingleClick(gameTime, keyboard.isShiftDown());
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
                            handleSingleClick(gameTime, keyboard.isShiftDown());
                        }
                    }
                    else // we've done a long click and release we're dragging - on the release handle the drag result
                    {
                        // At this point test to see which bufferview centre we're nearest and if we're near a
                        // different one then switch to that.
                        //
                        XygloView newView = m_context.m_project.testNearBufferView(eye);


                        if (newView != null)
                        {
                            // Only jump back for non BrazilViews
                            //
                            if (newView.GetType() != typeof(BrazilView))
                            {
                                // Calling this constructor sets active buffer only
                                //
                                OnViewChange(new XygloViewEventArgs(newView));
                            }
                        }
                        //else
                        //{
                            // Emit the event
                            //
                            //OnChangePosition(new PositionEventArgs(m_lastClickEyePosition));
                        //}
                        

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
                    if (keyboard.isShiftDown() && bv != null)
                    {
                        int linesDown = -(int)((m_lastMouseWheelValue - mouseAction.m_scrollWheel) / 120.0f);

                        if (linesDown < 0)
                        {
                            for (int i = 0; i < -linesDown; i++)
                            {
                                bv.moveCursorDown(false, keyboard.isShiftDown());
                            }
                        }
                        else
                        {
                            for (int i = 0; i < linesDown; i++)
                            {
                                bv.moveCursorUp(m_context.m_project, false, keyboard.isShiftDown());
                            }
                        }
                    }
                    else
                    {
                        float newZoomLevel = m_context.m_zoomLevel + (m_context.m_zoomStep * ((m_lastMouseWheelValue - mouseAction.m_scrollWheel) / 120.0f));
                        setZoomLevel(newZoomLevel, eye);
                    }

                    m_lastMouseWheelValue = mouseAction.m_scrollWheel;
                }


                // Check for the release of a sizing move
                //
                //if (mouseAction.m_mouse == Mouse.LeftButtonRelease && m_isResizing)
                //{
                    //m_isResizing = false;
                //}

                // Right mouse press
                //
                if (mouseAction.m_mouse == Mouse.RightButtonPress)
                {
                    StateAction sA = new StateAction(m_brazilContext.m_state, Mouse.RightButtonPress);

                    // scan components that could match
                    //
                    foreach (Component component in m_context.m_componentList.Where(item => item.getStateActions().Count != 0).ToList())
                    {
                        string name = component.getStateActions().First().getState().m_name;
                        if (!m_brazilContext.m_state.equals(component.getStateActions().First().getState().m_name) || (!component.isDestroyed() && !component.isHiding()))
                            continue;

                        //Mouse mouse = (MouseAction)(component.getStateActions().First().getActions().First());
                        Logger.logMsg("Got a matching component " + component.getName());
                        Mouse mouse = ((MouseAction)component.getStateActions().First().getActions().First()).m_mouse;

                        if (mouse == Mouse.RightButtonPress)
                        {
                            // Reset cursor to current position
                            //
                            Pair<BufferView, ScreenPosition> bS = getBufferViewIntersection();

                            if (bS.First == m_context.m_project.getSelectedView() && (bS.Second.X != -1 || bS.Second.Y != -1))
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
                                //if (sP.Y != m_project.getSelectedView().getHighlightStart().Y ||
                                //sP.Y != m_project.getSelectedView().getHighlightEnd().Y ||
                                //sP.X < m_project.getSelectedView().getHighlightStart().X ||
                                //sP.X > m_project.getSelectedView().getHighlightEnd().X)
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
                                    menu.buildBuffers(m_context.m_graphics.GraphicsDevice);
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
                    foreach (Component component in m_context.m_componentList.Where(item => item.getStateActions().Count != 0).ToList())
                    {
                        string name = component.getStateActions().First().getState().m_name;
                        if (!m_brazilContext.m_state.equals(component.getStateActions().First().getState().m_name))
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
        /// Zoom the view out to accomodate a given set of BufferViews
        /// </summary>
        /// <param name="accmodateThese"></param>
        protected void zoomToAccomodate(BufferView accomodateThis)
        {
            // See this to work out position of eye from object list:
            //
            // http://msdn.microsoft.com/en-us/library/bb197900(v=xnagamestudio.10).aspx
            //
            List<BufferView> accomodateThese = new List<BufferView>();
            accomodateThese.Add(m_context.m_project.getSelectedBufferView());
            accomodateThese.Add(accomodateThis);

            // Get the overall bounding box
            //
            BoundingBox containingBB = m_context.m_project.getBoundingBox(accomodateThese);

            Vector3 newEyePosition = Vector3.Zero;

            // Work out width and find out widest or highest part
            //
            float widthX = containingBB.Max.X - containingBB.Min.X;
            float widthY = containingBB.Max.Y - containingBB.Min.Y;
            float largest = Math.Max(widthX, widthY);

            // Position eye containing both bufferviews
            //
            newEyePosition.X = containingBB.Min.X + widthX / 2;
            newEyePosition.Y = -(containingBB.Min.Y + widthY / 2);
            newEyePosition.Z = largest / (float)Math.Cos(m_context.m_fov / 2);
            //newEyePosition.Z = largest / (float) Math.Sin(m_fov / 2);
            //newEyePosition.Z = largest * (float)Math.Atan(m_fov / 2);

            //flyToPosition(newEyePosition);
            OnChangePosition(new PositionEventArgs(newEyePosition));
        }

        /// <summary>
        /// Is the mouse hovering within a highlight boundary?
        /// </summary>
        /// <returns></returns>
        protected bool mouseWithinHighlight()
        {
            // Get intersection and check
            //
            Pair <BufferView, ScreenPosition> bS = getBufferViewIntersection();

            // Could be null
            BufferView bv = m_context.m_project.getSelectedBufferView();

            if (bS.First == m_context.m_project.getSelectedView() && bS.Second.X != -1 && bS.Second.Y != -1 && bS.Second.Y < bv.getFileBuffer().getLineCount())
            {
                ScreenPosition sP = bS.Second;
                string line = bv.getFileBuffer().getLine(bS.Second.Y);
                int maxX = Math.Max(m_context.m_project.screenToFile(line, line.Length - 1), 0);
                if (sP.X > maxX)
                {
                    sP.X = maxX;
                }
                else
                {
                    sP.X = m_context.m_project.screenToFile(line, sP.X);
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

            Vector3 nearsource = new Vector3((float)mouseX, (float)mouseY, m_context.m_zoomLevel);
            Vector3 farsource = new Vector3((float)mouseX, (float)mouseY, 0);

            Matrix world = Matrix.CreateScale(1, -1, 1); //Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = m_context.m_graphics.GraphicsDevice.Viewport.Unproject(nearsource, m_context.m_projection, m_context.m_viewMatrix, world);
            Vector3 farPoint = m_context.m_graphics.GraphicsDevice.Viewport.Unproject(farsource, m_context.m_projection, m_context.m_viewMatrix, world);

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
            Pair<BufferView, Pair<ScreenPosition, ScreenPosition>> testFind = m_context.m_project.testRayIntersection(getPickRay());
            BufferView bv = (BufferView)testFind.First;
            ScreenPosition fp = (ScreenPosition)testFind.Second.First;
            ScreenPosition screenRelativePosition = (ScreenPosition)testFind.Second.Second;

            // Check for validity of bv and position here
            //
            if (bv == null) // do nothing
                return;

            if (bv.isTailing())
                handleTailingDoubleClick(bv, fp, screenRelativePosition);
            else
                if (fp.Y >= 0 || fp.Y < bv.getFileBuffer().getLineCount() || fp.X >= 0 || fp.X < bv.getFileBuffer().getLine(fp.Y).Length)
                    handleStandardDoubleClick(bv, fp, screenRelativePosition);
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
                Pair<FileBuffer, ScreenPosition> found = m_context.m_project.findFileBufferFromText(line, m_modelBuilder);

                if (found.First != null)  // Found one.
                {
                    FileBuffer fb = (FileBuffer)found.First;
                    ScreenPosition sp = (ScreenPosition)found.Second;

                    // Try to find a BufferView for this FileBuffer
                    //
                    bv = m_context.m_project.getBufferView(fb.getFilepath());

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
                            //setHighlightAndCenter(bv, sp);
                            OnViewChange(new XygloViewEventArgs(bv, sp));
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
                        Vector3 newPos = m_context.m_project.getBestBufferViewPosition(m_context.m_project.getSelectedBufferView());
                        BufferView newBV = new BufferView(m_context.m_fontManager, fb, newPos, 0, 20, m_context.m_project.getFileIndex(fb), false);
                        int index = m_context.m_project.addBufferView(newBV);

                        int fileIndex = m_context.m_project.getFileIndex(fb);
                        newBV.setFileBufferIndex(fileIndex);

                        // Load the file
                        //
                        fb.loadFile(m_context.m_project.getSyntaxManager());

                        // Activate and centre
                        //
                        //setHighlightAndCenter(newBV, sp);
                        OnViewChange(new XygloViewEventArgs(newBV, sp));

                        return;
                    }
                }
                else // not found anything - look on the filesystem
                {
                    Logger.logMsg("XygloXNA::handleTailingDoubleClick() - inspecting filesystem");

                    // By default get the build directory - we'll probably want to change ths
                    //
                    string baseDir = m_context.m_project.getConfigurationValue("BUILDDIRECTORY");

                    // If we have retrieved a line to test
                    //
                    if (line != "")
                    {
                        // The getFileNamesFromText tries to find a filename and a FilePosition as well as
                        // an adjusted relative Scr

                        List<Pair<string, Pair<int, int>>> filePositionList = m_context.m_project.getFileNamesFromText(line);

                        foreach (Pair<string, Pair<int, int>> fpEntry in filePositionList)
                        {
                            // Clear the storage out first
                            //
                            m_context.m_fileSystemView.clearSearchDirectories();

                            // Do a search
                            //
                            m_context.m_fileSystemView.directorySearch(baseDir, fpEntry.First);

                            // Get results
                            //
                            List<string> rL = m_context.m_fileSystemView.getSearchDirectories();

                            if (rL.Count > 0)
                            {
                                Logger.logMsg("XygloXNA::handleTailingDoubleClick() - got " + rL.Count + " matches for file " + fpEntry.First);

                                // Set a highlight on the current BufferView
                                //
                                int xPosition = line.IndexOf(fpEntry.First);

                                // Set up the m_clickHighlight
                                //
                                m_clickHighlight.First = m_context.m_project.getSelectedBufferView();
                                m_clickHighlight.Second = new Highlight(screenRelativePosition.Y, xPosition, xPosition + fpEntry.First.Length, fpEntry.First, HighlightType.UserHighlight);

                                // Open file and zap to it
                                //

                                /**
                                 * 
                                 * TODO - FIX THIS NEXT SECTION TO WORK AGAIN - RWB
                                 */
                                //BufferView newBv = addNewFileBuffer(rL[0]);
                                ScreenPosition sp = new ScreenPosition(fpEntry.Second.Second, fpEntry.Second.First);
                                //setHighlightAndCenter(newBv, sp);
                                OnNewBufferViewEvent(new NewViewEventArgs(rL[0], sp, XygloView.ViewPosition.Right));
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
            if (fp.Y >= bv.getFileBuffer().getLineCount() || fp.X < 0 || fp.X >= bv.getFileBuffer().getLine(fp.Y).Length)
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
                ScreenPosition sp1 = new ScreenPosition(m_context.m_project.fileToScreen(line, startWord), fp.Y);
                ScreenPosition sp2 = new ScreenPosition(m_context.m_project.fileToScreen(line, endWord), fp.Y);
                bv.setHighlight(sp1, sp2);
            }
        }

                /// <summary>
        /// Handle a single left button mouse click
        /// </summary>
        /// <param name="gameTime"></param>
        protected void handleSingleClick(GameTime gameTime, bool shiftDown)
        {
            if (m_context.m_project == null)
            {
                return;
            }

            Pair<BufferView, Pair<ScreenPosition, ScreenPosition>> testFind = m_context.m_project.testRayIntersection(getPickRay());

            // Have we got a valid intersection?
            //
            if (testFind.First != null && testFind.Second.First != null)
            {
                BufferView bv = (BufferView)testFind.First;
                ScreenPosition fp = (ScreenPosition)testFind.Second.First;

                if (m_brazilContext.m_state.equals("DiffPicker"))
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
                        //setActiveBuffer(bv);
                        //bv.mouseCursorTo(m_shiftDown, sp);
                        OnViewChange(new XygloViewEventArgs(bv, sp, shiftDown));
                    }
                }
            }
        }

        /// <summary>
        /// Return a null BufferView if there is no intersection
        /// </summary>
        /// <returns></returns>
        protected Pair<BufferView, ScreenPosition> getBufferViewIntersection()
        {
            Pair<BufferView, Pair<ScreenPosition, ScreenPosition>> testFind = m_context.m_project.testRayIntersection(getPickRay());
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
            Pair<BufferView, Pair<ScreenPosition, ScreenPosition>> testFind = m_context.m_project.testRayIntersection(getPickRay());

            // Could be null
            BufferView bv = m_context.m_project.getSelectedBufferView();

            Logger.logMsg("XygloXNA::handleDiffPick() - got diff pick request");

            // We're only really interesed in the BufferView
            //
            if (testFind.First != null)
            {
                BufferView bv1 = m_context.m_project.getSelectedBufferView();
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
                    m_context.m_project.setLHSDiff(bv1);
                    m_context.m_project.setRHSDiff(bv2);

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
                        //setTemporaryMessage("No differences found.", 3, gameTime);
                        OnTemporaryMessage(new TextEventArgs("No differences found.", 3, gameTime));

                        // Set state back to default
                        //
                        m_brazilContext.m_state = State.Test("TextEditing");
                    }
                    else
                    {
                        //setTemporaryMessage("Diff selected", 1.5f, gameTime);
                        OnTemporaryMessage(new TextEventArgs("Diff selected", 1.5f, gameTime));

                        // Now set up the position of the diff previews and make sure we've
                        // pregenerated the lines
                        //
                        Vector2 leftBox = Vector2.Zero;
                        leftBox.X = (int)((m_context.m_graphics.GraphicsDevice.Viewport.Width / 2) - m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) * 20);
                        leftBox.Y = (int)(m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay) * 3);

                        Vector2 leftBoxEnd = leftBox;
                        leftBoxEnd.X += (int)(m_context.m_fontManager.getCharWidth(bv1.getViewSize()) * 16);
                        leftBoxEnd.Y += (int)(m_context.m_fontManager.getLineSpacing(bv1.getViewSize()) * 10);

                        Vector2 rightBox = Vector2.Zero;
                        rightBox.X = (int)((m_context.m_graphics.GraphicsDevice.Viewport.Width / 2) + m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) * 4);
                        rightBox.Y = leftBox.Y;

                        Vector2 rightBoxEnd = rightBox;
                        rightBoxEnd.X += (int)(m_context.m_fontManager.getCharWidth(bv2.getViewSize()) * 16);
                        rightBoxEnd.Y += (int)(m_context.m_fontManager.getLineSpacing(bv2.getViewSize()) * 10);

                        // Now generate some previews and store these positions
                        //
                        m_differ.generateDiffPreviews(leftBox, leftBoxEnd, rightBox, rightBoxEnd);

                        // Store the diff position as the left hand current cursor position - still need
                        // to check that this is valid and correct when it is expanded into the 'real' diff
                        // position.
                        //
                        m_diffPosition = m_differ.originalLhsFileToDiffPosition(bv.getBufferShowStartY());

                        // Now we want to fly to the mid position between the two views - we have to assume they are
                        // next to each other for this to make sense.  Only do this if it's configured as such.
                        //
                        if (m_context.m_project.getConfigurationValue("DIFFCENTRE").ToUpper() == "TRUE")
                        {
                            Vector3 look1 = (bv1.getEyePosition() + bv2.getEyePosition()) / 2;
                            look1.Z = bv1.getEyePosition().Z * 1.8f;

                            //flyToPosition(look1);
                            OnChangePosition(new PositionEventArgs(look1));
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
                    //setTemporaryMessage("Can't diff a BufferView with itself.", 3, gameTime);
                    OnTemporaryMessage(new TextEventArgs("Can't diff a BufferView with itself.", 3, gameTime));

                    // Set state back to default
                    //
                    m_brazilContext.m_state = State.Test("TextEditing");
                }
            }
        }

        /// <summary>
        /// Set a current zoom level
        /// </summary>
        /// <param name="zoomLevel"></param>
        public void setZoomLevel(float zoomLevel, Vector3 eye)
        {
            m_context.m_zoomLevel = zoomLevel;

            if (m_context.m_zoomLevel < 500.0f)
            {
                m_context.m_zoomLevel = 500.0f;
            }

            Vector3 eyePos = eye;
            eyePos.Z = m_context.m_zoomLevel;

            OnChangePosition(new PositionEventArgs(eyePos));
        }

         /// <summary>
        /// Return this value for use in drawables
        /// </summary>
        /// <returns></returns>
        public Vector3 getLastClickWorldPosition()
        {
            return m_lastClickWorldPosition;
        }

        /// <summary>
        /// Returns the offset from the last click position to the start of the string that
        /// was clicked on.  Used in drawables.
        /// </summary>
        /// <returns></returns>
        public Vector2 geLastClickCursorOffset()
        {
            return m_lastClickCursorOffsetPosition;
        }

        /// <summary>
        /// Return the click highlight information
        /// </summary>
        /// <returns></returns>
        public Pair<BufferView, Highlight> getClickHighlight() { return m_clickHighlight; }

        /// <summary>
        /// Have we recently got a double click - next release clears it
        /// </summary>
        bool m_gotDoubleClick = false;

        /// <summary>
        /// Have we left mouse clicked in a highlighted piece of text?
        /// </summary>
        bool m_clickInHighlight = false;
        
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
        /// A local Differ object
        /// </summary>
        protected Differ m_differ = null;

        /// <summary>
        /// Position we are in the diff
        /// </summary>
        protected int m_diffPosition = 0;

        /// <summary>
        /// Model builder realises a model from a tree
        /// </summary>
        protected ModelBuilder m_modelBuilder;

        /// <summary>
        /// A drawable, keyed component dictionary
        /// </summary>
        protected Dictionary<Component, XygloXnaDrawable> m_drawableComponents = new Dictionary<Component, XygloXnaDrawable>();

        /// <summary>
        /// The Xyglo context
        /// </summary>
        protected XygloContext m_context;

        /// <summary>
        /// BrazilContext
        /// </summary>
        protected BrazilContext m_brazilContext;
    }
}
