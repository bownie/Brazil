using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{

    /// <summary>
    /// A BrazilWorld defines an environment for the Brazil Components to live.
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com")]
    public class BrazilWorld
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BrazilWorld()
        {
            m_gravity = BrazilVector3.Zero;
            m_bounds = new BrazilBoundingBox(BrazilVector3.Zero, BrazilVector3.Zero);
        }

        /// <summary>
        /// Set bounds
        /// </summary>
        /// <param name="bb"></param>
        public void setBounds(BrazilBoundingBox bb)
        {
            m_bounds = bb;
        }

        /// <summary>
        /// Get bounds
        /// </summary>
        /// <returns></returns>
        public BrazilBoundingBox getBounds()
        {
            return m_bounds;
        }

        /// <summary>
        /// Get gravity
        /// </summary>
        /// <returns></returns>
        public BrazilVector3 getGravity()
        {
            return m_gravity;
        }

        /// <summary>
        /// Set gravity
        /// </summary>
        /// <param name="gravity"></param>
        public void setGravity(BrazilVector3 gravity)
        {
            m_gravity = gravity;
        }

        /// <summary>
        /// Set lives
        /// </summary>
        /// <param name="lives"></param>
        public void setLives(int lives)
        {
            m_lives = lives;
        }

        /// <summary>
        /// Get lives
        /// </summary>
        /// <returns></returns>
        public int getLives()
        {
            return m_lives;
        }

        /// <summary>
        /// Get repeat interval (seconds)
        /// </summary>
        /// <returns></returns>
        public double getKeyAutoRepeatInterval()
        {
            return m_repeatInterval;
        }

        /// <summary>
        /// Time before auto repeating of keys happens (seconds)
        /// </summary>
        /// <returns></returns>
        public double getKeyAutoRepeatHoldTime()
        {
            return m_repeatHoldTime;
        }

        /// <summary>
        /// Set autorepeat interval
        /// </summary>
        /// <param name="time"></param>
        public void setKeyAutoRepeatInterval(double time)
        {
            m_repeatInterval = time;
        }

        /// <summary>
        /// Set AutoRepeat key hold time
        /// </summary>
        /// <param name="time"></param>
        public void setKeyAutoRepeatHoldTime(double time)
        {
            m_repeatHoldTime = time;
        }

        /// <summary>
        /// Scale of the world
        /// </summary>
        /// <returns></returns>
        public float getWorldScale() { return m_worldScale; }

        /// <summary>
        /// Set the world scaling factor according to the size of box we're going to view it in
        /// </summary>
        /// <param name="viewBox"></param>
        public void setWorldScale(BrazilBoundingBox viewBox)
        {
            // Scale
            double xMult = m_bounds.getWidth() / viewBox.getWidth();
            double yMult = m_bounds.getHeight() / viewBox.getHeight();
            m_worldScale = Math.Min((float)xMult, (float)yMult);
        }

        /// <summary>
        /// Bounding area for our world
        /// </summary>
        [DataMember]
        protected BrazilBoundingBox m_bounds;

        /// <summary>
        /// Which way does gravity point?  This is our gravtiation acceleration
        /// in directional units per time unit.
        /// </summary>
        [DataMember]
        protected BrazilVector3 m_gravity;

        /// <summary>
        /// How many lives do people have in this world?
        /// </summary>
        [DataMember]
        protected int m_lives = 3;

        /// <summary>
        /// Key repeat start time in seconds (defaults to zero for games)
        /// </summary>
        [DataMember]
        protected double m_repeatHoldTime = 0.0f;

        /// <summary>
        /// Time between auto-key repeats
        /// </summary>
        [DataMember]
        protected double m_repeatInterval = 0.05f;

        /// <summary>
        /// If we need to scale the world then we can use this handy actor
        /// </summary>
        /// 
        [DataMember]
        protected float m_worldScale = 1.0f;
    }
}