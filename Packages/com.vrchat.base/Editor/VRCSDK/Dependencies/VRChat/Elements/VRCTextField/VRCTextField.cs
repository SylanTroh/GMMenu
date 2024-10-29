using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class VRCTextField: TextField
    {
        public new class UxmlFactory : UxmlFactory<VRCTextField, UxmlTraits> {}

        public new class UxmlTraits : TextField.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _placeholder = new UxmlStringAttributeDescription { name = "placeholder" };
            private readonly UxmlBoolAttributeDescription _required = new UxmlBoolAttributeDescription { name = "required" };
            
            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var textField = (VRCTextField) ve;
                textField._placeholder = _placeholder.GetValueFromBag(bag, cc);
                textField._required = _required.GetValueFromBag(bag, cc);
            }
        }

        private string _placeholder;
        private static readonly string PlaceholderClass = TextField.ussClassName + "__placeholder";
        private bool _required;
        private bool _loading;

        public bool Loading
        {
            get => _loading;
            set
            {
                _loading = value;
                SetEnabled(!_loading);
                if (_loading)
                {
                    text = "Loading...";
                }
                else
                {
                    if (text == "Loading...")
                    {
                        text = "";
                    }
                    FocusOut();
                }
                EnableInClassList(ussClassName + "__loading", _loading);
            }
        }

        public VRCTextField(): base()
        {
            RegisterCallback<FocusOutEvent>(evt => FocusOut());
            RegisterCallback<FocusInEvent>(evt => FocusIn());
            this.RegisterValueChangedCallback(ValueChanged);
        }

        public void Reset()
        {
            if (string.IsNullOrEmpty(text))
            {
                FocusOut();
                return;
            };
            RemoveFromClassList(PlaceholderClass);
        }

        private void ValueChanged(ChangeEvent<string> evt)
        {
            if (!_required) return;
            this.Q<TextInputBase>().EnableInClassList("border-red", string.IsNullOrWhiteSpace(evt.newValue));
        }

        private void FocusOut()
        {
            if (string.IsNullOrWhiteSpace(_placeholder)) return;
            if (!string.IsNullOrEmpty(text)) return;
            SetValueWithoutNotify(_placeholder);
            AddToClassList(ussClassName + "__placeholder");
        }

        private void FocusIn()
        {
            if (string.IsNullOrWhiteSpace(_placeholder)) return;
            if (!this.ClassListContains(ussClassName + "__placeholder")) return;
            this.value = string.Empty;
            this.RemoveFromClassList(ussClassName + "__placeholder");
        }
        
        public bool IsPlaceholder()
        {
            var placeholderClass = TextField.ussClassName + "__placeholder";
            return ClassListContains(placeholderClass);
        }
    }
}