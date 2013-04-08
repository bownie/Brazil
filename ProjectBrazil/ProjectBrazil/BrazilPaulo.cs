using System;
using System.Collections.Generic;
using System.Linq;
using Xyglo.Brazil;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{
    /// <summary>
    /// This is the top level class for our Paulo game.  It inherits a BrazilApp object which
    /// inherits the XNA Game implementation.
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com")]
    [KnownType(typeof(BrazilApp))]
    public class BrazilPaulo : BrazilApp
    {
        /// <summary>
        /// BrazilPaulo constructor - note we used the Hosted mode so we don't reinitialise
        /// anything at the drawing level.
        /// </summary>
        public BrazilPaulo(BrazilVector3 gravity, string homePath): base(homePath, BrazilAppMode.Hosted)
        {
            m_world.setGravity(gravity);
            m_world.setBounds(new BrazilBoundingBox(new BrazilVector3(-400, -400, -300), new BrazilVector3(400, 400, 300)));

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
            setState(state);

            // Connect some keys
            //
            setupConnections();

            // Generate our components for our levels and other things blocks
            //
            generateComponents();

            // Set up an interloper - we give it a name to aid debugging
            //
            BrazilInterloper paulo = new BrazilInterloper(BrazilColour.White, new BrazilVector3(0, 0, 0), new BrazilVector3(10, 10, 10));
            paulo.setName("Interloper");
            addComponent("PlayingGame", paulo);

            // Big dummy interloper
            //
            // Set up an interloper - we give it a name to aid debugging
            //
            BrazilInterloper paulo2 = new BrazilInterloper(BrazilColour.Yellow, new BrazilVector3(0, 0, 0), new BrazilVector3(100, 100, 100));
            paulo.setName("Interloper2");
            addComponent("ComponentTest", paulo2);

            // Banner screen
            //
            //BrazilBannerText menuScreen = new BrazilBannerText(BrazilColour.Blue, new BrazilVector3(360.0f, 240.0f, 0), 4.0, "Paulo");
            BrazilBannerText menuScreen = new BrazilBannerText(BrazilColour.Blue, BrazilPosition.Middle, 4.0, "Paulo");
            addComponent("Menu", menuScreen);

            //BrazilBannerText byLine = new BrazilBannerText(BrazilColour.Yellow, new BrazilVector3(430.0f, 345.0f, 0), 1.0, "@xyglo");
            BrazilBannerText byLine = new BrazilBannerText(BrazilColour.Yellow, BrazilRelativePosition.BeneathCentred, menuScreen, 0, 1.0f, "@xyglo");
            addComponent("Menu", byLine);

            //BrazilBannerText toPlay = new BrazilBannerText(BrazilColour.White, new BrazilVector3(380.0f, 500.0f, 0), 1.0, "Hit '1' to Play");
            BrazilBannerText toPlay = new BrazilBannerText(BrazilColour.White, BrazilPosition.BottomMiddle, 1.0f, "Hit '1' to Play");
            addComponent("Menu", toPlay);
        }


        /// <summary>
        /// Initialise the states - might find a way to genericise this
        /// </summary>
        protected void initialiseStates()
        {
            // States of the application - where are we in the navigation around the app.  States will affect what 
            // components are showing and how we interact with them.
            //
            string[] states = { "Menu", "PlayingGame", "LevelComplete", "ComponentTest", "GameOver" };
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

            // Connect up our test state
            //
            connectKey("ComponentTest", Keys.Escape, "QuitToMenu");
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
            addResource("logo", "xyglo-logo.png", ResourceType.Image, ResourceMode.Centre, block1);
            addComponent("PlayingGame", block1);

            /*
            BrazilFlyingBlock block2 = new BrazilFlyingBlock(BrazilColour.Brown, new BrazilVector3(0, 0, 0), new BrazilVector3(100, 20, 20));
            //block2.setVelocity(new BrazilVector3(0.5f, 0, 0.1f));
            //m_componentList.Add(block2);
            addComponent("PlayingGame", block2);
            */
            BrazilFlyingBlock block3 = new BrazilFlyingBlock(BrazilColour.Orange, new BrazilVector3(0, 150, 0), new BrazilVector3(200, 50, 0));
            block3.setRotation(0.2);
            block3.setHardness(10);
            block3.setName("LandingBlock1");
            addComponent("PlayingGame", block3);
            

            BrazilFlyingBlock block4 = new BrazilFlyingBlock(BrazilColour.Green, new BrazilVector3(0, 300, 0), new BrazilVector3(100, 10, 100));
            //block4.setRotation(0.03);
            //block4.setVelocity(new BrazilVector3(-0.5f, -1f, -0.4f));
            //m_componentList.Add(block4);
            block4.setHardness(10);
            block4.setMoveable(false);
            block4.setName("LandingBlock2");
            block4.setInitialAngle(Math.PI / 8);
            addComponent("PlayingGame", block4);
            

            // Setup the HUD
            //
            BrazilHud hud = new BrazilHud(BrazilColour.White, BrazilVector3.Zero, 1.0, "HUD");
            addComponent("PlayingGame", hud);

            // Add the finishing block
            //
            //BrazilFinishBlock finishBlock = new BrazilFinishBlock(BrazilColour.Pink, new BrazilVector3(0, 230, 0), new BrazilVector3(200, 10, 10), getState("LevelComplete"));
            //block4.setHardness(10);
            //addComponent("PlayingGame", finishBlock);

            /*
            // Add some prizes
            //
            BrazilGoody goody1 = new BrazilGoody(BrazilGoodyType.Coin, 50, new BrazilVector3(0, 100, 0), new BrazilVector3(5, 10, 10), DateTime.Now);
            goody1.setRotation(0.2);
            addComponent("PlayingGame", goody1);

            // Test state for new components
            //
            BrazilGoody goody2 = new BrazilGoody(BrazilGoodyType.Coin, 50, new BrazilVector3(0, 100, 0), new BrazilVector3(100, 50, 50), DateTime.Now);
            goody2.setRotation(0.01);
            addComponent("ComponentTest", goody2);

            for (int i = 0; i < 10; i++)
            {
                BrazilVector3 position = new BrazilVector3(-100.0f + (i * 12.0f), 120, 0); 
                BrazilGoody newGoody = new BrazilGoody(BrazilGoodyType.Coin, 50, position, new BrazilVector3(4, 10, 10), DateTime.Now);
                newGoody.setRotation(0.2);
                addComponent("PlayingGame", newGoody);
            }

            BrazilBannerText gameOverText = new BrazilBannerText(BrazilColour.Red, BrazilPosition.Middle, 2.0f, "GAME OVER");
            addComponent("GameOver", gameOverText);
             * */
        }
    }
}
