using UnityEngine;

namespace Scripting.Objects
{
    public class BaseballBat : MonoBehaviour
    {
        [SerializeField] private CapsuleCollider capsuleCollider;

        private void OnValidate()
        {
            if (!capsuleCollider) capsuleCollider = GetComponent<CapsuleCollider>();
        }

        public void PickUp(Transform weaponPosition)
        {
            transform.parent = weaponPosition;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public void Drop()
        {
            transform.parent = null;
            transform.position = new Vector3(5.796f, 0.489f, -5.296f);
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, -16.817f);
        }
    }
}
