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
        private readonly Dictionary<GameObject, WindowInstance> _instances = new ();
        private readonly Dictionary<WindowInstance, WindowStatus> _requestsDuringTransition = new ();
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
        
        private async Task TransitionToOpenStatus(WindowInstance instance, CancellationToken ct)
        {
            instance.Instance.SetActive(true);
            instance.Status = WindowStatus.Opening;
            if (instance.Transitions.HasValue)
            {
                await instance.Transitions.Value.Open(ct);
            }
            instance.Status = WindowStatus.Open;
        }

        private async Task TransitionToCloseStatus(WindowInstance instance, CancellationToken ct)
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

        private async Task WaitWindowToEndTransition(WindowInstance instance, CancellationToken ct)
        {
            while (IsTransitioning(instance) && !ct.IsCancellationRequested)
            {
                await Task.Yield();
            }
        }

        /// <summary>
        /// Request that a window should be in a given status after the current transition.
        /// 
        /// </summary>
        /// <param name="windowPrefab"></param>
        /// <param name="desiredStatus"></param>
        private async void RequestStatusAfterTransition(GameObject windowPrefab, WindowStatus desiredStatus)
        {
            var instance = GetWindowsInstance(windowPrefab);

            if (_requestsDuringTransition.ContainsKey(instance))
            {
                // update the desired final status
                _requestsDuringTransition[instance] = desiredStatus;
            }
            else
            {
                // add the request
                _requestsDuringTransition.Add(instance, desiredStatus);
                
                // wait for transition
                await WaitWindowToEndTransition(instance, destroyCancellationToken);
                
                // apply the request
                var lastDesiredStatus = _requestsDuringTransition[instance];
                _requestsDuringTransition.Remove(instance);
                await ApplyWindowStatus(instance, lastDesiredStatus);
            }
        }

        /// <summary>
        /// Opens or closes a window given a status.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="desiredStatus"></param>
        private async Task ApplyWindowStatus(WindowInstance instance, WindowStatus desiredStatus)
        {
            if (desiredStatus == instance.Status)
            {
                // the window is already in the desired status
                return;
            }
            
            if (desiredStatus == WindowStatus.Open)
            {
                await TransitionToOpenStatus(instance, destroyCancellationToken);
            }
            else if (desiredStatus == WindowStatus.Closed)
            {
                await TransitionToCloseStatus(instance, destroyCancellationToken);
            }
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
            // close current history list
            CloseAll(CurrentOpenedWindows);

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
                RequestStatusAfterTransition(windowPrefab, WindowStatus.Open);
            }
            else
            {
                await ApplyWindowStatus(instance, WindowStatus.Open);
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
                    RequestStatusAfterTransition(windowPrefab, WindowStatus.Closed);
                }
                else
                {
                    await ApplyWindowStatus(instance, WindowStatus.Closed);
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
