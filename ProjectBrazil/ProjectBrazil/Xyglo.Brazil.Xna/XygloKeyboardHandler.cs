using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Permissions;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Xyglo.Friendlier;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Manage keys and combinations and route happenings
    /// </summary>
    public class XygloKeyboardHandler : XygloEventEmitter
    {
        public XygloKeyboardHandler(XygloContext context, BrazilContext brazilContext, XygloGraphics graphics, XygloKeyboard keyboard, XygloMouse mouse)
        {
            m_context = context;
            m_brazilContext = brazilContext;
            m_keyboard = keyboard;
            m_graphics = graphics;
            m_mouse = mouse;
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

            //flyToPosition(eyePos);
            OnChangePosition(new PositionEventArgs(eyePos));
        }

        /// <summary>
        /// When the worker has been initiliased then pass in the reference
        /// </summary>
        /// <param name="smartHelpWorker"></param>
        public void setSmartHelpWorker(SmartHelpWorker smartHelpWorker)
        {
            m_smartHelpWorker = smartHelpWorker;
        }

        /// <summary>
        /// Run a search on the current BufferView
        /// </summary>
        /// <returns></returns>
        protected void doSearch(GameTime gameTime)
        {
            m_brazilContext.m_state = State.Test("TextEditing");

            // Test for type of view
            if (m_context.m_project.getSelectedView().GetType() != typeof(BufferView))
                return;

            BufferView bv = (BufferView)m_context.m_project.getSelectedView();

            // Don't search for nothing
            //
            if (bv.getSearchText() == "")
            {
                return;
            }

            // If we find something from cursor we're finished here
            //
            if (bv.findFromCursor(false))
            {
                return;
            }

            // Now try to find from the top of the file
            //
            if (bv.getCursorPosition().Y > 0)
            {
                // Try find from the top - if it finds something then let user know we've
                // wrapped around.
                //
                if (bv.find(new ScreenPosition(0, 0), false))
                {
                    //setTemporaryMessage("Search wrapped around end of file", 1.5f, gameTime);
                    OnTemporaryMessage(new TextEventArgs("Search wrapped around end of file", 1.5f, gameTime));
                    return;
                }
            }

            //setTemporaryMessage("\"" + m_context.m_project.getSelectedView().getSearchText() + "\" not found", 3, gameTime);
            OnTemporaryMessage(new TextEventArgs("\"" + bv.getSearchText() + "\" not found", 3, gameTime));
        }

        /// <summary>
        /// Process action keys for commands
        /// </summary>
        /// <param name="gameTime"></param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public bool processActionKey(GameTime gameTime, Game game, Vector3 eye, KeyAction keyAction)
        {
            List<Keys> keyList = new List<Keys>();
            keyList.Add(keyAction.m_key);
            bool consumed = false;

            if (m_brazilContext.m_state.equals("PositionScreenOpen") ||
                m_brazilContext.m_state.equals("PositionScreenNew"))
                return false;

            BufferView bv = m_context.m_project.getSelectedBufferView();

            // Main key handling statement
            //
            if (keyList.Contains(Keys.Up))
            {
                if (m_brazilContext.m_state.equals("FileSaveAs") || m_brazilContext.m_state.equals("FileOpen") || m_brazilContext.m_state.equals("ProjectOpen"))
                {
                    m_context.m_fileSystemView.incrementHighlightIndex(-1);
                    consumed = true;
                }
                else if (m_brazilContext.m_state.equals("ProjectNew"))
                {
                    if (m_context.m_templateManager.getHighlightIndex() > 0)
                    {
                        m_context.m_templateManager.setHighlightIndex(m_context.m_templateManager.getHighlightIndex() - 1);
                        consumed = true;
                    }
                }
                else if (m_brazilContext.m_state.equals("ManageProject") || (m_brazilContext.m_state.equals("Configuration") && m_editConfigurationItem == false)) // Configuration changes
                {
                    if (m_configPosition > 0)
                    {
                        m_configPosition--;
                        consumed = true;
                    }
                }
                else if (m_brazilContext.m_state.equals("DiffPicker"))
                {
                    if (m_diffPosition > 0)
                    {
                        m_diffPosition--;
                        consumed = true;
                    }
                }
                else
                {
                    if (m_keyboard.isAltDown() && m_keyboard.isShiftDown()) // Do zoom
                    {
                        setZoomLevel(m_context.m_zoomLevel - m_context.m_zoomStep, eye);
                        consumed = true;
                    }
                    else if (m_keyboard.isAltDown())
                    {
                        // Attempt to move right if there's a BufferView there
                        //
                        detectMove(BufferView.ViewPosition.Above, gameTime);
                        consumed = true;
                    }
                    else
                    {
                        if (bv != null)
                        {
                            ScreenPosition sP = bv.getCursorPosition();
                            bv.moveCursorUp(m_context.m_project, false, m_keyboard.isShiftDown());

                            if (m_keyboard.isShiftDown())
                            {
                                bv.extendHighlight(sP);  // Extend 
                            }
                            else
                            {
                                bv.noHighlight(); // Disable
                            }

                            consumed = true;
                        }
                    }
                }
            }
            //else if (keyActionList.Find(item => item.m_key == Keys.Down && item.m_modifier == KeyboardModifier.None).ToString() != "")
            else if (keyList.Contains(Keys.Down))
            {
                if (m_brazilContext.m_state.equals("FileSaveAs") || m_brazilContext.m_state.equals("FileOpen") || m_brazilContext.m_state.equals("ProjectOpen"))
                {
                    m_context.m_fileSystemView.incrementHighlightIndex(1);
                }
                else if (m_brazilContext.m_state.equals("Configuration") && m_editConfigurationItem == false) // Configuration changes
                {
                    if (m_configPosition < m_context.m_project.getConfigurationListLength() - 1)
                    {
                        m_configPosition++;
                        consumed = true;
                    }
                }
                else if (m_brazilContext.m_state.equals("ProjectNew"))
                {
                    if (m_context.m_templateManager.getHighlightIndex() < m_context.m_templateManager.getTotalTemplates() - 1)
                    {
                        m_context.m_templateManager.setHighlightIndex(m_context.m_templateManager.getHighlightIndex() + 1);
                        consumed = true;
                    }
                }
                else if (m_brazilContext.m_state.equals("ManageProject"))
                {
                    if (m_configPosition < m_modelBuilder.getLeafNodesPlaces() - 1)
                    {
                        m_configPosition++;
                        consumed = true;
                    }
                }
                else if (m_brazilContext.m_state.equals("DiffPicker"))
                {
                    if (m_differ != null && m_diffPosition < m_differ.getMaxDiffLength())
                    {
                        m_diffPosition++;
                        consumed = true;
                    }
                }
                else
                {
                    if (m_keyboard.isAltDown() && m_keyboard.isShiftDown()) // Do zoom
                    {
                        m_context.m_zoomLevel += m_context.m_zoomStep;

                        //setActiveBuffer();
                        OnViewChange(new XygloViewEventArgs(bv));
                        consumed = true;
                    }
                    else if (m_keyboard.isAltDown())
                    {
                        // Attempt to move right if there's a BufferView there
                        //
                        detectMove(BufferView.ViewPosition.Below, gameTime);
                        consumed = true;
                    }
                    else
                    {
                        if (bv != null)
                        {
                            ScreenPosition sP = bv.getCursorPosition();
                            bv.moveCursorDown(false, m_keyboard.isShiftDown());

                            if (m_keyboard.isShiftDown())
                            {
                                bv.extendHighlight(sP);
                            }
                            else
                            {
                                bv.noHighlight(); // Disable
                            }
                            consumed = true;
                        }
                    }
                }
            }
            else if (keyList.Contains(Keys.Left))
            {
                if (m_brazilContext.m_state.equals("FileSaveAs") || m_brazilContext.m_state.equals("FileOpen") || m_brazilContext.m_state.equals("ProjectOpen"))
                {
                    string parDirectory = "";

                    // Set the directory to the sub directory and reset the highlighter
                    //
                    try
                    {
                        DirectoryInfo parentTest = m_context.m_fileSystemView.getParent();

                        if (parentTest == null)
                        {
                            Logger.logMsg("Check devices");
                            m_context.m_fileSystemView.setDirectory(null);
                            consumed = true;
                        }
                        else
                        {
                            parDirectory = m_context.m_fileSystemView.getParent().Name;
                            DirectoryInfo[] testAccess = m_context.m_fileSystemView.getParent().GetDirectories();
                            FileInfo[] testFiles = m_context.m_fileSystemView.getParent().GetFiles();

                            m_context.m_fileSystemView.setDirectory(m_context.m_fileSystemView.getParent().FullName);
                            m_context.m_fileSystemView.setHighlightIndex(m_lastHighlightIndex);
                            consumed = true;
                        }
                    }
                    catch (Exception)
                    {
                        //setTemporaryMessage("Cannot access " + parDirectory.ToString(), 2, gameTime);
                        OnTemporaryMessage(new TextEventArgs("Cannot access " + parDirectory.ToString(), 2, gameTime));
                    }
                }
                else
                {
                    if (m_keyboard.isAltDown())
                    {
                        // Attempt to move right if there's a BufferView there
                        //
                        detectMove(BufferView.ViewPosition.Left, gameTime);
                        consumed = true;
                    }
                    else if (bv != null)
                    {
                        // Store cursor position
                        //
                        ScreenPosition sP = bv.getCursorPosition();

                        if (m_keyboard.isCtrlDown())
                        {
                            bv.wordJumpCursorLeft();
                        }
                        else
                        {
                            bv.moveCursorLeft(m_context.m_project, m_keyboard.isShiftDown());
                        }

                        if (m_keyboard.isShiftDown())
                        {
                            bv.extendHighlight(sP);  // Extend
                        }
                        else
                        {
                            bv.noHighlight(); // Disable
                        }
                        consumed = true;
                    }

                    if (m_context.m_project.getSelectedView().GetType() == typeof(BrazilView) && m_keyboard.isShiftDown())
                    {
                        m_context.m_componentPalette.decrementSelection();
                        consumed = true;
                    }
                }
            }
            else if (keyList.Contains(Keys.Right))
            {
                if (m_brazilContext.m_state.equals("FileSaveAs") || m_brazilContext.m_state.equals("FileOpen") || m_brazilContext.m_state.equals("ProjectOpen"))
                {
                    traverseDirectory(gameTime);
                    consumed = true;
                }
                else
                {
                    if (m_keyboard.isAltDown())
                    {
                        // Attempt to move right if there's a BufferView there
                        //
                        detectMove(BufferView.ViewPosition.Right, gameTime);
                        consumed = true;
                    }
                    else if (bv != null)
                    {
                        // Store cursor position
                        //
                        ScreenPosition sP = bv.getCursorPosition();

                        if (m_keyboard.isCtrlDown())
                        {
                            bv.wordJumpCursorRight();
                        }
                        else
                        {
                            bv.moveCursorRight(m_context.m_project, m_keyboard.isShiftDown());
                        }

                        if (m_keyboard.isShiftDown())
                        {
                            bv.extendHighlight(sP); // Extend
                        }
                        else
                        {
                            bv.noHighlight(); // Disable
                        }
                        consumed = true;
                    }

                    if (m_context.m_project.getSelectedView().GetType() == typeof(BrazilView) && m_keyboard.isShiftDown())
                    {
                        m_context.m_componentPalette.incrementSelection();
                        consumed = true;
                    }
                }

            }
            else if (keyList.Contains(Keys.End))
            {
                ScreenPosition fp = bv.getCursorPosition();
                ScreenPosition originalFp = fp;

                // Set X and allow for tabs
                //
                if (fp.Y < bv.getFileBuffer().getLineCount())
                {
                    fp.X = bv.getFileBuffer().getLine(fp.Y).Replace("\t", m_context.m_project.getTab()).Length;
                }
                bv.setCursorPosition(fp);

                // Change the X offset if the row is longer than the visible width
                //
                if (fp.X > bv.getBufferShowWidth())
                {
                    int bufferX = fp.X - bv.getBufferShowWidth();
                    bv.setBufferShowStartX(bufferX);
                }

                if (m_keyboard.isShiftDown())
                {
                    bv.extendHighlight(originalFp); // Extend
                }
                else
                {
                    bv.noHighlight(); // Disable
                }
                consumed = true;
            }
            else if (keyList.Contains(Keys.Home))
            {
                if (bv != null)
                {
                    // Store cursor position
                    //
                    ScreenPosition sP = bv.getCursorPosition();

                    // Reset the cursor to zero
                    //
                    ScreenPosition fp = bv.getFirstNonSpace(m_context.m_project);

                    bv.setCursorPosition(fp);

                    // Reset any X offset to zero
                    //
                    bv.setBufferShowStartX(0);

                    if (m_keyboard.isShiftDown())
                    {
                        bv.extendHighlight(sP); // Extend
                    }
                    else
                    {
                        bv.noHighlight(); // Disable
                    }
                    consumed = true;
                }
            }
                /*
            else if (keyList.Contains(Keys.F9)) // Spin anticlockwise though BVs
            {
                m_context.m_zoomLevel = 1000.0f;
                //setActiveBuffer(BufferView.ViewCycleDirection.Anticlockwise);
                consumed = true;
            }
            else if (keyList.Contains(Keys.F10)) // Spin clockwise through BVs
            {
                m_context.m_zoomLevel = 1000.0f;
                //setActiveBuffer(BufferView.ViewCycleDirection.Clockwise);
                consumed = true;
            }*/
            else if (keyList.Contains(Keys.F3))
            {
                doSearch(gameTime);
                consumed = true;
            }
            else if (keyList.Contains(Keys.F4))
            {
                m_context.m_project.setViewMode(Project.ViewMode.Fun);
                m_context.m_drawingHelper.startBanner(gameTime, VersionInformation.getProductName() + "\nv" + VersionInformation.getProductVersion(), 5);
                consumed = true;
            }
            else if (keyList.Contains(Keys.F6))
            {
                //doBuildCommand(gameTime);
                OnCommandEvent(new CommandEventArgs(gameTime, XygloCommand.Build));
                consumed = true;
            }
            else if (keyList.Contains(Keys.F7))
            {
                string command = m_context.m_project.getConfigurationValue("ALTERNATEBUILDCOMMAND");
                //doBuildCommand(gameTime, command);
                OnCommandEvent(new CommandEventArgs(gameTime, XygloCommand.AlternateBuild, command));
                consumed = true;
            }
            else if (keyList.Contains(Keys.F8))
            {
                //startGame(gameTime);
                OnCommandEvent(new CommandEventArgs(gameTime, XygloCommand.XygloClient, "Paulo"));
                consumed = true;
            }
            else if (keyList.Contains(Keys.F9))
            {
                OnCommandEvent(new CommandEventArgs(gameTime, XygloCommand.XygloComponent, "Test"));
                consumed = true;
            }
            else if (keyList.Contains(Keys.F11)) // Toggle full screen
            {
                if (m_context.m_project.isFullScreen())
                {
                    m_graphics.windowedMode(game);
                }
                else
                {
                    m_graphics.fullScreenMode(game);
                }

                m_context.m_drawingHelper.setSpriteFont();
                consumed = true;
            }
            else if (keyList.Contains(Keys.F1))  // Cycle down through BufferViews
            {
                int newValue = m_context.m_project.getSelectedViewId() - 1;
                if (newValue < 0)
                {
                    newValue += m_context.m_project.getViews().Count;
                }

                m_context.m_project.setSelectedViewId(newValue);
                //setActiveBuffer();
                OnViewChange(new XygloViewEventArgs(m_context.m_project.getSelectedBufferView()));
                consumed = true;
            }
            else if (keyList.Contains(Keys.F2)) // Cycle up through BufferViews
            {
                int newValue = (m_context.m_project.getSelectedViewId() + 1) % m_context.m_project.getViews().Count;
                m_context.m_project.setSelectedViewId(newValue);
                //setActiveBuffer();
                OnViewChange(new XygloViewEventArgs(m_context.m_project.getSelectedBufferView()));
                consumed = true;
            }
            else if (keyList.Contains(Keys.PageDown))
            {
                if (m_brazilContext.m_state.equals("TextEditing"))
                {
                    // Store cursor position
                    //
                    ScreenPosition sP = bv.getCursorPosition();

                    bv.pageDown(m_context.m_project);

                    if (m_keyboard.isShiftDown())
                    {
                        bv.extendHighlight(sP); // Extend
                    }
                    else
                    {
                        bv.noHighlight(); // Disable
                    }
                    consumed = true;
                }
                else if (m_brazilContext.m_state.equals("DiffPicker"))
                {
                    if (m_differ != null && m_diffPosition < m_differ.getMaxDiffLength())
                    {
                        m_diffPosition += bv.getBufferShowLength();

                        if (m_diffPosition >= m_differ.getMaxDiffLength())
                        {
                            m_diffPosition = m_differ.getMaxDiffLength() - 1;
                        }
                    }
                    consumed = true;
                }
            }
            else if (keyList.Contains(Keys.PageUp))
            {
                if (m_brazilContext.m_state.equals("TextEditing"))
                {
                    if (bv != null)
                    {
                        // Store cursor position
                        //
                        ScreenPosition sP = bv.getCursorPosition();

                        bv.pageUp(m_context.m_project);

                        if (m_keyboard.isShiftDown())
                        {
                            bv.extendHighlight(sP); // Extend
                        }
                        else
                        {
                            bv.noHighlight(); // Disable
                        }
                        consumed = true;
                    }
                }
                else if (m_brazilContext.m_state.equals("DiffPicker"))
                {
                    if (m_diffPosition > 0)
                    {
                        m_diffPosition -= bv.getBufferShowLength();

                        if (m_diffPosition < 0)
                        {
                            m_diffPosition = 0;
                        }
                    }
                    consumed = true;
                }
            }
            else if (keyList.Contains(Keys.Scroll))
            {
                if (bv.isLocked())
                {
                    bv.setLock(false, 0);
                }
                else
                {
                    bv.setLock(true, bv.getCursorPosition().Y);
                }
                consumed = true;
            }
            else if (keyList.Contains(Keys.Tab)) // Insert a tab space
            {
                bv.insertText(m_context.m_project, "\t");
                // NEED THIS
                //updateSmartHelp();
                consumed = true;
            }
            else if (keyList.Contains(Keys.Insert))
            {
                if (m_brazilContext.m_state.equals("ManageProject"))
                {
                    if (m_configPosition >= 0 && m_configPosition < m_modelBuilder.getLeafNodesPlaces())
                    {
                        string fileToEdit = m_modelBuilder.getSelectedModelString(m_configPosition);

                        //BufferView bv = m_context.m_project.getBufferView(fileToEdit);

                        if (bv != null)
                        {
                            //setActiveBuffer(bv);
                            OnViewChange(new XygloViewEventArgs(bv));
                        }
                        else // create and activate
                        {
                            try
                            {
                                FileBuffer fb = m_context.m_project.getFileBuffer(fileToEdit);
                                bv = new BufferView(m_context.m_fontManager, m_context.m_project.getBufferViews()[0], BufferView.ViewPosition.Left);
                                bv.setFileBuffer(fb);
                                int bvIndex = m_context.m_project.addBufferView(bv);
                                OnViewChange(new XygloViewEventArgs(bv));
                                //setActiveBuffer(bvIndex);

                                Vector3 rootPosition = m_context.m_project.getBufferViews()[0].getPosition();
                                Vector3 newPosition2 = bv.getPosition();

                                Logger.logMsg(rootPosition.ToString() + newPosition2.ToString());
                                //bv.setFileBufferIndex(
                                fb.loadFile(m_context.m_project.getSyntaxManager());

                                if (m_context.m_project.getConfigurationValue("SYNTAXHIGHLIGHT").ToUpper() == "TRUE")
                                {
                                    // NEED THIS
                                    //m_smartHelpWorker.updateSyntaxHighlighting(m_context.m_project.getSyntaxManager(), fb);
                                }

                                // Break out of Manage mode and back to editing
                                //
                                Vector3 newPosition = bv.getLookPosition();
                                newPosition.Z = 500.0f;
                                m_brazilContext.m_state = State.Test("TextEditing");
                                m_editConfigurationItem = false;
                            }
                            catch (Exception e)
                            {
                                //setTemporaryMessage("Failed to load file " + e.Message, 2, gameTime);
                                OnTemporaryMessage(new TextEventArgs("Failed to load file " + e.Message, 2, gameTime));
                            }
                        }
                    }
                }
                consumed = true;
            }
            else if (keyList.Contains(Keys.Delete) || keyList.Contains(Keys.Back))
            {

                if (m_brazilContext.m_state.equals("FileSaveAs") && keyList.Contains(Keys.Back))
                {
                    // Delete charcters from the file name if we have one
                    //
                    if (m_saveFileName.Length > 0)
                    {
                        m_saveFileName = m_saveFileName.Substring(0, m_saveFileName.Length - 1);
                    }
                    consumed = true;
                }
                else if (m_brazilContext.m_state.equals("FindText") && keyList.Contains(Keys.Back))
                {
                    string searchText = bv.getSearchText();
                    // Delete charcters from the file name if we have one
                    //
                    if (searchText.Length > 0)
                    {
                        bv.setSearchText(searchText.Substring(0, searchText.Length - 1));
                    }
                    consumed = true;
                }
                else if (m_brazilContext.m_state.equals("GotoLine") && keyList.Contains(Keys.Back))
                {
                    if (m_gotoLine.Length > 0)
                    {
                        m_gotoLine = m_gotoLine.Substring(0, m_gotoLine.Length - 1);
                    }
                    consumed = true;
                }
                else if (m_brazilContext.m_state.equals("Configuration") && m_editConfigurationItem && keyList.Contains(Keys.Back))
                {
                    if (m_editConfigurationItemValue.Length > 0)
                    {
                        m_editConfigurationItemValue = m_editConfigurationItemValue.Substring(0, m_editConfigurationItemValue.Length - 1);
                    }
                    consumed = true;
                }
                else if (m_brazilContext.m_state.equals("ManageProject"))
                {
                    if (m_configPosition >= 0 && m_configPosition < m_modelBuilder.getLeafNodesPlaces())
                    {
                        string fileToRemove = m_modelBuilder.getSelectedModelString(m_configPosition);
                        if (m_context.m_project.removeFileBuffer(fileToRemove))
                        {
                            Logger.logMsg("XygloXNA::processActionKeys() - removed FileBuffer for " + fileToRemove);

                            // Update Active Buffer as necessary
                            //
                            //setActiveBuffer();
                            OnViewChange(new XygloViewEventArgs(m_context.m_project.getSelectedBufferView()));

                            // Rebuild the file model - NEED TO SEND THIS
                            //
                            generateTreeModel();

                            //setTemporaryMessage("Removed " + fileToRemove + " from project", 5, m_context.m_gameTime);
                            OnTemporaryMessage(new TextEventArgs("Removed " + fileToRemove + " from project", 5, m_context.m_gameTime));
                        }
                        else
                        {
                            Logger.logMsg("XygloXNA::processActionKeys() - failed to remove FileBuffer for " + fileToRemove);
                        }
                    }
                    consumed = true;
                }
                else if (bv != null && bv.gotHighlight()) // If we have a valid highlighted selection then delete it (normal editing)
                {
                    // All the clever stuff with the cursor is done at the BufferView level and it also
                    // calls the command in the FileBuffer.
                    //
                    bv.deleteCurrentSelection(m_context.m_project);
                    consumed = true;
                    // NEED THIS
                    //updateSmartHelp();
                }
                else // delete at cursor
                {
                    if (keyList.Contains(Keys.Delete))
                    {
                        if (bv != null)
                        {
                            bv.deleteSingle(m_context.m_project);
                            consumed = true;
                        }
                    }
                    else if (keyList.Contains(Keys.Back))
                    {
                        // Start with a file position from the screen position
                        //
                        FilePosition fp = bv.screenToFilePosition(m_context.m_project);

                        // Get the character before the current one and backspace accordingly 

                        if (fp.X > 0)
                        {
                            string fetchLine = bv.getCurrentLine();

                            // Decrement and set X
                            //
                            fp.X--;

                            // Now convert back to a screen position
                            fp.X = fetchLine.Substring(0, fp.X).Replace("\t", m_context.m_project.getTab()).Length;

                        }
                        else if (fp.Y > 0)
                        {
                            fp.Y -= 1;

                            // Don't forget to do tab conversions here too
                            //
                            fp.X = bv.getFileBuffer().getLine(Convert.ToInt16(fp.Y)).Replace("\t", m_context.m_project.getTab()).Length;
                        }

                        bv.setCursorPosition(new ScreenPosition(fp));

                        bv.deleteSingle(m_context.m_project);
                        consumed = true;
                    }
                    // NEED THIS
                    //updateSmartHelp();
                }


            }
            else if (keyList.Contains(Keys.Enter))
            {
                //ScreenPosition fp = bv.getCursorPosition();

                if (m_brazilContext.m_state.equals("FileSaveAs"))
                {
                    // Check that the filename is valid
                    //
                    if (m_saveFileName != "" && m_saveFileName != null)
                    {
                        bv.getFileBuffer().setFilepath(m_context.m_fileSystemView.getPath() + m_saveFileName);

                        Logger.logMsg("XygloXNA::processActionKeys() - file name = " + bv.getFileBuffer().getFilepath());

                        completeSaveFile(gameTime);

                        // Now if we have remaining files to write then we need to carry on saving files
                        //
                        if (m_filesToWrite != null)
                        {
                            //m_filesToWrite.Remove(m_context.m_project.getSelectedView().getFileBuffer());

                            // If we have remaining files to edit then set the active BufferView to one that
                            // looks over this file - then fly to it and choose and file location.
                            //
                            if (m_filesToWrite.Count > 0)
                            {
                                m_context.m_project.setSelectedViewByFileBuffer(m_filesToWrite[0]);
                                //m_eye = m_context.m_project.getSelectedView().getEyePosition();
                                OnChangePosition(new PositionEventArgs(bv.getEyePosition()));

                                selectSaveFile(gameTime);
                            }
                            else // We're done 
                            {
                                m_filesToWrite = null;
                                Logger.logMsg("XygloXNA::processActionKeys() - saved some files.  Quitting.");

                                // Exit nicely and ensure we serialise
                                //
                                //checkExit(gameTime);
                                OnCleanExitEvent(new CleanExitEventArgs(gameTime));
                            }
                        }
                        else
                        {
                            // Exit nicely and ensure we serialise
                            //
                            if (m_saveAsExit)
                            {
                                //checkExit(gameTime);
                                OnCleanExitEvent(new CleanExitEventArgs(gameTime));
                            }
                        }
                    }

                    consumed = true;
                }
                else if (m_brazilContext.m_state.equals("FileOpen") || m_brazilContext.m_state.equals("ProjectOpen"))
                {
                    traverseDirectory(gameTime);
                    consumed = true;
                }
                else if (m_brazilContext.m_state.equals("Configuration"))
                {
                    // Set this status so that we edit the item
                    //
                    if (m_editConfigurationItem == false)
                    {
                        // Go into item edit mode and copy across the current value
                        m_editConfigurationItem = true;
                        m_editConfigurationItemValue = m_context.m_project.getConfigurationItem(m_configPosition).Value;
                    }
                    else
                    {
                        // Completed editing the item - now set it
                        //
                        m_editConfigurationItem = false;
                        m_context.m_project.updateConfigurationItem(m_context.m_project.getConfigurationItem(m_configPosition).Name, m_editConfigurationItemValue);
                    }
                    consumed = true;
                }
                else if (m_brazilContext.m_state.equals("ProjectNew"))
                {
                    OnNewProjectEvent(new NewProjectEventArgs()); // emit
                }
                else if (m_brazilContext.m_state.equals("FindText"))
                {
                    doSearch(gameTime);
                }
                else if (m_brazilContext.m_state.equals("GotoLine"))
                {
                    try
                    {
                        int gotoLine = Convert.ToInt16(m_gotoLine);

                        if (gotoLine > 0)
                        {
                            if (gotoLine < bv.getFileBuffer().getLineCount() - 1)
                            {
                                ScreenPosition sp = new ScreenPosition(0, gotoLine);
                                bv.setCursorPosition(sp);
                            }
                            else
                            {
                                //setTemporaryMessage("Attempted to go beyond end of file.", 2, gameTime);
                                OnTemporaryMessage(new TextEventArgs("Attempted to go beyond end of file.", 2, gameTime));
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Logger.logMsg("Probably got junk in the goto line dialog");
                        //setTemporaryMessage("Lines are identified by numbers.", 2, gameTime);
                        OnTemporaryMessage(new TextEventArgs("Lines are identified by numbers.", 2, gameTime));
                    }

                    m_gotoLine = "";
                    m_brazilContext.m_state = State.Test("TextEditing");
                }
                else
                {
                    // Insert a line into the editor
                    //
                    string indent = "";

                    try
                    {
                        indent = m_context.m_project.getConfigurationValue("AUTOINDENT");
                    }
                    catch (Exception e)
                    {
                        Logger.logMsg("XygloXNA::processActionKeys) - couldn't get AUTOINDENT from config - " + e.Message);
                    }

                    if (bv.gotHighlight())
                    {
                        bv.replaceCurrentSelection(m_context.m_project, "\n");
                    }
                    else
                    {
                        bv.insertNewLine(m_context.m_project, indent);
                    }
                    // NEED THIS
                    //updateSmartHelp();
                }
                consumed = true;
            }

            return consumed;
        }

        /// <summary>
        /// Generate a model from the Project
        /// </summary>
        public void generateTreeModel()
        {
            Logger.logMsg("XygloXNA::generateTreeModel() - starting");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            // Firstly get a root directory for the FileBuffer tree
            //
            string fileRoot = m_context.m_project.getFileBufferRoot();

            TreeBuilderGraph rG = m_treeBuilder.buildTreeFromFiles(fileRoot, m_context.m_project.getNonNullFileBuffers());

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
        /// Process key combinations and commands from the keyboard - return true if we've
        /// captured a command so we don't print that character
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public bool processCombinationsCommands(GameTime gameTime, List<KeyAction> keyActionList, EyeHandler eyeHandler)
        {
            bool rC = false;
            List<Keys> keyList = new List<Keys>();
            foreach (KeyAction keyAction in keyActionList)
            {
                keyList.Add(keyAction.m_key);
            }

            if (m_brazilContext.m_confirmState.equals("ConfirmQuit") && keyList.Contains(Keys.Y))
            {
                m_confirmQuit = true;
                //checkExit(gameTime, true);
                OnCleanExitEvent(new CleanExitEventArgs(gameTime, true));
                return true;
            }

            // This could be null
            //
            BufferView bv = m_context.m_project.getSelectedBufferView();

            // Check confirm state - this works out the complicated statuses of open file buffers
            //
            if (!m_brazilContext.m_confirmState.equals("None"))
            {
                if (keyList.Contains(Keys.Y))
                {
                    Logger.logMsg("XygloXNA::processCombinationsCommands() - confirm y/n");
                    try
                    {
                        if (m_brazilContext.m_confirmState.equals("FileSave"))
                        {
                            // Select a file path if we need one
                            //
                            if (bv.getFileBuffer().getFilepath() == "")
                            {
                                selectSaveFile(gameTime);
                            }
                            else
                            {
                                // Attempt save
                                //
                                if (checkFileSave())
                                {
                                    // Save has completed without error
                                    //
                                    //setTemporaryMessage("Saved.", 2, gameTime);
                                    OnTemporaryMessage(new TextEventArgs("Saved.", 2, gameTime));
                                }

                                m_brazilContext.m_state = State.Test("TextEditing");
                                rC = true;
                            }
                        }
                        else if (m_brazilContext.m_confirmState.equals("FileSaveCancel"))
                        {
                            // First of all save all open buffers we can write and save
                            // a list of all those we can't
                            //
                            m_filesToWrite = new List<FileBuffer>();

                            foreach (FileBuffer fb in m_context.m_project.getFileBuffers())
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
                                //checkExit(gameTime);
                                OnCleanExitEvent(new CleanExitEventArgs(gameTime));
                            }
                        }
                        else if (m_brazilContext.m_confirmState.equals("CancelBuild"))
                        {
                            Logger.logMsg("XygloXNA::processCombinationsCommands() - cancel build");
                            m_buildProcess.Close();
                            m_buildProcess = null;
                        }
                        else if (m_brazilContext.m_confirmState.equals("ConfirmQuit"))
                        {
                            m_confirmQuit = true;
                            //checkExit(gameTime, true);
                            OnCleanExitEvent(new CleanExitEventArgs(gameTime, true));
                        }
                        rC = true; // consume this letter
                    }
                    catch (Exception e)
                    {
                        //setTemporaryMessage("Save failed with \"" + e.Message + "\".", 5, gameTime);
                        OnTemporaryMessage(new TextEventArgs("Save failed with \"" + e.Message + "\".", 5, gameTime));
                    }

                    m_brazilContext.m_confirmState.set("None");
                }
                else if (keyList.Contains(Keys.N))
                {
                    // If no for single file save then continue - if no for FileSaveCancel then quit
                    //
                    if (m_brazilContext.m_confirmState.equals("FileSave"))
                    {
                        //m_temporaryMessage = "";
                        OnTemporaryMessage(new TextEventArgs("", 0.0f, gameTime));
                        m_brazilContext.m_confirmState.set("None");
                    }
                    else if (m_brazilContext.m_confirmState.equals("FileSaveCancel"))
                    {
                        // Exit nicely
                        //
                        //checkExit(gameTime, true);
                        OnCleanExitEvent(new CleanExitEventArgs(gameTime, true));
                    }
                    else if (m_brazilContext.m_confirmState.equals("CancelBuild"))
                    {
                        //setTemporaryMessage("Continuing build..", 2, gameTime);
                        OnTemporaryMessage(new TextEventArgs("Continuing build..", 2, gameTime));
                        m_brazilContext.m_confirmState.set("None");
                    }
                    else if (m_brazilContext.m_confirmState.equals("ConfirmQuit"))
                    {
                        //setTemporaryMessage("Cancelled quit", 2, gameTime);
                        OnTemporaryMessage(new TextEventArgs("Cancelled quit", 2, gameTime));
                        m_brazilContext.m_confirmState.set("None");
                    }
                    rC = true; // consume this letter
                }
                else if (keyList.Contains(Keys.C) && m_brazilContext.m_confirmState.equals("FileSaveCancel"))
                {
                    //setTemporaryMessage("Cancelled Quit.", 0.5, gameTime);
                    OnTemporaryMessage(new TextEventArgs("Cancelled Quit.", 0.5, gameTime));
                    m_brazilContext.m_confirmState.set("None");
                    rC = true;
                }
            }
            else if (m_keyboard.isCtrlDown() && !m_keyboard.isAltDown())  // CTRL down and no ALT
            {
                if (keyList.Contains(Keys.C)) // Copy
                {
                    if (m_brazilContext.m_state.equals("Configuration") && m_editConfigurationItem)
                    {
                        Logger.logMsg("XygloXNA::processCombinationsCommands() - copying from configuration");
                        System.Windows.Forms.Clipboard.SetText(m_editConfigurationItemValue);
                    }
                    else
                    {
                        if (bv != null)
                        {
                            Logger.logMsg("XygloXNA::processCombinationsCommands() - copying to clipboard");
                            string text = bv.getSelection(m_context.m_project).getClipboardString();

                            // We can only set this is the text is not empty
                            if (text != "")
                            {
                                System.Windows.Forms.Clipboard.SetText(text);
                            }
                            rC = true;
                        }
                    }
                }
                else if (keyList.Contains(Keys.X)) // Cut
                {
                    if (m_brazilContext.m_state.equals("Configuration") && m_editConfigurationItem)
                    {
                        Logger.logMsg("XygloXNA::processCombinationsCommands() - cutting from configuration");
                        System.Windows.Forms.Clipboard.SetText(m_editConfigurationItemValue);
                        m_editConfigurationItemValue = "";
                    }
                    else
                    {
                        if (bv != null)
                        {
                            Logger.logMsg("XygloXNA::processCombinationsCommands() - cut");

                            System.Windows.Forms.Clipboard.SetText(bv.getSelection(m_context.m_project).getClipboardString());
                            bv.deleteCurrentSelection(m_context.m_project);
                            rC = true;
                        }
                    }
                }
                else if (keyList.Contains(Keys.V)) // Paste
                {
                    if (System.Windows.Forms.Clipboard.ContainsText())
                    {
                        if (m_brazilContext.m_state.equals("Configuration") && m_editConfigurationItem)
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
                            // For file open/project open mode then allow cutting and pasting of the
                            // directory from buffer (if it's a valid directory)
                            //
                            if (m_brazilContext.m_state.equals("ProjectOpen") || m_brazilContext.m_state.equals("FileOpen"))
                            {
                                if (Directory.Exists(System.Windows.Forms.Clipboard.GetText()))
                                {
                                    m_context.m_fileSystemView.setDirectory(System.Windows.Forms.Clipboard.GetText());
                                }
                            }
                            else
                            {
                                Logger.logMsg("XygloXNA::processCombinationsCommands() - pasting text");
                                // If we have a selection then replace it - else insert
                                //
                                if (bv.gotHighlight())
                                {
                                    bv.replaceCurrentSelection(m_context.m_project, System.Windows.Forms.Clipboard.GetText());
                                }
                                else
                                {
                                    bv.insertText(m_context.m_project, System.Windows.Forms.Clipboard.GetText());
                                }
                                // NEED THIS
                                //updateSmartHelp();
                                rC = true;
                            }
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
                        if (bv.getFileBuffer().getUndoPosition() > 0)
                        {
                            bv.undo(m_context.m_project, 1);
                            // NEED THIS
                            //updateSmartHelp();
                        }
                        else
                        {
                            //setTemporaryMessage("Nothing to undo.", 1.0, gameTime);
                            OnTemporaryMessage(new TextEventArgs("Nothing to undo.", 1.0, gameTime));
                        }
                    }
                    catch (Exception e)
                    {
                        //System.Windows.Forms.MessageBox.Show("Undo stack is empty - " + e.Message);
                        Logger.logMsg("XygloXNA::processCombinationsCommands() - got exception " + e.Message);
                        //setTemporaryMessage("Nothing to undo with exception.", 2, gameTime);
                        OnTemporaryMessage(new TextEventArgs("Nothing to undo with exception.", 2, gameTime));
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
                        if (bv.getFileBuffer().getUndoPosition() <
                            bv.getFileBuffer().getCommandStackLength())
                        {
                            bv.redo(m_context.m_project, 1);

                            // NEED THIS
                            //updateSmartHelp();
                        }
                        else
                        {
                            //setTemporaryMessage("Nothing to redo.", 1.0, gameTime);
                            OnTemporaryMessage(new TextEventArgs("Nothing to redo.", 1.0, gameTime));
                        }
                    }
                    catch (Exception e)
                    {
                        //System.Windows.Forms.MessageBox.Show("Undo stack is empty - " + e.Message);
                        Logger.logMsg("XygloXNA::processCombinationsCommands() - got exception " + e.Message);
                        //setTemporaryMessage("Nothing to redo.", 2, gameTime);
                        OnTemporaryMessage(new TextEventArgs("Nothing to redo.", 2, gameTime));
                    }

                    // Always return true as we've captured the event
                    //
                    rC = true;
                }
                else if (keyList.Contains(Keys.A))  // Select all
                {
                    bv.selectAll();
                    rC = true;
                }
                else if (keyList.Contains(Keys.OemPlus)) // increment bloom state
                {
                    if (m_keyboard.isShiftDown())
                    {
                        // Increment view size and flash the bufferview
                        //
                        m_fontScaleOriginal = bv.incrementViewSize(gameTime, m_context.m_graphics.GraphicsDevice.Viewport.Width, m_context.m_graphics.GraphicsDevice.Viewport.Height, m_context.m_fontManager);

                        m_currentFontScale = 0.0f;

                        //setActiveBuffer();
                        OnViewChange(new XygloViewEventArgs(m_context.m_project.getSelectedBufferView()));
                    }
                    else
                    {
                        m_context.m_bloomSettingsIndex = (m_context.m_bloomSettingsIndex + 1) % BloomSettings.PresetSettings.Length;
                        m_context.m_bloom.Settings = BloomSettings.PresetSettings[m_context.m_bloomSettingsIndex];
                        m_context.m_bloom.Visible = true;

                        //setTemporaryMessage("Bloom set to " + BloomSettings.PresetSettings[m_context.m_bloomSettingsIndex].Name, 3, gameTime);
                        OnTemporaryMessage(new TextEventArgs("Bloom set to " + BloomSettings.PresetSettings[m_context.m_bloomSettingsIndex].Name, 3, gameTime));
                    }
                }
                else if (keyList.Contains(Keys.OemMinus)) // decrement bloom state
                {
                    if (m_keyboard.isShiftDown())
                    {
                        // Modify view size and flash the bufferview
                        //
                        m_fontScaleOriginal = bv.decrementViewSize(gameTime, m_context.m_graphics.GraphicsDevice.Viewport.Width, m_context.m_graphics.GraphicsDevice.Viewport.Height, m_context.m_fontManager);

                        m_currentFontScale = 0.0f;
                        //setActiveBuffer();
                        OnViewChange(new XygloViewEventArgs(m_context.m_project.getSelectedBufferView()));
                    }
                    else
                    {
                        m_context.m_bloomSettingsIndex = (m_context.m_bloomSettingsIndex - 1);

                        if (m_context.m_bloomSettingsIndex < 0)
                        {
                            m_context.m_bloomSettingsIndex += BloomSettings.PresetSettings.Length;
                        }

                        m_context.m_bloom.Settings = BloomSettings.PresetSettings[m_context.m_bloomSettingsIndex];
                        m_context.m_bloom.Visible = true;
                        //setTemporaryMessage("Bloom set to " + BloomSettings.PresetSettings[m_context.m_bloomSettingsIndex].Name, 3, gameTime);
                        OnTemporaryMessage(new TextEventArgs("Bloom set to " + BloomSettings.PresetSettings[m_context.m_bloomSettingsIndex].Name, 3, gameTime));
                    }
                }
                else if (keyList.Contains(Keys.B)) // Toggle bloom
                {
                    m_context.m_bloom.Visible = !m_context.m_bloom.Visible;
                    //setTemporaryMessage("Bloom " + (m_context.m_bloom.Visible ? "on" : "off"), 3, gameTime);
                    OnTemporaryMessage(new TextEventArgs("Bloom " + (m_context.m_bloom.Visible ? "on" : "off"), 3, gameTime));
                }

            }
            else if (m_keyboard.isAltDown() && !m_keyboard.isCtrlDown()) // ALT down action and no CTRL down
            {
                if (keyList.Contains(Keys.S) && bv.getFileBuffer().isModified())
                {
                    // If we want to confirm save then ask
                    //
                    if (m_confirmFileSave)
                    {
                        //setTemporaryMessage("Confirm Save? Y/N", 0, gameTime);
                        OnTemporaryMessage(new TextEventArgs("Confirm Save? Y/N", 0, gameTime));
                        m_brazilContext.m_confirmState.set("FileSave");
                    }
                    else  // just save
                    {
                        // Select a file path if we need one
                        //
                        if (bv.getFileBuffer().getFilepath() == "")
                        {
                            m_saveAsExit = false;
                            selectSaveFile(gameTime);
                        }
                        else
                        {
                            // Attempt save
                            //
                            if (checkFileSave())
                            {
                                // Save has completed without error
                                //
                                //setTemporaryMessage("Saved.", 2, gameTime);
                                OnTemporaryMessage(new TextEventArgs("Saved.", 2, gameTime));
                            }

                            m_brazilContext.m_state = State.Test("TextEditing");
                        }
                        rC = true;
                    }
                }
                else if (keyList.Contains(Keys.A)) // Explicit save as
                {
                    m_saveAsExit = false;
                    selectSaveFile(gameTime);
                    rC = true;
                }
                else if (keyList.Contains(Keys.N)) // New BufferView on new FileBuffer
                {
                    m_brazilContext.m_state = State.Test("PositionScreenNew");
                    rC = true;
                }
                else if (keyList.Contains(Keys.B)) // New BufferView on same FileBuffer (copy the existing BufferView)
                {
                    m_brazilContext.m_state = State.Test("PositionScreenCopy");
                    rC = true;
                }
                else if (keyList.Contains(Keys.O)) // Open a file
                {
                    // NEED THIS
                    //selectOpenFile();
                    m_brazilContext.m_state = State.Test("FileOpen");
                    rC = true;
                }
                else if (keyList.Contains(Keys.P)) // Open a project
                {
                    m_brazilContext.m_state = State.Test("ProjectOpen");
                    rC = true;
                }
                else if (keyList.Contains(Keys.T)) // New project
                {
                    m_brazilContext.m_state = State.Test("ProjectNew");
                    rC = true;
                }
                else if (keyList.Contains(Keys.E)) // Export
                {
                    OnCommandEvent(new CommandEventArgs(gameTime, XygloCommand.UrhoExport));
                }
                else if (keyList.Contains(Keys.H)) // Show the help screen
                {
                    // Reset page position and set information mode
                    //
                    m_textScreenPositionY = 0;
                    m_brazilContext.m_state = State.Test("Help");
                    rC = true;
                }
                else if (keyList.Contains(Keys.I)) // Show the information screen
                {
                    // Reset page position and set information mode
                    //
                    m_textScreenPositionY = 0;
                    m_brazilContext.m_state = State.Test("Information");
                    rC = true;
                }
                else if (keyList.Contains(Keys.G)) // Show the config screen
                {
                    // Reset page position and set information mode
                    //
                    m_textScreenPositionY = 0;

                    // NEED THIS
                    //showConfigurationScreen();
                    m_brazilContext.m_state = State.Test("Configuration");
                    rC = true;
                }
                else if (keyList.Contains(Keys.C)) // Close current BufferView
                {
                    // NEED THIS
                    closeActiveBuffer(gameTime);
                    rC = true;
                }
                else if (keyList.Contains(Keys.D))
                {
                    m_brazilContext.m_state = State.Test("DiffPicker");
                    //setTemporaryMessage("Pick a BufferView to diff against", 5, gameTime);
                    OnTemporaryMessage(new TextEventArgs("Pick a BufferView to diff against", 5, gameTime));

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

                    m_brazilContext.m_state = State.Test("ManageProject"); // Manage the files in the project

                    // Copy current position to m_projectPosition - then rebuild model
                    //
                    if (bv != null)
                    {
                        m_projectPosition = bv.getPosition();
                    }
                    else
                    {
                        m_projectPosition = eyeHandler.getEyePosition();
                    }


                    m_projectPosition.X = -1000.0f;
                    m_projectPosition.Y = -1000.0f;

                    // NEED THIS
                    generateTreeModel();

                    // Fly to a new position in this mode to view the model
                    //
                    Vector3 newPos = m_projectPosition;
                    newPos.Z = 800.0f;
                    //flyToPosition(newPos);
                    OnChangePosition(new PositionEventArgs(newPos));
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
                    m_gotoBufferView += m_keyboard.getNumberKey();
                    rC = true;
                }

                // Don't do any state transitions in this class now
                //  
                //else if (keyList.Contains(Keys.F)) // Find text
                //{
                    //Logger.logMsg("XygloXNA::processCombinationsCommands() - find");
                    //m_state = State.Test("FindText");
                //}
                //else if (keyList.Contains(Keys.L)) // go to line
                //{
                    //Logger.logMsg("XygloXNA::processCombinationsCommands() - goto line");
                    //m_state = State.Test("GotoLine");
                //}

            }
            else if (m_keyboard.isWindowsDown()) // Windows keys
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
        /// Clear all drawables from a view - one component at a time
        /// </summary>
        /// <param name="view"></param>
        protected void clearDrawables(BrazilView view)
        {
            List<Component> removeList = new List<Component>();

            foreach(Component component in view.getApp().getComponents())
            {
                if (m_context.m_drawableComponents.ContainsKey(component))
                {
                    if (!m_context.m_drawableComponents[component].hasParent())
                        removeList.Add(component);
                }
            }

            // Free and remove component from dictionary
            //
            foreach (Component component in removeList)
            {
                m_context.m_drawableComponents[component] = null;
                m_context.m_drawableComponents.Remove(component);
            }
        }


        /// <summary>
        /// Close the active buffer view
        /// </summary>
        protected void closeActiveBuffer(GameTime gameTime)
        {
            if (m_context.m_project.getViews().Count > 1)
            {
                string message = "Removed BufferView.";
                int index = m_context.m_project.getViews().IndexOf(m_context.m_project.getSelectedView());

                if (m_context.m_project.getViews()[index].GetType() == typeof(BrazilView))
                {
                    BrazilView brazilView = (BrazilView)m_context.m_project.getViews()[index];

                    // Clear down any physics associated with these components
                    //
                    // Unset the interloper
                    //
                    m_brazilContext.m_interloper = null;

                    // Clear the physics handler
                    //m_context.m_physicsHandler.clearAppComponents(brazilView.getApp().getComponents());
                    m_context.m_physicsHandler.clearAll();

                    // Remove drawables
                    //
                    foreach (Component component in brazilView.getApp().getComponents())
                    {
                        m_context.m_drawableComponents[component] = null;
                        m_context.m_drawableComponents.Remove(component);
                    }

                    // Also reset any statuses in the app components
                    //
                    brazilView.getApp().reset();

                    // Clear any highlight list
                    //
                    brazilView.getApp().clearHighlights();

                    // Need to clear down all the drawables associated with this component
                    //
                    clearDrawables(brazilView);

                    // Set a message
                    //
                    message = "Removed BrazilView.";
                }

                // Remove view
                //
                m_context.m_project.removeView(m_context.m_project.getSelectedView());

                // Ensure that the index is not greater than number of views left
                //
                if (index > m_context.m_project.getViews().Count - 1)
                    index = m_context.m_project.getViews().Count - 1;

                m_context.m_project.setSelectedViewId(index);
                //setActiveBuffer(index);
                OnViewChange(new XygloViewEventArgs(m_context.m_project.getViews()[index]));
                OnTemporaryMessage(new TextEventArgs(message, 2, gameTime));
            }
            else
            {
                //setTemporaryMessage("Can't remove the last BufferView.", 2, gameTime);
                OnTemporaryMessage(new TextEventArgs("Can't remove the last View.", 2, gameTime));
            }
        }

        /// <summary>
        /// Completing a File->Save operation
        /// </summary>
        /// <param name="gameTime"></param>
        protected void completeSaveFile(GameTime gameTime)
        {
            try
            {
                // Could be null
                BufferView bv = m_context.m_project.getSelectedBufferView();

                checkFileSave();

                if (m_filesToWrite != null && m_filesToWrite.Count > 0)
                {
                    m_filesToWrite.RemoveAt(0);
                    Logger.logMsg("XygloXNA::completeSaveFile() - files remaining to be written " + m_filesToWrite.Count);
                }

                Vector3 newPosition = bv.getEyePosition();
                newPosition.Z = 500.0f;

                //flyToPosition(newPosition);
                OnChangePosition(new PositionEventArgs(newPosition));
                m_brazilContext.m_state = State.Test("TextEditing");

                //setTemporaryMessage("Saved.", 2, gameTime);
                OnTemporaryMessage(new TextEventArgs("Saved.", 2, gameTime));
            }
            catch (Exception)
            {
                // Could be null
                BufferView bv = m_context.m_project.getSelectedBufferView();

                //setTemporaryMessage("Failed to save to " + bv.getFileBuffer().getFilepath(), 2, gameTime);
                OnTemporaryMessage(new TextEventArgs("Failed to save to " + bv.getFileBuffer().getFilepath(), 2, gameTime));
            }
        }

        /// <summary>
        /// Locate a BufferView located in a specified direction - if we find one then
        /// we set that as the active buffer view.
        /// </summary>
        /// <param name="position"></param>
        protected void detectMove(BufferView.ViewPosition position, GameTime gameTime)
        {
            // Could be null
            XygloView bv = m_context.m_project.getSelectedView();

            // First get the position of a potential BufferView
            //
            BoundingBox searchBox = bv.calculateRelativePositionBoundingBox(position);

            // Store the id of the current view
            //
            int fromView = m_context.m_project.getSelectedViewId();

            // Search by index
            //
            bool found = false;
            int iterations = 10;

            while (found == false && iterations > 0)
            {
                for (int i = 0; i < m_context.m_project.getViews().Count; i++)
                {
                    if (i == fromView)
                        continue;

                    if (m_context.m_project.getViews()[i].getBoundingBox().Intersects(searchBox))
                    {
                        m_context.m_project.setSelectedViewId(i);
                        found = true;
                        break;
                    }
                }

                // If not found then iterate the position
                //
                if (!found)
                {
                    // On position
                    switch (position)
                    {
                        case XygloView.ViewPosition.Above:
                            searchBox.Min.Y += bv.getViewCharHeight() * m_context.m_project.getSelectedView().getBufferShowLength() / 2;
                            searchBox.Max.Y += bv.getViewCharHeight() * m_context.m_project.getSelectedView().getBufferShowLength() / 2;
                            break;

                        case XygloView.ViewPosition.Below:
                            searchBox.Min.Y -= bv.getViewCharHeight() * m_context.m_project.getSelectedView().getBufferShowLength() / 2;
                            searchBox.Max.Y -= bv.getViewCharHeight() * m_context.m_project.getSelectedView().getBufferShowLength() / 2;
                            break;

                        case XygloView.ViewPosition.Left:
                            searchBox.Min.X -= bv.getViewCharWidth() * m_context.m_project.getSelectedView().getBufferShowWidth() / 2;
                            searchBox.Max.X -= bv.getViewCharWidth() * m_context.m_project.getSelectedView().getBufferShowWidth() / 2;
                            break;

                        case XygloView.ViewPosition.Right:
                            searchBox.Min.X += bv.getViewCharWidth() * m_context.m_project.getSelectedView().getBufferShowWidth() / 2;
                            searchBox.Max.X += bv.getViewCharWidth() * m_context.m_project.getSelectedView().getBufferShowWidth() / 2;
                            break;

                        default:
                            // somethign went wrong
                            //
                            throw new XygloException("detectMove", "unrecognised view position");
                    }
                }

                iterations--;

            }

            // Now set the active buffer if we need to - if not give a warning
            //
            if (fromView != m_context.m_project.getSelectedViewId())
            {
                //setActiveBuffer();
                OnViewChange(new XygloViewEventArgs(m_context.m_project.getSelectedBufferView()));
            }
            else
            {
                //setTemporaryMessage("No BufferView.", 2.0, gameTime);
                OnTemporaryMessage(new TextEventArgs("Didn't find a BufferView there.", 2.0, gameTime));
            }
        }

        /// <summary>
        /// Traverse a directory and allow opening/saving at that point according to state
        /// </summary>
        protected void traverseDirectory(GameTime gameTime, bool readOnly = false, bool tailFile = false)
        {
            // Could be null
            BufferView bv = m_context.m_project.getSelectedBufferView();

            //string fileToOpen = m_fileSystemView.getHighlightedFile();
            if (m_context.m_fileSystemView.atDriveLevel())
            {
                // First extract the drive letter and set the path
                //
                m_context.m_fileSystemView.setHighlightedDrive();
            }

            // If we're not at the root directory
            //
            if (m_context.m_fileSystemView.getHighlightIndex() > 0)
            {
                string subDirectory = "";

                // Set the directory to the sub directory and reset the highlighter
                //
                try
                {
                    if (m_context.m_fileSystemView.getHighlightIndex() - 1 < m_context.m_fileSystemView.getDirectoryInfo().GetDirectories().Length)
                    {
                        // Set error directory in case of failure to test access
                        //
                        DirectoryInfo directoryToAccess = m_context.m_fileSystemView.getDirectoryInfo().GetDirectories()[m_context.m_fileSystemView.getHighlightIndex() - 1];
                        subDirectory = directoryToAccess.Name;

                        // Test access
                        //
                        DirectoryInfo[] testAccess = directoryToAccess.GetDirectories();

                        FileInfo[] testFiles;

                        // Filter for project files or not
                        //
                        if (m_brazilContext.m_state.equals("ProjectOpen"))
                            testFiles = directoryToAccess.GetFiles("*.fpr");
                        else
                            testFiles = directoryToAccess.GetFiles();

                        m_context.m_fileSystemView.setDirectory(directoryToAccess.FullName);

                        // Store last highlight index
                        //
                        m_lastHighlightIndex = m_context.m_fileSystemView.getHighlightIndex();

                        m_context.m_fileSystemView.setHighlightIndex(0);
                    }
                    else
                    {
                        int fileIndex = m_context.m_fileSystemView.getHighlightIndex() - 1 - m_context.m_fileSystemView.getDirectoryInfo().GetDirectories().Length;
                        FileInfo fileInfo = m_context.m_fileSystemView.getDirectoryInfo().GetFiles()[fileIndex];

                        Logger.logMsg("Friendler::traverseDirectory() - selected a file " + fileInfo.Name);

                        // Set these values and the status
                        //
                        m_fileIsReadOnly = readOnly;
                        m_fileIsTailing = tailFile;
                        m_selectedFile = fileInfo.FullName;

                        if (m_brazilContext.m_state.equals("FileOpen"))
                        {
                            // Now we need to choose a position for the new file we're opening
                            //
                            m_brazilContext.m_state = State.Test("PositionScreenOpen");
                        }
                        else if (m_brazilContext.m_state.equals("ProjectOpen"))
                        {
                            OnOpenProjectEvent(new OpenProjectEventArgs(fileInfo.FullName));
                        }
                        else if (m_brazilContext.m_state.equals("FileSaveAs"))
                        {
                            // Set the FileBuffer path
                            //
                            bv.getFileBuffer().setFilepath(m_selectedFile);

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
                                            //checkExit(gameTime);
                                            OnCleanExitEvent(new CleanExitEventArgs(gameTime));
                                        }
                                        else
                                        {
                                            //setActiveBuffer();
                                            OnViewChange(new XygloViewEventArgs(m_context.m_project.getSelectedBufferView()));
                                        }
                                    }
                                }
                                else
                                {
                                    m_brazilContext.m_state = State.Test("TextEditing");
                                    //setActiveBuffer();
                                    OnViewChange(new XygloViewEventArgs(m_context.m_project.getSelectedBufferView()));
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //setTemporaryMessage("XygloXNA::traverseDirectory() - Cannot access \"" + subDirectory + "\"", 2, gameTime);
                    OnTemporaryMessage(new TextEventArgs("XygloXNA::traverseDirectory() - Cannot access \"" + subDirectory + "\"", 2, gameTime));
                }
            }
        }

        // Set up the file save mode
        //
        public void selectSaveFile(GameTime gameTime)
        {
            // Enter this mode and clear and existing message
            //
            m_brazilContext.m_state = State.Test("FileSaveAs");

            OnTemporaryMessage(new TextEventArgs("", 0.0f, gameTime));
            //m_temporaryMessage = "";

            // Clear the filename
            //
            m_saveFileName = "";
        }

        /// <summary>
        /// Checks to see if we are licenced before saving
        /// </summary>
        /// <returns></returns>
        protected bool checkFileSave()
        {
            if (m_context.m_project.getLicenced())
            {
                m_context.m_project.getSelectedBufferView().getFileBuffer().save();
                return true;
            }

            //setTemporaryMessage("Can't save due to licence issue.", 10, m_context.m_gameTime);
            OnTemporaryMessage(new TextEventArgs("Can't save due to licence issue.", 10, m_context.m_gameTime));

            return false;
        }

        /// <summary>
        /// Update syntax highlighting if we need to 
        /// </summary>
        public void updateSmartHelp()
        {
            BufferView bv = m_context.m_project.getSelectedBufferView();
            if (bv == null || bv.getFileBuffer().getLineCount() == 0)
                return;

            // Update the syntax highlighting
            //
            if (m_context.m_project.getConfigurationValue("SYNTAXHIGHLIGHT").ToUpper() == "TRUE")
            {
                FileBuffer fb = bv.getFileBuffer();
                int startLine = bv.getBufferShowStartY();
                int endLine = bv.getBufferShowStartY() + bv.getBufferShowLength();

                // Limit end line as necessary
                if (endLine >= fb.getLineCount())
                {
                    endLine = Math.Max(fb.getLineCount() - 1, 0);
                }

                FilePosition startPosition = new FilePosition(0, startLine);
                FilePosition endPosition = new FilePosition(fb.getLine(endLine).Length, endLine);

                // Process immediately the current visible buffer
                //
                m_context.m_project.getSyntaxManager().generateHighlighting(fb, startPosition, endPosition, false);

                // Ensure that the syntax manager isn't processing highlights at the time of the next request.
                //
                m_context.m_project.getSyntaxManager().interruptProcessing();

                // We process all highlighting in the SmartHelpWorker thread.  Note that you have to do this
                // all in the same thread or the main GUI gets locked out.  Although it would make sense to
                // do just the on-screen bit in main thread we can minimise latency by keeping the highlight
                // thread sleep
                //
                m_smartHelpWorker.updateSyntaxHighlighting(m_context.m_project.getSyntaxManager(), bv.getFileBuffer(),
                    startLine, endLine);
            }
        }

        /// <summary>
        /// Process keys for BrazilViews
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="keyAction"></param>
        public bool processBrazilViewKey(GameTime gameTime, KeyAction keyAction)
        {
            // Skip for all non bufferviews
            if (m_context.m_project == null || m_context.m_project.getSelectedView().GetType() != typeof(BrazilView))
                return false;

            // ignore if Alt is down
            if (keyAction.withAlt())
                return false;

            List<Keys> keyList = new List<Keys>();
            keyList.Add(keyAction.m_key);
            bool consumed = false;

            BrazilView bv = (BrazilView)m_context.m_project.getSelectedView();

            // Now we need to process some BrazilView keys that can modify our app model
            //
            if (keyList.Contains(Keys.Space))
            {
                bv.getApp().toggleRunning();
                consumed = true;
            } else if (keyList.Contains(Keys.OemComma)) // less than
            {
                bv.getApp().rewind(1);
                consumed = true;
            }
            else if (keyList.Contains(Keys.OemPeriod)) // greater than
            {
                bv.getApp().fastForward(1);
                consumed = true;
            }
            else if (keyList.Contains(Keys.Home))
            {
                // Dump all the existing xyglo drawables for this app
                //
                //List<XygloXnaDrawable> removeList = new List<XygloXnaDrawable>();

                // Clear the physics handler
                //
                //m_context.m_physicsHandler.clearAppComponents(bv.getApp().getComponents());
                m_context.m_physicsHandler.clearAll();

                // Clear drawables
                //
                foreach(Component toRemove in bv.getApp().getComponents())
                {
                    if (m_context.m_drawableComponents.ContainsKey(toRemove))
                    {
                        m_context.m_drawableComponents[toRemove] = null;
                        m_context.m_drawableComponents.Remove(toRemove);
                    }
                }

                // Unset the interloper
                //
                m_brazilContext.m_interloper = null;

                // Also reset any statuses in the app components
                //
                bv.getApp().reset();

                // Clear any highlight list
                //
                bv.getApp().clearHighlights();

                consumed = true;
            }
            else if (keyList.Contains(Keys.Delete)) // Delete any selected objects
            {
                // We want to delete the drawable but also the component from the model
                //
                foreach(Component component in bv.getApp().getHighlightList())
                {
                    m_context.m_physicsHandler.removeBodyForDrawable(m_context.m_drawableComponents[component]);
                    m_context.m_drawableComponents[component] = null;
                    m_context.m_drawableComponents.Remove(component);
                    bv.getApp().getComponents().Remove(component);
                }

                // And clear highlights
                //
                bv.getApp().clearHighlights();

                consumed = true;
            }
            else if (keyList.Contains(Keys.Down))
            {
                foreach (Component component in bv.getApp().getHighlightList())
                    moveAppComponent(component, 0, 10, 0);

                consumed = true;
            }
            else if (keyList.Contains(Keys.Up))
            {
                if (bv.getApp().isPlaying())
                {
                    if (m_brazilContext.m_interloper != null)
                        m_context.m_physicsHandler.accelerate(m_context.m_drawableComponents[m_brazilContext.m_interloper], new Vector3(0, -200, 0));
                }
                else
                {
                    foreach (Component component in bv.getApp().getHighlightList())
                        moveAppComponent(component, 0, -10, 0);
                }
                consumed = true;
            }
            else if (keyList.Contains(Keys.Left))
            {   
                // If the app is running then move the interloper
                //
                if (bv.getApp().isPlaying())
                {
                        //case "MoveLeft":
                        // accelerate will accelerate in mid air or move
                        if (m_brazilContext.m_interloper != null)
                            m_context.m_physicsHandler.accelerate(m_context.m_drawableComponents[m_brazilContext.m_interloper], new Vector3(-10, 0, 0));

                        //break;


                }
                else
                {
                    foreach (Component component in bv.getApp().getHighlightList())
                        moveAppComponent(component, -10, 0, 0);
                }
                consumed = true;
            }
            else if (keyList.Contains(Keys.Right))
            {
                // If the app is running then move the interloper
                //
                if (bv.getApp().isPlaying())
                {
                    //case "MoveRight":
                        // accelerate will accelerate in mid air or move
                        if (m_brazilContext.m_interloper != null)
                            m_context.m_physicsHandler.accelerate(m_context.m_drawableComponents[m_brazilContext.m_interloper], new Vector3(10, 0, 0));
                      //  break;

                }
                else
                {
                    foreach (Component component in bv.getApp().getHighlightList())
                        moveAppComponent(component, 10, 0, 0);
                }
                consumed = true;
            }
            else if (keyList.Contains(Keys.I))
            {
                insertAppComponent(bv);
                consumed = true;
            }

            return consumed;
        }

        /// <summary>
        /// Insert an app component and the currently selected point (somehow decided by mouse position or something)
        /// </summary>
        protected void insertAppComponent(BrazilView brazilView)
        {
            //Ray current m_mouse.getPickRay()
            Vector3? position = m_context.m_project.getZeroPlaneIntersection(m_mouse.getPickRay());
            if (position != null)
            {
                Vector3 placePosition = (Vector3)position;

                
                // Now we need to allow for the app offset from the top left of the visible 
                //
                //Plane zeroPlane = 
                BoundingBox bb = m_context.m_project.getZeroPlaneBoundingBox(m_context.m_fov, brazilView.getEyePosition(), m_context.m_graphics.GraphicsDevice.Viewport.AspectRatio);
                Vector3 appPosition = brazilView.getPosition();

                //BrazilFlyingBlock bfb = new BrazilFlyingBlock(BrazilColour.Blue, XygloConvert.getBrazilVector3(placePosition), new BrazilVector3(20, 20, 20));
                brazilView.getApp().addComponent(m_context.m_componentPalette.getComponentInstance(placePosition - appPosition, 20));
            }
        }


        /// <summary>
        /// Move an app component by an amount
        /// </summary>
        /// <param name="component"></param>
        protected void moveAppComponent(Component component, int dx, int dy, int dz)
        {
            if (component is BrazilFlyingBlock)
            {
                BrazilFlyingBlock fb = (BrazilFlyingBlock)component;
                BrazilVector3 pos = fb.getPosition();
                pos.X += dx;
                pos.Y += dy;
                pos.Z += dz;
                fb.setPosition(pos);
            } else if (component is BrazilInterloper)
            {
                BrazilInterloper bi = (BrazilInterloper)component;
                BrazilVector3 pos = bi.getPosition();
                pos.X += dx;
                pos.Y += dy;
                pos.Z += dz;
                bi.setPosition(pos);
            } // etc

            // Now adjust the drawable (and also need to do this for physics!)
            //
            Vector3 existPos = m_context.m_drawableComponents[component].getPosition();
            existPos.X += dx;
            existPos.Y += dy;
            existPos.Z += dz;
            m_context.m_drawableComponents[component].setPosition(existPos);
            
            // Rebuild the vertex buffer
            //
            m_context.m_drawableComponents[component].buildBuffers(m_context.m_graphics.GraphicsDevice);

            //m_context.m_drawableComponents.Remove(component);
        }

        /// <summary>
        /// Process any keys that need to be printed
        /// </summary>
        /// <param name="gameTime"></param>
        public bool  processBufferViewKey(GameTime gameTime, KeyAction keyAction)
        {
            // Skip for all non bufferviews
            if (m_context.m_project == null || m_context.m_project.getSelectedBufferView() == null)
                return false;

            // Ok, let's see if we can translate a key
            //
            string key = m_keyboard.getKey(keyAction);

            if (key == "")
                return false;

            bool consumed = false;

            // Now handle
            //
            if (m_brazilContext.m_state.equals("FileSaveAs")) // File name
            {
                //Logger.logMsg("Writing letter " + key);
                m_saveFileName += key;
                consumed = true;
            }
            else if (m_brazilContext.m_state.equals("FileOpen"))
            {
                m_context.m_fileSystemView.jumpToString(key);
                consumed = true;
            }
            else if (m_brazilContext.m_state.equals("Configuration") && m_editConfigurationItem) // Configuration item
            {
                m_editConfigurationItemValue += key;
                consumed = true;
            }
            else if (m_brazilContext.m_state.equals("FindText"))
            {
                if (m_context.m_project.getSelectedView().GetType() == typeof(BufferView))
                {
                    BufferView bv = (BufferView)m_context.m_project.getSelectedView();
                    bv.appendToSearchText(key);
                    consumed = true;
                }
            }
            else if (m_brazilContext.m_state.equals("GotoLine"))
            {
                m_gotoLine += key;
                consumed = true;
            }
            else if (m_brazilContext.m_state.equals("TextEditing"))
            {
                XygloView view = m_context.m_project.getSelectedView();

                if (view.GetType() == typeof(BufferView))
                {
                    BufferView bv = (BufferView)view;

                    // Do we need to do some deletion or replacing?
                    //
                    if (bv.gotHighlight())
                    {
                        bv.replaceCurrentSelection(m_context.m_project, key);
                    }
                    else
                    {
                        bv.insertText(m_context.m_project, key);
                    }
                    updateSmartHelp();
                    consumed = true;
                }
            }

            return consumed;
        }

        /// <summary>
        /// Do font scaling on an acceleration - yes I don't quite know either
        /// </summary>
        /// <param name="acc"></param>
        public void doFontScaling(float acc)
        {
            // Font scaling - should this be in here?
            //
            if (m_currentFontScale == 0.0f)
            {
                m_currentFontScale = m_fontScaleOriginal;
            }
            else if (m_currentFontScale != 1.0f)
            {
                if (m_fontScaleOriginal < 1.0f)
                {
                    m_currentFontScale = m_fontScaleOriginal + ((1.0f - m_fontScaleOriginal) * acc);
                }
                else
                {
                    m_currentFontScale = m_fontScaleOriginal - ((m_fontScaleOriginal - 1.0f) * acc);
                }
            }
        }

        /// <summary>
        /// Page up a text screen
        /// </summary>
        public void textScreenPageDown(int textScreenLength)
        {
            if (m_textScreenPositionY + m_context.m_project.getSelectedView().getBufferShowLength() < textScreenLength)
                m_textScreenPositionY += DrawingHelper.getTextScreenLength();  //m_context.m_project.getSelectedView().getBufferShowLength();
        }

        /// <summary>
        /// Page down a text screen
        /// </summary>
        public void textScreenPageUp()
        {
            if (m_textScreenPositionY > 0)
                m_textScreenPositionY = m_textScreenPositionY - Math.Min(DrawingHelper.getTextScreenLength() /* m_context.m_project.getSelectedView().getBufferShowLength() */ , m_textScreenPositionY);
        }



        public int getDiffPosition() { return m_diffPosition; }
        public int getConfigPosition() { return m_configPosition; }
        public Differ getDiffer() { return m_differ; }

        public string getEditConfigurationItemValue() { return m_editConfigurationItemValue; }
        public bool getEditConfigurationItem() { return m_editConfigurationItem; }
        public void setEditConfigurationItem(bool item) { m_editConfigurationItem = item; }

        public ModelBuilder getModelBuilder() { return m_modelBuilder; }

        public string getGotoLine() { return m_gotoLine; }

        public List<FileBuffer> getFilesToWrite() { return m_filesToWrite; }
        public void setFilesToWrite(List<FileBuffer> buffer) { m_filesToWrite = buffer; }

        public int getTextScreenPositionY() { return m_textScreenPositionY; }
        /// <summary>
        /// Return selected file
        /// </summary>
        /// <returns></returns>
        public string getSelectedFile() { return m_selectedFile; }

        public bool getFileIsReadOnly() { return m_fileIsReadOnly; }
        public bool getFileIsTailing() { return m_fileIsTailing; }

        public bool getConfirmQuit() { return m_confirmQuit; }
        public bool getSaveAsExit() { return m_saveAsExit; }
        public void setSaveAsExit(bool value) { m_saveAsExit = value; }

        public double getCurrentFontScale() { return m_currentFontScale; }
        public void setCurrentFontScale(double scale) { m_currentFontScale = scale; }

        public double getFontScaleOriginal() { return m_fontScaleOriginal; }
        public void setFontScaleOriginal(double scale) { m_fontScaleOriginal = scale; }

        public string getSaveFileName() { return m_saveFileName; }

        // Some contexts and 'globals'
        //
        protected XygloContext m_context;
        protected BrazilContext m_brazilContext;
        protected XygloKeyboard m_keyboard;
        protected XygloGraphics m_graphics;

        /// <summary>
        /// Position in configuration list when selecting something
        /// </summary>
        protected int m_configPosition;

        /// <summary>
        /// Position we are in the diff
        /// </summary>
        protected int m_diffPosition = 0;

        /// <summary>
        /// A local Differ object
        /// </summary>
        protected Differ m_differ = null;

        /// <summary>
        /// Use this to store number when we've got ALT down - to select a new BufferView
        /// </summary>
        protected string m_gotoBufferView = "";

        /// <summary>
        /// If we're in the Configuration state then look at this variable
        /// </summary>
        protected bool m_editConfigurationItem = false;

        /// <summary>
        /// The new value of the configuration item
        /// </summary>
        protected string m_editConfigurationItemValue;

        /// <summary>
        /// The index of the last directory we went into so we can save it
        /// </summary>
        protected int m_lastHighlightIndex = 0;

        /// <summary>
        /// Model builder realises a model from a tree
        /// </summary>
        protected ModelBuilder m_modelBuilder;

        /// Goto line string holder
        /// </summary>
        protected string m_gotoLine = "";

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
        /// Exit after save as
        /// </summary>
        protected bool m_saveAsExit = false;

        /// <summary>
        /// Flag used to confirm quit
        /// </summary>
        protected bool m_confirmQuit = false;

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
        /// Turn on and off file save confirmation
        /// </summary>
        protected bool m_confirmFileSave = false;

        /// <summary>
        /// Used to hold initial fractional value of a target font size when changing font sizes
        /// </summary>
        protected double m_fontScaleOriginal;

        /// <summary>
        /// Holds current font scale whilst scaling current BufferView
        /// </summary>
        protected double m_currentFontScale;

        /// <summary>
        /// A variable we use to store our save filename as we edit it (we have no forms)
        /// </summary>
        protected string m_saveFileName;

        /// <summary>
        /// Text information screen y offset for page up and page down purposes
        /// </summary>
        protected int m_textScreenPositionY = 0;

        /// <summary>
        /// Generate a tree from a Friendlier structure
        /// </summary>
        protected TreeBuilder m_treeBuilder = new TreeBuilder();

        /// <summary>
        /// The position where the project model will be viewable
        /// </summary>
        protected Vector3 m_projectPosition = Vector3.Zero;

        /// <summary>
        /// Smarthelp worker - this is the same reference as that from XygloXNA
        /// </summary>
        protected SmartHelpWorker m_smartHelpWorker;

        /// <summary>
        /// Store a reference to the XygloMouse handler
        /// </summary>
        protected XygloMouse m_mouse;
    }
}
