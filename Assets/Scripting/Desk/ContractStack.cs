using Scripting.Player;
using UnityEngine;

namespace Scripting.Desk
{
    public class ContractStack : MonoBehaviour
    {
        [SerializeField] private GameObject contract;
        [SerializeField] private GameObject attachPoint;
        [SerializeField] private Movement player;
        [SerializeField] private LayerMask layerMask;

        [SerializeField]
        private GameObject deskHands;

        [SerializeField]
        private Animator leftHandAnim;

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

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && player.CanAct())
            {
                if (player.HasContract) return;

                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        leftHandAnim.Play("Grabbing Paper");
                        deskHands.GetComponent<DeskArms>().BlockLeftHand();
                        Invoke(nameof(GrabPaper), .26f);
                    }
                }
            }
        }

        private void GrabPaper()
        {
            var obj = Instantiate(contract, attachPoint.transform).GetComponentInChildren<Contract>();
            obj.SetActive(true);

            player.SetActiveContract(obj);
        }
    }
}
