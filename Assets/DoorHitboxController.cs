using UnityEngine;

public class DoorHitboxController : MonoBehaviour
{
    [SerializeField] private DoorAnimController door;
    [SerializeField] private bool frontOrBack;


    private void OnTriggerEnter(Collider other)
    {
        if(!door.isOpen){
            if(other.gameObject.GetComponent<LaunchDetection>()!=null){
                //ai approacing door!
                if(other.gameObject.GetComponent<LaunchDetection>().lerpGate){
                    //Debug.Log("NPC Flying, SLAM OPEN");
                    door.SlamOpenDoor();
                }
                else if(frontOrBack){
                    //Debug.Log("opening from back");
                    door.OpenDoorFromBack();
                }
                else{
                    //Debug.Log("opening from front");
                    door.OpenDoorFromFront();
                }
            }
        }
        else{            
            //Still slam door open other direction even if its already open for effect
            if(other.gameObject.GetComponent<LaunchDetection>()!=null){
                //ai approacing door!
                if(other.gameObject.GetComponent<LaunchDetection>().lerpGate){
                    //Debug.Log("NPC Flying, SLAM OPEN");
                    door.SlamOpenDoor();
                }
            }

        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        {
            
        }
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
