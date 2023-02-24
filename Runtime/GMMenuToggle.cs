using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;
using JetBrains.Annotations;


namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class GMMenuToggle : UdonSharpBehaviour
    {
        private bool inVR;
        private bool menuToggle = false;
        private bool inputLookDown = false;
        private bool inputLookUp = false;
        [Header("------Don't Touch------")]
        [NotNull, SerializeField] private Canvas canvas;
        private RectTransform canvasTransform;
        private UdonSharpBehaviour[] MenuToggleEventListeners = new UdonSharpBehaviour[0];
        PlayerMover playerMover;
        void Start()
        {
            inVR = Networking.LocalPlayer.IsUserInVR();
            canvasTransform = canvas.GetComponent<RectTransform>();
            Utils.CanvasUtils.CanvasSetActive(canvas, false);
            if (inVR) SetCanvasTransformVR();
            else SetCanvasTransformPC();

            playerMover = Utils.Modules.PlayerMover(transform);
        }

        public override void PostLateUpdate()
        {
            if (inVR) VRUpdate();
            else PCUpdate();
        }
        private void VRUpdate()
        {
            if (menuToggle) UpdatePositionVR();
        }
        private void PCUpdate()
        {
            if (menuToggle) UpdatePositionPC();
            if (Input.GetKeyDown(KeyCode.Tab)) MenuToggle();
        }
        public void MenuToggle()
        {
            if (menuToggle)
            {
                menuToggle = false;
                Utils.CanvasUtils.CanvasSetActive(canvas, false);
                SendMenuToggleOffEvent();
            }
            else
            {
                menuToggle = true;
                Utils.CanvasUtils.CanvasSetActive(canvas, true);
                if(inVR) MenuScale();
                else UpdateRotationPC(transform);
                SendMenuToggleOnEvent();
            }
        }
        //VR Specific
        public override void InputLookVertical(float value, UdonInputEventArgs args)
        {
            if(!inVR) return;
            if (value <= -0.65)
            {
                inputLookDown = true;
                SendCustomEventDelayedSeconds("InputLookReset", 0.75f);
            }
            else if (value > 0.65 && inputLookDown)
            {
                MenuToggle();
                InputLookReset();
            }        
        }

        public void InputLookReset()
        {
            inputLookUp = false;
            inputLookDown = false;
        }

        private void SetCanvasTransformVR()
        {
            canvasTransform.eulerAngles = new Vector3(0.0f, 90.0f, 90.0f);
            canvasTransform.localScale = new Vector3(0.0004f, 0.0004f, 0.0004f);
        }
        private void SetCanvasTransformPC()
        {
            canvasTransform.anchoredPosition3D = new Vector3(-0.1f, -0.1f, 0.2f);
            canvasTransform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            canvasTransform.localScale = new Vector3(0.00025f, 0.00025f, 0.00025f);
        }
        private void UpdatePositionVR()
        {
            var handLocation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            transform.parent.position = handLocation.position;
            transform.parent.rotation = handLocation.rotation;
        }

        private void MenuScale()
        {
            var scale = (float)2.8447205e-4*Utils.AvatarUtils.AvatarHeight(Networking.LocalPlayer);
            canvasTransform.localScale = new Vector3(scale, scale, scale);
        }
        //PC Specific
        private void UpdatePositionPC()
        {
            var headlocation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            transform.parent.position = headlocation.position;
        }
        public static void UpdateRotationPC(Transform self)
        {
            var headlocation = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            self.root.rotation = Quaternion.Euler(0.0f, headlocation.rotation.eulerAngles.y, 0.0f);
        }
        //Events
        private void SendMenuToggleOnEvent()
        {
            Utils.Events.SendEvent("OnMenuToggleOn", MenuToggleEventListeners);
        }
        private void SendMenuToggleOffEvent()
        {
            Utils.Events.SendEvent("OnMenuToggleOff",MenuToggleEventListeners);
        }
        public void AddListener(UdonSharpBehaviour b)
        {
            Utils.ArrayUtils.Append(ref MenuToggleEventListeners, b);
        }
        //Get Values
        public bool MenuState()
        {
            return menuToggle;
        }
    }
}
