using UnityEngine;
using TMPro;
using Spaces.Core.Runtime;

public class InterestPointDetailsData {
    public string titleText;
    public string descriptionText;
    public Sprite footerSprite;
    public float panelWidth;
    public float titleHeight;
    public float footerHeight;
    public float padding;
    public Color backgroundColor;
    public Color textColor;
    public Sprite backgroundSprite;
    public float popDuration;
    public AnimationCurve popCurve;
}

public class ProceduralInfoPanel : MonoBehaviour
{
    private GameObject backgroundQuad;
    private TextMeshPro textMesh;
    private GameObject footerGO;
    private SpriteRenderer footerRenderer;

    // Store current config for re-layout if needed
    private InterestPointDetails currentDetails;
    private InterestPointDetailsData currentData;

    public void SetInfo(InterestPointDetails details)
    {
        currentDetails = details;
        currentData = null;
        GeneratePanelFromDetails(details);
    }
    public void SetInfo(InterestPointDetailsData data)
    {
        currentDetails = null;
        currentData = data;
        GeneratePanelFromDetails(data);
    }

    void Start()
    {
        if (currentDetails != null)
        {
            GeneratePanelFromDetails(currentDetails);
        }
        else if (currentData != null)
        {
            GeneratePanelFromDetails(currentData);
        }
        else
        {
            // Try to find a details object in the parent or scene
            var found = GetComponentInParent<InterestPointDetails>() ?? FindObjectOfType<InterestPointDetails>();
            if (found != null)
            {
                SetInfo(found);
            }
            else
            {
                // Show a default panel if nothing is found
                var dummy = new InterestPointDetailsData {
                    titleText = "Info Panel",
                    descriptionText = "No InterestPointDetails assigned.",
                    panelWidth = 2.5f,
                    titleHeight = 0.2f,
                    footerHeight = 0.2f,
                    padding = 0.12f,
                    backgroundColor = new Color(0,0,0,0.7f),
                    textColor = Color.white,
                    popDuration = 0.18f,
                    popCurve = AnimationCurve.EaseInOut(0,0,1,1)
                };
                SetInfo(dummy);
            }
        }
    }

    void OnEnable()
    {
        float popDuration = 0.18f;
        AnimationCurve popCurve = AnimationCurve.EaseInOut(0,0,1,1);
        if (currentDetails != null)
        {
            popDuration = currentDetails.popDuration;
            popCurve = currentDetails.popCurve;
        }
        else if (currentData != null)
        {
            popDuration = currentData.popDuration;
            popCurve = currentData.popCurve;
        }
        if (popDuration > 0.01f)
        {
            transform.localScale = Vector3.zero;
            StopAllCoroutines();
            StartCoroutine(PopIn(popDuration, popCurve));
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    System.Collections.IEnumerator PopIn(float popDuration, AnimationCurve popCurve)
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / popDuration;
            float s = popCurve.Evaluate(Mathf.Clamp01(t));
            transform.localScale = Vector3.one * s;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    void GeneratePanelFromDetails(InterestPointDetails details)
    {
        GeneratePanelFromDetails(new InterestPointDetailsData {
            titleText = details.titleText,
            descriptionText = details.descriptionText,
            footerSprite = details.footerSprite,
            panelWidth = details.panelWidth,
            titleHeight = details.titleHeight,
            footerHeight = details.footerHeight,
            padding = details.padding,
            backgroundColor = details.backgroundColor,
            textColor = details.textColor,
            backgroundSprite = details.backgroundSprite,
            popDuration = details.popDuration,
            popCurve = details.popCurve
        });
    }

    void GeneratePanelFromDetails(InterestPointDetailsData details)
    {
        // Destroy old children if reusing
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        // 1. Background Quad
        backgroundQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backgroundQuad.name = "PanelBackground";
        backgroundQuad.transform.SetParent(transform, false);
        backgroundQuad.transform.localPosition = Vector3.zero;
        backgroundQuad.transform.localRotation = Quaternion.identity;

        // Remove collider
        var collider = backgroundQuad.GetComponent<Collider>();
        if (collider) Destroy(collider);

        // Use a transparent shader for background
        float textAreaWidth = details.panelWidth - 2 * details.padding;
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = details.backgroundColor;
        if (details.backgroundSprite != null)
            mat.mainTexture = details.backgroundSprite.texture;
        backgroundQuad.GetComponent<MeshRenderer>().material = mat;

        // 2. Text (Title + Description)
        var textGO = new GameObject("PanelText");
        textGO.transform.SetParent(transform, false);
        textMesh = textGO.AddComponent<TextMeshPro>();
        textMesh.fontSize = 1.5f;
        textMesh.color = details.textColor;
        textMesh.alignment = TextAlignmentOptions.Top;
        textMesh.enableWordWrapping = true;
        textMesh.text = $"<b><size=110%>{details.titleText}</size></b>\n<size=85%>{details.descriptionText}</size>";
        textMesh.rectTransform.sizeDelta = new Vector2(textAreaWidth, 10f); // temp height

        // Calculate required text height
        textMesh.ForceMeshUpdate();
        float preferredTextHeight = textMesh.GetPreferredValues(textAreaWidth, Mathf.Infinity).y;

        // 3. Calculate panel height
        float totalHeight = details.padding + details.titleHeight + preferredTextHeight + details.footerHeight + details.padding;
        backgroundQuad.transform.localScale = new Vector3(details.panelWidth, totalHeight, 1f);

        // 4. Set text position and size
        textMesh.rectTransform.sizeDelta = new Vector2(textAreaWidth, preferredTextHeight + details.titleHeight);
        textGO.transform.localPosition = new Vector3(0, (totalHeight/2f) - details.padding - (details.titleHeight/2f) - (preferredTextHeight/2f), -0.01f);

        // 5. Footer Sprite
        footerGO = new GameObject("PanelFooterSprite");
        footerGO.transform.SetParent(transform, false);
        footerRenderer = footerGO.AddComponent<SpriteRenderer>();
        footerRenderer.sprite = details.footerSprite;
        footerRenderer.color = Color.white;
        if (details.footerSprite != null)
        {
            float spriteAspect = details.footerSprite.rect.width / details.footerSprite.rect.height;
            float spriteW = details.panelWidth - 2 * details.padding;
            float spriteH = details.footerHeight;
            if (spriteW / spriteH > spriteAspect)
                spriteW = spriteH * spriteAspect;
            else
                spriteH = spriteW / spriteAspect;
            footerGO.transform.localScale = new Vector3(spriteW / details.footerSprite.bounds.size.x, spriteH / details.footerSprite.bounds.size.y, 1f);
        }
        footerGO.transform.localPosition = new Vector3(0, (-totalHeight/2f) + details.padding + (details.footerHeight/2f), -0.01f);
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }
    }
} 