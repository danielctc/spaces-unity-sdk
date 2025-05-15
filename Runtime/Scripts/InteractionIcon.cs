using UnityEngine;
using System.Collections;

public class InteractionIcon : MonoBehaviour
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
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
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

    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        spriteRenderer.sprite = sprite;
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = transparency;
            spriteRenderer.color = color;
        }
    }
} 