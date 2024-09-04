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
        [SerializeField] private SerializableValueCallback<WindowsManager> _windowsManagerGetter;
        
        [SerializeField] private List<GameObject> _windowsPrefabs;
        public IReadOnlyList<GameObject> WindowsPrefabs => _windowsPrefabs.AsReadOnly();

        public bool IsOpen => AreAllInStatus(WindowStatus.Open);

        public bool IsClosed => AreAllInStatus(WindowStatus.Closed);
        
        public bool IsOpening => IsAnyInStatus(WindowStatus.Opening);

        public bool IsClosing => IsAnyInStatus(WindowStatus.Closing);

        public bool IsTransitioning => IsOpening || IsClosing;

        private bool AreAllInStatus(WindowStatus status)
        {
            var manager = _windowsManagerGetter.Value;
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
            var manager = _windowsManagerGetter.Value;
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
        
        public void Open(bool closeOthers = true)
        {
            if (closeOthers)
            {
                _windowsManagerGetter.Value.CloseOthers(_windowsPrefabs);
            }
            _windowsManagerGetter.Value.OpenAll(_windowsPrefabs);
        }

        public void Close()
        {
            _windowsManagerGetter.Value.CloseAll(_windowsPrefabs);
        }
    }
}