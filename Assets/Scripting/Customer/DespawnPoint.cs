using UnityEngine;

namespace Scripting.Customer
{
    public class DespawnPoint : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<CustomerMotor>(out var customerMotor))
            {
                GameManager.Singleton.RemoveCustomer(customerMotor);
            }
            Destroy(other.gameObject);
        }
    }
}
