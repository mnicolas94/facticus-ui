using System;
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
        public Option<IWindow> Transitions;
    }
}