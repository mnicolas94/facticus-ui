using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UI.Utils;
using UnityEngine;

namespace UI
{
    [DefaultExecutionOrder(-100)]
    public class WindowsManager : MonoBehaviour
    {
        private readonly PrefabsToInstanceMap _windowsInstances = new ();
        private readonly Dictionary<GameObject, IWindow> _objectToWindowMap = new ();
        private readonly List<GameObject> _lockedTransitions = new ();

        private GameObject GetWindowsInstance(GameObject windowPrefab)
        {
            var isNew = !_windowsInstances.ExistsInstance(windowPrefab);
            var instance = _windowsInstances.GetOrCreateInstance<GameObject>(windowPrefab);
            if (isNew)
            {
                instance.transform.SetParent(transform);
            }

            return instance;
        }

        private bool TryGetWindow(GameObject windowInstance, out IWindow window)
        {
            var found = true;
            if (!_objectToWindowMap.TryGetValue(windowInstance, out window))
            {
                found = windowInstance.TryGetComponent(out window);
                if (found)
                {
                    _objectToWindowMap.Add(windowInstance, window);
                }
            }

            return found;
        }
        
        private async Task OpenWindowTask(GameObject windowPrefab, CancellationToken ct)
        {
            var instance = GetWindowsInstance(windowPrefab);
            
            await WaitWindowToBeUnlocked(instance, ct);
            instance.SetActive(true);
            if (TryGetWindow(instance, out var window))
            {
                _lockedTransitions.Add(instance);  // lock transitions tasks of this window
                await window.Open(ct);
                _lockedTransitions.Remove(instance);  // unlock transitions
            }
        }

        private async Task CloseWindowTask(GameObject windowPrefab, CancellationToken ct)
        {
            var instance = GetWindowsInstance(windowPrefab);

            await WaitWindowToBeUnlocked(instance, ct);
            if (TryGetWindow(instance, out var window))
            {
                _lockedTransitions.Add(instance);  // lock transitions tasks of this window
                await window.Close(ct);
                _lockedTransitions.Remove(instance);  // unlock transitions
            }

            if (instance)  // check if the instance is still alive because it could have been destroyed after closing the app
            {
                instance.SetActive(false);
            }
        }

        private async Task WaitWindowToBeUnlocked(GameObject windowInstance, CancellationToken ct)
        {
            while (_lockedTransitions.Contains(windowInstance) && !ct.IsCancellationRequested)
            {
                await Task.Yield();
            }
        }

        public bool IsOpen(UiState state)
        {
            var prefabs = state.WindowsPrefabs;
            foreach (var windowPrefab in prefabs)
            {
                if (!IsOpen(windowPrefab))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsOpen(GameObject windowPrefab)
        {
            if (_windowsInstances.ExistsInstance(windowPrefab))
            {
                var instance = GetWindowsInstance(windowPrefab);
                return instance.activeSelf;
            }

            return false;
        }
        
        public async void OpenWindow(GameObject windowPrefab)
        {
            if (IsOpen(windowPrefab))
            {
                return;
            }
            
            await OpenWindowTask(windowPrefab, destroyCancellationToken);
        }

        public async void OpenAll(List<GameObject> windowsPrefabs)
        {
            foreach (var windowsPrefab in windowsPrefabs)
            {
                OpenWindow(windowsPrefab);
            }
        }

        public async void CloseWindow(GameObject windowPrefab)
        {
            if (!IsOpen(windowPrefab))
            {
                return;
            }
            
            await CloseWindowTask(windowPrefab, destroyCancellationToken);
        }
        
        public void CloseAll(List<GameObject> windowsPrefabs)
        {
            foreach (var windowPrefab in windowsPrefabs)
            {
                CloseWindow(windowPrefab);
            }
        }

        public void CloseAll()
        {
            var prefabs = _windowsInstances.GetAllPrefabsOfType<GameObject>();
            CloseAll(prefabs);
        }

        public void CloseOthers(List<GameObject> windowsPrefabs)
        {
            var prefabs = _windowsInstances.GetAllPrefabsOfType<GameObject>();
            var windowsToClose = prefabs.FindAll(windowPrefab => !windowsPrefabs.Contains(windowPrefab));
            CloseAll(windowsToClose);
        }
    }
}