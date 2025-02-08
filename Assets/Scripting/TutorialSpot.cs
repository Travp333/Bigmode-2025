using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripting
{
    public class TutorialSpot : MonoBehaviour
    {
        [SerializeField] private int order;
        [SerializeField] private string text;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private Sprite sprite;
        [SerializeField] private TextMeshPro textMesh;
        [SerializeField] private bool rotateUpDown;
        [SerializeField] private bool rotateLeftRight;

        [SerializeField] private GameObject rendererObject;
        [SerializeField] private GameObject textObject;

        [FormerlySerializedAs("spawnVisible")] [SerializeField]
        private bool atSpawnVisible;

        private bool _isActive;
        private bool _isVisible;

        public bool IsActive => _isActive;
        public bool IsVisible => _isVisible;
        public bool AtSpawnVisible => atSpawnVisible;
        public int Order => order;

        private void Awake()
        {
            _isActive = true;
            spriteRenderer.sprite = sprite;
            textMesh.text = text;

            name += $" ID: {order}";
        }

        private void Start()
        {
            if (GameManager.Singleton.upgrades.tutorialDone)
            {
                Hide();
            }
            else
                TutorialManager.Singleton.Register(this);
        }

        public void OnValidate()
        {
            if (!spriteRenderer)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (!textMesh)
                textMesh = GetComponentInChildren<TextMeshPro>();
        }

        public void Show()
        {
            _isActive = true;
        }

        public void Hide()
        {
            _isActive = false;
        }

        public void Update()
        {
            if (!_isActive && rendererObject.activeInHierarchy)
            {
                rendererObject.SetActive(false);
                textObject.SetActive(false);
            }

            if (_isActive && !rendererObject!.activeInHierarchy)
            {
                rendererObject.SetActive(true);
                textObject.SetActive(true);
            }

            if (_isActive && (rotateLeftRight || rotateUpDown))
            {
                var originalRotation = transform.localRotation.eulerAngles;
                transform.LookAt(Camera.main.transform.position);

                var currentLocalRotation = transform.localRotation.eulerAngles;

                if (!rotateLeftRight)
                    currentLocalRotation.y = originalRotation.y;

                if (!rotateUpDown)
                    currentLocalRotation.x = originalRotation.x;

                transform.localRotation = Quaternion.Euler(currentLocalRotation);
            }
        }
    }
}
