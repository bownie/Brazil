using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A Flying Block - of course
    /// </summary>
    public class FlyingBlock : Component3D
    {
        /// <summary>
        /// FlyingBlock constructor
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="position"></param>
        public FlyingBlock(BrazilColour colour, BrazilVector3 position)
        {
            m_colour = colour;
            m_position = position;
        }

        /// <summary>
        /// Update override - handle any modifications to this object
        /// </summary>
        public override void update()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Draw override - draw this object
        /// </summary>
        public override void draw()
        {
            throw new NotImplementedException();
        }

        protected BrazilVector3 m_dimensions = new BrazilVector3();

        protected BrazilColour m_colour;
    }
}
