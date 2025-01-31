using UnityEngine;

public class Assistant : MonoBehaviour
{
    private static Assistant _singleton;
    [SerializeField] AudioSource myBubbleGum;

    public static Assistant Singleton
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

    private void Awake()
    {
        Singleton = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PayBubbleGum()
    {
        myBubbleGum.Play();
    }
}
