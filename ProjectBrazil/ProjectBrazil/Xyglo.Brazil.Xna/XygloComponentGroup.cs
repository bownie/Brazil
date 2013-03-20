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
    /// A type that defines the look and behaviour.   We might want to generalise these types a little more
    /// later on as at the moment we already have the Component type from the Brazil side.
    /// </summary>
    public enum XygloComponentGroupType
    {
        Interloper,
        Fiend
    };

    /// <summary>
    /// A collection of XygloXnaDrawable objects.  These can have a collective position,
    /// size, spin and mass.  For the moment the order of the list means the render order
    /// also.  It is also a type of XygloXnaDrawable itself so it can be treated just like
    /// one.
    /// </summary>
    public class XygloComponentGroup : XygloXnaDrawableShape
    {
        /// <summary>
        /// Constructor with the minimum information we need - effect and position
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="position"></param>
        public XygloComponentGroup(XygloComponentGroupType type, BasicEffect effect, Vector3 position)
        {
            m_type = type;
            m_effect = effect;
            m_position = position;
        }

        /// <summary>
        /// Add a preformatted component to our internal list
        /// </summary>
        /// <param name="newComponent"></param>
        public void addComponent(XygloXnaDrawable newComponent)
        {
            newComponent.setParent(this);
            m_componentList.Add(newComponent);
        }

        /// <summary>
        /// Add a component at a relative position to this ComponentGroup
        /// </summary>
        /// <param name="newComponent"></param>
        /// <param name="relativePosition"></param>
        public void addComponentRelative(XygloXnaDrawable newComponent, Vector3 relativePosition)
        {
            // Set the parent
            //
            newComponent.setParent(this);

            // Set position relative to this group and store the component
            newComponent.setPosition(m_position + relativePosition);
            m_componentList.Add(newComponent);

            // Store the position we've used for this relative add - it's assumed that
            // this will be the relative centre from which all componets are measured
            // when doing an absolute move of this componentgroup later on.
            //
            m_groupCentrePosition = m_position;
        }

        /// <summary>
        /// Build all of our buffers
        /// </summary>
        /// <param name="device"></param>
        public override void buildBuffers(GraphicsDevice device)
        {
            foreach (XygloXnaDrawableShape component in m_componentList)
                component.buildBuffers(device);
        }

        /// <summary>
        /// Draw everything
        /// </summary>
        /// <param name="device"></param>
        public override void draw(GraphicsDevice device)
        {
            foreach (XygloXnaDrawableShape component in m_componentList)
            {
                component.draw(device);
            }
        }

        /// <summary>
        /// Draw preview
        /// </summary>
        /// <param name="device"></param>
        /// <param name="boundingBox"></param>
        public override void drawPreview(GraphicsDevice device, BoundingBox fullBoundingBox, BoundingBox previewBoundingBox, Texture2D texture)
        {
            foreach (XygloXnaDrawableShape component in m_componentList)
            {
                component.drawPreview(device, fullBoundingBox, previewBoundingBox, texture);
            }
        }

        /// <summary>
        /// Set the position of this group - keeping relative placements..
        /// </summary>
        /// <param name="position"></param>
        public override void setPosition(Vector3 position)
        {
            Vector3 offset = m_groupCentrePosition - position;

            foreach (XygloXnaDrawable component in m_componentList)
            {
                component.move(offset);
            }
            m_position = position;
        }

        /// <summary>
        /// Change velocity of group
        /// </summary>
        /// <param name="movement"></param>
        public override void setVelocity(Vector3 velocity)
        {
            foreach (XygloXnaDrawable component in m_componentList)
            {
                component.setVelocity(velocity);
            }

            // And set the velocity of this group
            //
            m_velocity = velocity;
            m_velocity = XygloConvert.roundVector(m_velocity);
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

            // And move ourself
            m_position += movement;
            m_groupCentrePosition += movement;

            // Round
            m_position = XygloConvert.roundVector(m_position);
        }

        /// <summary>
        /// Override the getBoundingBox call
        /// </summary>
        /// <returns></returns>
        public override BoundingBox getBoundingBox()
        {
            
            if (m_componentList.Count == 0)
                return new BoundingBox(Vector3.Zero, Vector3.Zero);

            Vector3 min = m_componentList[0].getBoundingBox().Min, max = m_componentList[0].getBoundingBox().Max;

            foreach (XygloXnaDrawable component in m_componentList)
            {
                if (component.getBoundingBox().Min.X < min.X ) min.X = component.getBoundingBox().Min.X;
                if (component.getBoundingBox().Min.Y < min.Y) min.Y = component.getBoundingBox().Min.Y;
                if (component.getBoundingBox().Min.Z < min.Z) min.Y = component.getBoundingBox().Min.Z;

                if (component.getBoundingBox().Max.X > max.X) max.X = component.getBoundingBox().Max.X;
                if (component.getBoundingBox().Max.Y > max.Y) max.Y = component.getBoundingBox().Max.Y;
                if (component.getBoundingBox().Max.Z > max.Z) max.Z = component.getBoundingBox().Max.Z;
                
            //component.move(movement);
            }

            return new BoundingBox(min, max);
        }

        /// <summary>
        /// Trickle down the moveDefault into sub components
        /// </summary>
        /// <param name="movement"></param>
        public override void moveDefault()
        {
            foreach (XygloXnaDrawable component in m_componentList)
            {
                component.move(m_velocity);
            }

            m_position += m_velocity;
            m_groupCentrePosition += m_velocity;

            m_position = XygloConvert.roundVector(m_position);
        }

        /// <summary>
        /// What we do to jump - add some impetus in the non gravitational direction
        /// </summary>
        public override void jump(Vector3 impulse)
        {
            foreach (XygloXnaDrawable component in m_componentList)
            {
                component.jump(impulse);
            }
            m_velocity += impulse;
        }

        /// <summary>
        /// Little cheat to update our position element following a physics update
        /// </summary>
        public void updatePositionAfterPhysics()
        {
            Vector3 averageVector = Vector3.Zero;
            foreach(XygloXnaDrawable drawable in m_componentList)
                averageVector += drawable.getPosition();
            m_position = averageVector / m_componentList.Count();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accelerationVector"></param>
        public override void accelerate(Vector3 accelerationVector)
        {
            //m_velocity += accelerationVector;
            if (m_maxVelocity.X == 0) // no limit on X
            {
                m_velocity.X += accelerationVector.X;
            }
            else if (accelerationVector.X > 0)
            {
                if (m_velocity.X < m_maxVelocity.X)
                {
                    // Incremement with a maxiumum
                    m_velocity.X = Math.Min(m_velocity.X + accelerationVector.X, m_maxVelocity.X);
                }
            }
            else if (accelerationVector.X < 0)
            {
                if (m_velocity.X > -m_maxVelocity.X)
                {
                    m_velocity.X = Math.Max(m_velocity.X + accelerationVector.X, -m_maxVelocity.X);
                }
            }

            m_velocity.Y += accelerationVector.Y;
            m_velocity.Z += accelerationVector.Z;

            // Round our interlopers velocity
            //
            m_velocity = XygloConvert.roundVector(m_velocity);
        }

        /// <summary>
        /// Trickle
        /// </summary>
        /// <param name="x"></param>
        public override void moveLeft(float x)
        {
            foreach (XygloXnaDrawable component in m_componentList)
            {
                component.moveLeft(x);
            }

            m_position.X -= x;
            m_groupCentrePosition.X -= x;

            m_position = XygloConvert.roundVector(m_position);
        }

        /// <summary>
        /// Trickle
        /// </summary>
        /// <param name="x"></param>
        public override void moveRight(float x)
        {
            foreach (XygloXnaDrawable component in m_componentList)
            {
                component.moveRight(x);
            }

            m_position.X += x;
            m_groupCentrePosition.X += x;

            m_position = XygloConvert.roundVector(m_position);
        }

        /// <summary>
        /// Polygons in this item - see this:
        /// 
        /// http://xboxforums.create.msdn.com/forums/p/23549/126997.aspx
        /// </summary>
        /// <returns></returns>
        public override int getPolygonCount()
        {
            int count = 0;

            foreach (XygloXnaDrawable component in m_componentList)
                count += component.getPolygonCount();

            return count;
        }

        /// <summary>
        /// Public accessor for this type
        /// </summary>
        /// <returns></returns>
        public XygloComponentGroupType getComponentGroupType() { return m_type; }

        /// <summary>
        /// List of Components in this group
        /// </summary>
        /// <returns></returns>
        public List<XygloXnaDrawable> getComponents() { return m_componentList; }

        /// <summary>
        /// List of the XygloXnaDrawable components
        /// </summary>
        protected List<XygloXnaDrawable> m_componentList = new List<XygloXnaDrawable>();

        /// <summary>
        /// Where this group is centred - this is set to the m_postion the last time a
        /// sub-component was added.
        /// </summary>
        protected Vector3 m_groupCentrePosition = Vector3.Zero;

        /// <summary>
        /// Component group type
        /// </summary>
        protected XygloComponentGroupType m_type;
    }
}
