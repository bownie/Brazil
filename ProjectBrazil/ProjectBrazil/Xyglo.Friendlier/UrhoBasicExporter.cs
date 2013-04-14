using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xyglo.Brazil;
using Xyglo.Brazil.Xna;

namespace Xyglo.Friendlier
{
    /// <summary>
    /// Create the necessay urho3d scripts/script to regenerate the exported app in Uhro.
    /// For this we need to create the AngelScript and copy that and the relevant assets to
    /// the correct build directory for packaging.  Then a packaging command can be kicked
    /// off and the app loaded on to device/simulator as necessary.
    /// 
    /// The exporter has to overlay these files on top of a supplied template.  For the moment
    /// we roll all included files into a monolithic blob of AngelScript output.
    /// 
    /// </summary>
    public class UrhoBasicExporter : ScriptExporter
    {
        /// <summary>
        /// As part of the constructor ensure that the supplied project directory structure is correct.
        /// Only then can we export the Urho file.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="directory"></param>
        public UrhoBasicExporter(XygloContext context, BrazilApp app, string outputDirectory, string templateDirectory)
            : base(app, outputDirectory, templateDirectory)
        {
            m_context = context;
            checkProjectStructure();
        }

        /// <summary>
        /// Urho is expecting a directory structure.  Project top level:
        /// 
        /// - assets
        /// - bin
        /// - libs
        /// - res
        /// - src
        /// 
        /// Also certain files listed 
        /// </summary>
        protected void checkProjectStructure()
        {
            // Check project directory
            //
            if (!checkDir(m_projectDirectory))
            {
                throw new Exception("Couldn't access project directory " + m_projectDirectory);
            }

            string[] dirsToCheck = { "assets", @"assets\Data", @"assets\Data\Scripts", @"assets\Data\Scenes", "bin", "libs", "res", "src" };
            foreach(string dir in dirsToCheck)
            {
                string dirToCheck = m_projectDirectory + @"\" + dir;
                if (!checkDir(dirToCheck))
                {
                    throw new Exception("Couldn't find required directory " + dirToCheck);
                }
            }
        }

        /// <summary>
        /// Export the app - various stages to this:
        /// 
        /// - generate AndroidManifest.xml depending on States defined (States equate to Activities)
        ///  [the xml needs to point to resources which need to be loaded]
        /// - generate java that hooks up the Activities
        /// - generates any AngelScript for Urho3D if there is a 3D implementation
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public override void export()
        {
            string exportFile = m_projectDirectory + @"\assets\Data\" + m_urhoExportFile;

            // Test to see if this file is writeable
            //
            try
            {
                testWriteable(exportFile);
            }
            catch(Exception e)
            {
                throw new Exception("Could not create export file " + exportFile + " with " + e.Message);
            }

            // We need to generate some other files as part of this export - set path so that references work
            //
            m_gameSceneXML = m_projectDirectory + @"\assets\Data\Scenes\Scene.xml";  // equiv of NinjaSnowWar.xml

            // First the main export file - header
            //
            writeAll(exportFile, buildHeader());

            // Import some classes
            //
            appendFile(exportFile, m_templateDirectory + @"\template_parts\Scripts\Player.as");

            // Go and generate
            //
            appendText(exportFile, buildBody());

            // Add a template add-on
            //
            appendFile(exportFile, m_templateDirectory + @"\template_parts\Scripts\Handlers.as");

            // Now generate the scene.xml
            //
            appendFile(m_gameSceneXML, m_templateDirectory + @"\template_parts\Scenes\Scene.xml");

            // Now we generate or copy and required asset files
            //
        }

        /// <summary>
        /// Generate any AngelScript as required by the components in the app.  We use this reference to
        /// generate our AngelScript output:
        /// 
        /// http://code.google.com/p/urho3d/wiki/Scripting
        /// 
        /// </summary>
        /// <param name="directory"></param>
        protected string buildHeader()
        {
            // Header with comment
            //
            string rS = @"// Friendlier v" + VersionInformation.getProductVersion() + " export to AngelScript for Urho3d\n";
            rS += "//\n// Automatically generated from project \"" + m_context.m_project.m_projectName + "\"";
            rS += " on " + System.DateTime.Now.ToShortDateString() + " " + System.DateTime.Now.ToShortTimeString() + "\n//\n\n";

            return rS;
        }

        /// <summary>
        /// Build the body for output
        /// </summary>
        /// <param name="indent"></param>
        /// <returns></returns>
        protected string buildBody(string indent = "")
        {
            // Insert some globals
            //
            //string indent = "";

            string rS = Globals(indent);

            // Roll in all these standard method calls whether then return anything or not
            //
            rS += Start(indent);
            rS += Stop(indent);
            rS += DelayedStart(indent);
            rS += Update(indent);
            rS += PostUpdate(indent);
            rS += FixedUpdate(indent);
            rS += FixedPostUpdate(indent);
            rS += Save(indent);
            rS += Load(indent);
            rS += WriteNetworkUpdate(indent);
            rS += ReadNetworkUpdate(indent);
            rS += ApplyAttributes(indent);

            // If there are components spread across more than one state then we can't do this export
            // so first check the state of all the components.
            //
            foreach (Component component in m_app.getComponents())
            {
                ;
            }

            return rS;
        }

        /// <summary>
        /// Global variable definitions
        /// </summary>
        /// <returns></returns>
        protected string Globals(string indent)
        {
            string rS = "";

            rS += indent + "// Globals\n//\n";
            rS += indent + "const float mouseSensitivity = 0.125;\n";
            rS += indent + "const float touchSensitivity = 2.0;\n";
            rS += indent + "const float joySensitivity = 0.5;\n";
            rS += indent + "const float joyMoveDeadZone = 0.333;\n";
            rS += indent + "const float joyLookDeadZone = 0.05;\n";
            rS += indent + "const float cameraMinDist = 25;\n";
            rS += indent + "const float cameraMaxDist = 500;\n";
            rS += indent + "const float cameraSafetyDist = 30;\n";
            rS += indent + "const int initialMaxEnemies = 5;\n";
            rS += indent + "const int finalMaxEnemies = 25;\n";
            rS += indent + "const int maxPowerups = 5;\n";
            rS += indent + "const int incrementEach = 10;\n";
            rS += indent + "const int playerHealth = 20;\n";
            rS += indent + "const float enemySpawnRate = 1;\n";
            rS += indent + "const float powerupSpawnRate = 15;\n";
            rS += indent + "const float spawnAreaSize = 500;\n";

            rS += indent + "Scene@ gameScene;\n";
            rS += indent + "Node@ gameCameraNode;\n";
            rS += indent + "Camera@ gameCamera;\n";
            rS += indent + "Text@ scoreText;\n";
            rS += indent + "Text@ hiscoreText;\n";
            rS += indent + "Text@ messageText;\n";
            rS += indent + "BorderImage@ healthBar;\n";
            rS += indent + "BorderImage@ sight;\n";
            rS += indent + "BorderImage@ moveButton;\n";
            rS += indent + "BorderImage@ fireButton;\n";
            rS += indent + "SoundSource@ musicSource;\n";

            rS += indent + "Controls playerControls;\n";
            rS += indent + "Controls prevPlayerControls;\n";
            rS += indent + "bool singlePlayer = true;\n";
            rS += indent + "bool gameOn = false;\n";
            rS += indent + "bool drawDebug = false;\n";
            rS += indent + "bool drawOctreeDebug = false;\n";
            rS += indent + "int maxEnemies = 0;\n";
            rS += indent + "int incrementCounter = 0;\n";
            rS += indent + "float enemySpawnTimer = 0;\n";
            rS += indent + "float powerupSpawnTimer = 0;\n";
            rS += indent + "uint clientNodeID = 0;\n";
            rS += indent + "int clientScore = 0;\n";

            rS += indent + "bool touchEnabled = false;\n";
            rS += indent + "int touchButtonSize = 96;\n";
            rS += indent + "int touchButtonBorder = 12;\n";
            rS += indent + "int moveTouchID = -1;\n";
            rS += indent + "int rotateTouchID = -1;\n";
            rS += indent + "int fireTouchID = -1;\n";

            rS += indent + "Array<Player> players;\n";
            rS += indent + "Array<HiscoreEntry> hiscores;\n";
            rS += "\n\n";
            return rS;
        }

        /// <summary>
        /// Default Urho Method
        /// </summary>
        /// <returns></returns>
        protected string Start(string indent) 
        {
            string rS = indent + "// Start method\n//\n";

            rS += indent + "void " + getCurrentMethod() + "() {\n";
            rS += indent + m_tabSpace + "InitAudio();\n";
            rS += indent + m_tabSpace + "InitConsole();\n";
            rS += indent + m_tabSpace + "InitScene();\n";
            rS += indent + m_tabSpace + "InitNetworking();\n";
            rS += indent + m_tabSpace + "CreateCamera();\n";
            rS += indent + m_tabSpace + "CreateOverlays();\n\n";

            // Add any subscriptions
            //
            rS += doSubscriptions(indent + m_tabSpace);

            rS += indent + m_tabSpace + "if (touchEnabled) {\n";
            rS += indent + getTabPrefix(2) + @"SubscribeToEvent(""TouchBegin"", ""HandleTouchBegin"");"+ "\n";
            rS += indent + getTabPrefix(2) + @"SubscribeToEvent(""TouchEnd"", ""HandleTouchEnd"");" + "\n";
            rS += indent + m_tabSpace + "}\n";
            rS += indent + m_tabSpace + "if (singlePlayer) {\n";
            rS += indent + getTabPrefix(2) + "StartGame(null);\n";
            rS += indent + getTabPrefix(2) + "engine.pauseMinimized = true;\n";
            rS += indent + m_tabSpace + "}\n";
            rS += indent + "}\n\n";

            // Completes the Start method - now the callees
            //
            rS += InitAudio(indent);
            rS += InitConsole(indent);
            rS += InitScene(indent);
            rS += InitNetworking(indent);
            rS += CreateCamera(indent);
            rS += CreateOverlays(indent);


            rS += SetMessage(indent);

            rS += StartGame(indent);

            return rS;
        }

        /// <summary>
        /// Default Urho Method
        /// </summary>
        /// <returns></returns>
        protected string Stop(string indent) 
        {
            string rS = indent + "// Stop method\n//\n";
            rS += indent + "void " + getCurrentMethod() + "() {\n";

            rS += indent + "}\n\n";
            return rS;
        }

        /// <summary>
        /// Default Urho Method
        /// </summary>
        /// <returns></returns>
        protected string DelayedStart(string indent) 
        {
            string rS = "";
            return rS;
        }

        /// <summary>
        /// Default Urho Method
        /// </summary>
        /// <returns></returns>
        protected string Update(string indent) 
        {
            string rS = "";
            return rS;
        }

        /// <summary>
        /// Default Urho Method
        /// </summary>
        /// <returns></returns>
        protected string PostUpdate(string indent)
        {
            string rS = "";
            return rS;
        }

        /// <summary>
        /// Default Urho Method
        /// </summary>
        /// <returns></returns>
        protected string FixedUpdate(string indent) 
        {
            string rS = "";
            return rS;
        }

        /// <summary>
        /// Default Urho Method
        /// </summary>
        /// <returns></returns>
        protected string FixedPostUpdate(string indent) 
        {
            string rS = "";
            return rS;
        }

        /// <summary>
        /// Default Urho Method
        /// </summary>
        /// <returns></returns>
        protected string Save(string indent) 
        {
            string rS = "";
            return rS;
        }

        /// <summary>
        /// Default Urho Method
        /// </summary>
        /// <returns></returns>
        protected string Load(string indent)
        {
            string rS = "";
            return rS;
        }

        /// <summary>
        /// Default Urho Method
        /// </summary>
        /// <returns></returns>
        protected string WriteNetworkUpdate(string indent) 
        {
            string rS = "";
            return rS;
        }

        /// <summary>
        /// Default Urho Method
        /// </summary>
        /// <returns></returns>
        protected string ReadNetworkUpdate(string indent) 
        {
            string rS = "";
            return rS;
        }

        /// <summary>
        /// Default Urho Method
        /// </summary>
        /// <returns></returns>
        protected string ApplyAttributes(string indent) 
        {
            string rS = "";
            return rS;
        }


        // -------------------------------------------------------------------------------------------------
        //
        // Useful helpers
        //
        protected string InitAudio(string indent)
        {
            string rS = indent + "// InitAudio method\n//\n";

            rS += indent + "void InitAudio() {\n";
            rS += indent + m_tabSpace + "if (engine.headless)\n";
            rS += indent + m_tabSpace + m_tabSpace + "return;\n";
            rS += indent + m_tabSpace + @"// Lower mastervolumes slightly." + "\n";
            rS += indent + m_tabSpace + "audio.masterGain[SOUND_MASTER] = 0.75;\n";
            rS += indent + m_tabSpace + "audio.masterGain[SOUND_MUSIC] = 0.75;\n";
            rS += indent + "}\n\n";

            // Add any audio background music references here
            //
            return rS;
        }

        protected string InitConsole(string indent)
        {
            string rS = indent + "// "+ getCurrentMethod() + " method\n//\n";
            rS += indent + "void "+ getCurrentMethod() + "() {\n";
            rS += indent + m_tabSpace + "if (engine.headless)\n";
            rS += indent + getTabPrefix(2) + " return;\n";
            rS += indent + m_tabSpace + @"XMLFile@ uiStyle = cache.GetResource(""XMLFile"", ""UI/DefaultStyle.xml"");" + "\n";
            rS += indent + m_tabSpace + "Console@ console = engine.CreateConsole();\n";
            rS += indent + m_tabSpace + "console.style = uiStyle;\n";
            rS += indent + m_tabSpace + "console.numRows = 16;\n";
            rS += indent + m_tabSpace + "engine.CreateDebugHud();\n";
            rS += indent + m_tabSpace + "debugHud.style = uiStyle;\n";
            rS += indent + "}\n\n";

            return rS;
        }

        protected string InitScene(string indent)
        {
            string rS = indent + "// " + getCurrentMethod() + " method\n//\n";
            rS += indent + "void " + getCurrentMethod() + "() {\n";

            rS += indent + m_tabSpace + @"gameScene = Scene(""NinjaSnowWar"");" + "\n";

            rS += indent + m_tabSpace + "// Enable access to this script file & scene from the console\n";
            rS += indent + m_tabSpace + "script.defaultScene = gameScene;\n";
            rS += indent + m_tabSpace + "script.defaultScriptFile = scriptFile;\n";

            rS += indent + m_tabSpace + "// For the multiplayer client, do not load the scene, let it load from the server\n";
            rS += indent + m_tabSpace + "if (runClient)\n";
            rS += indent + getTabPrefix(2) + "return;\n";

            rS += indent + m_tabSpace + @"gameScene.LoadXML(cache.GetFile(""" + m_gameSceneXML + @"""));" + "\n";

            rS += indent + m_tabSpace + "// On mobile devices render the shadowmap first\n";

            rS += indent + m_tabSpace + @"if (GetPlatform() == ""Android"" || GetPlatform() == ""iOS"")" + "\n";
            rS += indent + getTabPrefix(2) + "renderer.reuseShadowMaps = false;\n";
            rS += "}\n\n";

            return rS;
        }

        /// <summary>
        /// Empty for the moment
        /// </summary>
        /// <returns></returns>
        protected string InitNetworking(string indent)
        {
            string rS = indent + "// " + getCurrentMethod() + " method\n//\n";
            rS += indent + "void " + getCurrentMethod() + "() {\n";
            rS += indent + "}\n\n";
            return rS;
        }

        protected string CreateCamera(string indent)
        {
            string rS = indent + "// " + getCurrentMethod() + " method\n//\n";
            rS += indent + "void " + getCurrentMethod() + "() {\n";

            rS += indent + m_tabSpace + "// Note: the camera is not in the scene\n";
            rS += indent + m_tabSpace + "gameCameraNode = Node();\n";
            rS += indent + m_tabSpace + "gameCameraNode.position = Vector3(0, 200, -1000);\n";

            rS += indent + m_tabSpace +  @"gameCamera = gameCameraNode.CreateComponent(""Camera"");" + "\n";
            rS += indent + m_tabSpace + "gameCamera.nearClip = 50.0;\n";
            rS += indent + m_tabSpace + "gameCamera.farClip = 16000.0;\n";

            rS += indent + m_tabSpace + "if (!engine.headless) {\n";
            rS += indent + getTabPrefix(2) +  "renderer.viewports[0] = Viewport(gameScene, gameCamera);\n";
            rS += indent + m_tabSpace + @"audio.listener = gameCameraNode.CreateComponent(""SoundListener"");" + "\n";
            rS += indent + m_tabSpace + "}\n";
            rS += indent + "}\n\n";
            return rS;
        }

        protected string CreateOverlays(string indent)
        {
            string rS = indent + "// " + getCurrentMethod() + " method\n//\n";
            rS += indent + "void " + getCurrentMethod() + "() {\n";

            /*
             *     if (engine.headless || runServer)
        return;

    int height = graphics.height / 22;
    if (height > 64)
        height = 64;

    sight = BorderImage();
    sight.texture = cache.GetResource("Texture2D", "Textures/Sight.png");
    sight.SetAlignment(HA_CENTER, VA_CENTER);
    sight.SetSize(height, height);
    ui.root.AddChild(sight);

    Font@ font = cache.GetResource("Font", "Fonts/BlueHighway.ttf");

    scoreText = Text();
    scoreText.SetFont(font, 17);
    scoreText.SetAlignment(HA_LEFT, VA_TOP);
    scoreText.SetPosition(5, 5);
    scoreText.colors[C_BOTTOMLEFT] = Color(1, 1, 0.25);
    scoreText.colors[C_BOTTOMRIGHT] = Color(1, 1, 0.25);
    ui.root.AddChild(scoreText);

    @hiscoreText = Text();
    hiscoreText.SetFont(font, 17);
    hiscoreText.SetAlignment(HA_RIGHT, VA_TOP);
    hiscoreText.SetPosition(-5, 5);
    hiscoreText.colors[C_BOTTOMLEFT] = Color(1, 1, 0.25);
    hiscoreText.colors[C_BOTTOMRIGHT] = Color(1, 1, 0.25);
    ui.root.AddChild(hiscoreText);

    @messageText = Text();
    messageText.SetFont(font, 17);
    messageText.SetAlignment(HA_CENTER, VA_CENTER);
    messageText.SetPosition(0, -height * 2);
    messageText.color = Color(1, 0, 0);
    ui.root.AddChild(messageText);

    BorderImage@ healthBorder = BorderImage();
    healthBorder.texture = cache.GetResource("Texture2D", "Textures/HealthBarBorder.png");
    healthBorder.SetAlignment(HA_CENTER, VA_TOP);
    healthBorder.SetPosition(0, 8);
    healthBorder.SetSize(120, 20);
    ui.root.AddChild(healthBorder);

    healthBar = BorderImage();
    healthBar.texture = cache.GetResource("Texture2D", "Textures/HealthBarInside.png");
    healthBar.SetPosition(2, 2);
    healthBar.SetSize(116, 16);
    healthBorder.AddChild(healthBar);

    if (GetPlatform() == "Android" || GetPlatform() == "iOS")
    {
        touchEnabled = true;

        moveButton = BorderImage();
        moveButton.texture = cache.GetResource("Texture2D", "Textures/TouchInput.png");
        moveButton.imageRect = IntRect(0, 0, 96, 96);
        moveButton.SetPosition(touchButtonBorder, graphics.height - touchButtonSize - touchButtonBorder);
        moveButton.SetSize(touchButtonSize, touchButtonSize);
        ui.root.AddChild(moveButton);

        fireButton = BorderImage();
        fireButton.texture = cache.GetResource("Texture2D", "Textures/TouchInput.png");
        fireButton.imageRect = IntRect(96, 0, 192, 96);
        fireButton.SetPosition(graphics.width - touchButtonSize - touchButtonBorder, graphics.height - touchButtonSize -
            touchButtonBorder);
        fireButton.SetSize(touchButtonSize, touchButtonSize);
        ui.root.AddChild(fireButton);
    } 
             * 
             * */

            rS += indent + "}\n\n";
            return rS;
        }

        protected string doSubscriptions(string indent)
        {
            string rS = "";

            rS += indent + @"SubscribeToEvent(gameScene, ""SceneUpdate"", ""HandleUpdate"");" + "\n";
            rS += indent + "if (gameScene.physicsWorld !is null)\n";
            rS += indent + getTabPrefix(2) + @"SubscribeToEvent(gameScene.physicsWorld, ""PhysicsPreStep"", ""HandleFixedUpdate"");" + "\n";
            rS += indent + @"SubscribeToEvent(gameScene, ""ScenePostUpdate"", ""HandlePostUpdate"");" + "\n";
            rS += indent + @"SubscribeToEvent(""PostRenderUpdate"", ""HandlePostRenderUpdate"");" + "\n";
            rS += indent + @"SubscribeToEvent(""KeyDown"", ""HandleKeyDown"");" + "\n";
            rS += indent + @"SubscribeToEvent(""Points"", ""HandlePoints"");" + "\n";
            rS += indent + @"SubscribeToEvent(""Kill"", ""HandleKill"");" + "\n";
            rS += indent + @"SubscribeToEvent(""ScreenMode"", ""HandleScreenMode"");" + "\n\n";

            return rS;
        }


        protected string SetMessage(string indent)
        {
            string rS = indent + "// " + getCurrentMethod() + " method\n//\n";
            rS += indent + "void " + getCurrentMethod() + "() {\n";
            rS += indent + m_tabSpace + "if (messageText !is null)\n";
            rS += indent + getTabPrefix(2) + "messageText.text = message;\n";
            rS += indent + "}\n\n";

            return rS;
        }

        protected string StartGame(string indent)
        {
            string rS = indent + "// " + getCurrentMethod() + " method\n//\n";
            rS += indent + "void " + getCurrentMethod() + "() {\n";

            rS += indent + m_tabSpace + "// Clear the scene of all existing scripted objects\n";
            rS += indent + m_tabSpace + "{\n";
            rS += indent + getTabPrefix(2) + "Array<Node@> scriptedNodes = gameScene.GetChildrenWithScript(true);\n";
            rS += indent + getTabPrefix(2) + "for (uint i = 0; i < scriptedNodes.length; ++i)\n";
            rS += indent + getTabPrefix(3) + "scriptedNodes[i].Remove();\n";
            rS += indent + m_tabSpace + "}\n";

            rS += indent + m_tabSpace + "players.Clear();\n";
            rS += indent + m_tabSpace + "SpawnPlayer(connection);\n";
            rS += indent + m_tabSpace + "ResetAI();\n";
            rS += indent + m_tabSpace + "gameOn = true;\n";
            rS += indent + m_tabSpace + "maxEnemies = initialMaxEnemies;\n";
            rS += indent + m_tabSpace + "incrementCounter = 0;\n";
            rS += indent + m_tabSpace + "enemySpawnTimer = 0;\n";
            rS += indent + m_tabSpace + "powerupSpawnTimer = 0;\n";
            rS += indent + m_tabSpace + "if (singlePlayer) {\n";
            rS += indent + getTabPrefix(2) + "playerControls.yaw = 0;\n";
            rS += indent + getTabPrefix(2) + "playerControls.pitch = 0;\n";
            rS += indent + getTabPrefix(2) + @"SetMessage("""");" + "\n";
            rS += indent + m_tabSpace + "}\n";
            rS += indent + "}\n\n";

            return rS;
        }

        /// <summary>
        /// This has to match the name hard-coded in Urho3D.cpp
        /// </summary>
        protected readonly string m_urhoExportFile = @"Scripts\Friendlier.as";

        /// <summary>
        /// Store the xyglo context for useful things like the project
        /// </summary>
        XygloContext m_context;

        /// <summary>
        /// XML output file generated and referred by this exporter
        /// </summary>
        protected string m_gameSceneXML;
    }
}
