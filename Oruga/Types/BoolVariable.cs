using Oruga.Types.BaseClasses;
using UnityEngine;

namespace Oruga.Types
{
    [CreateAssetMenu]
    public class BoolVariable : BasicTypesVariable<bool>
    {
        protected override void OnEnable()
        {
            // No initialization needed
        }

        public override void ApplyChange(bool change)
        {
            Value = value && change;
        }

        public override void ApplyChange(BasicTypesVariable<bool> newValue)
        {
            Value = value && newValue.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}