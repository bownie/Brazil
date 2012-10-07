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
        /// <summary>
        /// Default constructor
        /// </summary>
        public Paulo(BrazilVector3 gravity):base()
        {
            m_world.setGravity(gravity);

            // Initialise the states
            //
            initialiseStates();
        }

        /// <summary>
        /// Allows the game to perform any initialisation of objects and positions before starting.
        /// </summary>
        public override void initialise(State state)
        {
            // Now set the initial state
            //
            setInitialState(state);

            // Connect some keys
            //
            setupConnections();

            // Generate our components for our levels and other things blocks
            //
            generateComponents();

            // Set up an interloper
            //
            BrazilInterloper paulo = new BrazilInterloper(BrazilColour.White, new BrazilVector3(0, 0, 0), new BrazilVector3(10, 10, 10));
            //paulo.setVelocity(new BrazilVector3(0.1f, 0, 0));
            addComponent(State.Test("PlayingGame"), paulo);

            // Banner screen
            //
            BannerText menuScreen = new BannerText(BrazilColour.Blue, new BrazilVector3(360.0f, 240.0f, 0), 4.0, "Paulo");
            addComponent(State.Test("Menu"), menuScreen);

            BannerText byLine = new BannerText(BrazilColour.Yellow, new BrazilVector3(340.0f, 345.0f, 0), 1.0, "By Richard Bown (@xyglo)");
            addComponent(State.Test("Menu"), byLine);

            BannerText toPlay = new BannerText(BrazilColour.White, new BrazilVector3(380.0f, 500.0f, 0), 1.0, "Hit '1' to Play");
            addComponent(State.Test("Menu"), toPlay);

            //BannerText toPlay = new BannerText(BrazilColour.White, new BrazilVector3(350.0f, 500.0f, 0), 1.0, "1 - Play the Game");
            //addComponent(State.Test("Menu"), toPlay);
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
            string[] targets = { "StartPlaying", "QuitToMenu", "Exit", "MoveLeft", "MoveRight", "Jump", "MoveForward", "MoveBack" };
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
        /// Set up the key and mouse connections
        /// </summary>
        protected void setupConnections()
        {
            // Connect up a transition - this key connection starts the game for us
            //
            connectKey(State.Test("Menu"), Keys.D1, Target.getTarget("StartPlaying"));

            // Connect up the escape key to exit the game or into the menu
            //
            connectKey(State.Test("Menu"), Keys.Escape, Target.getTarget("Exit"));
            connectKey(State.Test("PlayingGame"), Keys.Escape, Target.getTarget("QuitToMenu"));

            // Connect up the Interloper
            //
            connectKey(State.Test("PlayingGame"), Keys.Left, Target.getTarget("MoveLeft"));
            connectKey(State.Test("PlayingGame"), Keys.Right, Target.getTarget("MoveRight"));
            connectKey(State.Test("PlayingGame"), Keys.Up, Target.getTarget("MoveForward"));
            connectKey(State.Test("PlayingGame"), Keys.Down, Target.getTarget("MoveBack"));

            connectKey(State.Test("PlayingGame"), Keys.Left, KeyButtonState.Held, Target.getTarget("MoveLeft"));
            connectKey(State.Test("PlayingGame"), Keys.Right, KeyButtonState.Held, Target.getTarget("MoveRight"));

            connectKey(State.Test("PlayingGame"), Keys.Space, Target.getTarget("Jump"));
        }


        /// <summary>
        /// Set up some test blocks
        /// </summary>
        protected void generateComponents()
        {
            BrazilFlyingBlock block1 = new BrazilFlyingBlock(BrazilColour.Blue, new BrazilVector3(-10, -100, 0), new BrazilVector3(200.0f, 100.0f, 10.0f));
            //block1.setVelocity(new BrazilVector3(-1, 0, 0));

            // Push onto component list
            //
            //m_componentList.Add(block1);
            addComponent(State.Test("PlayingGame"), block1);

            BrazilFlyingBlock block2 = new BrazilFlyingBlock(BrazilColour.Brown, new BrazilVector3(0, 0, 0), new BrazilVector3(100, 20, 20));
            //block2.setVelocity(new BrazilVector3(0.5f, 0, 0.1f));
            //m_componentList.Add(block2);
            addComponent(State.Test("PlayingGame"), block2);

            BrazilFlyingBlock block3 = new BrazilFlyingBlock(BrazilColour.Orange, new BrazilVector3(0, 150, 0), new BrazilVector3(200, 50, 0));
            //block3.setRotation(0.01);
            block3.setHardness(10);
            //m_componentList.Add(block3);
            addComponent(State.Test("PlayingGame"), block3);

            BrazilFlyingBlock block4 = new BrazilFlyingBlock(BrazilColour.Green, new BrazilVector3(50, -50, -800), new BrazilVector3(20, 20, 200));
            block4.setRotation(0.03);
            //block4.setVelocity(new BrazilVector3(-0.5f, -1f, -0.4f));
            //m_componentList.Add(block4);
            addComponent(State.Test("PlayingGame"), block4);

            // Setup the HUD
            //
            BrazilHud hud = new BrazilHud(BrazilColour.White, BrazilVector3.Zero, 1.0, "HUD");
            addComponent(State.Test("PlayingGame"), hud);
        }

    }
}
