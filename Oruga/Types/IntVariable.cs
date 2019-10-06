using Oruga.Types.BaseClasses;
using UnityEngine;

namespace Oruga.Types
{
    [CreateAssetMenu]
    public class IntVariable : BasicTypesVariable<int>
    {
        protected override void OnEnable()
        {
            // No initialization needed
        }

        public override void ApplyChange(int change)
        {
            Value += change;
        }

        public override void ApplyChange(BasicTypesVariable<int> newValue)
        {
            Value = newValue.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}