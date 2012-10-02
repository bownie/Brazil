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
        public Paulo():base()
        {
        }

        /// <summary>
        /// Allows the game to perform any initialisation of objects and positions before starting.
        /// </summary>
        public override void initialise()
        {
            // Initialise the states
            //
            initialiseStates();

            // Connect some keys
            //
            //connectEditorKeys(State.Test("TextEditing"));

            // Connect up a specific transition
            //
            connectKey(State.Test("Menu"), Keys.D2, Target.getTarget("StartPlaying"));

            // Set up some test blocks
            //
            testBlocks();

            // Set up an interloper
            //
            Interloper paulo = new Interloper(BrazilColour.White, new BrazilVector3(0, 0, 0), new BrazilVector3(10, 10, 10));
            //paulo.setVelocity(new BrazilVector3(0.1f, 0, 0));
            addComponent(State.Test("PlayingGame"), paulo);

            BannerText menuScreen = new BannerText(BrazilColour.Blue, BrazilVector3.Zero, 10.0, "Paulo");
            addComponent(State.Test("Menu"), menuScreen);
        }


        /// <summary>
        /// Initialise the states - might find a way to genericise this
        /// </summary>
        protected void initialiseStates()
        {
            // States of the application - where are we in the navigation around the app.  States will affect what 
            // components are showing and how we interact with them.
            //
            string[] states = { "Menu", "PlayingGame" };
            foreach (string state in states)
            {
                addState(state);
            }

            // Targets are actions we can perform in the application - a default target will usually mean the object
            // in focus within the current State.  This should be defined by the component.
            //
            string[] targets = { "StartPlaying", "Exit", "MoveLeft", "MoveRight", "Jump", "MoveForward", "MoveBack" };
            foreach (string target in targets)
            {
                addTarget(target);
            }

            // Confirm states are substates used when confirming actions or movements between states.  These
            // give greater control over movement between States.
            //
            string[] confirmStates = { };
            foreach (string confirmState in confirmStates)
            {
                addConfirmState(confirmState);
            }
        }

        /// <summary>
        /// Set up some test blocks
        /// </summary>
        protected void testBlocks()
        {
            FlyingBlock block1 = new FlyingBlock(BrazilColour.Blue, new BrazilVector3(-10, -100, 0), new BrazilVector3(100.0f, 100.0f, 10.0f));
            //block1.setVelocity(new BrazilVector3(-1, 0, 0));

            // Push onto component list
            //
            //m_componentList.Add(block1);
            addComponent(State.Test("PlayingGame"), block1);

            FlyingBlock block2 = new FlyingBlock(BrazilColour.Brown, new BrazilVector3(0, 0, 0), new BrazilVector3(100, 20, 20));
            //block2.setVelocity(new BrazilVector3(0.5f, 0, 0.1f));
            //m_componentList.Add(block2);
            addComponent(State.Test("PlayingGame"), block2);

            FlyingBlock block3 = new FlyingBlock(BrazilColour.Orange, new BrazilVector3(-30, 50, 200), new BrazilVector3(20, 5, 10));
            block3.setRotation(0.01);
            //m_componentList.Add(block3);
            addComponent(State.Test("PlayingGame"), block3);

            FlyingBlock block4 = new FlyingBlock(BrazilColour.Green, new BrazilVector3(50, -50, -800), new BrazilVector3(20, 20, 200));
            block4.setRotation(0.03);
            //block4.setVelocity(new BrazilVector3(-0.5f, -1f, -0.4f));
            //m_componentList.Add(block4);
            addComponent(State.Test("PlayingGame"), block4);
        }

    }
}
