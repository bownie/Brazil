using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Store transport information at the Xyglo layer level
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com")]
    public class BrazilTransport
    {
        public BrazilTransport(ViewSpace viewSpace)
        {
            m_viewSpace = viewSpace;
        }

        /// <summary>
        /// Get the current app run time
        /// </summary>
        /// <returns></returns>
        public DateTime getAppTime()
        {
            return new DateTime(m_viewSpace.getAppTime().TotalGameTime.Ticks);
        }

        /// <summary>
        /// XNA won't update local app time itself so we need to use this to push the app time
        /// across occasionally.
        /// </summary>
        /// <param name="dateTime"></param>
        public void setAppTime(DateTime dateTime)
        {
            BrazilGameTime gameTime = new BrazilGameTime();
            gameTime.TotalGameTime = new TimeSpan(dateTime.Ticks);
            //gameTime.ElapsedGameTime 
            m_viewSpace.setLocalAppTime(gameTime);
        }

        /// <summary>
        /// Get the current transport state
        /// </summary>
        /// <returns></returns>
        public BrazilAppTransport getTransportState() { return m_transport; }

        /// <summary>
        /// Start the transport/app
        /// </summary>
        public void start()
        {
            m_lastStartTime = new DateTime(m_viewSpace.getAppTime().TotalGameTime.Ticks);
            m_lastUpdateTime = m_lastStartTime;
            m_transport = BrazilAppTransport.Playing;
        }

        /// <summary>
        /// Stop the transport/app
        /// </summary>
        public void stop()
        {
            m_transport = BrazilAppTransport.Stopped;
            m_lastUpdateTime = new DateTime(m_viewSpace.getAppTime().TotalGameTime.Ticks);
        }

        /// <summary>
        /// Is the app playing?
        /// </summary>
        /// <returns></returns>
        public bool isPlaying()
        {
            return (m_transport == BrazilAppTransport.Playing);
        }

        /// <summary>
        /// Or is it stopped?
        /// </summary>
        /// <returns></returns>
        public bool isStopped()
        {
            return !isPlaying();
        }

        /// <summary>
        /// Current app time
        /// </summary>
        [NonSerialized]
        protected BrazilGameTime m_appTime;

        /// <summary>
        /// DateTime we were last updated
        /// </summary>
        [NonSerialized]
        DateTime m_lastUpdateTime;

        /// <summary>
        /// DateTime we were last started
        /// </summary>
        [NonSerialized]
        DateTime m_lastStartTime;

        /// <summary>
        /// Application transport state - what is the app doing currently
        /// </summary>
        [DataMember]
        protected BrazilAppTransport m_transport = BrazilAppTransport.Stopped;

        /// <summary>
        /// Viewspace handle
        /// </summary>
        [NonSerialized]
        ViewSpace m_viewSpace = null;
    }
}
