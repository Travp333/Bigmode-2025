using System.Linq;
using UnityEngine;

namespace Scripting
{
    public class Outline : MonoBehaviour
    {
        [SerializeField] private Material toonMaterial;
        [SerializeField] private Renderer materialRenderer;
        [SerializeField] private float interactRayLength = 3.0f;
        [SerializeField] private LayerMask mask;

        private Camera _cam;
        private Material[] _originalMaterials;
        private bool _deactivate;
        private bool _isActive;

        public bool OutlineActive => _isActive;

        public void IsToonable(bool value)
        {
            _deactivate = !value;
        }

        private void OnValidate()
        {
            if (!materialRenderer)
                materialRenderer = GetComponent<MeshRenderer>();
        }

        void Awake()
        {
            _originalMaterials = materialRenderer.materials;
            _cam = Camera.main;
        }

        // Update is called once per frame
        void Update()
        {
            if (_deactivate)
            {
                if (_isActive)
                {
                    _isActive = false;
                    materialRenderer.materials = _originalMaterials.ToArray();
                }

                return;
            }

            var ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, interactRayLength, mask))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    if (!_isActive)
                    {
                        _isActive = true;

                        materialRenderer.materials = materialRenderer.materials.Concat(new[] {toonMaterial}).ToArray();
                    }
                }
                else
                {
                    if (_isActive)
                    {
                        _isActive = false;
                        materialRenderer.materials = materialRenderer.materials
                            .Where(n => n.shader != toonMaterial.shader).ToArray();
                    }
                }
            }
            else
            {
                if (_isActive)
                {
                    _isActive = false;
                    materialRenderer.materials = materialRenderer.materials.Where(n => n.shader != toonMaterial.shader)
                        .ToArray();
                    // materialRenderer.materials = _originalMaterials.ToArray();
                }
            }
        }
    }
}
