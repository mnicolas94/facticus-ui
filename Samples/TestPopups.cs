using System;
using Facticus.UI;
using Facticus.UI.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Samples
{
    public class TestPopups : MonoBehaviour
    {
        [SerializeField] private UiStatePrefab _simplePopupStatePrefab;
        [SerializeField] private UiStatePrefab _messagePopupStatePrefab;
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
            await _simplePopupStatePrefab.OpenAsPopup(UiStateOpenInfo.Keep, destroyCancellationToken);
        }
        
        private async void OpenPopupMessage()
        {
            await _messagePopupStatePrefab.OpenAsPopup(_inputField.text, UiStateOpenInfo.Keep, destroyCancellationToken);
        }
    }
}