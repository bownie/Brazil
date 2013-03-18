using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// A wrapper class to define a finger pointer object.  At the moment this subclasses the XygloFlyingBlock so we need
    /// to do calculations here to translate the start and end points to the position/size metaphor.
    /// </summary>
    public class XygloFingerBone : XygloFlyingBlock
    {
        public XygloFingerBone(int fingerId, string handIndex, BasicEffect effect, Vector3 startPosition, Vector3 endPosition) : base(Color.Beige, effect)
        {
            m_fingerId = fingerId;
            m_handIndex = handIndex;
            setFingerEndpoints(startPosition, endPosition);

            m_blockSize.X = m_width;
            m_blockSize.Y = m_width;
            m_blockSize.Z = m_width;
        }

        protected void setFingerEndpoints(Vector3 startPosition, Vector3 endPosition)
        {
            m_position = (startPosition + endPosition) / 2;
            //m_orientation 
            //Ray fingerRay = new Ray(startPosition, endPosition.Normalize);
            //fingerRay.

        }

        protected float m_width = 50.0f;
        /// <summary>
        /// Id of this finger pointer
        /// </summary>
        protected int m_fingerId;

        /// <summary>
        /// Store the hand index to allow remapping
        /// </summary>
        protected string m_handIndex;

    }
}
