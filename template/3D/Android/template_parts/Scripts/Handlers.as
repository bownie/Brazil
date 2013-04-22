void StartGame(Connection@ connection)
{
    // Clear the scene of all existing scripted objects
    {
        Array<Node@> scriptedNodes = gameScene.GetChildrenWithScript(true);
        for (uint i = 0; i < scriptedNodes.length; ++i)
            scriptedNodes[i].Remove();
    }

    players.Clear();
    SpawnPlayer(connection);

    ResetAI();

    gameOn = true;
    maxEnemies = initialMaxEnemies;
    incrementCounter = 0;
    enemySpawnTimer = 0;
    powerupSpawnTimer = 0;

    if (singlePlayer)
    {
        playerControls.yaw = 0;
        playerControls.pitch = 0;
        SetMessage("");
    }
}

void SpawnPlayer(Connection@ connection)
{
    Vector3 spawnPosition;
    if (singlePlayer)
        spawnPosition = Vector3(0, 90, 0);
    else
        spawnPosition = Vector3(Random(spawnAreaSize) - spawnAreaSize * 0.5, 90, Random(spawnAreaSize) - spawnAreaSize);

    Node@ playerNode = SpawnObject(spawnPosition, Quaternion(), "Ninja");
    // Set owner connection. Owned nodes are always updated to the owner at full frequency
    playerNode.owner = connection;
    playerNode.name = "Player";

    // Initialize variables
    Ninja@ playerNinja = cast<Ninja>(playerNode.scriptObject);
    playerNinja.health = playerNinja.maxHealth = playerHealth;
    playerNinja.side = SIDE_PLAYER;
    // Make sure the player can not shoot on first frame by holding the button down
    if (connection is null)
        playerNinja.controls = playerNinja.prevControls = playerControls;
    else
        playerNinja.controls = playerNinja.prevControls = connection.controls;

    // Check if player entry already exists
    int playerIndex = -1;
    for (uint i = 0; i < players.length; ++i)
    {
        if (players[i].connection is connection)
        {
            playerIndex = i;
            break;
        }
    }

    // Does not exist, create new
    if (playerIndex < 0)
    {
        playerIndex = players.length;
        players.Resize(players.length + 1);
        players[playerIndex].connection = connection;

        if (connection !is null)
        {
            players[playerIndex].name = connection.identity["UserName"].GetString();
            // In multiplayer, send current hiscores to the new player
            SendHiscores(playerIndex);
        }
        else
        {
            players[playerIndex].name = "Player";
            // In singleplayer, create also the default hiscore entry immediately
            HiscoreEntry newHiscore;
            newHiscore.name = players[playerIndex].name;
            newHiscore.score = 0;
            hiscores.Push(newHiscore);
        }
    }

    players[playerIndex].nodeID = playerNode.id;
    players[playerIndex].score = 0;

    if (connection !is null)
    {
        // In multiplayer, send initial score, then send a remote event that tells the spawned node's ID
        // It is important for the event to be in-order so that the node has been replicated first
        SendScore(playerIndex);
        VariantMap eventData;
        eventData["NodeID"] = playerNode.id;
        connection.SendRemoteEvent("PlayerSpawned", true, eventData);
    }
}

void HandleUpdate(StringHash eventType, VariantMap& eventData)
{
    float timeStep = eventData["TimeStep"].GetFloat();

    UpdateControls();
    CheckEndAndRestart();

    if (engine.headless)
    {
        String command = GetConsoleInput();
        if (command.length > 0)
            script.Execute(command);
    }
    else
    {
        if (debugHud.mode != DEBUGHUD_SHOW_NONE)
        {
            Node@ playerNode = FindOwnNode();
            if (playerNode !is null)
            {
                debugHud.SetAppStats("Player Pos", playerNode.worldPosition.ToString());
                debugHud.SetAppStats("Player Yaw", Variant(playerNode.worldRotation.yaw));
            }
            else
                debugHud.ClearAppStats();
        }
    }
}

void HandleFixedUpdate(StringHash eventType, VariantMap& eventData)
{
    float timeStep = eventData["TimeStep"].GetFloat();

    // Spawn new objects, singleplayer or server only
    if (singlePlayer || runServer)
        SpawnObjects(timeStep);
}

void HandlePostUpdate()
{
    UpdateCamera();
    UpdateStatus();
}

void HandlePostRenderUpdate()
{
    if (engine.headless)
        return;

    if (drawDebug)
        gameScene.physicsWorld.DrawDebugGeometry(true);
    if (drawOctreeDebug)
        gameScene.octree.DrawDebugGeometry(true);
}

void HandleTouchBegin(StringHash eventType, VariantMap& eventData)
{
    int touchID = eventData["TouchID"].GetInt();
    IntVector2 pos(eventData["X"].GetInt(), eventData["Y"].GetInt());
    UIElement@ element = ui.GetElementAt(pos, false);

    if (element is moveButton)
        moveTouchID = touchID;
    else if (element is fireButton)
        fireTouchID = touchID;
    else
        rotateTouchID = touchID;
}

void HandleTouchEnd(StringHash eventType, VariantMap& eventData)
{
    int touchID = eventData["TouchID"].GetInt();

    if (touchID == moveTouchID)
        moveTouchID = -1;
    if (touchID == rotateTouchID)
        rotateTouchID = -1;
    if (touchID == fireTouchID)
        fireTouchID = -1;
}

void HandleKeyDown(StringHash eventType, VariantMap& eventData)
{
    int key = eventData["Key"].GetInt();

    if (key == KEY_ESC)
    {
        if (!console.visible)
            engine.Exit();
        else
            console.visible = false;
    }

    if (key == KEY_F1)
        console.Toggle();
    
    if (key == KEY_F2)
        debugHud.ToggleAll();
    
    if (key == KEY_F3)
        drawDebug = !drawDebug;
    
    if (key == KEY_F4)
        drawOctreeDebug = !drawOctreeDebug;

    // Allow pause only in singleplayer
    if (key == 'P' && singlePlayer && !console.visible && gameOn)
    {
        gameScene.active = !gameScene.active;
        if (!gameScene.active)
            SetMessage("PAUSED");
        else
            SetMessage("");
    }
}

void HandlePoints(StringHash eventType, VariantMap& eventData)
{
    if (eventData["DamageSide"].GetInt() == SIDE_PLAYER)
    {
        // Get node ID of the object that should receive points -> use it to find player index
        int playerIndex = FindPlayerIndex(eventData["Receiver"].GetInt());
        if (playerIndex >= 0)
        {
            players[playerIndex].score += eventData["Points"].GetInt();
            SendScore(playerIndex);

            bool newHiscore = CheckHiscore(playerIndex);
            if (newHiscore)
                SendHiscores(-1);
        }
    }
}

void HandleKill(StringHash eventType, VariantMap& eventData)
{
    if (eventData["DamageSide"].GetInt() == SIDE_PLAYER)
    {
        MakeAIHarder();

        // Increment amount of simultaneous enemies after enough kills
        incrementCounter++;
        if (incrementCounter >= incrementEach)
        {
            incrementCounter = 0;
            if (maxEnemies < finalMaxEnemies)
                maxEnemies++;
        }
    }
}

void HandleClientIdentity(StringHash eventType, VariantMap& eventData)
{
    Connection@ connection = GetEventSender();
    // If user has empty name, invent one
    if (connection.identity["UserName"].GetString().Trimmed().empty)
        connection.identity["UserName"] = "user" + RandomInt(1000);
    // Assign scene to begin replicating it to the client
    connection.scene = gameScene;
}

void HandleClientSceneLoaded(StringHash eventType, VariantMap& eventData)
{
    // Now client is actually ready to begin. If first player, clear the scene and restart the game
    Connection@ connection = GetEventSender();
    if (players.empty)
        StartGame(connection);
    else
        SpawnPlayer(connection);
}

void HandleClientDisconnected(StringHash eventType, VariantMap& eventData)
{
    Connection@ connection = GetEventSender();
    // Erase the player entry, and make the player's ninja commit seppuku (if exists)
    for (uint i = 0; i < players.length; ++i)
    {
        if (players[i].connection is connection)
        {
            players[i].connection = null;
            Node@ playerNode = FindPlayerNode(i);
            if (playerNode !is null)
            {
                Ninja@ playerNinja = cast<Ninja>(playerNode.scriptObject);
                playerNinja.health = 0;
                playerNinja.lastDamageSide = SIDE_NEUTRAL; // No-one scores from this
            }
            players.Erase(i);
            return;
        }
    }
}

void HandlePlayerSpawned(StringHash eventType, VariantMap& eventData)
{
    // Store our node ID and mark the game as started
    clientNodeID = eventData["NodeID"].GetInt();
    gameOn = true;
    SetMessage("");

    // Copy initial yaw from the player node (we should have it replicated now)
    Node@ playerNode = FindOwnNode();
    if (playerNode !is null)
    {
        playerControls.yaw = playerNode.rotation.yaw;
        playerControls.pitch = 0;
    }
}

void HandleUpdateScore(StringHash eventType, VariantMap& eventData)
{
    clientScore = eventData["Score"].GetInt();
    scoreText.text = "Score " + clientScore;
}

void HandleUpdateHiscores(StringHash eventType, VariantMap& eventData)
{
    VectorBuffer data = eventData["Hiscores"].GetBuffer();
    hiscores.Resize(data.ReadVLE());
    for (uint i = 0; i < hiscores.length; ++i)
    {
        hiscores[i].name = data.ReadString();
        hiscores[i].score = data.ReadInt();
    }

    String allHiscores;
    for (uint i = 0; i < hiscores.length; ++i)
        allHiscores += hiscores[i].name + " " + hiscores[i].score + "\n";
    hiscoreText.text = allHiscores;    
}

void HandleNetworkUpdateSent()
{
    // Clear accumulated buttons from the network controls
    if (network.serverConnection !is null)
        network.serverConnection.controls.Set(CTRL_ALL, false);
}

void SpawnObjects(float timeStep)
{
    // If game not running, run only the random generator
    if (!gameOn)
    {
        Random();
        return;
    }

    // Spawn powerups
    powerupSpawnTimer += timeStep;
    if (powerupSpawnTimer >= powerupSpawnRate)
    {
        powerupSpawnTimer = 0;
        int numPowerups = gameScene.GetChildrenWithScript("SnowCrate", true).length + gameScene.GetChildrenWithScript("Potion", true).length;

        if (numPowerups < maxPowerups)
        {
            const float maxOffset = 4000;
            float xOffset = Random(maxOffset * 2.0f) - maxOffset;
            float zOffset = Random(maxOffset * 2.0f) - maxOffset;

            SpawnObject(Vector3(xOffset, 5000, zOffset), Quaternion(), "SnowCrate");
        }
    }

    // Spawn enemies
    enemySpawnTimer += timeStep;
    if (enemySpawnTimer > enemySpawnRate)
    {
        enemySpawnTimer = 0;
        int numEnemies = 0;        
        Array<Node@> ninjaNodes = gameScene.GetChildrenWithScript("Ninja", true);
        for (uint i = 0; i < ninjaNodes.length; ++i)
        {
            Ninja@ ninja = cast<Ninja>(ninjaNodes[i].scriptObject);
            if (ninja.side == SIDE_ENEMY)
                ++numEnemies;
        }

        if (numEnemies < maxEnemies)
        {
            const float maxOffset = 4000;
            float offset = Random(maxOffset * 2.0) - maxOffset;
            // Random north/east/south/west direction
            int dir = RandomInt() & 3;
            dir *= 90;
            Quaternion rotation(0, dir, 0);

            Node@ enemyNode = SpawnObject(rotation * Vector3(offset, 1000, -12000), rotation, "Ninja");

            // Initialize variables
            Ninja@ enemyNinja = cast<Ninja>(enemyNode.scriptObject);
            enemyNinja.side = SIDE_ENEMY;
            @enemyNinja.controller = AIController();
            RigidBody@ enemyBody = enemyNode.GetComponent("RigidBody");
            enemyBody.linearVelocity = rotation * Vector3(0, 1000, 3000);
        }
    }
}

void CheckEndAndRestart()
{
    // Only check end of game if singleplayer or client
    if (runServer)
        return;

    // Check if player node has vanished
    Node@ playerNode = FindOwnNode();
    if (gameOn && playerNode is null)
    {
        gameOn = false;
        SetMessage("Press Fire or Jump to restart!");
        return;
    }

    // Check for restart (singleplayer only)
    if (!gameOn && singlePlayer && playerControls.IsPressed(CTRL_FIRE | CTRL_JUMP, prevPlayerControls))
        StartGame(null);
}

void UpdateControls()
{
    if (singlePlayer || runClient)
    {
        prevPlayerControls = playerControls;
        playerControls.Set(CTRL_ALL, false);

        if (touchEnabled)
        {
            for (uint i = 0; i < input.numTouches; ++i)
            {
                TouchState@ touch = input.touches[i];

                if (touch.touchID == rotateTouchID)
                {
                    playerControls.yaw += touchSensitivity * gameCamera.fov / graphics.height * touch.delta.x;
                    playerControls.pitch += touchSensitivity * gameCamera.fov / graphics.height * touch.delta.y;
                }
                
                if (touch.touchID == moveTouchID)
                {
                    int relX = touch.position.x - touchButtonSize / 2;
                    int relY = touch.position.y - (graphics.height - touchButtonSize / 2);
                    if (relY < 0 && Abs(relX * 3 / 2) < Abs(relY))
                        playerControls.Set(CTRL_UP, true);
                    if (relY > 0 && Abs(relX * 3 / 2) < Abs(relY))
                        playerControls.Set(CTRL_DOWN, true);
                    if (relX < 0 && Abs(relY * 3 / 2) < Abs(relX))
                        playerControls.Set(CTRL_LEFT, true);
                    if (relX > 0 && Abs(relY * 3 / 2) < Abs(relX))
                        playerControls.Set(CTRL_RIGHT, true);
                }
            }
            
            if (fireTouchID >= 0)
                playerControls.Set(CTRL_FIRE, true);
        }
        
        if (input.numJoysticks > 0)
        {
            JoystickState@ joystick = input.joysticks[0];
            if (joystick.numButtons > 0)
            {
                if (joystick.buttonDown[0])
                    playerControls.Set(CTRL_JUMP, true);
                if (joystick.buttonDown[1])
                    playerControls.Set(CTRL_FIRE, true);
                if (joystick.numButtons >= 6)
                {
                    if (joystick.buttonDown[4])
                        playerControls.Set(CTRL_JUMP, true);
                    if (joystick.buttonDown[5])
                        playerControls.Set(CTRL_FIRE, true);
                }
    
                if (joystick.numHats > 0)
                {
                    if (joystick.hatPosition[0] & HAT_LEFT != 0)
                        playerControls.Set(CTRL_LEFT, true);
                    if (joystick.hatPosition[0] & HAT_RIGHT != 0)
                        playerControls.Set(CTRL_RIGHT, true);
                    if (joystick.hatPosition[0] & HAT_UP != 0)
                        playerControls.Set(CTRL_UP, true);
                    if (joystick.hatPosition[0] & HAT_DOWN != 0)
                        playerControls.Set(CTRL_DOWN, true);
                }
                if (joystick.numAxes >= 2)
                {
                    if (joystick.axisPosition[0] < -joyMoveDeadZone)
                        playerControls.Set(CTRL_LEFT, true);
                    if (joystick.axisPosition[0] > joyMoveDeadZone)
                        playerControls.Set(CTRL_RIGHT, true);
                    if (joystick.axisPosition[1] < -joyMoveDeadZone)
                        playerControls.Set(CTRL_UP, true);
                    if (joystick.axisPosition[1] > joyMoveDeadZone)
                        playerControls.Set(CTRL_DOWN, true);
                }
                if (joystick.numAxes >= 4)
                {
                    float lookX = joystick.axisPosition[joystick.numAxes - 2];
                    float lookY = joystick.axisPosition[joystick.numAxes - 1];
    
                    if (lookX < -joyLookDeadZone)
                        playerControls.yaw -= joySensitivity * lookX * lookX;
                    if (lookX > joyLookDeadZone)
                        playerControls.yaw += joySensitivity * lookX * lookX;
                    if (lookY < -joyLookDeadZone)
                        playerControls.pitch -= joySensitivity * lookY * lookY;
                    if (lookY > joyLookDeadZone)
                        playerControls.pitch += joySensitivity * lookY * lookY;
                }
            }
        }

        // For the triggered actions (fire & jump) check also for press, in case the FPS is low
        // and the key was already released
        if ((console is null) || (!console.visible))
        {
            if (input.keyDown['W'])
                playerControls.Set(CTRL_UP, true);
            if (input.keyDown['S'])
                playerControls.Set(CTRL_DOWN, true);
            if (input.keyDown['A'])
                playerControls.Set(CTRL_LEFT, true);
            if (input.keyDown['D'])
                playerControls.Set(CTRL_RIGHT, true);
            if (input.keyDown[KEY_LCTRL] || input.keyPress[KEY_LCTRL])
                playerControls.Set(CTRL_FIRE, true);
            if (input.keyDown[' '] || input.keyPress[' '])
                playerControls.Set(CTRL_JUMP, true);
        }

        if (input.mouseButtonDown[MOUSEB_LEFT] || input.mouseButtonPress[MOUSEB_LEFT])
            playerControls.Set(CTRL_FIRE, true);
        if (input.mouseButtonDown[MOUSEB_RIGHT] || input.mouseButtonPress[MOUSEB_RIGHT])
            playerControls.Set(CTRL_JUMP, true);

        playerControls.yaw += mouseSensitivity * input.mouseMoveX;
        playerControls.pitch += mouseSensitivity * input.mouseMoveY;
        playerControls.pitch = Clamp(playerControls.pitch, -60, 60);

        // In singleplayer, set controls directly on the player's ninja. In multiplayer, transmit to server
        if (singlePlayer)
        {
            Node@ playerNode = gameScene.GetChild("Player", true);
            if (playerNode !is null)
            {
                Ninja@ playerNinja = cast<Ninja>(playerNode.scriptObject);
                playerNinja.controls = playerControls;
            }
        }
        else if (network.serverConnection !is null)
        {
            // Set the latest yaw & pitch to server controls, and accumulate the buttons so that we do not miss any presses
            network.serverConnection.controls.yaw = playerControls.yaw;
            network.serverConnection.controls.pitch = playerControls.pitch;
            network.serverConnection.controls.buttons |= playerControls.buttons;

            // Tell the camera position to server for interest management
            network.serverConnection.position = gameCameraNode.worldPosition;
        }
    }

    if (runServer)
    {
        // Apply each connection's controls to the ninja they control
        for (uint i = 0; i < players.length; ++i)
        {
            Node@ playerNode = FindPlayerNode(i);
            if (playerNode !is null)
            {
                Ninja@ playerNinja = cast<Ninja>(playerNode.scriptObject);
                playerNinja.controls = players[i].connection.controls;
            }
            else
            {
                // If player has no ninja, respawn if fire/jump is pressed
                if (players[i].connection.controls.IsPressed(CTRL_FIRE | CTRL_JUMP, players[i].lastControls))
                    SpawnPlayer(players[i].connection);
            }
            players[i].lastControls = players[i].connection.controls;
        }
    }
}

void UpdateCamera()
{
    if (engine.headless)
        return;

    // On the server, use a simple freelook camera
    if (runServer)
    {
        UpdateFreelookCamera();
        return;
    }

    Node@ playerNode = FindOwnNode();
    if (playerNode is null)
        return;

    Vector3 pos = playerNode.position;
    Quaternion dir;
    
    // Make controls seem more immediate by forcing the current mouse yaw into player ninja's Y-axis rotation
    if (playerNode.vars["Health"].GetInt() > 0)
        playerNode.rotation = Quaternion(0, playerControls.yaw, 0);

    dir = dir * Quaternion(playerNode.rotation.yaw, Vector3(0, 1, 0));
    dir = dir * Quaternion(playerControls.pitch, Vector3(1, 0, 0));

    Vector3 aimPoint = pos + Vector3(0, 100, 0);
    Vector3 minDist = aimPoint + dir * Vector3(0, 0, -cameraMinDist);
    Vector3 maxDist = aimPoint + dir * Vector3(0, 0, -cameraMaxDist);

    // Collide camera ray with static objects (collision mask 2)
    Vector3 rayDir = (maxDist - minDist).Normalized();
    float rayDistance = cameraMaxDist - cameraMinDist + cameraSafetyDist;
    PhysicsRaycastResult result = gameScene.physicsWorld.RaycastSingle(Ray(minDist, rayDir), rayDistance, 2);
    if (result.body !is null)
        rayDistance = Min(rayDistance, result.distance - cameraSafetyDist);

    gameCameraNode.position = minDist + rayDir * rayDistance;
    gameCameraNode.rotation = dir;
}

void UpdateFreelookCamera()
{
    float timeStep = time.timeStep;
    float speedMultiplier = 1.0;
    if (input.keyDown[KEY_LSHIFT])
        speedMultiplier = 5.0;
    if (input.keyDown[KEY_LCTRL])
        speedMultiplier = 0.1;

    if (input.keyDown['W'])
        gameCameraNode.TranslateRelative(Vector3(0, 0, 1000) * timeStep * speedMultiplier);
    if (input.keyDown['S'])
        gameCameraNode.TranslateRelative(Vector3(0, 0, -1000) * timeStep * speedMultiplier);
    if (input.keyDown['A'])
        gameCameraNode.TranslateRelative(Vector3(-1000, 0, 0) * timeStep * speedMultiplier);
    if (input.keyDown['D'])
        gameCameraNode.TranslateRelative(Vector3(1000, 0, 0) * timeStep * speedMultiplier);

    playerControls.yaw += mouseSensitivity * input.mouseMoveX;
    playerControls.pitch += mouseSensitivity * input.mouseMoveY;
    playerControls.pitch = Clamp(playerControls.pitch, -90, 90);
    gameCameraNode.rotation = Quaternion(playerControls.pitch, playerControls.yaw, 0);
}

void UpdateStatus()
{
    if (engine.headless || runServer)
        return;

    if (singlePlayer)
    {
        if (players.length > 0)
            scoreText.text = "Score " + players[0].score;
        if (hiscores.length > 0)
            hiscoreText.text = "Hiscore " + hiscores[0].score;
    }

    Node@ playerNode = FindOwnNode();
    if (playerNode !is null)
    {
        int health = 0;
        if (singlePlayer)
        {
            GameObject@ object = cast<GameObject>(playerNode.scriptObject);
            health = object.health;
        }
        else
        {
            // In multiplayer the client does not have script logic components, but health is replicated via node user variables
            health = playerNode.vars["Health"].GetInt();
        }
        healthBar.width = 116 * health / playerHealth;
    }
}

void HandleScreenMode()
{
    int height = graphics.height / 22;
    if (height > 64)
        height = 64;
    sight.SetSize(height, height);
    messageText.SetPosition(0, -height * 2);
}
