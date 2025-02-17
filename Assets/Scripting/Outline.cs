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
        [SerializeField] private bool isSeatedActive;
        [SerializeField] private bool isNightActive;
        [SerializeField] private bool isDayActive;

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
            if (GameManager.Singleton.IsPause)
                return;

            if (isSeatedActive != GameManager.Singleton.Player.IsSeated ||
                isNightActive != GameManager.Singleton.IsNightTime)
            {
                if (_isActive)
                {
                    _isActive = false;
                    materialRenderer.materials = materialRenderer.materials.Where(n => n.shader != toonMaterial.shader)
                        .ToArray();
                }

                return;
            }

            if (_deactivate)
            {
                if (_isActive)
                {
                    _isActive = false;
                    materialRenderer.materials = materialRenderer.materials.Where(n => n.shader != toonMaterial.shader)
                        .ToArray();
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
