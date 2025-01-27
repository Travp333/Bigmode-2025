using Scripting;
using System.Collections;
using UnityEngine;

public class StorePicker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var screenRay = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));
        //var rayResults = Physics.BoxCastAll(screenRay.GetPoint(0.0f), Vector3.one * 0.1f, screenRay.GetPoint(2.0f));
        var rayResults = Physics.RaycastAll(screenRay, 2.0f);
        foreach (var hit in rayResults)
        {
            UpgradeButton temp;
            if (hit.collider.gameObject.TryGetComponent<UpgradeButton>(out temp))
            {
                //show flavor text
                temp.showFlavorText();

                if (Input.GetMouseButtonDown(0))
                {
                    temp.pressed();
                }
                break;
            }

        }
    }

    private void OnDrawGizmos()
    {
        var screenRay = Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0));
        Gizmos.DrawSphere(screenRay.GetPoint(2.0f), 0.1f);
        //Gizmos.DrawRay(screenRay);

    }

}
