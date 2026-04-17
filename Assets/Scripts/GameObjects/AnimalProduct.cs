using UnityEngine;
using ChickenCoop.Interfaces;
using ChickenCoop.Managers;

public class AnimalProduct : MonoBehaviour, IInteractable, IFeedable, IZoneMember
{
    [SerializeField] private string productId = "Milk";
    [SerializeField] private float productionTime = 12f;
    
    private float productionTimer = 0f;
    private bool isHungry = false;
    private bool isProducing = false;
    private string zoneId;
    private int slotIndex;
    
    public bool CanInteract() => !isHungry && isProducing;
    public bool NeedsFeeding() => isHungry;
    public float GetProductionProgress() => productionTimer / productionTime;
    public bool CanAcceptFood(string itemID) => isHungry;
    
    public void Initialize(string outputId, float prodTime)
    {
        productId = outputId;
        productionTime = prodTime;
        productionTimer = productionTime; // Start ready to produce
        isProducing = true;
    }
    
    public void Initialize(string zone, int index)
    {
        zoneId = zone;
        slotIndex = index;
    }
    
    private void Update()
    {
        if (isProducing && !isHungry)
        {
            productionTimer -= Time.deltaTime * GameManager.Instance.SpeedMultiplier;
            if (productionTimer <= 0)
            {
                productionTimer = 0;
                isProducing = true;
            }
        }
    }
    
    public bool Interact()
    {
        if (isHungry) return false;
        if (productionTimer > 0) return false;
        
        // Produce and reset
        GameManager.Instance.AddItem(productId, 1, transform.position);
        productionTimer = productionTime;
        return true;
    }
    
    public void Feed(string itemID)
    {
        if (!isHungry) return;
        isHungry = false;
        isProducing = true;
        productionTimer = productionTime;
        
        // Visual feedback
        SpawnHeartEffect();
    }
    
    private void SpawnHeartEffect()
    {
        GameObject heart = new GameObject("Heart");
        heart.transform.position = transform.position + Vector3.up * 1.5f;
        var sr = heart.AddComponent<SpriteRenderer>();
        sr.color = Color.red;
        heart.transform.localScale = Vector3.one * 0.5f;
        Destroy(heart, 0.5f);
    }
}
