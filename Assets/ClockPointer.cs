using UnityEngine;

public class ClockPointer : MonoBehaviour
{
    [SerializeField] private GameObject pointer;

    public void SetDayTime(float value)
    {
        var angle = Mathf.Lerp(0, 360, value); // Convert 0-1 range to degrees

        pointer.transform.localRotation = Quaternion.Euler(-angle, 90, 0);
        pointer.transform.localPosition = Quaternion.Euler(-angle, 90, 0) * Vector3.up * 0.201f; // Rotate pointer
    }
}
