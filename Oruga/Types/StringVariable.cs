using Oruga.Types.BaseClasses;
using UnityEngine;

namespace Oruga.Types
{
    [CreateAssetMenu]
    public class StringVariable : BasicTypesVariable<string>
    {
        protected override void OnEnable()
        {
            if (value == null)
                value = "";
        }

        public override void ApplyChange(string change)
        {
            Value = change;
        }

        public override void ApplyChange(BasicTypesVariable<string> newValue)
        {
            Value = newValue.Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
