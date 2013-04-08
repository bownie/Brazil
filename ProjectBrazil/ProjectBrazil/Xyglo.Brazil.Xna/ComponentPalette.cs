using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xyglo.Brazil.Xna
{
    public enum BrazilComponentType
    {
        PlainBlock,
        TextureBlock,
        Interloper,
        Coin,
        Baddy,
        Sphere
    }

    /// <summary>
    /// Show the BrazilComponents and allows us to pick them in a BrazilView
    /// </summary>
    public class ComponentPalette
    {
        public ComponentPalette(XygloContext context, BrazilContext brazilContext, EyeHandler eyeHandler)
        {
            m_context = context;
            m_brazilContext = brazilContext;
            m_eyeHandler = eyeHandler;

            // Default component to show
            m_selectedComponent = BrazilComponentType.PlainBlock;
        }

        /// <summary>
        /// Object palette - this is a display area on the screen which is used to select objects for placement in
        /// the BrazilApp
        /// </summary>
        public void draw(GameTime gameTime)
        {
            float outsideBorder = 6.0f;
            Vector2 topLeft = new Vector2(m_context.m_graphics.GraphicsDevice.Viewport.Width / 2, outsideBorder);
            Vector2 bottomRight = new Vector2(m_context.m_graphics.GraphicsDevice.Viewport.Width - outsideBorder, m_context.m_graphics.GraphicsDevice.Viewport.Height / 3);
            m_context.m_drawingHelper.drawQuad(m_context.m_overlaySpriteBatch, topLeft, bottomRight, Color.LightGreen, 0.08f);

            float insideBorder = 6.0f;
            topLeft.X += insideBorder;
            topLeft.Y += insideBorder;
            bottomRight.X -= insideBorder;
            bottomRight.Y -= insideBorder;
            m_context.m_drawingHelper.drawQuad(m_context.m_overlaySpriteBatch, topLeft, bottomRight, Color.LightGreen, 0.08f);

            XygloXnaDrawable drawable = paletteFactory(m_selectedComponent);

            drawable.buildBuffers(m_context.m_graphics.GraphicsDevice);
            drawable.draw(m_context.m_graphics.GraphicsDevice);

            drawTextRepresentation(m_selectedComponent, topLeft, bottomRight);

        }

        /// <summary>
        /// Decode the item type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
         protected string getItemString(BrazilComponentType type)
        {
            string rS = "<none>";
            switch(type)
            {
                case BrazilComponentType.TextureBlock:
                    rS = "TextureBlock";
                    break;

                case BrazilComponentType.Sphere:
                    rS = "Sphere";
                    break;

                case BrazilComponentType.PlainBlock:
                    rS = "PlainBlock";
                    break;

                case BrazilComponentType.Interloper:
                    rS = "Interloper";
                    break;

                case BrazilComponentType.Coin:
                    rS = "Coin";
                    break;

                case BrazilComponentType.Baddy:
                    rS = "Baddy";
                    break;

                default:
                    rS = "<default>";
                    break;
            }

            return rS;
        }
        /// <summary>
        /// Draw a temporary text representation of the compoennt type.
        /// </summary>
        /// <param name="type"></param>
        protected void drawTextRepresentation(BrazilComponentType type, Vector2 topLeft, Vector2 bottomRight)
        {
            //m_context.m_overlaySpriteBatch.Begin();

            string selectedItem = getItemString(type);
            float yPos = (bottomRight.Y - topLeft.Y - m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay))/ 2.0f;
            float xPos = (2 * bottomRight.X - topLeft.X - selectedItem.Length * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay)) / 2.0f;

            m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), selectedItem, new Vector2((int)xPos, (int)yPos), Color.White, 0, Vector2.Zero, 2.0f, 0, 0);

            //m_context.m_overlaySpriteBatch.End();
        }


        protected XygloXnaDrawable paletteFactory(BrazilComponentType type)
        {
            // Build it if it doesn't exist
            //
            if (!m_palette.ContainsKey(type))
            {
                Vector3 position = Vector3.Zero;
                Vector3 size = new Vector3(20, 20, 20);

                //BoundingFrustum bf = new BoundingFrustum(
                //m_eyeHandler.getTargetPosition();

                //m_context.m_drawingHelper.getScreenPlaneIntersection(

                switch (type)
                {
                    case BrazilComponentType.PlainBlock:
                        m_palette[type] = new XygloFlyingBlock(Color.Yellow, m_context.m_lineEffect, position, size);
                        break;

                    case BrazilComponentType.TextureBlock:
                        m_palette[type] = new XygloTexturedBlock(Color.Blue, m_context.m_lineEffect, position, size);
                        break;

                    default:
                        break;

                }

                // Make it spin attractively
                //
                m_palette[type].setRotation(2.0f);
            }

            return m_palette[type];
        }

        /// <summary>
        /// List of components that we are previewing in this view
        /// </summary>
        Dictionary<BrazilComponentType, XygloXnaDrawable> m_palette = new Dictionary<BrazilComponentType,XygloXnaDrawable>();

        /// <summary>
        /// Context
        /// </summary>
        protected XygloContext m_context;

        /// <summary>
        /// BrazilContext
        /// </summary>
        protected BrazilContext m_brazilContext;

        /// <summary>
        /// Currently selected component type
        /// </summary>
        protected BrazilComponentType m_selectedComponent;

        /// <summary>
        /// Eye handler reference
        /// </summary>
        protected EyeHandler m_eyeHandler;
    }
}
