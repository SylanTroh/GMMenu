using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class TagsField: VisualElement
    {

        private VisualElement _tagsContainer;
        private readonly VisualElement _addTagBlock;
        private readonly TextField _addTagField;
        private Label _tagsLabel;

        private IList<string> _tags;
        public IList<string> tags
        {
            get => _tags;
            set
            {
                if (TagFilter != null)
                {
                    value = TagFilter(value);
                }
                _tags = value;
                UpdateTags(ref _tagsContainer);
            }
        }

        public EventHandler<string> OnAddTag;
        public EventHandler<string> OnRemoveTag;
        public Func<bool> CanAddTag;
        public Func<string, string> FormatTagDisplay;
        public Func<IList<string>, IList<string>> TagFilter;
        public int TagLimit = 5;

        public new class UxmlFactory : UxmlFactory<TagsField, UxmlTraits> {}

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
                var tagsField = (TagsField) ve;
                var label = _label.GetValueFromBag(bag, cc);
                if (string.IsNullOrWhiteSpace(label))
                {
                    tagsField.Q<Label>("tags-label").AddToClassList("d-none");
                }
                else
                {
                    tagsField.Q<Label>("tags-label").text = label;
                }
            }
        }
        
        public TagsField()
        {
            Resources.Load<VisualTreeAsset>("TagsField").CloneTree(this);
            styleSheets.Add(Resources.Load<StyleSheet>("TagsFieldStyles"));
            
            _tagsContainer = this.Q(null, "tags-row");
            _addTagBlock = this.Q("add-tag-block");
            _addTagField = this.Q<TextField>("tag-input");
            _addTagField.RegisterCallback<KeyDownEvent>(AddTagKeyDown);
            _tagsLabel = this.Q<Label>("tags-label");
            this.Q<Button>("add-tag-button").clicked += AddTagConfirm;
            this.Q<Button>("cancel-add-tag-button").clicked += AddTagCancel;
            tags = new List<string>();
        }

        public TagsField(List<string> tags) : this()
        {
            this.tags = tags;
        }

        private Button CreateAddTagButton()
        {
            var tagElement = new Button();
            tagElement.AddToClassList("add-tag-button");
            tagElement.text = "Add Tag";
            return tagElement;
        }
        
        private VisualElement CreateTag(string tag)
        {
            var tagElement = new VisualElement();
            tagElement.AddToClassList("tag");
            var tagText = tag;
            if (FormatTagDisplay != null)
            {
                tagText = FormatTagDisplay(tagText);
            }
            var tagLabel = new Label(tagText);
            tagLabel.AddToClassList("tag-label");
            tagElement.Add(tagLabel);
            var tagRemoveButton = new Button(() => OnRemoveTag?.Invoke(this, tag));
            tagRemoveButton.AddToClassList("tag-remove-button");
            tagElement.Add(tagRemoveButton);
            return tagElement;
        }
        
        private void UpdateTags(ref VisualElement tagContainer)
        {
            if (tagContainer == null)
            {
                return;
            }
            tagContainer.Clear();
            foreach (var tag in tags)
            {
                tagContainer.Add(CreateTag(tag));
            }

            if ((!CanAddTag?.Invoke() ?? true) && tags.Count >= TagLimit)
            {
                return;
            }
            var addButton = CreateAddTagButton();
            addButton.clicked += AddTagClicked;
            tagContainer.Add(addButton);
        }
        
        private void AddTagClicked()
        {
            if (_addTagBlock == null) return;
            if (!enabledSelf) return;
            _addTagBlock.RemoveFromClassList("d-none");
            _addTagField.Q("unity-text-input").Focus();
        }

        private void AddTagConfirm()
        {
            if (!enabledSelf) return;
            if (_addTagBlock == null || _addTagField == null) return;
            if (string.IsNullOrWhiteSpace(_addTagField.value)) return;
            OnAddTag?.Invoke(this, _addTagField.value);
            _addTagField.value = "";
            _addTagBlock.AddToClassList("d-none");
        }
        
        private void AddTagCancel()
        {
            if (!enabledSelf) return;
            if (_addTagBlock == null || _addTagField == null) return;
            _addTagField.value = "";
            _addTagBlock.AddToClassList("d-none");
        }

        private void AddTagKeyDown(KeyDownEvent evt)
        {
            if (!enabledSelf) return;
            if (evt.keyCode == KeyCode.Return)
            {
                AddTagConfirm();
                return;
            }

            if (evt.keyCode == KeyCode.Escape)
            {
                AddTagCancel();
            }
        }
    }
}