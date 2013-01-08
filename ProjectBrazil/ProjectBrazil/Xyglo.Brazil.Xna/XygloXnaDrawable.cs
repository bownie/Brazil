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
    /// An abstract base class that we can use to hold the things we need to 
    /// draw an XNA item.
    /// </summary>
    public abstract class XygloXnaDrawable // --> DrawableGameComponent
    {
        public Color getColour()
        {
            return m_colour;
        }

        public void setColour(Color colour)
        {
            m_colour = colour;
        }

        /// <summary>
        /// Draw this component
        /// </summary>
        public abstract void draw(GraphicsDevice device);

        /// <summary>
        /// Draw a preview of this component scaled into a BoundingBox
        /// </summary>
        /// <param name="device"></param>
        /// <param name="scale"></param>
        public abstract void drawPreview(GraphicsDevice device, BoundingBox fullBoundingBox, BoundingBox previewBoundingBox, Texture2D texture);

        /// <summary>
        /// We need to define a bounding box for any component we're interested in
        /// </summary>
        /// <returns></returns>
        public abstract BoundingBox getBoundingBox();

        /// <summary>
        /// Build the Vertex and Index buffers
        /// </summary>
        /// <param name="device"></param>
        public abstract void buildBuffers(GraphicsDevice device);

        /// <summary>
        /// Return some information on polygons being drawn by this item
        /// </summary>
        /// <returns></returns>
        public abstract int getPolygonCount();

        /// <summary>
        /// Set the position
        /// </summary>
        /// <param name="position"></param>
        public virtual void setPosition(Vector3 position)
        {
            m_position = position;
        }

        /// <summary>
        /// Get the position
        /// </summary>
        /// <returns></returns>
        public virtual Vector3 getPosition()
        {
            return m_position;
        }

        /// <summary>
        /// Add a Vector3 to our position - make this virtual so we can override it
        /// in some base classes as required.
        /// </summary>
        /// <param name="translation"></param>
        public virtual void move(Vector3 translation)
        {
#if SPECIFIC_DEBUG
            if (m_name == "LandingBlock1" && m_velocity != Vector3.Zero)
            {
                Logger.logMsg("LandingBlock1");
            }
#endif
            m_position += translation;
        }

        /// <summary>
        /// Default move is just to add velocity on to position
        /// </summary>
        public virtual void moveDefault()
        {
#if SPECIFIC_DEBUG
            if (m_name == "LandingBlock1" && m_velocity != Vector3.Zero)
            {
                Logger.logMsg("LandingBlock1");
            }
#endif
            m_position += m_velocity;
        }

        /// <summary>
        /// Set the rotation
        /// </summary>
        /// <param name="rotation"></param>
        public void setRotation(double rotation)
        {
            m_rotation = rotation;
        }

        /// <summary>
        /// Incremenet a rotation
        /// </summary>
        /// <param name="rotation"></param>
        public void incrementRotation(double rotation)
        {
            m_rotation += rotation;

            // Check bounds
            if (m_rotation > 2 * Math.PI)
            {
                m_rotation -= 2 * Math.PI;
            }
            else if (m_rotation < -2 * Math.PI)
            {
                m_rotation += 2 * Math.PI;
            }
        }

        /// <summary>
        /// Get the rotation
        /// </summary>
        /// <returns></returns>
        public double getRotation()
        {
            return m_rotation;
        }

        /// <summary>
        /// Make this a virtual method
        /// </summary>
        /// <param name="x"></param>
        public virtual void moveLeft(float x)
        {
            m_position.X -= x;
        }

        /// <summary>
        /// Make this a virtual method 
        /// </summary>
        /// <param name="x"></param>
        public virtual void moveRight(float x)
        {
            m_position.X += x;
        }

        /// <summary>
        /// What we do to jump - add some impetus in the non gravitational direction
        /// </summary>
        public virtual void jump(Vector3 impulse)
        {
            m_velocity += impulse;
        }

        /// <summary>
        /// Get the velocity
        /// </summary>
        /// <returns></returns>
        public Vector3 getVelocity()
        {
            return m_velocity;
        }

        /// <summary>
        /// Set the velocity
        /// </summary>
        /// <param name="velocity"></param>
        public virtual void setVelocity(Vector3 velocity)
        {
            m_velocity = velocity;
        }

        /// <summary>
        /// Accelerate the velocity vector
        /// </summary>
        /// <param name="accelerationVector"></param>
        public virtual void accelerate(Vector3 accelerationVector)
        {
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

            // Round
            m_velocity = XygloConvert.roundVector(m_velocity);
        }

        /// <summary>
        /// Get the name
        /// </summary>
        /// <returns></returns>
        public string getName()
        {
            return m_name;
        }

        /// <summary>
        /// Set the name
        /// </summary>
        /// <param name="name"></param>
        public void setName(string name)
        {
            m_name = name;
        }

        /// <summary>
        /// Should we be destroyed?
        /// </summary>
        /// <returns></returns>
        public bool shouldBeDestroyed()
        {
            return m_pleaseDestroy;
        }

        /// <summary>
        /// Do we want to destroy this drawable?
        /// </summary>
        /// <param name="pleaseDestroy"></param>
        public void setDestroy(bool pleaseDestroy)
        {
            m_pleaseDestroy = pleaseDestroy;
        }

        /// <summary>
        /// Set a parent ComponentGroup
        /// </summary>
        /// <param name="group"></param>
        public void setParent(XygloComponentGroup group)
        {
            m_group = group;
        }

        /// <summary>
        /// Get a parent ComponentGroup
        /// </summary>
        /// <returns></returns>
        public XygloComponentGroup getParent() { return m_group;  }

        /// <summary>
        /// Test for parent
        /// </summary>
        /// <returns></returns>
        public bool hasParent() { return m_group != null; }

        /// <summary>
        /// This is the hashcode of an object used to represent this object in the PhysicsHandler
        /// </summary>
        /// <param name="hashCode"></param>
        public void setPhysicsHash(int hashCode) { m_physicsHash = hashCode; }

        /// <summary>
        /// The hashcode of an object used to represent this object in the PhysicsHandler
        /// </summary>
        /// <returns></returns>
        public int getPhysicsHash() { return m_physicsHash; }

        /// <summary>
        /// Position of this block
        /// </summary>
        public Vector3 m_position;

        /// <summary>
        /// Store locally our colour
        /// </summary>
        protected Color m_colour;

        /// <summary>
        /// Rotation angle per frame
        /// </summary>
        protected double m_rotation = 0.0;

        /// <summary>
        /// Store the velocity at the XygloXnaDrawable as a copy of the initial state of the components
        /// </summary>
        protected Vector3 m_velocity = Vector3.Zero;

        /// <summary>
        /// Define a set of max velocities (-+) for our drawable
        /// </summary>
        protected Vector3 m_maxVelocity = new Vector3(3, 0, 0);

        /// <summary>
        /// Name for this component just in case we want to track it easily for some insane reason
        /// </summary>
        protected string m_name;

        /// <summary>
        /// Set this from collision code if we want to remove this item at a safe point in time
        /// </summary>
        protected bool m_pleaseDestroy = false;

        /// <summary>
        /// A parent ComponentGroup
        /// </summary>
        protected XygloComponentGroup m_group = null;

        /// <summary>
        /// A hashcode we use to identify a physical model
        /// </summary>
        protected int m_physicsHash = 0;

    }
}
