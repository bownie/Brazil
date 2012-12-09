using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Class that takes an eye position and perturbs it in circular or oval motion around
    /// the initial point.
    /// </summary>
    public class EyePerturber
    {
        public EyePerturber(BrazilVector3 initialEyePosition, float majorAxis, float minorAxis, double initialPeriod, double startTime)
        {
            m_initialEyePosition = initialEyePosition;
            m_startTime = startTime;
            m_initialPeriod = initialPeriod;
            m_majorAxis = majorAxis;
            m_minorAxis = minorAxis;
        }

        public BrazilVector3 getInitialPosition()
        {
            return m_initialEyePosition;
        }

        /// <summary>
        /// Get the position perturbed over time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public BrazilVector3 getPerturbedPosition(double time)
        {
            BrazilVector3 rV = m_initialEyePosition;

            double positionNow = time % m_initialPeriod;
            double angle = positionNow / m_initialPeriod * 2 * Math.PI;

            // Rotate
            //
            rV.X += (float)(m_majorAxis * Math.Cos(angle));
            rV.Y += (float)(m_minorAxis * Math.Sin(angle));

            return rV;
        }

        /// <summary>
        /// When this perturber started
        /// </summary>
        protected double m_startTime;

        /// <summary>
        /// Size of the minor axis
        /// </summary>
        protected float m_minorAxis;

        /// <summary>
        /// Size of the major axis
        /// </summary>
        protected float m_majorAxis;

        /// <summary>
        /// Initial period of the rotation
        /// </summary>
        protected double m_initialPeriod;

        /// <summary>
        /// Initial eye position
        /// </summary>
        protected BrazilVector3 m_initialEyePosition;

    }
}
