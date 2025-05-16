using UnityEngine;
// using UnityEngine.UI; // No longer needed for Image
using TMPro;

namespace Spaces.Core.Runtime
{
    public class InterestPointDetails : MonoBehaviour
    {
        [Header("Content")]
        public string titleText = "Title";
        [TextArea(3, 10)]
        public string descriptionText = "Description goes here.";
        public Sprite footerSprite;

        [Header("Panel Layout (World Units)")]
        public float panelWidth = 3.0f;
        [Range(0.05f, 0.5f)] public float titleHeight = 0.2f;
        [Range(0.05f, 0.5f)] public float footerHeight = 0.2f;
        [Range(0.01f, 0.2f)] public float padding = 0.12f;
        public Color backgroundColor = new Color(0,0,0,0.7f);
        public Color textColor = Color.white;
        [Header("Background Style")]
        public Sprite backgroundSprite; // PNG with rounded corners and alpha

        [Header("Pop Animation")]
        public float popDuration = 0.18f;
        public AnimationCurve popCurve = AnimationCurve.EaseInOut(0,0,1,1);
    }
} 