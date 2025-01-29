using Scripting.Desk;
using UnityEngine;

public class PhoneReferenceHolder : MonoBehaviour
{
    [SerializeField]
    GameObject deskReciever;
    [SerializeField]
    GameObject handReciever;
    [SerializeField]
    DeskArms arms; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void PickupPhone(){
        arms.BlockLeftHand();
        deskReciever.SetActive(false);
        handReciever.SetActive(true);
    }
    void HangupPhone(){
        Invoke("DelayedUnBlock", .3f);
        deskReciever.SetActive(true);
        handReciever.SetActive(false);
    }
    void DelayedUnBlock(){
        arms.UnblockLeftHand();
    }

}
