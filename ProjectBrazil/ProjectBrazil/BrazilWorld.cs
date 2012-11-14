using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{

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
        /// Bounding area for our world
        /// </summary>
        protected BrazilBoundingBox m_bounds;

        /// <summary>
        /// Which way does gravity point?  This is our gravtiation acceleration
        /// in directional units per time unit.
        /// </summary>
        protected BrazilVector3 m_gravity;

        /// <summary>
        /// How many lives do people have in this world?
        /// </summary>
        protected int m_lives = 3;
    }
}