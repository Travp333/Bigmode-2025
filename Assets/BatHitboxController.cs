using UnityEngine;

public class BatHitboxController : MonoBehaviour
{
    [SerializeField]
    BoxCollider hitBox;
    void EnableHitbox(){
        hitBox.enabled = true;
    }
    void DisableHitbox(){
        hitBox.enabled = false;
    }
}
