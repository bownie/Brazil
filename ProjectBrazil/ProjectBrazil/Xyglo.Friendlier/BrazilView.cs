using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

using Xyglo.Brazil;
using Xyglo.Brazil.Xna;

namespace Xyglo.Friendlier
{
    /// <summary>
    /// A BrazilView is a XygloView implementation for hosting a BrazilApp.
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com")]
    [KnownType(typeof(BrazilView))]
    public class BrazilView : XygloView
    {
        public BrazilView(BrazilApp app, Vector3 Position)
        {
            m_app = app;
            m_position = Position;

            if (m_defaultMeasure.X == 0)
                m_defaultMeasure.X = 12;

            if (m_defaultMeasure.Y == 0)
                m_defaultMeasure.Y = 6;

            // Apply app to all components
            //
            applyComponents();
        }

        /// <summary>
        /// Apply app to components within app
        /// </summary>
        protected void applyComponents()
        {
            foreach (Component comp in m_app.getComponents())
            {
                comp.setApp(m_app);
            }
        }

        /// <summary>
        /// Position of the Eye over our view
        /// </summary>
        /// <returns></returns>
        public override Vector3 getEyePosition()
        {
            Vector3 rV = m_position;
            rV.Y = -rV.Y; // invert Y
            rV.X += m_defaultMeasure.X * m_bufferShowWidth / 2;
            rV.Y -= m_defaultMeasure.Y * m_bufferShowLength / 2;
            rV.Z += 600.0f;
            return rV;
        }

        /// <summary>
        /// Get the eye position at a zoom level
        /// </summary>
        /// <param name="zoomLevel"></param>
        /// <returns></returns>
        public override Vector3 getEyePosition(float zoomLevel)
        {
            Vector3 rV = m_position;
            rV.Y = -rV.Y; // invert Y
            rV.X += m_defaultMeasure.X * m_bufferShowWidth / 2;
            rV.Y -= m_defaultMeasure.Y * m_bufferShowLength / 2;
            rV.Z = zoomLevel;
            return rV;
        }

        public override float getViewCharHeight()
        {
            return m_defaultMeasure.Y;
        }

        public override float getViewCharWidth()
        {
            return m_defaultMeasure.X;
        }

        public override BoundingBox calculateRelativePositionBoundingBox(ViewPosition position, int widthChars = -1, int heightChars = -1, int factor = 1)
        {
            // If these are unset then use the size of this BufferView as default
            if (widthChars == -1)
                widthChars = m_bufferShowWidth;

            if (heightChars == -1)
                heightChars = m_bufferShowLength;

            BoundingBox bb = new BoundingBox();
            bb.Min = calculateRelativePositionVector(position, factor);
            bb.Max = bb.Min;
            bb.Max.X += m_defaultMeasure.X * widthChars + m_viewWidthSpacing;
            bb.Max.Y += m_defaultMeasure.Y * heightChars + m_viewHeightSpacing;
            return bb;
        }

        public override void draw(Project project, State state, GameTime gameTime, SpriteBatch spriteBatch, Effect effect)
        {
            throw new NotImplementedException();
        }

        public override void drawTextures(Effect effect)
        {
            throw new NotImplementedException();
        }

        public override Vector3 calculateRelativePositionVector(ViewPosition position, int factor = 1)
        {
            Vector3 rP = m_position;

            switch (position)
            {
                case ViewPosition.Above:
                    rP = m_position - (new Vector3(0.0f, factor * (m_bufferShowLength + m_viewHeightSpacing) * m_defaultMeasure.Y, 0.0f));
                    break;

                case ViewPosition.Below:
                    rP = m_position + (new Vector3(0.0f, factor * (m_bufferShowLength + m_viewHeightSpacing) * m_defaultMeasure.Y, 0.0f));
                    break;

                case ViewPosition.Left:
                    rP = m_position - (new Vector3(factor * m_defaultMeasure.X * (m_bufferShowWidth + m_viewWidthSpacing), 0.0f, 0.0f));
                    break;

                case ViewPosition.Right:
                    rP = m_position + (new Vector3(factor * m_defaultMeasure.X * (m_bufferShowWidth + m_viewWidthSpacing), 0.0f, 0.0f));
                    break;

                default:
                    throw new Exception("Unknown position parameter passed");
            }

            return rP;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }


        /// <summary>
        /// Get the BrazilApp
        /// </summary>
        /// <returns></returns>
        public BrazilApp getApp()
        {
            return m_app;
        }

        /// <summary>
        /// BoundingBox for the BrazilView
        /// </summary>
        /// <returns></returns>
        public override BoundingBox getBoundingBox()
        {
            Vector3 bottomRight = m_position;
            bottomRight.X += m_bufferShowWidth * m_defaultMeasure.X;
            bottomRight.Y += m_bufferShowLength * m_defaultMeasure.Y;
            return new BoundingBox(m_position, bottomRight);
        }

        public override float getDepth()
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override float getHeight() { return m_bufferShowLength * m_defaultMeasure.Y; }
        public override float getWidth() { return m_bufferShowWidth * m_defaultMeasure.X; }

        public override string ToString()
        {
            return base.ToString();
        }

        /// <summary>
        /// For this type of view we hardcode the 'character' size but still
        /// use the aspect ratio set by bufferShowWidth/Length.
        /// </summary>
        [DataMember]
        protected Vector2 m_defaultMeasure = new Vector2();

        /// <summary>
        /// The BrazilApp defines an application that sits within this view
        /// </summary>
        [DataMember]
        protected BrazilApp m_app;
    }
}
