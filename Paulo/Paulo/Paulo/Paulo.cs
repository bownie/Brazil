using System;
using System.Collections.Generic;
using System.Linq;
using Xyglo.Brazil;

namespace Paulo
{
    /// <summary>
    /// This is the top level class for our Paulo game.  It inherits a BrazilApp object which
    /// inherits the XNA Game implementation.
    /// </summary>
    public class Paulo : BrazilApp
    {
        public Paulo()
        {
        }

        /// <summary>
        /// Allows the game to perform any initialisation of objects and positions before starting.
        /// </summary>
        public override void initialise()
        {
            connectEditorKeys(State.TextEditing);

            testBlocks();

            Interloper paulo = new Interloper(BrazilColour.White, new BrazilVector3(0, 0, 0), new BrazilVector3(100, 100, 100));
            paulo.setVelocity(new BrazilVector3(0.1f, 0, 0));
            m_componentList.Add(paulo);
        }


        protected void testBlocks()
        {
            FlyingBlock block1 = new FlyingBlock(BrazilColour.Blue, new BrazilVector3(-10, -100, 0), new BrazilVector3(100.0f, 100.0f, 10.0f));
            block1.setVelocity(new BrazilVector3(-1, 0, 0));

            // Push onto component list
            //
            m_componentList.Add(block1);

            FlyingBlock block2 = new FlyingBlock(BrazilColour.Brown, new BrazilVector3(0, 0, 0), new BrazilVector3(100, 20, 20));
            block2.setVelocity(new BrazilVector3(0.5f, 0, 0.1f));
            m_componentList.Add(block2);

            FlyingBlock block3 = new FlyingBlock(BrazilColour.Orange, new BrazilVector3(-30, 50, 200), new BrazilVector3(20, 5, 10));
            block3.setRotation(0.01);
            m_componentList.Add(block3);

            FlyingBlock block4 = new FlyingBlock(BrazilColour.Green, new BrazilVector3(50, -50, -800), new BrazilVector3(20, 20, 200));
            block4.setRotation(0.03);
            block4.setVelocity(new BrazilVector3(-0.5f, -1f, -0.4f));
            m_componentList.Add(block4);
        }

    }
}
