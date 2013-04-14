using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Jitter;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.Dynamics.Constraints;

namespace Xyglo.Brazil.Xna.Physics
{
    public enum BodyTag { DrawMe, DontDrawMe }

    /// <summary>
    /// The Xyglo wrapper to whatever physics implementation we are going
    /// to use.  Allows us to potentially change physics engines whenever
    /// we want to.   Currently we're using Jitter Physics and this class
    /// provides handles for initial setup, updates and movements.
    /// </summary>
    public class PhysicsHandler
    {
        /// <summary>
        /// http://cycling74.com/physics/
        /// </summary>
        public PhysicsHandler(Game game, XygloContext context)
        {
            m_context = context;

            // Set up collision and world for physics
            //
            CollisionSystem collision = new CollisionSystemPersistentSAP();
            m_world = new World(collision);
            m_world.AllowDeactivation = true;
            m_world.Gravity = new JVector(0, 500f, 0);
            //m_world.SetDampingFactors(0.8f, 0.8f);
        }

        /*
        public void initialise()
        {
            primitives[(int)Primitives.box] = new Primitives3D.BoxPrimitive(m_context.m_graphics.GraphicsDevice);
            primitives[(int)Primitives.capsule] = new Primitives3D.CapsulePrimitive(m_context.m_graphics.GraphicsDevice);
            primitives[(int)Primitives.cone] = new Primitives3D.ConePrimitive(m_context.m_graphics.GraphicsDevice);
            primitives[(int)Primitives.cylinder] = new Primitives3D.CylinderPrimitive(m_context.m_graphics.GraphicsDevice);
            primitives[(int)Primitives.sphere] = new Primitives3D.SpherePrimitive(m_context.m_graphics.GraphicsDevice);
        }

        private void AddShapeToDrawList(Shape shape, JMatrix ori, JVector pos)
        {
            Primitives3D.GeometricPrimitive primitive = null;
            Matrix scaleMatrix = Matrix.Identity;

            if (shape is BoxShape)
            {
                primitive = primitives[(int)Primitives.box];
                scaleMatrix = Matrix.CreateScale(Conversion.ToXNAVector((shape as BoxShape).Size));
            }
            else if (shape is SphereShape)
            {
                primitive = primitives[(int)Primitives.sphere];
                scaleMatrix = Matrix.CreateScale((shape as SphereShape).Radius);
            }
            else if (shape is CylinderShape)
            {
                primitive = primitives[(int)Primitives.cylinder];
                CylinderShape cs = shape as CylinderShape;
                scaleMatrix = Matrix.CreateScale(cs.Radius, cs.Height, cs.Radius);
            }
            else if (shape is CapsuleShape)
            {
                primitive = primitives[(int)Primitives.capsule];
                CapsuleShape cs = shape as CapsuleShape;
                scaleMatrix = Matrix.CreateScale(cs.Radius * 2, cs.Length, cs.Radius * 2);

            }
            else if (shape is ConeShape)
            {
                ConeShape cs = shape as ConeShape;
                scaleMatrix = Matrix.CreateScale(cs.Radius, cs.Height, cs.Radius);
                primitive = primitives[(int)Primitives.cone];
            }

            if (primitive != null)
                primitive.AddWorldMatrix(scaleMatrix * Conversion.ToXNAMatrix(ori) *
                            Matrix.CreateTranslation(Conversion.ToXNAVector(pos)));
        }

        private void AddBodyToDrawList(RigidBody rb)
        {
            if (rb.Tag is BodyTag && ((BodyTag)rb.Tag) == BodyTag.DontDrawMe) return;

            bool isCompoundShape = (rb.Shape is CompoundShape);

            if (!isCompoundShape)
            {
                AddShapeToDrawList(rb.Shape, rb.Orientation, rb.Position);
            }
            else
            {
                CompoundShape cShape = rb.Shape as CompoundShape;
                JMatrix orientation = rb.Orientation;
                JVector position = rb.Position;

                foreach (var ts in cShape.Shapes)
                {
                    JVector pos = ts.Position;
                    JMatrix ori = ts.Orientation;

                    JVector.Transform(ref pos, ref orientation, out pos);
                    JVector.Add(ref pos, ref position, out pos);

                    JMatrix.Multiply(ref ori, ref orientation, out ori);

                    AddShapeToDrawList(ts.Shape, ori, pos);
                }
            }
        }*/

        /// <summary>
        /// Accelerate a rigidbody by a certain vector - this is an input to the model
        /// </summary>
        /// <param name="drawable"></param>
        /// <param name="acceleration"></param>
        public void accelerate(XygloXnaDrawable drawable, Vector3 acceleration)
        {
            // First search in the 
            List<RigidBody> bodyList = getRigidBodiesForDrawable(drawable);

            foreach(RigidBody body in bodyList)
                body.LinearVelocity += Conversion.ToJitterVector(acceleration);
        }

        /// <summary>
        /// Get a RigidBody for an Xna drawable
        /// </summary>
        /// <param name="drawable"></param>
        /// <returns></returns>
        public List<RigidBody> getRigidBodiesForDrawable(XygloXnaDrawable drawable)
        {
            List<RigidBody> bodyList = m_world.RigidBodies.Where(item => item.GetHashCode() == drawable.getPhysicsHash()).ToList();

            // If no match then check for ComponentGroup
            //
            if (bodyList.Count == 0)
            {
                if (drawable.GetType() == typeof(XygloComponentGroup))
                {
                    XygloComponentGroup group = (XygloComponentGroup)drawable;
                    foreach (XygloXnaDrawable subDrawable in group.getComponents())
                        bodyList.AddRange(m_world.RigidBodies.Where(item => item.GetHashCode() == subDrawable.getPhysicsHash()).ToList());
                }
            }

            return bodyList;
        }

        /// <summary>
        /// Interpret a Drawable and add it to the Physics model according to type
        /// </summary>
        /// <param name="drawable"></param>
        /// <param name="affectedByGravity"></param>
        /// <param name="moveable"></param>
        public RigidBody createPhysical(Component component, XygloXnaDrawable drawable)
        {
            RigidBody body = null;

            if (drawable is XygloFlyingBlock)
            {
                XygloFlyingBlock fb = (XygloFlyingBlock)drawable;
                body = new RigidBody(new BoxShape(Conversion.ToJitterVector(fb.getSize())));
            }
            else if (drawable is XygloTexturedBlock)
            {
                XygloTexturedBlock fb = (XygloTexturedBlock)drawable;
                JVector size = Conversion.ToJitterVector(fb.getSize());
                body = new RigidBody(new BoxShape(size));
            }
            else if (drawable is XygloSphere)
            {
                XygloSphere sphere = (XygloSphere)drawable;
                body = new RigidBody(new SphereShape(sphere.getRadius()));
            }
            else if (drawable is XygloComponentGroup)
            {
                createPhysicalComponentGroup(component, (XygloComponentGroup)drawable);
            }

            // If we've constructed a body then populate and add
            //
            if (body != null)
            {
                body.EnableSpeculativeContacts = true;
                body.Position = Conversion.ToJitterVector(drawable.getPosition());
                body.AffectedByGravity = component.isAffectedByGravity();
                body.IsStatic = !component.isMoveable();
                body.Mass = m_testMass; // Math.Max(component.getMass(), 1000);

                // Ensure that the body is oriented in the same manner as the drawable
                //
                body.Orientation = Conversion.ToJitterMatrix(drawable.getTotalOrientation());

                // Set a velocity if we're not static
                //
                if (!body.IsStatic)
                    body.LinearVelocity = Conversion.ToJitterVector(drawable.getVelocity());

                // Store this relationship in the calling drawable so we can link them back again
                //
                drawable.setPhysicsHash(body.GetHashCode());

                // Set restitution from hardness
                //
                body.Material.Restitution = m_testRestitution; // component.getHardness();
                body.Damping = RigidBody.DampingType.Angular;
                m_context.m_physicsHandler.addRigidBody(body);

                return body;
            }
            else
            {
                Logger.logMsg("Not constructed a physics objects from a XygloDrawable");
            }

            return null;
        }

        protected float m_testRestitution = 0.1f;
        protected float m_testHardness = 0.1f;
        protected float m_testMass = 1.0f;

        /// <summary>
        /// Link a set of components to a component group and perform some coupling
        /// between them.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="group"></param>
        protected void createPhysicalComponentGroup(Component component, XygloComponentGroup group)
        {

            if (group.getComponentGroupType() == XygloComponentGroupType.Interloper)
            {
                XygloXnaDrawable headDrawable = group.getComponents().Where(item => item.GetType() == typeof(XygloSphere)).ToList()[0];
                RigidBody head = createPhysical(component, headDrawable);

                // Stop rotations  - this might be wrong!
                //
                //head.SetMassProperties(JMatrix.Zero, 1.0f / 1000.0f, true);
                head.Material.Restitution = m_testRestitution; // component.getHardness();
                head.Damping = RigidBody.DampingType.Angular;
                head.Mass = m_testMass; // component.getMass();
                head.EnableSpeculativeContacts = true;

                XygloXnaDrawable bodyDrawable = group.getComponents().Where(item => item.GetType() == typeof(XygloFlyingBlock)).ToList()[0];
                RigidBody body = createPhysical(component, bodyDrawable);

                // See above caveat!
                //
                body.SetMassProperties(JMatrix.Zero, 1.0f / 1000.0f, true);
                body.Material.Restitution = m_testRestitution; // component.getHardness();
                body.Damping = RigidBody.DampingType.Angular;
                body.Mass = m_testMass; // component.getMass();
                body.EnableSpeculativeContacts = true;
                // Connect head and torso with a hard point to point connection like so
                //
                PointPointDistance headTorso = new PointPointDistance(head, body, head.Position, body.Position);
                headTorso.Softness = 0.00001f;

                // Add the connection - the body parts are already add implicitly (might want to change that)
                //
                m_context.m_physicsHandler.addConstraint(headTorso);

                //sphere.EnableSpeculativeContacts = true;

                // set restitution
                //sphere.Material.Restitution = box.Material.Restitution = 1.0f / 10.0f * i;
                //sphere.LinearVelocity = new JVector(0, 20, 0);


                //sphere.Damping = RigidBody.DampingType.Angular;

                // Special value for collection to indicate it
                //
                group.setPhysicsHash(-1);
            }
            else if (group.getComponentGroupType() == XygloComponentGroupType.Fiend)
            {
                XygloXnaDrawable headDrawable = group.getComponents().Where(item => item.GetType() == typeof(XygloSphere)).ToList()[0];
                RigidBody head = createPhysical(component, headDrawable);

                // Stop rotations  - this might be wrong!
                //
                head.SetMassProperties(JMatrix.Zero, 1.0f / 1000.0f, true);

                XygloXnaDrawable bodyDrawable = group.getComponents().Where(item => item.GetType() == typeof(XygloFlyingBlock)).ToList()[0];
                RigidBody body = createPhysical(component, bodyDrawable);

                // See above caveat!
                //
                body.SetMassProperties(JMatrix.Zero, 1.0f / 1000.0f, true);

                // Connect head and torso with a hard point to point connection like so
                //
                PointPointDistance headTorso = new PointPointDistance(head, body, head.Position, body.Position);
                headTorso.Softness = 0.00001f;

                // Add the connection - the body parts are already add implicitly (might want to change that)
                //
                m_context.m_physicsHandler.addConstraint(headTorso);

                // Special value for collection to indicate it
                //
                group.setPhysicsHash(-1);
            }
        }


        /* From Restitution.cs
         */
        /*
        public override void Build()
        {
            AddGround();

            for (int i = 0; i < 11; i++)
            {
                RigidBody box = new RigidBody(new BoxShape(1, 0.01f, 1));
                this.Demo.World.AddBody(box);
                JVector boxPos = new JVector(-15 + i * 3 + 1, 5, 0);

                box.Position = boxPos;
                box.IsStatic = true;

                RigidBody sphere = new RigidBody(new SphereShape(0.5f));
                this.Demo.World.AddBody(sphere);

                sphere.Position = boxPos + JVector.Up * 30;
                sphere.EnableSpeculativeContacts = true;

                // set restitution
                sphere.Material.Restitution = box.Material.Restitution = 1.0f / 10.0f * i;
                sphere.LinearVelocity = new JVector(0, 20, 0);


                sphere.Damping = RigidBody.DampingType.Angular;
            }
        }
        */

        /// <summary>
        /// Run the physics model for a given time
        /// </summary>
        /// <param name="gameTime"></param>
        public void update(GameTime gameTime, List<XygloXnaDrawable> includeList)
        {
            // Don't do anything for no drawables
            //
            if (includeList.Count() == 0)
                return;

            // Physics model updating
            //
            float step = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bool multithread = true;
            if (step > 1.0f / 100.0f) step = 1.0f / 100.0f;
            m_world.Step(step, multithread);

            //List<XygloXnaDrawable> activeList = m_context.m_drawableComponents.Where(item => excludeList.Contains(item)).ToList();

            // Prescan for any component groups and add these to a temporary list for updating
            //
            List<XygloXnaDrawable> cgList = includeList.Where(item => item.GetType() == typeof(XygloComponentGroup)).ToList();
            List<XygloXnaDrawable> addList = new List<XygloXnaDrawable>();
            foreach (XygloComponentGroup xCG in cgList)
                addList.AddRange(xCG.getComponents());

            List<RigidBody> remainderList = new List<RigidBody>();
            remainderList.AddRange(m_world.RigidBodies);

            // Now perform any updates on composite component groups and maintain the remainder list
            //
            foreach (XygloXnaDrawable drawable in addList)
            {
                List<RigidBody> bodyList = m_world.RigidBodies.Where(item => item.GetHashCode() == drawable.getPhysicsHash()).ToList();

                foreach (RigidBody body in bodyList)
                {
                    // Update position
                    //
                    Vector3 position = Conversion.ToXNAVector(body.Position);
                    Vector3 oldPosition = drawable.getPosition();
                    drawable.setPosition(Conversion.ToXNAVector(body.Position));
                    drawable.setOrientation(Conversion.ToXNAMatrix(body.Orientation));

                    // Remove from the remainder list
                    //
                    remainderList.Remove(body);
                }
            }

            /*
            // Update XNA model after physics has completed
            //
            foreach (RigidBody body in remainderList)
            {
                //int hashCode = body.GetHashCode();
                List<XygloXnaDrawable> drawableList = includeList.Where(item => item.getPhysicsHash() == body.GetHashCode()).ToList();

//                List<XygloXnaDrawable> componentGroupDrawableList = m_context.m_drawableComponents.Values.Where(item => item.get
                
                // We can get more than one object with the same hash code if we have a Collection of
                // Drawables for example in a ComponentGroup.
                //
                foreach (XygloXnaDrawable drawable in drawableList)
                {
                    // Update position
                    //
                    Vector3 position = Conversion.ToXNAVector(body.Position);
                    Vector3 oldPosition = drawableList[0].getPosition();
                    //drawableList[0].setPosition(Conversion.ToXNAVector(body.Position));
                    //drawableList[0].setOrientation(Conversion.ToXNAMatrix(body.Orientation));
                }

                // This is causing the disappearance of objects
                //
                //body.Update();
            }
            */
        }

        /// <summary>
        /// Ensure that the physics arena is clear of objects
        /// </summary>
        public void clearAll()
        {
            m_world.Clear();
        }

        /// <summary>
        /// Remove only a list of specified components
        /// </summary>
        /// <param name="removeList"></param>
        public void clearAppComponents(List<Component> removeList)
        {
            foreach (Component component in removeList)
            {
                if (m_context.m_drawableComponents.ContainsKey(component))
                    removeBodyForDrawable(m_context.m_drawableComponents[component]);
            }
        }

        /// <summary>
        /// Remove by drawable from physics
        /// </summary>
        /// <param name="drawables"></param>
        public void removeBodyForDrawable(XygloXnaDrawable drawable)
        {
            List<RigidBody> bodyList = m_world.RigidBodies.Where(item => item.GetHashCode() == drawable.getPhysicsHash()).ToList();

            foreach(RigidBody body in bodyList)
            {
                m_world.RemoveBody(body);
            }
        }


        /// <summary>
        /// Draw physics items in debug positions.  Not used.
        /// </summary>
        protected void drawDebug()
        {
            int cc = 0;

            m_activeBodies = 0;

            // Draw all shapes
            foreach (RigidBody body in m_world.RigidBodies)
            {
                if (body.IsActive) m_activeBodies++;
                if (body.Tag is int || body.IsParticle) continue;
                //AddBodyToDrawList(body);
            }

            foreach (Constraint constr in m_world.Constraints)
                constr.DebugDraw(DebugDrawer);

            foreach (RigidBody body in m_world.RigidBodies)
            {
                DebugDrawer.Color = Color.White;
                body.DebugDraw(DebugDrawer);
                cc++;
            }

            foreach (Primitives3D.GeometricPrimitive prim in primitives) prim.Draw(m_context.m_physicsEffect);

            //GraphicsDevice.RasterizerState = cullMode;
        }

        /// <summary>
        /// Add ground to this world
        /// </summary>
        /// <param name="game"></param>
        public void addGround(Game game)
        {
            m_ground = new RigidBody(new BoxShape(new JVector(200, 20, 200)));
            m_ground.Position = new JVector(0, -10, 0);
            m_ground.Tag = BodyTag.DontDrawMe;
            m_ground.IsStatic = true;

            // This kills performance
            //
            //m_ground.EnableDebugDraw = true;
            m_world.AddBody(m_ground);

            //ground.Restitution = 1.0f;
            m_ground.Material.KineticFriction = 0.0f;
            
            m_quadDrawer = new QuadDrawer(game, m_context, 1000);

            // This does do something...
            //
            m_quadDrawer.setPosition(new Vector3(0, -300, 0));
            game.Components.Add(m_quadDrawer);

            addTestContent();
        }

        /// <summary>
        /// Some test content from Jenga example
        /// </summary>
        public void addTestContent()
        {
            for (int i = 0; i < 15; i++)
            {
                bool even = (i % 2 == 0);

                for (int e = 0; e < 3; e++)
                {
                    JVector size = (even) ? new JVector(1, 1, 3) : new JVector(3, 1, 1);
                    RigidBody body = new RigidBody(new BoxShape(size));
                    body.Position = new JVector(3.0f + (even ? e : 1.0f), i + 0.5f, -13.0f + (even ? 1.0f : e));

                    m_world.AddBody(body);
                }
            }
        }

        /// <summary>
        /// Add rigid body
        /// </summary>
        /// <param name="body"></param>
        public void addRigidBody(RigidBody body)
        {
            m_world.AddBody(body);
        }

        /// <summary>
        /// Add contraint
        /// </summary>
        /// <param name="constraint"></param>
        public void addConstraint(Constraint constraint)
        {
            m_world.AddConstraint(constraint);
        }

        /// <summary>
        /// This is the JitterPhysics World definition
        /// </summary>
        protected World m_world;

        /// <summary>
        /// XygloContext is useful
        /// </summary>
        protected XygloContext m_context;

        /// <summary>
        /// Probably don't need these
        /// </summary>
        protected Primitives3D.GeometricPrimitive[] primitives = new Primitives3D.GeometricPrimitive[5];

        /// <summary>
        /// Or these..
        /// </summary>
        protected enum Primitives { box, sphere, cylinder, cone, capsule }

        /// <summary>
        /// Or this..
        /// </summary>
        protected DebugDrawer DebugDrawer { private set; get; }

        /// <summary>
        /// Or this..
        /// </summary>
        protected int m_activeBodies = 0;

        /// <summary>
        /// Or this..
        /// </summary>
        protected RigidBody m_ground = null;

        /// <summary>
        /// Or indeed this..
        /// </summary>
        protected QuadDrawer m_quadDrawer = null;
    }
}
