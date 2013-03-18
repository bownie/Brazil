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
    /// Indicate screen position with a screen pointer - also carry finger position information tip and root
    /// </summary>
    public class ScreenPositionEventArgs : System.EventArgs
    {
        public ScreenPositionEventArgs(Vector screenPosition, Vector startPosition, Vector endPosition, string indexHand, int id)
        {
            m_screenPosition = new Vector3(screenPosition.x, screenPosition.y, screenPosition.z);
            m_fingerStartPosition = new Vector3(startPosition.x, startPosition.y, startPosition.z);
            m_fingerEndPosition = new Vector3(endPosition.x, endPosition.y, endPosition.z);
            m_id = id;
            m_handIndex = indexHand;
        }

        public float X() { return m_screenPosition.X; }
        public float Y() { return m_screenPosition.Y; }
        public Vector3 getScreenPosition() { return m_screenPosition; }
        public int getId() { return m_id; }
        public string getHandIndex() { return m_handIndex; }
        public Vector3 getFingerStartPosition() { return m_fingerStartPosition; }
        public Vector3 getFingerEndPosition() { return m_fingerEndPosition; }
        protected int m_id;

        protected Vector3 m_screenPosition;

        protected Vector3 m_fingerStartPosition;
        protected Vector3 m_fingerEndPosition;

        /// <summary>
        /// Keep a track of which hand this finger might be from
        /// </summary>
        protected string m_handIndex;
    }

    public delegate void SwipeEventHandler(object sender, SwipeEventArgs e);
    public delegate void ScreenTapEventHandler(object sender, ScreenTapEventArgs e);
    public delegate void ScreenPositionEventHandler(object sender, ScreenPositionEventArgs e);

}
