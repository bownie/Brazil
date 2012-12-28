#region File Description
//-----------------------------------------------------------------------------
// FileSystemView.cs
//
// Copyright (C) Xyglo Ltd. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;


namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Convenience Xyglo class to keep all the file and drive related stuff together
    /// </summary>
    public class FileSystemView
    {
        ///////////////////////// CONSTRUCTORS ////////////////////////
        public FileSystemView(XygloContext context, BrazilContext brazilContext, string path, Vector3 position, Project project, FontManager fontManager)
        {
            m_context = context;
            m_brazilContext = brazilContext;
            m_path = fixPathEnding(path);
            m_position = position;
            m_project = project;
            m_fontManager = fontManager;
            scanDirectory();
        }

        /// <summary>
        /// Set the project
        /// </summary>
        /// <param name="project"></param>
        public void setProject(Project project)
        {
            m_project = project;
        }


        ////////////////////////// METHODS ///////////////////////////

        /// <summary>
        /// Ensure that a path always ends in a backslash
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        protected string fixPathEnding(string path)
        {
            if (path.Length > 0)
            {
                if (path[path.Length - 1] != '\\')
                {
                    path += @"\";
                }
            }

            return path;
        }

        /// <summary>
        /// Return current directory
        /// </summary>
        /// <returns></returns>
        public string getPath()
        {
            return m_path;
        }

        /// <summary>
        /// Reset to this directory and test for directory and file access at this level
        /// </summary>
        /// <param name="directory"></param>
        public void setDirectory(string directory)
        {
            if (directory != null)
            {
                m_driveLevel = false;
                m_path = fixPathEnding(directory);
                scanDirectory();
            }
            else
            {
                m_driveLevel = true;
                m_directoryHighlight = 0;
            }
        }

        /// <summary>
        /// Take the drive letter of the current value m_directoryHighlight
        /// </summary>
        public void setHighlightedDrive()
        {
            int i = 0;
            foreach (DriveInfo drive in getDrives())
            {
                if (!drive.IsReady)
                {
                    continue;
                }

                if (i++ == m_directoryHighlight)
                {
                    setDirectory(drive.Name);
                    m_directoryHighlight = 0;
                    return;
                }
            }
        }

        /// <summary>
        /// Are we scanning at drive level?
        /// </summary>
        /// <returns></returns>
        public bool atDriveLevel()
        {
            return m_driveLevel;
        }

        /// <summary>
        /// Getting drive info for active drives
        /// </summary>
        /// <returns></returns>
        public DriveInfo[] getDrives()
        {
            return DriveInfo.GetDrives();
        }

        /// <summary>
        /// Return the count of active drives
        /// </summary>
        /// <returns></returns>
        public int countActiveDrives()
        {
            int count = 0;
            foreach (DriveInfo drive in getDrives())
            {
                if (drive.IsReady)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Fetch the directory information - simple wrapper
        /// </summary>
        /// <returns></returns>
        public DirectoryInfo getDirectoryInfo()
        {
            return m_directoryInfo;
        }

        /// <summary>
        /// Fetch the sub directories of this node
        /// </summary>
        /// <returns></returns>
        public DirectoryInfo[] getSubDirectories()
        {
            return m_directoryInfo.GetDirectories();
        }

        /// <summary>
        /// Get the parent directory
        /// </summary>
        /// <returns></returns>
        public DirectoryInfo getParent()
        {
            return m_directoryInfo.Parent;
        }

        // Get total number of directories and files
        //
        public int getDirectoryLength()
        {
            return (m_directoryInfo.GetDirectories().Length + m_directoryInfo.GetFiles().Length);
        }

        protected void scanDirectory()
        {
            // Attempt to get Directory and File info for this path
            //
            m_directoryInfo = new DirectoryInfo(m_path);
            m_fileInfo = new FileInfo(m_path);
        }

        public Vector3 getPosition()
        {
            return m_position;
        }

        public Vector3 getEyePosition()
        {
            Vector3 rV = m_position;
            rV.Y = -rV.Y; // invert Y
            rV.X += m_fontManager.getCharWidth(m_project.getSelectedView().getViewSize()) * m_project.getSelectedView().getBufferShowWidth() / 2;
            rV.Y -= m_fontManager.getLineSpacing(m_project.getSelectedView().getViewSize()) * m_project.getSelectedView().getBufferShowLength() / 2;
            rV.Z += 600.0f;
            return rV;
        }

        /// <summary>
        /// Get the highlight index
        /// </summary>
        /// <returns></returns>
        public int getHighlightIndex()
        {
            return m_directoryHighlight;
        }

        /// <summary>
        /// Set highlight position
        /// </summary>
        /// <param name="directoryHighlight"></param>
        public void setHighlightIndex(int directoryHighlight)
        {
            m_directoryHighlight = directoryHighlight;
        }

        /// <summary>
        /// Increment highlight position
        /// </summary>
        /// <param name="inc"></param>
        public void incrementHighlightIndex(int inc)
        {
            // Drives are highlighted slightly differently to directories as the zero index is 
            // counted for drives (1 for directories) hence the adjustment in the RH term
            //
            int testMax = m_driveLevel ? countActiveDrives() : getDirectoryLength() + 1;

            if (m_directoryHighlight + inc >= 0 && m_directoryHighlight + inc < testMax)
                m_directoryHighlight += inc;
        }

        /// <summary>
        /// Get the highlight position
        /// </summary>
        /// <returns></returns>
        public string getHighlightedFile()
        {
            string file = m_directoryInfo.FullName;

            // Now work out the directory or filename
            //
            return file;
        }

        public void directorySearch(string sDir, string filename)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    foreach (string f in Directory.GetFiles(d, filename))
                    {
                        m_fileHolder.Add(f);
                    }
                    directorySearch(d, filename);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        public List<string> getSearchDirectories()
        {
            return m_fileHolder;
        }

        public void clearSearchDirectories()
        {
            m_fileHolder.Clear();
        }

        /// <summary>
        /// Jump the selection to a certain string
        /// </summary>
        /// <param name="match"></param>
        public void jumpToString(string searchString)
        {
            if (m_driveLevel)
            {
                // First search the bottom half of the directory list for a matching first letter
                int i = m_directoryHighlight;
                bool matched = false;
                foreach (DriveInfo d in getDrives())
                {
                    if (i > m_directoryHighlight)
                    {
                        if (d.Name.ToLower() == searchString[0].ToString().ToLower())
                        {
                            m_directoryHighlight = i;
                            matched = true;
                            break;
                        }
                    }
                    i++;
                }

                // If not matched in the lower half then wrap around to the top
                //
                if (!matched)
                {
                    i = 0;
                    foreach(DriveInfo d in getDrives())
                    {
                        if (i++ < m_directoryHighlight)
                        {
                            if (d.Name.ToLower() == searchString[0].ToString().ToLower())
                            {
                                m_directoryHighlight = i;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                // For file and directories
                //
                List<string> fullList = new List<string>();
                foreach (DirectoryInfo dirInfo in getDirectoryInfo().GetDirectories())
                    fullList.Add(dirInfo.Name);

                foreach (FileInfo fileInfo in getDirectoryInfo().GetFiles())
                    fullList.Add(fileInfo.Name);

                bool found = false;

                // Search the bottom half
                //
                if (m_directoryHighlight < fullList.Count())
                {
                    for (int i = m_directoryHighlight; i < fullList.Count(); i++)
                    {
                        if (fullList[i][0].ToString().ToLower() == searchString[0].ToString().ToLower())
                        {
                            m_directoryHighlight = i + 1; // directories/files are 1 based
                            found = true;
                            break;
                        }
                    }
                }

                // If not found then wrap and search the top half
                //
                if (!found)
                {
                    for (int i = 0; i < m_directoryHighlight; i++)
                    {
                        if (fullList[i][0].ToString().ToLower() == searchString[0].ToString().ToLower())
                        {
                            m_directoryHighlight = i + 1; // directories/files are 1 based
                            break;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// This is a list of directories and files based on the current position of the FileSystemView
        /// </summary>
        /// <param name="gameTime"></param>
        public void drawDirectoryChooser(GameTime gameTime, XygloKeyboardHandler keyboardHandler, string temporaryMessage, double temporaryMessageEndTime)
        {
            // Could be null
            BufferView bv = m_context.m_project.getSelectedBufferView();

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
            m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), line, new Vector2((int)startPosition.X, (int)(startPosition.Y - (bv == null ? 0 : bv.getLineSpacing() * 3))), Color.White, 0, lineOrigin, 1.0f, 0, 0);

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
            int showOffset = (int)(((float)m_directoryHighlight) / showPage);

            // This works out where the list that we're showing should end
            //
            int endShowing = (m_directoryHighlight < dropStep ? dropStep : m_directoryHighlight) + (int)showPage;

            // Draw the drives
            //
            if (m_driveLevel)
            {
                foreach (DriveInfo d in getDrives())
                {
                    if (!d.IsReady)
                        continue;

                    if (lineNumber > m_directoryHighlight - dropStep
                        && lineNumber <= endShowing)
                    {
                        if (lineNumber < endShowing)
                        {
                            line = "[" + d.Name + "] " + d.VolumeLabel;
                        }
                        else
                        {
                            yPosition += m_context.m_fontManager.getOverlayFont().LineSpacing;
                            line = "...";
                        }

                        m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(),
                             line,
                             new Vector2((int)startPosition.X, (int)(startPosition.Y + yPosition)),
                             (lineNumber == m_directoryHighlight ? ColourScheme.getHighlightColour() : (lineNumber == endShowing ? Color.White : dirColour)),
                             0,
                             lineOrigin,
                             1.0f,
                             0, 0);

                        yPosition += m_context.m_fontManager.getOverlayFont().LineSpacing;
                    }

                    lineNumber++;
                }
            }
            else // This is where we draw Directories and Files
            {
                if (!Directory.Exists(m_path))
                    m_context.m_fileSystemView.setDirectory(@"C:\");

                // For drives and directories we highlight item 1  - not zero
                //
                lineNumber = 1;
                FileInfo[] fileInfo = getDirectoryInfo().GetFiles();
                DirectoryInfo[] dirInfo = getDirectoryInfo().GetDirectories();

#if DIRECTORY_CHOOSER_DEBUG
                Logger.logMsg("showPage = " + showPage);
                Logger.logMsg("showOffset = " + showOffset);
                Logger.logMsg("m_directoryHighlight = " + m_directoryHighlight);
#endif

                line = m_path + keyboardHandler.getSaveFileName();
                m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), line, new Vector2((int)startPosition.X, (int)startPosition.Y), (m_directoryHighlight == 0 ? ColourScheme.getHighlightColour() : dirColour), 0, lineOrigin, 1.0f, 0, 0);

                yPosition += m_context.m_fontManager.getOverlayFont().LineSpacing * 3.0f;

                foreach (DirectoryInfo d in dirInfo)
                {
                    if (lineNumber > m_directoryHighlight - dropStep && lineNumber <= endShowing)
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
                             (lineNumber == m_directoryHighlight ? ColourScheme.getHighlightColour() : (lineNumber == endShowing ? Color.White : dirColour)),
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
                    if (lineNumber > m_directoryHighlight - dropStep && lineNumber <= endShowing)
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
                                                 (lineNumber == m_directoryHighlight ? ColourScheme.getHighlightColour() : (lineNumber == endShowing ? Color.White : ColourScheme.getItemColour())),
                                                 0,
                                                 lineOrigin,
                                                 1.0f,
                                                 0, 0);

                        yPosition += m_context.m_fontManager.getOverlayFont().LineSpacing;
                    }
                    lineNumber++;
                }
            }

            if (temporaryMessageEndTime > gameTime.TotalGameTime.TotalSeconds && temporaryMessage != "")
            {
                // Add any temporary message on to the end of the message
                //
                m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(),
                                         temporaryMessage,
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

        ///////////////////// MEMBER VARIABLES //////////////////////

        /// <summary>
        /// Our current directory
        /// </summary>
        protected string m_path = null;

        /// <summary>
        /// Are we scanning at drive level?
        /// </summary>
        bool m_driveLevel = false;

        /// <summary>
        /// Directory information object
        /// </summary>
        protected DirectoryInfo m_directoryInfo;

        /// <summary>
        /// File information object
        /// </summary>
        protected FileInfo m_fileInfo;

        /// <summary>
        /// Project associated with this FileSystemView
        /// </summary>
        protected Project m_project;

        /// <summary>
        /// Local ref to FontManager
        /// </summary>
        protected FontManager m_fontManager;

        /// <summary>
        /// Index of the currently highlighted directory in a directory picker
        /// </summary>
        protected int m_directoryHighlight = 0;

        /// <summary>
        /// Position in 3D land
        /// </summary>
        Vector3 m_position;
        protected BrazilContext m_brazilContext;

        protected XygloContext m_context;

        List<string> m_fileHolder = new List<string>();
    }
}
