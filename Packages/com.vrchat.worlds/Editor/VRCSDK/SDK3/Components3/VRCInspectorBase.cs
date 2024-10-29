using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace VRC.SDK3.Editor
{

    public static class VRCInspectorExtensions
    {
        /// <summary>
        /// Toggle whether or not to display a VisualElement or to hide it from the layout.
        /// </summary>
        public static void SetVisible(this VisualElement element, bool value)
        {
            element.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
    
    public abstract class VRCInspectorBase : UnityEditor.Editor
    {

        protected VisualElement Root;


        public override VisualElement CreateInspectorGUI()
        {
            Root = new ();
            BuildInspectorGUI();
            return Root;
        }

        public virtual void BuildInspectorGUI()
        {
        }
        
        protected Foldout AddKeyedFoldout(string label, string key)
        {
            Foldout foldout = new ()
            {
                text = label,
                viewDataKey = key
            };
            Root.Add(foldout);
            return foldout;
        }

        protected PropertyField AddField(SerializedProperty prop)
        {
            PropertyField field = new (prop);
            Root.Add(field);
            return field;
        }

        protected PropertyField AddField(SerializedProperty prop, string label, string tooltip)
        {
            PropertyField field = new (prop)
            {
                label = label,
                tooltip = tooltip
            };
            Root.Add(field);
            return field;
        }

        protected PropertyField AddFieldLabel(SerializedProperty prop, string label)
        {
            PropertyField field = new (prop)
            {
                label = label
            };
            Root.Add(field);
            return field;
        }

        protected PropertyField AddFieldTooltip(SerializedProperty prop, string tooltip)
        {
            PropertyField field = new (prop)
            {
                tooltip = tooltip
            };
            Root.Add(field);
            return field;
        }

    }
}