using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
//using JitterDemo.Vehicle;


namespace Xyglo.Brazil.Xna.Physics
{
    /// <summary>
    /// A Scene in Jitter is an abstract base class - here we're going to
    /// have a dynamically built scene which operates on a selection of 
    /// XygloXnaDrawables we pass to it.
    /// </summary>
    public class Scene
    {
        public PhysicsHandler m_handler { get; private set; }

        protected RigidBody m_ground = null;

        public Scene(PhysicsHandler handler)
        {
            this.m_handler = handler;
        }


        public void addGround()
        {
            m_ground = new RigidBody(new BoxShape(new JVector(200, 20, 200)));
            m_ground.Position = new JVector(0, -10, 0);
            //m_ground.Tag = BodyTag.DontDrawMe;
            m_ground.IsStatic = true;
            m_handler.m_world.AddBody(m_ground);
            //ground.Restitution = 1.0f;
            m_ground.Material.KineticFriction = 0.0f;

            //quadDrawer = new QuadDrawer(Demo, 100);
            //Demo.Components.Add(quadDrawer);
        }

        public virtual void build()
        {
            addGround();
        }

        //private QuadDrawer quadDrawer = null;
        //protected RigidBody ground = null;
        //protected CarObject car = null;

        /*
        public void AddGround()
        {
            ground = new RigidBody(new BoxShape(new JVector(200, 20, 200)));
            ground.Position = new JVector(0, -10, 0);
            ground.Tag = BodyTag.DontDrawMe;
            ground.IsStatic = true; Demo.World.AddBody(ground);
            //ground.Restitution = 1.0f;
            ground.Material.KineticFriction = 0.0f;

            quadDrawer = new QuadDrawer(Demo, 100);
            PhysicsHandler.Components.Add(quadDrawer);
        }

        public void RemoveGround()
        {
            Demo.World.RemoveBody(ground);
            PhysicsHandler.Components.Remove(quadDrawer);
            quadDrawer.Dispose();
        }

        public void AddCar(JVector position)
        {
            //car = new CarObject(Demo);
            PhysicsHandler.Demo.Components.Add(car);

            car.carBody.Position = position;
        }

        public void RemoveCar()
        {
            Demo.World.RemoveBody(car.carBody);
            Demo.Components.Remove(quadDrawer);
            Demo.Components.Remove(car);
        }

        public virtual void Draw() { }
        */

    }
}
