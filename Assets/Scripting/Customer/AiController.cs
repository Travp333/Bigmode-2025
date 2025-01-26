using System.Collections.Generic;
using System.Linq;
using AI;
using UnityEngine;

namespace Scripting.Customer
{
    public class AiController : MonoBehaviour
    {
        [SerializeField] private GameObject entrancePoint;
        
        [SerializeField] private List<AiSpot> tableWaitingSpots;
        [SerializeField] private List<DespawnPoint> despawnPoints;
        [SerializeField] private GameManager gameManager;
        
        public GameObject EntrancePoint => entrancePoint;

        private List<AiSpot> _distractionSpots;

        void Start()
        {
            _distractionSpots = gameManager.GetDistractionSpots();
        }
        
        public AiSpot GetFreeSpot()
        {
            var listFreeSpots = _distractionSpots.Where(n => !n.IsOccupied && n.IsBought).ToList();
            if (!listFreeSpots.Any())
                return null;
            
            return listFreeSpots[Random.Range(0, listFreeSpots.Count)];
        }

        public AiSpot GetTableSpot()
        {
            return tableWaitingSpots[Random.Range(0, tableWaitingSpots.Count)];
        }
 
        public DespawnPoint GetRandomDespawnPoint()
        {
            return despawnPoints[Random.Range(0, despawnPoints.Count)];
        }
    }
}
