using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Ray based on XNA's Ray
    /// </summary>
    public class BrazilRay
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BrazilRay()
        {
        }

        /// <summary>
        /// Double constructor
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        public BrazilRay(BrazilVector3 v1, BrazilVector3 v2)
        {
            m_startPos = v1;
            m_endPos = v1;
        }

        /// <summary>
        /// Start position
        /// </summary>
        protected BrazilVector3 m_startPos = null;

        /// <summary>
        /// End position
        /// </summary>
        protected BrazilVector3 m_endPos = null;
    }
}
