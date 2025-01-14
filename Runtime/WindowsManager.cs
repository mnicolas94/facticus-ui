using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UI.Utils;
using UnityEngine;

namespace UI
{
    [DefaultExecutionOrder(-100)]
    public class WindowsManager : MonoBehaviour
    {
        private static WindowsManager _instance;
        public static WindowsManager Instance
        {
            get
            {
                if (!_instance)
                {
                    var go = new GameObject("Windows Manager");
                    _instance = go.AddComponent<WindowsManager>();
                    DontDestroyOnLoad(go);
                }

                return _instance;
            }
        }

        private readonly Dictionary<GameObject, WindowInstance> _instances = new ();
        private readonly List<List<GameObject>> _openedWindowsHistory = new ();

        private List<GameObject> CurrentOpenedWindows
        {
            get
            {
                if (_openedWindowsHistory.Count == 0)
                {
                    _openedWindowsHistory.Add(new());
                }
                return _openedWindowsHistory[^1];
            }
        }

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

        public WindowStatus GetStatus(GameObject windowPrefab)
        {
            if (_instances.ContainsKey(windowPrefab))
            {
                var instance = GetWindowsInstance(windowPrefab);
                return instance.Status;
            }
            
            return WindowStatus.Closed;
        }
        
#region Transitions
        
        private async UniTask TransitionToOpenStatus(WindowInstance instance, CancellationToken ct)
        {
            instance.Instance.SetActive(true);
            instance.Status = WindowStatus.Opening;
            if (instance.Transitions.HasValue)
            {
                await instance.Transitions.Value.Open(ct);
            }
            instance.Status = WindowStatus.Open;
        }

        private async UniTask TransitionToCloseStatus(WindowInstance instance, CancellationToken ct)
        {
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

        private bool IsTransitioning(WindowInstance instance)
        {
            return instance.Status is WindowStatus.Opening or WindowStatus.Closing;
        }

        private async UniTask WaitWindowToEndTransition(WindowInstance instance, CancellationToken ct)
        {
            while (IsTransitioning(instance) && !ct.IsCancellationRequested)
            {
                await UniTask.Yield();
            }
        }

        /// <summary>
        /// Opens or closes a window given a status.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="desiredStatus"></param>
        private async UniTask TransitionToStatus(WindowInstance instance, WindowStatus desiredStatus)
        {
            if (desiredStatus == instance.Status)
            {
                // the window is already in the desired status
                return;
            }

            instance.RequestedStatus = desiredStatus;
            
            if (desiredStatus == WindowStatus.Open)
            {
                await TransitionToOpenStatus(instance, destroyCancellationToken);
            }
            else if (desiredStatus == WindowStatus.Closed)
            {
                await TransitionToCloseStatus(instance, destroyCancellationToken);
            }

            if (instance.Status != instance.RequestedStatus)
            {
                // during the transition, a different status was requested 
                ResolveRequestedStatus(instance);
            }
        }

        private async void ResolveRequestedStatus(WindowInstance instance)
        {
            await TransitionToStatus(instance, instance.RequestedStatus);
        }
#endregion

#region History management

        private void AddToHistory(GameObject windowPrefab)
        {
            if (!CurrentOpenedWindows.Contains(windowPrefab))
            {
                CurrentOpenedWindows.Add(windowPrefab);
            }
        }

        private void RemoveFromHistory(GameObject windowPrefab)
        {
            if (CurrentOpenedWindows.Contains(windowPrefab))
            {
                CurrentOpenedWindows.Remove(windowPrefab);
            }
        }

        public void OpenNewHistoryList()
        {
            // store current windows in temporal list to close them after we open a new history list
            var currentOpenedWindows = CurrentOpenedWindows;
            
            // add empty history list first
            _openedWindowsHistory.Add(new List<GameObject>());
            
            // close currently opened windows.
            // they won't be removed from the history since the current list is the new one
            CloseAll(currentOpenedWindows);
        }

        public void CloseCurrentHistoryList()
        {
            // create a temporal list to avoid modifying CurrenOpenedWindows list when a window is closed
            var temp = new List<GameObject>(CurrentOpenedWindows);
            // close current history list
            CloseAll(temp);

            // remove last list from history
            if (_openedWindowsHistory.Count > 0)
            {
                _openedWindowsHistory.RemoveAt(_openedWindowsHistory.Count - 1);
            }
            
            // open the previous list in history
            OpenAll(CurrentOpenedWindows);
        }

#endregion
        
        public async void OpenWindow(GameObject windowPrefab)
        {
            // add prefab to list of opened
            AddToHistory(windowPrefab);

            var instance = GetWindowsInstance(windowPrefab);
            
            if (IsTransitioning(instance))
            {
                // request to open window when transition ends
                instance.RequestedStatus = WindowStatus.Open;
            }
            else
            {
                await TransitionToStatus(instance, WindowStatus.Open);
            }
        }

        public void OpenAll(List<GameObject> windowsPrefabs)
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
                // remove from list of opened windows
                RemoveFromHistory(windowPrefab);
                
                var instance = GetWindowsInstance(windowPrefab);
                if (IsTransitioning(instance))
                {
                    // request to close window when transition ends
                    instance.RequestedStatus = WindowStatus.Closed;
                }
                else
                {
                    await TransitionToStatus(instance, WindowStatus.Closed);
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
