#if !COMPILER_UDONSHARP && UNITY_EDITOR
using Sylan.AudioManager.EditorUtilities;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Sylan.AudioManager
{
    [CustomEditor(typeof(AudioZoneCollider))]
    public class AudioZoneEditor : Editor
    {
        private AudioZoneCollider audioZone;
        BoxCollider boxCollider;
        CapsuleCollider capsuleCollider;
        SphereCollider sphereCollider;
        MeshCollider meshCollider;
        private const float handleSize = 0.1f;
        private float shrinkAmount = 0.5f;
        private bool showFoldout = true;
        private bool hasValidMeshCollider = false;

        SerializedProperty zoneID;

        private void OnEnable()
        {
            audioZone = target as AudioZoneCollider;
            boxCollider = audioZone.GetComponent<BoxCollider>();
            capsuleCollider = audioZone.GetComponent<CapsuleCollider>();
            sphereCollider = audioZone.GetComponent<SphereCollider>();
            meshCollider = audioZone.GetComponent<MeshCollider>();
            zoneID = serializedObject.FindProperty("zoneID");
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.Space();
            showFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(showFoldout, "Audiozone Editor Settings", EditorStyles.foldoutHeader);
            if (!showFoldout)
            {
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }

            hasValidMeshCollider = meshCollider != null && meshCollider.isTrigger;

            if (boxCollider == null && sphereCollider == null && capsuleCollider == null && !hasValidMeshCollider)
            {
                EditorGUILayout.HelpBox("Add a Collider component to enable resizing handles in the scene view.", MessageType.Info);

                if (GUILayout.Button("Add BoxCollider"))
                {
                    audioZone.gameObject.AddComponent<BoxCollider>();
                    boxCollider = audioZone.gameObject.GetComponent<BoxCollider>();
                    ResetBoxCollider(boxCollider);
                }
                if (GUILayout.Button("Add SphereCollider"))
                {
                    audioZone.gameObject.gameObject.AddComponent<SphereCollider>();
                    sphereCollider = audioZone.gameObject.GetComponent<SphereCollider>();
                    ResetSphereCollider(sphereCollider);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }
            if (boxCollider != null)
            {
                EditorGUILayout.LabelField("Shrink Audiozone (This can help with players clipping)", EditorStyles.boldLabel);
                shrinkAmount = EditorGUILayout.FloatField("Shrink Amount", shrinkAmount);

                if (GUILayout.Button("Shrink"))
                {
                    ShrinkCollider(boxCollider as BoxCollider, shrinkAmount);
                }
                if (GUILayout.Button("Reset Audiozone Size"))
                {
                    ResetBoxCollider(boxCollider as BoxCollider);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }
            if (sphereCollider != null)
            {
                if (GUILayout.Button("Reset Audiozone Size"))
                {
                    ResetSphereCollider(sphereCollider as SphereCollider);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                return;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }
        private void ShrinkCollider(BoxCollider collider, float amount)
        {
            Vector3 newSize = collider.size - Vector3.one * amount;
            newSize = Vector3.Max(newSize, Vector3.zero); // Ensure the size doesn't go below zero
            collider.size = newSize;
        }
        private void ResetBoxCollider(BoxCollider collider)
        {
            var meshFilter = collider.transform.parent.gameObject.GetComponent<MeshFilter>();
            Bounds bounds;
            if (meshFilter == null) bounds = new Bounds(Vector3.zero, Vector3.one);
            else bounds = meshFilter.sharedMesh.bounds;
            collider.center = bounds.center;
            collider.size = bounds.size;
            collider.isTrigger = true;
            audioZone.transform.localPosition = Vector3.zero;
            audioZone.transform.localRotation = Quaternion.identity;
        }
        private void ResetSphereCollider(SphereCollider collider)
        {
            var meshFilter = collider.transform.parent.gameObject.GetComponent<MeshFilter>();
            Bounds bounds;
            if (meshFilter == null) bounds = new Bounds(Vector3.zero, Vector3.one);
            else bounds = meshFilter.sharedMesh.bounds; collider.center = bounds.center;
            collider.radius = bounds.extents.magnitude;
            collider.isTrigger = true;
            audioZone.transform.localPosition = Vector3.zero;
            audioZone.transform.localRotation = Quaternion.identity;
        }
        private void OnSceneGUI()
        {
            hasValidMeshCollider = meshCollider != null && meshCollider.isTrigger;

            if (boxCollider == null && sphereCollider == null && !hasValidMeshCollider) return;

            if (boxCollider != null)
            {
                EditorGUI.BeginChangeCheck();

                Vector3[] handles = GetHandlePositions();

                for (int i = 0; i < handles.Length; i++)
                {
                    handles[i] = boxCollider.transform.TransformPoint(handles[i]);
                    var fmh_140_69_638376709796116633 = Quaternion.identity; handles[i] = Handles.FreeMoveHandle(handles[i], handleSize, Vector3.one * 0.1f, Handles.SphereHandleCap);
                    handles[i] = boxCollider.transform.InverseTransformPoint(handles[i]);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(boxCollider, "Resize Cube");

                    Vector3 newSize = Vector3.zero;
                    newSize.x = Mathf.Abs(handles[1].x - handles[0].x);
                    newSize.y = Mathf.Abs(handles[3].y - handles[2].y);
                    newSize.z = Mathf.Abs(handles[5].z - handles[4].z);

                    boxCollider.size = newSize;

                    Vector3 newCenter = Vector3.zero;
                    newCenter.x = (handles[1].x + handles[0].x) / 2;
                    newCenter.y = (handles[3].y + handles[2].y) / 2;
                    newCenter.z = (handles[5].z + handles[4].z) / 2;
                    boxCollider.center = newCenter;
                }
                return;
            }
            if (sphereCollider != null)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 centerHandle = GetCenterHandlePosition();
                Vector3 radiusHandle = GetRadiusHandlePosition();

                var fmh_170_69_638376709796133627 = Quaternion.identity; centerHandle = Handles.FreeMoveHandle(centerHandle, handleSize, Vector3.one * 0.1f, Handles.SphereHandleCap);
                var fmh_171_69_638376709796135328 = Quaternion.identity; radiusHandle = Handles.FreeMoveHandle(radiusHandle, handleSize, Vector3.one * 0.1f, Handles.SphereHandleCap);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(sphereCollider, "Modify Sphere");

                    Vector3 newCenter = sphereCollider.transform.InverseTransformPoint(centerHandle);
                    var delta = newCenter - sphereCollider.center;
                    sphereCollider.center = newCenter;

                    radiusHandle += delta;
                    float newRadius = Vector3.Distance(centerHandle, radiusHandle);
                    sphereCollider.radius = newRadius;
                }
            }
        }

        private Vector3[] GetHandlePositions()
        {
            Vector3[] positions = new Vector3[6];

            Vector3 halfSize = boxCollider.size / 2;
            positions[0] = (boxCollider.center - Vector3.right * halfSize.x);
            positions[1] = (boxCollider.center + Vector3.right * halfSize.x);
            positions[2] = (boxCollider.center - Vector3.up * halfSize.y);
            positions[3] = (boxCollider.center + Vector3.up * halfSize.y);
            positions[4] = (boxCollider.center - Vector3.forward * halfSize.z);
            positions[5] = (boxCollider.center + Vector3.forward * halfSize.z);

            return positions;
        }
        private Vector3 GetCenterHandlePosition()
        {
            return sphereCollider.transform.TransformPoint(sphereCollider.center);
        }

        private Vector3 GetRadiusHandlePosition()
        {
            return sphereCollider.transform.TransformPoint(sphereCollider.center + Vector3.up * sphereCollider.radius);
        }
        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
        private static void DrawGizmos(AudioZoneCollider audioZone, GizmoType gizmoType)
        {
            var colliderTransform = audioZone.transform.Find("AudioZoneCollider");
            if (colliderTransform == null) return;
            var colliderObject = colliderTransform.gameObject;

            BoxCollider boxCollider = colliderObject.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Gizmos.color = new Color(0, 1, 0, 1.0f);
                Gizmos.matrix = Matrix4x4.TRS(boxCollider.transform.position, boxCollider.transform.rotation, boxCollider.transform.lossyScale);
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
                return;
            }
            SphereCollider sphereCollider = colliderObject.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                Gizmos.color = new Color(0, 1, 0, 1.0f);
                Gizmos.matrix = Matrix4x4.TRS(sphereCollider.transform.position, sphereCollider.transform.rotation, sphereCollider.transform.lossyScale);
                Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
            }
            MeshCollider meshCollider = colliderObject.GetComponent<MeshCollider>();
            bool hasValidMeshCollider = (meshCollider != null) && meshCollider.isTrigger;
            if (hasValidMeshCollider)
            {
                Gizmos.color = new Color(0, 1, 0, 1.0f);
                Gizmos.matrix = Matrix4x4.TRS(meshCollider.transform.position, meshCollider.transform.rotation, meshCollider.transform.lossyScale);
                Gizmos.DrawWireMesh(meshCollider.sharedMesh);
            }
        }
    }
    [InitializeOnLoad]
    public class AudioZoneInitialize : Editor, IVRCSDKBuildRequestedCallback
    {
        private static int FindAudioZoneLayer()
        {
            int layerIndex = -1;
            for (int i = 22; i < 32; i++)
            {
                if (LayerMask.LayerToName(i) == "AudioZones")
                {
                    layerIndex = i;
                    Debug.Log("Found AudioZones layer at index " + i + ".");
                }
            }

            Debug.LogWarning("No AudioZones layer found after index 21.");
            return layerIndex;
        }

        private static bool RunOnBuild()
        {
            //Object with Serialized Property(s)
            if (!SerializedPropertyUtils.GetSerializedObjects<AudioZoneCollider>(out SerializedObject[] serializedObjects)) return false;

            foreach (var serializedObject in serializedObjects)
            {
                //Set Serialized Property
                SerializedPropertyUtils.PopulateSerializedProperty<AudioZoneManager>(serializedObject, AudioZoneCollider.AudioZoneManagerPropertyName);
            }

            int collisionLayer = FindAudioZoneLayer();
            if (collisionLayer != -1)
            {
                SerializedPropertyUtils.GetObjects<AudioZoneCollider>(out AudioZoneCollider[] objects);
                foreach (var obj in objects)
                {
                    obj.gameObject.layer = FindAudioZoneLayer();
                }
            }

            return true;
        }
        //
        //Run On Play
        //
        static AudioZoneInitialize()
        //Rename Static Constructor to match Class name
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;
            RunOnBuild();
        }
        //
        // Run On Build
        //
        public int callbackOrder => 0;

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            if (requestedBuildType != VRCSDKRequestedBuildType.Scene) return false;
            return RunOnBuild();
        }
    }
}
#endif