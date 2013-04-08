using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    public enum BrazilComponentType
    {
        PlainBlock,
        TextureBlock,
        //Interloper,
        Coin
        //Baddy,
        //Sphere
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

            int width = (int)(bottomRight.X - topLeft.X - 100);
            int height = (int)(bottomRight.Y - topLeft.Y - 40);

            if (m_previewTarget == null)
            {
                m_previewTarget = new RenderTarget2D(m_context.m_graphics.GraphicsDevice, width, height);
                m_previewRectangle = new Rectangle((int)topLeft.X + 50, (int)bottomRight.Y - 20, width, height);
            }

            //renderObjectPreview(); 
            //spriteBatch.Draw(m_previewTarget, new Rectangle((int)topLeft.X + 50, (int)topLeft.Y + 20, width, height), Color.Transparent);

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

                //case BrazilComponentType.Sphere:
                    //rS = "Sphere";
                    //break;

                case BrazilComponentType.PlainBlock:
                    rS = "PlainBlock";
                    break;

                //case BrazilComponentType.Interloper:
                    //rS = "Interloper";
                    //break;

                case BrazilComponentType.Coin:
                    rS = "Coin";
                    break;

                //case BrazilComponentType.Baddy:
                    //rS = "Baddy";
                    //break;

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

        /// <summary>
        /// Create a component at a position for insertion into the model
        /// </summary>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public Component getComponentInstance(Vector3 position, float size)
        {
            Component rC = null;

            switch (m_selectedComponent)
            {
                case BrazilComponentType.PlainBlock:
                    rC = new BrazilFlyingBlock(BrazilColour.Blue, XygloConvert.getBrazilVector3(position), new BrazilVector3(size, size, size));
                    break;

                case BrazilComponentType.TextureBlock:
                    rC = new BrazilFlyingBlock(BrazilColour.Red, XygloConvert.getBrazilVector3(position), new BrazilVector3(size, size, size));
                    // and add a texture to it
                    //m_palette[type] = new XygloTexturedBlock(Color.Blue, m_context.m_physicsEffect, position, size);
                    break;

                case BrazilComponentType.Coin:
                    rC = new BrazilGoody(BrazilGoodyType.Coin, 20, XygloConvert.getBrazilVector3(position), new BrazilVector3(size, size, size), DateTime.Now);
                    BrazilGoody coin = (BrazilGoody)rC;
                    coin.setRotation(0.2f);
                    break;

                //case BrazilComponentType.:
                    //m_palette[type] = new XygloSphere(Color.White, m_context.m_lineEffect, position, maxSize);
                    //break;

                default:
                    Logger.logMsg("Couldn't find a valid BrazilComponentType");
                    break;

            }

            return rC;
        }

        /// <summary>
        /// Createa a palette drawable component
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected XygloXnaDrawable paletteFactory(BrazilComponentType type)
        {
            // Build it if it doesn't exist
            //
            if (!m_palette.ContainsKey(type))
            {
                Vector3 position = new Vector3(m_previewRectangle.Width / 2, m_previewRectangle.Height / 2, 0);
                float maxSize = 2 * Math.Min(m_previewRectangle.Width, m_previewRectangle.Height) / 3;
                Vector3 size = new Vector3(maxSize, maxSize, maxSize);

                //BoundingFrustum bf = new BoundingFrustum(
                //m_eyeHandler.getTargetPosition();

                //m_context.m_drawingHelper.getScreenPlaneIntersection(

                switch (type)
                {
                    case BrazilComponentType.PlainBlock:
                        m_palette[type] = new XygloFlyingBlock(Color.Yellow, m_context.m_lineEffect, position, size);
                        break;

                    case BrazilComponentType.TextureBlock:
                        m_palette[type] = new XygloTexturedBlock(Color.Blue, m_context.m_physicsEffect, position, size);
                        break;

                    case BrazilComponentType.Coin:
                        m_palette[type] = new XygloCoin(Color.Yellow, m_context.m_lineEffect, position, maxSize);
                        break;

                    //case BrazilComponentType.Sphere:
                        //m_palette[type] = new XygloSphere(Color.White, m_context.m_lineEffect, position, maxSize);
                        //break;

                    default:
                        Logger.logMsg("Couldn't find a valid BrazilComponentType");
                        break;

                }

                // Make it spin attractively
                //
                m_palette[type].setRotation(2.0f);
            }

            return m_palette[type];
        }


        /// <summary>
        /// Draw the object preview
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void renderObjectPreview(SpriteBatch spriteBatch)
        {
            // These can be uninitialised
            //
            if (m_previewTarget == null || m_previewRectangle == null)
                return;

            // Set the render target and clear the buffer
            //
            m_context.m_graphics.GraphicsDevice.SetRenderTarget(m_previewTarget);
            m_context.m_graphics.GraphicsDevice.Clear(Color.Yellow);

            XygloXnaDrawable drawable = paletteFactory(m_selectedComponent);
            drawable.buildBuffers(m_context.m_graphics.GraphicsDevice);
            drawable.draw(m_context.m_graphics.GraphicsDevice);

            // Now reset the render target to the back buffer
            //
            m_context.m_graphics.GraphicsDevice.SetRenderTarget(null);

            spriteBatch.Draw(m_previewTarget, m_previewRectangle, Color.Red);

            //spriteBatch.Draw(m_previewRectangle, 
            spriteBatch.Draw(m_previewTarget, new Vector2(0, 0), null, Color.White, 0, new Vector2(0, 0), 0.4f, SpriteEffects.None, 1);
            //spriteBatch.Draw(m_previewTarget, new Rectangle(0, 0, 500, 500), Color.Red);

            //m_context.m_textScrollTexture = (Texture2D)m_context.m_textScroller;
        }

        /// <summary>
        /// Change the selection in the palette
        /// </summary>
        public void incrementSelection()
        {
            if (m_selectedComponent == BrazilComponentType.Coin)
                m_selectedComponent = BrazilComponentType.PlainBlock;
            else
                m_selectedComponent++;
        }

        /// <summary>
        /// Change the selection in the palette
        /// </summary>
        public void decrementSelection()
        {
            if (m_selectedComponent == BrazilComponentType.PlainBlock)
                m_selectedComponent = BrazilComponentType.Coin;
            else
                m_selectedComponent--;
        }


        protected RenderTarget2D m_previewTarget;

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
        /// Target rectangle for the preview
        /// </summary>
        protected Rectangle m_previewRectangle;

        /// <summary>
        /// Eye handler reference
        /// </summary>
        protected EyeHandler m_eyeHandler;
    }
}
