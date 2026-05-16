using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ChickenCoop.Managers
{
    public sealed class SceneRegistry : MonoBehaviour
    {
        public static SceneRegistry Instance { get; private set; }

        [Header("Core Roots")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private EnvironmentManager environmentManager;
        [SerializeField] private PlayerController player;
        [SerializeField] private StoreCounter store;
        [SerializeField] private Transform helperSpawn;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Canvas canvas;
        [SerializeField] private EventSystem eventSystem;

        [Header("Zones")]
        [SerializeField] private List<FarmZoneController> zones = new List<FarmZoneController>();

        public GameManager GameManager => gameManager;
        public UIManager UIManager => uiManager;
        public EnvironmentManager EnvironmentManager => environmentManager;
        public PlayerController Player => player;
        public StoreCounter Store => store;
        public Transform HelperSpawn => helperSpawn;
        public Camera MainCamera => mainCamera;
        public Canvas Canvas => canvas;
        public EventSystem EventSystem => eventSystem;
        public IReadOnlyList<FarmZoneController> Zones => zones;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            RefreshCache();
        }

        public void RefreshCache()
        {
            zones.RemoveAll(zone => zone == null);

            if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
            if (uiManager == null) uiManager = FindFirstObjectByType<UIManager>();
            if (environmentManager == null) environmentManager = FindFirstObjectByType<EnvironmentManager>();
            if (player == null) player = FindFirstObjectByType<PlayerController>();
            if (store == null) store = FindFirstObjectByType<StoreCounter>();
            if (mainCamera == null) mainCamera = Camera.main;
            if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
            if (eventSystem == null) eventSystem = FindFirstObjectByType<EventSystem>();

            FarmZoneController[] foundZones = FindObjectsByType<FarmZoneController>(FindObjectsSortMode.None);
            foreach (FarmZoneController zone in foundZones)
            {
                if (zone != null && !zones.Contains(zone))
                {
                    zones.Add(zone);
                }
            }

            if (helperSpawn == null && player != null)
            {
                helperSpawn = player.transform;
            }
        }

        public FarmZoneController GetZone(string zoneId)
        {
            if (string.IsNullOrWhiteSpace(zoneId))
            {
                return null;
            }

            RefreshCache();
            return zones.FirstOrDefault(zone => zone != null && zone.ZoneIdMatches(zoneId));
        }

        public bool Validate(out string[] issues)
        {
            RefreshCache();

            List<string> results = new List<string>();
            if (gameManager == null) results.Add("GameManager missing");
            if (uiManager == null) results.Add("UIManager missing");
            if (environmentManager == null) results.Add("EnvironmentManager missing");
            if (player == null) results.Add("Player missing");
            if (store == null) results.Add("Store missing");
            if (canvas == null) results.Add("Canvas missing");
            if (eventSystem == null) results.Add("EventSystem missing");
            if (zones.Count == 0) results.Add("No FarmZoneController roots found");

            issues = results.ToArray();
            return issues.Length == 0;
        }
    }
}
