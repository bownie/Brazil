using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A Brazil Container accepts a BrazilApp and rescales it ready for drawing inside another
    /// app.   So this class containerises a regular app for injection into Friendlier/Brazil
    /// to run as a sub-app.  The app itself holds its own state model and transitions as well
    /// as components - some components will need to be modified (full screen ones for example)
    /// and everything needs to be scaled to fit into the provided BoundingBox.
    /// </summary>
    [DataContract(Name = "BrazilContainer", Namespace = "http://www.xyglo.com")]
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
        /// Set a reference to this container so that they can be identified as belonging to it.
        /// Additionally we need to scale components according to the BoundingBox and also potentially
        /// convert some components into other types for use as a Container.
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
        [DataMember]
        protected BrazilApp m_app;
    }
}
