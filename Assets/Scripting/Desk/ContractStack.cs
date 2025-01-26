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
                Debug.Log("YO");
                
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        
                if (Physics.Raycast(ray, out var hit))
                {
                    
                    Debug.Log("YO2");

                    Debug.Log(hit.transform.gameObject.name);

                    // System's up, but the syndrome's down!
                    if (hit.collider.gameObject == gameObject)
                    {
                        
                        Debug.Log("Y3");

                        OnObjectClicked();
                    }
                }
            }
        }
        
        private void OnObjectClicked()
        {  
            Instantiate(contract, attachPoint.transform.position, attachPoint.transform.rotation, attachPoint.transform);
   
        }
    }
}
