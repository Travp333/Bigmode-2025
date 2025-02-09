using UnityEngine;

namespace Scripting
{
    public class Death : MonoBehaviour
    {
        [SerializeField]
        private float deathLowerBound = 1f;

        [SerializeField]
        private float deathUpperBound = 3f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            Invoke(nameof(Die), Random.Range(deathLowerBound, deathUpperBound));
        }

        private void Die()
        {
            Destroy(gameObject);
        }
    }
}
