using Scripting.Player;
using UnityEngine;

namespace Scripting.Desk
{
    public class Phone : MonoBehaviour
    {
        [SerializeField]
        int day1PhoneReward = 8333, day2PhoneReward = 11166, day3PhoneReward = 11166, day4PhoneReward = 12014, day5PhoneReward = 14014, day6PhoneReward = 15485, day7PhoneReward = 14300, day8PhoneReward = 14266, day9PhoneReward = 14500, day10PhoneReward = 18200;
        [SerializeField]
        Animator phoneAnim;
        [SerializeField] private float phoneCallLowerBound, phoneCallUpperBound;
        private bool callBlocker;
        private DeskArms _deskArms;
        [SerializeField] private AudioSource myPhoneDialogue;
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
        int value;

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
                    if(myPhoneDialogue.isPlaying){
                        myPhoneDialogue.Stop();
                    }
                    if(!myPhonePutdown.isPlaying){
                        myPhonePutdown.Play();
                    }
                    handAnim.GetComponent<PhoneReferenceHolder>().HangupPhone();
                    player.onPhone = false;
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
                            myPhoneDialogue.Play();
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
                handAnim.GetComponent<PhoneReferenceHolder>().HangupPhone();
                Invoke(nameof(ConversationNOReward), 1f);
            }
        }

        public void ConversationNOReward()
        {
            callBlocker = false;
            _telephoneTimer = 15f;
            player.NotifyNotOnPhone();
            //NO Money!! 
            myPhonePutdown.Play();
            myPhoneDialogue.Stop();
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
            myPhonePutdown.Play();
            myPhoneDialogue.Stop();

            if(GameManager.Singleton.day ==1){
                value = day1PhoneReward + Random.Range(-1000, 1000)* (GameManager.Singleton.IsLoanAgreementRunning ? 2 : 1);
            }
            else if(GameManager.Singleton.day ==2){
                value = day2PhoneReward+ Random.Range(-1000, 1000)* (GameManager.Singleton.IsLoanAgreementRunning ? 2 : 1);
            }
            else if(GameManager.Singleton.day ==3){
                value = day3PhoneReward+ Random.Range(-1000, 1000)* (GameManager.Singleton.IsLoanAgreementRunning ? 2 : 1);
            }
            else if(GameManager.Singleton.day ==4){
                value = day4PhoneReward+ Random.Range(-1000, 1000)* (GameManager.Singleton.IsLoanAgreementRunning ? 2 : 1);
            }
            else if(GameManager.Singleton.day ==5){
                value = day5PhoneReward+ Random.Range(-1000, 1000)* (GameManager.Singleton.IsLoanAgreementRunning ? 2 : 1);
            }
            else if(GameManager.Singleton.day ==6){
                value = day6PhoneReward+ Random.Range(-1000, 1000)* (GameManager.Singleton.IsLoanAgreementRunning ? 2 : 1);
            }
            else if(GameManager.Singleton.day ==7){
                value = day7PhoneReward+ Random.Range(-1000, 1000)* (GameManager.Singleton.IsLoanAgreementRunning ? 2 : 1);
            }
            else if(GameManager.Singleton.day ==8){
                value = day8PhoneReward+ Random.Range(-1000, 1000)* (GameManager.Singleton.IsLoanAgreementRunning ? 2 : 1);
            }
            else if(GameManager.Singleton.day ==9){
                value = day9PhoneReward+ Random.Range(-1000, 1000)* (GameManager.Singleton.IsLoanAgreementRunning ? 2 : 1);
            }
            else if(GameManager.Singleton.day ==10){
                value = day10PhoneReward+ Random.Range(-1000, 1000)* (GameManager.Singleton.IsLoanAgreementRunning ? 2 : 1);
            }
            else {
                value = 0; //error day 11 
            }
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
