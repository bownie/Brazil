using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{

    /// <summary>
    /// Convenince class for graphics calls - could be confused with DrawingHelper
    /// </summary>
    public class XygloGraphics
    {
        public XygloGraphics(XygloContext context)
        {
            m_context = context; 
        }

        /// <summary>
        /// Enable windowed mode for our applications.  Some popular sizes:
        /// 
        /// Some of the modes we've used
        ///
        /// initGraphicsMode(640, 480, false);
        /// initGraphicsMode(720, 576, false);
        /// initGraphicsMode(800, 500, false);
        /// initGraphicsMode(960, 768, false);
        /// initGraphicsMode(1920, 1080, true);
        /// 
        /// </summary>
        public void windowedMode(Game game)
        {
            int maxWidth = 0;
            int maxHeight = 0;

            foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                // Set both when either is greater
                if (dm.Width > maxWidth || dm.Height > maxHeight)
                {
                    maxWidth = dm.Width;
                    maxHeight = dm.Height;
                }
            }

            // Defaults
            //
            if (m_context.m_project != null) m_context.m_fontManager.setSmallScreen(true);
            int windowWidth = 640;
            int windowHeight = 480;

            if (maxWidth >= 1920)
            {
                windowWidth = 960;
                windowHeight = 768;
                if (m_context.m_project != null) m_context.m_fontManager.setSmallScreen(false);
            }
            else if (maxWidth >= 1280)
            {
                windowWidth = 800;
                windowHeight = 500;
            }
            else if (maxWidth >= 1024)
            {
                windowWidth = 720;
                windowHeight = 576;
            }

            // Set this for storage
            //
            if (m_context.m_project != null)
            {
                m_context.m_project.setWindowSize(windowWidth, windowHeight);
                m_context.m_project.setFullScreen(false);
            }

            // Update this to ensure scanner appears in the right place
            //
            if (m_context.m_drawingHelper != null)
            {
                // Set the graphics modes
                //
                initGraphicsMode(m_context, game, windowWidth, windowHeight, false);
                m_context.m_drawingHelper.setPreviewBoundingBox(m_context.m_graphics.GraphicsDevice.Viewport.Width, m_context.m_graphics.GraphicsDevice.Viewport.Height);
            }
        }

        /// <summary>
        /// Enable full screen mode
        /// </summary>
        public void fullScreenMode(Game game)
        {
            int maxWidth = 0;
            int maxHeight = 0;

            foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                // Set both when either is greater
                if (dm.Width > maxWidth || dm.Height > maxHeight)
                {
                    maxWidth = dm.Width;
                    maxHeight = dm.Height;
                }
            }

            m_context.m_project.setFullScreen(true);

            // Update this to ensure scanner appears in the right place
            //
            if (m_context.m_drawingHelper != null)
            {
                // Set the graphics modes
                initGraphicsMode(m_context, game, maxWidth, maxHeight, true);
                m_context.m_drawingHelper.setPreviewBoundingBox(m_context.m_graphics.GraphicsDevice.Viewport.Width, m_context.m_graphics.GraphicsDevice.Viewport.Height);
            }
        }

        /// <summary>
        /// Attempt to set the display mode to the desired resolution.  Itterates through the display
        /// capabilities of the default graphics adapter to determine if the graphics adapter supports the
        /// requested resolution.  If so, the resolution is set and the function returns true.  If not,
        /// no change is made and the function returns false.
        /// </summary>
        /// <param name="iWidth">Desired screen width.</param>
        /// <param name="iHeight">Desired screen height.</param>
        /// <param name="bFullScreen">True if you wish to go to Full Screen, false for Windowed Mode.</param>
        public bool initGraphicsMode(XygloContext context, Game game, int iWidth, int iHeight, bool bFullScreen)
        {
            // If we aren't using a full screen mode, the height and width of the window can
            // be set to anything equal to or smaller than the actual screen size.
            if (bFullScreen == false)
            {
                if ((iWidth <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                    && (iHeight <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height))
                {
                    context.m_graphics.PreferredBackBufferWidth = iWidth;
                    context.m_graphics.PreferredBackBufferHeight = iHeight;
                    context.m_graphics.IsFullScreen = bFullScreen;
                    context.m_graphics.ApplyChanges();

                    // Reload the bloom component
                    //
                    if (context.m_bloom != null)
                    {
                        game.Components.Remove(context.m_bloom);
                        ////m_bloom.Dispose();
                        context.m_bloom = new BloomComponent(game);
                        game.Components.Add(context.m_bloom);
                    }

                    Logger.logMsg("DrawingHelper::initGraphicsMode() - width = " + iWidth + ", height = " + iHeight + ", fullscreen = " + bFullScreen.ToString());
                    return true;
                }
            }
            else
            {
                // If we are using full screen mode, we should check to make sure that the display
                // adapter can handle the video mode we are trying to set.  To do this, we will
                // iterate thorugh the display modes supported by the adapter and check them against
                // the mode we want to set.
                foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                {
                    // Check the width and height of each mode against the passed values
                    if ((dm.Width == iWidth) && (dm.Height == iHeight))
                    {
                        // The mode is supported, so set the buffer formats, apply changes and return
                        context.m_graphics.PreferredBackBufferWidth = iWidth;
                        context.m_graphics.PreferredBackBufferHeight = iHeight;
                        context.m_graphics.IsFullScreen = bFullScreen;
                        context.m_graphics.ApplyChanges();

                        // Reload the bloom component
                        //
                        if (context.m_bloom != null)
                        {
                            game.Components.Remove(context.m_bloom);
                            ////m_bloom.Dispose();
                            context.m_bloom = new BloomComponent(game);
                            game.Components.Add(context.m_bloom);
                        }

                        Logger.logMsg("DrawingHelper::initGraphicsMode() - width = " + iWidth + ", height = " + iHeight + ", fullscreen = " + bFullScreen.ToString());
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Our XygloContext
        /// </summary>
        protected XygloContext m_context;
    }
}
