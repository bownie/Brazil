using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A BoundingBox
    /// </summary>
    public class BrazilBoundingBox
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BrazilBoundingBox()
        {
            m_startPos = BrazilVector3.Zero;
            m_endPos = BrazilVector3.Zero;
        }

        /// <summary>
        /// Single constructor
        /// </summary>
        /// <param name="single"></param>
        public BrazilBoundingBox(BrazilVector3 v1, BrazilVector3 v2)
        {
            m_startPos = v1;
            m_endPos = v2;
        }

        /// <summary>
        /// Start position for bounding box
        /// </summary>
        protected BrazilVector3 m_startPos;

        /// <summary>
        /// End position for bounding box
        /// </summary>
        protected BrazilVector3 m_endPos;
    }
}
