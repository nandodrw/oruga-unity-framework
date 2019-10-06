using UnityEngine;

namespace Oruga.Types.BaseClasses
{
    public abstract class BaseVariable<T> : ScriptableObject
    {
        public abstract override string ToString();

        public abstract void SetValue(T newValue);
    }
}
