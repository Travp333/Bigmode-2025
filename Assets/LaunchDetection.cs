
using Scripting.Player;
using UnityEngine;

public class LaunchDetection : MonoBehaviour
{
    Animator anim;
    Vector3 distToPlayer;
    RaycastHit hit;
    bool lerpGate;
    Vector3 lerpTarget;
    [SerializeField]
    LayerMask mask;
    private void Awake() {
        anim = GetComponent<Animator>();
    }
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.GetComponent<Movement>() != null){
            //Got player!
            distToPlayer = this.transform.position - other.transform.position;
            Debug.DrawRay(this.transform.position, distToPlayer, Color.green, 2f);
            anim.Play("AIR");
            this.transform.rotation = Quaternion.LookRotation(-other.gameObject.transform.forward, this.transform.up);
            if(Physics.Raycast(this.transform.position, other.gameObject.transform.forward, out hit, 999f, mask)){
                lerpTarget = hit.point;
                Debug.DrawRay(this.transform.position, Vector3.Normalize(this.transform.position - lerpTarget), Color.yellow, 2f);
                lerpGate = true;
            }
        }
    }
    private void Update() {
        if(lerpGate){
            this.transform.position = Vector3.Lerp(this.transform.position, lerpTarget, Time.deltaTime);
        }
    }
}
