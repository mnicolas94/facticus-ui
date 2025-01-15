using UnityEngine;

namespace UI.Samples
{
    public class UiStarter : MonoBehaviour
    {
        [SerializeField] private UiStatePrefabGroup _uiState;
        
        private void Start()
        {
            _uiState.Open(new UiStateOpenInfo()
            {
                
            });
        }
    }
}