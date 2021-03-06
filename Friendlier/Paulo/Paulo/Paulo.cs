using System;
using System.Collections.Generic;
using System.Linq;
using Xyglo.Brazil;

namespace Xyglo.Friendlier
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
        public Paulo(BrazilVector3 gravity, string homePath):base(homePath)
        {
            m_world.setGravity(gravity);
            m_world.setBounds(new BrazilBoundingBox(new BrazilVector3(-400, -400, -50), new BrazilVector3(3000, 800, 100)));

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
            //testBlocks();

            // Big dummy interloper
            //
            // Set up an interloper - we give it a name to aid debugging
            //
            //BrazilInterloper paulo2 = new BrazilInterloper(BrazilColour.Yellow, new BrazilVector3(0, 0, 0), new BrazilVector3(100, 100, 100));
            //paulo.setName("Interloper2");
            //addComponent("ComponentTest", paulo2);

            // Banner screen
            //
            BrazilBannerText menuScreen = new BrazilBannerText(BrazilColour.Blue, BrazilPosition.Middle, 4.0, "Paulo");
            addComponent("Menu", menuScreen);

            BrazilBannerText byLine = new BrazilBannerText(BrazilColour.Yellow, BrazilRelativePosition.BeneathCentred, menuScreen, 0, 1.0f, "@xyglo");
            addComponent("Menu", byLine);

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
            string[] states = { "Menu", "PlayingGame", "LevelComplete", "ComponentTest" };
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

        protected void testBlocks()
        {
            BrazilTestBlock testBlock1 = new BrazilTestBlock(BrazilColour.Pink, new BrazilVector3(0, 0, 0), new BrazilVector3(40, 40, 40));
            //testBlock1.setAffectedByGravity(false);
            addResource("logo", "xyglo-logo.png", ResourceType.Image, ResourceMode.Centre, testBlock1);
            addComponent("PlayingGame", testBlock1);

            BrazilTestBlock testBlock2 = new BrazilTestBlock(BrazilColour.Pink, new BrazilVector3(-120, 0, 0), new BrazilVector3(40, 40, 40));
            testBlock2.setAffectedByGravity(false);
            testBlock2.setRotation(2.0f);
            testBlock2.setHardness(1.0f);
            addComponent("PlayingGame", testBlock2);

            BrazilFlyingBlock testFlyingBlock1 = new BrazilFlyingBlock(BrazilColour.White, new BrazilVector3(-200, 0, 0), new BrazilVector3(40, 40, 40));
            testFlyingBlock1.setAffectedByGravity(true);
            testFlyingBlock1.setHardness(1.0f);
            addComponent("PlayingGame", testFlyingBlock1);

            BrazilTestBlock testBlock3 = new BrazilTestBlock(BrazilColour.Pink, new BrazilVector3(-200, 0, 0), new BrazilVector3(40, 40, 40));
            //testBlock2.setAffectedByGravity(false);
            testBlock3.setHardness(1.0f);
            addComponent("PlayingGame", testBlock3);
        }

        /// <summary>
        /// Set up some test blocks
        /// </summary>
        protected void generateComponents()
        {
            // Setup the HUD
            //
            BrazilHud hud = new BrazilHud(BrazilColour.White, BrazilVector3.Zero, 1.0, "HUD");
            addComponent("PlayingGame", hud);

            // Set up an interloper - we give it a name to aid debugging
            //
            BrazilInterloper paulo = new BrazilInterloper(BrazilColour.White, new BrazilVector3(0, 0, 0), new BrazilVector3(10, 10, 10));
            paulo.setName("Interloper");
            paulo.setAffectedByGravity(true);
            paulo.setMoveable(true);
            paulo.setVelocity(new BrazilVector3(250, 0, 0));
            addComponent("PlayingGame", paulo);


            BrazilFlyingBlock block1 = new BrazilFlyingBlock(BrazilColour.Blue, new BrazilVector3(0, 200, 0), new BrazilVector3(600, 20, 10));
            block1.setAffectedByGravity(false);
            //block1.setHardness(0.3f);
            block1.setMoveable(false);
            addResource("logo", "xyglo-logo.png", ResourceType.Image, ResourceMode.Centre, block1);
            addComponent("PlayingGame", block1);

            // Add another block below 
            //
            BrazilFlyingBlock block2 = new BrazilFlyingBlock(BrazilColour.Blue, new BrazilVector3(630, 260, 0), new BrazilVector3(600, 20, 10));
            block2.setMoveable(false);
            addComponent("PlayingGame", block2);

            //BrazilBaddy baddy = new BrazilBaddy(BrazilColour.Brown, new BrazilVector3(20, 80, 0), new BrazilVector3(20, 20, 20));
            //addComponent("PlayingGame", baddy);


            // Invisible walls front and back
            //
            BrazilInvisibleBlock invisibleWallFront = new BrazilInvisibleBlock(new BrazilVector3(-1000, -1000, 10), new BrazilVector3(2000, 2000, 10));
            //addComponent("PlayingGame", invisibleWallFront);

            BrazilInvisibleBlock invisibleWallBack = new BrazilInvisibleBlock(new BrazilVector3(-1000, -1000, -20), new BrazilVector3(2000, 2000, 10));
            //addComponent("PlayingGame", invisibleWallBack);

            /*
            BrazilFlyingBlock block2 = new BrazilFlyingBlock(BrazilColour.Brown, new BrazilVector3(0, 0, 0), new BrazilVector3(100, 20, 20));
            block2.setAffectedByGravity(false);
            block2.setHardness(10);
            addComponent("PlayingGame", block2);
           */

           
            BrazilFlyingBlock block3 = new BrazilFlyingBlock(BrazilColour.Orange, new BrazilVector3(0, 150, 0), new BrazilVector3(200, 50, 0));
            //block3.setRotation(0.01);
            block3.setHardness(10);
            block3.setName("LandingBlock1");
            addComponent("PlayingGame", block3);

            BrazilFlyingBlock block4 = new BrazilFlyingBlock(BrazilColour.Green, new BrazilVector3(-170, 170, 0), new BrazilVector3(70, 20, 10));
            block4.setRotation(0.03);
            //block4.setVelocity(new BrazilVector3(-0.5f, -1f, -0.4f));
            //m_componentList.Add(block4);
            block4.setHardness(10);
            block4.setName("LandingBlock2");
            block4.setInitialAngle(Math.PI / 8);
            addComponent("PlayingGame", block4);

            // Add the finishing block
            //
            BrazilFinishBlock finishBlock = new BrazilFinishBlock(BrazilColour.Pink, new BrazilVector3(0, -500, 0), new BrazilVector3(200, 10, 10), getState("LevelComplete"));
            addComponent("PlayingGame", finishBlock);

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

            // Setup some coins
            //
            for (int i = 0; i < 10; i++)
            {
                BrazilVector3 position = new BrazilVector3(-100.0f + (i * 12.0f), 120, 0); 
                BrazilGoody newGoody = new BrazilGoody(BrazilGoodyType.Coin, 50, position, new BrazilVector3(4, 10, 10), DateTime.Now);
                newGoody.setRotation(0.2);
                addComponent("PlayingGame", newGoody);
            }

        }

        /// <summary>
        /// Provide a control interface for this app
        /// </summary>
        /// <param name="command"></param>
        //public override void sendCommand(BrazilAppControl command)
        //{
        //}

    }
}
