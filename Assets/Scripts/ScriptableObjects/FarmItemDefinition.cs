using UnityEngine;

namespace ChickenCoop.Data
{
    /// <summary>
    /// FarmItemDefinition - Defines a resource item in the game (e.g., Corn, Egg, Milk).
    /// </summary>
    [CreateAssetMenu(fileName = "NewItemDefinition", menuName = "ChickenCoop/Item Definition")]
    public class FarmItemDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public Sprite icon;
        public int basePrice;
        public Color themeColor = Color.white;
    }
}
