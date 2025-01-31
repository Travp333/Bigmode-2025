using Scripting;
using Scripting.Player;
using UnityEngine;
using Random = UnityEngine.Random;

public class VandalismSpot : MonoBehaviour
{
    [SerializeField] private GameObject aiSpot;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private BoxCollider boxCollider;

    [SerializeField] private Movement player;

    public bool IsVisible => spriteRenderer.enabled;
    public bool IsLocked { get; set; }

    void Awake()
    {
        boxCollider.enabled = false;
        spriteRenderer.enabled = false;
    }

    public void Spray()
    {
        boxCollider.enabled = true;
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
        GameManager.Singleton.RemoveVandalismSpot(aiSpot);
        IsLocked = true;
    }

    public void Remove()
    {
        boxCollider.enabled = false;
        spriteRenderer.enabled = false;
        GameManager.Singleton.RegisterVandalismSpot(aiSpot);
        IsLocked = false;
    }
}
