using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SmallHUD : GMMenuPart
    {
        Transform hudOffset;
        Canvas canvas;
        RectTransform canvasTransform;

        void Start()
        {
            hudOffset = transform.GetChild(0);
            canvas = hudOffset.GetChild(0).GetComponent<Canvas>();
            canvasTransform = canvas.GetComponent<RectTransform>();
            SetCanvasTransform();
        }
        public override void PostLateUpdate()
        {
            UpdatePosition();
            UpdateRotation();
        }
        void UpdatePosition()
        {
            var headlocation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            transform.position = headlocation.position;
        }
        public void UpdateRotation()
        {
            var headlocation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            transform.rotation = headlocation.rotation;
        }
        void SetCanvasTransform()
        {
            canvasTransform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            canvasTransform.localScale = new Vector3(0.00125f, 0.00125f, 0.00125f);
            hudOffset.localPosition = new Vector3(0.42f, -0.34f, 1.0f);
            hudOffset.localRotation.SetLookRotation(-hudOffset.localPosition, new Vector3(0, 1, 0));
        }
    }
}
