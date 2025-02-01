using Scripting.Desk;
using UnityEngine;

public class PhoneReferenceHolder : MonoBehaviour
{
    [SerializeField] private GameObject deskReciever;
    [SerializeField] private GameObject handReciever;
    [SerializeField] private DeskArms arms;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void PickupPhone()
    {
        arms.BlockLeftHand();
        deskReciever.SetActive(false);
        handReciever.SetActive(true);
    }

    public void HangupPhone()
    {
        Invoke(nameof(DelayedUnBlock), .3f);
        deskReciever.SetActive(true);
        handReciever.SetActive(false);
    }

    private void DelayedUnBlock()
    {
        arms.UnblockLeftHand();
    }
}
