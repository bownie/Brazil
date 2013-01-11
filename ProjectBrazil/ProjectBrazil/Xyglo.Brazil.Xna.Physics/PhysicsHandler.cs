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
    /// we want to.
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
            World = new World(collision); World.AllowDeactivation = true;

            // Setup the random colours
            //
            Random rr = new Random();
            rndColors = new Color[20];

            for (int i = 0; i < 20; i++)
            {
                rndColors[i] = new Color((float)rr.NextDouble(), (float)rr.NextDouble(), (float)rr.NextDouble());
            }

            DebugDrawer = new DebugDrawer(game, context);
            //this.Components.Add(DebugDrawer);
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

        public void update(GameTime gameTime)
        {
            // Physics model updating
            //
            float step = (float)gameTime.ElapsedGameTime.TotalSeconds;
            bool multithread = true;
            if (step > 1.0f / 100.0f) step = 1.0f / 100.0f;
            World.Step(step, multithread);

            // Update XNA model after physics has completed
            //
            foreach (RigidBody body in World.RigidBodies)
            {
                List<XygloXnaDrawable> drawableList = m_context.m_drawableComponents.Values.Where(item => item.getPhysicsHash() == body.GetHashCode()).ToList();

                if (drawableList.Count() == 1)
                {
                    Vector3 position = Conversion.ToXNAVector(body.Position);
                    Vector3 oldPosition = drawableList[0].getPosition();
                    //drawableList[0].setPosition(Conversion.ToXNAVector(body.Position));
                }

                //body.Update();
            }
        }

        public List<Scene> Scenes { private set; get; }
        public World World { private set; get; }

        public void addDrawable(XygloXnaDrawable drawable)
        {
            if (drawable is XygloFlyingBlock)
            {
                // add a flying block to our physical world
                // 
                //Logger.logMsg("ADDING FLYING BLOCK");

                XygloFlyingBlock fb = (XygloFlyingBlock)drawable;

                JVector size = new JVector(1, 5, 2); // Conversion.ToJitterVector(fb.getSize());
                RigidBody body = new RigidBody(new BoxShape(size));
                body.Position = new JVector(0, 0, 0); // Conversion.ToJitterVector(fb.getPosition());
                body.AffectedByGravity = false;

                // set any velocity
                //
                // Conversion.ToXNAVector
                //
                body.LinearVelocity = Conversion.ToJitterVector(fb.getVelocity());
                body.EnableDebugDraw = true;
                
                // Store this relationship in the calling drawable so we can link them back again
                //
                drawable.setPhysicsHash(body.GetHashCode());

                World.AddBody(body);
            }
        }

        public DebugDrawer DebugDrawer { private set; get; }
        Color[] rndColors;


        /// <summary>
        /// Draw physics items in debug positions
        /// </summary>
        public void drawDebug()
        {
            int cc = 0;

            activeBodies = 0;

            // Draw all shapes
            foreach (RigidBody body in World.RigidBodies)
            {
                if (body.IsActive) activeBodies++;
                if (body.Tag is int || body.IsParticle) continue;
                AddBodyToDrawList(body);
            }

            foreach (Constraint constr in World.Constraints)
                constr.DebugDraw(DebugDrawer);

            foreach (RigidBody body in World.RigidBodies)
            {
                DebugDrawer.Color = rndColors[cc % rndColors.Length];
                body.DebugDraw(DebugDrawer);
                cc++;
            }

            //foreach (Primitives3D.GeometricPrimitive prim in primitives) prim.Draw(m_context.m_physicsEffect);

            //GraphicsDevice.RasterizerState = cullMode;
        }

        private int activeBodies = 0;
        protected RigidBody m_ground = null;
        private QuadDrawer m_quadDrawer = null;

        public void addGround(Game game)
        {
            m_ground = new RigidBody(new BoxShape(new JVector(200, 20, 200)));
            m_ground.Position = new JVector(0, -10, 0);
            m_ground.Tag = BodyTag.DontDrawMe;
            m_ground.Tag = 
            m_ground.IsStatic = true;
            m_ground.EnableDebugDraw = true;
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


        protected XygloContext m_context;

        private Primitives3D.GeometricPrimitive[] primitives = new Primitives3D.GeometricPrimitive[5];

        private enum Primitives { box, sphere, cylinder, cone, capsule }
    }
}
