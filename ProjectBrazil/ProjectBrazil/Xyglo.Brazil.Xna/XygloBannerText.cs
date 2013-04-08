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
        public XygloBannerText(string label, SpriteBatch spriteBatch, SpriteFont spriteFont, Color colour, Vector3 position, double size, string text)
        {
            // Store the effect
            //m_effect = effect;
            m_label = label;
            m_colour = colour;

            m_size = size;
            m_text = text;
            m_position = position;

            m_spriteBatch = spriteBatch;
            m_spriteFont = spriteFont;

            if (m_alphaBlendingTest) m_colour.A = 10;
        }
        
        /// <summary>
        /// We need to implement this override
        /// </summary>
        /// <param name="device"></param>
        public override void buildBuffers(GraphicsDevice device)
        {
            Logger.logMsg("UPdating BB");
        }

        /// <summary>
        /// Override the getBoundingBox call
        /// </summary>
        /// <returns></returns>
        public override BoundingBox getBoundingBox()
        {
            return new BoundingBox();
        }

        /// <summary>
        /// Polygons in this item
        /// </summary>
        /// <returns></returns>
        public override int getPolygonCount()
        {
            return 1;
        }

        /// <summary>
        /// Draw this FlyingBlock by setting and swriting the 
        /// </summary>
        /// <param name="device"></param>
        public override void draw(GraphicsDevice device, FillMode fillMode = FillMode.Solid)
        {
            m_spriteBatch.Begin();
            m_spriteBatch.DrawString(m_spriteFont, m_text, new Vector2((int)m_position.X, (int)m_position.Y), m_colour, 0, new Vector2(0, 0), (float)m_size, SpriteEffects.None, 0);
            m_spriteBatch.End();
        }

        /// <summary>
        /// Draw a preview of this BannerText
        /// </summary>
        /// <param name="device"></param>
        /// <param name="boundingBox"></param>
        public override void drawPreview(GraphicsDevice device, BoundingBox fullBoundingBox, BoundingBox previewBoundingBox, Texture2D texture)
        {
        }

        /// <summary>
        /// Text setter
        /// </summary>
        /// <param name="text"></param>
        public void setText(string text) { m_text = text; }

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

        /// <summary>
        /// The label allows us to find a piece of text
        /// </summary>
        public string m_label;
    }
}


