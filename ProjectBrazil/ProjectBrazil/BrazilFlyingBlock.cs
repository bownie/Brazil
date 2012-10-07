using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A Flying Block - of course
    /// </summary>
    public class BrazilFlyingBlock : Component3D
    {
        /// <summary>
        /// FlyingBlock constructor
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="position"></param>
        public BrazilFlyingBlock(BrazilColour colour, BrazilVector3 position, BrazilVector3 size)
        {
            m_colour = colour;
            m_position = position;
            m_dimensions = size;
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
        /// The size of this goddamn FlyingBlock
        /// </summary>
        protected BrazilVector3 m_dimensions = new BrazilVector3();

    }
}
