Scene@ helloScene;
PhysicsWorld@ physicsWorld;

void Start()
{
    helloScene = Scene();

	// Create the physics world
	//
	//physicsWorld = helloScene.CreateComponent("PhysicsWorld");

	createFloor();

    //CreateObjects();

	//createBox(Vector3(1, 0, 0), Quaternion(0, 120.0, 120.0));

    //CreateText();
    SubscribeToEvents();
}

void CreateObjects()
{
    helloScene.CreateComponent("Octree");

    Node@ objectNode = helloScene.CreateChild();
    Node@ lightNode = helloScene.CreateChild();
    Node@ cameraNode = helloScene.CreateChild();

	objectNode.position = Vector3(-1, 0, 0);
	objectNode.rotation = Quaternion(0, 120.0, 120.0);
	objectNode.SetScale(0.5);

    StaticModel@ object = objectNode.CreateComponent("StaticModel");
	object.model = cache.GetResource("Model", "Models/Box.mdl");
	object.material = cache.GetResource("Material", "Materials/BrightBlueUnlit.xml");
	object.castShadows = true;

    RigidBody@ body = objectNode.CreateComponent("RigidBody");
	body.mass = 10.0;
	body.friction = 1.0;

    CollisionShape@ shape = objectNode.CreateComponent("CollisionShape");
    shape.SetTriangleMesh(cache.GetResource("Model", "Models/Mushroom.mdl"), 0);

    //object.model = cache.GetResource("Model", "Models/Mushroom.mdl");
    //object.material = cache.GetResource("Material", "Materials/Mushroom.xml");

    Light@ light = lightNode.CreateComponent("Light");
    light.lightType = LIGHT_DIRECTIONAL;
    lightNode.direction = Vector3(-1, -1, -1);

    Camera@ camera = cameraNode.CreateComponent("Camera");
    cameraNode.position = Vector3(0, 0.3, -3);

    renderer.viewports[0] = Viewport(helloScene, camera);
}

void createBox(Vector3 position, Quaternion rotation)
{
	Node@ newNode = helloScene.CreateChild();
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



void CreateText()
{
    Text@ helloText = Text();
    helloText.SetFont(cache.GetResource("Font", "Fonts/Anonymous Pro.ttf"), 30);
    helloText.text = "Hello World from Urho3D";
    helloText.color = Color(0, 1, 0);
    helloText.horizontalAlignment = HA_CENTER;
    helloText.verticalAlignment = VA_CENTER;

    ui.root.AddChild(helloText);
}

void createFloor()
{
    Node@ objectNode = helloScene.CreateChild("Floor");
    objectNode.position = Vector3(0, -0.5, 0);
    objectNode.scale = Vector3(500, 1, 500);

    StaticModel@ object = objectNode.CreateComponent("StaticModel");
    object.model = cache.GetResource("Model", "Models/Box.mdl");
    object.material = cache.GetResource("Material", "Materials/StoneTiled.xml");

    RigidBody@ body = objectNode.CreateComponent("RigidBody");
    CollisionShape@ shape = objectNode.CreateComponent("CollisionShape");
    shape.SetBox(Vector3(1, 1, 1));
}

void SubscribeToEvents()
{
    SubscribeToEvent("Update", "HandleUpdate");
}

void HandleUpdate(StringHash eventType, VariantMap& eventData)
{
    float timeStep = eventData["TimeStep"].GetFloat();

    if (input.keyPress[KEY_ESC])
        engine.Exit();
}
