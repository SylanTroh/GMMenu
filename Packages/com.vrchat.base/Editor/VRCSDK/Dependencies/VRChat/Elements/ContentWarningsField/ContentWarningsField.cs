using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class ContentWarningsField : VisualElement
    {
        private static readonly string[] CONTENT_WARNING_TAGS = { "content_sex", "content_adult", "content_violence", "content_gore", "content_horror" };

        private string GetContentWarningName(string tag)
        {
            switch (tag)
            {
                case "content_sex":
                    return "Sexually Suggestive";
                case "content_adult":
                    return "Adult Language and Themes";
                case "content_violence":
                    return "Graphic Violence";
                case "content_gore":
                    return "Excessive Gore";
                case "content_horror":
                    return "Extreme Horror";
                default:
                    return null;
            }
        }

        private VisualElement _tagsContainer;
        private Label _tagsLabel;

        private IList<string> _tags;
        public IList<string> tags
        {
            get => _tags;
            set
            {
                _tags = value;
                UpdateTags(ref _tagsContainer);
            }
        }
        private IList<string> _originalTags = new List<string>();
        public IList<string> originalTags
        {
            get => _originalTags;
            set
            {
                _originalTags = value;
                UpdateTags(ref _tagsContainer);
            }
        }

        public EventHandler<string> OnToggleTag;

        public new class UxmlFactory : UxmlFactory<ContentWarningsField, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlStringAttributeDescription _label = new UxmlStringAttributeDescription { name = "label" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var tagsField = (ContentWarningsField)ve;
                var label = _label.GetValueFromBag(bag, cc);

                if (string.IsNullOrWhiteSpace(label))
                    tagsField.Q<Label>("tags-label").AddToClassList("d-none");
                else
                    tagsField.Q<Label>("tags-label").text = label;
            }
        }

        public ContentWarningsField()
        {
            Resources.Load<VisualTreeAsset>("ContentWarningsField").CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("ContentWarningsFieldStyles"));

            _tagsContainer = this.Q(null, "tags-row");
            _tagsLabel = this.Q<Label>("tags-label");
            tags = new List<string>();
        }

        private VisualElement CreateTag(string tag)
        {
            var tagElement = new Button(() => OnToggleTag?.Invoke(this, tag));
            tagElement.AddToClassList("content-warning");
            tagElement.SetEnabled(!IsLocked(tag));

            var tagToggle = new Toggle();
            tagToggle.value = tags.Contains(tag);
            tagToggle.RegisterValueChangedCallback((change) => OnToggleTag?.Invoke(this, tag));
            tagElement.Add(tagToggle);

            var tagText = GetContentWarningName(tag);
            var tagLabel = new Label(tagText);
            tagLabel.AddToClassList("tag-label");
            tagElement.Add(tagLabel);

            return tagElement;
        }

        private void UpdateTags(ref VisualElement tagContainer)
        {
            if (tagContainer == null)
                return;

            tagContainer.Clear();
            foreach (var tag in CONTENT_WARNING_TAGS)
                tagContainer.Add(CreateTag(tag));
        }

        private bool IsLocked(string tag)
        {
            return originalTags.Contains("admin_content_reviewed") && originalTags.Contains(tag);
        }
    }
}