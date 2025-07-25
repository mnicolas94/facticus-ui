﻿#if ENABLED_LOCALIZATION
using Facticus.UI.WindowInterfaces;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace Facticus.UI.Popups
{
    [AddComponentMenu("Facticus.UI/Popups/Popup Localized Message")]
    public class PopupMessageLocalized : PopupSimple, IWindowInitializable<LocalizedString>
    {
        [SerializeField] private LocalizeStringEvent _text;

        public void Initialize(LocalizedString message)
        {
            _text.StringReference = message;
        }
    }
}
#endif