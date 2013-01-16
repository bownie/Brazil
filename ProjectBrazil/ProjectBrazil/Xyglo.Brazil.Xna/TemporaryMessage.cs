using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Handle temporary messages
    /// </summary>
    public class TemporaryMessage : XygloEventEmitter
    {
        public TemporaryMessage(XygloContext context)
        {
            m_context = context;
        }

        /// <summary>
        /// Emits a welcome message depending on the licencing situation
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="licenced"></param>
        /// <returns></returns>
        public void sendWelcomeMessage(GameTime gameTime, bool licenced)
        {
            if (licenced)
            {
                // Set the welcome message once
                //
                if (m_flipFlop)
                {
                    if (m_context.m_project.getInitialMessage() != "")
                        OnTemporaryMessage(new TextEventArgs(m_context.m_project.getInitialMessage(), 5, gameTime));
                    else
                        OnTemporaryMessage(new TextEventArgs(VersionInformation.getProductName() + " " + VersionInformation.getProductVersion(), 3, gameTime));

                    // Turn this message off for the run
                    //
                    m_flipFlop = false;
                }
            }
            else
            {
                if (gameTime.TotalGameTime.TotalSeconds > m_nextLicenceMessage)
                {
                    // Emit message depending on status
                    if (m_flipFlop)
                        OnTemporaryMessage(new TextEventArgs("Friendlier demo period has expired.", 3, gameTime));
                    else
                        OnTemporaryMessage(new TextEventArgs("Please see www.xyglo.com for licencing details.", 3, gameTime));

                    m_flipFlop = !m_flipFlop;
                    m_nextLicenceMessage = gameTime.TotalGameTime.TotalSeconds + 5;
                }
            }
        }

        /// <summary>
        /// Set a temporary message until a given end time (seconds into the future)
        /// </summary>
        /// <param name="message"></param>
        /// <param name="gameTime"></param>
        public void setTemporaryMessage(string message, double seconds, GameTime gameTime = null)
        {
            // Recover this if not set
            if (gameTime == null)
                gameTime = m_context.m_gameTime;


            m_temporaryMessage = message;

            if (seconds == 0)
            {
                seconds = 604800; // a week should be long enough to signal infinity
            }

            // Store the start and end time for this message - start time is used for
            // scrolling.
            //
            m_temporaryMessageStartTime = (gameTime != null ? gameTime.TotalGameTime.TotalSeconds : 0);
            m_temporaryMessageEndTime = (gameTime != null ? m_temporaryMessageStartTime : 0) + seconds;
        }

        public string getTemporaryMessage() { return m_temporaryMessage; }
        public double getTemporaryMessageStartTime() { return m_temporaryMessageStartTime; }
        public double getTemporaryMessageEndTime() { return m_temporaryMessageEndTime; }

        /// <summary>
        /// The XygloContext
        /// </summary>
        protected XygloContext m_context;

        /// <summary>
        /// We can use this to communicate something to the user about the last command
        /// </summary>
        protected string m_temporaryMessage = "";

        /// <summary>
        /// Start time for the temporary message
        /// </summary>
        protected double m_temporaryMessageStartTime;

        /// <summary>
        /// End time for the temporary message
        /// </summary>
        protected double m_temporaryMessageEndTime;

        /// <summary>
        /// Used when displaying licence messages
        /// </summary>
        protected bool m_flipFlop = true;

        /// <summary>
        /// Something to do with licence messages
        /// </summary>
        protected double m_nextLicenceMessage = 0.0f;
    }
}
