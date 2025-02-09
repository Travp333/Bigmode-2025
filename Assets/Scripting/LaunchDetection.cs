using Scripting.Customer;
using Scripting.Player;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Scripting
{
    public class LaunchDetection : MonoBehaviour
    {
        [SerializeField] private LayerMask mask;
        [SerializeField] private float splatDistance = .1f;
        [SerializeField] private float launchSpeed = .1f;
        [SerializeField] private GameObject hellPortalPrefab;
        [SerializeField] private SkinnedMeshRenderer myMesh;
        [SerializeField] private GameObject stoneFistPrefab;
        [SerializeField] private AudioSource talkAudio;
        [SerializeField] private AudioSource hurtAudio;
        [SerializeField] private AudioSource screamAudio;
        [SerializeField] private AudioSource flyAudio;

        private Animator _anim;
        private RaycastHit _hit;
        [FormerlySerializedAs("lerpGate")] public bool isFlying;
        private Vector3 _lerpTarget;
        private Vector3 _hitNormal;
        private NavMeshAgent _agent;
        private CustomerMotor _motor;
        private GameObject _player;
        private bool _scared;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _motor = GetComponent<CustomerMotor>();
            _player = FindFirstObjectByType<Movement>().gameObject;
        }

        public void DisableAI()
        {
            _agent.enabled = false;
            _motor.enabled = false;
        }

        public void EnableAI()
        {
            if (_scared && !isFlying)
            {
                _motor.RunOut();
            }

            _agent.enabled = true;
            _motor.enabled = true;
        }

        public void GetPowerFisted()
        {
            transform.rotation = Quaternion.LookRotation(-GetCameraForward(), transform.up);
            var portal = Instantiate(stoneFistPrefab, transform.position, Quaternion.identity);
            portal.GetComponent<PortalManager>().mesh.GetComponent<SkinnedMeshRenderer>().sharedMesh = myMesh.sharedMesh;
            portal.GetComponent<PortalManager>().mesh.GetComponent<SkinnedMeshRenderer>().sharedMaterials =
                myMesh.sharedMaterials;
            portal.transform.localScale = transform.localScale;
            Destroy(gameObject);
        }
    
        private Vector3 GetCameraForward()
        {
            var cam = _player.GetComponent<Movement>().Cam;
            var forward = cam.transform.forward;
            forward.y = 0;
            return forward.normalized;
        }
    
        public void GetHellGrabbed()
        {
            transform.rotation = Quaternion.LookRotation(-GetCameraForward(), transform.up);
            var portal = Instantiate(hellPortalPrefab, transform.position, Quaternion.identity);
            portal.GetComponent<PortalManager>().mesh.GetComponent<SkinnedMeshRenderer>().sharedMesh = myMesh.sharedMesh;
            portal.GetComponent<PortalManager>().mesh.GetComponent<SkinnedMeshRenderer>().sharedMaterials =
                myMesh.sharedMaterials;
            portal.transform.localScale = transform.localScale;
            Destroy(gameObject);
        }

        void LaunchNPC(Collider other)
        {
            if (_motor.IsMotherfucker)
            {
                _motor.InterruptSpraying();
            }

            if (_motor.IsThief)
            {
                _motor.InterruptStealing();
            }

            _motor._runOut = false;
            _motor._sneakOut = false;
            _motor.IsHit();
            StartScreamAudio();
            _anim.Play("AIR");
            _lerpTarget = _hit.point;
            _hitNormal = _hit.normal;
            //Debug.DrawRay(this.transform.position, _lerpTarget - this.transform.position, Color.yellow, 1f);
            //Debug.DrawRay(_lerpTarget, _hitNormal, Color.blue, 1f);
            isFlying = true;
            _scared = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<BatHitboxCollision>() != null)
            {
                //Got Bat Hitbox!
                transform.rotation = Quaternion.LookRotation(-GetCameraForward(), transform.up);
                if (Physics.Raycast(transform.position, GetCameraForward(), out _hit, 999f, mask))
                {
                    other.gameObject.GetComponent<BatHitboxCollision>().cont.DisableHitbox();
                    LaunchNPC(other);
                }
            }

            if (other.gameObject.GetComponent<Movement>() != null)
            {
                //Got Player Hitbox!
                if (other.gameObject.GetComponent<Movement>().rageMode == true)
                {
                    transform.rotation = Quaternion.LookRotation(-GetCameraForward(), transform.up);
                    if (Physics.Raycast(transform.position, GetCameraForward(), out _hit, 999f, mask))
                    {
                        LaunchNPC(other);
                    }
                }
            }
            else if (other.gameObject.GetComponent<HuellHitboxCollision>() != null)
            {
                //Got Huell Hitbox!
                var huellCollision = other.gameObject.GetComponent<HuellHitboxCollision>().rootHuell;
                transform.rotation = Quaternion.LookRotation(huellCollision.transform.forward, transform.up);
                if (Physics.Raycast(transform.position, huellCollision.transform.forward, out _hit, 999f, mask))
                {
                    LaunchNPC(other);
                }
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            //Debug.Log(other.gameObject);
            if (other.gameObject.GetComponent<LaunchDetection>() != null)
            {
                if (other.gameObject != gameObject &&
                    other.gameObject.GetComponent<LaunchDetection>().isFlying == true && isFlying == false)
                {
                    // if (_agent.enabled)
                    // {
                    //     if (!_agent.isStopped)
                    //     {
                    _motor.GetHit();
                    //     }
                    // }
                    
                    //Debug.Log("Hit by a flying person!");
                    transform.rotation = Quaternion.LookRotation(other.transform.position - transform.position,
                        transform.up);
                    PlayRandomDamageAnimation();
                    if (_motor._runOut)
                    {
                        Debug.Log("HIT BY FLYING PERSON WHILE RUNNING AWAY");
                        _agent.isStopped = false;
                        EnableAI();
                        isFlying = false;
                        
                        _motor.RunOut();
                    }
                }
            }
        }

        private void PlayRandomDamageAnimation()
        {
            StartHurtAudio();

            int rand = Random.Range(0, 3);
            //Debug.Log(rand);
            if (rand == 0)
            {
                //Debug.Log("Playing Damage anim1");
                _anim.Play("Take Damage 1");
            }
            else if (rand == 1)
            {
                //Debug.Log("Playing Damage anim2");
                _anim.Play("Take Damage 2");
            }
            else if (rand == 2)
            {
                //Debug.Log("Playing Damage anim3");
                _anim.Play("Take Damage 3");
            }
            else
            {
                //Debug.Log("WTF");
            }
        }

        private void Update()
        {
            if (isFlying)
            {
                if (Vector3.Distance(transform.position, _lerpTarget) < splatDistance)
                {
                    //Debug.Log("ALREADY AT TARGET, SKIPPING");
                    PlayRandomDamageAnimation();
                    isFlying = false;
                }
                else
                {
                    //Debug.Log("Not Yet at target, lerping", this.gameObject);
                    transform.position =
                        Vector3.MoveTowards(transform.position, _lerpTarget, launchSpeed * Time.deltaTime);
                    //this.transform.position = Vector3.Lerp(this.transform.position, lerpTarget, Time.deltaTime);
                    if (_hitNormal != null)
                    {
                        if (Vector3.Distance(transform.position, _lerpTarget) < splatDistance)
                        {
                            transform.rotation = Quaternion.LookRotation(_hitNormal, transform.up);
                            _anim.Play("WALLSPLAT");
                            isFlying = false;
                        }
                    }
                    else
                    {
                        //Debug.Log("INVALID HIT NORMAL, SKIPPING");
                        PlayRandomDamageAnimation();
                        isFlying = false;
                    }
                }
            }
            else
            {
                if (screamAudio.isPlaying)
                {
                    screamAudio.Stop();
                }

                if (flyAudio.isPlaying)
                {
                    flyAudio.Stop();
                }

                if (_anim.GetCurrentAnimatorStateInfo(0).IsName("AIR"))
                {
                    PlayRandomDamageAnimation();
                }

                if (_agent.velocity.magnitude > 0.01f)
                {
                    if (!_anim.GetBool("isWalking"))
                    {
                        _anim.SetBool("isWalking", true);
                    }
                }
                else if (_agent.velocity.magnitude < 0.01f)
                {
                    if (_anim.GetBool("isWalking"))
                    {
                        _anim.SetBool("isWalking", false);
                    }
                }
            }

            _talkCooldown -= Time.deltaTime;
            if (_talkCooldown <= 0)
            {
                _talkCooldown = Random.Range(3.5f, 7f);
                StartTalkAudio();
            }
        }

        private float _talkCooldown;

        private void StartHurtAudio()
        {
            //if (!hurtAudio.isPlaying)
            //{
            talkAudio.Stop();
            hurtAudio.Play();
            //}
        }

        private void StartTalkAudio()
        {
            if (!hurtAudio.isPlaying && !talkAudio.isPlaying && !screamAudio.isPlaying)
            {
                talkAudio.Play();
            }
        }

        public bool IsWalking()
        {
            return _anim.GetBool("isWalking");
        }

        private void StartScreamAudio()
        {
            //if (!screamAudio.isPlaying)
            //{
            talkAudio.Stop();
            screamAudio.Play();
            flyAudio.Play();
            //}
        }
    }
}
