using System;
using System.Collections.Generic;
using SerializableCallback;
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
            WindowsManager.Instance.OpenAll(_windowsPrefabs);
        }
        
        [Obsolete("Use Open() or OpenCloseOthers()")]
        public void Open(bool closeOthers)
        {
            var windowsManager = WindowsManager.Instance;
            if (closeOthers)
            {
                windowsManager.CloseOthers(_windowsPrefabs);
            }
            windowsManager.OpenAll(_windowsPrefabs);
        }

        public void OpenCloseOthers()
        {
            var windowsManager = WindowsManager.Instance;
            windowsManager.CloseOthers(_windowsPrefabs);
            windowsManager.OpenAll(_windowsPrefabs);
        }

        public void OpenCloseOthersKeepHistory()
        {
            var windowsManager = WindowsManager.Instance;
            windowsManager.OpenNewHistoryList();
            windowsManager.OpenAll(_windowsPrefabs);
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
    }
}