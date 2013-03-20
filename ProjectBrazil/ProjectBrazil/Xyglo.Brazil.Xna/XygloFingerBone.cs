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

            m_blockSize.Y = m_width;
            m_blockSize.Z = m_width;
            m_blockSize.X = (endPosition - startPosition).Length();
        }

        /// <summary>
        /// modify position and 
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        protected void setFingerEndpoints(Vector3 startPosition, Vector3 endPosition)
        {
            m_position = (startPosition + endPosition) / 2;
            m_orientation = createQuarternion(startPosition, endPosition);




            m_blockSize.Z = (endPosition - startPosition).Length();
            m_blockSize.X = m_width;
            m_blockSize.Y = m_width;
        }

        /// <summary>
        /// Create a quarternion from the difference of two vectors - this quarternion points the
        /// start position to the end position.
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="endPosition"></param>
        /// <returns></returns>
        protected Matrix createQuarternion(Vector3 startPosition, Vector3 endPosition)
        {
            Matrix quart = Matrix.Identity;

            Vector3 diff = endPosition - startPosition;

            // Do x axis rotation
            //
            double hypX = Math.Sqrt(diff.Z * diff.Z + diff.Y * diff.Y);
            quart *= Matrix.CreateFromAxisAngle(Vector3.UnitX, (float)Math.Acos(diff.Z / hypX));

            // Do y axis rotation
            //
            double hypY = Math.Sqrt(diff.Z * diff.Z + diff.X * diff.X);
            quart *= Matrix.CreateFromAxisAngle(Vector3.UnitY, (float)Math.Acos(diff.X / hypY));

            // Do z axis rotation
            //
            double hypZ = Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
            quart *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, (float)Math.Acos(diff.X / hypZ));

            //quart 
            return quart;
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
