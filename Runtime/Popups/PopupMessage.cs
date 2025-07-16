#if ENABLED_TEXTMESHPRO
using Facticus.UI.WindowInterfaces;
using TMPro;
using UnityEngine;

namespace Facticus.UI.Popups
{
    [AddComponentMenu("Facticus.UI/Popups/Popup Message")]
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