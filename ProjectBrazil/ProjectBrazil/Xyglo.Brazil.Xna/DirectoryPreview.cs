using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;


namespace Xyglo.Brazil.Xna
{
    public class DirectoryPreview
    {
        public DirectoryPreview(XygloContext context, string initialDirectory, string filter)
        {
            m_fileFilter = filter;
            m_context = context;
            changeDirectory(initialDirectory);
        }

        /// <summary>
        /// Change the preview generation directory only if it's actually changed.
        /// </summary>
        /// <param name="newDirectory"></param>
        public void changeDirectory(string newDirectory, bool force = false)
        {
            if (newDirectory != m_currentDirectory || force)
            {
                m_currentDirectory = newDirectory;
                generateTextures();
            }
        }

        /// <summary>
        /// Generate new textures if we need them
        /// </summary>
        protected void generateTextures()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(m_currentDirectory);
            foreach (FileInfo f in dirInfo.GetFiles(m_fileFilter))
            {
                if (m_previewList.ContainsKey(f.FullName))
                    continue;

                if (isImage(f.FullName))
                {
                    FileStream imageStream = new FileStream(f.FullName, FileMode.Open);
                    Texture2D sourceImage = Texture2D.FromStream(m_context.m_graphics.GraphicsDevice, imageStream);
                    //Rectangle destinationRectangle = new Rectangle((int)startPosition.X - 100, (int)startPosition.Y, 40, 40);
                    m_previewList[f.FullName] = new Pair<Texture2D, DateTime>(sourceImage, DateTime.Now);
                    imageStream.Close();
                }
            }
        }

        /// <summary>
        /// Check a filename for an image extension
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public bool isImage(string filename)
        {
            return (filename.Length > 4 && m_imageExtensions.Contains(filename.Substring(filename.Length - 4, 4).ToUpperInvariant()));
        }
        
        /// <summary>
        /// Set the file filter and rescan directory if it's changed
        /// </summary>
        /// <param name="filter"></param>
        public void setFilter(string filter)
        {
            if (filter != m_fileFilter)
            {
                m_fileFilter = filter;
                changeDirectory(m_currentDirectory, true);
            }
            else
            {
                m_fileFilter = filter;
            }
        }

        /// <summary>
        /// Get a pregenerated texture
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public Texture2D getTexture(string filename)
        {
            return m_previewList[filename].First;
        }

        /// <summary>
        /// Current directory
        /// </summary>
        protected string m_currentDirectory = "";

        /// <summary>
        /// File filter
        /// </summary>
        protected string m_fileFilter = "";

        /// <summary>
        /// XygloContext
        /// </summary>
        protected XygloContext m_context;

        /// <summary>
        /// Maximum number of textures to use
        /// </summary>
        public int m_maxTextures = 100;

        /// <summary>
        /// Dictionary of textures with added time
        /// </summary>
        protected Dictionary<string, Pair<Texture2D, DateTime>> m_previewList = new Dictionary<string, Pair<Texture2D, DateTime>>();

        /// <summary>
        /// Image extensions
        /// </summary>
        public static readonly List<string> m_imageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };
    }
}
