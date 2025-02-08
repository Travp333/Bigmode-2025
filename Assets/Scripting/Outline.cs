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
        
        public bool isActive;


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
                if (isActive)
                {
                    isActive = false;
                    materialRenderer.materials = _originalMaterials.ToArray();
                }

                return;
            }

            var ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, mask))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    if (!isActive)
                    {
                        isActive = true;
                        // Add outline by extending the materials array
                        var newMaterials = new Material[_originalMaterials.Length + 1];
                        for (var i = 0; i < _originalMaterials.Length; i++)
                            newMaterials[i] = _originalMaterials[i];  // Copy original materials
            
                        newMaterials[^1] = toonMaterial; // Add outline material
                        materialRenderer.materials = newMaterials;
                    }
                }
                else
                {
                    if (isActive)
                    {
                        isActive = false;
                        materialRenderer.materials = _originalMaterials.ToArray();
                    }
                }
            }
            else
            {
                if (isActive)
                {
                    isActive = false;
                    materialRenderer.materials = _originalMaterials.ToArray();
                }
            }
        }
    }
}
