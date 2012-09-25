﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// An abstract base class that we can use to hold the things we need to 
    /// draw an XNA item.
    /// </summary>
    public abstract class XygloXnaDrawable // --> DrawableGameComponent
    {
        public Color getColour()
        {
            return m_colour;
        }

        public void setColour(Color colour)
        {
            m_colour = colour;
        }

        /// <summary>
        /// Draw this component
        /// </summary>
        public abstract void draw(GraphicsDevice device);

        /// <summary>
        /// Build the Vertex and Index buffers
        /// </summary>
        /// <param name="device"></param>
        public abstract void buildBuffers(GraphicsDevice device);

        /// <summary>
        /// Add a vector3 to our position
        /// </summary>
        /// <param name="translation"></param>
        public void move(Vector3 translation)
        {
            m_position += translation;
        }

        /// <summary>
        /// Set the rotation
        /// </summary>
        /// <param name="rotation"></param>
        public void setRotation(double rotation)
        {
            m_rotation = rotation;
        }

        /// <summary>
        /// Incremenet a rotation
        /// </summary>
        /// <param name="rotation"></param>
        public void incrementRotation(double rotation)
        {
            m_rotation += rotation;

            // Check bounds
            if (m_rotation > 2 * Math.PI)
            {
                m_rotation -= 2 * Math.PI;
            }
            else if (m_rotation < -2 * Math.PI)
            {
                m_rotation += 2 * Math.PI;
            }
        }

        /// <summary>
        /// Get the rotation
        /// </summary>
        /// <returns></returns>
        public double getRotation()
        {
            return m_rotation;
        }

        /// <summary>
        /// Position of this block
        /// </summary>
        public Vector3 m_position;

        /// <summary>
        /// Store locally our effect
        /// </summary>
        protected BasicEffect m_effect;

        /// <summary>
        /// Store locally our colour
        /// </summary>
        protected Color m_colour;

        /// <summary>
        /// Rotation angle per frame
        /// </summary>
        protected double m_rotation = 0.0;
    }
}