using System;
using System.Collections.Generic;
using System.Text;


namespace Xyglo.Brazil
{
    /// <summary>
    /// The Interloper is our hero
    /// </summary>
    public class BrazilInterloper : Component3D
    {
        public BrazilInterloper(BrazilColour colour, BrazilVector3 position, BrazilVector3 size)
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
        /// Override the virtual bounding box - note that we draw things from the m_position at a size around that
        /// point and not starting at that point - hence the BoundingBox has to match
        /// </summary>
        /// <returns></returns>
        public override BrazilBoundingBox getBoundingBox()
        {
            BrazilVector3 tl = m_position;
            tl.X -= m_dimensions.X / 2;
            tl.Y -= m_dimensions.Y / 2;
            tl.Z -= m_dimensions.Z / 2;

            BrazilVector3 br = m_position;
            br.X += m_dimensions.X / 2;
            br.Y += m_dimensions.Y / 2;
            br.Z += m_dimensions.Z / 2;

            return new BrazilBoundingBox(tl, br);
        }

        /// <summary>
        /// Dimensions of this Interloper
        /// </summary>
        protected BrazilVector3 m_dimensions;

    }
}
