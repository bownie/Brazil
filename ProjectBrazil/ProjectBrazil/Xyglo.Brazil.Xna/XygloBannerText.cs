using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    public class XygloBannerText : XygloXnaDrawable
    {
        /// <summary>
        /// Positional constructor
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="effect"></param>
        /// <param name="size"></param>
        /// <param name="position"></param>
        public XygloBannerText(Color colour, Vector3 position, double size, string text)
        {
            // Store the effect
            //m_effect = effect;
            m_colour = colour;

            m_size = size;
            m_text = text;
            m_position = position;

            if (m_alphaBlendingTest) m_colour.A = 10;
        }

        public XygloBannerText(Color colour, BrazilVector3 position, double size, string text)
        {
            // Store the effect
            //m_effect = effect;
            m_colour = colour;

            m_size = size;
            m_text = text;

            m_position.X = position.X;
            m_position.Y = position.Y;
            m_position.Z = position.Z;

            if (m_alphaBlendingTest) m_colour.A = 10;
        }
        
        /// <summary>
        /// We need to implement this override
        /// </summary>
        /// <param name="device"></param>
        public override void buildBuffers(GraphicsDevice device)
        {
        }

        /// <summary>
        /// Draw this FlyingBlock by setting and swriting the 
        /// </summary>
        /// <param name="device"></param>
        public override void draw(GraphicsDevice device)
        {
        }

        /// <summary>
        /// Size of this block
        /// </summary>
        public double m_size;

        /// <summary>
        /// The text for this Banner
        /// </summary>
        public string m_text;

        /// <summary>
        /// Our texture
        /// </summary>
        public Texture2D m_shapeTexture;

        /// <summary>
        /// Alpha Blend text
        /// </summary>
        protected bool m_alphaBlendingTest = false;
    }
}


