using System;
using System.Collections.Generic;
using System.Text;


namespace Xyglo.Brazil
{
    /// <summary>
    /// The type of our Goody
    /// </summary>
    public enum BrazilGoodyType
    {
        Coin,
        Egg,
        Box
    }

    /// <summary>
    /// A prize that can be consumed by our Interloper.  A goody has position, a birth time, a worth and 
    /// can potentially automatically reset after being eaten.
    /// </summary>
    public class BrazilGoody : Component3D
    {
        public BrazilGoody(BrazilGoodyType type, int worth, BrazilVector3 position, BrazilVector3 size, DateTime birthTime)
        {
            m_colour = BrazilColour.White; // probably not used so we default it
            m_position = position;
            m_dimensions = size;
            m_birthTime = birthTime;
            m_worth = worth;
            m_type = type; // default type

            // Is affected by gravity - set mass and hardness
            //
            m_gravityAffected = true;
            m_mass = 10;
            m_hardness = 0.3f;
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
        /// When is this Goody born?
        /// </summary>
        public DateTime m_birthTime { get; set; } 

        /// <summary>
        /// Dimensions of this Goody
        /// </summary>
        protected BrazilVector3 m_dimensions;

        /// <summary>
        /// Does this BrazilGoody reset after being eaten?
        /// </summary>
        public bool m_reset { get; set; }

        /// <summary>
        /// How many points is this Goody worth?
        /// </summary>
        public int m_worth { get; set; }

        /// <summary>
        /// Type of our Goody determines shape and colouration
        /// </summary>
        public BrazilGoodyType m_type { get; set; }
    }
}
