using UnityEngine;

public class HuellController : MonoBehaviour
{
    [SerializeField] private GameObject footHitbox;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void HideHitbox(){
        footHitbox.SetActive(false);
    }
    public void ShowHitbox(){
        footHitbox.SetActive(true);
    }

    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
