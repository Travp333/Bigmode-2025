using UnityEngine;

namespace Scripting
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField] private Sprite[] sprites;

        private int? _index;

        private SpriteRenderer _renderer;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _renderer.enabled = false;
        }

        public void SetSprite(int? index)
        {
            _index = index;
            if (index.HasValue)
            {
                _renderer.enabled = true;
                _renderer.sprite = sprites[_index.Value];
            }
            else
            {
                _renderer.enabled = false;
            }
        }

        void LateUpdate()
        {
            transform.LookAt(Camera.main.transform.position, Vector3.up);
        }
    }
}
