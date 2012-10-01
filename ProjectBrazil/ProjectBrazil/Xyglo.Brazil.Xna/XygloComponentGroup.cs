using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// A collection of XygloXnaDrawable objects.  These can have a collective position,
    /// size, spin and mass.  For the moment the order of the list means the render order
    /// also.  It is also a type of XygloXnaDrawable itself so it can be treated just like
    /// one.
    /// </summary>
    public class XygloComponentGroup : XygloXnaDrawable
    {
        /// <summary>
        /// Constructor with the minimum information we need - effect and position
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="position"></param>
        public XygloComponentGroup(BasicEffect effect, Vector3 position)
        {
            m_effect = effect;
            m_position = position;
        }

        /// <summary>
        /// Add a preformatted component to our internal list
        /// </summary>
        /// <param name="newComponent"></param>
        public void addComponent(XygloXnaDrawable newComponent)
        {
            m_componentList.Add(newComponent);
        }

        /// <summary>
        /// Add a component at a relative position to this ComponentGroup
        /// </summary>
        /// <param name="newComponent"></param>
        /// <param name="relativePosition"></param>
        public void addComponentRelative(XygloXnaDrawable newComponent, Vector3 relativePosition)
        {
            // Set position relative to this group and store the component
            newComponent.m_position = m_position + relativePosition;
            m_componentList.Add(newComponent);
        }

        /// <summary>
        /// Build all of our buffers
        /// </summary>
        /// <param name="device"></param>
        public override void buildBuffers(GraphicsDevice device)
        {
            foreach (XygloXnaDrawable component in m_componentList)
            {
                component.buildBuffers(device);
            }
        }

        /// <summary>
        /// Draw everything
        /// </summary>
        /// <param name="device"></param>
        public override void draw(GraphicsDevice device)
        {
            foreach (XygloXnaDrawable component in m_componentList)
            {
                component.draw(device);
            }
        }

        /// <summary>
        /// Trickle down any movement into the components
        /// </summary>
        /// <param name="movement"></param>
        public override void move(Vector3 movement)
        {
            foreach (XygloXnaDrawable component in m_componentList)
            {
                component.move(movement);
            }
        }

        /// <summary>
        /// Set the Velocity
        /// </summary>
        /// <param name="velocity"></param>
        public void setVelocity(Vector3 velocity)
        {
            m_velocity = velocity;
        }
            
        /// <summary>
        /// Velocity of this ComponentGroup
        /// </summary>
        protected Vector3 m_velocity;

        /// <summary>
        /// List of the XygloXnaDrawable components
        /// </summary>
        protected List<XygloXnaDrawable> m_componentList = new List<XygloXnaDrawable>();
    }
}
