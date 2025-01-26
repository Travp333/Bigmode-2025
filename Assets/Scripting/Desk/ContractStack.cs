using UnityEngine;

namespace Scripting.Desk
{
    public class ContractStack : MonoBehaviour
    {
        [SerializeField] private GameObject contract;
        [SerializeField] private GameObject attachPoint;

        private GameObject _currentContract;
        private PlayerInput _input;
        private bool _isUp;
        
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

        private void OnMouseDown()
        {
            Debug.Log("OnMouseDown");
            if (Input.GetMouseButtonDown(0))
            {
                Instantiate(contract, attachPoint.transform.position, attachPoint.transform.rotation, attachPoint.transform);
            }
        }
    }
}
