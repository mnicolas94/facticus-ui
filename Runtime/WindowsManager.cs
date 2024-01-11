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

        private CancellationTokenSource _cts;

        private void OnEnable()
        {
            _cts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }

            _cts.Dispose();
            _cts = null;
        }

        private async Task OpenWindowTask(GameObject windowPrefab, CancellationToken ct)
        {
            var instance = _windowsInstances.GetOrCreateInstance<GameObject>(windowPrefab);
            instance.transform.SetParent(transform);
            
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
            var instance = _windowsInstances.GetOrCreateInstance<GameObject>(windowPrefab);
            await WaitWindowToBeUnlocked(instance, ct);
            if (TryGetWindow(instance, out var window))
            {
                _lockedTransitions.Add(instance);  // lock transitions tasks of this window
                await window.Close(ct);
                _lockedTransitions.Remove(instance);  // unlock transitions
            }

            instance.SetActive(false);
        }
        
        public async void OpenWindow(GameObject windowPrefab)
        {
            await OpenWindowTask(windowPrefab, _cts.Token);
        }

        public async void OpenAll(List<GameObject> windowsPrefabs)
        {
            var ct = _cts.Token;
            var openTasks = windowsPrefabs.ConvertAll(window => OpenWindowTask(window, ct));
            await Task.WhenAll(openTasks);
        }

        public async void CloseWindow(GameObject windowPrefab)
        {
            await CloseWindowTask(windowPrefab, _cts.Token);
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

        private async Task WaitWindowToBeUnlocked(GameObject windowInstance, CancellationToken ct)
        {
            while (_lockedTransitions.Contains(windowInstance) && !ct.IsCancellationRequested)
            {
                await Task.Yield();
            }
        }
    }
}