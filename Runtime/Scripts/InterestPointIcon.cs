using UnityEngine;

public class InterestPointIcon : MonoBehaviour
{
    [Header("Pop Animation")]
    public float popDuration = 0.18f;
    public AnimationCurve popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Appearance")]
    [Range(0f, 1f)]
    public float transparency = 0.8f;

    private Vector3 originalScale = Vector3.one;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        StopAllCoroutines();
        StartCoroutine(PopIn());

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = transparency;
            spriteRenderer.color = color;
        }
    }

    System.Collections.IEnumerator PopIn()
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / popDuration;
            float s = popCurve.Evaluate(Mathf.Clamp01(t));
            transform.localScale = originalScale * s;
            yield return null;
        }
        transform.localScale = originalScale;
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.LookAt(
                transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up
            );
        }
    }
} 