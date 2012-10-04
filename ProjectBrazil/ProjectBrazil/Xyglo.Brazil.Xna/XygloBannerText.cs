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
        public XygloBannerText(SpriteBatch spriteBatch, SpriteFont spriteFont, Color colour, Vector3 position, double size, string text)
        {
            // Store the effect
            //m_effect = effect;
            m_colour = colour;

            m_size = size;
            m_text = text;
            m_position = position;

            m_spriteBatch = spriteBatch;
            m_spriteFont = spriteFont;

            if (m_alphaBlendingTest) m_colour.A = 10;
        }

        public XygloBannerText(SpriteBatch spriteBatch, SpriteFont spriteFont, Color colour, BrazilVector3 position, double size, string text)
        {
            // Store the effect
            //m_effect = effect;
            m_colour = colour;

            m_size = size;
            m_text = text;

            m_spriteBatch = spriteBatch;
            m_spriteFont = spriteFont;

            m_position.X = position.X;
            m_position.Y = position.Y;
            m_position.Z = position.Z;

            if (m_alphaBlendingTest) m_colour.A = 10;
        }

        //public XygloBannerText(SpriteBatch spriteBatch, SpriteFont spriteFont, Color colour, BrazilAlignment vertAlign, BrazilAlignment horzAlign, double size, string text)
        //{
        //}
        
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
            m_spriteBatch.Begin();
            m_spriteBatch.DrawString(m_spriteFont, m_text, new Vector2(m_position.X, m_position.Y), m_colour, 0, new Vector2(0, 0), (float)m_size, SpriteEffects.None, 0);
            m_spriteBatch.End();
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

        /// <summary>
        /// SpriteBatch
        /// </summary>
        protected SpriteBatch m_spriteBatch = null;

        /// <summary>
        /// SpriteFont
        /// </summary>
        protected SpriteFont m_spriteFont = null;
    }
}


