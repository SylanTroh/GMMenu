#if VRC_SDK_VRCSDK3 && UNITY_EDITOR

using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRCPickup = VRC.SDK3.Components.VRCPickup;

namespace VRC.SDK3.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VRCPickup))]
    public class VRCPickupEditor3 : VRCInspectorBase
    {

        private SerializedProperty propMomentumTransferMethod;
        private SerializedProperty propDisallowTheft;
        private SerializedProperty propExactGun;
        private SerializedProperty propExactGrip;
        private SerializedProperty propAllowManipulationWhenEquipped;
        private SerializedProperty propOrientation;
        private SerializedProperty propAutoHold;
        private SerializedProperty propInteractionText;
        private SerializedProperty propUseText;
        private SerializedProperty propThrowVelocityBoostMinSpeed;
        private SerializedProperty propThrowVelocityBoostScale;
        private SerializedProperty propPickupable;
        private SerializedProperty propProximity;

        private PropertyField fieldMomentumTransferMethod;
        private PropertyField fieldDisallowTheft;
        private PropertyField fieldExactGun;
        private PropertyField fieldExactGrip;
        private PropertyField fieldAllowManipulationWhenEquipped;
        private PropertyField fieldOrientation;
        private PropertyField fieldAutoHold;
        private PropertyField fieldInteractionText;
        private PropertyField fieldUseText;
        private PropertyField fieldThrowVelocityBoostMinSpeed;
        private PropertyField fieldThrowVelocityBoostScale;
        private PropertyField fieldPickupable;
        private PropertyField fieldProximity;

        
        private void OnEnable()
        {
            propMomentumTransferMethod = serializedObject.FindProperty(nameof(VRCPickup.MomentumTransferMethod));
            propDisallowTheft = serializedObject.FindProperty(nameof(VRCPickup.DisallowTheft));
            propExactGun = serializedObject.FindProperty(nameof(VRCPickup.ExactGun));
            propExactGrip = serializedObject.FindProperty(nameof(VRCPickup.ExactGrip));
            propAllowManipulationWhenEquipped = serializedObject.FindProperty(nameof(VRCPickup.allowManipulationWhenEquipped));
            propOrientation = serializedObject.FindProperty(nameof(VRCPickup.orientation));
            propAutoHold = serializedObject.FindProperty(nameof(VRCPickup.AutoHold));
            propInteractionText = serializedObject.FindProperty(nameof(VRCPickup.InteractionText));
            propUseText = serializedObject.FindProperty(nameof(VRCPickup.UseText));
            propThrowVelocityBoostMinSpeed = serializedObject.FindProperty(nameof(VRCPickup.ThrowVelocityBoostMinSpeed));
            propThrowVelocityBoostScale = serializedObject.FindProperty(nameof(VRCPickup.ThrowVelocityBoostScale));
            propPickupable = serializedObject.FindProperty(nameof(VRCPickup.pickupable));
            propProximity = serializedObject.FindProperty(nameof(VRCPickup.proximity));
        }

        public override void BuildInspectorGUI()
        {
            base.BuildInspectorGUI();
            
            fieldInteractionText = AddFieldTooltip(propInteractionText, 
                "Text displayed when user hovers over the pickup.");
            fieldUseText = AddFieldTooltip(propUseText,
                "Text to display describing action for clicking button, when this pickup is already being held.");
            fieldProximity = AddField(propProximity);
            
            fieldAutoHold = AddFieldTooltip(propAutoHold, 
                "If the pickup is supposed to be aligned to the hand (i.e. orientation field is set to Gun or Grip), auto-detect means that it will be Equipped(not dropped when they release trigger),  otherwise just hold as a normal pickup.");
            fieldAutoHold.RegisterValueChangeCallback(AutoHoldCallback);
            
            fieldOrientation = AddField(propOrientation);
            fieldOrientation.RegisterValueChangeCallback(OrientationCallback);
            
            fieldExactGun = AddField(propExactGun);
            fieldExactGrip = AddField(propExactGrip);
            fieldPickupable = AddField(propPickupable);
            fieldDisallowTheft = AddField(propDisallowTheft);
            fieldAllowManipulationWhenEquipped = AddField(propAllowManipulationWhenEquipped);
            fieldMomentumTransferMethod = AddField(propMomentumTransferMethod);
            fieldThrowVelocityBoostMinSpeed = AddField(propThrowVelocityBoostMinSpeed);
            fieldThrowVelocityBoostScale = AddField(propThrowVelocityBoostScale);
            
            AutoHoldChanged();
            OrientationChanged();
        }

        private void AutoHoldCallback(SerializedPropertyChangeEvent evt) =>AutoHoldChanged();

        private void AutoHoldChanged()
        {
            switch ((VRC_Pickup.AutoHoldMode)propAutoHold.enumValueIndex)
            {
                case VRC_Pickup.AutoHoldMode.AutoDetect:
                    fieldUseText.SetVisible(true);
                    break;
                case VRC_Pickup.AutoHoldMode.Yes:
                    fieldUseText.SetVisible(true);
                    break;
                case VRC_Pickup.AutoHoldMode.No:
                    fieldUseText.SetVisible(false);
                    break;
            }
        }

        private void OrientationCallback(SerializedPropertyChangeEvent evt) => OrientationChanged();

        private void OrientationChanged()
        {
            switch ((VRC_Pickup.PickupOrientation)propOrientation.enumValueIndex)
            {
                case VRC_Pickup.PickupOrientation.Any:
                    fieldExactGun.SetVisible(true);
                    fieldExactGrip.SetVisible(true);
                    break;
                case VRC_Pickup.PickupOrientation.Grip:
                    fieldExactGun.SetVisible(false);
                    fieldExactGrip.SetVisible(true);
                    break;
                case VRC_Pickup.PickupOrientation.Gun:
                    fieldExactGun.SetVisible(true);
                    fieldExactGrip.SetVisible(false);
                    break;
            }
        }

    }
}
#endif