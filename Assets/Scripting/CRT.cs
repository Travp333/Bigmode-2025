using UnityEngine;

namespace Scripting
{
    public class CRT : MonoBehaviour
    {
        public static CRT Instance;
        [SerializeField] AudioSource myVendSound;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Vend()
        {
            myVendSound.Play();
        }
    }
}
