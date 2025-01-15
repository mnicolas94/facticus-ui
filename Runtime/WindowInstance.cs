using System;
using System.Collections.Generic;
using UI.Utils;
using UnityEngine;

namespace UI
{
    public class WindowInstance
    {
        public GameObject Prefab;
        
        public GameObject Instance;
        
        /// <summary>
        /// Current status
        /// </summary>
        public WindowStatus Status;
        
        /// <summary>
        /// Last status requested. It may be different from the current status if a transition is in progress.
        /// </summary>
        public WindowStatus RequestedStatus;
        
        private readonly Dictionary<Type, IWindowInterface> _cachedInterfaces = new();

        public bool TryGetWindowInterface<T>(out T windowInterface) where T : IWindowInterface
        {
            if (_cachedInterfaces.TryGetValue(typeof(T), out var interfaceObject))
            {
                windowInterface = (T) interfaceObject;
                return true;
            }

            if (Instance.TryGetComponent(out windowInterface))
            {
                _cachedInterfaces.Add(typeof(T), windowInterface);
                return true;
            }
            
            windowInterface = default;
            return false;
        }
    }
}