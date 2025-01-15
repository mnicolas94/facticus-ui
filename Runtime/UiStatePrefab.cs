using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// A component meant to be attached to window prefab in order to be able to open them individually with
    /// the <see cref="WindowsManager"/>.
    /// It is not needed if a window belongs to a <see cref="UiStatePrefabGroup"/>.
    /// </summary>
    public class UiStatePrefab : MonoBehaviour, IUiState
    {
        private readonly List<GameObject> _windowsPrefabs = new();

        private void Awake()
        {
            // destroy this component in game objects instances since it is only needed in a prefab
            if (Application.isPlaying)  // this is an optional check just in case
            {
                Destroy(this);
            }
        }

        public List<GameObject> GetWindowsPrefabs()
        {
            if (_windowsPrefabs.Count == 0)
            {
                _windowsPrefabs.Add(gameObject);
            }

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