using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;
using Microsoft.Xna.Framework;

namespace Xyglo.Gesture
{
    /// <summary>
    /// SwipeEvent wrapper to a Leap Swipe Gesture
    /// </summary>
    public class SwipeEventArgs : System.EventArgs
    {
        public SwipeEventArgs(SwipeGesture swipe)
        {
            m_swipe = swipe;
        }

        /// <summary>
        /// Return the swipe direction in an XNA friendly format
        /// </summary>
        /// <returns></returns>
        public Vector3 getDirection()
        {
            return new Vector3(m_swipe.Direction.x, m_swipe.Direction.y, m_swipe.Direction.z);
        }

        public float getSpeed()
        {
            return m_swipe.Speed;
        }

        public SwipeGesture getSwipe() { return m_swipe; }

        protected SwipeGesture m_swipe;
    }

    public delegate void SwipeEventHandler(object sender, SwipeEventArgs e);

}
