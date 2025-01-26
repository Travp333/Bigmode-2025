using UnityEngine;

namespace Scripting.Desk
{
    public class ContractStack : MonoBehaviour
    {
        [SerializeField] private GameObject contract;
        [SerializeField] private GameObject attachPoint;

        private GameObject _currentContract;
        private PlayerInput _input;

        private void Awake()
        {
            _input = new PlayerInput();
        }

        private void OnEnable()
        {
            _input?.Enable();
        }

        private void OnDisable()
        {
            _input?.Disable();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        Instantiate(contract, attachPoint.transform);
                    }
                }
            }
        }
    }
}
