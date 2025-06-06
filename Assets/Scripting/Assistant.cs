using System.Collections;
using Scripting.Customer;
using UnityEditor;
using UnityEngine;

namespace Scripting
{
    public class Assistant : MonoBehaviour
    {
        private static Assistant _singleton;
        [SerializeField] AudioSource myBubbleGum;
        [SerializeField] private GameObject bg;

        public static Assistant Singleton
        {
            get => _singleton;
            private set
            {
                if (_singleton == null)
                    _singleton = value;
                else if (_singleton != value)
                {
                    Debug.Log($"{nameof(SpecialStoreManager)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }

        private void Awake()
        {
            Singleton = this;
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.Label(transform.position + transform.up * 4f, "locked: " + AiController.Singleton.AssistantLocked);
        }
#endif

        public void PopBubbleGum()
        {
            myBubbleGum.Play();
            StartCoroutine(BubbleGum());
        }

        public IEnumerator BubbleGum()
        {
            var elapsed = 0.0f;

            var gum = Instantiate(bg, transform);
            gum.transform.localScale = Vector3.one * 0.001f;

            while (elapsed < 0.85f)
            {
                elapsed += Time.deltaTime;

                gum.transform.localScale = Vector3.Lerp(Vector3.one * 0.001f, Vector3.one * 0.5f, elapsed);

                yield return null;
            }

            Destroy(gum);
        }
    }
}
