using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
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
            m_texture = flip(Texture2D.FromStream(device, fs), true, false);
            fs.Close();
        }


        /// <summary>
        /// Flip a texture - we use this to match our axes.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="vertical"></param>
        /// <param name="horizontal"></param>
        /// <returns></returns>
        public Texture2D flip(Texture2D source, bool vertical, bool horizontal)
        {
            Texture2D flipped = new Texture2D(source.GraphicsDevice, source.Width, source.Height);
            Color[] data = new Color[source.Width * source.Height];
            Color[] flippedData = new Color[data.Length];

            source.GetData<Color>(data);

            for (int x = 0; x < source.Width; x++)
                for (int y = 0; y < source.Height; y++)
                {
                    int idx = (horizontal ? source.Width - 1 - x : x) + ((vertical ? source.Height - 1 - y : y) * source.Width);
                    flippedData[x + y * source.Width] = data[idx];
                }

            flipped.SetData<Color>(flippedData);

            return flipped;
        }


        public Texture2D getTexture() { return m_texture; }

        /// <summary>
        /// Our texture
        /// </summary>
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
