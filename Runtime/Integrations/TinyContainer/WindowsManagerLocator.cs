#if ENABLED_TINY_CONTAINER
using UnityEngine;

namespace UI.Integrations.TinyContainer
{
    [CreateAssetMenu(fileName = "WindowsManagerLocator", menuName = "Facticus.UI/WindowsManagerLocator", order = 0)]
    public class WindowsManagerLocator : ScriptableObject
    {
        public WindowsManager GetWindowsManager()
        {
            return Jnk.TinyContainer.TinyContainer.Global.Get<WindowsManager>();
        }
    }
}
#endif