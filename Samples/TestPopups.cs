using System;
using TMPro;
using UI.Popups;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Samples
{
    public class TestPopups : MonoBehaviour
    {
        [SerializeField] private PopupSimple _simplePopup;
        [SerializeField] private PopupMessage _messagePopup;
        
        [SerializeField] private Button _simplePopupButton;
        [SerializeField] private Button _messagePopupButton;
        [SerializeField] private TMP_InputField _inputField;

        private void Start()
        {
            _simplePopupButton.onClick.AddListener(OpenPopupSimple);
            _messagePopupButton.onClick.AddListener(OpenPopupMessage);
        }

        private async void OpenPopupSimple()
        {
            var popupPrefab = _simplePopup.gameObject;
            var windowsManager = WindowsManager.Instance;
            windowsManager.OpenNewHistoryList();
            windowsManager.OpenWindow(popupPrefab);
            
            // wait popup
            if (WindowsManager.Instance.TryGetWindowInstance(popupPrefab, out var windowInstance))
            {
                if (windowInstance.Instance.TryGetComponent<IWindowPopup>(out var windowWithResult))
                {
                    await windowWithResult.WaitPopup(destroyCancellationToken);
                }
            }
            
            windowsManager.CloseCurrentHistoryList();
        }
        
        private async void OpenPopupMessage()
        {
            var popupPrefab = _messagePopup.gameObject;
            
            var windowsManager = WindowsManager.Instance;
            windowsManager.OpenNewHistoryList();
            windowsManager.OpenWindow(popupPrefab);
            
            if (WindowsManager.Instance.TryGetWindowInstance(popupPrefab, out var windowInstance))
            {
                // initialize
                if (windowInstance.Instance.TryGetComponent<IWindowInitializable<string>>(out var windowInitializable))
                {
                    windowInitializable.Initialize(_inputField.text);
                }
                
                // wait popup
                if (windowInstance.Instance.TryGetComponent<IWindowPopup>(out var windowWithResult))
                {
                    await windowWithResult.WaitPopup(destroyCancellationToken);
                }
            }
            
            windowsManager.CloseCurrentHistoryList();
        }
    }
}