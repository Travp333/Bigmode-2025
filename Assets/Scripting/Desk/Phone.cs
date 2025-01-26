using Scripting.Player;
using UnityEngine;

namespace Scripting.Desk
{
    public class Phone : MonoBehaviour
    {
        [SerializeField] private Movement player;

        // Update is called once per frame
        private float _telephoneTimer = 15.0f;

        void Update()
        {
            _telephoneTimer -= Time.deltaTime;

            if (_telephoneTimer <= 0)
            {
                _telephoneTimer = 15.0f;
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
                        }
                    }
                }
            }
        }

        private bool _isRinging;

        private void Ring()
        {
            _isRinging = true;
            player.NotifyPhoneRinging();
        }

        private void StopRinging()
        {
            _isRinging = false;
            player.NotifyPhoneStopped();
        }

        private void OnGUI()
        {
            if (_isRinging)
                GUI.Label(new Rect(5, Screen.height / 2f, 200, 50), "RING RING RING RING RING - BANANAPHONE");
        }
    }
}
