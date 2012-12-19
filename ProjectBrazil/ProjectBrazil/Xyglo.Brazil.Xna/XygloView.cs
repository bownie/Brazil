#region File Description
//-----------------------------------------------------------------------------
// XygloView.cs
//
// Copyright (C) Xyglo Ltd. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace Xyglo.Brazil.Xna
{
    
    /// <summary>
    /// At the XygloView level we want to start building up the XygloXnaDrawable abstraction
    /// - for a later date.
    /// </summary>
    [DataContract(Name = "Friendlier", Namespace = "http://www.xyglo.com")]
    public abstract class XygloView //: DrawableComponent
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public XygloView()
        {
        }

        /// <summary>
        /// Define some window viewing sizes for different layouts of bufferview
        /// </summary>
        public enum ViewSize
        {
            Micro,
            Tiny,
            Small,
            Medium,
            Large,
            VeryLarge,
            Huge,
            Enormous
        };


        /// <summary>
        /// ViewPosition is used to help determine positions of other BufferViews
        /// </summary>
        public enum ViewPosition
        {
            Above,
            Below,
            Left,
            Right
        };

        /// <summary>
        /// Which Quadrant of four BufferViews are we viewing from the current one - cycling
        /// through these possibilities makes it nine total screens we can view
        /// </summary>
        public enum ViewQuadrant
        {
            TopRight,
            BottomRight,
            BottomLeft,
            TopLeft
        }

        /// <summary>
        /// Which direction we're cycling through quadrant views
        /// </summary>
        public enum ViewCycleDirection
        {
            Clockwise,
            Anticlockwise
        }


        /// <summary>
        /// Greyed out colour
        /// </summary>
        protected Color m_greyedColour = new Color(30, 30, 30, 50);

        /// <summary>
        /// Default text colour
        /// </summary>
        [DataMember]
        protected Color m_textColour = Color.White;

        /// <summary>
        /// Default cursor colour
        /// </summary>
        [DataMember]
        protected Color m_cursorColour = Color.Yellow;

        /// <summary>
        /// Current cursor coordinates in this BufferView
        /// </summary>
        [DataMember]
        protected ScreenPosition m_cursorPosition = new ScreenPosition(0, 0);

        /// <summary>
        /// Default highlight colour
        /// </summary>
        [DataMember]
        protected Color m_highlightColour = Color.PaleVioletRed;

        /// <summary>
        /// 3d position of the BufferView
        /// </summary>
        [DataMember]
        protected Vector3 m_position;

        /// <summary>
        /// Font manager reference passed in to provide font sizes for rendering
        /// </summary>
        [NonSerialized]
        protected FontManager m_fontManager = null;

        /// <summary>
        /// Length of visible buffer
        /// </summary>
        [DataMember]
        protected int m_bufferShowLength = getDefaultBufferShowLength();

        /// <summary>
        /// Number of characters to show in a BufferView line
        /// </summary>
        [DataMember]
        protected int m_bufferShowWidth = getDefaultBufferShowWidth();

        /// <summary>
        /// Width spacing between views
        /// </summary>
        [DataMember]
        protected int m_viewWidthSpacing = 15;

        /// <summary>
        /// Height spacing between views
        /// </summary>
        [DataMember]
        protected int m_viewHeightSpacing = 10;

        /// <summary>
        /// Viewing size of the BufferViews
        /// </summary>
        [DataMember]
        protected ViewSize m_viewSize = getDefaultViewSize();

        /// <summary>
        /// How many characters across we are showing the FileBuffer view from
        /// </summary>
        [DataMember]
        protected int m_bufferShowStartX = 0;

        /// <summary>
        /// How many lines we are stepped into the underlying FileBuffer
        /// </summary>
        [DataMember]
        protected int m_bufferShowStartY = 0;

        // ----------------------------------------- METHODS ------------------------------------------------------
        //
        /// <summary>
        /// Accessor for BufferShowWidth
        /// </summary>
        /// <returns></returns>
        public int getBufferShowWidth()
        {
            return m_bufferShowWidth;
        }

        /// <summary>
        /// Get BufferShow length
        /// </summary>
        /// <returns></returns>
        public int getBufferShowLength()
        {
            return m_bufferShowLength;
        }

        /// <summary>
        /// Default setting for height
        /// </summary>
        /// <returns></returns>
        static public int getDefaultBufferShowLength()
        {
            return 20;
        }

        /// <summary>
        /// Default setting for width
        /// </summary>
        /// <returns></returns>
        static public int getDefaultBufferShowWidth()
        {
            return 80;
        }

        /// <summary>
        /// Default view size
        /// </summary>
        /// <returns></returns>
        static public ViewSize getDefaultViewSize()
        {
            return ViewSize.Medium;
        }

        /// <summary>
        /// Ensure that XygloViews can describe their Width
        /// </summary>
        /// <returns></returns>
        public abstract float getWidth();

        /// <summary>
        /// Ensure that XygloViews can describe their height
        /// </summary>
        /// <returns></returns>
        public abstract float getHeight();

        /// <summary>
        /// Ensure that XygloViews can describe their depth
        /// </summary>
        /// <returns></returns>
        public abstract float getDepth();

        /// <summary>
        /// Draw this object utilising an external SpriteBatch - main rendering for text
        /// </summary>
        public abstract void draw(Project project, State state, GameTime gameTime, SpriteBatch spriteBatch, Effect effect);

        /// <summary>
        /// Draw any textured objects such as highlights
        /// </summary>
        /// <param name="effect"></param>
        public abstract void drawTextures(Effect effect);

        /// <summary>
        /// Get the generic eye position
        /// </summary>
        /// <returns></returns>
        public abstract Vector3 getEyePosition();

        /// <summary>
        /// Return the eye vector of the centre of this view for a given zoom level
        /// </summary>
        /// <returns></returns>
        public abstract Vector3 getEyePosition(float zoomLevel);


        /// <summary>
        /// Calculate the position of the next BufferView relative to us - these factors aren't constant
        /// and shouldn't be declared as such but for the moment they usually do.  We can also specify a 
        /// factor to spread out the bounding boxes further if they are required.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public abstract Vector3 calculateRelativePositionVector(ViewPosition position, int factor = 1);

        /// <summary>
        /// Do the same as above but return a BoundingBox
        /// </summary>
        /// <param name="position"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public abstract BoundingBox calculateRelativePositionBoundingBox(ViewPosition position, int width, int height, int factor = 1);

        /// <summary>
        /// Get the view size
        /// </summary>
        /// <returns></returns>
        public ViewSize getViewSize()
        {
            return m_viewSize;
        }

        /// <summary>
        /// Description of view size
        /// </summary>
        /// <returns></returns>
        public string getViewSizeDescription()
        {
            switch (m_viewSize)
            {
                case ViewSize.Tiny:
                    return "Tiny";

                case ViewSize.Small:
                    return "Small";

                case ViewSize.Micro:
                    return "Micro";

                case ViewSize.Medium:
                    return "Medium";

                case ViewSize.Large:
                    return "Large";

                case ViewSize.Huge:
                    return "Huge";

                case ViewSize.Enormous:
                    return "Enormous";

                case ViewSize.VeryLarge:
                    return "Very Large";

                default:
                    return "Default";
            }
        }

        /// <summary>
        /// Set the view size and dimensions accordingly.  Width and height of a BufferView/XygloView
        /// has to be adjusted in accordance with the window size so we need to pass that in too along
        /// with the font manager so we can measure the font size.
        /// </summary>
        /// <param name="viewSize"></param>
        public void setViewSize(ViewSize viewSize, int windowWidth, int windowHeight, FontManager fontmanager)
        {
            m_viewSize = viewSize;

            SpriteFont font = fontmanager.getViewFont(viewSize);

            int lines = windowHeight / font.LineSpacing;
            int cols = (int)((float)windowWidth / (font.MeasureString("X").X));

            lines = Math.Max(lines - 6, 8);
            cols = Math.Max(cols - 10, 12);

            m_bufferShowWidth = cols;
            m_bufferShowLength = lines;
            /*
            switch (viewSize)
            {
                case ViewSize.Micro:
                case ViewSize.Tiny:
                case ViewSize.Small:
                    m_bufferShowWidth = 140;
                    m_bufferShowLength = 40;
                    break;

                default:
                case ViewSize.Medium:
                    m_bufferShowWidth = 80;
                    m_bufferShowLength = 20;
                    break;

                case ViewSize.Large:
                case ViewSize.VeryLarge:
                case ViewSize.Huge:
                case ViewSize.Enormous:
                    m_bufferShowWidth = 50;
                    m_bufferShowLength = 12;
                    break;
            }*/
        }

        /// <summary>
        /// Increment view size - returning the scale of the current font according to the new font we're
        /// selecting so we can scale up smoothly.
        /// </summary>
        public double incrementViewSize(GameTime gameTime, int windowWidth, int windowHeight, FontManager fontManager)
        {
            double scale = 1.0f;

            if (m_viewSize < ViewSize.Enormous)
            {
                scale = fontManager.getViewFont(m_viewSize).MeasureString("X").X / fontManager.getViewFont(m_viewSize + 1).MeasureString("X").X;
                m_viewSize++;
            }

            setViewSize(m_viewSize, windowWidth, windowHeight, fontManager);

            // Keep cursor visible and flash
            keepVisible();
            flashBufferView(gameTime);

            return scale;
        }

        /// <summary>
        /// Decrement view size - returning the scale of the current font according to the new font
        /// we're selecting so we can scale down smoothly.
        /// </summary>
        public double decrementViewSize(GameTime gameTime, int windowWidth, int windowHeight, FontManager fontManager)
        {
            double scale = 1.0f;
            if (m_viewSize > ViewSize.Micro)
            {
                scale = fontManager.getViewFont(m_viewSize).MeasureString("X").X / fontManager.getViewFont(m_viewSize - 1).MeasureString("X").X;
                m_viewSize--;
            }

            setViewSize(m_viewSize, windowWidth, windowHeight, fontManager);

            // Keep cursor visible and flash
            keepVisible();
            flashBufferView(gameTime);


            return scale;
        }

        /// <summary>
        /// Ensure that the cursor is on the screen
        /// </summary>
        protected void keepVisible()
        {
            // Ensure Y is visible
            //
            if (m_cursorPosition.Y < m_bufferShowStartY)
            {
                m_bufferShowStartY = m_cursorPosition.Y;
            }
            else if (m_cursorPosition.Y > (m_bufferShowStartY + m_bufferShowLength - 1))
            {
                m_bufferShowStartY = m_cursorPosition.Y - (m_bufferShowLength - 1);
            }

            // Ensure X is visible
            //
            if (m_cursorPosition.X < m_bufferShowStartX)
            {
                m_bufferShowStartX = m_cursorPosition.X;
            }
            else if (m_cursorPosition.X > (m_bufferShowStartX + m_bufferShowWidth - 1))
            {
                m_bufferShowStartX = m_cursorPosition.X - (m_bufferShowWidth - 1);
            }
        }

        [NonSerialized]
        protected double m_startFlashTime = 0.0f;  // seconds

        [NonSerialized]
        protected double m_endFlashTime = 0.0f; // seconds

        [DataMember]
        protected float m_minAlpha = 0.05f;

        [DataMember]
        protected double m_flashInterval = 0.1f;

        /// <summary>
        /// Provoke the background to flash for a period
        /// </summary>
        /// <param name="gameTime"></param>
        protected void flashBufferView(GameTime gameTime)
        {
            m_startFlashTime = gameTime.TotalGameTime.TotalSeconds;
            m_endFlashTime = m_startFlashTime + m_flashInterval;
            //Logger.logMsg("START GAME TIME = " + m_startFlashTime);
        }

    }
}
