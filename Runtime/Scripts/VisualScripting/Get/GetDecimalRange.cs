using System;
using System.Globalization;
using UnityEngine;
using GameCreator.Runtime.VisualScripting;

namespace GameCreator.Runtime.Common
{
    [Title("Hotspot Range")]
    [Category("Spaces/Hotspot Decimal Range")]
    
    [Image(typeof(IconNumber), ColorTheme.Type.TextNormal)]
    [Description("A decimal range where the hotspot is only active between minimum and maximum values")]

    [Keywords("Float", "Decimal", "Double", "Range", "Min", "Max")]
    
    [Serializable]
    public class GetDecimalRange : PropertyTypeGetDecimal
    {
        [SerializeField] protected double m_MinValue;
        [SerializeField] protected double m_MaxValue;

        public override double Get(Args args)
        {
            if (args?.Self == null) return 0;
            
            Hotspot hotspot = args.Self.GetComponent<Hotspot>();
            if (hotspot == null) return 0;
            
            float distance = hotspot.Distance;
            if (distance < m_MinValue || distance > m_MaxValue)
            {
                return 0;
            }
            
            return m_MaxValue;
        }

        public override double Get(GameObject gameObject)
        {
            if (gameObject == null) return 0;
            
            Hotspot hotspot = gameObject.GetComponent<Hotspot>();
            if (hotspot == null) return 0;
            
            float distance = hotspot.Distance;
            if (distance < m_MinValue || distance > m_MaxValue)
            {
                return 0;
            }
            
            return m_MaxValue;
        }

        public override double EditorValue => this.m_MaxValue;

        public GetDecimalRange() : base()
        {
            this.m_MinValue = 0f;
            this.m_MaxValue = 10f;
        }
        
        public GetDecimalRange(double minValue, double maxValue) : this()
        {
            this.m_MinValue = minValue;
            this.m_MaxValue = maxValue;
        }

        public static PropertyGetDecimal Create(double minValue = 0f, double maxValue = 10f) => new PropertyGetDecimal(
            new GetDecimalRange(minValue, maxValue)
        );

        public override string String => $"Range: {this.m_MinValue} - {this.m_MaxValue}";
    }
} 