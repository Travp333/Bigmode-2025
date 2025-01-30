using Scripting.Customer;
using Scripting.Player;
using UnityEngine;
using UnityEngine.AI;

public class LaunchDetection : MonoBehaviour
{
    [SerializeField] private LayerMask mask;
    [SerializeField] private float splatDistance = .1f;
    [SerializeField] private float launchSpeed = .1f;
    [SerializeField] private GameObject hellPortalPrefab;
    [SerializeField] private SkinnedMeshRenderer myMesh;
    [SerializeField] private GameObject stoneFistPrefab;
    [SerializeField] AudioSource talkAudio;
    [SerializeField] AudioSource hurtAudio;
    [SerializeField] AudioSource screamAudio;

    private Animator _anim;
    private RaycastHit _hit;
    public bool lerpGate;
    private Vector3 _lerpTarget;
    private Vector3 _hitNormal;
    private NavMeshAgent _agent;
    private CustomerMotor _motor;
    private GameObject _player;

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
        _agent.enabled = true;
        _motor.enabled = true;
    }
    public void GetPowerFisted()
    {
        this.transform.rotation = Quaternion.LookRotation(-_player.transform.forward, this.transform.up);
        var portal = Instantiate(stoneFistPrefab, this.transform.position, Quaternion.identity);
        portal.GetComponent<PortalManager>().mesh.GetComponent<SkinnedMeshRenderer>().sharedMesh = myMesh.sharedMesh;
        portal.GetComponent<PortalManager>().mesh.GetComponent<SkinnedMeshRenderer>().sharedMaterials = myMesh.sharedMaterials;
        portal.transform.localScale = this.transform.localScale;
        Destroy(this.gameObject);
    }
    public void GetHellGrabbed()
    {
        this.transform.rotation = Quaternion.LookRotation(-_player.transform.forward, this.transform.up);
        var portal = Instantiate(hellPortalPrefab, this.transform.position, Quaternion.identity);
        portal.GetComponent<PortalManager>().mesh.GetComponent<SkinnedMeshRenderer>().sharedMesh = myMesh.sharedMesh;
        portal.GetComponent<PortalManager>().mesh.GetComponent<SkinnedMeshRenderer>().sharedMaterials = myMesh.sharedMaterials;
        portal.transform.localScale = this.transform.localScale;
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<BatHitboxCollision>() != null)
        {
            //Got Bat Hitbox!
            this.transform.rotation = Quaternion.LookRotation(-_player.transform.forward, this.transform.up);
            if (Physics.Raycast(this.transform.position, _player.transform.forward, out _hit, 999f, mask))
            {
                StartScreamAudio();
                other.gameObject.GetComponent<BatHitboxCollision>().cont.DisableHitbox();
                _anim.Play("AIR");
                _lerpTarget = _hit.point;
                _hitNormal = _hit.normal;
                Debug.DrawRay(this.transform.position, _lerpTarget - this.transform.position, Color.yellow, 1f);
                Debug.DrawRay(_lerpTarget, _hitNormal, Color.blue, 1f);
                lerpGate = true;
            }
        }

        if (other.gameObject.GetComponent<Movement>() != null)
        {
            //Got Player Hitbox!
            if (other.gameObject.GetComponent<Movement>().rageMode == true)
            {
                this.transform.rotation = Quaternion.LookRotation(-_player.transform.forward, this.transform.up);
                if (Physics.Raycast(this.transform.position, _player.transform.forward, out _hit, 999f, mask))
                {
                    StartScreamAudio();
                    _anim.Play("AIR");
                    _lerpTarget = _hit.point;
                    _hitNormal = _hit.normal;
                    Debug.DrawRay(this.transform.position, _lerpTarget - this.transform.position, Color.yellow, 1f);
                    Debug.DrawRay(_lerpTarget, _hitNormal, Color.blue, 1f);
                    lerpGate = true;
                }
            }
            else
            {
                //TESTING FIST DOCUMENT
                //this.transform.rotation = Quaternion.LookRotation(-_player.transform.forward, this.transform.up);
                //var portal = Instantiate(stoneFistPrefab, this.transform.position, Quaternion.identity);
                // portal.GetComponent<PortalManager>().mesh.GetComponent<SkinnedMeshRenderer>().sharedMesh =
                //     myMesh.sharedMesh;
                // portal.GetComponent<PortalManager>().mesh.GetComponent<SkinnedMeshRenderer>().sharedMaterials =
                //     myMesh.sharedMaterials;
                // portal.transform.localScale = this.transform.localScale;
                //  Destroy(this.gameObject);
            }
        }
        else if (other.gameObject.GetComponent<HuellHitboxCollision>() != null)
        {
            //Got Huell Hitbox!
            var huellCollision = other.gameObject.GetComponent<HuellHitboxCollision>().rootHuell;
            this.transform.rotation = Quaternion.LookRotation(huellCollision.transform.forward, this.transform.up);
            if (Physics.Raycast(this.transform.position, huellCollision.transform.forward, out _hit, 999f, mask))
            {
                StartScreamAudio();
                huellCollision.GetComponent<HuellController>().HideHitbox();
                _anim.Play("AIR");
                _lerpTarget = _hit.point;
                _hitNormal = _hit.normal;
                //Debug.DrawRay(this.transform.position, lerpTarget - this.transform.position, Color.yellow, 1f);
                //Debug.DrawRay(lerpTarget, hitNormal, Color.blue, 1f);
                lerpGate = true;
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        //Debug.Log(other.gameObject);
        if (other.gameObject.GetComponent<LaunchDetection>() != null)
        {
            if (other.gameObject != this.gameObject &&
                other.gameObject.GetComponent<LaunchDetection>().lerpGate == true && lerpGate == false)
            {
                if (_agent.enabled)
                {
                    if (!_agent.isStopped)
                    {
                        _motor.GetHit();
                    }
                }

                //Debug.Log("Hit by a flying person!");
                this.transform.rotation = Quaternion.LookRotation(other.transform.position - this.transform.position,
                    this.transform.up);
                PlayRandomDamageAnimation();
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
        if (lerpGate)
        {
            if (Vector3.Distance(this.transform.position, _lerpTarget) < splatDistance)
            {
                //Debug.Log("ALREADY AT TARGET, SKIPPING");
                PlayRandomDamageAnimation();
                lerpGate = false;
            }
            else
            {
                //Debug.Log("Not Yet at target, lerping", this.gameObject);
                this.transform.position =
                    Vector3.MoveTowards(this.transform.position, _lerpTarget, launchSpeed * Time.deltaTime);
                //this.transform.position = Vector3.Lerp(this.transform.position, lerpTarget, Time.deltaTime);
                if (_hitNormal != null)
                {
                    if (Vector3.Distance(this.transform.position, _lerpTarget) < splatDistance)
                    {
                        this.transform.rotation = Quaternion.LookRotation(_hitNormal, this.transform.up);
                        _anim.Play("WALLSPLAT");
                        lerpGate = false;
                    }
                }
                else
                {
                    //Debug.Log("INVALID HIT NORMAL, SKIPPING");
                    PlayRandomDamageAnimation();
                    lerpGate = false;
                }
            }
        }
        else
        {
            if (screamAudio.isPlaying)
            {
                screamAudio.Stop();
            }
            if (_anim.GetCurrentAnimatorStateInfo(0).IsName("AIR"))
            {
                PlayRandomDamageAnimation();
            }

            if (_agent.velocity.magnitude > 0.01f)
            {
                _anim.SetBool("isWalking", true);
            }
            else if (_agent.velocity.magnitude < 0.01f)
            {
                _anim.SetBool("isWalking", false);
            }
        }

        StartTalkAudio();
    }

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
        //}
    }
}
