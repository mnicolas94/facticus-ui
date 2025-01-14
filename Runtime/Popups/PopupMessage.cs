#if ENABLED_TEXTMESHPRO
using TMPro;
using UnityEngine;

namespace UI.Popups
{
    public class PopupMessage : PopupSimple, IWindowInitializable<string>
    {
        [SerializeField] private TextMeshProUGUI _text;

        public void Initialize(string message)
        {
            _text.text = message;
        }
    }
}
#endif