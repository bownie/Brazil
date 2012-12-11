using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Permissions;
using Microsoft.Xna.Framework;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Manage keys and combinations and route happenings
    /// </summary>
    public class XygloKeyboardHandler : XygloEventEmitter
    {
        public XygloKeyboardHandler(XygloContext context, BrazilContext brazilContext, XygloKeyboard keyboard)
        {
            m_context = context;
            m_brazilContext = brazilContext;
            m_keyboard = keyboard;
        }

        /*
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
                if (m_brazilContext.m_state.equals("FileSaveAs") || m_brazilContext.m_state.equals("FileOpen"))
                {
                    if (m_context.m_fileSystemView.getHighlightIndex() > 0)
                    {
                        m_context.m_fileSystemView.incrementHighlightIndex(-1);
                    }
                }
                else if (m_brazilContext.m_state.equals("ManageProject") || (m_brazilContext.m_state.equals("Configuration") && m_editConfigurationItem == false)) // Configuration changes
                {
                    if (m_configPosition > 0)
                    {
                        m_configPosition--;
                    }
                }
                else if (m_brazilContext.m_state.equals("DiffPicker"))
                {
                    if (m_diffPosition > 0)
                    {
                        m_diffPosition--;
                    }
                }
                else
                {
                    if (m_keyboard.isAltDown() && m_keyboard.isShiftDown()) // Do zoom
                    {
                        m_mouse.setZoomLevel(m_context.m_zoomLevel - m_context.m_zoomStep, m_eye);
                    }
                    else if (m_keyboard.isAltDown())
                    {
                        // Attempt to move right if there's a BufferView there
                        //
                        detectMove(BufferView.ViewPosition.Above, gameTime);
                    }
                    else
                    {
                        ScreenPosition sP = m_context.m_project.getSelectedBufferView().getCursorPosition();
                        m_context.m_project.getSelectedBufferView().moveCursorUp(m_context.m_project, false, m_keyboard.isShiftDown());

                        if (m_keyboard.isShiftDown())
                        {
                            m_context.m_project.getSelectedBufferView().extendHighlight(sP);  // Extend 
                        }
                        else
                        {
                            m_context.m_project.getSelectedBufferView().noHighlight(); // Disable
                        }
                    }
                }
            }
            //else if (keyActionList.Find(item => item.m_key == Keys.Down && item.m_modifier == KeyboardModifier.None).ToString() != "")
            else if (keyList.Contains(Keys.Down))
            {
                if (m_brazilContext.m_state.equals("FileSaveAs") || m_brazilContext.m_state.equals("FileOpen"))
                {
                    if (m_context.m_fileSystemView.atDriveLevel())
                    {
                        // Drives are highlighted slightly differently to directories as the zero index is 
                        // counted for drives (1 for directories) hence the adjustment in the RH term
                        //
                        if (m_context.m_fileSystemView.getHighlightIndex() < m_context.m_fileSystemView.countActiveDrives() - 1)
                        {
                            m_context.m_fileSystemView.incrementHighlightIndex(1);
                        }
                    }
                    else if (m_context.m_fileSystemView.getHighlightIndex() < m_context.m_fileSystemView.getDirectoryLength())
                    {
                        m_context.m_fileSystemView.incrementHighlightIndex(1);
                    }
                }
                else if (m_brazilContext.m_state.equals("Configuration") && m_editConfigurationItem == false) // Configuration changes
                {
                    if (m_configPosition < m_context.m_project.getConfigurationListLength() - 1)
                    {
                        m_configPosition++;
                    }
                }
                else if (m_brazilContext.m_state.equals("ManageProject"))
                {
                    if (m_configPosition < m_modelBuilder.getLeafNodesPlaces() - 1)
                    {
                        m_configPosition++;
                    }
                }
                else if (m_brazilContext.m_state.equals("DiffPicker"))
                {
                    if (m_differ != null && m_diffPosition < m_differ.getMaxDiffLength())
                    {
                        m_diffPosition++;
                    }
                }
                else
                {
                    if (m_keyboard.isAltDown() && m_keyboard.isShiftDown()) // Do zoom
                    {
                        m_context.m_zoomLevel += m_context.m_zoomStep;
                        setActiveBuffer();
                    }
                    else if (m_keyboard.isAltDown())
                    {
                        // Attempt to move right if there's a BufferView there
                        //
                        detectMove(BufferView.ViewPosition.Below, gameTime);
                    }
                    else
                    {
                        ScreenPosition sP = m_context.m_project.getSelectedBufferView().getCursorPosition();
                        m_context.m_project.getSelectedBufferView().moveCursorDown(false, m_keyboard.isShiftDown());

                        if (m_keyboard.isShiftDown())
                        {
                            m_context.m_project.getSelectedBufferView().extendHighlight(sP);
                        }
                        else
                        {
                            m_context.m_project.getSelectedBufferView().noHighlight(); // Disable
                        }
                    }
                }
            }
            else if (keyList.Contains(Keys.Left))
            {
                if (m_brazilContext.m_state.equals("FileSaveAs") || m_brazilContext.m_state.equals("FileOpen"))
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
                        }
                        else
                        {
                            parDirectory = m_context.m_fileSystemView.getParent().Name;
                            DirectoryInfo[] testAccess = m_context.m_fileSystemView.getParent().GetDirectories();
                            FileInfo[] testFiles = m_context.m_fileSystemView.getParent().GetFiles();

                            m_context.m_fileSystemView.setDirectory(m_context.m_fileSystemView.getParent().FullName);
                            m_context.m_fileSystemView.setHighlightIndex(m_lastHighlightIndex);
                        }
                    }
                    catch (Exception)
                    {
                        setTemporaryMessage("Cannot access " + parDirectory.ToString(), 2, gameTime);
                    }
                }
                else
                {
                    // Store cursor position
                    //
                    ScreenPosition sP = m_context.m_project.getSelectedBufferView().getCursorPosition();

                    if (m_keyboard.isCtrlDown())
                    {
                        m_context.m_project.getSelectedBufferView().wordJumpCursorLeft();
                    }
                    else if (m_keyboard.isAltDown())
                    {
                        // Attempt to move right if there's a BufferView there
                        //
                        detectMove(BufferView.ViewPosition.Left, gameTime);
                    }
                    else
                    {
                        m_context.m_project.getSelectedBufferView().moveCursorLeft(m_context.m_project, m_keyboard.isShiftDown());
                    }

                    if (m_keyboard.isShiftDown())
                    {
                        m_context.m_project.getSelectedBufferView().extendHighlight(sP);  // Extend
                    }
                    else
                    {
                        m_context.m_project.getSelectedBufferView().noHighlight(); // Disable
                    }
                }
            }
            else if (keyList.Contains(Keys.Right))
            {
                if (m_brazilContext.m_state.equals("FileSaveAs") || m_brazilContext.m_state.equals("FileOpen"))
                {
                    traverseDirectory(gameTime);
                }
                else
                {
                    // Store cursor position
                    //
                    ScreenPosition sP = m_context.m_project.getSelectedBufferView().getCursorPosition();

                    if (m_keyboard.isCtrlDown())
                    {
                        m_context.m_project.getSelectedBufferView().wordJumpCursorRight();
                    }
                    else if (m_keyboard.isAltDown())
                    {
                        // Attempt to move right if there's a BufferView there
                        //
                        detectMove(BufferView.ViewPosition.Right, gameTime);
                    }
                    else
                    {
                        m_context.m_project.getSelectedBufferView().moveCursorRight(m_context.m_project, m_keyboard.isShiftDown());
                    }

                    if (m_keyboard.isShiftDown())
                    {
                        m_context.m_project.getSelectedBufferView().extendHighlight(sP); // Extend
                    }
                    else
                    {
                        m_context.m_project.getSelectedBufferView().noHighlight(); // Disable
                    }
                }
            }
            else if (keyList.Contains(Keys.End))
            {
                ScreenPosition fp = m_context.m_project.getSelectedBufferView().getCursorPosition();
                ScreenPosition originalFp = fp;

                // Set X and allow for tabs
                //
                if (fp.Y < m_context.m_project.getSelectedBufferView().getFileBuffer().getLineCount())
                {
                    fp.X = m_context.m_project.getSelectedBufferView().getFileBuffer().getLine(fp.Y).Replace("\t", m_context.m_project.getTab()).Length;
                }
                m_context.m_project.getSelectedBufferView().setCursorPosition(fp);

                // Change the X offset if the row is longer than the visible width
                //
                if (fp.X > m_context.m_project.getSelectedBufferView().getBufferShowWidth())
                {
                    int bufferX = fp.X - m_context.m_project.getSelectedBufferView().getBufferShowWidth();
                    m_context.m_project.getSelectedBufferView().setBufferShowStartX(bufferX);
                }

                if (m_keyboard.isShiftDown())
                {
                    m_context.m_project.getSelectedBufferView().extendHighlight(originalFp); // Extend
                }
                else
                {
                    m_context.m_project.getSelectedBufferView().noHighlight(); // Disable
                }

            }
            else if (keyList.Contains(Keys.Home))
            {
                // Store cursor position
                //
                ScreenPosition sP = m_context.m_project.getSelectedBufferView().getCursorPosition();

                // Reset the cursor to zero
                //
                ScreenPosition fp = m_context.m_project.getSelectedBufferView().getFirstNonSpace(m_context.m_project);

                m_context.m_project.getSelectedBufferView().setCursorPosition(fp);

                // Reset any X offset to zero
                //
                m_context.m_project.getSelectedBufferView().setBufferShowStartX(0);

                if (m_keyboard.isShiftDown())
                {
                    m_context.m_project.getSelectedBufferView().extendHighlight(sP); // Extend
                }
                else
                {
                    m_context.m_project.getSelectedBufferView().noHighlight(); // Disable
                }
            }
            else if (keyList.Contains(Keys.F9)) // Spin anticlockwise though BVs
            {
                m_context.m_zoomLevel = 1000.0f;
                setActiveBuffer(BufferView.ViewCycleDirection.Anticlockwise);
            }
            else if (keyList.Contains(Keys.F10)) // Spin clockwise through BVs
            {
                m_context.m_zoomLevel = 1000.0f;
                setActiveBuffer(BufferView.ViewCycleDirection.Clockwise);
            }
            else if (keyList.Contains(Keys.F3))
            {
                doSearch(gameTime);
            }
            else if (keyList.Contains(Keys.F4))
            {
                m_context.m_project.setViewMode(Project.ViewMode.Fun);
                m_context.m_drawingHelper.startBanner(gameTime, "Friendlier\nv1.0", 5);
            }
            else if (keyList.Contains(Keys.F6))
            {
                doBuildCommand(gameTime);
            }
            else if (keyList.Contains(Keys.F7))
            {
                string command = m_context.m_project.getConfigurationValue("ALTERNATEBUILDCOMMAND");
                doBuildCommand(gameTime, command);
            }
            else if (keyList.Contains(Keys.F8))
            {
                startGame(gameTime);
            }
            else if (keyList.Contains(Keys.F11)) // Toggle full screen
            {
                if (m_context.m_project.isFullScreen())
                {
                    m_graphics.windowedMode(this);
                }
                else
                {
                    m_graphics.fullScreenMode(this);
                }
                setSpriteFont();
            }
            else if (keyList.Contains(Keys.F1))  // Cycle down through BufferViews
            {
                int newValue = m_context.m_project.getSelectedBufferViewId() - 1;
                if (newValue < 0)
                {
                    newValue += m_context.m_project.getBufferViews().Count;
                }

                m_context.m_project.setSelectedBufferViewId(newValue);
                setActiveBuffer();
            }
            else if (keyList.Contains(Keys.F2)) // Cycle up through BufferViews
            {
                int newValue = (m_context.m_project.getSelectedBufferViewId() + 1) % m_context.m_project.getBufferViews().Count;
                m_context.m_project.setSelectedBufferViewId(newValue);
                setActiveBuffer();
            }
            else if (keyList.Contains(Keys.PageDown))
            {
                if (m_brazilContext.m_state.equals("TextEditing"))
                {
                    // Store cursor position
                    //
                    ScreenPosition sP = m_context.m_project.getSelectedBufferView().getCursorPosition();

                    m_context.m_project.getSelectedBufferView().pageDown(m_context.m_project);

                    if (m_keyboard.isShiftDown())
                    {
                        m_context.m_project.getSelectedBufferView().extendHighlight(sP); // Extend
                    }
                    else
                    {
                        m_context.m_project.getSelectedBufferView().noHighlight(); // Disable
                    }
                }
                else if (m_brazilContext.m_state.equals("DiffPicker"))
                {
                    if (m_differ != null && m_diffPosition < m_differ.getMaxDiffLength())
                    {
                        m_diffPosition += m_context.m_project.getSelectedBufferView().getBufferShowLength();

                        if (m_diffPosition >= m_differ.getMaxDiffLength())
                        {
                            m_diffPosition = m_differ.getMaxDiffLength() - 1;
                        }
                    }
                }
            }
            else if (keyList.Contains(Keys.PageUp))
            {
                if (m_brazilContext.m_state.equals("TextEditing"))
                {
                    // Store cursor position
                    //
                    ScreenPosition sP = m_context.m_project.getSelectedBufferView().getCursorPosition();

                    m_context.m_project.getSelectedBufferView().pageUp(m_context.m_project);

                    if (m_keyboard.isShiftDown())
                    {
                        m_context.m_project.getSelectedBufferView().extendHighlight(sP); // Extend
                    }
                    else
                    {
                        m_context.m_project.getSelectedBufferView().noHighlight(); // Disable
                    }
                }
                else if (m_brazilContext.m_state.equals("DiffPicker"))
                {
                    if (m_diffPosition > 0)
                    {
                        m_diffPosition -= m_context.m_project.getSelectedBufferView().getBufferShowLength();

                        if (m_diffPosition < 0)
                        {
                            m_diffPosition = 0;
                        }
                    }
                }
            }
            else if (keyList.Contains(Keys.Scroll))
            {
                if (m_context.m_project.getSelectedBufferView().isLocked())
                {
                    m_context.m_project.getSelectedBufferView().setLock(false, 0);
                }
                else
                {
                    m_context.m_project.getSelectedBufferView().setLock(true, m_context.m_project.getSelectedBufferView().getCursorPosition().Y);
                }
            }
            else if (keyList.Contains(Keys.Tab)) // Insert a tab space
            {
                m_context.m_project.getSelectedBufferView().insertText(m_context.m_project, "\t");
                updateSmartHelp();
            }
            else if (keyList.Contains(Keys.Insert))
            {
                if (m_brazilContext.m_state.equals("ManageProject"))
                {
                    if (m_configPosition >= 0 && m_configPosition < m_modelBuilder.getLeafNodesPlaces())
                    {
                        string fileToEdit = m_modelBuilder.getSelectedModelString(m_configPosition);

                        BufferView bv = m_context.m_project.getBufferView(fileToEdit);

                        if (bv != null)
                        {
                            setActiveBuffer(bv);
                        }
                        else // create and activate
                        {
                            try
                            {
                                FileBuffer fb = m_context.m_project.getFileBuffer(fileToEdit);
                                bv = new BufferView(m_context.m_fontManager, m_context.m_project.getBufferViews()[0], BufferView.ViewPosition.Left);
                                bv.setFileBuffer(fb);
                                int bvIndex = m_context.m_project.addBufferView(bv);
                                setActiveBuffer(bvIndex);

                                Vector3 rootPosition = m_context.m_project.getBufferViews()[0].getPosition();
                                Vector3 newPosition2 = bv.getPosition();

                                Logger.logMsg(rootPosition.ToString() + newPosition2.ToString());
                                //bv.setFileBufferIndex(
                                fb.loadFile(m_context.m_project.getSyntaxManager());

                                if (m_context.m_project.getConfigurationValue("SYNTAXHIGHLIGHT").ToUpper() == "TRUE")
                                {
                                    //m_context.m_project.getSyntaxManager().generateAllHighlighting(fb, true);
                                    m_smartHelpWorker.updateSyntaxHighlighting(m_context.m_project.getSyntaxManager(), fb);
                                }

                                // Break out of Manage mode and back to editing
                                //
                                Vector3 newPosition = m_context.m_project.getSelectedBufferView().getLookPosition();
                                newPosition.Z = 500.0f;
                                m_brazilContext.m_state = State.Test("TextEditing");
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

                if (m_brazilContext.m_state.equals("FileSaveAs") && keyList.Contains(Keys.Back))
                {
                    // Delete charcters from the file name if we have one
                    //
                    if (m_saveFileName.Length > 0)
                    {
                        m_saveFileName = m_saveFileName.Substring(0, m_saveFileName.Length - 1);
                    }
                }
                else if (m_brazilContext.m_state.equals("FindText") && keyList.Contains(Keys.Back))
                {
                    string searchText = m_context.m_project.getSelectedBufferView().getSearchText();
                    // Delete charcters from the file name if we have one
                    //
                    if (searchText.Length > 0)
                    {
                        m_context.m_project.getSelectedBufferView().setSearchText(searchText.Substring(0, searchText.Length - 1));
                    }
                }
                else if (m_brazilContext.m_state.equals("GotoLine") && keyList.Contains(Keys.Back))
                {
                    if (m_gotoLine.Length > 0)
                    {
                        m_gotoLine = m_gotoLine.Substring(0, m_gotoLine.Length - 1);
                    }
                }
                else if (m_brazilContext.m_state.equals("Configuration") && m_editConfigurationItem && keyList.Contains(Keys.Back))
                {
                    if (m_editConfigurationItemValue.Length > 0)
                    {
                        m_editConfigurationItemValue = m_editConfigurationItemValue.Substring(0, m_editConfigurationItemValue.Length - 1);
                    }
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
                            setActiveBuffer();

                            // Rebuild the file model
                            //
                            generateTreeModel();

                            setTemporaryMessage("Removed " + fileToRemove + " from project", 5, m_context.m_gameTime);
                        }
                        else
                        {
                            Logger.logMsg("XygloXNA::processActionKeys() - failed to remove FileBuffer for " + fileToRemove);
                        }
                    }
                }
                else if (m_context.m_project.getSelectedBufferView().gotHighlight()) // If we have a valid highlighted selection then delete it (normal editing)
                {
                    // All the clever stuff with the cursor is done at the BufferView level and it also
                    // calls the command in the FileBuffer.
                    //
                    m_context.m_project.getSelectedBufferView().deleteCurrentSelection(m_context.m_project);
                    updateSmartHelp();
                }
                else // delete at cursor
                {
                    if (keyList.Contains(Keys.Delete))
                    {
                        m_context.m_project.getSelectedBufferView().deleteSingle(m_context.m_project);
                    }
                    else if (keyList.Contains(Keys.Back))
                    {
                        // Start with a file position from the screen position
                        //
                        FilePosition fp = m_context.m_project.getSelectedBufferView().screenToFilePosition(m_context.m_project);

                        // Get the character before the current one and backspace accordingly 

                        if (fp.X > 0)
                        {
                            string fetchLine = m_context.m_project.getSelectedBufferView().getCurrentLine();

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
                            fp.X = m_context.m_project.getSelectedBufferView().getFileBuffer().getLine(Convert.ToInt16(fp.Y)).Replace("\t", m_context.m_project.getTab()).Length;
                        }

                        m_context.m_project.getSelectedBufferView().setCursorPosition(new ScreenPosition(fp));

                        m_context.m_project.getSelectedBufferView().deleteSingle(m_context.m_project);
                    }
                    updateSmartHelp();
                }
            }
            else if (keyList.Contains(Keys.Enter))
            {
                //ScreenPosition fp = m_context.m_project.getSelectedBufferView().getCursorPosition();

                if (m_brazilContext.m_state.equals("FileSaveAs"))
                {
                    // Check that the filename is valid
                    //
                    if (m_saveFileName != "" && m_saveFileName != null)
                    {
                        m_context.m_project.getSelectedBufferView().getFileBuffer().setFilepath(m_context.m_fileSystemView.getPath() + m_saveFileName);

                        Logger.logMsg("XygloXNA::processActionKeys() - file name = " + m_context.m_project.getSelectedBufferView().getFileBuffer().getFilepath());

                        completeSaveFile(gameTime);

                        // Now if we have remaining files to write then we need to carry on saving files
                        //
                        if (m_filesToWrite != null)
                        {
                            //m_filesToWrite.Remove(m_context.m_project.getSelectedBufferView().getFileBuffer());

                            // If we have remaining files to edit then set the active BufferView to one that
                            // looks over this file - then fly to it and choose and file location.
                            //
                            if (m_filesToWrite.Count > 0)
                            {
                                m_context.m_project.setSelectedBufferView(m_filesToWrite[0]);
                                m_eye = m_context.m_project.getSelectedBufferView().getEyePosition();
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
                else if (m_brazilContext.m_state.equals("FileOpen"))
                {
                    traverseDirectory(gameTime);
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
                            if (gotoLine < m_context.m_project.getSelectedBufferView().getFileBuffer().getLineCount() - 1)
                            {
                                ScreenPosition sp = new ScreenPosition(0, gotoLine);
                                m_context.m_project.getSelectedBufferView().setCursorPosition(sp);
                            }
                            else
                            {
                                setTemporaryMessage("Attempted to go beyond end of file.", 2, gameTime);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Logger.logMsg("Probably got junk in the goto line dialog");
                        setTemporaryMessage("Lines are identified by numbers.", 2, gameTime);
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

                    if (m_context.m_project.getSelectedBufferView().gotHighlight())
                    {
                        m_context.m_project.getSelectedBufferView().replaceCurrentSelection(m_context.m_project, "\n");
                    }
                    else
                    {
                        m_context.m_project.getSelectedBufferView().insertNewLine(m_context.m_project, indent);
                    }
                    updateSmartHelp();
                }
            }
        }
        */

        /*
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

            if (m_brazilContext.m_confirmState.equals("ConfirmQuit") && keyList.Contains(Keys.Y))
            {
                m_confirmQuit = true;
                checkExit(gameTime, true);
                return true;
            }

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
                            if (m_context.m_project.getSelectedBufferView().getFileBuffer().getFilepath() == "")
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
                                checkExit(gameTime);
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
                            checkExit(gameTime, true);
                        }
                        rC = true; // consume this letter
                    }
                    catch (Exception e)
                    {
                        setTemporaryMessage("Save failed with \"" + e.Message + "\".", 5, gameTime);
                    }

                    m_brazilContext.m_confirmState.set("None");
                }
                else if (keyList.Contains(Keys.N))
                {
                    // If no for single file save then continue - if no for FileSaveCancel then quit
                    //
                    if (m_brazilContext.m_confirmState.equals("FileSave"))
                    {
                        m_temporaryMessage = "";
                        m_brazilContext.m_confirmState.set("None");
                    }
                    else if (m_brazilContext.m_confirmState.equals("FileSaveCancel"))
                    {
                        // Exit nicely
                        //
                        checkExit(gameTime, true);
                    }
                    else if (m_brazilContext.m_confirmState.equals("CancelBuild"))
                    {
                        setTemporaryMessage("Continuing build..", 2, gameTime);
                        m_brazilContext.m_confirmState.set("None");
                    }
                    else if (m_brazilContext.m_confirmState.equals("ConfirmQuit"))
                    {
                        setTemporaryMessage("Cancelled quit", 2, gameTime);
                        m_brazilContext.m_confirmState.set("None");
                    }
                    rC = true; // consume this letter
                }
                else if (keyList.Contains(Keys.C) && m_brazilContext.m_confirmState.equals("FileSaveCancel"))
                {
                    setTemporaryMessage("Cancelled Quit.", 0.5, gameTime);
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
                        Logger.logMsg("XygloXNA::processCombinationsCommands() - copying to clipboard");
                        string text = m_context.m_project.getSelectedBufferView().getSelection(m_context.m_project).getClipboardString();

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
                    if (m_brazilContext.m_state.equals("Configuration") && m_editConfigurationItem)
                    {
                        Logger.logMsg("XygloXNA::processCombinationsCommands() - cutting from configuration");
                        System.Windows.Forms.Clipboard.SetText(m_editConfigurationItemValue);
                        m_editConfigurationItemValue = "";
                    }
                    else
                    {
                        Logger.logMsg("XygloXNA::processCombinationsCommands() - cut");

                        System.Windows.Forms.Clipboard.SetText(m_context.m_project.getSelectedBufferView().getSelection(m_context.m_project).getClipboardString());
                        m_context.m_project.getSelectedBufferView().deleteCurrentSelection(m_context.m_project);
                        rC = true;
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
                            Logger.logMsg("XygloXNA::processCombinationsCommands() - pasting text");
                            // If we have a selection then replace it - else insert
                            //
                            if (m_context.m_project.getSelectedBufferView().gotHighlight())
                            {
                                m_context.m_project.getSelectedBufferView().replaceCurrentSelection(m_context.m_project, System.Windows.Forms.Clipboard.GetText());
                            }
                            else
                            {
                                m_context.m_project.getSelectedBufferView().insertText(m_context.m_project, System.Windows.Forms.Clipboard.GetText());
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
                        if (m_context.m_project.getSelectedBufferView().getFileBuffer().getUndoPosition() > 0)
                        {
                            m_context.m_project.getSelectedBufferView().undo(m_context.m_project, 1);
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
                        if (m_context.m_project.getSelectedBufferView().getFileBuffer().getUndoPosition() <
                            m_context.m_project.getSelectedBufferView().getFileBuffer().getCommandStackLength())
                        {
                            m_context.m_project.getSelectedBufferView().redo(m_context.m_project, 1);
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
                    m_context.m_project.getSelectedBufferView().selectAll();
                    rC = true;
                }
                else if (keyList.Contains(Keys.OemPlus)) // increment bloom state
                {
                    if (m_keyboard.isShiftDown())
                    {
                        m_fontScaleOriginal = m_context.m_project.getSelectedBufferView().incrementViewSize(m_context.m_graphics.GraphicsDevice.Viewport.Width, m_context.m_graphics.GraphicsDevice.Viewport.Height, m_context.m_fontManager);
                        m_currentFontScale = 0.0f;
                        setActiveBuffer();
                    }
                    else
                    {
                        m_context.m_bloomSettingsIndex = (m_context.m_bloomSettingsIndex + 1) % BloomSettings.PresetSettings.Length;
                        m_context.m_bloom.Settings = BloomSettings.PresetSettings[m_context.m_bloomSettingsIndex];
                        m_context.m_bloom.Visible = true;

                        setTemporaryMessage("Bloom set to " + BloomSettings.PresetSettings[m_context.m_bloomSettingsIndex].Name, 3, gameTime);
                    }
                }
                else if (keyList.Contains(Keys.OemMinus)) // decrement bloom state
                {
                    if (m_keyboard.isShiftDown())
                    {
                        m_fontScaleOriginal = m_context.m_project.getSelectedBufferView().decrementViewSize(m_context.m_graphics.GraphicsDevice.Viewport.Width, m_context.m_graphics.GraphicsDevice.Viewport.Height, m_context.m_fontManager);
                        m_currentFontScale = 0.0f;
                        setActiveBuffer();
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
                        setTemporaryMessage("Bloom set to " + BloomSettings.PresetSettings[m_context.m_bloomSettingsIndex].Name, 3, gameTime);
                    }
                }
                else if (keyList.Contains(Keys.B)) // Toggle bloom
                {
                    m_context.m_bloom.Visible = !m_context.m_bloom.Visible;
                    setTemporaryMessage("Bloom " + (m_context.m_bloom.Visible ? "on" : "off"), 3, gameTime);
                }

            }
            else if (m_keyboard.isAltDown() && !m_keyboard.isCtrlDown()) // ALT down action and no CTRL down
            {
                if (keyList.Contains(Keys.S) && m_context.m_project.getSelectedBufferView().getFileBuffer().isModified())
                {
                    // If we want to confirm save then ask
                    //
                    if (m_confirmFileSave)
                    {
                        setTemporaryMessage("Confirm Save? Y/N", 0, gameTime);
                        m_brazilContext.m_confirmState.set("FileSave");
                    }
                    else  // just save
                    {
                        // Select a file path if we need one
                        //
                        if (m_context.m_project.getSelectedBufferView().getFileBuffer().getFilepath() == "")
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

                            m_brazilContext.m_state = State.Test("TextEditing");
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
                    selectOpenFile();
                    rC = true;
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
                    m_brazilContext.m_state = State.Test("DiffPicker");
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

                    m_brazilContext.m_state = State.Test("ManageProject"); // Manage the files in the project

                    // Copy current position to m_projectPosition - then rebuild model
                    //
                    m_projectPosition = m_context.m_project.getSelectedBufferView().getPosition();
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
        */

        /// <summary>
        /// Locate a BufferView located in a specified direction - if we find one then
        /// we set that as the active buffer view.
        /// </summary>
        /// <param name="position"></param>
        protected void detectMove(BufferView.ViewPosition position, GameTime gameTime)
        {
            // First get the position of a potential BufferView
            //
            BoundingBox searchBox = m_context.m_project.getSelectedBufferView().calculateRelativePositionBoundingBox(position);

            // Store the id of the current view
            //
            int fromView = m_context.m_project.getSelectedBufferViewId();

            // Search by index
            //
            for (int i = 0; i < m_context.m_project.getBufferViews().Count; i++)
            {
                if (m_context.m_project.getBufferViews()[i].getBoundingBox().Intersects(searchBox))
                {
                    m_context.m_project.setSelectedBufferViewId(i);
                    break;
                }
            }

            // Now set the active buffer if we need to - if not give a warning
            //
            if (fromView != m_context.m_project.getSelectedBufferViewId())
            {
                //setActiveBuffer();
                OnBufferViewChange(new BufferViewEventArgs(m_context.m_project.getSelectedBufferView()));
            }
            else
            {
                //setTemporaryMessage("No BufferView.", 2.0, gameTime);
                OnTemporaryMessage(new TextEventArgs("No BufferView.", 2.0, gameTime));
            }
        }


        /// <summary>
        /// Position in configuration list when selecting something
        /// </summary>
        protected int m_configPosition;

        // Some contexts and 'globals'
        //
        protected XygloContext m_context;
        protected BrazilContext m_brazilContext;
        protected XygloKeyboard m_keyboard;
    }
}
