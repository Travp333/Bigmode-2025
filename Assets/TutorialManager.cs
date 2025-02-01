using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    private void Awake()
    {
        Singleton = this;
    }

    private List<TutorialSpot> _spots = new();

    private int _currentTutorial;

    public void ShowOrderNumber(int number)
    {
        _spots.Where(n => n.Order == number)
            .ToList()
            .ForEach(n => n.Show());
    }

    public void HideOrderNumber(int number)
    {
        _spots.Where(n => n.Order == number)
            .ToList()
            .ForEach(n => n.Hide());
    }

    public void Next()
    {
        _spots.ForEach(n =>
        {
            if (n.IsVisible)
            {
                n.Hide();
            }
        });

        _currentTutorial++;

        _spots.Where(n => n.Order == _currentTutorial)
            .ToList()
            .ForEach(n => n.Show());
    }

    public void Register(TutorialSpot spot)
    {
        if (spot.Order == _currentTutorial)
        {
            spot.Show();
        }

        _spots.Add(spot);
    }
}
