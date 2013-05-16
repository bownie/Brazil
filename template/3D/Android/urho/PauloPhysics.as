#include "Brazil/Interloper.as"
#include "Brazil/Coin.as"

Scene@ testScene;
Camera@ camera;
Node@ cameraNode;

float yaw = 0.0;
float pitch = 0.0;
int drawDebug = 0;
Node@ interloperNode;
RigidBody@ interloperBody;
RigidBody@ interloperHead;

Text@ downloadsText;

Array<Node@> m_coins;

void Start()
{
    if (!engine.headless)
    {
        InitConsole();
        InitUI();
    }
    else
        OpenConsoleWindow();

    //ParseNetworkArguments();

    InitScene();

	for(uint i = 0; i < 10; i++)
	{
		addCoin(Vector3((i * 2) + 10 , 0, 0));
	}

	//attemptCoinLoad();
	
    SubscribeToEvent("Update", "HandleUpdate");
    SubscribeToEvent("KeyDown", "HandleKeyDown");
    SubscribeToEvent("MouseMove", "HandleMouseMove");
    SubscribeToEvent("MouseButtonDown", "HandleMouseButtonDown");
    SubscribeToEvent("MouseButtonUp", "HandleMouseButtonUp");
    SubscribeToEvent("PostRenderUpdate", "HandlePostRenderUpdate");
    SubscribeToEvent("SpawnBox", "HandleSpawnBox");

    network.RegisterRemoteEvent("SpawnBox");
}

void InitConsole()
{
    XMLFile@ uiStyle = cache.GetResource("XMLFile", "UI/DefaultStyle.xml");

    engine.CreateDebugHud();
    //debugHud.style = uiStyle;
    debugHud.mode = DEBUGHUD_SHOW_ALL;

    engine.CreateConsole();
    //console.style = uiStyle;
}

void InitUI()
{
    XMLFile@ uiStyle = cache.GetResource("XMLFile", "UI/DefaultStyle.xml");

    Cursor@ newCursor = Cursor("Cursor");
    //newCursor.style = uiStyle;
    newCursor.position = IntVector2(graphics.width / 2, graphics.height / 2);
    ui.cursor = newCursor;
    if (GetPlatform() == "Android")
        ui.cursor.visible = false;

    downloadsText = Text();
    downloadsText.SetAlignment(HA_CENTER, VA_CENTER);
    downloadsText.SetFont(cache.GetResource("Font", "Fonts/Anonymous Pro.ttf"), 20);
    ui.root.AddChild(downloadsText);
}

void attemptCoinLoad()
{
	// New Coin
	//
	Print("Creating coin\r\n");
	Node @coinNode = SpawnObject(Vector3(0, -10, 0), Quaternion(), "Coin");
	Coin@ coin= cast<Coin>(coinNode.scriptObject);
	Print("Created coin\r\n");
    //interloper.buildBody();

}

void addCoin(Vector3 position)
{
    Node@ coinNode = testScene.CreateChild("Coin");
    coinNode.position = position;
    coinNode.scale = Vector3(0.2, 0.2, 0.2);

	StaticModel@ coinObject = coinNode.CreateComponent("StaticModel");
	coinObject.model = cache.GetResource("Model", "Models/Coin.mdl");
	coinObject.material = cache.GetResource("Material", "Materials/BrightYellowUnlit.xml");
	coinObject.castShadows = true;

	// Store the new coin
	//
	int coinIndex = m_coins.length;
    m_coins.Resize(coinIndex + 1);
	m_coins[coinIndex] = coinNode;
}

void InitScene()
{
    testScene = Scene("TestScene");

    // Enable access to this script file & scene from the console
    script.defaultScene = testScene;
    script.defaultScriptFile = scriptFile;

    // Create the camera outside the scene so it is unaffected by scene load/save
    cameraNode = Node();
    camera = cameraNode.CreateComponent("Camera");
    cameraNode.position = Vector3(0, 2, -50);

    if (!engine.headless)
    {
        renderer.viewports[0] = Viewport(testScene, camera);
        
        // Add bloom & FXAA effects to the renderpath. Clone the default renderpath so that we don't affect it
        RenderPath@ newRenderPath = renderer.viewports[0].renderPath.Clone();
        newRenderPath.Append(cache.GetResource("XMLFile", "PostProcess/Bloom.xml"));
        newRenderPath.Append(cache.GetResource("XMLFile", "PostProcess/EdgeFilter.xml"));
        newRenderPath.SetEnabled("Bloom", false);
        newRenderPath.SetEnabled("EdgeFilter", false);
        renderer.viewports[0].renderPath = newRenderPath;

        audio.listener = cameraNode.CreateComponent("SoundListener");
    }


    PhysicsWorld@ world = testScene.CreateComponent("PhysicsWorld");
    testScene.CreateComponent("Octree");
    testScene.CreateComponent("DebugRenderer");

    Node@ zoneNode = testScene.CreateChild("Zone");
    Zone@ zone = zoneNode.CreateComponent("Zone");
    zone.ambientColor = Color(0, 0, 0);
    zone.fogColor = Color(0, 0, 0);
    zone.fogStart = 100.0;
    zone.fogEnd = 300.0;
    zone.boundingBox = BoundingBox(-1000, 1000);

    {
        Node@ lightNode = testScene.CreateChild("GlobalLight");
        lightNode.direction = Vector3(0.3, -0.5, 0.425);

        Light@ light = lightNode.CreateComponent("Light");
        light.lightType = LIGHT_DIRECTIONAL;
        light.castShadows = true;
        light.shadowBias = BiasParameters(0.0001, 0.5);
        light.shadowCascade = CascadeParameters(10.0, 50.0, 200.0, 0.0, 0.8);
        light.specularIntensity = 0.5;
    }

	/*
    {
        Node@ objectNode = testScene.CreateChild("Floor");
        objectNode.position = Vector3(0, -0.5, 0);
        objectNode.scale = Vector3(500, 1, 500);

        StaticModel@ object = objectNode.CreateComponent("StaticModel");
        object.model = cache.GetResource("Model", "Models/Box.mdl");
        object.material = cache.GetResource("Material", "Materials/StoneTiled.xml");

        RigidBody@ body = objectNode.CreateComponent("RigidBody");
        CollisionShape@ shape = objectNode.CreateComponent("CollisionShape");
        shape.SetBox(Vector3(1, 1, 1));
    }*/

	// New Interloper
	//
	Node @interloperNodeNew = SpawnObject(Vector3(0, -10, 0), Quaternion(), "Interloper");
	//Interloper@ interloper = cast<Interloper>(interloperNodeNew.scriptObject);
    //interloper.buildBody();

	// Interloper
	//
	{
		interloperNode = testScene.CreateChild("Interloper");
		interloperNode.position = Vector3(0, 15, 0);
		interloperNode.SetScale(1);

		StaticModel@ object = interloperNode.CreateComponent("StaticModel");
        object.model = cache.GetResource("Model", "Models/Box.mdl");
        object.material = cache.GetResource("Material", "Materials/BrightRedUnlit.xml");
        object.castShadows = true;

        interloperBody = interloperNode.CreateComponent("RigidBody");
        interloperBody.mass = 10.0;
        interloperBody.friction = 1.0;
		interloperBody.SetAttribute("Use Gravity", true);
        interloperBody.collisionEventMode = COLLISION_NEVER;

        CollisionShape@ shape = interloperNode.CreateComponent("CollisionShape");
        shape.SetBox(Vector3(1, 1, 1));


		// head
		//
		Node @headNode = interloperNode.CreateChild("Head");
		headNode.position = headNode.position + Vector3(0, 1, 0);
		headNode.SetScale(1);

		StaticModel@ headObject = headNode.CreateComponent("StaticModel");
        headObject.model = cache.GetResource("Model", "Models/Sphere.mdl");
        headObject.material = cache.GetResource("Material", "Materials/BrightRedUnlit.xml");
        headObject.castShadows = true;

		interloperHead = headNode.CreateComponent("RigidBody");
        interloperHead.mass = 10.0;
        interloperHead.friction = 1.0;
		interloperHead.SetAttribute("Use Gravity", true);
        interloperHead.collisionEventMode = COLLISION_NEVER;

        CollisionShape@ headShape = headNode.CreateComponent("CollisionShape");
        headShape.SetBox(Vector3(1, 1, 1));

		// Constraint
		//
		Constraint@ constraint = interloperNode.CreateComponent("Constraint", LOCAL);
		constraint.constraintType = CONSTRAINT_HINGE;
		constraint.disableCollision = true;
		// The connected body must be specified before setting the world position
		constraint.otherBody = interloperHead;
		constraint.worldPosition = interloperNode.worldPosition;
		constraint.axis = Vector3(0, 0, -1); 
		constraint.otherAxis = Vector3(0, 0, -1);
		constraint.highLimit = Vector2(0, 0);
		constraint.lowLimit = Vector2(0, 0);
	}



    for (uint i = 0; i < 5; ++i)
    {
        Node@ objectNode = testScene.CreateChild("Box");
        objectNode.position = Vector3(0, i * 2, 0);
        objectNode.scale = Vector3(i, 1, 1);

        StaticModel@ object = objectNode.CreateComponent("StaticModel");
        object.model = cache.GetResource("Model", "Models/Box.mdl");
        object.material = cache.GetResource("Material", "Materials/BrightBlueUnlit.xml");
        object.castShadows = true;

        RigidBody@ body = objectNode.CreateComponent("RigidBody");
        //body.mass = 10.0;
        //body.friction = 1.0;
		float iFloat = i;
		body.SetAttribute("Use Gravity", (i / 2 == iFloat / 2));
		//body.SetAttribute("Is Kinematic", false);
        // When object count is high, disabling collision events gives a significant performance increase
        body.collisionEventMode = COLLISION_NEVER;
        CollisionShape@ shape = objectNode.CreateComponent("CollisionShape");
        shape.SetBox(Vector3(1, 1, 1));

		// Constraint
		//

		/*
		Constraint@ constraint = objectNode.CreateComponent("Constraint", LOCAL);
		constraint.constraintType = CONSTRAINT_HINGE;
		constraint.otherBody = body;
		constraint.disableCollision = true;
		constraint.worldPosition = objectNode.worldPosition;
		constraint.axis = Vector3(0, 0, -1); 
		constraint.otherAxis = Vector3(0, 0, -1);
		constraint.highLimit = Vector2(90, 0);
		constraint.lowLimit = Vector2(0, 0);*/
    }

	/*
    for (uint i = 0; i < 50; ++i)
    {
        Node@ objectNode = testScene.CreateChild("Mushroom");
        objectNode.position = Vector3(Random() * 400 - 200, 0, Random() * 400 - 200);
        objectNode.rotation = Quaternion(0, Random(360.0), 0);
        objectNode.SetScale(5);

        StaticModel@ object = objectNode.CreateComponent("StaticModel");
        object.model = cache.GetResource("Model", "Models/Mushroom.mdl");
        object.material = cache.GetResource("Material", "Materials/Mushroom.xml");
        object.castShadows = true;

        RigidBody@ body = objectNode.CreateComponent("RigidBody");
        CollisionShape@ shape = objectNode.CreateComponent("CollisionShape");
        shape.SetTriangleMesh(cache.GetResource("Model", "Models/Mushroom.mdl"), 0);
    }*/
}

Node@ SpawnObject(const Vector3&in position, const Quaternion&in rotation, const String&in className)
{
    XMLFile@ xml = cache.GetResource("XMLFile", "Objects/" + className + ".xml");
    return scene.InstantiateXML(xml, position, rotation);
}

void CreateRagdollConstraint(Node@ root, const String&in boneName, const String&in parentName, ConstraintType type,
    const Vector3&in axis, const Vector3&in parentAxis, const Vector2&in highLimit, const Vector2&in lowLimit)
{
    Node@ boneNode = root.GetChild(boneName, true);
    Node@ parentNode = root.GetChild(parentName, true);
    if (boneNode is null || parentNode is null || boneNode.HasComponent("Constraint"))
        return;

    Constraint@ constraint = boneNode.CreateComponent("Constraint", LOCAL);
    constraint.constraintType = type;
    constraint.disableCollision = true;
    // The connected body must be specified before setting the world position
    constraint.otherBody = parentNode.GetComponent("RigidBody");
    constraint.worldPosition = boneNode.worldPosition;
    constraint.axis = axis;
    constraint.otherAxis = parentAxis;
    constraint.highLimit = highLimit;
    constraint.lowLimit = lowLimit;
}


void HandleUpdate(StringHash eventType, VariantMap& eventData)
{
    float timeStep = eventData["TimeStep"].GetFloat();

	// Update coins
	//
	for (uint i = 0; i < m_coins.length; ++i)
    {
        if (m_coins[i] !is null)
        {
            //Print("Rotating coin " + i + " at timestep " + timeStep + "\r\n");
			Quaternion rot = m_coins[i].rotation;
			//rot. += 0.02;
			m_coins[i].rotation = rot * Quaternion(0, 2, 0);
        }
    }

    if (ui.focusElement is null)
    {
        float speedMultiplier = 1.0;
        if (input.keyDown[KEY_LSHIFT])
            speedMultiplier = 5.0;
        if (input.keyDown[KEY_LCTRL])
            speedMultiplier = 0.1;

		//if (input.keyDown'Up')
		    

        if (input.keyDown['W'])
            cameraNode.TranslateRelative(Vector3(0, 0, 10) * timeStep * speedMultiplier);
        if (input.keyDown['S'])
            cameraNode.TranslateRelative(Vector3(0, 0, -10) * timeStep * speedMultiplier);
        if (input.keyDown['A'])
            cameraNode.TranslateRelative(Vector3(-10, 0, 0) * timeStep * speedMultiplier);
        if (input.keyDown['D'])
            cameraNode.TranslateRelative(Vector3(10, 0, 0) * timeStep * speedMultiplier);
    }

    // Update package download status
    if (network.serverConnection !is null)
    {
        Connection@ connection = network.serverConnection;
        if (connection.numDownloads > 0)
        {
            downloadsText.text = "Downloads: " + connection.numDownloads + " Current download: " +
                connection.downloadName + " (" + int(connection.downloadProgress * 100.0) + "%)";
        }
        else if (!downloadsText.text.empty)
            downloadsText.text = "";
    }
}

void HandleKeyDown(StringHash eventType, VariantMap& eventData)
{
    int key = eventData["Key"].GetInt();

    if (key == KEY_ESC)
    {
        //if (ui.focusElement is null)
            engine.Exit();
        //else
            //console.visible = false;
    }

    if (key == KEY_F1)
        console.Toggle();

    if (ui.focusElement is null)
    {
        if (key == '1')
        {
            int quality = renderer.textureQuality;
            ++quality;
            if (quality > 2)
                quality = 0;
            renderer.textureQuality = quality;
        }

        if (key == '2')
        {
            int quality = renderer.materialQuality;
            ++quality;
            if (quality > 2)
                quality = 0;
            renderer.materialQuality = quality;
        }

        if (key == '3')
            renderer.specularLighting = !renderer.specularLighting;

        if (key == '4')
            renderer.drawShadows = !renderer.drawShadows;

        if (key == '5')
        {
            int size = renderer.shadowMapSize;
            size *= 2;
            if (size > 2048)
                size = 512;
            renderer.shadowMapSize = size;
        }

        if (key == '6')
            renderer.shadowQuality = renderer.shadowQuality + 1;

        if (key == '7')
        {
            bool occlusion = renderer.maxOccluderTriangles > 0;
            occlusion = !occlusion;
            renderer.maxOccluderTriangles = occlusion ? 5000 : 0;
        }

        if (key == '8')
            renderer.dynamicInstancing = !renderer.dynamicInstancing;

        if (key == ' ')
        {
            drawDebug++;
            if (drawDebug > 2)
                drawDebug = 0;
        }

        if (key == 'B')
            renderer.viewports[0].renderPath.ToggleEnabled("Bloom");

        if (key == 'F')
            renderer.viewports[0].renderPath.ToggleEnabled("EdgeFilter");

        if (key == 'O')
            camera.orthographic = !camera.orthographic;

        if (key == 'T')
            debugHud.Toggle(DEBUGHUD_SHOW_PROFILER);

        if (key == KEY_F5)
        {
            File@ xmlFile = File(fileSystem.programDir + "Data/Scenes/Physics.xml", FILE_WRITE);
            testScene.SaveXML(xmlFile);
        }

        if (key == KEY_F7)
        {
            File@ xmlFile = File(fileSystem.programDir + "Data/Scenes/Physics.xml", FILE_READ);
            if (xmlFile.open)
                testScene.LoadXML(xmlFile);
        }
    }
   
	if (key == KEY_UP)
	{
		interloperHead.linearVelocity = Vector3(0, 10, 0);
	}
	else if (key == KEY_LEFT)
	{
		interloperHead.linearVelocity = interloperHead.linearVelocity + Vector3(-10, 0, 0);
	} else if (key == KEY_RIGHT)
	{
		interloperHead.linearVelocity = interloperHead.linearVelocity + Vector3(10, 0, 0);
	}
}

void HandleMouseMove(StringHash eventType, VariantMap& eventData)
{
    if (eventData["Buttons"].GetInt() & MOUSEB_RIGHT != 0)
    {
        int mousedx = eventData["DX"].GetInt();
        int mousedy = eventData["DY"].GetInt();
        yaw += mousedx / 10.0;
        pitch += mousedy / 10.0;
        if (pitch < -90.0)
            pitch = -90.0;
        if (pitch > 90.0)
            pitch = 90.0;

        cameraNode.rotation = Quaternion(pitch, yaw, 0);
    }
}

void HandleMouseButtonDown(StringHash eventType, VariantMap& eventData)
{
    int button = eventData["Button"].GetInt();
    if (button == MOUSEB_RIGHT)
        ui.cursor.visible = false;

    // Test either creating a new physics object or painting a decal (SHIFT down)
    if (button == MOUSEB_LEFT && ui.GetElementAt(ui.cursorPosition, true) is null && ui.focusElement is null)
    {
        if (!input.qualifierDown[QUAL_SHIFT])
        {
            VariantMap eventData;
            eventData["Pos"] = cameraNode.position;
            eventData["Rot"] = cameraNode.rotation;
    
            SendEvent("SpawnBox", eventData);
        }
        else
        {
            IntVector2 pos = ui.cursorPosition;
            if (ui.GetElementAt(pos, true) is null && testScene.octree !is null)
            {
                Ray cameraRay = camera.GetScreenRay(float(pos.x) / graphics.width, float(pos.y) / graphics.height);
                RayQueryResult result = testScene.octree.RaycastSingle(cameraRay, RAY_TRIANGLE, 250.0, DRAWABLE_GEOMETRY);
                if (result.drawable !is null)
                {
                    Vector3 rayHitPos = cameraRay.origin + cameraRay.direction * result.distance;
                    DecalSet@ decal = result.drawable.node.GetComponent("DecalSet");
                    if (decal is null)
                    {
                        decal = result.drawable.node.CreateComponent("DecalSet");
                        decal.material = cache.GetResource("Material", "Materials/UrhoDecal.xml");
                        // Increase max. vertices/indices if the target is skinned
                        if (result.drawable.typeName == "AnimatedModel")
                        {
                            decal.maxVertices = 2048;
                            decal.maxIndices = 4096;
                        }
                    }
                    decal.AddDecal(result.drawable, rayHitPos, cameraNode.worldRotation, 0.5, 1.0, 1.0, Vector2(0, 0),
                        Vector2(1, 1));
                }
            }
        }
    }
}

void HandleSpawnBox(StringHash eventType, VariantMap& eventData)
{
    Vector3 position = eventData["Pos"].GetVector3();
    Quaternion rotation = eventData["Rot"].GetQuaternion();

    Node@ newNode = testScene.CreateChild();
    newNode.position = position;
    newNode.rotation = rotation;
    newNode.SetScale(0.2);

    RigidBody@ body = newNode.CreateComponent("RigidBody");
    body.mass = 1.0;
    body.friction = 1.0;
    body.linearVelocity = rotation * Vector3(0.0, 1.0, 10.0);

    CollisionShape@ shape = newNode.CreateComponent("CollisionShape");
    shape.SetBox(Vector3(1, 1, 1));

    StaticModel@ object = newNode.CreateComponent("StaticModel");
    object.model = cache.GetResource("Model", "Models/Box.mdl");
    object.material = cache.GetResource("Material", "Materials/StoneSmall.xml");
    object.castShadows = true;
    object.shadowDistance = 150.0;
    object.drawDistance = 200.0;
}

void HandleMouseButtonUp(StringHash eventType, VariantMap& eventData)
{
    if (eventData["Button"].GetInt() == MOUSEB_RIGHT)
        ui.cursor.visible = true;
}

void HandlePostRenderUpdate()
{
    if (engine.headless)
        return;

    // Draw rendering debug geometry without depth test to see the effect of occlusion
    if (drawDebug == 1)
        renderer.DrawDebugGeometry(false);
    if (drawDebug == 2)
        testScene.physicsWorld.DrawDebugGeometry(true);

    IntVector2 pos = ui.cursorPosition;
    if (ui.GetElementAt(pos, true) is null && testScene.octree !is null)
    {
        Ray cameraRay = camera.GetScreenRay(float(pos.x) / graphics.width, float(pos.y) / graphics.height);
        RayQueryResult result = testScene.octree.RaycastSingle(cameraRay, RAY_TRIANGLE, 250.0, DRAWABLE_GEOMETRY);
        if (result.drawable !is null)
        {
            Vector3 rayHitPos = cameraRay.origin + cameraRay.direction * result.distance;
            testScene.debugRenderer.AddBoundingBox(BoundingBox(rayHitPos + Vector3(-0.01, -0.01, -0.01), rayHitPos +
                Vector3(0.01, 0.01, 0.01)), Color(1.0, 1.0, 1.0), true);
        }
    }
}

void HandleClientConnected(StringHash eventType, VariantMap& eventData)
{
    Connection@ connection = eventData["Connection"].GetConnection();
    connection.scene = testScene; // Begin scene replication to the client
    connection.logStatistics = true;
}
