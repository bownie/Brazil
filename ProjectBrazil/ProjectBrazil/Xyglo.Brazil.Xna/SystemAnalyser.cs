using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Provide a thread for the analysis of the system - performance counters, file counters
    /// and other stuff that takes some time.
    /// </summary>
    public class SystemAnalyser : IDisposable
    {
        public SystemAnalyser(XygloContext context)
        {
            // Start up the worker thread for the performance counters
            //
            m_counterWorker = new PerformanceWorker();
            m_counterWorkerThread = new Thread(m_counterWorker.startWorking);
            m_counterWorkerThread.Start();

            // Loop until worker thread activates.
            //
            while (!m_counterWorkerThread.IsAlive);
            Thread.Sleep(1);

            // Set physical memory
            //
            Microsoft.VisualBasic.Devices.ComputerInfo ci = new Microsoft.VisualBasic.Devices.ComputerInfo();
            m_physicalMemory = (float)(ci.TotalPhysicalMemory / (1024 * 1024));
        }


        /// <summary>
        /// Fetch stats can be called as often as you like but we'll only update at certain times
        /// </summary>
        /// <param name="mySpan"></param>
        public void fetchStats(TimeSpan mySpan)
        {
              if (mySpan - m_lastSystemFetch > m_systemFetchSpan)
            {
                if (m_counterWorker.m_cpuCounter != null && m_counterWorker.m_memCounter != null)
                {
                    CounterSample newCS = m_counterWorker.getCpuSample();
                    CounterSample newMem = m_counterWorker.getMemorySample();

                    // Calculate the percentages
                    //
                    m_systemLoad = CounterSample.Calculate(m_lastCPUSample, newCS);
                    m_memoryAvailable = CounterSample.Calculate(m_lastMemSample, newMem);

                    // Store the last samples
                    //
                    m_lastCPUSample = newCS;
                    m_lastMemSample = newMem;
                }

                m_lastSystemFetch = mySpan;

#if SYTEM_DEBUG
                Logger.logMsg("XygloXNA::drawSystemLoad() - load is now " + m_systemLoad);
                Logger.logMsg("XygloXNA::drawSystemLoad() - memory is now " + m_memoryAvailable);
                Logger.logMsg("XygloXNA::drawSystemLoad() - physical memory available is " + m_physicalMemory);
#endif
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            stop();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose all managed objects
                stop();
            }

            // Release unmanaged resources
        }

        /// <summary>
        /// Stop everything and tidy up - link to dispose
        /// </summary>
        public void stop()
        {
            if (m_counterWorker != null)
            {
                // Clear the worker thread and exit
                //
                m_counterWorker.requestStop();
                m_counterWorkerThread.Join();
                m_counterWorker = null;
            }
        }

        /// <summary>
        /// Are we analysing?
        /// </summary>
        /// <returns></returns>
        public bool isWorking() { return (m_counterWorker != null); }

        /// <summary>
        /// Get Physical Memory
        /// </summary>
        /// <returns></returns>
        public float getPhysicalMemory() { return m_physicalMemory; }

        /// <summary>
        /// Get System Load
        /// </summary>
        /// <returns></returns>
        public float getSystemLoad() { return m_systemLoad; }

        /// <summary>
        /// Get Available Memory
        /// </summary>
        /// <returns></returns>
        public float getAvailableMemory() { return m_memoryAvailable; }

        /// <summary>
        /// Store the last performance counter for CPU
        /// </summary>
        protected CounterSample m_lastCPUSample;

        /// <summary>
        /// Store the last performance counter for CPU
        /// </summary>
        protected CounterSample m_lastMemSample;

        /// <summary>
        /// Number of milliseconds between system status fetches
        /// </summary>
        protected TimeSpan m_systemFetchSpan = new TimeSpan(0, 0, 0, 1, 0);

        /// <summary>
        /// When we last fetched the system status
        /// </summary>
        protected TimeSpan m_lastSystemFetch = new TimeSpan(0, 0, 0, 0, 0);

        /// <summary>
        /// Percentage of system load
        /// </summary>
        protected float m_systemLoad = 0.0f;

        /// <summary>
        /// Percentage of system load
        /// </summary>
        protected float m_memoryAvailable = 0.0f;

        /// <summary>
        /// Physical Memory 
        /// </summary>
        protected float m_physicalMemory;

        /// <summary>
        /// Worker thread for the PerformanceCounters
        /// </summary>
        protected PerformanceWorker m_counterWorker;

        /// <summary>
        /// The thread that is used for the counter
        /// </summary>
        protected Thread m_counterWorkerThread;

        /// <summary>
        /// Directory previewer is run in this thread as well
        /// </summary>
        protected DirectoryPreview m_directoryPreview;

        /// <summary>
        /// Xyglo context - be careful with threading!
        /// </summary>
        protected XygloContext m_context;
    }
}
