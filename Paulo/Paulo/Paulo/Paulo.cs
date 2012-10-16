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
            addComponent("PlayingGame", paulo);

            // Banner screen
            //
            BannerText menuScreen = new BannerText(BrazilColour.Blue, new BrazilVector3(360.0f, 240.0f, 0), 4.0, "Paulo");
            addComponent("Menu", menuScreen);

            BannerText byLine = new BannerText(BrazilColour.Yellow, new BrazilVector3(430.0f, 345.0f, 0), 1.0, "@xyglo");
            addComponent("Menu", byLine);

            BannerText toPlay = new BannerText(BrazilColour.White, new BrazilVector3(380.0f, 500.0f, 0), 1.0, "Hit '1' to Play");
            addComponent("Menu", toPlay);

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
            connectKey("Menu", Keys.D1, "StartPlaying");

            // Connect up the escape key to exit the game or into the menu
            //
            connectKey("Menu", Keys.Escape, "Exit");
            connectKey("PlayingGame", Keys.Escape, "QuitToMenu");

            // Connect up the Interloper
            //
            connectKey("PlayingGame", Keys.Left, "MoveLeft");
            connectKey("PlayingGame", Keys.Right, "MoveRight");
            connectKey("PlayingGame", Keys.Up, "MoveForward");
            connectKey("PlayingGame", Keys.Down, "MoveBack");

            connectKey("PlayingGame", Keys.Left, KeyButtonState.Held, "MoveLeft");
            connectKey("PlayingGame", Keys.Right, KeyButtonState.Held, "MoveRight");

            connectKey("PlayingGame", Keys.Space, "Jump");
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
            addComponent("PlayingGame", block1);

            BrazilFlyingBlock block2 = new BrazilFlyingBlock(BrazilColour.Brown, new BrazilVector3(0, 0, 0), new BrazilVector3(100, 20, 20));
            //block2.setVelocity(new BrazilVector3(0.5f, 0, 0.1f));
            //m_componentList.Add(block2);
            addComponent("PlayingGame", block2);

            BrazilFlyingBlock block3 = new BrazilFlyingBlock(BrazilColour.Orange, new BrazilVector3(0, 150, 0), new BrazilVector3(200, 50, 0));
            //block3.setRotation(0.01);
            block3.setHardness(10);
            //m_componentList.Add(block3);
            block3.setName("LandingBlock1");
            addComponent("PlayingGame", block3);

            BrazilFlyingBlock block4 = new BrazilFlyingBlock(BrazilColour.Green, new BrazilVector3(-170, 170, 0), new BrazilVector3(70, 20, 10));
            //block4.setRotation(0.03);
            //block4.setVelocity(new BrazilVector3(-0.5f, -1f, -0.4f));
            //m_componentList.Add(block4);
            block4.setHardness(10);
            block4.setName("LandingBlock2");
            block4.setInitialAngle(Math.PI / 8);
            addComponent("PlayingGame", block4);

            // Setup the HUD
            //
            BrazilHud hud = new BrazilHud(BrazilColour.White, BrazilVector3.Zero, 1.0, "HUD");
            addComponent("PlayingGame", hud);
        }
    }
}
