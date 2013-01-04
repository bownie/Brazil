using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A 3D Drawable Component
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com", IsReference = true)]
    [KnownType(typeof(Component3D))]
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
        /// Accelerate this object by a Vector if it is affected by gravity and has a velocity
        /// </summary>
        /// <param name="acceleration"></param>
        public void accelerate(BrazilVector3 acceleration)
        {
            // Check for null velocity - a lot of objects won't need this at all so
            // a valid check.
            //
            //if (m_velocity == null)
            //{
                m_velocity = BrazilVector3.Zero;
            //}

            m_velocity.X += acceleration.X;
            m_velocity.Y += acceleration.Y;
            m_velocity.Z += acceleration.Z;
        }


        /// <summary>
        /// Get the Colour
        /// </summary>
        /// <returns></returns>
        public BrazilColour getColour()
        {
            return m_colour;
        }

        /// <summary>
        /// Set the Colour
        /// </summary>
        /// <param name="colour"></param>
        public void setColour(BrazilColour colour)
        {
            m_colour = colour;
        }

        /// <summary>
        /// Get any rotation
        /// </summary>
        /// <returns></returns>
        public double getRotation()
        {
            return m_rotation;
        }

        /// <summary>
        /// Set any rotation
        /// </summary>
        /// <param name="rotation"></param>
        public void setRotation(double rotation)
        {
            m_rotation = rotation;
        }

        /// <summary>
        /// Get initial angle
        /// </summary>
        /// <returns></returns>
        public double getInitialAngle()
        {
            return m_initialAngle;
        }

        /// <summary>
        /// Set initial angle
        /// </summary>
        /// <param name="rotation"></param>
        public void setInitialAngle(double rotation)
        {
            m_initialAngle = rotation;
        }

        /// <summary>
        /// Position
        /// </summary>
        [DataMember]
        protected BrazilVector3 m_position;

        /// <summary>
        /// Give a velocity vector
        /// </summary>
        [DataMember]
        protected BrazilVector3 m_velocity;

        /// <summary>
        /// A rotation angle (per frame currently)
        /// </summary>
        [DataMember]
        protected double m_rotation = 0.0f;

        /// <summary>
        /// An initial angle for this element (in one dimension only)
        /// </summary>
        [DataMember]
        protected double m_initialAngle = 0.0f;

        /// <summary>
        /// A colour associated with our 3D component
        /// </summary>
        [DataMember]
        protected BrazilColour m_colour;
    }
}
