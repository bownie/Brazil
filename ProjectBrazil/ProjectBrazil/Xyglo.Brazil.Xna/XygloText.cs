using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// A XygloText drawable component
    /// </summary>
    public class XygloText : XygloXnaDrawableShape
    {

        /// <summary>
        /// Alpha Blend text
        /// </summary>
        protected bool m_alphaBlendingTest = true;

        /// <summary>
        /// Save our local SpriteBatch
        /// </summary>
        protected SpriteBatch m_spriteBatch;

        /// <summary>
        /// The overall FontManager
        /// </summary>
        protected FontManager m_fontManager;

        /// <summary>
        /// Store the text
        /// </summary>
        protected string m_text;

        /// <summary>
        /// Size of the font we're using
        /// </summary>
        XygloView.ViewSize m_fontSize = XygloView.ViewSize.Medium;

        /// <summary>
        /// The offset at which we've grabbed this piece of text (for example)
        /// </summary>
        Vector3 m_pickupOffset = Vector3.Zero;

        /// <summary>
        /// Define a XygloText drawable
        /// </summary>
        /// <param name="fontManager"></param>
        /// <param name="spriteBatch"></param>
        /// <param name="colour"></param>
        /// <param name="effect"></param>
        /// <param name="position"></param>
        /// <param name="viewFontSize"></param>
        /// <param name="text"></param>
        public XygloText(FontManager fontManager, SpriteBatch spriteBatch, Color colour, BasicEffect effect, Vector3 position, XygloView.ViewSize viewFontSize, string text)
        {
            m_fontSize = viewFontSize;
            m_fontManager = fontManager;
            m_spriteBatch = spriteBatch;
            m_text = text;

            // Store the effect
            //
            m_effect = effect;
            m_colour = colour;
            m_position = position;

            if (m_alphaBlendingTest) m_colour.A = 10;
        }

        /// <summary>
        /// Add a menu option
        /// </summary>
        /// <param name="item"></param>
        public void setText(string text)
        {
            m_text = text;
        }

        /// <summary>
        /// Set the pickup offset to ensure alignment
        /// </summary>
        /// <param name="offset"></param>
        public void setPickupOffset(Vector3 offset)
        {
            m_pickupOffset = offset;
        }


        /// <summary>
        /// Build the shape and populate the Vertex and Index buffers.  Needs to be called after any change
        /// to position.
        /// </summary>
        /// <param name="device"></param>
        public override void buildBuffers(GraphicsDevice device)
        {
        }

        /// <summary>
        /// Draw this XygloText
        /// </summary>
        /// <param name="device"></param>
        public override void draw(GraphicsDevice device)
        {
            // Reenable the texture
            //
            m_effect.TextureEnabled = true;

            float textScale = 1.0f;

            m_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, m_effect);

            m_spriteBatch.DrawString(
                m_fontManager.getViewFont(m_fontSize),
                m_text,
                new Vector2(m_position.X - m_pickupOffset.X, m_position.Y - m_pickupOffset.Y),
                m_colour,
                0,
                Vector2.Zero,
                m_fontManager.getTextScale() * (float)textScale,
                0,
                0);

            m_spriteBatch.End();
        }

        /// <summary>
        /// Draw a preview of this text
        /// </summary>
        /// <param name="device"></param>
        /// <param name="boundingBox"></param>
        public override void drawPreview(GraphicsDevice device, BoundingBox fullBoundingBox, BoundingBox previewBoundingBox)
        {
            double factor = (double)((previewBoundingBox.Max - previewBoundingBox.Min).Length()) / (double)((fullBoundingBox.Max - fullBoundingBox.Min).Length());

            // Reenable the texture
            //
            m_effect.TextureEnabled = true;
            m_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, m_effect);

            if (m_text.Contains("\n"))
            {
                string[] splitText = m_text.Split('\n');
                for (int i = 0; i < splitText.Count(); i++)
                {
                    m_spriteBatch.DrawString(
                    m_fontManager.getViewFont(m_fontSize),
                    splitText[i],
                    new Vector2(previewBoundingBox.Min.X + (float)(m_position.X * factor) - m_pickupOffset.X,
                                previewBoundingBox.Min.Y + (float)(m_position.Y * factor) + i * m_fontManager.getViewFont(m_fontSize).LineSpacing - m_pickupOffset.Y),
                    m_colour,
                    0,
                    Vector2.Zero,
                    Math.Max(m_fontManager.getTextScale() * (float)factor, 0.2f),
                    0,
                    0);
                }
            }
            else
            {
                m_spriteBatch.DrawString(
                    m_fontManager.getViewFont(m_fontSize),
                    m_text,
                    new Vector2(previewBoundingBox.Min.X + (float)(m_position.X * factor) - m_pickupOffset.X,
                                previewBoundingBox.Min.Y + (float)(m_position.Y * factor) - m_pickupOffset.Y),
                    m_colour,
                    0,
                    Vector2.Zero,
                    Math.Max(m_fontManager.getTextScale() * (float)factor, 0.2f),
                    0,
                    0);
            }
            m_spriteBatch.End();
                
        }

        /// <summary>
        /// Override the getBoundingBox call - examine vertex data and return a bounding box
        /// based on that.
        /// </summary>
        /// <returns></returns>
        public override BoundingBox getBoundingBox()
        {
            BoundingBox bb = new BoundingBox();

            bb.Min.X = m_position.X;
            bb.Max.X = bb.Min.X + (m_text.Length * m_fontManager.getViewFont(m_fontSize).MeasureString("X").X);
            bb.Min.Y = m_position.Y;
            bb.Max.Y = bb.Min.Y + m_fontManager.getViewFont(m_fontSize).LineSpacing; // allow for line breaks !!!!!
            bb.Min.Z = m_position.Z;
            bb.Max.Z = bb.Min.Z;

            return bb;
        }
    }
}
