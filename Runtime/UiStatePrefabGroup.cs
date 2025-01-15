using System;
using System.Collections.Generic;
using UnityEngine;

namespace Facticus.UI
{
    /// <summary>
    /// A list of windows to be open in a given UI state.
    /// </summary>
    [CreateAssetMenu(fileName = "UIState", menuName = "Facticus.UI/UI State", order = 0)]
    public class UiStatePrefabGroup : ScriptableObject, IUiState
    {
        [SerializeField] private List<GameObject> _windowsPrefabs;
        
        public List<GameObject> GetWindowsPrefabs()
        {
            return _windowsPrefabs;
        }
        
        public void Open()
        {
            ((IUiState)this).Open(new UiStateOpenInfo()
            {
                CloseOther = false,
                KeepHistory = false,
            });
        }
        
        [Obsolete("Use Open() or OpenCloseOthers()")]
        public void Open(bool closeOthers)
        {
            ((IUiState)this).Open(new UiStateOpenInfo()
            {
                CloseOther = closeOthers,
                KeepHistory = false,
            });
        }

        public void OpenCloseOthers()
        {
            ((IUiState)this).Open(new UiStateOpenInfo()
            {
                CloseOther = true,
                KeepHistory = false,
            });
        }

        public void OpenCloseOthersKeepHistory()
        {
            ((IUiState)this).Open(new UiStateOpenInfo()
            {
                CloseOther = true,
                KeepHistory = true,
            });
        }
        
        public void CloseGoBackHistory()
        {
            ((IUiState)this).CloseGoBackHistory();
        }

        public void Close()
        {
            ((IUiState)this).Close();
        }
    }
}