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

namespace Xyglo.Brazil.Xna.Physics
{
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
        public PhysicsHandler(XygloContext context)
        {
            m_context = context;

            // Set up collision and world for physics
            //
            CollisionSystem collision = new CollisionSystemPersistentSAP();
            World = new World(collision); World.AllowDeactivation = true;
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
                    drawableList[0].setPosition(Conversion.ToXNAVector(body.Position));
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

                JVector size = Conversion.ToJitterVector(fb.getSize());
                RigidBody body = new RigidBody(new BoxShape(size));
                body.Position = Conversion.ToJitterVector(fb.getPosition());
                body.AffectedByGravity = false;

                // set any velocity
                //
                // Conversion.ToXNAVector
                //
                body.LinearVelocity = Conversion.ToJitterVector(fb.getVelocity());
                
                // Store this relationship in the calling drawable so we can link them back again
                //
                drawable.setPhysicsHash(body.GetHashCode());

                World.AddBody(body);
            }
        }

        //GraphicsDeviceManager m_graphics;
        //ContentManager content;

        //Model boxModel, sphereModel, capsuleModel, compoundModel, terrainModel, cylinderModel, carModel, wheelModel, staticModel, planeModel, pinModel;

        //PhysicsSystem physicSystem;

        //DebugDrawer debugDrawer;
        //Camera camera;

        //CarObject carObject;

        //ConstraintWorldPoint objectController = new ConstraintWorldPoint();
        //ConstraintVelocity damperController = new ConstraintVelocity();


        protected XygloContext m_context;
    }
}
