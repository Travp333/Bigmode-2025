using UnityEngine;

namespace Scripting.Customer
{
    public class DespawnPoint : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Destroy(other.gameObject);
        }
    }
}
