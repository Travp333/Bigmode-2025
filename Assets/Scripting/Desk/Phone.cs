using Scripting.Player;
using UnityEngine;

namespace Scripting.Desk
{
    public class Phone : MonoBehaviour
    {
        [SerializeField]
        Animator phoneAnim;
        [SerializeField] private float phoneCallLowerBound, phoneCallUpperBound;
        private bool callBlocker;
        private DeskArms _deskArms;

        [SerializeField]
        private GameObject arms;

        [SerializeField] private float telephoneCooldown = 60f;

        [SerializeField] private Animator handAnim;
        [SerializeField] private Movement player;

        // Update is called once per frame
        private float _telephoneTimer;

        private void Awake()
        {
            _deskArms = arms.GetComponent<DeskArms>();

            _telephoneTimer = telephoneCooldown;
        }

        private void Update()
        {
            if (GameManager.Singleton.IsNightTime)
            {
                if (_isRinging)
                    StopRinging();

                if (player.onPhone)
                {
                    handAnim.Play("Dropping Phone");
                    Invoke(nameof(ConversationReward), 1f);
                }

                _telephoneTimer = telephoneCooldown;

                return;
            }

            _telephoneTimer -= Time.deltaTime;

            if (_telephoneTimer <= 0 && !callBlocker)
            {
                _telephoneTimer = telephoneCooldown;
                Ring();
            }

            if (_isRinging && player.CanAct())
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out var hit))
                    {
                        if (hit.collider.gameObject == gameObject)
                        {
                            StopRinging();
                            _deskArms.BlockLeftHand();
                            handAnim.Play("Grabbing Phone");
                            callBlocker = true;
                            Invoke(nameof(ConversationEnd), Random.Range(phoneCallLowerBound, phoneCallUpperBound));
                            player.NotifyOnPhone();
                        }
                    }
                }
            }

            if (player.onPhone && Input.GetMouseButtonDown(1))
            {
                _deskArms.UnblockLeftHand();
                ConversationEndEarly();
                //CancelInvoke();
            }
        }

        public void ConversationEndEarly()
        {
            if (player.onPhone)
            {
                handAnim.Play("Dropping Phone");
                Invoke(nameof(ConversationNOReward), 1f);
            }
        }

        public void ConversationNOReward()
        {
            callBlocker = false;
            _telephoneTimer = 15f;
            player.NotifyNotOnPhone();
            //NO Money!! 
        }

        private bool _isRinging;

        private void ConversationEnd()
        {
            if (player.onPhone)
            {
                handAnim.Play("Dropping Phone");
                Invoke(nameof(ConversationReward), 1f);
            }
        }

        private void ConversationReward()
        {
            callBlocker = false;
            _telephoneTimer = 15f;
            player.NotifyNotOnPhone();
            //Money!! 
        }

        private void Ring()
        {
            _isRinging = true;
            player.NotifyPhoneRinging();
            phoneAnim.Play("Phone Ringing");
        }

        private void StopRinging()
        {
            _isRinging = false;
            player.NotifyPhoneStopped();
            phoneAnim.Play("Phone Not Ringing");
        }

        private void OnGUI()
        {
            if (_isRinging)
                GUI.Label(new Rect(5, Screen.height / 2f, 200, 50), "RING RING RING RING RING - BANANAPHONE");
        }
    }
}
