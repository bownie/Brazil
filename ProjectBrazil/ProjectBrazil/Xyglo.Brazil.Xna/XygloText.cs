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
        /// The first row of the text we're displayed could be indented - allow for this
        /// </summary>
        protected int m_firstRowIndent = 0;

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
        public XygloText(FontManager fontManager, SpriteBatch spriteBatch, Color colour, BasicEffect effect, Vector3 position, XygloView.ViewSize viewFontSize, string text, int firstRowIndent)
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
            m_firstRowIndent = firstRowIndent;

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

            if (m_text.Contains("\n"))
            {
                string[] splitText = m_text.Split('\n');
                for (int i = 0; i < splitText.Count(); i++)
                {
                    m_spriteBatch.DrawString(
                        m_fontManager.getViewFont(m_fontSize),
                        splitText[i],
                        new Vector2(m_position.X - m_pickupOffset.X - (i == 0 ? 0 : m_fontManager.getCharWidth(m_fontSize) * m_firstRowIndent),
                        m_position.Y + (i * m_fontManager.getLineSpacing(m_fontSize)) - m_pickupOffset.Y ),
                        m_colour,
                        0,
                        Vector2.Zero,
                        m_fontManager.getTextScale() * (float)textScale,
                        0,
                        0);
                }
            }
            else
            {
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
            }

            m_spriteBatch.End();
        }


        /// <summary>
        /// Override the getBoundingBox call - examine string data and return a bounding box
        /// based on that if there are any line breaks.
        /// </summary>
        /// <returns></returns>
        public override BoundingBox getBoundingBox()
        {
            BoundingBox bb = new BoundingBox();

            if (m_text.Contains("\n"))
            {
                string[] splitText = m_text.Split('\n');
                bool first = true;
                for (int i = 0; i < splitText.Count(); i++)
                {
                    // First time through set the min and max values to the first text string
                    //
                    if (first)
                    {
                        bb.Min.X = m_position.X - m_pickupOffset.X - (i == 0 ? 0 : m_fontManager.getCharWidth(m_fontSize) * m_firstRowIndent);
                        bb.Min.Y = m_position.Y + (i * m_fontManager.getLineSpacing(m_fontSize)) - m_pickupOffset.Y;

                        bb.Max.X = bb.Min.X + m_fontManager.getCharWidth(m_fontSize) * splitText[i].Length;
                        bb.Max.Y = bb.Min.Y + m_fontManager.getLineSpacing(m_fontSize);

                        first = false;
                    }
                    else
                    {
                        float newMinX = m_position.X - m_pickupOffset.X;
                        float newMinY = m_position.Y + (i * m_fontManager.getLineSpacing(m_fontSize)) - m_pickupOffset.Y;
                        float newMaxX = bb.Min.X + m_fontManager.getCharWidth(m_fontSize) * splitText[i].Length;
                        float newMaxY = bb.Min.Y + m_fontManager.getLineSpacing(m_fontSize);

                        if (newMinX < bb.Min.X)
                            bb.Min.X = newMinX;

                        if (newMinY < bb.Min.Y)
                            bb.Min.Y = newMinY;

                        if (newMaxX > bb.Max.X)
                            bb.Max.X = newMaxX;

                        if (newMaxY > bb.Max.Y)
                            bb.Max.Y = newMaxY;
                    }
                }
            }
            else
            {
                bb.Min.X = m_position.X;
                bb.Max.X = bb.Min.X + (m_text.Length * m_fontManager.getViewFont(m_fontSize).MeasureString("X").X);
                bb.Min.Y = m_position.Y;
                bb.Max.Y = bb.Min.Y + m_fontManager.getViewFont(m_fontSize).LineSpacing; // allow for line breaks !!!!!
                bb.Min.Z = m_position.Z;
                bb.Max.Z = bb.Min.Z;
            }

            return bb;
        }


        /// <summary>
        /// Draw a preview of this text
        /// </summary>
        /// <param name="device"></param>
        /// <param name="boundingBox"></param>
        public override void drawPreview(GraphicsDevice device, BoundingBox fullBoundingBox, BoundingBox previewBoundingBox, Texture2D texture)
        {
            //double factor = (double)((previewBoundingBox.Max - previewBoundingBox.Min).Length()) / (double)((fullBoundingBox.Max - fullBoundingBox.Min).Length());
            float xFactor = (previewBoundingBox.Max.X - previewBoundingBox.Min.X) / (fullBoundingBox.Max.X - fullBoundingBox.Min.X);
            float yFactor = (previewBoundingBox.Max.Y - previewBoundingBox.Min.Y) / (fullBoundingBox.Max.Y - fullBoundingBox.Min.Y);

            // Reenable the texture
            //
            m_effect.TextureEnabled = true;
            m_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, m_effect);

            if (m_text.Contains("\n"))
            {
                string[] splitText = m_text.Split('\n');
                for (int i = 0; i < splitText.Count(); i++)
                {
                    float xPos = previewBoundingBox.Min.X + (m_position.X - m_pickupOffset.X) * xFactor - (i == 0 ? 0 : m_fontManager.getCharWidth(m_fontSize) * m_firstRowIndent * xFactor);
                    float yPos = previewBoundingBox.Min.Y + (m_position.Y * yFactor) + i * (m_fontManager.getLineSpacing(m_fontSize) * yFactor) - m_pickupOffset.Y;

                    m_spriteBatch.DrawString(
                    m_fontManager.getViewFont(m_fontSize),
                    splitText[i],
                    new Vector2(xPos,
                                yPos),
                    m_colour,
                    0,
                    Vector2.Zero,
                    Math.Max(m_fontManager.getTextScale() * xFactor, 0.2f),
                    0,
                    0);
                }
            }
            else
            {

                float xPos = previewBoundingBox.Min.X + (m_position.X - m_pickupOffset.X) * xFactor;
                float yPos = previewBoundingBox.Min.Y + (m_position.Y * yFactor);

                /*
                m_spriteBatch.DrawString(
                    m_fontManager.getViewFont(m_fontSize),
                    m_text,
                    new Vector2(xPos, //previewBoundingBox.Min.X + (float)(m_position.X * xFactor) - m_pickupOffset.X,
                                yPos), //previewBoundingBox.Min.Y + (float)(m_position.Y * yFactor) - m_pickupOffset.Y),
                    m_colour,
                    0,
                    Vector2.Zero,
                    Math.Max(m_fontManager.getTextScale() * xFactor, 0.1f),
                    0,
                    0);
                 */
                
                m_spriteBatch.Draw(texture, new Rectangle((int)(fullBoundingBox.Min.X + xPos), (int)(fullBoundingBox.Min.Y + yPos), 10, 1), m_colour);
            }
            m_spriteBatch.End();
                
        }
    }
}
