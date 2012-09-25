﻿using System;
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
        /// Position
        /// </summary>
        protected BrazilVector3 m_position;

        /// <summary>
        /// Give a velocity vector
        /// </summary>
        protected BrazilVector3 m_velocity;

        /// <summary>
        /// A rotation angle (per frame currently)
        /// </summary>
        protected double m_rotation = 0.0f;

        /// <summary>
        /// A colour associated with our 3D component
        /// </summary>
        protected BrazilColour m_colour;
    }
}