using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// An image resource
    /// </summary>
    public class XygloImageResource : XygloResource
    {
        public XygloImageResource(string resourceName, string filePath):base(resourceName, filePath)
        {
        }

        /// <summary>
        /// Load the resource
        /// </summary>
        /// <param name="device"></param>
        /// <param name="filePath"></param>
        public override void loadResource(GraphicsDevice device)
        {
            FileStream fs = File.OpenRead(m_fileName);
            m_texture = Texture2D.FromStream(device, fs);
            fs.Close();
        }

        public Texture2D getTexture() { return m_texture; }
        protected Texture2D m_texture;
    }

    /// <summary>
    /// Base class for Xyglo side definition of a resource
    /// </summary>
    public abstract class XygloResource
    {
        public XygloResource(string resourceName, string filePath)
        {
            m_name = resourceName;
            m_fileName = filePath;

            if (!File.Exists(filePath))
                throw new FileNotFoundException();
        }

        public abstract void loadResource(GraphicsDevice device);

        /// <summary>
        /// Name of the resource linked to the (Brazil) Resource
        /// </summary>
        protected string m_name;

        /// <summary>
        /// Filename
        /// </summary>
        protected string m_fileName;
    }
}
