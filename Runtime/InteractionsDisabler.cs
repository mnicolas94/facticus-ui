using UnityEngine;

namespace UI
{
    public class InteractionsDisabler : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        private void OnEnable()
        {
            EnableInteractions();
        }

        public void EnableInteractions()
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void DisableInteractions()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

#if UNITY_EDITOR
        
        private void OnValidate()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponentInParent<CanvasGroup>(true);
            }
        }
#endif
    }
}