using UnityEngine;

public class StorePicker : MonoBehaviour
{
    private Camera _cam;

    void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        var screenRay = _cam.ScreenPointToRay(new Vector3(_cam.pixelWidth / 2f, _cam.pixelHeight / 2f, 0));
        //var rayResults = Physics.BoxCastAll(screenRay.GetPoint(0.0f), Vector3.one * 0.1f, screenRay.GetPoint(2.0f));
        var rayResults = Physics.RaycastAll(screenRay, 2.0f);
        if (rayResults.Length <= 0)
        {
            FlavorTextManager.Singleton.ClearFlavorText();
            return;
        }

        foreach (var hit in rayResults)
        {
            if (hit.collider.gameObject.TryGetComponent<UpgradeButton>(out var temp))
            {
                if (temp.beenPressed == false)
                {
                    //show flavor text
                    //temp.ShowFlavorText();
                    FlavorTextManager.Singleton.UpdateFlavorText(temp);

                    if (Input.GetMouseButtonDown(0))
                    {
                        temp.Pressed();
                    }

                    break;
                }
            }

            if (hit.collider.gameObject.TryGetComponent<LightSwitch>(out var tempTemp))
            {
                //tempTemp.ShowFlavorText();
                FlavorTextManager.Singleton.UpdateFlavorText(tempTemp);

                if (Input.GetMouseButtonDown(0))
                {
                    tempTemp.Pressed();
                }

                break;
            }

            FlavorTextManager.Singleton.ClearFlavorText();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var screenRay =
            Camera.main.ScreenPointToRay(new Vector3(Camera.main.pixelWidth / 2f, Camera.main.pixelHeight / 2f, 0));
        Gizmos.DrawSphere(screenRay.GetPoint(2.0f), 0.1f);
        //Gizmos.DrawRay(screenRay);
    }
#endif
}
