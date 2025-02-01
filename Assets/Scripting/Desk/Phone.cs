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
        [SerializeField] private AudioSource myPhoneRinger;
        [SerializeField] private AudioSource myPhonePickup;
        [SerializeField] private AudioSource myPhonePutdown;

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
                    myPhonePutdown.Play();
                    handAnim.Play("Dropping Phone");
                    Invoke(nameof(ConversationReward), 1f);
                    handAnim.GetComponent<PhoneReferenceHolder>().HangupPhone();
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
                            myPhonePickup.Play();
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
                myPhonePutdown.Play();
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
       
            var value = Random.Range(7500, 10000) * (GameManager.Singleton.IsLoanAgreementRunning ? 2 : 1);

            //GameManager.Singleton.upgrades.money += value;
            GameManager.Singleton.ChangeMoneyAmount(value);
        }

        private void Ring()
        {
            _isRinging = true;
            myPhoneRinger.Play();
            player.NotifyPhoneRinging();
            phoneAnim.Play("Phone Ringing");
        }

        private void StopRinging()
        {
            _isRinging = false;
            myPhoneRinger.Stop();
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
