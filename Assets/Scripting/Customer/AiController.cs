using System.Collections.Generic;
using System.Linq;
using AI;
using UnityEngine;

namespace Scripting.Customer
{
    public class AiController : MonoBehaviour
    {
        [SerializeField] private GameObject entrancePoint;
        
        [SerializeField] private List<AiSpot> distractionSpots;
        [SerializeField] private List<AiSpot> tableWaitingSpots;
        [SerializeField] private List<DespawnPoint> despawnPoints;
        [SerializeField] private GameObject deskChairSpot;
        [SerializeField] private GameObject leaveChairSpot;
        
        public GameObject EntrancePoint => entrancePoint;
        
        public AiSpot GetFreeSpot()
        {
            var listFreeSpots = distractionSpots.Where(n => !n.IsOccupied && n.IsBought).ToList();
            if (!listFreeSpots.Any())
                return null;
            
            return listFreeSpots[Random.Range(0, listFreeSpots.Count)];
        }

        public AiSpot GetTableSpot()
        {
            return tableWaitingSpots[Random.Range(0, tableWaitingSpots.Count)];
        }

        public GameObject GetDeskChairSpot()
        {
            return deskChairSpot;
        }
        
        public GameObject GetDeskLeaveSpot()
        {
            return leaveChairSpot;
        }

        public DespawnPoint GetRandomDespawnPoint()
        {
            return despawnPoints[Random.Range(0, despawnPoints.Count)];
        }
    }
}
