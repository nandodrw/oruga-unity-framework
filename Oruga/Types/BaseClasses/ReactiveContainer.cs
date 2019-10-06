using UnityEngine;

namespace Oruga.Types.BaseClasses
{
    public abstract class ReactiveContainer : ScriptableObject
    {
        public abstract void React<T>(T change, string identifier);
    }
}
