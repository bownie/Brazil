using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{
    /// <summary>
    /// The Baddy is one of our villians
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com")]
    public class BrazilBaddy : Component3D
    {
        public BrazilBaddy(BrazilColour colour, BrazilVector3 position, BrazilVector3 size)
        {
            m_colour = colour;
            m_position = position;
            m_dimensions = size;

            // Is affected by gravity - set mass and hardness
            //
            m_gravityAffected = true;
            m_mass = 10;
            m_hardness = 0.3f;
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
