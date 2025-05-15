using UnityEngine;

public class InteractionIconDetails : MonoBehaviour
{
    public string prettyName => "Interaction";
    public string tooltip => "Displays a visual icon indicating how users can interact with this object";

    public enum IconType
    {
        None,
        Click,
        Link,
        Door,
        Expand,
        Grab,
        PDF,
        Talk,
        Custom
    }

    [Header("Icon Configuration")]
    public IconType currentIconType = IconType.None;
    public Sprite[] iconSprites;
    public Sprite customIcon;

    [Header("References")]
    [SerializeField] private SpriteRenderer iconRenderer;

    void Awake()
    {
        if (iconRenderer == null)
        {
            iconRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void SetIconType(IconType type)
    {
        currentIconType = type;
        UpdateIcon();
    }

    private void UpdateIcon()
    {
        if (iconRenderer == null) return;

        switch (currentIconType)
        {
            case IconType.None:
                iconRenderer.sprite = null;
                break;
            case IconType.Custom:
                iconRenderer.sprite = customIcon;
                break;
            default:
                int index = (int)currentIconType - 1; // Subtract 1 because None is 0
                if (iconSprites != null && index >= 0 && index < iconSprites.Length)
                {
                    iconRenderer.sprite = iconSprites[index];
                }
                break;
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        UpdateIcon();
    }
} 