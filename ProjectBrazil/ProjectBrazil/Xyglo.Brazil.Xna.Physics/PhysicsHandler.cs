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
    /// we want to.   Currently we're using Jitter Physics:
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
            World = new World(collision);
            World.AllowDeactivation = true;
            World.Gravity = new JVector(0, 500, 0);
        }

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
        }

        /// <summary>
        /// Accelerate a rigidbody by a certain vector
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
            List<RigidBody> bodyList = World.RigidBodies.Where(item => item.GetHashCode() == drawable.getPhysicsHash()).ToList();

            // If no match then check for ComponentGroup
            //
            if (bodyList.Count == 0)
            {
                if (drawable.GetType() == typeof(XygloComponentGroup))
                {
                    XygloComponentGroup group = (XygloComponentGroup)drawable;
                    foreach (XygloXnaDrawable subDrawable in group.getComponents())
                        bodyList.AddRange(World.RigidBodies.Where(item => item.GetHashCode() == subDrawable.getPhysicsHash()).ToList());
                }
            }

            return bodyList;
        }


        /// <summary>
        /// Run the physics model for a given time
        /// </summary>
        /// <param name="gameTime"></param>
        public void update(GameTime gameTime)
        {
            // Physics model updating
            //
            float step = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bool multithread = true;
            if (step > 1.0f / 100.0f) step = 1.0f / 100.0f;
            World.Step(step, multithread);

            // Prescan for any component groups and add these to a temporary list for updating
            //
            List<XygloXnaDrawable> cgList = m_context.m_drawableComponents.Values.Where(item => item.GetType() == typeof(XygloComponentGroup)).ToList();
            List<XygloXnaDrawable> addList = new List<XygloXnaDrawable>();
            foreach (XygloComponentGroup xCG in cgList)
                addList.AddRange(xCG.getComponents());

            List<RigidBody> remainderList = new List<RigidBody>();
            remainderList.AddRange(World.RigidBodies);

            // Now perform any updates on composite component groups and maintain the remainder list
            //
            foreach (XygloXnaDrawable drawable in addList)
            {
                List<RigidBody> bodyList = World.RigidBodies.Where(item => item.GetHashCode() == drawable.getPhysicsHash()).ToList();

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

            // Update XNA model after physics has completed
            //
            foreach (RigidBody body in remainderList)
            {
                //int hashCode = body.GetHashCode();
                List<XygloXnaDrawable> drawableList = m_context.m_drawableComponents.Values.Where(item => item.getPhysicsHash() == body.GetHashCode()).ToList();

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
                    drawableList[0].setPosition(Conversion.ToXNAVector(body.Position));
                    drawableList[0].setOrientation(Conversion.ToXNAMatrix(body.Orientation));
                }

                body.Update();
            }
        }


        protected void buildComponentGroup(Component component, XygloComponentGroup group)
        {

            if (group.getComponentGroupType() == XygloComponentGroupType.Interloper)
            {
                XygloXnaDrawable headDrawable = group.getComponents().Where(item => item.GetType() == typeof(XygloSphere)).ToList()[0];
                RigidBody head = addDrawable(component, headDrawable);

                XygloXnaDrawable bodyDrawable = group.getComponents().Where(item => item.GetType() == typeof(XygloFlyingBlock)).ToList()[0];
                RigidBody body = addDrawable(component, bodyDrawable);
                
                // connect head and torso
                PointPointDistance headTorso = new PointPointDistance(head, body, head.Position, body.Position);
                    //position + new JVector(0, 1.6f, 0), position + new JVector(0, 1.5f, 0));

                headTorso.Softness = 0.00001f;
                // Add the parts and connections
                //
                //World.AddBody(head);
                //World.AddBody(body);
                World.AddConstraint(headTorso);

                //XygloComponentGroup group = (XygloComponentGroup)drawable;
                //foreach (XygloXnaDrawable subDrawable in group.getComponents())
                //{
                    //addDrawable(component, subDrawable);
                //}

                // Special value for collection
                //
                group.setPhysicsHash(-1);
            }
        }

        /// <summary>
        /// Interpret a Drawable and add it to the 
        /// </summary>
        /// <param name="drawable"></param>
        /// <param name="affectedByGravity"></param>
        /// <param name="moveable"></param>
        public RigidBody addDrawable(Component component, XygloXnaDrawable drawable)
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
                buildComponentGroup(component, (XygloComponentGroup)drawable);
            }

            // If we've constructed a body then populate and add
            //
            if (body != null)
            {
                body.Position = Conversion.ToJitterVector(drawable.getPosition());
                body.AffectedByGravity = component.isAffectedByGravity();
                body.IsStatic = !component.isMoveable();
                body.Mass = Math.Max(component.getMass(), 1000);

                // Set a velocity if we're not static
                //
                if (!body.IsStatic)
                    body.LinearVelocity = Conversion.ToJitterVector(drawable.getVelocity());
                
                // Store this relationship in the calling drawable so we can link them back again
                //
                drawable.setPhysicsHash(body.GetHashCode());

                body.EnableSpeculativeContacts = true;

                // set restitution
                body.Material.Restitution = component.getHardness();
                //body.LinearVelocity = new JVector(0, 0, 0);  
                body.Damping = RigidBody.DampingType.Angular;

                World.AddBody(body);

                return body;
            }else
            {
                Logger.logMsg("Not constructed a physics objects from a XygloDrawable");
            }

            return null;
        }

        /// <summary>
        /// Draw physics items in debug positions.  Not used.
        /// </summary>
        protected void drawDebug()
        {
            int cc = 0;

            m_activeBodies = 0;

            // Draw all shapes
            foreach (RigidBody body in World.RigidBodies)
            {
                if (body.IsActive) m_activeBodies++;
                if (body.Tag is int || body.IsParticle) continue;
                AddBodyToDrawList(body);
            }

            foreach (Constraint constr in World.Constraints)
                constr.DebugDraw(DebugDrawer);

            foreach (RigidBody body in World.RigidBodies)
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
            World.AddBody(m_ground);

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

                    World.AddBody(body);
                }
            }
        }

        /// <summary>
        /// This is the JitterPhysics World definition
        /// </summary>
        public World World { private set; get; }

        /// <summary>
        /// XygloContext is useful
        /// </summary>
        protected XygloContext m_context;

        /// <summary>
        /// Probably don't need these
        /// </summary>
        private Primitives3D.GeometricPrimitive[] primitives = new Primitives3D.GeometricPrimitive[5];

        /// <summary>
        /// Or these..
        /// </summary>
        private enum Primitives { box, sphere, cylinder, cone, capsule }

        /// <summary>
        /// Or this..
        /// </summary>
        public DebugDrawer DebugDrawer { private set; get; }

        /// <summary>
        /// Or this..
        /// </summary>
        private int m_activeBodies = 0;

        /// <summary>
        /// Or this..
        /// </summary>
        protected RigidBody m_ground = null;

        /// <summary>
        /// Or indeed this..
        /// </summary>
        private QuadDrawer m_quadDrawer = null;
    }
}
