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

        /// <summary>
        /// Speed of the swipe
        /// </summary>
        /// <returns></returns>
        public float getSpeed()
        {
            return m_swipe.Speed;
        }

        /// <summary>
        /// Start position of the swipe
        /// </summary>
        /// <returns></returns>
        public Vector3 getStartPosition()
        {
            return new Vector3(m_swipe.StartPosition.x, m_swipe.StartPosition.y, m_swipe.StartPosition.z);
        }

        public Vector3 getEndPosition()
        {
            return new Vector3(m_swipe.Position.x, m_swipe.Position.y, m_swipe.Position.z);
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

    public class CircleEventArgs : System.EventArgs
    {
        public CircleEventArgs(Vector screenPosition, float duration, bool clockwise)
        {
            m_clockwise = clockwise;
            m_screenPosition = new Vector3(screenPosition.x, screenPosition.y, screenPosition.z);
            m_duration = duration;
        }

        public bool isClockwise() { return m_clockwise; }
        public Vector3 getPosition() { return m_screenPosition; }
        public float getDuration() { return m_duration; }

        protected Vector3 m_screenPosition;
        protected bool m_clockwise;
        protected float m_duration;
    }


    public delegate void SwipeEventHandler(object sender, SwipeEventArgs e);
    public delegate void ScreenTapEventHandler(object sender, ScreenTapEventArgs e);
    public delegate void ScreenPositionEventHandler(object sender, ScreenPositionEventArgs e);
    public delegate void CircleEventHandler(object sender, CircleEventArgs e);
}
