using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A Brazil Container accepts a BrazilApp and rescales it ready for drawing inside another
    /// app.   So this class containerises a regular app for injection.
    /// </summary>
    public class BrazilContainer : Component
    {
        /// <summary>
        /// Construct with a BrazilApp and constrain within a BrazilBoundingBox
        /// </summary>
        /// <param name="app"></param>
        /// <param name="boundingBox"></param>
        public BrazilContainer(BrazilApp app, BrazilBoundingBox boundingBox)
        {
            m_targetBoundingBox = boundingBox;
            m_app = app;

            // Apply this container to all the components
            applyContainerToContents();
        }

        /// <summary>
        /// Set a reference to this container 
        /// </summary>
        public void applyContainerToContents()
        {
            foreach(Component component in m_app.getComponents())
            {
                component.setContainer(this);
            }
        }

        /// <summary>
        /// Get the App
        /// </summary>
        /// <returns></returns>
        public BrazilApp getApp()
        {
            return m_app;
        }


        /// <summary>
        /// Set the BoundingBox for this container
        /// </summary>
        /// <param name="boundingBox"></param>
        public void setBoundingBox(BrazilBoundingBox boundingBox)
        {
            m_targetBoundingBox = boundingBox;
        }

        /// <summary>
        /// Get the BoundingBox for this container
        /// </summary>
        /// <returns></returns>
        public BrazilBoundingBox getBoundingBox()
        {
            return m_targetBoundingBox;
        }

        /// <summary>
        /// Define the BoundingBox for this Container - the limits of the BrazilApp
        /// </summary>
        protected BrazilBoundingBox m_targetBoundingBox;

        /// <summary>
        /// Our app
        /// </summary>
        protected BrazilApp m_app;
    }
}
