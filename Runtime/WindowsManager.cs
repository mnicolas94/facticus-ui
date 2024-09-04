using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UI
{
    [DefaultExecutionOrder(-100)]
    public class WindowsManager : MonoBehaviour
    {
        private readonly Dictionary<GameObject, WindowInstance> _instances = new ();

        private WindowInstance GetWindowsInstance(GameObject windowPrefab)
        {
            if (!_instances.TryGetValue(windowPrefab, out var window))
            {
                var instance = Instantiate(windowPrefab, transform);
                instance.TryGetComponent(out IWindow transitions);
                window = new WindowInstance()
                {
                    Prefab = windowPrefab,
                    Instance = instance,
                    Status = WindowStatus.Closed,
                    Transitions = new Option<IWindow>(transitions)
                };
                _instances.Add(windowPrefab, window);
            }

            return window;
        }
        
        private async Task OpenWindowTask(GameObject windowPrefab, CancellationToken ct)
        {
            var instance = GetWindowsInstance(windowPrefab);
            
            instance.Instance.SetActive(true);
            instance.Status = WindowStatus.Opening;
            if (instance.Transitions.HasValue)
            {
                await instance.Transitions.Value.Open(ct);
            }
            instance.Status = WindowStatus.Open;
        }

        private async Task CloseWindowTask(GameObject windowPrefab, CancellationToken ct)
        {
            var instance = GetWindowsInstance(windowPrefab);

            instance.Status = WindowStatus.Closing;
            if (instance.Transitions.HasValue)
            {
                await instance.Transitions.Value.Close(ct);
            }

            if (instance.Instance)  // check if the instance is still alive because it could have been destroyed after closing the app
            {
                instance.Instance.SetActive(false);
            }
            instance.Status = WindowStatus.Closed;
        }

        private async Task WaitWindowToEndTransition(GameObject windowPrefab, CancellationToken ct)
        {
            var instance = GetWindowsInstance(windowPrefab);

            while (instance.Status is WindowStatus.Closing or WindowStatus.Opening && !ct.IsCancellationRequested)
            {
                await Task.Yield();
            }
        }

        public WindowStatus GetStatus(GameObject windowPrefab)
        {
            if (_instances.ContainsKey(windowPrefab))
            {
                var instance = GetWindowsInstance(windowPrefab);
                return instance.Status;
            }
            
            return WindowStatus.Closed;
        }
        
        public async void OpenWindow(GameObject windowPrefab)
        {
            // wait for any transition to end
            await WaitWindowToEndTransition(windowPrefab, destroyCancellationToken);
            
            var instance = GetWindowsInstance(windowPrefab);
            if (instance.Status == WindowStatus.Closed)
            {
                await OpenWindowTask(windowPrefab, destroyCancellationToken);
            }
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
            if (_instances.ContainsKey(windowPrefab))
            {
                // wait for any transition to end
                await WaitWindowToEndTransition(windowPrefab, destroyCancellationToken);
            
                var instance = GetWindowsInstance(windowPrefab);
                if (instance.Status == WindowStatus.Open)
                {
                    await CloseWindowTask(windowPrefab, destroyCancellationToken);
                }
            }
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
            foreach (var prefab in _instances.Keys)
            {
                CloseWindow(prefab);
            }
        }

        public void CloseOthers(List<GameObject> except)
        {
            foreach (var prefab in _instances.Keys)
            {
                if (!except.Contains(prefab))
                {
                    CloseWindow(prefab);
                }
            }
        }
    }
}