﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A place which signifies the end of the screen or level and can be used to indicate
    /// a transition should take place.
    /// </summary>
    public class BrazilFinishBlock : Component3D
    {
        /// <summary>
        /// FinishBlock constructor
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="position"></param>
        public BrazilFinishBlock(BrazilColour colour, BrazilVector3 position, BrazilVector3 size, State transitionState)
        {
            m_colour = colour;
            m_position = position;
            m_dimensions = size;
            m_transitionState = transitionState;
        }

        /// <summary>
        /// Get the Size
        /// </summary>
        /// <returns></returns>
        public BrazilVector3 getSize()
        {
            return m_dimensions;
        }

        /// <summary>
        /// Set the Size
        /// </summary>
        /// <param name="size"></param>
        public void setSize(BrazilVector3 size)
        {
            m_dimensions = size;
        }

        /// <summary>
        /// Other setting method
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void setSize(float x, float y, float z)
        {
            m_dimensions.X = x;
            m_dimensions.Y = y;
            m_dimensions.Z = z;
        }

        /// <summary>
        /// The size of this goddamn FlyingBlock
        /// </summary>
        protected BrazilVector3 m_dimensions = new BrazilVector3();

        /// <summary>
        /// State to which we will transition when we hit this
        /// </summary>
        protected State m_transitionState;

        /// <summary>
        /// Angle of our flying/floating block
        /// </summary>
        protected double m_pitch = 0.0f;

        /// <summary>
        /// How often does this block oscillate?
        /// </summary>
        protected float m_oscillationFrequency = 0.0f;

        /// <summary>
        /// What path does this block oscillate upon?
        /// </summary>
        protected BrazilRay m_oscillationPath = null;
    }
}