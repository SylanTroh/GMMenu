
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Sylan.GMMenu
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WatchCamera : UdonSharpBehaviour
    {
        private Camera watchCamera;
        private Camera thumbnailCamera;
        [FieldChangeCallback(nameof(CameraEnabled))]
        private bool _cameraEnabled = false;
        public GameObject watchCameraPanel;
        private UdonSharpBehaviour[] WatchCameraEventListeners = new UdonSharpBehaviour[0];

        [SerializeField] RenderTexture thumbnailTexture;
        [SerializeField] RenderTexture watchTexture;

        [NotNull] GMMenuToggle menuToggle;
        [NotNull] PlayerPermissions permissions;

        private VRCPlayerApi player;
        private VRCPlayerApi[] playerList = new VRCPlayerApi[0];

        bool idsNeedUpdate = false;
        public void Start()
        {
            menuToggle = Utils.Modules.GMMenuToggle(transform);
            permissions = Utils.Modules.PlayerPermissions(transform);
            watchCamera = (Camera)transform.Find("WatchCamera").GetComponent(typeof(Camera));
            thumbnailCamera = (Camera)transform.Find("ThumbnailCamera").GetComponent(typeof(Camera));

            SendCustomEventDelayedSeconds("EnableMenuToggleListener", 0.0f);
            SendCustomEventDelayedSeconds("EnablePermissionListener", 0.0f);
        }
        public void EnablePermissionListener()
        {
            permissions.AddListener(this);
        }
        public void EnableMenuToggleListener()
        {
            menuToggle.AddListener(this);
        }
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!menuToggle.MenuState()) 
            {
                idsNeedUpdate = true;
                return; 
            }
            UpdateThumbnailIds();
            SendUpdateThumbnailIDEvent();

        }
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!menuToggle.MenuState())
            {
                idsNeedUpdate = true;
                return;
            }
            UpdateThumbnailIds();
            UpdateThumbnailsSingle();
        }
        public void UpdateThumbnailIds()
        {
            playerList = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(playerList);
            SendUpdateThumbnailIDEvent();
        }
        public bool CameraEnabled
        {
            set
            {
                if (!value)
                {
                    Debug.Log("[MenuWatchPlayer]: Set camera enabled false");
                    _cameraEnabled = false;
                    return;
                }
                Debug.Log("[MenuWatchPlayer]: Set camera enabled true");
                _cameraEnabled = true;
            }
            get => _cameraEnabled;
        }
        public void WatchOn()
        {
            watchCameraPanel.SetActive(true);
            if (!CameraEnabled)
            {
                CameraEnabled = true;
                UpdateWatch();
                return;
            }
            UpdateWatchSingle();
        }
        public void WatchOff()
        {
            watchCameraPanel.SetActive(false);
            CameraEnabled = false;
        }
        public void WatchPlayer(VRCPlayerApi p)
        {
            player = p;
            WatchOn();
        }
        public void UpdateWatch()
        {
            if (!CameraEnabled) return;
            UpdateWatchSingle();
            SendCustomEventDelayedSeconds("UpdateWatch", 3.0f);
        }
        public void UpdateWatchSingle()
        {
            if (!Utilities.IsValid(player)) return;
            MoveWatchCamera();
            watchCamera.Render();
        }
        private void MoveWatchCamera()
        {
            var headlocation = player.GetBonePosition(HumanBodyBones.Head);
            var headrotation = player.GetBoneRotation(HumanBodyBones.Head);
            transform.position = headlocation;
            transform.rotation = Quaternion.Euler(0.0f, headrotation.eulerAngles.y, 0.0f);
            float scale = (1.0f / 1.61f) * Utils.AvatarUtils.AvatarHeight(player);
            watchCamera.transform.localPosition = new Vector3(0.0f, 0.25f * scale, -1.25f * scale);
        }
        void UpdateThumbnails()
        {
            if (!menuToggle.MenuState()) return;
            UpdateThumbnailsSingle();
            SendCustomEventDelayedSeconds("UpdateThumbnails", 3.0f);
        }
        public void UpdateThumbnailsSingle()
        {
            if (permissions.getPermissionLevel() < 1) return;
            Debug.Log("[WatchCamera]: Updating Thumbnails");
            for (int i = 0; i < playerList.Length; i++)
            {
                var player = playerList[i];
                if (Utilities.IsValid(player)) UpdateThumbnail(player, i);
            }
        }
        private void UpdateThumbnail(VRCPlayerApi player, int i)
        {
            MoveThumbnailCamera(player, i);
            thumbnailCamera.Render();
        }
        private void MoveThumbnailCamera(VRCPlayerApi player, int i)
        {
            var headlocation = player.GetBonePosition(HumanBodyBones.Head);
            var headrotation = player.GetBoneRotation(HumanBodyBones.Head);
            transform.position = headlocation;
            transform.rotation = Quaternion.Euler(0.0f, headrotation.eulerAngles.y, 0.0f);
            thumbnailCamera.rect = new Rect((i % 9) * 0.1111111f, (i / 9) * 0.1111111f, 0.1111111f, 0.1111111f);
            float scale = (1.0f / 1.61f) * Utils.AvatarUtils.AvatarHeight(player);
            thumbnailCamera.transform.localPosition = new Vector3(0.0f, 0.04328103f * scale, 0.5929253f * scale);
        }
        public static void SetThumbnailUV(RawImage image, int i)
        {
            image.uvRect = new Rect((i % 9) * 0.1111111f, (i / 9) * 0.1111111f, 0.1111111f, 0.1111111f);
        }
        public int GetThumbnailID(VRCPlayerApi player)
        {
            for (int i = 0; i < playerList.Length; i++)
            {
                if (playerList[i] == player) return i;
            }
            return 0;
        }
        public void OnMenuToggleOn()
        {
            if(idsNeedUpdate) UpdateThumbnailIds();
            UpdateThumbnailsSingle();
        }
        public void OnMenuToggleOff()
        {
            WatchOff();
        }
        public void OnPermissionUpdate()
        {
            UpdateThumbnailsSingle();
        }
        private void SendUpdateThumbnailIDEvent()
        {
            Utils.Events.SendEvent("OnUpdateThumbnailID", WatchCameraEventListeners);
        }
        public void AddListener(UdonSharpBehaviour b)
        {
            Utils.ArrayUtils.Append(ref WatchCameraEventListeners, b);
        }
    }
}