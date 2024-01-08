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