using UnityEngine;

namespace ChickenCoop.Interfaces
{
    /// <summary>
    /// Interface for any object that can be harvested (Crops, Milk, etc.)
    /// </summary>
    public interface IHarvestable : IInteractable
    {
        bool IsReadyToHarvest();
        void Harvest();
        float GetGrowthProgress();
    }

    /// <summary>
    /// Interface for any object that can be fed (Chickens, Cows, etc.)
    /// </summary>
    public interface IFeedable : IInteractable
    {
        bool NeedsFeeding();
        void Feed(string itemID);
        bool CanAcceptFood(string itemID);
        float GetProductionProgress();
    }

    /// <summary>
    /// Interface for members within a FarmZone.
    /// </summary>
    public interface IZoneMember
    {
        void Initialize(string zoneID, int slotIndex);
    }
}
