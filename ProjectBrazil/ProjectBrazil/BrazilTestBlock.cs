﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A Flying Block - of course
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com", IsReference = true)]
    [KnownType(typeof(BrazilTestBlock))]
    public class BrazilTestBlock : Component3D, IPhysicalObject
    {
        /// <summary>
        /// FlyingBlock constructor
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="position"></param>
        public BrazilTestBlock(BrazilColour colour, BrazilVector3 position, BrazilVector3 size, bool affectedByGravity = true)
        {
            m_colour = colour;
            m_position = position;
            m_dimensions = size;
            m_gravityAffected = affectedByGravity;
            //m_mass = 1000; // default mass
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

        public bool isStatic() { return m_isStatic; }
        public void setStatic(bool isStatic) { m_isStatic = isStatic; }

        /// <summary>
        /// The size of this block
        /// </summary>
        [DataMember]
        protected BrazilVector3 m_dimensions = new BrazilVector3();

        /// <summary>
        /// Angle of our flying/floating block
        /// </summary>
        [DataMember]
        protected double m_pitch = 0.0f;

        /// <summary>
        /// How often does this block oscillate?
        /// </summary>
        [DataMember]
        protected float m_oscillationFrequency = 0.0f;

        /// <summary>
        /// What path does this block oscillate upon?
        /// </summary>
        [DataMember]
        protected BrazilRay m_oscillationPath = null;

        /// <summary>
        /// Static member?
        /// </summary>
        [DataMember]
        protected bool m_isStatic = false;
    }
}
