using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripting
{
    public class TutorialManager : MonoBehaviour
    {
        private static TutorialManager _singleton;

        public static TutorialManager Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    Debug.Log($"{nameof(TutorialManager)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }

        private readonly List<TutorialSpot> _spots = new();

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
            if (GameManager.Singleton.upgrades.tutorialDone)
            {
                _spots.ForEach(n => n.Hide());
                return;
            }
        
            _spots.ForEach(n =>
            {
                if (n.AtSpawnVisible)
                {
                    n.Show();
                }
                else
                {
                    n.Hide();
                }
            });
        }

        public void ShowOrderNumber(int number, bool only = false)
        {
            if (GameManager.Singleton.upgrades.tutorialDone)
            {
                _spots.ForEach(n => n.Hide());
                return;
            }
        
            if (only)
                _spots.ToList()
                    .ForEach(n => n.Hide());

            _spots.Where(n => n.Order == number)
                .ToList()
                .ForEach(n => n.Show());
        }
    
        public void ShowOrderNumber(IEnumerable<int> numbers, bool only = false)
        {
            if (GameManager.Singleton.upgrades.tutorialDone)
            {
                _spots.ForEach(n => n.Hide());
                return;
            }
        
            if (only)
                _spots.ToList()
                    .ForEach(n => n.Hide());

            _spots.Where(n => numbers.Contains(n.Order))
                .ToList()
                .ForEach(n => n.Show());
        }

        public void HideOrderNumber(int number)
        {
            _spots.Where(n => n.Order == number)
                .ToList()
                .ForEach(n => n.Hide());
        }

        public void Register(TutorialSpot spot)
        {
            if (GameManager.Singleton.upgrades.tutorialDone)
            {
                _spots.ForEach(n => n.Hide());
                return;
            }
        
            if (spot.AtSpawnVisible)
            {
                spot.Show();
            }
            else
            {
                spot.Hide();
            }

            _spots.Add(spot);
        }
    }
}
