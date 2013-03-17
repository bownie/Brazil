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

    /// <summary>
    /// Arguments for a Screen Tap
    /// </summary>
    public class ScreenTapEventArgs : System.EventArgs
    {
        public ScreenTapEventArgs(ScreenTapGesture screenTap)
        {
            m_screenTap = screenTap;
        }

        protected ScreenTapGesture m_screenTap;
    }

    
    /// <summary>
    /// Screen position with the a pointer
    /// </summary>
    public class ScreenPositionEventArgs : System.EventArgs
    {
        public ScreenPositionEventArgs(Vector screenPosition, int id)
        {
            m_position = new Vector3(screenPosition.x, screenPosition.y, screenPosition.z);
            m_id = id;
        }

        public float X() { return m_position.X; }
        public float Y() { return m_position.Y; }
        public Vector3 getPosition() { return m_position; }
        public int getId() { return m_id; }

        protected int m_id;
        protected Vector3 m_position;
    }

    public delegate void SwipeEventHandler(object sender, SwipeEventArgs e);
    public delegate void ScreenTapEventHandler(object sender, ScreenTapEventArgs e);
    public delegate void ScreenPositionEventHandler(object sender, ScreenPositionEventArgs e);

}
