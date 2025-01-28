using UnityEngine;

public class BatHitboxController : MonoBehaviour
{
    [SerializeField]
    BoxCollider hitBox;
    public void EnableHitbox(){
        hitBox.enabled = true;
    }
    public void DisableHitbox(){
        hitBox.enabled = false;
    }
}
