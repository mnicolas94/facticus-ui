using UnityEngine;

namespace UI.Samples
{
    public class UiStarter : MonoBehaviour
    {
        [SerializeField] private UiState _uiState;
        
        private void Start()
        {
            _uiState.Open();
        }
    }
}