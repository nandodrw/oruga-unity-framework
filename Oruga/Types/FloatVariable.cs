using Oruga.Types.BaseClasses;
using UnityEngine;

namespace Oruga.Types
{
    [CreateAssetMenu]
    public class FloatVariable : BasicTypesVariable<float>
    {
#if UNITY_EDITOR
        [Multiline]
        public string DeveloperDescription = "";
#endif

        protected override void OnEnable()
        {
            // No initialization needed
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override void ApplyChange(float change)
        {
            Value += change;
        }

        public override void ApplyChange(BasicTypesVariable<float> newValue)
        {
            Value = newValue.Value;
        }
    }
}