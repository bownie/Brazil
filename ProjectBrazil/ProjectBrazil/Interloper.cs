using System;
using System.Collections.Generic;
using System.Text;


namespace Xyglo.Brazil
{
    /// <summary>
    /// The Interloper is our hero
    /// </summary>
    public class Interloper : Component3D
    {
        public Interloper(BrazilColour colour, BrazilVector3 position, BrazilVector3 size)
        {
            m_colour = colour;
            m_position = position;
            m_dimensions = size;
        }

        /// <summary>
        /// Get the size of this interloper
        /// </summary>
        /// <returns></returns>
        public BrazilVector3 getSize()
        {
            return m_dimensions;
        }

        /// <summary>
        /// Dimensions of this Interloper
        /// </summary>
        protected BrazilVector3 m_dimensions;

    }
}
