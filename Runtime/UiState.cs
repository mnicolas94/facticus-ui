using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// A list of windows that need to be open in a given UI state.
    /// </summary>
    [CreateAssetMenu(fileName = "UIState", menuName = "Facticus.UI/UI State", order = 0)]
    public class UiState : ScriptableObject
    {
        [SerializeField] private List<GameObject> _windowsPrefabs;
        public IReadOnlyList<GameObject> WindowsPrefabs => _windowsPrefabs.AsReadOnly();

        public bool IsOpen => AreAllInStatus(WindowStatus.Open);

        public bool IsClosed => AreAllInStatus(WindowStatus.Closed);
        
        public bool IsOpening => IsAnyInStatus(WindowStatus.Opening);

        public bool IsClosing => IsAnyInStatus(WindowStatus.Closing);

        public bool IsTransitioning => IsOpening || IsClosing;

        private bool AreAllInStatus(WindowStatus status)
        {
            var manager = WindowsManager.Instance;
            foreach (var windowPrefab in _windowsPrefabs)
            {
                var isInStatus = manager.GetStatus(windowPrefab) == status;
                if (!isInStatus)
                {
                    return false;
                }
            }
                
            return true;
        }
        
        private bool IsAnyInStatus(WindowStatus status)
        {
            var manager = WindowsManager.Instance;
            foreach (var windowPrefab in _windowsPrefabs)
            {
                var isInStatus = manager.GetStatus(windowPrefab) == status;
                if (isInStatus)
                {
                    return true;
                }
            }
                
            return false;
        }
        
        public void Open()
        {
            Open(new UiStateOpenInfo()
            {
                CloseOther = false,
                KeepHistory = false,
            });
        }
        
        [Obsolete("Use Open() or OpenCloseOthers()")]
        public void Open(bool closeOthers)
        {
            Open(new UiStateOpenInfo()
            {
                CloseOther = closeOthers,
                KeepHistory = false,
            });
        }

        public void OpenCloseOthers()
        {
            Open(new UiStateOpenInfo()
            {
                CloseOther = true,
                KeepHistory = false,
            });
        }

        public void OpenCloseOthersKeepHistory()
        {
            Open(new UiStateOpenInfo()
            {
                CloseOther = true,
                KeepHistory = true,
            });
        }

        public void Open(UiStateOpenInfo info)
        {
            var windowsManager = WindowsManager.Instance;
            
            if (info.CloseOther)
            {
                if (info.KeepHistory)
                {
                    windowsManager.OpenNewHistoryList();
                }
                else
                {
                    windowsManager.CloseOthers(_windowsPrefabs);
                }
            }
            
            windowsManager.OpenAll(_windowsPrefabs);
        }

        public void Open<T>(T initializationData, UiStateOpenInfo info)
        {
            var windowsManager = WindowsManager.Instance;
            Open(info);
            foreach (var windowPrefab in _windowsPrefabs)
            {
                if (windowsManager.TryGetWindowInstance(windowPrefab, out var windowInstance))
                {
                    if (windowInstance.Instance.TryGetComponent<IWindowInitializable<T>>(out var windowInitializable))
                    {
                        windowInitializable.Initialize(initializationData);
                    }
                }
            }
        }
        
        /// <summary>
        /// Open a UiState that has a <see cref="IWindowWithResult{T}"/> window and will close automatically after
        /// the result is available. Useful for popups.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ct"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async UniTask<T> OpenWaitResult<T>(UiStateOpenInfo info, CancellationToken ct)
        {
            // open the windows
            Open(info);

            var result = await WaitResultAndClose<T>(info, ct);

            // return result
            return result;
        }

        /// <summary>
        /// Same as <see cref="OpenWaitResult{T}"/> but the window can be initialized as well.
        /// </summary>
        /// <param name="initializationData"></param>
        /// <param name="info"></param>
        /// <param name="ct"></param>
        /// <typeparam name="TI"></typeparam>
        /// <typeparam name="TO"></typeparam>
        /// <returns></returns>
        public async UniTask<TO> OpenWaitResult<TI, TO>(TI initializationData, UiStateOpenInfo info, CancellationToken ct)
        {
            // open the windows
            Open(initializationData, info);

            // wait for result
            var result = await WaitResultAndClose<TO>(info, ct);
            
            // return result
            return result;
        }
        
        /// <summary>
        /// This is a useful method to access WindowsManager.CloseCurrentHistoryList() method, but it is not
        /// tied to this class' particular instance.
        /// </summary>
        public void CloseGoBackHistory()
        {
            WindowsManager.Instance.CloseCurrentHistoryList();
        }

        public void Close()
        {
            WindowsManager.Instance.CloseAll(_windowsPrefabs);
        }

        public async UniTask WaitUntilClose(CancellationToken ct)
        {
            while (!IsClosed && !ct.IsCancellationRequested)
            {
                await UniTask.Yield();
            }
        }
        
        private async UniTask<T> WaitResult<T>(CancellationToken ct)
        {
            var firstPrefab = _windowsPrefabs[0];  // only supports getting a result from the first window
            if (WindowsManager.Instance.TryGetWindowInstance(firstPrefab, out var windowInstance))
            {
                if (windowInstance.Instance.TryGetComponent<IWindowWithResult<T>>(out var windowWithResult))
                {
                    var result = await windowWithResult.WaitForResult(ct);
                    
                    return result;
                }
            }
            
            return default;
        }
        
        private async UniTask<T> WaitResultAndClose<T>(UiStateOpenInfo info, CancellationToken ct)
        {
            // wait for the result
            var result = await WaitResult<T>(ct);

            // close it
            if (info.CloseOther && info.KeepHistory)
            {
                CloseGoBackHistory();
            }
            else
            {
                Close();
            }

            return result;
        }
    }
}