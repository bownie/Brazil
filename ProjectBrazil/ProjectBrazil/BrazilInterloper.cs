using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{
    /// <summary>
    /// The Interloper is our hero
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com")]
    [KnownType(typeof(BrazilInterloper))]
    public class BrazilInterloper : Component3D
    {
        public BrazilInterloper(BrazilColour colour, BrazilVector3 position, BrazilVector3 size)
        {
            m_colour = colour;
            m_position = position;
            m_dimensions = size;

            // Is affected by gravity - set mass and hardness
            //
            m_gravityAffected = true;
            m_mass = 10;
            m_hardness = 6f;
        }

        /// <summary>
        /// Get the size of this interloper
        /// </summary>
        /// <returns></returns>
        public BrazilVector3 getSize()
        {
            return m_dimensions;
        }

        /// <summary>
        /// Get the score
        /// </summary>
        /// <returns></returns>
        public int getScore()
        {
            return m_score;
        }

        /// <summary>
        /// Add to score
        /// </summary>
        /// <param name="score"></param>
        public void incrementScore(int score)
        {
            m_score += score;
        }

        /// <summary>
        /// Dimensions of this Interloper
        /// </summary>
        protected BrazilVector3 m_dimensions;

        /// <summary>
        /// Interloper score
        /// </summary>
        protected int m_score;

    }
}
