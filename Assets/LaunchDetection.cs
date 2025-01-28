
using Scripting.Customer;
using Scripting.Player;
using UnityEngine;
using UnityEngine.AI;
public class LaunchDetection : MonoBehaviour
{
    Animator anim;
    RaycastHit hit;
    public bool lerpGate;
    Vector3 lerpTarget;
    [SerializeField]
    LayerMask mask;
    Vector3 hitNormal;
    [SerializeField]
    float splatDistance = .1f;
    [SerializeField]
    float launchSpeed = .1f;
    NavMeshAgent agent;
    CustomerMotor motor;
    GameObject player;
   

    private void Awake() {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        motor = GetComponent<CustomerMotor>();
        player = FindFirstObjectByType<Movement>().gameObject;
    }
    public void DisableAI(){
        agent.enabled = false;
        motor.enabled = false;
    }
    public void EnableAI(){
        agent.enabled = true;
        motor.enabled = true;
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.GetComponent<BatHitboxCollision>() != null){
            //Got Bat Hitbox!
            this.transform.rotation = Quaternion.LookRotation(-player.transform.forward, this.transform.up);
            if(Physics.Raycast(this.transform.position, player.transform.forward, out hit, 999f, mask)){
                anim.Play("AIR");
                lerpTarget = hit.point;
                hitNormal = hit.normal;
                Debug.DrawRay(this.transform.position, lerpTarget - this.transform.position, Color.yellow, 1f);
                Debug.DrawRay(lerpTarget, hitNormal, Color.blue, 1f);
                lerpGate = true;
            }
        }
    }
    void OnCollisionEnter(Collision other) {
        //Debug.Log(other.gameObject);
        if(other.gameObject.GetComponent<LaunchDetection>()!= null){
            if(other.gameObject != this.gameObject && other.gameObject.GetComponent<LaunchDetection>().lerpGate == true && lerpGate == false){
                if(agent.enabled){
                    if(!agent.isStopped){
                        motor.GetHit();
                    }
                }
                Debug.Log("Hit by a flying person!");
                this.transform.rotation = Quaternion.LookRotation(other.transform.position - this.transform.position, this.transform.up);
                PlayRandomDamageAnimation();
                
            }
        }
    }
    private void PlayRandomDamageAnimation(){
        int rand = Random.Range(0,3);
        Debug.Log(rand);
        if(rand == 0){
            //Debug.Log("Playing Damage anim1");
            anim.Play("Take Damage 1");
        }
        else if(rand == 1){
            //Debug.Log("Playing Damage anim2");
            anim.Play("Take Damage 2");
        }
        else if(rand == 2){
            //Debug.Log("Playing Damage anim3");
            anim.Play("Take Damage 3");
        }
        else{
            //Debug.Log("WTF");
        }
    }
    private void Update() {

        if(lerpGate){
            if(Vector3.Distance(this.transform.position, lerpTarget) < splatDistance){
                //Debug.Log("ALREADY AT TARGET, SKIPPING");
                PlayRandomDamageAnimation();
                lerpGate = false;
            }
            else{
                //Debug.Log("Not Yet at target, lerping");
                this.transform.position = Vector3.MoveTowards(this.transform.position, lerpTarget, launchSpeed * Time.deltaTime);
                //this.transform.position = Vector3.Lerp(this.transform.position, lerpTarget, Time.deltaTime);
                if(hitNormal != null){
                    if(Vector3.Distance(this.transform.position, lerpTarget) < splatDistance){
                        this.transform.rotation = Quaternion.LookRotation(hitNormal, this.transform.up);
                        anim.Play("WALLSPLAT");
                        lerpGate = false;
                    }
                }
                else{
                    //Debug.Log("INVALID HIT NORMAL, SKIPPING");
                    PlayRandomDamageAnimation();
                    lerpGate = false;
                }
            }

        }
        else{
            if(agent.velocity.magnitude > 0.01f){
                anim.SetBool("isWalking", true);
            }
            else if(agent.velocity.magnitude < 0.01f){
                anim.SetBool("isWalking", false);
            }
        }

    }

}
