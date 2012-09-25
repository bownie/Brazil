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
        /// <param name="movement"></param>
        public void move(Vector3 movement)
        {
            m_position += movement;
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
    }
}
