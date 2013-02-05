#region File Description
//-----------------------------------------------------------------------------
// DrawingHelper.cs
//
// Copyright (C) Xyglo Ltd. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Class that we can use to stick a lot of our drawing code in.
    /// </summary>
    public class DrawingHelper
    {
        // ---------------------------------- CONSTRUCTORS -------------------------------
        //
        /// <summary>
        /// Construct our helper
        /// </summary>
        /// <param name="project"></param>
        /// <param name="flatTexture"></param>
        public DrawingHelper(XygloContext context, BrazilContext brazilContext, FrameCounter frameCounter, EyeHandler eyeHandler)
        {
            m_context = context;
            m_brazilContext = brazilContext;
            m_frameCounter = frameCounter;
            m_eyeHandler = eyeHandler;
            setPreviewBoundingBox(m_context.m_graphics.GraphicsDevice.Viewport.Width, m_context.m_graphics.GraphicsDevice.Viewport.Height);

            // Populate the user help
            //
            populateUserHelp();
        }

        // ---------------------------------- METHODS -----------------------------------
        //
        /// <summary>
        /// Return a max bounding box for a list of drawables
        /// </summary>
        /// <param name="drawables"></param>
        /// <returns></returns>
        public BoundingBox? getDrawablesBoundingBox(List<XygloXnaDrawable> drawables)
        {
            if (drawables.Count == 0)
                return null;

            // Set it up
            //
            BoundingBox bb = drawables[0].getBoundingBox();

            foreach(XygloXnaDrawable drawable in drawables)
            {
                if (drawable.getBoundingBox().Min.X < bb.Min.X)
                    bb.Min.X = drawable.getBoundingBox().Min.X;

                if (drawable.getBoundingBox().Max.X > bb.Max.X)
                    bb.Max.X = drawable.getBoundingBox().Max.X;

                if (drawable.getBoundingBox().Min.Y < bb.Min.Y)
                    bb.Min.Y = drawable.getBoundingBox().Min.Y;

                if (drawable.getBoundingBox().Max.Y > bb.Max.Y)
                    bb.Max.Y = drawable.getBoundingBox().Max.Y;
            }

            return bb;
        }

        /// <summary>
        /// Get a BoundingBox which encompasses both
        /// </summary>
        /// <param name="bb1"></param>
        /// <param name="bb2"></param>
        /// <returns></returns>
        public BoundingBox getMetaBoundingBox(BoundingBox bb1, BoundingBox bb2)
        {
            BoundingBox rBB = bb1;

            if (bb2.Min.X < rBB.Min.X)
                rBB.Min.X = bb2.Min.X;

            if (bb2.Max.X > rBB.Max.X)
                rBB.Max.X = bb2.Min.X;

            if (bb2.Min.Y < rBB.Min.Y)
                rBB.Min.Y = bb2.Min.Y;

            if (bb2.Max.Y > rBB.Max.Y)
                rBB.Max.Y = bb2.Max.Y;

            return rBB;
        }

        /// <summary>
        /// Set the current main display SpriteFont to something in keeping with the resolution and reset some important variables.
        /// </summary>
        public void setSpriteFont()
        {
            // Font loading - set our text size a bit fluffily at the moment
            //
            if (m_context.m_graphics.GraphicsDevice.Viewport.Width < 960)
                Logger.logMsg("XygloXNA:setSpriteFont() - using Small Window font");
            else if (m_context.m_graphics.GraphicsDevice.Viewport.Width < 1024)
                Logger.logMsg("XygloXNA:setSpriteFont() - using Window font");
            else
                m_context.m_fontManager.setFontState(FontManager.FontType.Large);

            // Now recalculate positions
            //
            foreach (BufferView bv in m_context.m_project.getBufferViews())
            {
                bv.calculateMyRelativePosition();
            }
        }

        /// <summary>
        /// Check for and destroy any drawables that need removing.  Also do we need to check PhysicsHandler here?
        /// </summary>
        public void checkForDestroyedDrawables()
        {
            // Check for any drawables which need removing and get rid of them
            //
            Dictionary<Component, XygloXnaDrawable> destroyDict = m_context.m_drawableComponents.Where(item => item.Value.shouldBeDestroyed() == true).ToDictionary(p => p.Key, p => p.Value);
            foreach (Component destroyKey in destroyDict.Keys)
            {
                XygloXnaDrawable drawable = m_context.m_drawableComponents[destroyKey];
                m_context.m_drawableComponents.Remove(destroyKey);
                drawable = null;

                // Now set the Component to be destroyed so it's not recreated by the next event loop
                //
                destroyKey.setDestroyed(true);
            }
        }




        /// <summary>
        /// Draw the system CPU load and memory usage next to the FileBuffer
        /// </summary>
        /// <param name="gameTime"></param>
        protected void drawSystemLoad(GameTime gameTime, SpriteBatch spriteBatch, SystemAnalyser systemAnalyser)
        {
            if (systemAnalyser == null)
                return;

            Vector2 startPosition = Vector2.Zero;
            int linesHigh = 6;

            // Bufferview
            BufferView bv = m_context.m_project.getSelectedBufferView();

            startPosition.X += m_context.m_graphics.GraphicsDevice.Viewport.Width - m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) * 3;
            startPosition.Y += (m_context.m_graphics.GraphicsDevice.Viewport.Height / 2) - m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay) * linesHigh / 2;

            float height = m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay) * linesHigh;
            float width = m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) / 2;

            // Only fetch some new sample stats when a certain timespan has elapsed so
            // we provide current time to the SystemAnalyser.
            //
            systemAnalyser.fetchStats(gameTime.TotalGameTime);

            // Draw background for CPU counter
            //
            Vector2 p1 = startPosition;
            Vector2 p2 = startPosition;

            p1.Y += height;
            p1.X += 1;
            m_context.m_drawingHelper.drawBox(spriteBatch, p1, p2, Color.DarkGray, 0.8f);

            // Draw CPU load over the top
            //
            p1 = startPosition;
            p2 = startPosition;

            p1.Y += height;
            p2.Y += height - (systemAnalyser.getSystemLoad() * height / 100.0f);
            p1.X += 1;

            m_context.m_drawingHelper.drawBox(spriteBatch, p1, p2, Color.DarkGreen, 0.8f);

            // Draw background for Memory counter
            //
            startPosition.X += m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) * 0.5f;
            p1 = startPosition;
            p2 = startPosition;

            p1.Y += height;
            p1.X += 1;

            m_context.m_drawingHelper.drawBox(spriteBatch, p1, p2, Color.DarkGray, 0.8f);

            // Draw Memory over the top
            //
            p1 = startPosition;
            p2 = startPosition;

            p1.Y += height;
            p2.Y += height - (height * systemAnalyser.getAvailableMemory() / systemAnalyser.getPhysicalMemory());
            p1.X += 1;

            m_context.m_drawingHelper.drawBox(spriteBatch, p1, p2, Color.DarkOrange, 0.8f);
            //m_pannerSpriteBatch.End();
        }

        /// <summary>
        /// Draw a scroll bar for a BufferView
        /// </summary>
        /// <param name="view"></param>
        /// <param name="file"></param>
        protected void drawScrollbar(BufferView view, XygloKeyboardHandler keyboardHandler)
        {
            if (view == null)
                return;

            Vector3 sbPos = view.getPosition();
            float height = view.getBufferShowLength() * m_context.m_fontManager.getLineSpacing(view.getViewSize());

            Rectangle sbBackGround = new Rectangle(Convert.ToInt16(sbPos.X - m_context.m_fontManager.getTextScale() * 30.0f),
                                                   Convert.ToInt16(sbPos.Y),
                                                   1,
                                                   Convert.ToInt16(height));

            // Draw scroll bar
            //
            m_context.m_spriteBatch.Draw(m_context.m_flatTexture, sbBackGround, Color.DarkCyan);

            // Draw viewing window
            //
            float start = view.getBufferShowStartY();

            // Override this for the diff view
            //
            if (keyboardHandler.getDiffer() != null && m_brazilContext.m_state.equals("DiffPicker"))
            {
                start = keyboardHandler.getDiffPosition();
            }

            float length = 0;

            // Get the line count
            //
            if (view.getFileBuffer() != null)
            {
                // Make this work for diff view as well as normal view
                //
                if (keyboardHandler.getDiffer() != null && m_brazilContext.m_state.equals("DiffPicker"))
                    length = keyboardHandler.getDiffer().getMaxDiffLength();
                else
                    length = view.getFileBuffer().getLineCount();
            }

            // Check for length of FileBuffer in case it's empty
            //
            if (length > 0)
            {
                float scrollStart = start / length * height;
                float scrollLength = height; // full height unless we have anything to scroll

                if (length > view.getBufferShowLength())
                {
                    scrollLength = view.getBufferShowLength() / length * height;

                    // Ensure that scroll bar highlight is no longer than scroll bar
                    //
                    //if (scrollStart + scrollLength > height)
                    //{
                    //scrollLength = height - scrollStart;
                    //}
                }

                // Minimum scrollLength
                //
                if (scrollLength < 2)
                {
                    scrollLength = 2;
                }

                // Ensure that the highlight doens't jump over the end of the scrollbar
                //
                if (scrollStart + scrollLength > height)
                {
                    scrollStart = height - scrollLength;
                }

                Rectangle sb = new Rectangle(Convert.ToInt16(sbPos.X - m_context.m_fontManager.getTextScale() * 30.0f),
                                             Convert.ToInt16(sbPos.Y + scrollStart),
                                             1,
                                             Convert.ToInt16(scrollLength));

                // Draw scroll bar window position
                //
                m_context.m_spriteBatch.Draw(m_context.m_flatTexture, sb, Color.LightGoldenrodYellow);
            }

            // Draw a highlight overview
            //
            if (view.gotHighlight() && length > 0)
            {
                //float hS = view.getHighlightStart().Y;

                float highlightStart = ((float)view.getHighlightStart().Y) / length * height;
                float highlightEnd = ((float)view.getHighlightEnd().Y) / length * height;

                Rectangle hl = new Rectangle(Convert.ToInt16(sbPos.X - m_context.m_fontManager.getTextScale() * 40.0f),
                                             Convert.ToInt16(sbPos.Y + highlightStart),
                                             1,
                                             Convert.ToInt16(highlightEnd - highlightStart));

                m_context.m_spriteBatch.Draw(m_context.m_flatTexture, hl, view.getHighlightColor());
            }
        }


        /// <summary>
        /// Draw an overview of all currently live drawables
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="List"></param>
        public void drawXnaDrawableOverview(GraphicsDevice device, GameTime gameTime, List<XygloXnaDrawable> drawables, SpriteBatch spriteBatch)
        {
            // Get the bounding box of bounding boxes defined by project and also any drawables
            //
            BoundingBox ?bb = getDrawablesBoundingBox(drawables);;

            // Do nothing if there's nothing to draw
            if (bb == null)
                return;

            // If we have a project do this
            if (m_context.m_project != null)
            {
                bb = getMetaBoundingBox(m_context.m_project.getBoundingBox(), (BoundingBox)bb);
            }

            BoundingBox cBB = (BoundingBox)bb;
                        
            // Modify alpha according to the type of the line
            //
            float minX = cBB.Min.X;
            float minY = cBB.Min.Y;
            float maxX = cBB.Max.X;
            float maxY = cBB.Max.Y;

            float diffX = maxX - minX;
            float diffY = maxY - minY;

            Vector2 topLeft = Vector2.Zero;
            Vector2 bottomRight = Vector2.Zero;

            float previewX = m_previewBoundingBox.Max.X - m_previewBoundingBox.Min.X;
            float previewY = m_previewBoundingBox.Max.Y - m_previewBoundingBox.Min.Y;

            // Now draw the previews - the drawable works out the scaling itself according to what
            // the size and shapes of the bounding boxen are.
            //
            foreach (XygloXnaDrawable drawable in drawables)
            {
                //drawable.drawPreview(device, (BoundingBox)bb, m_previewBoundingBox, m_context.m_flatTexture);
                topLeft.X = m_previewBoundingBox.Min.X;
                topLeft.Y = m_previewBoundingBox.Min.Y;
                bottomRight.X = m_previewBoundingBox.Min.X;
                bottomRight.Y = m_previewBoundingBox.Min.Y;

                topLeft.X += ((drawable.getBoundingBox().Min.X - minX) / diffX) * previewX;
                topLeft.Y += ((drawable.getBoundingBox().Min.Y - minY) / diffY) * previewY;

                bottomRight.X += ((drawable.getBoundingBox().Max.X - minX) / diffX) * previewX;
                bottomRight.Y += ((drawable.getBoundingBox().Max.Y - minY) / diffY) * previewY;
                drawQuad(spriteBatch, topLeft, bottomRight, drawable.getColour());
            }
        }

        /// <summary>
        /// Draws a little map of our BufferViews onto a panner/scanner area
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        protected void drawViewMap(GameTime gameTime, SpriteBatch spriteBatch)
        {
            BoundingBox bb = m_context.m_project.getBoundingBox();

            // Modify alpha according to the type of the line
            //
            float minX = bb.Min.X;
            float minY = bb.Min.Y;
            float maxX = bb.Max.X;
            float maxY = bb.Max.Y;

            float diffX = maxX - minX;
            float diffY = maxY - minY;

            Vector2 topLeft = Vector2.Zero;
            Vector2 bottomRight = Vector2.Zero;

            float previewX = m_previewBoundingBox.Max.X - m_previewBoundingBox.Min.X;
            float previewY = m_previewBoundingBox.Max.Y - m_previewBoundingBox.Min.Y;

            foreach (XygloView bv in m_context.m_project.getViews())
            {
                topLeft.X = m_previewBoundingBox.Min.X;
                topLeft.Y = m_previewBoundingBox.Min.Y;
                bottomRight.X = m_previewBoundingBox.Min.X;
                bottomRight.Y = m_previewBoundingBox.Min.Y;

                topLeft.X += ((bv.getBoundingBox().Min.X - minX) / diffX) * previewX;
                topLeft.Y += ((bv.getBoundingBox().Min.Y - minY) / diffY) * previewY;

                bottomRight.X += ((bv.getBoundingBox().Max.X - minX) / diffX) * previewX;
                bottomRight.Y += ((bv.getBoundingBox().Max.Y - minY) / diffY) * previewY;

                Color bg = bv.getBackgroundColour(gameTime, true);
                if (bg.R == 0 && bg.G == 0 && bg.B == 0)
                    bg = Color.LightBlue;

                drawQuad(spriteBatch, topLeft, bottomRight, bg, (bv == m_context.m_project.getSelectedView()) ? 0.5f : 0.2f);
                //drawQuad(spriteBatch, topLeft, bottomRight, Color.Blue, (bv == m_context.m_project.getSelectedView()) ? 0.5f : 0.2f);
            }
        }

        /// <summary>
        /// Draw a box for us - accepts floats and we don't force these to int so watch
        /// your inputs please.
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="colour"></param>
        /// <param name="width"></param>
        public void drawBox(SpriteBatch spriteBatch, Vector2 topLeft, Vector2 bottomRight, Color colour, float alpha = 1.0f, int width = 1)
        {
            Vector2 xDiff = bottomRight - topLeft;
            Vector2 yDiff = xDiff;
            xDiff.Y = 0;
            yDiff.X = 0;

            drawLine(spriteBatch, topLeft, topLeft + xDiff, colour, alpha, width);
            drawLine(spriteBatch, topLeft + xDiff, bottomRight, colour, alpha, width);
            drawLine(spriteBatch, bottomRight, topLeft + yDiff, colour, alpha, width);
            drawLine(spriteBatch, topLeft + yDiff, topLeft, colour, alpha, width);
        }


        /// <summary>
        /// Update the preview bounding box with new coordinates if the screen size changes for example
        /// </summary>
        /// <param name="windowWidth"></param>
        /// <param name="windowHeight"></param>
        public void setPreviewBoundingBox(float windowWidth, float windowHeight)
        {
            Vector3 topLeft = Vector3.Zero;
            topLeft.X = windowWidth - m_context.m_fontManager.getOverlayFont().MeasureString("X").X * 10;
            topLeft.Y = windowHeight - m_context.m_fontManager.getOverlayFont().LineSpacing * 6;

            Vector3 bottomRight = Vector3.Zero;
            bottomRight.X = windowWidth - m_context.m_fontManager.getOverlayFont().MeasureString("X").X * 3;
            bottomRight.Y = windowHeight - m_context.m_fontManager.getOverlayFont().LineSpacing * 2;

            m_previewBoundingBox.Min = topLeft;
            m_previewBoundingBox.Max = bottomRight;
        }


        /// <summary>
        /// Draw line wrapper - accepts floats and we don't force these to int so watch
        /// your inputs please.
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="colour"></param>
        /// <param name="width"></param>
        public void drawLine(SpriteBatch spriteBatch, Vector2 topLeft, Vector2 bottomRight, Color colour, float alpha = 1.0f, int width = 1)
        {
            float angle = (float)Math.Atan2(bottomRight.Y - topLeft.Y, bottomRight.X - topLeft.X);
            float length = Vector2.Distance(topLeft, bottomRight);
            spriteBatch.Draw(m_context.m_flatTexture, topLeft, null, colour * alpha,
                             angle, Vector2.Zero, new Vector2(length, width),
                             SpriteEffects.None, 0);
        }

        /// <summary>
        /// Just a wrapper for a draw quad
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="colour"></param>
        /// <param name="alpha"></param>
        /// <param name="width"></param>
        public void drawQuad(SpriteBatch spriteBatch, Vector2 topLeft, Vector2 bottomRight, Color colour, float alpha = 1.0f)
        {
            spriteBatch.Draw(m_context.m_flatTexture, new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y)), colour * alpha);
        }

        /// <summary>
        /// Render a quad to a supplied SpriteBatch
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        /// <param name="quadColour"></param>
        /// <param name="spriteBatch"></param>
        protected void renderQuad(Vector3 topLeft, Vector3 bottomRight, Color quadColour, SpriteBatch spriteBatch)
        {
            m_bottomLeft.X = topLeft.X;
            m_bottomLeft.Y = bottomRight.Y;
            m_bottomLeft.Z = topLeft.Z;

            m_topRight.X = bottomRight.X;
            m_topRight.Y = topLeft.Y;
            m_topRight.Z = bottomRight.Z;

            spriteBatch.Draw(m_context.m_flatTexture, new Rectangle(Convert.ToInt16(topLeft.X),
                                                 Convert.ToInt16(topLeft.Y),
                                                 Convert.ToInt16(bottomRight.X) - Convert.ToInt16(topLeft.X),
                                                 Convert.ToInt16(bottomRight.Y) - Convert.ToInt16(topLeft.Y)),
                                                 quadColour);
        }


        /// <summary>
        /// This draws a highlight area on the screen when we hold shift down
        /// </summary>
        protected void drawHighlight(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (m_context.m_project.getSelectedView().GetType() != typeof(BufferView))
                return;
            BufferView bv = (BufferView)m_context.m_project.getSelectedView();

            List<BoundingBox> bb = bv.computeHighlight(m_context.m_project);

            // Draw the bounding boxes
            //
            foreach (BoundingBox highlight in bb)
            {
                renderQuad(highlight.Min, highlight.Max, bv.getHighlightColor(), spriteBatch);
            }
        }

        /// <summary>
        /// Draw the help screen
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="gameTime"></param>
        protected int drawHelpScreen(SpriteBatch spriteBatch, GameTime gameTime, GraphicsDeviceManager graphics, int textScreenPositionY)
        {
            spriteBatch.Begin();
            int screenLength = drawTextScreen(spriteBatch, gameTime, graphics, m_userHelp, textScreenPositionY);
            spriteBatch.End();

            return screenLength;
        }


        /// <summary>
        /// Format a screen of information text - also return how many formatted rows of information we
        /// are displaying so we can decide about paging.
        /// </summary>
        /// <param name="text"></param>
        public int drawTextScreen(SpriteBatch spriteBatch, GameTime gameTime, GraphicsDeviceManager graphics, string text, int textScreenPositionY, int fixedWidth = 0, int highlight = -1)
        {
            Vector3 fp = m_context.m_project.getSelectedView().getPosition();

            // Always start from 0 for offsets
            //
            float yPos = 0.0f;
            float xPos = 0.0f;

            // Split out the input line
            //
            string[] infoRows = text.Split('\n');

            // We need to store this value so that page up and page down work
            //
            int textScreenLength = infoRows.Length;

            //  Position the information centrally
            //
            int longestRow = 0;
            for (int i = 0; i < textScreenLength; i++)
            {
                if (infoRows[i].Length > longestRow)
                {
                    longestRow = infoRows[i].Length;
                }
            }

            // Limit the row length when centring
            //
            if (fixedWidth == 0)
            {
                if (longestRow > m_context.m_project.getSelectedView().getBufferShowWidth())
                {
                    longestRow = m_context.m_project.getSelectedView().getBufferShowWidth();
                }
            }
            else
            {
                longestRow = fixedWidth;
            }

            // Used to be:
            // m_context.m_project.getSelectedView().getBufferShowLength()
            //
            int pageLength = DrawingHelper.getTextScreenLength();

            // Calculate endline
            //
            int endLine = textScreenPositionY + Math.Min(infoRows.Length - textScreenPositionY, pageLength);

            // Modify by height of the screen to centralise
            //
            yPos += (graphics.GraphicsDevice.Viewport.Height / 2) - (m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay) * (endLine - textScreenPositionY) / 2);

            // Adjust xPos
            //
            xPos = (graphics.GraphicsDevice.Viewport.Width / 2) - (longestRow * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) / 2);

            // hardcode the font size to 1.0f so it looks nice
            //
            for (int i = textScreenPositionY; i < endLine; i++)
            {
                // Always Always Always render a string on an integer - never on a float as it looks terrible
                //
                spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), infoRows[i], new Vector2((int)xPos, (int)yPos), (highlight == i ? Color.LightBlue : Color.White), 0, Vector2.Zero, 1.0f, 0, 0);
                yPos += m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay);
            }

            // Draw a page header if we need to
            //
            yPos = m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay) * 5;

            double dPages = Math.Ceiling((float)textScreenLength / (float)pageLength);
            double cPage = Math.Ceiling((float)(textScreenPositionY + 1) / ((float)pageLength));

            if (dPages > 1)
            {
                string pageString = "---- Page " + cPage + " of " + dPages + " ----";

                // 3 character adjustment below
                //
                xPos = (graphics.GraphicsDevice.Viewport.Width / 2) - ((pageString.Length + 3) * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) / 2);

                // Always Always Always render a string on an integer - never on a float as it looks terrible
                //
                spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), pageString, new Vector2((int)xPos, (int)yPos), Color.LightSeaGreen);
            }
            return textScreenLength;
        }

        // Define this as a static for use elsewhere
        static public int getTextScreenLength()
        {
            return 15;
        }

        /// <summary>
        /// Format a screen of information text - slightly different to a help screen as
        /// the text can be dynamic (i.e. times)
        /// </summary>
        /// <param name="text"></param>
        protected void drawInformationScreen(SpriteBatch spriteBatch, GameTime gameTime, GraphicsDeviceManager graphics, int textScreenPositionY)
        {
            // Set up the string
            //
            string text = "";

            XygloView view = m_context.m_project.getSelectedView();

            if (view == null)
            {
                m_textScreenLength = 0;
                return;
            }

            // Start spritebatch
            //
            spriteBatch.Begin();

            drawCentredTextOverlay(spriteBatch, graphics, 3, "File Information", Color.AntiqueWhite);

            if (view.GetType() == typeof(BufferView))
            {
                BufferView bv = (BufferView)view;

                string truncFileName = m_context.m_project.estimateFileStringTruncation("", bv.getFileBuffer().getFilepath(), 75);
                text += truncFileName + "\n\n";
                text += "View Type          : BufferView\n";
                text += "File status        : " + (bv.getFileBuffer().isWriteable() ? "Writeable " : "Read Only") + "\n";
                text += "File lines         : " + bv.getFileBuffer().getLineCount() + "\n";
                text += "File created       : " + bv.getFileBuffer().getCreationSystemTime().ToString() + "\n";
                text += "File last modified : " + bv.getFileBuffer().getLastWriteSystemTime().ToString() + "\n";
                text += "File last accessed : " + bv.getFileBuffer().getLastFetchSystemTime().ToString() + "\n";
            }
            else if (view.GetType() == typeof(BrazilView))
            {
                BrazilView bv = (BrazilView)view;

                text += "View Type          : BrazilView\n";
                text += "App type           : " + bv.getApp().ToString() + "\n";
                text += "Xyglo components   : " + bv.getApp().getComponents().Count + "\n";
            }
            else
            {
                text += "View Type          : XygloView (undifferentiated)";
            }

            text += "\n"; // divider
            text += "Project name:      : " + m_context.m_project.m_projectName + "\n";
            text += "Project created    : " + m_context.m_project.getCreationTime().ToString() + "\n";
            text += "Project file       : " + m_context.m_project.getExternalProjectDefinitionFile() + "\n";
            text += "Profile base dir   : " + m_context.m_project.getExternalProjectBaseDirectory() + "\n";
            text += "Number of files    : " + m_context.m_project.getFileBuffers().Count + "\n";
            text += "File lines         : " + m_context.m_project.getFilesTotalLines() + "\n";
            text += "FileBuffers        : " + m_context.m_project.getFileBuffers().Count + "\n";
            text += "BufferViews        : " + m_context.m_project.getBufferViews().Count + "\n";
            text += "\n"; // divider

            text += "BufferView size    : " + m_context.m_project.getSelectedView().getViewSizeDescription() + "\n";
            text += "\n";


            // Some timings
            //
            TimeSpan nowDiff = (DateTime.Now - m_context.m_project.getCreationTime());
            TimeSpan activeTime = m_context.m_project.m_activeTime + (DateTime.Now - m_context.m_project.m_lastAccessTime);
            text += "Project age        : " + nowDiff.Days + " days, " + nowDiff.Hours + " hours, " + nowDiff.Minutes + " minutes\n"; //, " + nowDiff.Seconds + " seconds\n";
            text += "Total editing time : " + activeTime.Days + " days, " + activeTime.Hours + " hours, " + activeTime.Minutes + " minutes, " + activeTime.Seconds + " seconds\n";

            // Draw screen of a fixed width
            //
            m_textScreenLength = drawTextScreen(spriteBatch, gameTime, graphics, text, textScreenPositionY, 75);

            spriteBatch.End();
        }

        /// <summary>
        /// Useful helper method to draw some text with specified colour and position on the pre-opened spriteBatch.
        /// </summary>
        /// <param name="screenPosition"></param>
        /// <param name="text"></param>
        /// <param name="textColour"></param>
        protected void drawTextOverlay(SpriteBatch spriteBatch, FilePosition position, string text, Color textColour)
        {
            //int xPos = (int)((float)position.X * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay));
            //int yPos = (int)((float)position.Y * m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay));

            //spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), text, new Vector2(xPos, yPos), textColour);
        }

        /// <summary>
        /// Automatically draw and centre a string - if there are line breaks in it then compensate for that
        /// </summary>
        /// <param name="spriteBatch"></param>
        /// <param name="graphics"></param>
        /// <param name="lines"></param>
        /// <param name="text"></param>
        /// <param name="textColour"></param>
        protected void drawCentredTextOverlay(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, int lines, string text, Color textColour)
        {
            int maxWidth = 0;
            foreach (string subString in text.Split('\n'))
            {
                if (subString.Length > maxWidth)
                {
                    maxWidth = subString.Length;
                }
            }
            int xPos = (graphics.GraphicsDevice.Viewport.Width / 2) - (int)((float)maxWidth / 2 * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay));
            int yPos = (int)((float)lines * m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay));
            spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), text, new Vector2(xPos, yPos), textColour);
        }


        /// <summary>
        /// Populate the user help string - could do this from a resource file really
        /// </summary>
        protected void populateUserHelp()
        {
            m_userHelp += "Key Help\n\n";

            m_userHelp += "F1  - Cycle down through buffer views\n";
            m_userHelp += "F2  - Cycle up through buffer views\n";
            m_userHelp += "F3  - Search again\n";
            m_userHelp += "F6  - Perform Build\n";
            //m_userHelp += "F7  - Zoom Out\n";
            //m_userHelp += "F8  - Zoom In\n";
            m_userHelp += "F9  - Rotate anticlockwise around group of 4\n";
            m_userHelp += "F10 - Rotate clockwise around group of 4\n";
            m_userHelp += "F11 - Toggle Full Screen Mode\n";
            //m_userHelp += "F12 - Windowed Mode\n";

            m_userHelp += "Alt + N - New buffer view on new buffer\n";
            m_userHelp += "Alt + B - Copy existing buffer view on existing buffer\n";
            m_userHelp += "Alt + O - Open file\n";
            m_userHelp += "Alt + S - Save (as) file\n";
            m_userHelp += "Alt + C - Close buffer view\n";

            m_userHelp += "Alt + H - Help screen\n";
            m_userHelp += "Alt + G - Settings screen\n";

            m_userHelp += "Alt + Z - Undo\n";
            m_userHelp += "Alt + Y - Redo\n";
            m_userHelp += "Alt + A - Select All\n";
            m_userHelp += "Alt + F - Find\n";
            m_userHelp += "Alt + [number keys] - Jump to numbered buffer view\n";

            m_userHelp += "\n\n";

            m_userHelp += "Mouse Help\n\n";
            m_userHelp += "Left click & drag   - Move Window with gravity on new window centres\n";
            m_userHelp += "Left click on File  - Move Cursor there\n";
            m_userHelp += "Left click shift    - Change highlight in BufferView\n";
            m_userHelp += "Scrollwheel in/out  - Zoom in/out\n";
            m_userHelp += "Shift & Scrollwheel - Cursor up and down in current BufferView\n";

        }

        /// <summary>
        /// Draw a BufferView in any state that we wish to - this means showing the lines of the
        /// file/buffer we want to see at the current cursor position with highlighting as required.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="gameTime"></param>
        protected void drawFileBuffer(SpriteBatch spriteBatch, BufferView view, GameTime gameTime, State state, BufferView buildStdOutView, BufferView buildStdErrView, float zoomLevel, double textScale)
        {
            Color bufferColour = view.getTextColour();

            if (state.notEquals("TextEditing") && state.notEquals("GotoLine") && state.notEquals("FindText") && state.notEquals("DiffPicker"))
            {
                bufferColour = m_greyedColour;
            }

            // Take down the colours and alpha of the non selected buffer views to draw a visual distinction
            //
            if (view != m_context.m_project.getSelectedView())
            {
                bufferColour.R = (byte)(bufferColour.R / m_greyDivisor);
                bufferColour.G = (byte)(bufferColour.G / m_greyDivisor);
                bufferColour.B = (byte)(bufferColour.B / m_greyDivisor);
                bufferColour.A = (byte)(bufferColour.A / m_greyDivisor);
            }

            float yPosition = 0.0f;

            //Vector2 lineOrigin = new Vector2();
            Vector3 viewSpaceTextPosition = view.getPosition();

            // Draw all the text lines to the height of the buffer
            //
            // This is default empty line character
            string line, fetchLine;
            int bufPos = view.getBufferShowStartY();
            Color highlightColour;

            // If we are tailing a file then let's look at the last X lines of it only
            //
            if (view.isTailing())
            {
                // We don't do this all the time so let the FileBuffer work out when we've updated
                // the file and need to change the viewing position to tail it.
                //
                view.getFileBuffer().refetchFile(gameTime, m_context.m_project.getSyntaxManager());
                //bufPos = view.getFileBuffer().getLineCount() - view.getBufferShowLength();


                // Ensure that we're always at least at zero
                //
                if (bufPos < 0)
                {
                    bufPos = 0;
                }
            }

            // We do tailing and read only files here
            //
            if (view.isTailing() && view.isReadOnly())
            {
                // Ensure that we're tailing correctly by adjusting bufferview position
                //


                // We let the view do the hard work with the wrapped lines
                //
                List<string> lines;

                if (view == buildStdOutView)
                {
                    lines = view.getWrappedEndofBuffer(m_context.m_project.getStdOutLastLine());
                }
                else if (view == buildStdErrView)
                {
                    lines = view.getWrappedEndofBuffer(m_context.m_project.getStdErrLastLine());
                }
                else
                {
                    // Default
                    //
                    lines = view.getWrappedEndofBuffer();
                }

                Color bufferColourLastRun = new Color(50, 50, 50, 50);

                for (int i = 0; i < lines.Count; i++)
                {
                    spriteBatch.DrawString(m_context.m_fontManager.getViewFont(view.getViewSize()), lines[i], new Vector2((int)viewSpaceTextPosition.X, (int)viewSpaceTextPosition.Y + yPosition),
                        (i < view.getLogRunTerminator() ? bufferColourLastRun : bufferColour), 0, Vector2.Zero, m_context.m_fontManager.getTextScale(), 0, 0);
                    yPosition += m_context.m_fontManager.getLineSpacing(view.getViewSize());
                }
            }
            else
            {
                for (int i = 0; i < view.getBufferShowLength(); i++)
                {
                    line = "~";

                    if (i + bufPos < view.getFileBuffer().getLineCount() && view.getFileBuffer().getLineCount() != 0)
                    {
                        // Fetch the line and convert any tabs to relevant spaces
                        //
                        fetchLine = view.getFileBuffer().getLine(i + bufPos).Replace("\t", m_context.m_project.getTab());

                        if (fetchLine.Length - view.getBufferShowStartX() > view.getBufferShowWidth())
                        {
                            // Get a valid section of it
                            //
                            line = fetchLine.Substring(view.getBufferShowStartX(), Math.Min(fetchLine.Length - view.getBufferShowStartX(), view.getBufferShowWidth()));

                            if (view.getBufferShowStartX() + view.getBufferShowWidth() < fetchLine.Length)
                            {
                                line += " [>]";
                            }
                        }
                        else
                        {
                            if (view.getBufferShowStartX() >= 0 && view.getBufferShowStartX() < fetchLine.Length)
                            {
                                line = fetchLine.Substring(view.getBufferShowStartX(), fetchLine.Length - view.getBufferShowStartX());
                            }
                            else
                            {
                                line = "";
                            }
                        }
                    }


                    // Get the highlighting for the line
                    //
                    m_highlights = view.getFileBuffer().getHighlighting(i + bufPos, view.getBufferShowStartX(), view.getBufferShowWidth());

                    // Only do syntax highlighting when we're not greyed out
                    //
                    // !!! Could be performance problem here with highlights
                    //
                    if (m_highlights.Count > 0 && bufferColour != m_greyedColour)
                    {
                        // Need to print the line by section with some unhighlighted
                        //
                        int nextHighlight = 0;

                        // Start from wherever we're showing from
                        //
                        int xPos = 0; // view.getBufferShowStartX();

                        // Step through with xPos all the highlights in our collection
                        //
                        while (nextHighlight < m_highlights.Count && xPos < line.Length)
                        {
                            // Sort out the colour
                            //
                            highlightColour = m_highlights[nextHighlight].getColour();

                            // If not active view then temper colour
                            //
                            if (view != m_context.m_project.getSelectedView())
                            {
                                highlightColour.R = (byte)(highlightColour.R / m_greyDivisor);
                                highlightColour.G = (byte)(highlightColour.G / m_greyDivisor);
                                highlightColour.B = (byte)(highlightColour.B / m_greyDivisor);
                                highlightColour.A = (byte)(highlightColour.A / m_greyDivisor);
                            }

                            // If the highlight starts beyond the string end then skip it -
                            // and we quit out of the highlighting process as highlights are
                            // sorted (hopefully correctly).
                            //
                            if (m_highlights[nextHighlight].m_startHighlight.X >= line.Length)
                            {
                                xPos = line.Length;
                                continue;
                            }

                            if (xPos < m_highlights[nextHighlight].m_startHighlight.X || nextHighlight >= m_highlights.Count)
                            {
                                string subLineToHighlight = line.Substring(xPos, m_highlights[nextHighlight].m_startHighlight.X - xPos);

                                // Not sure we need this for the moment
                                //
                                int screenXpos = m_context.m_project.fileToScreen(line.Substring(0, xPos)).Length;

                                //if (screenXpos != xPos)
                                //{
                                //Logger.logMsg("GOT TAB");
                                //}

                                spriteBatch.DrawString(
                                    m_context.m_fontManager.getViewFont(view.getViewSize()),
                                    subLineToHighlight,
                                    new Vector2((int)viewSpaceTextPosition.X + m_context.m_fontManager.getCharWidth(view.getViewSize()) * xPos, (int)(viewSpaceTextPosition.Y + yPosition)),
                                    bufferColour,
                                    0,
                                    Vector2.Zero,
                                    m_context.m_fontManager.getTextScale() * (float)textScale,
                                    0,
                                    0);

                                xPos = m_highlights[nextHighlight].m_startHighlight.X;
                            }

                            if (xPos == m_highlights[nextHighlight].m_startHighlight.X)
                            {
                                // Capture substring, increment xPos and draw the highlighted area - watch for
                                // highlights that span lines longer than our presented line (line).
                                //
                                string subLineInHighlight = line.Substring(m_highlights[nextHighlight].m_startHighlight.X,
                                                                           Math.Min(m_highlights[nextHighlight].m_endHighlight.X - m_highlights[nextHighlight].m_startHighlight.X, line.Length - m_highlights[nextHighlight].m_startHighlight.X));

                                spriteBatch.DrawString(
                                    m_context.m_fontManager.getViewFont(view.getViewSize()),
                                    subLineInHighlight,
                                    new Vector2((int)viewSpaceTextPosition.X + m_context.m_fontManager.getCharWidth(view.getViewSize()) * xPos, (int)(viewSpaceTextPosition.Y + yPosition)),
                                    highlightColour,
                                    0,
                                    Vector2.Zero,
                                    m_context.m_fontManager.getTextScale() * (float)textScale,
                                    0,
                                    0);

                                // Step past this highlight
                                //
                                xPos = m_highlights[nextHighlight].m_endHighlight.X;
                                nextHighlight++;
                            }
                     
                        }

                        // Draw the remainder of the line
                        //
                        if (xPos < line.Length)
                        {
                            string remainder = line.Substring(xPos, line.Length - xPos);

                            spriteBatch.DrawString(
                                m_context.m_fontManager.getViewFont(view.getViewSize()),
                                remainder,
                                new Vector2((int)viewSpaceTextPosition.X + m_context.m_fontManager.getCharWidth(view.getViewSize()) * xPos, (int)(viewSpaceTextPosition.Y + yPosition)),
                                bufferColour,
                                0,
                                Vector2.Zero,
                                m_context.m_fontManager.getTextScale() * (float)textScale,
                                0,
                                0);
                        }
                    }
                    else  // draw the line without highlighting
                    {
                        spriteBatch.DrawString(
                            m_context.m_fontManager.getViewFont(view.getViewSize()),
                            line,
                            new Vector2((int)viewSpaceTextPosition.X, (int)(viewSpaceTextPosition.Y + yPosition)),
                            bufferColour,
                            0,
                            Vector2.Zero,
                            m_context.m_fontManager.getTextScale() * (float)textScale,
                            0,
                            0);
                    }
                    
                    yPosition += m_context.m_fontManager.getLineSpacing(view.getViewSize());
                }
            }

            // Draw overlaid ID on this window if we're far enough away to use it
            //
            if (zoomLevel > 950.0f)
            {
                int viewId = m_context.m_project.getBufferViews().IndexOf(view);
                string bufferId = viewId.ToString();

                if (view.isTailing())
                {
                    bufferId += "(T)";
                }
                else if (view.isReadOnly())
                {
                    bufferId += "(RO)";
                }

                Color seeThroughColour = bufferColour * 0.4f;
                //seeThroughColour.A = 30;
                //spriteBatch.DrawString(m_context.m_fontManager.getViewFont(BufferView.ViewSize.Medium), bufferId, new Vector2((int)viewSpaceTextPosition.X, (int)viewSpaceTextPosition.Y), seeThroughColour, 0, Vector2.Zero, m_context.m_fontManager.getTextScale() * 16.0f, 0, 0);

                // Show a filename
                //
                string fileName = view.getFileBuffer().getShortFileName();
                spriteBatch.DrawString(m_context.m_fontManager.getViewFont(BufferView.ViewSize.Medium), fileName, new Vector2((int)viewSpaceTextPosition.X, (int)viewSpaceTextPosition.Y), seeThroughColour, 0, Vector2.Zero, m_context.m_fontManager.getTextScale() * 4.0f, 0, 0);
           }
        }

        /// <summary>
        /// Return the polygon count for all components that are being drawn.  We need to
        /// make sure that we don't count components that are in componentgroups twice here
        /// so we need a mechanism of indicating this in the XygloXnaDrawable.
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        public int polygonCount(List<Component> components)
        {
            int count = 0;
            foreach (Component component in components)
            {
                if (m_context.m_drawableComponents.ContainsKey(component))
                {
                    // Don't count child drawables twice, only the parents
                    //
                    if (!m_context.m_drawableComponents[component].hasParent())
                        count += m_context.m_drawableComponents[component].getPolygonCount();
                }
            }
            return count;
        }

        /// <summary>
        /// Draw the HUD Overlay for the editor with information about the current file we're viewing
        /// and position in that file.
        /// </summary>
        protected void drawOverlay(SpriteBatch spriteBatch, GameTime gameTime, GraphicsDeviceManager graphics, State state, string gotoLine, bool shiftDown, bool ctrlDown, bool altDown, Vector3 eye,
                                   string temporaryMessage, double temporaryMessageStartTime, double temporaryMessageEndTime)
        {
#if SCROLLING_TEXT
            // Flag that we set during this method
            //
            bool drawScrollingText = false;
#endif
            // Set our colour according to the state of Friendlier
            //
            Color overlayColour = Color.White;
            if (!state.equals("TextEditing") && !state.equals("GotoLine") && !state.equals("FindText") && !state.equals("DiffPicker"))
            {
                overlayColour = m_greyedColour;
            }

            // Display according to type of view
            //
            XygloView view = m_context.m_project.getSelectedView();

            // BufferView
            //
            if (view.GetType() == typeof(BufferView))
            {
                BufferView bv = (BufferView)view;

                // Set up some of these variables here
                //
                string positionString = bv.getCursorPosition().Y + "," + bv.getCursorPosition().X;
                float positionStringXPos = graphics.GraphicsDevice.Viewport.Width - positionString.Length * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) - (m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) * 14);
                float filePercent = 0.0f;

                // Filename is where we put the filename plus other assorted gubbins or we put a
                // search string in there depending on the mode.
                //
                string fileName = "";

                if (state.equals("FindText"))
                {
                    // Draw the search string down there
                    fileName = "Search: " + bv.getSearchText();
                }
                else if (state.equals("GotoLine"))
                {
                    fileName = "Goto line: " + gotoLine;
                }
                else
                {
                    if (m_context.m_project.getSelectedView() != null && bv.getFileBuffer() != null)
                    {
                        // Set the filename
                        if (bv.getFileBuffer().getShortFileName() != "")
                        {
                            fileName = "\"" + bv.getFileBuffer().getShortFileName() + "\"";
                        }
                        else
                        {
                            fileName = "<New Buffer>";
                        }

                        if (bv.getFileBuffer().isModified())
                        {
                            fileName += " [Modified]";
                        }

                        fileName += " " + bv.getFileBuffer().getLineCount() + " lines";
                    }
                    else
                    {
                        fileName = "<New Buffer>";
                    }

                    // Add some other useful states to our status line
                    //
                    if (bv.isReadOnly())
                        fileName += " [RDONLY]";

                    if (bv.isTailing())
                        fileName += " [TAIL]";

                    if (shiftDown)
                        fileName += " [SHFT]";

                    if (ctrlDown)
                        fileName += " [CTRL]";

                    if (altDown)
                        fileName += " [ALT]";
                }

                // Convert lineHeight back to normal size by dividing by m_textSize modifier
                //
                float yPos = graphics.GraphicsDevice.Viewport.Height - m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay);

                string modeString = "none";
                switch (state.m_name)
                {
                    case "TextEditing":
                        modeString = "edit";
                        break;

                    case "FileOpen":
                        modeString = "browsing";
                        break;

                    case "FileSaveAs":
                        modeString = "saving file";
                        break;

                    case "DiffPicker":
                        modeString = "performing diff";
                        break;

                    default:
                        modeString = "free";
                        break;
                }

                float modeStringXPos = graphics.GraphicsDevice.Viewport.Width - modeString.Length * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) - (m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) * 8);

                if (bv.getFileBuffer() != null && bv.getFileBuffer().getLineCount() > 0)
                {
                    filePercent = (float)(bv.getCursorPosition().Y) /
                                  (float)(Math.Max(1, bv.getFileBuffer().getLineCount() - 1));
                }

                string filePercentString = ((int)(filePercent * 100.0f)) + "%";
                float filePercentStringXPos = graphics.GraphicsDevice.Viewport.Width - filePercentString.Length * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) - (m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) * 3);

                // hardcode the font size to 1.0f so that it looks nice
                //
                spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), fileName, new Vector2(0.0f, (int)yPos), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
                spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), modeString, new Vector2((int)modeStringXPos, 0.0f), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
                spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), positionString, new Vector2((int)positionStringXPos, (int)yPos), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
                spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), filePercentString, new Vector2((int)filePercentStringXPos, (int)yPos), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
            }
            else if (view.GetType() == typeof(BrazilView))
            {
                BrazilView bv = (BrazilView)view;
                string brazilViewAppType = "Showing App Type: " + bv.getApp().ToString();
                int yPos = (int)(graphics.GraphicsDevice.Viewport.Height - m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay));
                spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), brazilViewAppType, new Vector2(0, yPos), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);

                // Get objects and polygons
                //
                string brazilViewInfo = "Components: " + bv.getApp().getComponents().Count() + ", Polygons: " + polygonCount(bv.getApp().getComponents());
                int xPos = (int)(graphics.GraphicsDevice.Viewport.Width - brazilViewInfo.Length * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) - (m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay)));
                spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), brazilViewInfo, new Vector2(xPos, yPos), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
            }


            // Debug eye position
            //
            if (m_context.m_project.getViewMode() != Project.ViewMode.Formal)
            {
                string eyePosition = "[EyePosition] X " + eye.X + ",Y " + eye.Y + ",Z " + eye.Z;
                float xPos = graphics.GraphicsDevice.Viewport.Width - eyePosition.Length * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay);
                spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), eyePosition, new Vector2(0.0f, 0.0f), overlayColour, 0, Vector2.Zero, 1.0f, 0, 0);
            }

            // Draw any temporary message
            //
            drawTemporaryMessage(spriteBatch, graphics, gameTime, Color.HotPink, temporaryMessage, temporaryMessageStartTime, temporaryMessageEndTime);

#if SCROLLING_TEXT
            // Draw the scrolling text
            //
            if (m_textScrollTexture != null && drawScrollingText)
            {
                m_spriteBatch.Begin();
                m_spriteBatch.Draw(m_textScrollTexture, new Rectangle((int)((fileName.Length + 1) * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay)), (int)yPos, m_textScrollTexture.Width, m_textScrollTexture.Height), Color.White);
                m_spriteBatch.End();
            }
#endif

        }

        /*
        /// <summary>
        /// Render some scrolling text to a texture.  This takes the current m_temporaryMessage and renders
        /// to a texture according to how much time has passed since the message was created.
        /// </summary>
        protected void renderTextScroller()
        {
            if (m_brazilContext.m_state.notEquals("TextEditing"))
            {
                return;
            }
            if (m_temporaryMessage == "")
            {
                return;
            }

            // Speed - higher is faster
            //
            float speed = 120.0f;

            // Set the render target and clear the buffer
            //
            m_context.m_graphics.GraphicsDevice.SetRenderTarget(m_context.m_textScroller);
            m_context.m_graphics.GraphicsDevice.Clear(Color.Black);

            // Start with whole message showing and scroll it left
            //
            int newPosition = (int)((m_context.m_gameTime.TotalGameTime.TotalSeconds - m_temporaryMessageStartTime) * -speed);

            if ((newPosition + (int)(m_temporaryMessage.Length * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay))) < 0)
            {
                // Set the temporary message to start again and adjust position/time 
                // by width of the textScroller.
                //
                m_temporaryMessageStartTime = m_context.m_gameTime.TotalGameTime.TotalSeconds + m_context.m_textScroller.Width / speed;
            }

            // xPosition holds the scrolling position of the text in the temporary message window
            int xPosition = 0;
            float delayScroll = 0.7f; // delay the scrolling by this amount so we can read it before it starts moving

            if (m_context.m_gameTime.TotalGameTime.TotalSeconds - m_temporaryMessageStartTime > delayScroll)
            {
                xPosition = (int)((m_context.m_gameTime.TotalGameTime.TotalSeconds - delayScroll - m_temporaryMessageStartTime) * -120.0f);
            }

            // Draw to the render target
            //
            m_context.m_spriteBatch.Begin();
            m_context.m_spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), m_temporaryMessage, new Vector2((int)xPosition, 0), Color.Pink, 0, new Vector2(0, 0), 1.0f, 0, 0);
            m_context.m_spriteBatch.End();

            // Now reset the render target to the back buffer
            //
            m_context.m_graphics.GraphicsDevice.SetRenderTarget(null);
            m_context.m_textScrollTexture = (Texture2D)m_context.m_textScroller;
        }*/

        /// <summary>
        /// Draw temporary message by fade in/fade out
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="overlayColour"></param>
        protected void drawTemporaryMessage(SpriteBatch spriteBatch, GraphicsDeviceManager graphics, GameTime gameTime, Color overlayColour, string temporaryMessage, double temporaryMessageStartTime, double temporaryMessageEndTime)
        {
            if (temporaryMessage == "" || gameTime.TotalGameTime.TotalSeconds > temporaryMessageEndTime)
            {
                return;
            }

            // Could be null
            XygloView bv = m_context.m_project.getSelectedView();

            // Now calculate the colour according to the time - fade in/fade out is currently linear
            //
            Color fadeColour = overlayColour;

            float blankTime = 0.1f; // seconds
            float fadeTime = 0.4f; // seconds
            if (gameTime.TotalGameTime.TotalSeconds - temporaryMessageStartTime < blankTime)
            {
                fadeColour = Color.Black;
                fadeColour.A = 0;
            }
            else if (gameTime.TotalGameTime.TotalSeconds - temporaryMessageStartTime < fadeTime)
            {
                double percent = (gameTime.TotalGameTime.TotalSeconds - temporaryMessageStartTime - blankTime) / (fadeTime - blankTime);
                fadeColour.R = (byte)((double)overlayColour.R * percent);
                fadeColour.G = (byte)((double)overlayColour.G * percent);
                fadeColour.B = (byte)((double)overlayColour.B * percent);
                fadeColour.A = (byte)((double)overlayColour.A * percent);
            }
            else if (gameTime.TotalGameTime.TotalSeconds > temporaryMessageEndTime - fadeTime)
            {
                double percent = (temporaryMessageEndTime - gameTime.TotalGameTime.TotalSeconds) / fadeTime;
                fadeColour.R = (byte)((double)overlayColour.R * percent);
                fadeColour.G = (byte)((double)overlayColour.G * percent);
                fadeColour.B = (byte)((double)overlayColour.B * percent);
                fadeColour.A = (byte)((double)overlayColour.A * percent);
            }

            // How many lines are we going to show for this temporary message?
            //
            List<string> splitString = splitStringNicely(temporaryMessage, bv.getBufferShowWidth());

            // Set x and Y accordingly
            //
            float yPos = graphics.GraphicsDevice.Viewport.Height - ((splitString.Count + 3) * m_context.m_fontManager.getOverlayFont().LineSpacing);

            //m_overlaySpriteBatch.Begin();
            for (int i = 0; i < splitString.Count; i++)
            {
                float xPos = graphics.GraphicsDevice.Viewport.Width / 2 - m_context.m_fontManager.getOverlayFont().MeasureString("X").X * splitString[i].Length / 2;
                spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), splitString[i], new Vector2(xPos, yPos), fadeColour, 0, Vector2.Zero, 1.0f, 0, 0);
                yPos += m_context.m_fontManager.getOverlayFont().LineSpacing;
            }
            //m_overlaySpriteBatch.End();
        }



        /// <summary>
        /// Split at string along a given length along word boundaries.   We try to split on space first,
        /// then forwardslash, then backslash.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        protected List<string> splitStringNicely(string line, int width)
        {
            List<string> rS = new List<string>();

            if (line.Length < width)
            {
                rS.Add(line);
                return rS;
            }

            int splitPos = 0;
            while (splitPos < line.Length)
            {
                // Split to max width
                //
                string splitString = line.Substring(splitPos, Math.Min(width, line.Length - splitPos));

                int findPos = splitString.LastIndexOf(" ");

                if (findPos == -1)
                {
                    findPos = splitString.LastIndexOf("/");

                    if (findPos == -1)
                    {
                        findPos = splitString.LastIndexOf("\\");

                        if (findPos == -1)
                        {
                            findPos = splitString.LastIndexOf("/");
                        }
                    }
                }

                if (findPos != -1)
                {
                    // Step past this match character for next match
                    //
                    if (findPos + splitPos < line.Length)
                    {
                        findPos++;
                    }
                    splitString = line.Substring(splitPos, findPos);
                }

                /*
                // If there's no space in our substring then we cheat
                //
                if (splitString == "")
                {
                    if ((line.Length - splitPos) < width)
                    {
                        rS.Add(line.Substring(splitPos, line.Length - splitPos));
                        splitPos = line.Length; // and exit
                    }
                    else // greater than width
                    {
                        rS.Add(line.Substring(splitPos, width));
                        splitPos += width; // and continue splitting
                    }
                }
                else
                {*/
                splitPos += splitString.Length;

                //                    if (splitPos < line.Length)
                //{
                //  splitPos++;
                //}

                rS.Add(splitString);
                //}
            }

            return rS;
        }

        /// <summary>
        /// Get the banner start time
        /// </summary>
        /// <returns></returns>
        public double getBannerStartTime()
        {
            return m_bannerStartTime;
        }

        /// <summary>
        /// Start drawing a zooming banner
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="bannerString"></param>
        /// <param name="seconds"></param>
        public void startBanner(GameTime gameTime, string bannerString, float seconds)
        {
            m_bannerStartTime = gameTime.TotalGameTime.TotalSeconds;
            m_bannerString = bannerString;
            m_bannerDuration = seconds;
            m_bannerColour.R = 255;
            m_bannerColour.B = 255;
            m_bannerColour.G = 255;
            m_bannerColour.A = 180;

            m_bannerStringList = new List<string>();

            foreach (string str in m_bannerString.Split('\n'))
            {
                m_bannerStringList.Add(str);
            }
        }

        /// <summary>
        /// Draw a zooming banner
        /// </summary>
        /// <param name="gameTime"></param>
        protected void drawBanner(SpriteBatch spriteBatch, GameTime gameTime, Effect basicEffect, Texture2D splashScreen)
        {
            // Don't do anything if we don't have anything to draw
            //
            if (m_bannerStringList == null || m_bannerStringList.Count == 0)
            {
                return;
            }

            float scale = (float)(Math.Pow(gameTime.TotalGameTime.TotalSeconds - m_bannerStartTime + 0.4, 6));

            // Stop display this at some point by resetting the m_bannerStartTime - we can also stop displaying if the scale is too big
            //
            if ((m_bannerStartTime + m_bannerDuration) < gameTime.TotalGameTime.TotalSeconds)
            {
                m_bannerStartTime = -1;
                return;
            }

            if (scale > 100.0f)
            {
                m_bannerColour.A--; ;
                m_bannerColour.R--;
                m_bannerColour.B--;
                m_bannerColour.G--;
            }

            //if (m_bannerAlpha < 0)
            //{
            //m_bannerStartTime = -1;
            //return;
            //}

            Vector3 position = m_context.m_project.getSelectedBufferView().getPosition();

            // Start with centering adjustments
            //
            int maxLength = 0;
            foreach (string banner in m_bannerStringList)
            {
                if (banner.Length > maxLength)
                {
                    maxLength = banner.Length;
                }
            }
            int xPosition = 0;
            int yPosition = 0;

            bool isText = false;

            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, basicEffect);

            if (isText)
            {
                xPosition = -(int)((scale * maxLength * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay) / 2));
                yPosition = -(int)((m_bannerStringList.Count * scale * m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay) / 2));

                // Add half the editing window width and height
                //
                xPosition += (int)(m_context.m_project.getSelectedBufferView().getVisibleWidth() / 2);
                yPosition += (int)(m_context.m_project.getSelectedBufferView().getVisibleHeight() / 2);

                // Add window position - so we're doing this a bit backwards but you get the idea
                //
                xPosition += (int)(position.X);
                yPosition += (int)(position.Y);

                if (m_bannerColour.R == m_bannerColour.G && m_bannerColour.G == m_bannerColour.B && m_bannerColour.B == 0)
                {
                    m_bannerStartTime = -1;
                    return;
                }


                //float scale = diff.Ticks;
                // Draw to the render target
                //
                Vector3 curPos = m_context.m_project.getSelectedBufferView().getPosition();

                spriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), m_bannerString, new Vector2((int)xPosition, (int)yPosition), m_bannerColour, 0, new Vector2(0, 0), scale, 0, 0);
            }
            else
            {
                xPosition = (int)m_context.m_project.getEyePosition().X;
                yPosition = (int)m_context.m_project.getEyePosition().Y;

                xPosition += (int)(scale * (float)splashScreen.Width / 2.0f);
                yPosition += (int)(scale * (float)splashScreen.Height / 2.0f);

                spriteBatch.Draw(splashScreen, new Vector2((int)xPosition, (int)yPosition), null, Color.White, 0f, Vector2.Zero, scale, 0, 0);
            }

            spriteBatch.End();
        }

        /// <summary>
        /// Draw an overview of the project from a file perspective and allow modification
        /// </summary>
        public void drawManageProject(SpriteBatch spriteBatch, GameTime gameTime, ModelBuilder modelBuilder, GraphicsDeviceManager graphics, int configPosition)
        {
            string text = "";

            // Star the spritebatch
            //
            spriteBatch.Begin();

            // The maximum width of an entry in the file list
            //
            int maxWidth = ((int)((float)m_context.m_project.getSelectedView().getBufferShowWidth() * 0.9f));

            // This is very simply modelled at the moment
            //
            foreach (string fileName in modelBuilder.getReturnString().Split('\n'))
            {
                // Ignore the last split
                //
                if (fileName != "")
                {
                    if ((modelBuilder.getRootString().Length + fileName.Length) < maxWidth)
                    {
                        text += modelBuilder.getRootString() + fileName + "\n";
                    }
                    else
                    {
                        //text += m_context.m_project.buildFileString(m_modelBuilder.getRootString(), fileName, maxWidth) + "\n";
                        text += m_context.m_project.estimateFileStringTruncation(modelBuilder.getRootString(), fileName, maxWidth) + "\n";
                    }

                }
            }

            if (text == "")
            {
                text = "[Project contains no Files]";
            }

            // Draw the main text screen - using the m_configPosition as the place holder
            //
            m_textScreenLength = drawTextScreen(spriteBatch, gameTime, graphics, text, 0, 0, configPosition);

            // Draw the project file name
            //
            drawCentredTextOverlay(spriteBatch, graphics, 3, "Project Overview", Color.AntiqueWhite);
            drawCentredTextOverlay(spriteBatch, graphics, 5, "Project file : " + m_context.m_project.getProjectFile(), Color.LightSeaGreen);

            // Help text
            //
            string commandText = "[Delete] - remove file from project       [Insert]  - edit file\n";
            commandText += "[Home]   - change project file location   [N]       - create new project file";
            drawCentredTextOverlay(spriteBatch, graphics, 30, commandText, Color.LightCoral);

            // End the batch
            //
            spriteBatch.End();
        }

        /// <summary>
        /// Draw the differ - it's two mini document overviews and we provide an overlay so that
        /// we know what position in the diff we're currently looking at.
        /// </summary>
        /// <param name="v"></param>
        protected void drawDiffer(GameTime gameTime, SpriteBatch spriteBatch, BrazilContext brazilContext, XygloKeyboardHandler keyboardHandler)
        {
            // Fetch local ref
            //
            Differ differ = keyboardHandler.getDiffer();

            // Don't draw the cursor if we're not the active window or if we're confirming 
            // something on the screen.
            //
            if (differ == null || brazilContext.m_state.notEquals("DiffPicker") || differ.hasDiffs() == false)
            {
                return;
            }

            Color myColour = Color.White;

            m_context.m_drawingHelper.drawBox(spriteBatch, differ.getLeftBox(), differ.getLeftBoxEnd(), myColour, 0.5f);
            m_context.m_drawingHelper.drawBox(spriteBatch, differ.getRightBox(), differ.getRightBoxEnd(), myColour, 0.5f);

            // Modify alpha according to the type of the line
            //
            float alpha = 1.0f;

            // Draw LHS preview
            //
            foreach (DiffPreview dp in differ.getLhsDiffPreview())
            {
                if (dp.m_colour == differ.m_unchangedColour)
                {
                    alpha = 0.5f;
                }
                else
                {
                    alpha = 0.8f;
                }

                m_context.m_drawingHelper.drawLine(spriteBatch, dp.m_startPos, dp.m_endPos, dp.m_colour, alpha);
            }

            // Draw RHS preview
            //
            foreach (DiffPreview dp in differ.getRhsDiffPreview())
            {
                if (dp.m_colour == differ.m_unchangedColour)
                {
                    alpha = 0.5f;
                }
                else
                {
                    alpha = 0.8f;
                }

                m_context.m_drawingHelper.drawLine(spriteBatch, dp.m_startPos, dp.m_endPos, dp.m_colour, alpha);
            }

            // Now we want to render a position viewer box overlay
            //
            float startY = Math.Min(differ.getLeftBox().Y + differ.getYMargin(), differ.getRightBox().Y + differ.getYMargin());
            float endY = Math.Min(differ.getLeftBoxEnd().Y - differ.getYMargin(), differ.getRightBoxEnd().Y - differ.getYMargin());

            double diffPercent = ((double)keyboardHandler.getDiffPosition()) / ((double)differ.getMaxDiffLength());
            double height = ((double)m_context.m_project.getSelectedView().getBufferShowLength()) / ((double)differ.getMaxDiffLength());

            Vector2 topLeft = new Vector2(differ.getLeftBox().X - 10.0f, startY + ((endY - startY) * ((float)diffPercent)));
            Vector2 topRight = new Vector2(differ.getRightBoxEnd().X + 10.0f, startY + ((endY - startY) * ((float)diffPercent)));
            Vector2 bottomRight = topRight;
            bottomRight.Y += Math.Max(((float)height * (endY - startY)), 3.0f);

            // Now render the quad
            //
            m_context.m_drawingHelper.drawQuad(spriteBatch, topLeft, bottomRight, Color.LightYellow, 0.3f);
        }

        /// <summary>
        /// Draw information screens and other fluff like choosers and previews
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="keyboardHandler"></param>
        /// <param name="keyboard"></param>
        /// <param name="tempMessage"></param>
        /// <param name="eyeHandler"></param>
        /// <param name="systemAnalyser"></param>
        public void drawScreenFluff(GameTime gameTime, XygloKeyboardHandler keyboardHandler, XygloKeyboard keyboard, TemporaryMessage tempMessage, EyeHandler eyeHandler, SystemAnalyser systemAnalyser)
        {
            // If we're choosing a file then
            //
            if (m_brazilContext.m_state.equals("FileSaveAs") || m_brazilContext.m_state.equals("FileOpen") || m_brazilContext.m_state.equals("PositionScreenOpen") || m_brazilContext.m_state.equals("PositionScreenNew") || m_brazilContext.m_state.equals("PositionScreenCopy"))
            {
                m_context.m_fileSystemView.drawDirectoryChooser(gameTime, keyboardHandler, tempMessage.getTemporaryMessage(), tempMessage.getTemporaryMessageEndTime());
            }
            else if (m_brazilContext.m_state.equals("Help"))
            {
                // Get the text screen length back from the drawing method
                //
                m_textScreenLength = drawHelpScreen(m_context.m_overlaySpriteBatch, gameTime, m_context.m_graphics, keyboardHandler.getTextScreenPositionY());
            }
            else if (m_brazilContext.m_state.equals("Information"))
            {
                drawInformationScreen(m_context.m_overlaySpriteBatch, gameTime, m_context.m_graphics, keyboardHandler.getTextScreenPositionY());
            }
            else if (m_brazilContext.m_state.equals("Configuration"))
            {
                drawConfigurationScreen(gameTime, keyboardHandler);
            }
            else
            {
                // http://forums.create.msdn.com/forums/p/61995/381650.aspx
                //
                m_context.m_overlaySpriteBatch.Begin();

                // Draw the Overlay HUD
                //
                drawOverlay(m_context.m_overlaySpriteBatch, gameTime, m_context.m_graphics, m_brazilContext.m_state, keyboardHandler.getGotoLine(), keyboard.isShiftDown(), keyboard.isCtrlDown(), keyboard.isAltDown(),
                                            eyeHandler.getEyePosition(), tempMessage.getTemporaryMessage(), tempMessage.getTemporaryMessageStartTime(), tempMessage.getTemporaryMessageEndTime());

                // Draw map preview of all Views.
                //
                drawViewMap(gameTime, m_context.m_overlaySpriteBatch);
                m_context.m_overlaySpriteBatch.End();

                // Draw any differ overlay
                //
                m_context.m_pannerSpriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone /*, m_pannerEffect */ );

                // Draw the differ
                //
                drawDiffer(gameTime, m_context.m_pannerSpriteBatch, m_brazilContext, keyboardHandler);

                // Draw system load
                //
                drawSystemLoad(gameTime, m_context.m_pannerSpriteBatch, systemAnalyser);

                m_context.m_pannerSpriteBatch.End();
            }

            // Draw a welcome banner
            //
            if (getBannerStartTime() != -1 && m_context.m_project.getViewMode() != Project.ViewMode.Formal)
                drawBanner(m_context.m_spriteBatch, gameTime, m_context.m_basicEffect, m_context.m_splashScreen);
        }

        /// <summary>
        /// Draw the file buffers, highlight and cursor.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="isActive"></param>
        /// <param name="keyboardHandler"></param>
        /// <param name="mouse"></param>
        public void drawFileBuffers(GameTime gameTime, bool isActive, XygloKeyboardHandler keyboardHandler, XygloMouse mouse, BufferView buildStdOutView, BufferView buildStdErrView)
        {
            // Here we need to vary the parameters to the SpriteBatch - to the BasicEffect and also the font size.
            // For large fonts we need to be able to downscale them effectively so that they will still look good
            // at higher resolutions.
            //
            m_context.m_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, m_context.m_basicEffect);

            // Draw all the BufferViews for all remaining modes
            //
            for (int i = 0; i < m_context.m_project.getBufferViews().Count; i++)
            {
                if (keyboardHandler.getDiffer() != null && keyboardHandler.getDiffer().hasDiffs() &&
                    (keyboardHandler.getDiffer().getSourceBufferViewLhs() == m_context.m_project.getBufferViews()[i] ||
                        keyboardHandler.getDiffer().getSourceBufferViewRhs() == m_context.m_project.getBufferViews()[i]))
                {
                    drawDiffBuffer(m_context.m_project.getBufferViews()[i], gameTime, keyboardHandler);
                }
                else
                {
                    // We have to invert the BoundingBox along the Y axis to ensure that
                    // it matches with the frustrum we're culling against.
                    //
                    BoundingBox bb = m_context.m_project.getBufferViews()[i].getBoundingBox();
                    bb.Min.Y = -bb.Min.Y;
                    bb.Max.Y = -bb.Max.Y;

                    // We only do frustrum culling for BufferViews for the moment
                    // - intersects might be too grabby but Disjoint didn't appear 
                    // to be grabby enough.
                    //
                    //if (m_context.m_frustrum.Intersects(bb))
                    if (m_context.m_frustrum.Contains(bb) != ContainmentType.Disjoint)
                    {
                        drawFileBuffer(m_context.m_spriteBatch, m_context.m_project.getBufferViews()[i], gameTime, m_brazilContext.m_state, buildStdOutView, buildStdErrView, m_context.m_zoomLevel, keyboardHandler.getCurrentFontScale());
                    }

                    // Draw a background square for all buffer views if they are coloured
                    //
                    //if (m_context.m_project.getViewMode() == Project.ViewMode.Coloured)
                    //{
                    renderQuad(m_context.m_project.getBufferViews()[i].getTopLeft(), m_context.m_project.getBufferViews()[i].getBottomRight(), m_context.m_project.getBufferViews()[i].getBackgroundColour(gameTime), m_context.m_spriteBatch);
                    //}
                }
            }

            // We only draw the scrollbar on the active view in the right mode
            //
            if (m_brazilContext.m_state.equals("TextEditing"))
                drawScrollbar(m_context.m_project.getSelectedBufferView(), keyboardHandler);

            // Cursor and cursor highlight
            //
            if (m_brazilContext.m_state.equals("TextEditing"))
            {
                // Stop and use a different spritebatch for the highlighting and cursor
                //
                m_context.m_spriteBatch.End();
                m_context.m_spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, m_context.m_basicEffect);

                if (isActive && m_brazilContext.m_confirmState.equals("None") && m_brazilContext.m_state.notEquals("FindText") && m_brazilContext.m_state.notEquals("GotoLine"))
                    drawCursor(gameTime, m_context.m_spriteBatch, mouse);

                drawHighlight(gameTime, m_context.m_spriteBatch);
            }

            m_context.m_spriteBatch.End();
            
        }

        /// <summary>
        /// Draw a cursor and make it blink in position on a FileBuffer
        /// </summary>
        /// <param name="v"></param>
        protected void drawCursor(GameTime gameTime, SpriteBatch spriteBatch, XygloMouse mouse)
        {
            BufferView bv = m_context.m_project.getSelectedBufferView();

            if (bv == null)
                return;

            // Don't draw the cursor if we're not the active window or if we're confirming 
            // something on the screen.
            //
            // No cursor for tailing BufferViews
            //
            if (!bv.isTailing())
            {
                double dTS = gameTime.TotalGameTime.TotalSeconds;
                int blinkRate = 4;

                // Test for when we're showing this
                //
                if (Convert.ToInt32(dTS * blinkRate) % 2 != 0)
                {
                    return;
                }

                // Blinks rate
                //
                Vector3 v1 = bv.getCursorCoordinates();
                v1.Y += bv.getLineSpacing();

                Vector3 v2 = bv.getCursorCoordinates();
                v2.X += 1;

                m_context.m_drawingHelper.renderQuad(v1, v2, bv.getHighlightColor(), spriteBatch);
            }
            // Draw any temporary highlight
            //
            if (mouse.getClickHighlight().First != null &&
                ((BufferView)mouse.getClickHighlight().First) == m_context.m_project.getSelectedView())
            {
                Highlight h = (Highlight)mouse.getClickHighlight().Second;
                Vector3 h1 = bv.getSpaceCoordinates(h.m_startHighlight.asScreenPosition());
                Vector3 h2 = bv.getSpaceCoordinates(h.m_endHighlight.asScreenPosition());

                // Add some height here so we can see the highlight
                //
                h2.Y += m_context.m_fontManager.getLineSpacing(bv.getViewSize());

                m_context.m_drawingHelper.renderQuad(h1, h2, h.getColour(), spriteBatch);
            }
        }

        /// <summary>
        /// How to draw a diff'd BufferView on the screen - we key on m_diffPosition rather
        /// than using the cursor.  Always start from the translated lhs window position.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="gameTime"></param>
        protected void drawDiffBuffer(BufferView view, GameTime gameTime, XygloKeyboardHandler keyboardHandler)
        {
            Differ differ = keyboardHandler.getDiffer();

            // Only process for diff views
            //
            if (view != differ.getSourceBufferViewLhs() && view != differ.getSourceBufferViewRhs())
            {
                return;
            }

            int sourceLine = keyboardHandler.getDiffPosition();
            string line = "";
            Color colour = Color.White;
            Vector3 viewSpaceTextPosition = view.getPosition();
            float yPosition = 0;
            List<Pair<DiffResult, int>> diffList;

            // Get the diffList generated in the Differ object - this holds all the expanded
            // diff information which we'll need to adjust for (on the right hand side) if we're
            // to generate a meaningful side by side diff whilst we scroll through it.
            //
            if (view == differ.getSourceBufferViewLhs())
                diffList = differ.getLhsDiff();
            else
                diffList = differ.getRhsDiff();

            // Need to adjust the sourceLine by the number of padding lines in the diffList up to this
            // point - otherwise we lost alignment as we scroll through the document.
            //
            for (int j = 0; j < keyboardHandler.getDiffPosition(); j++)
            {
                if (j < diffList.Count)
                {
                    if (diffList[j].First == DiffResult.Padding)
                    {
                        sourceLine--;
                    }
                }
            }

            // Now iterate down the view and pull in the lines as required
            //
            for (int i = 0; i < view.getBufferShowLength(); i++)
            {
                if ((i + keyboardHandler.getDiffPosition()) < diffList.Count)
                {
                    switch (diffList[i + keyboardHandler.getDiffPosition()].First)
                    {
                        case DiffResult.Unchanged:
                            colour = differ.m_unchangedColour;

                            if (sourceLine < view.getFileBuffer().getLineCount())
                                line = view.getFileBuffer().getLine(sourceLine++);
                            // print line
                            break;

                        case DiffResult.Deleted:
                            // print deleted line (colour change?)
                            colour = differ.m_deletedColour;

                            if (sourceLine < view.getFileBuffer().getLineCount())
                                line = view.getFileBuffer().getLine(sourceLine++);
                            break;

                        case DiffResult.Inserted:
                            // print inserted line (colour)
                            colour = differ.m_insertedColour;

                            if (sourceLine < view.getFileBuffer().getLineCount())
                                line = view.getFileBuffer().getLine(sourceLine++);
                            break;

                        case DiffResult.Padding:
                        default:
                            colour = differ.m_paddingColour;
                            line = "";
                            // add a padding line
                            break;
                    }

                }
                else
                {
                    // Do something to handle blank lines beyond end of list
                    //
                    colour = differ.m_paddingColour;
                    line = "";
                }

                // Truncate the line as necessary
                //
                string drawLine = line.Substring(view.getBufferShowStartX(), Math.Min(line.Length - view.getBufferShowStartX(), view.getBufferShowWidth()));
                if (view.getBufferShowStartX() + view.getBufferShowWidth() < line.Length)
                {
                    drawLine += " [>]";
                }

                m_context.m_spriteBatch.DrawString(
                                m_context.m_fontManager.getViewFont(view.getViewSize()),
                                drawLine,
                                new Vector2((int)viewSpaceTextPosition.X /* + m_context.m_fontManager.getCharWidth() * xPos */, (int)(viewSpaceTextPosition.Y + yPosition)),
                                colour,
                                0,
                                Vector2.Zero,
                                m_context.m_fontManager.getTextScale(),
                                0,
                                0);

                //sourceLine++;

                yPosition += m_context.m_fontManager.getLineSpacing(view.getViewSize());

            }
        }

        /// <summary>
        /// Draw a screen which allows us to configure some settings
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="text"></param>
        protected void drawConfigurationScreen(GameTime gameTime, XygloKeyboardHandler keyboardHandler)
        {
            bool editConfigurationItem = keyboardHandler.getEditConfigurationItem();
            string editConfigurationItemValue = keyboardHandler.getEditConfigurationItemValue();

            Vector3 fp = m_context.m_project.getSelectedBufferView().getPosition();

            // Starting positions
            //
            float yPos = 5.5f * m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay);
            float xPos = 10 * m_context.m_fontManager.getCharWidth(FontManager.FontType.Overlay);

            // Start the spritebatch
            //
            m_context.m_overlaySpriteBatch.Begin();

            if (editConfigurationItem) // Edit a single configuration item
            {
                string text = "Edit configuration item";

                m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), text, new Vector2((int)xPos, (int)yPos), Color.White, 0, Vector2.Zero, 1.0f, 0, 0);
                yPos += m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay) * 2;

                m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), m_context.m_project.getConfigurationItem(keyboardHandler.getConfigPosition()).Name, new Vector2((int)xPos, (int)yPos), ColourScheme.getItemColour(), 0, Vector2.Zero, 1.0f, 0, 0);
                yPos += m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay);

                string configString = editConfigurationItemValue;
                if (configString.Length > m_context.m_project.getSelectedView().getBufferShowWidth())
                {
                    configString = "[..]" + configString.Substring(configString.Length - m_context.m_project.getSelectedView().getBufferShowWidth() + 4, m_context.m_project.getSelectedView().getBufferShowWidth() - 4);
                }

                m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), configString, new Vector2((int)xPos, (int)yPos), ColourScheme.getHighlightColour(), 0, Vector2.Zero, 1.0f, 0, 0);
            }
            else
            {
                string text = "Configuration Items";

                m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), text, new Vector2((int)xPos, (int)yPos), Color.White, 0, Vector2.Zero, 1.0f, 0, 0);
                yPos += m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay) * 2;

                // Write all the configuration items out - if we're highlight one of them then change
                // the colour.
                //
                for (int i = 0; i < m_context.m_project.getConfigurationListLength(); i++)
                {
                    string configItem = m_context.m_project.estimateFileStringTruncation("", m_context.m_project.getConfigurationItem(i).Value, 60 - m_context.m_project.getConfigurationItem(i).Name.Length);
                    //string item = m_context.m_project.getConfigurationItem(i).Name + "  =  " + m_context.m_project.getConfigurationItem(i).Value;
                    string item = m_context.m_project.getConfigurationItem(i).Name + "  =  " + configItem;

                    item = m_context.m_project.estimateFileStringTruncation("", item, m_context.m_project.getSelectedView().getBufferShowWidth());

                    /*
                    if (item.Length > m_context.m_project.getSelectedView().getBufferShowWidth())
                    {
                        item = item.Substring(m_configXOffset, m_context.m_project.getSelectedView().getBufferShowWidth());
                    }
                    */

                    m_context.m_overlaySpriteBatch.DrawString(m_context.m_fontManager.getOverlayFont(), item, new Vector2((int)xPos, (int)yPos), (i == keyboardHandler.getConfigPosition() ? ColourScheme.getHighlightColour() : ColourScheme.getItemColour()), 0, Vector2.Zero, 1.0f, 0, 0);
                    yPos += m_context.m_fontManager.getLineSpacing(FontManager.FontType.Overlay);
                }
            }

            m_context.m_overlaySpriteBatch.End();
        }

  /*
        /// <summary>
        /// Check for collisions in m_context.m_drawableComponentss that have some form (hardness != 0).   Return true
        /// if we have a collision and we're also modifying the drawables to have correct velocity
        /// changes.
        /// </summary>
        protected bool computeCollisions()
        {
            // We'll have to iterate the realDict twice but here we filter in any goodies or baddies
            // and also anything that has a hardness (isCorporeal).
            //
            Dictionary<Component, XygloXnaDrawable> realDict = m_context.m_drawableComponents.Where(item => item.Key.isCorporeal() || item.Key.getBehaviour() != Behaviour.Goody).ToDictionary(p => p.Key, p => p.Value);

            // Keep a list of all elements we've applied collisions to
            //
            List<XygloXnaDrawable> collisionList = new List<XygloXnaDrawable>();

            // Everything we're interating over has hardness or is a prize/baddy - so they could potentially interact 
            //
            foreach (Component realKey in realDict.Keys)
            {
                // Skip all but Interloper for the moment
                //
                if (realKey.GetType() != typeof(Xyglo.Brazil.BrazilInterloper))
                    continue;

                BrazilInterloper il = (Xyglo.Brazil.BrazilInterloper)realKey;

                // For the moment we only 
                foreach (Component testKey in realDict.Keys)
                {
                    // Ignore ourself
                    if (testKey == realKey)
                        continue;

                    XygloXnaDrawable realComp = realDict[realKey];
                    XygloXnaDrawable testComp = realDict[testKey];

                    // If we've already processed this element
                    //
                    if (collisionList.Contains(testComp))
                        continue;

                    // Deal with Goodies
                    //
                    if (testKey.GetType() == typeof(Xyglo.Brazil.BrazilGoody))
                    {
                        BoundingBox bb1 = realComp.getBoundingBox();
                        BoundingSphere bs1 = ((XygloCoin)testComp).getBoundingSphere();

                        // Check for a collision
                        //
                        if (bb1.Intersects(bs1))
                        {
                            BrazilGoody goody = (BrazilGoody)testKey;
                            il.incrementScore(goody.m_worth);

                            // Set this item for destruction
                            //
                            testComp.setDestroy(true);

                            // Add collision item
                            //
                            collisionList.Add(testComp);
                        }
                    }
                }
            }

            return (collisionList.Count > 0);
        }*/

        /// <summary>
        /// Update all components both in the current context and within any embedded apps
        /// </summary>
        public void updateAllComponents()
        {
            // Update the components on the main component list (in case we have any)
            //
            m_context.m_drawingHelper.updateComponents(m_context.m_componentList, m_brazilContext.m_world);

            if (m_context.m_project != null)
            {
                List<BrazilView> brazilViews = m_context.m_project.getViews().Where(item => item.GetType() == typeof(BrazilView)).Cast<BrazilView>().ToList();

                foreach (BrazilView view in brazilViews)
                {
                    updateComponents(view.getApp().getComponents(), view.getApp().getWorld());
                }
            }
        }

        /// <summary>
        /// Process components for MOVEMENT or creation depending on key context
        /// </summary>
        /// <param name="components"></param>
        protected void updateComponents(List<Component> components, BrazilWorld world)
        {
            foreach (Component component in components)
            {
                // Has this component already been added to the drawableComponent dictionary?
                //
                if (m_context.m_drawableComponents.ContainsKey(component)) // && m_drawableComponents[component].getVelocity() != Vector3.Zero)
                {
                    // If so then process it for any movement
                    //
                    if (component.GetType() == typeof(Xyglo.Brazil.BrazilFlyingBlock))
                    {
                        // Found a FlyingBlock - initialise it and add it to the dictionary
                        //
                        BrazilFlyingBlock fb = (Xyglo.Brazil.BrazilFlyingBlock)component;
                        m_context.m_drawableComponents[component].buildBuffers(m_context.m_graphics.GraphicsDevice);
                    }
                    else if (component.GetType() == typeof(Xyglo.Brazil.BrazilInterloper))
                    {
                        BrazilInterloper il = (Xyglo.Brazil.BrazilInterloper)component;
                        m_context.m_drawableComponents[component].buildBuffers(m_context.m_graphics.GraphicsDevice);

                        // Check for collisions and adjust the position and velocity accordingly before drawing this
                        //
                        //computeCollisions();

                        // Store our interloper object
                        //
                        if (m_brazilContext.m_interloper == null)
                            m_brazilContext.m_interloper = il;
                    }
                    else if (component.GetType() == typeof(Xyglo.Brazil.BrazilGoody))
                    {
                        //Logger.logMsg("Draw Goody for the first time");
                        BrazilGoody bg = (BrazilGoody)component;

                        if (bg.m_type == BrazilGoodyType.Coin)
                        {
                            // For the moment the only movement is a rotation
                            //
                            if (bg.getRotation() != 0)
                            {
                                m_context.m_drawableComponents[component].incrementRotation(bg.getRotation());
                                m_context.m_drawableComponents[component].buildBuffers(m_context.m_graphics.GraphicsDevice);
                            }
                        }
                        else
                        {
                            throw new XygloException("Update", "Unsupported Goody Type");
                        }


                        //if (!m_context.m_drawableComponents[component].shouldBeDestroyed())
                            //m_context.m_drawableComponents[component].draw(m_context.m_graphics.GraphicsDevice);
                    }
                    else if (component.GetType() == typeof(BrazilTestBlock))
                    {
                        if (!m_context.m_drawableComponents[component].shouldBeDestroyed())
                        {
                            m_context.m_drawableComponents[component].buildBuffers(m_context.m_graphics.GraphicsDevice);
                        }
                    }
                    else if (component.GetType() == typeof(BrazilHud))
                    {
                        string bannerString = "";

                        if (m_frameCounter.getFrameRate() > 0)
                            bannerString += "FPS = " + m_frameCounter.getFrameRate() + "\n";

                        if (m_context.m_project != null)
                            bannerString += "[EyePosition] X " + m_eyeHandler.getEyePosition().X + ",Y " + m_eyeHandler.getEyePosition().Y + ",Z " + m_eyeHandler.getEyePosition().Z + "\n";

                        else if (m_brazilContext.m_interloper != null)
                        {
                            // Interloper position - get the component group and reverse engineer average position by cheating
                            //
                            XygloComponentGroup group = (XygloComponentGroup)m_context.m_drawableComponents[m_brazilContext.m_interloper];
                            group.updatePositionAfterPhysics();

                            //Vector3 ipPos = m_context.m_drawableComponents[m_brazilContext.m_interloper].getPosition();
                            //bannerString += "Interloper Position X = " + ipPos.X + ", Y = " + ipPos.Y + ", Z = " + ipPos.Z + "\n";

                            // Interloper score
                            //
                            bannerString += "Score = " + m_brazilContext.m_interloper.getScore() + "\n";
                            bannerString += "Lives = " + m_brazilContext.m_world.getLives() + "\n";
                        }

                        XygloBannerText bannerText = (XygloBannerText)m_context.m_drawableComponents[component];

                        if (bannerText != null)
                            bannerText.setText(bannerString);
                    }


                    //else if (component.GetType() == typeof(Xyglo.Brazil.BrazilMenu))
                    //{
                    //BrazilMenu menu = (BrazilMenu)component;
                    //}
                }
            }
        }

        /// <summary>
        /// This value is updated by drawing code to reflect changing sizing
        /// </summary>
        /// <returns></returns>
        public int getLastDrawTextScreenLength() { return m_textScreenLength; }

        /// <summary>
        /// BoundingBox for the BufferView preview
        /// </summary>
        protected BoundingBox m_previewBoundingBox;

        /// <summary>
        /// Greyed out colour for background text
        /// </summary>
        protected Color m_greyedColour = new Color(30, 30, 30, 50);

        /// <summary>
        /// How dark should our non-highlighted BufferViews be?
        /// </summary>
        protected float m_greyDivisor = 2.0f;

        /// <summary>
        /// List of highlights we're going to draw.  We don't want to fetch this everytime we
        /// draw the BufferView.
        /// </summary>
        protected List<Highlight> m_highlights;

        /// <summary>
        /// The colour of our banner
        /// </summary>
        protected Color m_bannerColour = new Color(180, 180, 180, 180);

        /// <summary>
        /// Start time for a banner
        /// </summary>
        protected double m_bannerStartTime = -1;

        /// <summary>
        /// Banner message
        /// </summary>
        protected string m_bannerString;

        /// <summary>
        /// Duration of a banner
        /// </summary>
        protected float m_bannerDuration;

        /// <summary>
        /// Strings within a banner if there are multiple
        /// </summary>
        protected List<string> m_bannerStringList;

        /// <summary>
        /// Set preview bounding box
        /// </summary>
        /// <param name="bb"></param>
        public void setPreviewBoundingBox(BoundingBox bb)
        {
            m_previewBoundingBox = bb;
        }

        /// <summary>
        /// Local top left vector
        /// </summary>
        protected Vector3 m_bottomLeft = new Vector3();

        /// <summary>
        /// Local top right vector
        /// </summary>
        protected Vector3 m_topRight = new Vector3();

        /// <summary>
        /// User help string
        /// </summary>
        protected string m_userHelp;

        /// <summary>
        /// XygloContext passed from the main app
        /// </summary>
        protected XygloContext m_context;

        /// <summary>
        /// BrazilContext
        /// </summary>
        protected BrazilContext m_brazilContext;

        /// <summary>
        /// Length of information screen - so we know if we can page up or down
        /// </summary>
        protected int m_textScreenLength = 0;

        /// <summary>
        /// FrameCounter reference
        /// </summary>
        protected FrameCounter m_frameCounter;

        /// <summary>
        /// EyeHandler reference
        /// </summary>
        protected EyeHandler m_eyeHandler;
    }

}
