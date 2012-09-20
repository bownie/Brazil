using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A 3D Drawable Component
    /// </summary>
    public abstract class Component3D : DrawableComponent
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Component3D()
        {
        }

        /// <summary>
        /// Get the position
        /// </summary>
        /// <returns></returns>
        public BrazilVector3 getPosition()
        {
            return m_position;
        }

        /// <summary>
        /// Set the position
        /// </summary>
        /// <param name="position"></param>
        public void setPosition(BrazilVector3 position)
        {
            m_position = position;
        }

        /// <summary>
        /// Get the velocity
        /// </summary>
        /// <returns></returns>
        public BrazilVector3 getVelocity()
        {
            return m_velocity;
        }

        /// <summary>
        /// Set the velocity
        /// </summary>
        /// <param name="velocity"></param>
        public void setVelocity(BrazilVector3 velocity)
        {
            m_velocity = velocity;
        }

        /// <summary>
        /// Position
        /// </summary>
        protected BrazilVector3 m_position;

        /// <summary>
        /// Give a velocity vector
        /// </summary>
        protected BrazilVector3 m_velocity;
    }
}
