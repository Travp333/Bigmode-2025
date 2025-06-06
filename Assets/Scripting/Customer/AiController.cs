using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripting.Customer
{
    public class AiController : MonoBehaviour
    {
        [Header("Yo Wassup")]
        [SerializeField] private Transform entrancePoint;

        [Header("IdleSpots")]
        [SerializeField] private Transform leftTopSpot;

        [SerializeField] private Transform rightBottomSpot;

        [Header("Walking Around")]
        [SerializeField] private List<AiSpot> aiSpotsPainting;

        [SerializeField] private List<AiSpot> aiSpotsChairs;
        [SerializeField] private GameObject assistantSpot;
        [SerializeField] private List<VandalismSpot> vandalismSpots;
        [SerializeField] private GameObject thiefSpot;

        [Header("Map")]
        [SerializeField] private List<DespawnPoint> despawnPoints;

        private bool _thiefSpotLocked;

        public Transform EntrancePoint => entrancePoint;

        private List<AiSpot> _distractionSpots = new();

        private static AiController _singleton;

        public bool AssistantActive { get; private set; }

        public bool AssistantLocked { get; set; }

        public List<VandalismSpot> VandalismSpotList => vandalismSpots;

        public void UnlockEverything()
        {
            AssistantLocked = false;
            _distractionSpots.ForEach(n => n.Unlock());
            _thiefSpotLocked = false;
        }

        public static AiController Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    Debug.Log($"{nameof(SpecialStoreManager)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }

        public void AddDistractionSpots(List<AiSpot> aiSpots)
        {
            _distractionSpots.AddRange(aiSpots);
        }

        private void Awake()
        {
            Singleton = this;
        }

        public Vector3 GetRandomPoint()
        {
            var randomX = Random.Range(leftTopSpot.position.x, rightBottomSpot.position.x);
            var randomZ = Random.Range(rightBottomSpot.position.z, leftTopSpot.position.z);
            var fixedY = leftTopSpot.position.y;

            return new Vector3(randomX, fixedY, randomZ);
        }

        public AiSpot GetFreeSpot()
        {
            var listFreeSpots = _distractionSpots.Where(n => !n.IsOccupied).ToList();
            if (!listFreeSpots.Any())
                return null;

            return listFreeSpots[Random.Range(0, listFreeSpots.Count)];
        }

        public Vector3 GetRandomWaitingSpot() => GetRandomPoint();

        public DespawnPoint GetRandomDespawnPoint() => despawnPoints[Random.Range(0, despawnPoints.Count)];

        public void SetAssistantActive()
        {
            AssistantActive = true;
        }

        public void ActivateChairDistractionSpots()
        {
            Singleton.AddDistractionSpots(aiSpotsChairs);
        }

        public void ActivatePaintingDistractionSpots()
        {
            Singleton.AddDistractionSpots(aiSpotsPainting);
        }

        public VandalismSpot GetRandomVandalismSpot()
        {
            var availableSpots = vandalismSpots.Where(n => !n.IsLocked).ToArray();

            return availableSpots.Any() ? availableSpots[Random.Range(0, availableSpots.Length)] : null;
        }

        public bool HasVandalismSpots => vandalismSpots.Any(n => !n.IsVisible);

        public GameObject AssistantSpot => assistantSpot;

        public bool HasThiefSpot => !_thiefSpotLocked;

        public GameObject GetThiefSpot() => HasThiefSpot ? thiefSpot : null;

        public void SetThiefLocked(bool value)
        {
            _thiefSpotLocked = value;
        }
    }
}
