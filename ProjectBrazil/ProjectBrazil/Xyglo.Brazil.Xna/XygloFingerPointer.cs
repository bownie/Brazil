using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// A wrapper class to define a finger pointer object
    /// </summary>
    public class XygloFingerPointer : XygloSphere
    {
        public XygloFingerPointer(int fingerId, BasicEffect effect, Vector3 position)  : base(Color.Pink, effect, position, 10.0f)
        {
            m_fingerId = fingerId;
        }

        /// <summary>
        /// Id of this finger pointer
        /// </summary>
        protected int m_fingerId;

    }
}
