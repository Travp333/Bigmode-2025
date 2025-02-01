using Scripting;
using Scripting.Player;
using UnityEditor;
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
        IsLocked = true;
    }

    public void Remove()
    {
        boxCollider.enabled = false;
        spriteRenderer.enabled = false;
        IsLocked = false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Handles.Label(transform.position + Vector3.up * 1.5f,
            $"isVisible {IsVisible}, isLocked {IsLocked}");
    }
#endif
}
