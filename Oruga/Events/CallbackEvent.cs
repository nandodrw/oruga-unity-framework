using System.Collections;
using System.Collections.Generic;
using Oruga.Types;
using Oruga.Types.BaseClasses;
using UnityEngine;

namespace Oruga.Events
{
    public abstract class CallbackEvent<T>
    {
        private readonly ReactiveContainer _container;
        private readonly string _identifier;

        protected CallbackEvent(ReactiveContainer container, string identifier)
        {
            this._container = container;
            this._identifier = identifier;
        }

        public void Call(T value)
        {
            _container.React<T>(value, _identifier);
        }
    }
}
