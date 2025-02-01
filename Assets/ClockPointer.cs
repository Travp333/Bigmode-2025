using UnityEngine;

public class ClockPointer : MonoBehaviour
{
    [SerializeField] private Transform pivotPoint; // Assign the pivot GameObject in the Inspector
    [SerializeField] private GameObject pointer;

    public void SetDayTime(float value)
    {
        var angle = Mathf.Lerp(0, 360, value); // Convert 0-1 range to degrees

        var vector3 = pivotPoint.position +
                      Quaternion.Euler(angle, 0, 0) * Vector3.up * 0.25f;

        pointer.transform.position = vector3;
        pointer.transform.rotation = Quaternion.Euler(angle, 0, 0); // Rotate pointer
    }
}
