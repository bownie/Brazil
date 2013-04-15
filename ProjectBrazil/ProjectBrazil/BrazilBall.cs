using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A nice big ball
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com")]
    public class BrazilBall : Component3D
    {
        public BrazilBall(BrazilColour colour, BrazilVector3 position, float size, bool affectedByGravity = true)
        {
            m_colour = colour;
            m_position = position;
            m_mass = 1.0f;
            m_radius = size;
            m_gravityAffected = affectedByGravity;
        }

        public float getRadius() { return m_radius; }

        [DataMember]
        protected float m_radius;
    }
}
