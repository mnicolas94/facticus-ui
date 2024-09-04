using System;
using UI.Utils;
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
}