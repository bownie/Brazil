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
        public PhysicsHandler(Game game, XygloContext context, BrazilContext brazilContext)
        {
            m_context = context;
            m_brazilContext = brazilContext;

            // Set up collision and world for physics
            //
            //CollisionSystem collision = new CollisionSystemPersistentSAP();
            CollisionSystem collision = new CollisionSystemSAP();
            m_world = new World(collision);
            collision.CollisionDetected += new CollisionDetectedHandler(collisionHandler);
            m_world.AllowDeactivation = true;
            m_world.Gravity = new JVector(0, 10f, 0);
            //m_world.SetDampingFactors(0.1f, 0.1f);

            // Can play with this
            //
            //m_world.SetInactivityThreshold(0.1f, 1.0f, 0.3f);
        }

        /// <summary>
        /// Perform collision management
        /// </summary>
        /// <param name="body1"></param>
        /// <param name="body2"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="value"></param>
        protected void collisionHandler(RigidBody body1, RigidBody body2, JVector x, JVector y, JVector z, float value)
        {
            // Ignore everything that doesn't have anything to do with the interloper
            //
            RigidBody interloperBody = m_world.RigidBodies.Where(item => item.GetHashCode() == m_interloperBodyPhysicsHash).ToList()[0];
            if (body1 != interloperBody && body2 != interloperBody)
                return;

            List<RigidBody> bodyList = new List<RigidBody>();
            bodyList.Add(body1);
            bodyList.Add(body2);

            List<XygloXnaDrawable> drawables = getDrawableForRigidBodies(bodyList);
            //XygloXnaDrawable drawable2 = getDrawableForRigidBody(body2);

            if (drawables.Count() != 2)
            {
                Logger.logMsg("Failed to find drawables from RigidBodies");
                return;
            }

            // Ok we need to check if the interloper bottom block is in contact with a flying block
            //
            bool grounded = (drawables[0] is XygloFlyingBlock && drawables[1] is XygloFlyingBlock);

            

            /*
            if (grounded != m_interloperGrounded)
            {
                Logger.logMsg("Grounding state changed");
                m_interloperGrounded = grounded;
            }
            */

            XygloCoin coin = null;

            if (drawables[0] is XygloCoin)
                coin = (XygloCoin)drawables[0];

            if (drawables[1] is XygloCoin)
                coin = (XygloCoin)drawables[1];

            // We've got a coin
            //
            if (coin != null)
            {
                BrazilGoody brazilCoin = (BrazilGoody)getComponentForDrawable(coin);

                if (brazilCoin != null)
                {
                    m_brazilContext.m_interloper.incrementScore(brazilCoin.m_worth);
                    coin.setDestroy(true);
                }
            }
        }

        protected Component getComponentForDrawable(XygloXnaDrawable drawable)
        {
            foreach (Component component in m_context.m_drawableComponents.Keys)
            {
                if (m_context.m_drawableComponents[component] == drawable)
                    return component;
            }

            return null;
        }
        /// <summary>
        /// Is the interloper grounded and ready for jumping?
        /// </summary>
        //protected bool m_interloperGrounded = false;

        /// <summary>
        ///  From 
        /// </summary>
        private void DrawIslands()
        {
            JBBox box;

            foreach (CollisionIsland island in m_world.Islands)
            {
                box = JBBox.SmallBox;

                foreach (RigidBody body in island.Bodies)
                {
                    box = JBBox.CreateMerged(box, body.BoundingBox);
                }

                DebugDrawer.DrawAabb(box.Min, box.Max, island.IsActive() ? Color.Green : Color.Yellow);
            }
        }

        /// <summary>
        /// Check to see if the interloper is in contact with a rigidbody and capable of pushing off
        /// </summary>
        /// <returns></returns>
        protected bool isInterloperGrounded()
        {
            RigidBody interloperBody = m_world.RigidBodies.Where(item => item.GetHashCode() == m_interloperBodyPhysicsHash).ToList()[0];

            // Want to get a list of blocks at the top level - none of these will be componentgroups so we're safe
            //
            List<XygloXnaDrawable> blocks = m_context.m_drawableComponents.Values.Where(item => item.GetType() == typeof(XygloFlyingBlock)).ToList();
            List<RigidBody> bodies = getRigidBodiesForDrawables(blocks);
            List<RigidBody> bodiesIamInContactWith = new List<RigidBody>();

            // every body "b" in the island is likely to touch our character "body"
            foreach (RigidBody b in interloperBody.CollisionIsland.Bodies)
            {
                foreach (RigidBody blockBody in bodies)
                {
                    // check if it's a direct contact, which means there is an arbiter in the global arbitermap
                    // which holds all arbiters
                    if (m_world.ArbiterMap.ContainsArbiter(blockBody, b))
                        bodiesIamInContactWith.Add(b);
                }
            }

            return (bodiesIamInContactWith.Count() > 0);
        }


        /// <summary>
        /// Test to see if the Interloper base block is on another flying block
        /// </summary>
        /// <returns></returns>
        protected bool isInterloperGroundedTest1()
        {
            // Flags for finding the interloper block on same island as another flying block
            //
            bool foundInterloperBase = false;
            bool foundFlyingBlock = false;
            foreach (CollisionIsland island in m_world.Islands)
            {
                foreach(XygloXnaDrawable drawable in getDrawableForRigidBodies(island.Bodies.ToList()))
                {
                    if (drawable is XygloFlyingBlock)
                    {
                        if (drawable.getPhysicsHash() == m_interloperBodyPhysicsHash)
                            foundInterloperBase = true;
                        else
                            foundFlyingBlock = true;
                    }
                }

                // Check for both found in the same island
                //
                if (foundInterloperBase && foundFlyingBlock)
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Accelerate a rigidbody by a certain vector - this is an input to the model
        /// </summary>
        /// <param name="drawable"></param>
        /// <param name="acceleration"></param>
        public void accelerate(XygloXnaDrawable drawable, Vector3 acceleration)
        {
            if (!isInterloperGrounded())
                return;

            //if (!m_interloperGrounded)
                //return;

            // First search in the 
            List<RigidBody> bodyList = getRigidBodiesForDrawable(drawable);

            foreach(RigidBody body in bodyList)
                body.LinearVelocity += Conversion.ToJitterVector(acceleration * m_physicScale);
        }

        protected List<RigidBody> getRigidBodiesForDrawable(XygloXnaDrawable drawable)
        {
            List<XygloXnaDrawable> drawableList = new List<XygloXnaDrawable>();
            drawableList.Add(drawable);
            return getRigidBodiesForDrawables(drawableList);
        }

        /// <summary>
        /// Do a recursive pull of all the drawable components
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        protected List<XygloXnaDrawable> getExpandedDrawables(List<XygloXnaDrawable> list = null)
        {
            List<XygloXnaDrawable> rL = new List<XygloXnaDrawable>();

            if (list == null)
            {
                list = m_context.m_drawableComponents.Values.ToList();
            }

            foreach (XygloXnaDrawable drawable in list)
            {
                if (drawable is XygloComponentGroup)
                {
                    rL.AddRange(getExpandedDrawables(((XygloComponentGroup)drawable).getComponents()));
                }
                else
                    rL.Add(drawable);
            }
            return rL;
        }

        /// <summary>
        /// Get a RigidBody for an Xna drawable
        /// </summary>
        /// <param name="drawable"></param>
        /// <returns></returns>
        protected List<RigidBody> getRigidBodiesForDrawables(List<XygloXnaDrawable> drawables)
        {
            List<RigidBody> bodyList = new List<RigidBody>();
            
            // Build body list
            foreach(XygloXnaDrawable drawable in drawables)
            {
                if (drawable.GetType() == typeof(XygloComponentGroup))
                {
                    XygloComponentGroup xCG = (XygloComponentGroup)drawable;
                    bodyList.AddRange(getRigidBodiesForDrawables(xCG.getComponents()));
                }
                else
                {
                    bodyList.AddRange(m_world.RigidBodies.Where(item => item.GetHashCode() == drawable.getPhysicsHash()).ToList());
                }
            }
            
            /*
            // If no match then check for ComponentGroup
            //
            if (bodyList.Count == 0)
            {
                
                    XygloComponentGroup group = (XygloComponentGroup)drawable;
                    foreach (XygloXnaDrawable subDrawable in group.getComponents())
                        bodyList.AddRange(m_world.RigidBodies.Where(item => item.GetHashCode() == subDrawable.getPhysicsHash()).ToList());
                }
            }*/

            return bodyList;
        }

        /// <summary>
        /// Get a drawable for a RigidBody - if we have component groups then we
        /// recursively call this method.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public List<XygloXnaDrawable> getDrawableForRigidBodies(List<RigidBody> bodies)
        {
            List<XygloXnaDrawable> rL = new List<XygloXnaDrawable>();

            foreach (XygloXnaDrawable drawable in getExpandedDrawables())
            {
                foreach (RigidBody body in bodies)
                {
                    if (drawable.getPhysicsHash() == body.GetHashCode())
                        rL.Add(drawable);
                }
            }

            return rL;
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
                body = new RigidBody(new BoxShape(Conversion.ToJitterVector(fb.getSize() * m_physicScale)));
            }
            else if (drawable is XygloTexturedBlock)
            {
                XygloTexturedBlock fb = (XygloTexturedBlock)drawable;
                JVector size = Conversion.ToJitterVector(fb.getSize() * m_physicScale);
                body = new RigidBody(new BoxShape(size));
            }
            else if (drawable is XygloCoin)
            {
                XygloCoin coin = (XygloCoin)drawable;
                body = new RigidBody(new SphereShape(coin.getRadius() * m_physicScale));

            }
            else if (drawable is XygloSphere)
            {
                XygloSphere sphere = (XygloSphere)drawable;
                body = new RigidBody(new SphereShape(sphere.getRadius() * m_physicScale));
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
                body.Position = Conversion.ToJitterVector(drawable.getPosition() * m_physicScale);
                body.AffectedByGravity = component.isAffectedByGravity();
                body.IsStatic = !component.isMoveable();
                body.Mass = m_testMass; // Math.Max(component.getMass(), 1000);

                // Ensure that the body is oriented in the same manner as the drawable
                //
                body.Orientation = Conversion.ToJitterMatrix(drawable.getTotalOrientation());

                // Set a velocity if we're not static
                //
                if (!body.IsStatic)
                    body.LinearVelocity = Conversion.ToJitterVector(drawable.getVelocity() * m_physicScale);

                // Store this relationship in the calling drawable so we can link them back again
                //
                drawable.setPhysicsHash(body.GetHashCode());

                // Set restitution from hardness
                //
                body.Material.Restitution = m_testRestitution; // component.getHardness();
                body.Damping = RigidBody.DampingType.Angular;
                body.Material.KineticFriction = 0.5f;
                body.Material.StaticFriction = 0.5f;
                m_context.m_physicsHandler.addRigidBody(body);

                return body;
            }
            else
            {
                Logger.logMsg("Not constructed a physics objects from a XygloDrawable");
            }

            return null;
        }

        /// <summary>
        /// Scaling factor between our drawbles and jitter physics
        /// </summary>
        protected float m_physicScale = 0.01f;

        /// <summary>
        /// Test restitution
        /// </summary>
        protected float m_testRestitution = 0.5f; //0000001f;

        /// <summary>
        /// Test mass
        /// </summary>
        protected float m_testMass = 0.1f;

        protected int m_interloperBodyPhysicsHash = 0;

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
                head.SetMassProperties(JMatrix.Zero, 0.5f, true);
                //head.Material.Restitution = m_testRestitution; // component.getHardness();
                //head.Damping = RigidBody.DampingType.Linear | RigidBody.DampingType.Angular;
                //head.Mass = 0.0f; // component.getMass();
                //head.EnableSpeculativeContacts = true;

                XygloXnaDrawable bodyDrawable = group.getComponents().Where(item => item.GetType() == typeof(XygloFlyingBlock)).ToList()[0];
                RigidBody body = createPhysical(component, bodyDrawable);

                // See above caveat!
                //
                //body.SetMassProperties(JMatrix.Zero, 1.0f / 1000.0f, true);
                //body.Material.Restitution = m_testRestitution; // component.getHardness();
                //body.Damping = RigidBody.DampingType.Linear | RigidBody.DampingType.Angular;
                //body.Mass = m_testMass; // component.getMass();
                //body.EnableSpeculativeContacts = true;
                body.Material.KineticFriction = 0.3f;
                body.Material.StaticFriction = 0.0f;

                // Store the body RigidBody hashcode
                //
                m_interloperBodyPhysicsHash = body.GetHashCode();

                // connect head and torso
                //PointPointDistance headTorso = new PointPointDistance(head, torso,
                    //position + new JVector(0, 1.6f, 0), position + new JVector(0, 1.5f, 0));

                //JVector headConnection = new JVector(head.Position.X, head.BoundingBox.Max.Y, head.Position.Z);
                //PointPointDistance headTorso = new PointPointDistance(head, body, headConnection, headConnection + new JVector(0, -0.1f,0));
                PointPointDistance headTorso = new PointPointDistance(head, body, head.Position, body.Position);
                headTorso.Softness = 0.01f; // make sure this is within tolerance otherwise weird effects can happen!
                addConstraint(headTorso);

                // Add a fixed angle constraint to keep the interloper upright
                //
                //Jitter.Dynamics.Constraints.SingleBody.FixedAngle fixedAngle = new Jitter.Dynamics.Constraints.SingleBody.FixedAngle(body);
                //fixedAngle.InitialOrientation = new JMatrix(0, 0, 0, 0, 0.9f, 0, 0, 0, 0);
                //fixedAngle.Softness = 0.1f;
                //addConstraint(fixedAngle);

                // Use this instead of the fixed angle constraint
                //
                body.SetMassProperties(JMatrix.Zero, 1, true);

                // Add another fixed length constraint to attach the head
                //
                //FixedAngle headBody = new FixedAngle(head, body);
                //headBody.InitialOrientationBody1 = new JMatrix(0, 0, 0, 0, 1, 0, 0, 0, 0);
                //headBody.InitialOrientationBody2 = new JMatrix(0, 0, 0, 0, 1, 0, 0, 0, 0);
                //headBody.Softness = 0.000001f;
                //addConstraint(headBody);


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
                    //Vector3 oldPosition = drawable.getPosition() / m_physicScale;
                    drawable.setPosition(Conversion.ToXNAVector(body.Position) / m_physicScale);
                    drawable.setOrientation(Conversion.ToXNAMatrix(body.Orientation));
                    drawable.buildBuffers(m_context.m_graphics.GraphicsDevice);

                    // Remove from the remainder list
                    //
                    remainderList.Remove(body);
                }
            }

            
            // Update XNA model after physics has completed
            //
            foreach (RigidBody body in remainderList)
            {
                List<XygloXnaDrawable> drawableList = includeList.Where(item => item.getPhysicsHash() == body.GetHashCode()).ToList();

                foreach (XygloXnaDrawable drawable in drawableList)
                {
                    // Update position
                    //
                    drawable.setPosition(Conversion.ToXNAVector(body.Position) / m_physicScale);

                    //if (body.Shape is SphereShape)
                    //{
                        //Logger.logMsg("Position = " + drawable.getPosition());
                    //}

                    // Have to rebuild
                    //
                    drawable.buildBuffers(m_context.m_graphics.GraphicsDevice);

                    // Don't set the orientation here because in the drawable we measure it in two ways
                    //
                    //drawable.setOrientation(Conversion.ToXNAMatrix(body.Orientation));
                }

                // This is causing the disappearance of objects
                //
                //body.Update();
            }
            
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
            m_ground.Material.KineticFriction = 0.5f;
            
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

        /// <summary>
        /// Brazil context
        /// </summary>
        protected BrazilContext m_brazilContext = null;
    }
}
