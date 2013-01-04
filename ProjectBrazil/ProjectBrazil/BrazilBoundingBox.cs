using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A BoundingBox
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com")]
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
        /// Vector constructor - get the position of the vectors correct
        /// </summary>
        /// <param name="single"></param>
        public BrazilBoundingBox(BrazilVector3 v1, BrazilVector3 v2)
        {
            if (v1.X * v1.Y * v1.Z < v2.X * v2.Y * v2.Z)
            {
                m_startPos = v1;
                m_endPos = v2;
            }
            else
            {
                m_startPos = v2;
                m_endPos = v1;
            }
        }

        /// <summary>
        /// Get the minimum point of the BoundingBox
        /// </summary>
        /// <returns></returns>
        public BrazilVector3 getMinimum()
        {
            if (m_startPos.X * m_startPos.Y * m_startPos.Z < m_endPos.X * m_endPos.Y * m_endPos.Z)
            {
                return m_startPos;
            }
            else
            {
                return m_endPos;
            }
        }

        /// <summary>
        /// Get the maximum point of the BoundingBox
        /// </summary>
        /// <returns></returns>
        public BrazilVector3 getMaximum()
        {
            if (m_startPos.X * m_startPos.Y * m_startPos.Z > m_endPos.X * m_endPos.Y * m_endPos.Z)
            {
                return m_startPos;
            }
            else
            {
                return m_endPos;
            }
        }

        public float getWidth() { return m_endPos.X - m_startPos.X; }
        public float getHeight() { return m_endPos.Y - m_startPos.Y; }
        public float getDepth() { return m_endPos.Z - m_startPos.Y; }

        /// <summary>
        /// Start position for bounding box
        /// </summary>
        [DataMember]
        protected BrazilVector3 m_startPos;

        /// <summary>
        /// End position for bounding box
        /// </summary>
        [DataMember]
        protected BrazilVector3 m_endPos;
    }
}
