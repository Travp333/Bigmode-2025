using UnityEngine;

public class BatHitboxController : MonoBehaviour
{
    [SerializeField] private BoxCollider hitBox;
    public void EnableHitbox(){
        hitBox.enabled = true;
    }
    public void DisableHitbox(){
        hitBox.enabled = false;
    }
}
