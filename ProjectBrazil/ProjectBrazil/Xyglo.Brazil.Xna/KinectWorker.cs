#region File Description
//-----------------------------------------------------------------------------
// KinectWorker.cs
//
// Copyright (C) Xyglo Ltd. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using System.Management;
using Xyglo.Gesture;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// We have a KinecteWorker thread for initialising the (slow to start) Kinect interface.
    /// </summary>
    public class KinectWorker : XygloEventEmitter
    {
        XygloKinectManager m_kinectManager = null;

        /// <summary>
        /// Initialise out Kinect
        /// </summary>
        public void initialise()
        {
#if GOT_KINECT
            // Generate a kinect manager
            //
            m_kinectManager = new XygloKinectManager();

            if (!m_kinectManager.initialise())
            {
                Logger.logMsg("XygloXNA::initialiseProject() - no kinect device found");
            }
#endif

            m_leapListener = new LeapListener();
            m_leapController = new Leap.Controller(m_leapListener);


            m_leapListener.SwipeEvent += new SwipeEventHandler(handleQueueEvent);
            m_leapListener.ScreenTapEvent += new ScreenTapEventHandler(handleQueueEvent);
            m_leapListener.ScreenPositionEvent += new ScreenPositionEventHandler(handleQueueEvent);
        }

        /// <summary>
        /// Generic event handler for our local events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void handleQueueEvent(object sender, System.EventArgs args)
        {
            m_eventQueueMutex.WaitOne();
            m_eventArgs.Add(args);
            m_eventQueueMutex.ReleaseMutex();
        }


        /// <summary>
        /// Get the event queue
        /// </summary>
        /// <returns></returns>
        public System.EventArgs getNextEvent()
        {
            m_eventQueueMutex.WaitOne();

            if (m_eventArgs.Count == 0)
            {
                m_eventQueueMutex.ReleaseMutex();
                return null;
            }

            System.EventArgs rE = m_eventArgs[0];
            m_eventArgs.RemoveAt(0);
            m_eventQueueMutex.ReleaseMutex();

            return rE;
        }

        /// <summary>
        /// Get all the event args that are queued and clear down
        /// </summary>
        /// <returns></returns>
        public System.EventArgs [] getAllEvents()
        {
            m_eventQueueMutex.WaitOne();
            System.EventArgs []rL = new System.EventArgs[m_eventArgs.Count];
            m_eventArgs.CopyTo(rL);
            m_eventArgs.Clear();
            m_eventQueueMutex.ReleaseMutex();
            return rL;
        }

        /// <summary>
        /// This method is called when the thread is started
        /// </summary>
        public void startWorking()
        {
            if (m_kinectManager == null)
            {
                Logger.logMsg("KinectWorker::startWorking() - initialising the worker thread for kinect");
                initialise();
            }

            while (!m_shouldStop)
            {
                processDepthInformation();
                Thread.Sleep(50); // sleep for 10ms
            }

#if GOT_KINECT
            m_kinectManager.close();
#endif

            // Remove the listener
            //
            m_leapController.RemoveListener(m_leapListener);

            Console.WriteLine("KinectWorker::startWorking() - terminating gracefully");
        }

        
        /// <summary>
        /// Depth position memory
        /// </summary>
        public float m_lastDepthPosition = 0;

        /// <summary>
        /// Initial depth position
        /// </summary>
        public float m_initialDepthPosition = 0;

        /// <summary>
        /// Process depth information from the Kinect
        /// </summary>
        protected void processDepthInformation()
        {
#if GOT_KINECT
            if (m_kinectManager.getDepthValue() != 0)
            {
                if (m_initialDepthPosition == 0)
                {
                    m_initialDepthPosition = m_kinectManager.getDepthValue();
                }

                /*
                if (gameTime.TotalGameTime.Milliseconds % 250 == 0)
                {
                    if (m_kinectManager.depthIsStable(m_lastDepthPosition))
                    {
                        //Logger.logMsg("DEPTH STABLE @ " + m_kinectManager.getDepthValue());
                        ;
                    }
                    else
                    {
                        //m_lastDepthPosition = m_kinectManager.getDepthValue();
                        //Logger.logMsg("LAST = " + m_lastDepthPosition);
                        //Logger.logMsg("NEW  = " + m_kinectManager.getDepthValue());


                        // Move to new position
                        //Vector3 newPosition = m_eye;
                        //newPosition.Z += (m_kinectManager.getDepthValue() - m_initialDepthPosition) / 100.0f;
                        //flyToPosition(newPosition);
                    }

                    m_lastDepthPosition = m_kinectManager.getDepthValue();

                }
                 * */
            }
#endif // GOT_KINECT
        }

        /// <summary>
        /// Stop this thread
        /// </summary>
        public void requestStop()
        {
            m_shouldStop = true;
        }

        /// <summary>
        /// Volatile is used as hint to the compiler that this data member will be accessed by multiple threads.
        /// </summary>
        private volatile bool m_shouldStop;

        /// <summary>
        /// Xyglo Leap listener
        /// </summary>
        protected LeapListener m_leapListener;

        /// <summary>
        /// Leap controller
        /// </summary>
        protected Leap.Controller m_leapController;

        /// <summary>
        /// List of event args that we've received on this thread
        /// </summary>
        protected List<System.EventArgs> m_eventArgs = new List<System.EventArgs>();

        /// <summary>
        /// Control access to the m_eventArgs list using this mutex
        /// </summary>
        public Mutex m_eventQueueMutex = new Mutex();
    }
}
