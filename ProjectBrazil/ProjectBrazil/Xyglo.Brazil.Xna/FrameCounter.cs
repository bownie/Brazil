using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Helper class to count some frames
    /// </summary>
    public class FrameCounter
    {
        public FrameCounter()
        {
        }

        public TimeSpan getElapsedTime()
        {
            return m_elapsedTime;
        }

        public void setElapsedTime(TimeSpan timeSpan)
        {
            m_elapsedTime = timeSpan;
        }

        public void incrementElapsedTime(TimeSpan timeSpan)
        {
            m_elapsedTime += timeSpan;
        }

        /// <summary>
        /// Set the frame rate and reset counters
        /// </summary>
        public void setFrameRate()
        {
            m_frameRate = m_frameCounter;
            m_frameCounter = 0;
            m_elapsedTime -= TimeSpan.FromSeconds(1);
        }

        public int getFrameRate() { return m_frameRate; }
        public void incrementFrameCounter() { m_frameCounter++; }

        /// <summary>
        /// Frame rate
        /// </summary>
        protected int m_frameRate = 0;

        /// <summary>
        /// Frame counter
        /// </summary>
        protected int m_frameCounter = 0;

        /// <summary>
        /// Elapse time to help calculate frame rate
        /// </summary>
        TimeSpan m_elapsedTime = TimeSpan.Zero;
    }
}
