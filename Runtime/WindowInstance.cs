using System;
using UnityEngine;

namespace UI
{
    public class WindowInstance
    {
        public GameObject Prefab;
        public GameObject Instance;
        public WindowStatus Status;
        public Option<IWindow> Transitions;
    }

    public struct Option<T>
    {
        public T Value { get; private set; }
        public bool HasValue { get; private set; }

        public Option(T value)
        {
            Value = value;
            HasValue = value != null;
        }

        public static implicit operator Option<T>(T value)
        {
            return new Option<T>(value);
        }
        
        public static implicit operator T(Option<T> option)
        {
            return option.Value;
        }
    }
}