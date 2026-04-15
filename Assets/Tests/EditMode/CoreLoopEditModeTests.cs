using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CoreLoopEditModeTests
{
    private GameObject gameManagerObject;
    private GameManager gameManager;

    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteAll();
        ResetGameManagerSingleton();

        gameManagerObject = new GameObject("GameManager_Test");
        gameManager = gameManagerObject.AddComponent<GameManager>();
    }

    [TearDown]
    public void TearDown()
    {
        if (gameManagerObject != null)
        {
            Object.DestroyImmediate(gameManagerObject);
        }

        foreach (HarvestableField field in Object.FindObjectsByType<HarvestableField>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(field.gameObject);
        }

        foreach (Chicken chicken in Object.FindObjectsByType<Chicken>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(chicken.gameObject);
        }

        foreach (StoreCounter store in Object.FindObjectsByType<StoreCounter>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(store.gameObject);
        }

        foreach (CollectibleEgg egg in Object.FindObjectsByType<CollectibleEgg>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(egg.gameObject);
        }

        PlayerPrefs.DeleteAll();
        ResetGameManagerSingleton();
    }

    [Test]
    public void Harvest_AddsCornImmediately()
    {
        GameObject fieldObject = new GameObject("CornField_Test");
        fieldObject.AddComponent<SpriteRenderer>();
        HarvestableField field = fieldObject.AddComponent<HarvestableField>();

        Assert.AreEqual(0, gameManager.Corn);

        field.Harvest();

        Assert.AreEqual(1, gameManager.Corn);
    }

    [Test]
    public void Feed_ConsumesCornImmediately()
    {
        GameObject chickenObject = new GameObject("Chicken_Test");
        chickenObject.AddComponent<SpriteRenderer>();
        chickenObject.AddComponent<CircleCollider2D>();
        Chicken chicken = chickenObject.AddComponent<Chicken>();

        gameManager.AddCorn(1);
        Assert.AreEqual(1, gameManager.Corn);
        Assert.IsTrue(chicken.CanInteract());

        chicken.Feed();

        Assert.AreEqual(0, gameManager.Corn);
    }

    [Test]
    public void SellEgg_ConvertsEggToCoins()
    {
        GameObject storeObject = new GameObject("Store_Test");
        storeObject.AddComponent<SpriteRenderer>();
        storeObject.AddComponent<BoxCollider2D>();
        StoreCounter store = storeObject.AddComponent<StoreCounter>();

        gameManager.AddEgg(1);
        int coinsBefore = gameManager.Coins;

        store.SellEgg();

        Assert.AreEqual(0, gameManager.Eggs);
        Assert.AreEqual(coinsBefore + gameManager.EggSellPrice, gameManager.Coins);
    }

    private static void ResetGameManagerSingleton()
    {
        FieldInfo backingField = typeof(GameManager).GetField("<Instance>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic);
        backingField?.SetValue(null, null);
    }
}
