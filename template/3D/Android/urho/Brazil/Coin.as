#include "Brazil/GameObject.as"

class Coin : GameObject
{
    Coin()
    {
		Print("Constructing a coin");
		buildBody();
    }

	void buildBody()
	{
		Print("Coin::buildBody()");
		Node@ coinNode = node.CreateChild("Coin");
		coinNode.position = Vector3(-10, 0, 0);
		coinNode.scale = Vector3(1, 1, 1);

		StaticModel@ coinObject = coinNode.CreateComponent("StaticModel");
		coinObject.model = cache.GetResource("Model", "Models/Coin.mdl");
		coinObject.material = cache.GetResource("Material", "Materials/BrightYellowUnlit.xml");
		coinObject.castShadows = true;
	}

    void Start()
    {
        SubscribeToEvent(node, "NodeCollision", "HandleNodeCollision");
    }
}