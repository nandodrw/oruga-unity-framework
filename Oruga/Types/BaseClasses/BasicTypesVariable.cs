using UnityEngine;
using System.Collections.Generic;
using Oruga.Events;

namespace Oruga.Types.BaseClasses
{
    public abstract class BasicTypesVariable<T> : BaseVariable<T>
    {
        [SerializeField]
        protected T value;

        protected List<CallbackEvent<T>> valueUpdateCallbacks;

        public T Value
        {
            get { return value; }
            set {
                    this.value = value;
                    this.FireCallbacks(this.value);
                }
        }
        
        public override void SetValue(T newValue)
        {
            Value = newValue;
        }

        public void SetValue(BasicTypesVariable<T> newValue)
        {
            if (newValue == null)
            {
                Value = default(T);
            }
            else
            {
                Value = newValue.Value;
            }
        }

        protected abstract void OnEnable();

        public abstract void ApplyChange(T change);

        public abstract void ApplyChange(BasicTypesVariable<T> newValue);

        public abstract override string ToString();

        public void RegisterListener(CallbackEvent<T> listener)
        {
            if (valueUpdateCallbacks == null)
                valueUpdateCallbacks = new List<CallbackEvent<T>>();

            if (!valueUpdateCallbacks.Contains(listener))
                valueUpdateCallbacks.Add(listener);
        }

        public void UnregisterListener(CallbackEvent<T> listener)
        {
            if (valueUpdateCallbacks == null)
                return;
            
            if (valueUpdateCallbacks.Contains(listener))
                valueUpdateCallbacks.Remove(listener);

            if (valueUpdateCallbacks.Count == 0)
                valueUpdateCallbacks = null;
        }

        private void FireCallbacks(T newValue)
        {
            //Debug.Log("Trying to call callback");
            if (valueUpdateCallbacks == null)
                return;

            valueUpdateCallbacks.ForEach((callback) => callback.Call(value));
        }
    }
}