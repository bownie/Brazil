using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Ray based on XNA's Ray
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com")]
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
        protected BrazilVector3 m_startPos;

        /// <summary>
        /// End position
        /// </summary>
        protected BrazilVector3 m_endPos;
    }
}
