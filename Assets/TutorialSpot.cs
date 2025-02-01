using TMPro;
using UnityEngine;

public class TutorialSpot : MonoBehaviour
{
    [SerializeField] private int order;
    [SerializeField] private string text;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite sprite;
    [SerializeField] private TextMeshPro textMesh;
    [SerializeField] private bool rotateUpDown;
    [SerializeField] private bool rotateLeftRight;

    private bool _isActive;
    private bool _isVisible;

    public bool IsActive => _isActive;
    public bool IsVisible => _isVisible;
    public int Order => order;

    private void Awake()
    {
        _isActive = true;
        spriteRenderer.sprite = sprite;
        textMesh.text = text;

        name += $" ID: {order}";
    }

    private void Start()
    {
        TutorialManager.Singleton.Register(this);
    }

    public void OnValidate()
    {
        if (!spriteRenderer)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!textMesh)
            textMesh = GetComponentInChildren<TextMeshPro>();
    }

    public void Show()
    {
        _isActive = true;
    }

    public void Hide()
    {
        _isActive = false;
    }

    public void Next()
    {
        _isActive = false;

        TutorialManager.Singleton.Next();
    }

    public void Update()
    {
        if (!_isActive && spriteRenderer.enabled)
        {
            spriteRenderer.enabled = false;
        }

        if (_isActive && !spriteRenderer.enabled)
        {
            spriteRenderer.enabled = true;
        }

        if (_isActive && (rotateLeftRight || rotateUpDown))
        {
            var originalRotation = transform.localRotation.eulerAngles;
            transform.LookAt(Camera.main.transform.position);

            var currentLocalRotation = transform.localRotation.eulerAngles;

            if (!rotateLeftRight)
                currentLocalRotation.y = originalRotation.y;

            if (!rotateUpDown)
                currentLocalRotation.x = originalRotation.x;

            transform.localRotation = Quaternion.Euler(currentLocalRotation);
        }
    }
}
