
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using VRC.Core;
using VRC.Editor;
using VRC.SDK3.Editor;
using VRC.SDK3.Editor.Elements;
using VRC.SDKBase;
using VRC.SDKBase.Editor;
using VRC.SDKBase.Editor.Api;
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.SDKBase.Editor.Elements;
using VRC.SDKBase.Editor.V3;
using VRC.SDKBase.Editor.Validation;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
using Object = UnityEngine.Object;
using VRC.SDK3A.Editor.Elements;
using PopupWindow = UnityEditor.PopupWindow;
using UnityEngine.Rendering.PostProcessing;
using Button = UnityEngine.UIElements.Button;
using Toggle = UnityEngine.UIElements.Toggle;

[assembly: VRCSdkControlPanelBuilder(typeof(VRCSdkControlPanelWorldBuilder))]
namespace VRC.SDK3.Editor
{
    public class VRCSdkControlPanelWorldBuilder : IVRCSdkWorldBuilderApi
    {
        private static Type _postProcessVolumeType; 
        protected VRCSdkControlPanel _builder;
        protected VRC_SceneDescriptor[] _scenes;

#if UNITY_ANDROID || UNITY_IOS
        private readonly List<GameObject> _rootGameObjectsBuffer = new List<GameObject>();
#endif

        private const string COMMUNITY_LABS_HELP_URL =
            "https://creators.vrchat.com/worlds/submitting-a-world-to-be-made-public/#submitting-to-community-labs";
        private readonly string[] COMMUNITY_LABS_BLOCKED_TAGS = {"admin_approved", "system_labs", "admin_lock_labs", "system_troll"};
        private const int WORLD_MAX_CAPAITY = 80;

        #region Main Interface Methods
        
        // Gets called by "you must add a scene descriptor" SDK Panel code
        public virtual void SelectAllComponents()
        {
            List<Object> show = new List<Object>(Selection.objects);
            foreach (VRC_SceneDescriptor s in _scenes)
                show.Add(s.gameObject);
            Selection.objects = show.ToArray();
        }

        public virtual void ShowSettingsOptions()
        {
            // Draw GUI inside the Settings tab
        }

        public virtual bool IsValidBuilder(out string message)
        {
            if (VRC.SDKBase.Editor.V3.V3SdkUI.V3Enabled())
            {
                message = "Multiple pipelines are present. V3 pipeline will take priority";
                return false;
            }
            FindScenes();
            message = null;
            _pipelineManagers = Tools.FindSceneObjectsOfTypeAll<PipelineManager>();
            if (_pipelineManagers.Length > 1)
            {
                message = "Multiple Pipeline Managers found in scene. Please remove all but one.";
                return false;
            } 
            if (_scenes != null && _scenes.Length > 0) return true;
            message = "A VRCSceneDescriptor or VRCAvatarDescriptor\nis required to build VRChat SDK Content";
            return false;
        }

        protected void FindScenes()
        {
            VRC_SceneDescriptor[] newScenes = Tools.FindSceneObjectsOfTypeAll<VRC_SceneDescriptor>();

            if (_scenes != null)
            {
                foreach (VRC_SceneDescriptor s in newScenes)
                    if (_scenes.Contains(s) == false)
                        _builder.CheckedForIssues = false;
            }

            _scenes = newScenes;
        }

        public virtual void ShowBuilder()
        {
            List<UdonBehaviour> failedBehaviours = ShouldShowPrimitivesWarning();
            if (failedBehaviours.Count > 0)
            {
                _builder.OnGUIWarning(null,
                    "Udon Objects reference builtin Unity mesh assets, this won't work. Consider making a copy of the mesh to use instead.",
                    () =>
                    {
                        Selection.objects = failedBehaviours.Select(s => s.gameObject).Cast<Object>().ToArray();
                    }, FixPrimitivesWarning);
            }
            
            if (_postProcessVolumeType != null)
            {
                if (Camera.main != null && Camera.main.GetComponentInChildren(_postProcessVolumeType))
                {
                    _builder.OnGUIWarning(null,
                        "Scene has a PostProcessVolume on the Reference Camera (Main Camera). This Camera is disabled at runtime. Please move the PostProcessVolume to another GameObject.",
                        () => { Selection.activeGameObject = Camera.main.gameObject; },
                        TryMovePostProcessVolumeAwayFromMainCamera
                    );
                }
            }

            using (new GUILayout.VerticalScope())
            {
                if (_scenes.Length > 1)
                {
                    _scenes = _scenes.Where(s => s != null).ToArray();
                    Object[] gos = new Object[_scenes.Length];
                    for (int i = 0; i < _scenes.Length; ++i)
                    { 
                        gos[i] = _scenes[i].gameObject;
                    }
                    _builder.OnGUIError(null,
                        "A Unity scene containing a VRChat Scene Descriptor should only contain one Scene Descriptor.",
                        () => { Selection.objects = gos; }, null);

                    EditorGUILayout.Separator();
                    _builder.OnGUIShowIssues();
                    return;
                }

                if (_scenes.Length == 1)
                {
                    try
                    {
                        bool setupRequired = OnGUISceneSetup();
                        if (setupRequired)
                        {
                            _builder.OnGuiFixIssuesToBuildOrTest();
                            return;
                        }

                        if (!_builder.CheckedForIssues)
                        {
                            _builder.ResetIssues();
                            OnGUISceneCheck(_scenes[0]);
                            _builder.CheckedForIssues = true;
                        }
                        
                        if (_builder.NoGuiErrorsOrIssuesForItem(_scenes[0]) &&
                            _builder.NoGuiErrorsOrIssuesForItem(_builder))
                        {
                            _builder.OnGUIInformation(_scenes[0], "Everything looks good");
                        }

                        // Show general issues
                        _builder.OnGUIShowIssues();
                        // Show scene-related issues
                        _builder.OnGUIShowIssues(_scenes[0]);


                        GUILayout.FlexibleSpace();
                    }
                    catch (Exception)
                    {
                        // no-op
                    }

                    return;
                }

                EditorGUILayout.Space();
                if (UnityEditor.BuildPipeline.isBuildingPlayer)
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("Building – Please Wait ...", VRCSdkControlPanel.titleGuiStyle,
                        GUILayout.Width(VRCSdkControlPanel.SdkWindowWidth));
                }
            }
            
        }

        public virtual void RegisterBuilder(VRCSdkControlPanel baseBuilder)
        {
            _builder = baseBuilder;
        }
        
        #endregion

        #region World Validations (IMGUI)
        
        private void TryMovePostProcessVolumeAwayFromMainCamera()
        {
            if (Camera.main == null)
                return;
            if (_postProcessVolumeType == null)
                return;
            Component oldVolume = Camera.main.GetComponentInChildren(_postProcessVolumeType);
            if (!oldVolume)
                return;
            GameObject oldObject = oldVolume.gameObject;
            GameObject newObject = Object.Instantiate(oldObject);
            newObject.name = "Post Processing Volume";
            newObject.tag = "Untagged";
            foreach (Transform child in newObject.transform)
            {
                Object.DestroyImmediate(child.gameObject);
            }

            var newVolume = newObject.GetComponentInChildren(_postProcessVolumeType);
            foreach (Component c in newObject.GetComponents<Component>())
            {
                if ((c == newObject.transform) || (c == newVolume))
                    continue;
                Object.DestroyImmediate(c);
            }

            Object.DestroyImmediate(oldVolume);
            _builder.Repaint();
            Selection.activeGameObject = newObject;
        }

        [UnityEditor.Callbacks.DidReloadScripts(int.MaxValue)]
        static void DidReloadScripts()
        {
            DetectPostProcessingPackage();
        }

        static void DetectPostProcessingPackage()
        {
            _postProcessVolumeType = null;
            try
            {
                System.Reflection.Assembly
                    postProcAss = System.Reflection.Assembly.Load("Unity.PostProcessing.Runtime");
                _postProcessVolumeType = postProcAss.GetType("UnityEngine.Rendering.PostProcessing.PostProcessVolume");
            }
            catch
            {
                // -> post processing not installed
            }
        }

        
        private static bool OnGUISceneSetup()
        {
            bool areLayersSetUp = UpdateLayers.AreLayersSetup();
            bool isCollisionMatrixSetUp = UpdateLayers.IsCollisionLayerMatrixSetup();
            bool mandatoryExpand = !areLayersSetUp || !isCollisionMatrixSetUp;

            if (!mandatoryExpand) return false;

            using (new EditorGUILayout.VerticalScope())
            {
                if (!areLayersSetUp)
                {
                    using (new GUILayout.VerticalScope(VRCSdkControlPanel.boxGuiStyle))
                    {
                        GUILayout.Label("Layers", EditorStyles.boldLabel);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Label(
                                "VRChat scenes must have the same Unity layer configuration as VRChat so we can all predict things like physics and collisions. Pressing this button will configure your project's layers to match VRChat."
                                , EditorStyles.wordWrappedLabel);
                            EditorGUILayout.Space(5);
                            if (GUILayout.Button("Setup Layers for VRChat", GUILayout.Width(172)))
                            {
                                bool doIt = EditorUtility.DisplayDialog("Setup Layers for VRChat",
                                    "This adds all VRChat layers to your project and pushes any custom layers down the layer list. If you have custom layers assigned to gameObjects, you'll need to reassign them. Are you sure you want to continue?",
                                    "Do it!", "Don't do it");
                                if (doIt)
                                    UpdateLayers.SetupEditorLayers();
                            }
                        }
                    }
                }
                else
                {
                    using (new GUILayout.VerticalScope(VRCSdkControlPanel.boxGuiStyle))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Label("Layers", EditorStyles.boldLabel);
                            GUILayout.Label("Step Complete!", new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight});
                        }
                    }

                }
                
                EditorGUILayout.Space(5);

                if (!isCollisionMatrixSetUp)
                {
                    using (new GUILayout.VerticalScope(VRCSdkControlPanel.boxGuiStyle))
                    {
                        GUILayout.Label("Collision Matrix", EditorStyles.boldLabel);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Label(
                                "VRChat uses specific layers for collision. In order for testing and development to run smoothly it is necessary to configure your project's collision matrix to match that of VRChat."
                                , EditorStyles.wordWrappedLabel);
                            EditorGUILayout.Space(5);
                            if (!areLayersSetUp)
                            {
                                GUILayout.Label(
                                    "You must first configure your layers for VRChat to proceed. Please see above.",
                                    EditorStyles.wordWrappedLabel);
                            }
                            else
                            {
                                if (GUILayout.Button("Set Collision Matrix", GUILayout.Width(172)))
                                {
                                    bool doIt = EditorUtility.DisplayDialog("Setup Collision Layer Matrix for VRChat",
                                        "This will setup the correct physics collisions in the PhysicsManager for VRChat layers. Are you sure you want to continue?",
                                        "Do it!", "Don't do it");
                                    if (doIt)
                                    {
                                        UpdateLayers.SetupCollisionLayerMatrix();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return true;
        }

        private void OnGUISceneCheck(VRC_SceneDescriptor scene)
        {
            CheckUploadChanges(scene);

            List<VRC_EventHandler> sdkBaseEventHandlers = GatherComponentsOfTypeInScene<VRC_EventHandler>();

            if (sdkBaseEventHandlers.Count > 0)
            {
                _builder.OnGUIError(scene,
                    "You have Event Handlers in your scene that are not allowed in this build configuration.",
                    delegate
                    {
                        List<Object> gos = sdkBaseEventHandlers.ConvertAll(item => (Object) item.gameObject);
                        Selection.objects = gos.ToArray();
                    },
                    delegate
                    {
                        foreach (VRC_EventHandler eh in sdkBaseEventHandlers)
                        {
                            Object.DestroyImmediate(eh);
                        }
                    });
            }

            // If the user is trying to use native text components and has no TextMeshPro components, inform them that
            // TMP tends to appear clearer since it uses a signed distance field for rendering text.
            if (
                GatherComponentsOfTypeInScene<UnityEngine.UI.Text>().Count > 0 ||
                GatherComponentsOfTypeInScene<UnityEngine.TextMesh>().Count > 0
            )
            {
                // Search several common TMP types.
                if (
                    GatherComponentsOfTypeInScene<TMPro.TMP_Text>().Count == 0 &&
                    GatherComponentsOfTypeInScene<TMPro.TMP_Dropdown>().Count == 0 &&
                    GatherComponentsOfTypeInScene<TMPro.TMP_InputField>().Count == 0
                )
                {
                    _builder.OnGUIInformation(scene, "Your world contains one or more Unity text components, but no TextMeshPro components. Consider using TextMeshPro instead, since it's typically clearer and easier to read than native Unity text.");
                }
            }

            Vector3 g = Physics.gravity;
            if (Math.Abs(g.x) > float.Epsilon || Math.Abs(g.z) > float.Epsilon)
                _builder.OnGUIWarning(scene,
                    "Gravity vector is not straight down. Though we support different gravity, player orientation is always 'upwards' so things don't always behave as you intend.",
                    delegate { SettingsService.OpenProjectSettings("Project/Physics"); }, null);
            if (g.y > 0)
                _builder.OnGUIWarning(scene,
                    "Gravity vector is not straight down, inverted or zero gravity will make walking extremely difficult.",
                    delegate { SettingsService.OpenProjectSettings("Project/Physics"); }, null);
            if (Math.Abs(g.y) < float.Epsilon)
                _builder.OnGUIWarning(scene,
                    "Zero gravity will make walking extremely difficult, though we support different gravity, player orientation is always 'upwards' so this may not have the effect you're looking for.",
                    delegate { SettingsService.OpenProjectSettings("Project/Physics"); }, null);

            if (CheckFogSettings())
            {
                _builder.OnGUIWarning(
                    scene,
                    "Fog shader stripping is set to Custom, this may lead to incorrect or unnecessary shader variants being included in the build. You should use Automatic unless you change the fog mode at runtime.",
                    delegate { SettingsService.OpenProjectSettings("Project/Graphics"); },
                    delegate
                    {
                        EnvConfig.SetFogSettings(
                            new EnvConfig.FogSettings(EnvConfig.FogSettings.FogStrippingMode.Automatic));
                    });
            }

            if (scene.autoSpatializeAudioSources)
            {
                _builder.OnGUIWarning(scene,
                    "Your scene previously used the 'Auto Spatialize Audio Sources' feature. This has been deprecated, press 'Fix' to disable. Also, please add VRC_SpatialAudioSource to all your audio sources. Make sure Spatial Blend is set to 3D for the sources you want to spatialize.",
                    null,
                    delegate { scene.autoSpatializeAudioSources = false; }
                );
            }

            List<AudioSource> audioSources = GatherComponentsOfTypeInScene<AudioSource>();
            foreach (AudioSource a in audioSources)
            {
                if (a.GetComponent<ONSPAudioSource>() != null)
                {
                    _builder.OnGUIWarning(scene,
                        "Found audio source(s) using ONSP, this is deprecated. Press 'fix' to convert to VRC_SpatialAudioSource.",
                        delegate { Selection.activeObject = a.gameObject; },
                        delegate
                        {
                            Selection.activeObject = a.gameObject;
                            AutoAddSpatialAudioComponents.ConvertONSPAudioSource(a);
                        }
                    );
                    break;
                }

                if (a.GetComponent<VRC_SpatialAudioSource>() == null)
                {
                    string msg =
                        "Found 3D audio source with no VRC Spatial Audio component, this is deprecated. Press 'fix' to add a VRC_SpatialAudioSource.";
                    if (IsAudioSource2D(a))
                        msg =
                            "Found 2D audio source with no VRC Spatial Audio component, this is deprecated. Press 'fix' to add a (disabled) VRC_SpatialAudioSource.";

                    _builder.OnGUIWarning(scene, msg,
                        delegate { Selection.activeObject = a.gameObject; },
                        delegate
                        {
                            Selection.activeObject = a.gameObject;
                            AutoAddSpatialAudioComponents.AddVRCSpatialToBareAudioSource(a);
                        }
                    );
                    break;
                }
            }

            if (VRCSdkControlPanel.HasSubstances())
            {
                _builder.OnGUIWarning(scene,
                    "One or more scene objects have Substance materials. This is not supported and may break in game. Please bake your Substances to regular materials.",
                    () => { Selection.objects = VRCSdkControlPanel.GetSubstanceObjects(); },
                    null);
            }

            if (VRCSdkControlPanel.ReferenceCameraHasTAAEnabled())
            {
                _builder.OnGUIWarning(scene,
                    "The Reference Camera has a Post-process Layer component that uses Temporal Anti-Aliasing. This is not supported. Please select a different anti-aliasing method.",
                    () => {
                        var refCam = VRCSdkControlPanel.GetReferenceCameraObject();
                        Selection.objects = refCam != null 
                                            ? new Object[] { refCam } 
                                            : new Object[0]; 
                    },
                    () => {
                        PostProcessLayer ppl = VRCSdkControlPanel.GetReferenceCameraObject()?.GetComponent<PostProcessLayer>();
                        if (ppl != null)
                            ppl.antialiasingMode = PostProcessLayer.Antialiasing.None;
                    });
            }

            string vrcFilePath = UnityWebRequest.UnEscapeURL(EditorPrefs.GetString("lastVRCPath"));
            bool isMobilePlatform = ValidationEditorHelpers.IsMobilePlatform();
            if (!string.IsNullOrEmpty(vrcFilePath))
            {
                if (ValidationEditorHelpers.CheckIfAssetBundleFileTooLarge(ContentType.World, vrcFilePath, out int fileSize, isMobilePlatform))
                {
                    _builder.OnGUIWarning(scene,
                        ValidationHelpers.GetAssetBundleOverSizeLimitMessageSDKWarning(ContentType.World, fileSize, isMobilePlatform), null, null);
                }
            }

            if (ValidationEditorHelpers.CheckIfUncompressedAssetBundleFileTooLarge(ContentType.World, out int fileSizeUncompressed, isMobilePlatform))
            {
                _builder.OnGUIWarning(scene,
                    ValidationHelpers.GetAssetBundleOverSizeLimitMessageSDKWarning(ContentType.World, fileSizeUncompressed, isMobilePlatform, false), null, null);
            }

#if UNITY_ANDROID || UNITY_IOS
            _rootGameObjectsBuffer.Clear();
            scene.gameObject.scene.GetRootGameObjects(_rootGameObjectsBuffer);
            foreach (GameObject go in _rootGameObjectsBuffer)
            {
                // check root game objects for illegal shaders
                IEnumerable<Shader> illegalShaders = VRC.SDKBase.Validation.WorldValidation.FindIllegalShaders(go);
                foreach (Shader s in illegalShaders)
                {
                    _builder.OnGUIWarning(scene, "World uses unsupported shader '" + s.name + "'. This could cause low performance or future compatibility issues.", null, null);
                }
            }
#endif

            // Recommend super sampled UI if the shader is available.
            Shader ssShader = Shader.Find("VRChat/Mobile/Worlds/Supersampled UI");
            if (ssShader != null)
            {
                List<Graphic> uiDefaultGraphics = GatherComponentsOfTypeInScene<Graphic>();
                for (int i = uiDefaultGraphics.Count - 1; i >= 0; i--)
                {
                    // Only keep Graphic components using UI/Default
                    // Since material is virtual and could be overridden by anything, protect with try-catch
                    // in case it throws.
                    bool usingDefaultUi;
                    try
                    {
                        const string defaultMaterialName = "UI/Default";

                        Graphic graphic = uiDefaultGraphics[i];
                        if (graphic is TMPro.TMP_SubMeshUI tmpSubMeshUi)
                        {
                            // Special treatment of this TMP type, because this class will throw when accessing material
                            // if sharedMaterial is null...
                            usingDefaultUi = tmpSubMeshUi.sharedMaterial != null &&
                                             tmpSubMeshUi.sharedMaterial.shader.name == defaultMaterialName;
                        }
                        else
                        {
                            usingDefaultUi = uiDefaultGraphics[i].material != null &&
                                             uiDefaultGraphics[i].material.shader.name == defaultMaterialName &&
                                             uiDefaultGraphics[i].materialForRendering != null &&
                                             uiDefaultGraphics[i].materialForRendering.shader.name == defaultMaterialName;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);

                        // We can't tell if this is default or not. Just treat it as not default and leave it alone.
                        usingDefaultUi = false;
                    }

                    if (!usingDefaultUi)
                    {
                        uiDefaultGraphics.RemoveAt(i);
                    }
                }
                if (uiDefaultGraphics.Count > 0)
                {
                    _builder.OnGUIInformation(scene,
                        "Found one or more UI graphics using Unity's built-in UI shader. Press 'Auto Fix' to switch affected graphics to a super-sampled UI shader, which can help improve readability in VR.",
                        () =>
                        {
                            Object[] graphicObjects = new Object[uiDefaultGraphics.Count];
                            for (int i = 0; i < uiDefaultGraphics.Count; i++)
                            {
                                graphicObjects[i] = uiDefaultGraphics[i].gameObject;
                            }
                            Selection.objects = graphicObjects;
                        },
                        () =>
                        {
                            bool hasReportedMissingBuiltInSsMaterial = false;
                            // ReSharper disable once CoVariantArrayConversion
                            Undo.RecordObjects(uiDefaultGraphics.ToArray(), "Switch to Super-Sampled UI");
                            foreach (Graphic graphic in uiDefaultGraphics)
                            {
                                Material material = graphic.material;
                                if (!AssetDatabase.IsMainAsset(material))
                                {
                                    // This material isn't a file in the project, most likely the Default UI material built into Unity.
                                    // We can't modify this, so replace it instead.
                                    Material replacementMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.vrchat.worlds/Editor/VRCSDK/SDK3/VRCSuperSampledUIMaterial.mat");
                                    if (replacementMaterial != null)
                                    {
                                        material = replacementMaterial;
                                    }
                                    else
                                    {
                                        if (!hasReportedMissingBuiltInSsMaterial)
                                        {
                                            Debug.LogWarning("Could not find substitute super-sampled UI material in the worlds package. (VRCSuperSampledUIMaterial)");
                                            hasReportedMissingBuiltInSsMaterial = true;
                                        }
                                        continue;
                                    }
                                }

                                material.shader = ssShader;
                                graphic.material = material;
                            }
                        });
                }
            }
            
            foreach (VRC.SDK3.Components.VRCObjectSync os in GatherComponentsOfTypeInScene<VRC.SDK3.Components.VRCObjectSync>())
            {
                if (os.GetComponents<VRC.Udon.UdonBehaviour>().Any((ub) => ub.SyncIsManual))
                    _builder.OnGUIError(scene, "Object Sync cannot share an object with a manually synchronized Udon Behaviour",
                        delegate { Selection.activeObject = os.gameObject; }, null);
                if (os.GetComponent<VRC.SDK3.Components.VRCObjectPool>() != null)
                    _builder.OnGUIError(scene, "Object Sync cannot share an object with an object pool",
                        delegate { Selection.activeObject = os.gameObject; }, null);
            }
            
            VerifyMaxTextureSize(scene);
        }

        private void VerifyMaxTextureSize(VRC_SceneDescriptor scene)
        {
            var badTextureImporters = VRCSdkControlPanel.GetOversizeTextureImporters(GatherComponentsOfTypeInScene<Renderer>());
            
            if (badTextureImporters.Count == 0)
                return;

            _builder.OnGUIWarning(scene, $"This scene has textures bigger than {VRCSdkControlPanel.MAX_SDK_TEXTURE_SIZE}. Please reduce them to save memory in your world.",
                null,
                () =>
                {
                    List<string> paths = new List<string>();
                    foreach (TextureImporter t in badTextureImporters)
                    {
                        Undo.RecordObject(t, $"Set Max Texture Size to {VRCSdkControlPanel.MAX_SDK_TEXTURE_SIZE}");
                        t.maxTextureSize = VRCSdkControlPanel.MAX_SDK_TEXTURE_SIZE;
                        EditorUtility.SetDirty(t);
                        paths.Add(t.assetPath);
                    }

                    AssetDatabase.ForceReserializeAssets(paths);
                    AssetDatabase.Refresh();
                });
        }

        /// <summary>
        /// Get all components of a given type in loaded scenes, including disabled components.
        /// </summary>
        private static List<T> GatherComponentsOfTypeInScene<T>() where T : UnityEngine.Component
        {
            T[] candidates = Resources.FindObjectsOfTypeAll<T>();
            List<T> results = new List<T>(candidates.Length);

            foreach (T candidate in candidates)
            {
                if (!EditorUtility.IsPersistent(candidate.transform.root.gameObject) && !(candidate.hideFlags == HideFlags.NotEditable || candidate.hideFlags == HideFlags.HideAndDontSave))
                {
                    results.Add(candidate);
                }
            }

            return results;
        }

        private static void CheckUploadChanges(VRC_SceneDescriptor scene)
        {
            if (!EditorPrefs.HasKey("VRC.SDKBase_scene_changed") ||
                !EditorPrefs.GetBool("VRC.SDKBase_scene_changed")) return;
            EditorPrefs.DeleteKey("VRC.SDKBase_scene_changed");

            if (EditorPrefs.HasKey("VRC.SDKBase_capacity"))
            {
                scene.capacity = EditorPrefs.GetInt("VRC.SDKBase_capacity");
                EditorPrefs.DeleteKey("VRC.SDKBase_capacity");
            }

            if (EditorPrefs.HasKey("VRC.SDKBase_content_sex"))
            {
                scene.contentSex = EditorPrefs.GetBool("VRC.SDKBase_content_sex");
                EditorPrefs.DeleteKey("VRC.SDKBase_content_sex");
            }

            if (EditorPrefs.HasKey("VRC.SDKBase_content_violence"))
            {
                scene.contentViolence = EditorPrefs.GetBool("VRC.SDKBase_content_violence");
                EditorPrefs.DeleteKey("VRC.SDKBase_content_violence");
            }

            if (EditorPrefs.HasKey("VRC.SDKBase_content_gore"))
            {
                scene.contentGore = EditorPrefs.GetBool("VRC.SDKBase_content_gore");
                EditorPrefs.DeleteKey("VRC.SDKBase_content_gore");
            }

            if (EditorPrefs.HasKey("VRC.SDKBase_content_other"))
            {
                scene.contentOther = EditorPrefs.GetBool("VRC.SDKBase_content_other");
                EditorPrefs.DeleteKey("VRC.SDKBase_content_other");
            }

            if (EditorPrefs.HasKey("VRC.SDKBase_release_public"))
            {
                scene.releasePublic = EditorPrefs.GetBool("VRC.SDKBase_release_public");
                EditorPrefs.DeleteKey("VRC.SDKBase_release_public");
            }

            EditorUtility.SetDirty(scene);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static bool CheckFogSettings()
        {
            EnvConfig.FogSettings fogSettings = EnvConfig.GetFogSettings();
            if (fogSettings.fogStrippingMode == EnvConfig.FogSettings.FogStrippingMode.Automatic)
            {
                return false;
            }

            return fogSettings.keepLinear || fogSettings.keepExp || fogSettings.keepExp2;
        }

        private static bool IsAudioSource2D(AudioSource src)
        {
            AnimationCurve curve = src.GetCustomCurve(AudioSourceCurveType.SpatialBlend);
            return Math.Abs(src.spatialBlend) < float.Epsilon && (curve == null || curve.keys.Length <= 1);
        }
        
        private static Mesh[] _primitiveMeshes;

        private static List<UdonBehaviour> ShouldShowPrimitivesWarning()
        {
            if (_primitiveMeshes == null)
            {
                PrimitiveType[] primitiveTypes = (PrimitiveType[]) System.Enum.GetValues(typeof(PrimitiveType));
                _primitiveMeshes = new Mesh[primitiveTypes.Length];

                for (int i = 0; i < primitiveTypes.Length; i++)
                {
                    PrimitiveType primitiveType = primitiveTypes[i];
                    GameObject go = GameObject.CreatePrimitive(primitiveType);
                    _primitiveMeshes[i] = go.GetComponent<MeshFilter>().sharedMesh;
                    Object.DestroyImmediate(go);
                }
            }

            UdonBehaviour[] allBehaviours = Object.FindObjectsOfType<UdonBehaviour>();
            List<UdonBehaviour> failedBehaviours = new List<UdonBehaviour>(allBehaviours.Length);
            foreach (UdonBehaviour behaviour in allBehaviours)
            {
                IUdonVariableTable publicVariables = behaviour.publicVariables;
                foreach (string symbol in publicVariables.VariableSymbols)
                {
                    if (!publicVariables.TryGetVariableValue(symbol, out Mesh mesh))
                    {
                        continue;
                    }

                    if (mesh == null)
                    {
                        continue;
                    }

                    bool all = true;
                    foreach (Mesh primitiveMesh in _primitiveMeshes)
                    {
                        if (mesh != primitiveMesh)
                        {
                            continue;
                        }

                        all = false;
                        break;
                    }

                    if (all)
                    {
                        continue;
                    }

                    failedBehaviours.Add(behaviour);
                }
            }

            return failedBehaviours;
        }

        private void FixPrimitivesWarning()
        {
            UdonBehaviour[] allObjects = Object.FindObjectsOfType<UdonBehaviour>();
            foreach (UdonBehaviour behaviour in allObjects)
            {
                IUdonVariableTable publicVariables = behaviour.publicVariables;
                foreach (string symbol in publicVariables.VariableSymbols)
                {
                    if (!publicVariables.TryGetVariableValue(symbol, out Mesh mesh))
                    {
                        continue;
                    }

                    if (mesh == null)
                    {
                        continue;
                    }

                    bool all = true;
                    foreach (Mesh primitiveMesh in _primitiveMeshes)
                    {
                        if (mesh != primitiveMesh)
                        {
                            continue;
                        }

                        all = false;
                        break;
                    }

                    if (all)
                    {
                        continue;
                    }

                    Mesh clone = Object.Instantiate(mesh);

                    Scene scene = behaviour.gameObject.scene;
                    string scenePath = Path.GetDirectoryName(scene.path) ?? "Assets";

                    string folderName = $"{scene.name}_MeshClones";
                    string folderPath = Path.Combine(scenePath, folderName);

                    if (!AssetDatabase.IsValidFolder(folderPath))
                    {
                        AssetDatabase.CreateFolder(scenePath, folderName);
                    }

                    string assetPath = Path.Combine(folderPath, $"{clone.name}.asset");

                    Mesh existingClone = AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
                    if (existingClone == null)
                    {
                        AssetDatabase.CreateAsset(clone, assetPath);
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        clone = existingClone;
                    }

                    publicVariables.TrySetVariableValue(symbol, clone);
                    EditorSceneManager.MarkSceneDirty(behaviour.gameObject.scene);
                }
            }
        }
        
        #endregion
        
        #region World Builder UI (UIToolkit)
        
        public void CreateBuilderErrorGUI(VisualElement root)
        {
            var errorContainer = new VisualElement();
            errorContainer.AddToClassList("builder-error-container");
            root.Add(errorContainer);
            var errorLabel = new Label(_pipelineManagers.Length > 1 ?
                "You can only have a single Pipeline Manager in a Scene" :
                "A VRCSceneDescriptor is required to build a VRChat World"
            );
            errorLabel.AddToClassList("mb-2");
            errorLabel.AddToClassList("text-center");
            errorLabel.AddToClassList("white-space-normal");
            errorLabel.style.maxWidth = 450;
            errorContainer.Add(errorLabel);

            if (_pipelineManagers.Length > 1)
            {
                var selectButton = new Button
                {
                    text = "Select all PipelineManagers"
                };
                selectButton.clicked += () =>
                {
                    Selection.objects = _pipelineManagers.Select(p => (Object) p.gameObject).ToArray();
                };
                errorContainer.Add(selectButton);
                return;
            }
            
            var addButton = new Button
            {
                text = "Add a VRCSceneDescriptor",
                tooltip = "Adds a VRCSceneDescriptor to the Scene"
            };
            addButton.clickable.clicked += () =>
            {
                var VRCWorld =
                    AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.vrchat.worlds/Samples/UdonExampleScene/Prefabs/VRCWorld.prefab");
                if (VRCWorld != null)
                {
                    var newVrcWorld = GameObject.Instantiate(VRCWorld);
                    Undo.RecordObject(newVrcWorld, "Adjusted Name");
                    newVrcWorld.name = "VRCWorld";
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
                _builder.ResetIssues();
            };
            errorContainer.Add(addButton);
        }

        private VRCWorld _worldData;
        private VRCWorld _originalWorldData;
        private VisualElement _saveChangesBlock;
        private VisualElement _visualRoot;
        private TagsField _tagsField;
        private VRCTextField _nameField;
        private VRCTextField _descriptionField;
        private IntegerField _capacityField;
        private IntegerField _recommendedCapacityField;
        private Label _lastUpdatedLabel;
        private Label _versionLabel;
        private Button _saveChangesButton;
        private Button _discardChangesButton;
        private Button _updateCancelButton;
        private Foldout _infoFoldout;
        private ThumbnailFoldout _thumbnailFoldout;
        private Thumbnail _thumbnail;
        private Button _buildAndTestButton;
        private Button _buildAndUploadButton;
        private VisualElement _progressBar;
        private VisualElement _progressBlock;
        private Label _progressText;
        private Button _testLastBuildButton;
        private Button _uploadLastBuildButton;
        private VisualElement _newWorldBlock;
        private Checklist _creationChecklist;
        private Toggle _worldDebuggingToggle;
        private ContentWarningsField _contentWarningsField;
        protected VisualElement _v3Block;
        private VisualElement _platformSwitcher;
        private VisualElement _publishBlock;
        private Button _publishButton;
        private Label _visibilityLabel;
        private VisualElement _visibilityFoldout;
        private Toggle _acceptTermsToggle;
        private Dictionary<string, Foldout> _foldouts = new Dictionary<string, Foldout>();
        
        protected PipelineManager[] _pipelineManagers;
        private string _lastBlueprintId;

        private string _newThumbnailImagePath;
        
        private bool _isContentInfoDirty;
        private bool IsContentInfoDirty
        {
            get => _isContentInfoDirty;
            set
            {
                _isContentInfoDirty = value;
                var isDirty = CheckDirty();
                var alreadyDirty = !_saveChangesBlock.ClassListContains("d-none");
                _saveChangesBlock.EnableInClassList("d-none", !isDirty);
                if (isDirty && !alreadyDirty)
                {
                    _saveChangesBlock.experimental.animation.Start(new Vector2(_visualRoot.layout.width, 0), new Vector2(_visualRoot.layout.width, 30), 250, (element, vector2) =>
                    {
                        element.style.height = vector2.y;
                    });
                }
            }
        }

        private bool UpdateCancelEnabled
        {
            get => !_updateCancelButton.ClassListContains("d-none");
            set
            {
                bool wasEnabled = UpdateCancelEnabled;
                if (wasEnabled != value)
                {
                    _updateCancelButton.EnableInClassList("d-none", wasEnabled);
                }
            }
        }

        private bool _uiEnabled;
        private bool UiEnabled
        {
            get => _uiEnabled;
            set
            {
                _uiEnabled = value;
                _infoFoldout.SetEnabled(value);
                _saveChangesButton.SetEnabled(value);
                _discardChangesButton.SetEnabled(value);
                _buildAndTestButton?.SetEnabled(value);
                _buildAndUploadButton?.SetEnabled(value);
                _testLastBuildButton?.SetEnabled(value);
                _uploadLastBuildButton?.SetEnabled(value);
                _thumbnailFoldout.SetEnabled(value);
                _platformSwitcher.SetEnabled(value);
                _visibilityFoldout.SetEnabled(value);
                _acceptTermsToggle?.SetEnabled(value);
            }
        }
        
        private struct ProgressBarStateData
        {
            public bool Visible { get; set; }
            public string Text { get; set; }
            public float Progress { get; set; }
            
            public static implicit operator ProgressBarStateData(bool visible)
            {
                return new ProgressBarStateData {Visible = visible};
            }
        }

        private ProgressBarStateData _progressBarState;
        private ProgressBarStateData ProgressBarState
        {
            get => _progressBarState;
            set
            {
                if (_progressBarState.Visible != value.Visible)
                {
                    _progressBlock.EnableInClassList("d-none", !value.Visible);
                    if (value.Visible)
                    {
                        _progressBar.style.width = 0;
                    }
                }

                _progressText.text = value.Text;
                if (Mathf.Abs(_progressBarState.Progress - value.Progress) > float.Epsilon)
                {
                    // execute on next frame to allow for layout to calculate
                    _visualRoot.schedule.Execute(() =>
                    {
                        _progressBar.experimental.animation.Start(
                            new StyleValues {width = _progressBar.layout.width, height = 28f}, 
                            new StyleValues {width = _progressBlock.layout.width * value.Progress, height = 28f}, 
                            500
                        );
                    }).StartingIn(50);
                }
                _progressBarState = value;
            }
        }

        private bool _isNewWorld;
        private bool IsNewWorld
        {
            get => _isNewWorld;
            set
            {
                _isNewWorld = value;
                _newWorldBlock.EnableInClassList("d-none", !value);
            }
        }

        public async void CreateContentInfoGUI(VisualElement root)
        {
            root.Clear();
            root.UnregisterCallback<DetachFromPanelEvent>(HandlePanelDetach);
            EditorSceneManager.sceneClosed -= HandleSceneClosed;
            VRCSdkControlPanel.OnSdkPanelDisable -= HandleSdkPanelDisable;
            
            var tree = Resources.Load<VisualTreeAsset>("VRCSdkWorldBuilderContentInfo");
            tree.CloneTree(root);
            var styles = Resources.Load<StyleSheet>("VRCSdkWorldBuilderContentInfoStyles");
            if (!root.styleSheets.Contains(styles))
            {
                root.styleSheets.Add(styles);
            }
            
            root.RegisterCallback<DetachFromPanelEvent>(HandlePanelDetach);
            EditorSceneManager.sceneClosed += HandleSceneClosed;
            VRCSdkControlPanel.OnSdkPanelDisable += HandleSdkPanelDisable;
            
            _visualRoot = root;
            _nameField = root.Q<VRCTextField>("content-name");
            _descriptionField = root.Q<VRCTextField>("content-description");
            _capacityField = root.Q<IntegerField>("content-capacity");
            _infoFoldout = root.Q<Foldout>("info-foldout");
            var capacityFieldHelpButton = root.Q<Button>("show-capacity-help-button");
            _recommendedCapacityField = root.Q<IntegerField>("content-recommended-capacity");
            var recommendedCapacityFieldHelpButton = root.Q<Button>("show-recommended-capacity-help-button");
            _thumbnailFoldout = root.Q<ThumbnailFoldout>();
            _thumbnail = _thumbnailFoldout.Thumbnail;
            _tagsField = root.Q<TagsField>("content-tags");
            _contentWarningsField = root.Q<ContentWarningsField>("content-warnings");
            var worldDebuggingHelpButton = root.Q<Button>("show-world-debugging-help-button");
            var platformsBlock = root.Q<Label>("content-platform-info");
            _lastUpdatedLabel = root.Q<Label>("last-updated-label");
            _versionLabel = root.Q<Label>("version-label");
            _saveChangesBlock = root.panel.visualTree.Q("save-changes-block");
            _saveChangesButton = _saveChangesBlock.Q<Button>("save-changes-button");
            _discardChangesButton = _saveChangesBlock.Q<Button>("discard-changes-button");
            _newWorldBlock = root.Q("new-world-block");
            _creationChecklist = root.Q<Checklist>("new-world-checklist");
            _worldDebuggingToggle = root.Q<Toggle>("world-debugging-toggle");
            _platformSwitcher = _builder.rootVisualElement.Q("platform-switcher");
            _progressBlock = _builder.rootVisualElement.Q("progress-section");
            _progressBar = _builder.rootVisualElement.Q("update-progress-bar");
            _progressText = _builder.rootVisualElement.Q<Label>("update-progress-text");
            _progressBarState = false;
            _updateCancelButton = _builder.rootVisualElement.Q<Button>("update-cancel-button");

            _publishBlock = root.Q("publish-block");
            _visibilityFoldout = root.Q("visibility-foldout");
            _publishButton = root.Q<Button>("publish-button");
            _visibilityLabel = root.Q<Label>("visibility-label");
            var communityLabsHelpButton = root.Q<Button>("community-labs-help-button");
            
            var foldouts = root.Query<Foldout>().ToList();
            _foldouts.Clear();
            foreach (var foldout in foldouts)
            {
                _foldouts[foldout.name] = foldout;
                foldout.RegisterValueChangedCallback(HandleFoldoutToggle);
                foldout.SetValueWithoutNotify(SessionState.GetBool($"{WorldBuilderSessionState.SESSION_STATE_PREFIX}.Foldout.{foldout.name}", true));
            }

            // Load the world data
            _nameField.Loading = true;
            _descriptionField.Loading = true;
            _thumbnail.Loading = true;
            _capacityField.SetValueWithoutNotify(32);
            _recommendedCapacityField.SetValueWithoutNotify(16);
            _nameField.Reset();
            _descriptionField.Reset();
            _thumbnail.ClearImage();
            IsNewWorld = false;
            UiEnabled = false;

            capacityFieldHelpButton.clicked += () =>
            {
                root.Q("capacity-help-text").ToggleInClassList("d-none");
            };
            recommendedCapacityFieldHelpButton.clicked += () =>
            {
                root.Q("recommended-capacity-help-text").ToggleInClassList("d-none");
            };
            worldDebuggingHelpButton.clicked += () =>
            {
                root.Q("world-debugging-help-text").ToggleInClassList("d-none");
            };
            communityLabsHelpButton.clicked += () =>
            {
                Application.OpenURL(COMMUNITY_LABS_HELP_URL);
            };
            
            _pipelineManagers = Tools.FindSceneObjectsOfTypeAll<PipelineManager>();
            if (_pipelineManagers.Length == 0)
            {
                Core.Logger.LogError("No PipelineManager found in scene, make sure you have added a scene descriptor");
                return;
            }

            var worldId = _pipelineManagers[0].blueprintId;
            _lastBlueprintId = worldId;
            _worldData = new VRCWorld();
            if (string.IsNullOrWhiteSpace(worldId))
            {
                IsNewWorld = true;
            }
            else
            {
                try
                {
                    _worldData = await VRCApi.GetWorld(worldId, true);

                    if (APIUser.CurrentUser != null && _worldData.AuthorId != APIUser.CurrentUser?.id)
                    {
                        Core.Logger.LogError("Loaded data for a world we do not own, clearing blueprint ID");
                        Undo.RecordObject(_pipelineManagers[0], "Cleared the blueprint ID we do not own");
                        _pipelineManagers[0].blueprintId = "";
                        worldId = "";
                        _lastBlueprintId = "";
                        _worldData = new VRCWorld();
                        IsNewWorld = true;
                    }

                }
                catch (TaskCanceledException)
                {
                    // world scene was changed
                    return;
                }
                catch (ApiErrorException ex)
                {
                    if (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        // 404 here with a defined blueprint usually means we do not own the content
                        // so we clear the blueprint ID and treat it as a new avatar
                        Core.Logger.LogError("Attempted to load the data for a world we do not own, clearing blueprint ID");
                        Undo.RecordObject(_pipelineManagers[0], "Cleared the blueprint ID we do not own");
                        _pipelineManagers[0].blueprintId = "";
                        worldId = "";
                        _lastBlueprintId = "";
                        _worldData = new VRCWorld();
                        IsNewWorld = true;
                    }
                    else
                    {
                        Debug.LogError(ex.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            if (IsNewWorld)
            {
                RestoreSessionState();

                platformsBlock.parent.AddToClassList("d-none");

                _worldData.CreatedAt = DateTime.Now;
                _worldData.UpdatedAt = DateTime.MinValue;
                _lastUpdatedLabel.parent.AddToClassList("d-none");
                _versionLabel.parent.AddToClassList("d-none");
                _worldData.Capacity = 32;
                _worldData.RecommendedCapacity = 16;

                _creationChecklist.RemoveFromClassList("d-none");
                _creationChecklist.Items = new List<Checklist.ChecklistItem>
                {
                    new Checklist.ChecklistItem
                    {
                        Value = "name",
                        Label = "Give your world a name",
                        Checked = false
                    },
                    new Checklist.ChecklistItem
                    {
                        Value = "thumbnail",
                        Label = "Select a thumbnail image",
                        Checked = false
                    },
                    new Checklist.ChecklistItem
                    {
                        Value = "build",
                        Label = "Click \"Build & Upload\"",
                        Checked = false
                    }
                };

                ValidateChecklist();
                
                _visibilityFoldout.AddToClassList("d-none");
                _publishBlock.AddToClassList("d-none");
            }
            else
            {
                WorldBuilderSessionState.Clear();

                platformsBlock.parent.RemoveFromClassList("d-none");
                _visibilityFoldout.RemoveFromClassList("d-none");
                _creationChecklist.AddToClassList("d-none");
                
                _nameField.value = _worldData.Name;
                _descriptionField.value = _worldData.Description;
                _capacityField.value = _worldData.Capacity;
                _recommendedCapacityField.value = _worldData.RecommendedCapacity;
                // handle worlds without recommended capacity set
                if (_worldData.RecommendedCapacity == 0)
                {
                    _recommendedCapacityField.value = Mathf.FloorToInt(_worldData.Capacity / 2f);
                }

                var platforms = new HashSet<string>();
                foreach (var p in _worldData.UnityPackages.Select(p => VRCSdkControlPanel.CONTENT_PLATFORMS_MAP[p.Platform]))
                {
                    platforms.Add(p);
                }
                platformsBlock.text = string.Join(", ", platforms);

                _lastUpdatedLabel.text = (_worldData.UpdatedAt != DateTime.MinValue ? _worldData.UpdatedAt : _worldData.CreatedAt).ToLocalTime().ToString(CultureInfo.CurrentCulture);
                _lastUpdatedLabel.parent.RemoveFromClassList("d-none");
                
                _versionLabel.text = _worldData.Version.ToString();
                _versionLabel.parent.RemoveFromClassList("d-none");

                _worldDebuggingToggle.value = _worldData.Tags?.Contains("debug_allowed") ?? false;

                var isPrivate = _worldData.ReleaseStatus == "private";
                _visibilityLabel.text = isPrivate ? "Private" : "Public";
                _publishBlock.RemoveFromClassList("d-none");

                var shouldShowPublishToLabs = isPrivate && !(_worldData.Tags?.Any(t => COMMUNITY_LABS_BLOCKED_TAGS.Contains(t)) ?? false);
                var canPublish = false;
                try
                {
                    canPublish = await VRCApi.GetCanPublishWorld(_worldData.ID);
                }
                catch (ApiErrorException e)
                {
                    if (e.StatusCode != HttpStatusCode.Unauthorized)
                    {
                        Debug.LogError("Failed to get publish status: " + e.ErrorMessage);
                    }
                }

                var shouldShowCantPublish = shouldShowPublishToLabs && !canPublish;

                if (shouldShowCantPublish)
                {
                    _publishButton.SetEnabled(false);
                    _publishButton.text = "You can't publish worlds right now";
                } else if (shouldShowPublishToLabs)
                {
                    _publishButton.text = "Publish to Community Labs";
                    _publishButton.SetEnabled(true);
                }
                else
                {
                    _publishButton.text = "Unpublish";
                    _publishButton.SetEnabled(true);
                }
                communityLabsHelpButton.EnableInClassList("d-none", !isPrivate);

                _publishButton.clicked += HandlePublishClick;
            
                await _thumbnail.SetImageUrl(_worldData.ThumbnailImageUrl);
            }
            
           
            _nameField.Loading = false;
            _descriptionField.Loading = false;
            _thumbnail.Loading = false;
            UiEnabled = true;

            _originalWorldData = _worldData;
            _originalWorldData.Tags = new List<string>(_worldData.Tags ?? new List<string>());

            var worldTags = _worldData.Tags ?? new List<string>();
            _tagsField.TagFilter = tagList => tagList.Where(t =>
                (APIUser.CurrentUser?.hasSuperPowers ?? false) || t.StartsWith("author_tag_")).ToList();
            _tagsField.TagLimit = APIUser.CurrentUser?.hasSuperPowers ?? false ? 100 : 5; 
            _tagsField.FormatTagDisplay = input => input.Replace("author_tag_", "");
            _tagsField.tags = worldTags;
            _tagsField.OnAddTag += HandleAddTag;
            _tagsField.OnRemoveTag += HandleRemoveTag;

            _contentWarningsField.originalTags = _originalWorldData.Tags;
            _contentWarningsField.tags = worldTags;
            _contentWarningsField.OnToggleTag += HandleToggleTag;

            _nameField.RegisterValueChangedCallback(HandleNameChange);
            _descriptionField.RegisterValueChangedCallback(HandleDescriptionChange);
            _capacityField.RegisterValueChangedCallback(HandleCapacityChange);
            _recommendedCapacityField.RegisterValueChangedCallback(HandleRecommendedCapacityChange);
            _thumbnailFoldout.OnNewThumbnailSelected += HandleThumbnailChanged;
            _worldDebuggingToggle.RegisterValueChangedCallback(HandleWorldDebuggingChange);

            _discardChangesButton.clicked += HandleDiscardChangesClick;
            _saveChangesButton.clicked += HandleSaveChangesClick;

            root.schedule.Execute(CheckBlueprintChanges).Every(1000);
        }

        private void RestoreSessionState()
        {
            _worldData.Name = WorldBuilderSessionState.WorldName;
            _nameField.SetValueWithoutNotify(_worldData.Name);

            _worldData.Description = WorldBuilderSessionState.WorldDesc;
            _descriptionField.SetValueWithoutNotify(_worldData.Description);

            _worldData.Tags = new List<string>(WorldBuilderSessionState.WorldTags.Split(new [] { "|" }, StringSplitOptions.RemoveEmptyEntries));
            _tagsField.tags = _contentWarningsField.tags = _worldData.Tags;

            _worldData.Capacity = WorldBuilderSessionState.WorldCapacity;
            _capacityField.SetValueWithoutNotify(_worldData.Capacity);
            _recommendedCapacityField.SetValueWithoutNotify(WorldBuilderSessionState.WorldRecommendedCapacity);

            _worldDebuggingToggle.SetValueWithoutNotify(_worldData.Tags.Contains("debug_allowed"));

            _newThumbnailImagePath = WorldBuilderSessionState.WorldThumbPath;
            if (!string.IsNullOrWhiteSpace(_newThumbnailImagePath))
                _thumbnail.SetImage(_newThumbnailImagePath);
        }

        #region Event Handlers

        private void HandlePanelDetach(DetachFromPanelEvent evt)
        {
            EditorSceneManager.sceneClosed -= HandleSceneClosed;
        }

        // This auto-cancels uploads when the user changes scenes
        private void HandleSceneClosed(Scene scene)
        {
            if (_buildState == SdkBuildState.Building) return;
            if (_worldUploadCancellationTokenSource == null) return;
            _worldUploadCancellationTokenSource.Cancel();
            _worldUploadCancellationTokenSource = null;
        }

        private void HandleSdkPanelDisable(object sender, EventArgs evt)
        {
            if (_worldUploadCancellationTokenSource == null) return;
            _worldUploadCancellationTokenSource.Cancel();
            _worldUploadCancellationTokenSource = null;
        }

        private void HandleFoldoutToggle(ChangeEvent<bool> evt)
        {
            SessionState.SetBool($"{WorldBuilderSessionState.SESSION_STATE_PREFIX}.Foldout.{((VisualElement) evt.currentTarget).name}", evt.newValue);
        }
        
        private void HandleNameChange(ChangeEvent<string> evt)
        {
            _worldData.Name = evt.newValue;
            if (IsNewWorld)
                WorldBuilderSessionState.WorldName = _worldData.Name;

            // do not allow empty names
            _saveChangesButton.SetEnabled(!string.IsNullOrWhiteSpace(evt.newValue));
            IsContentInfoDirty = CheckDirty();

            ValidateChecklist();
        }

        private void HandleDescriptionChange(ChangeEvent<string> evt)
        {
            _worldData.Description = evt.newValue;
            if (IsNewWorld)
                WorldBuilderSessionState.WorldDesc = _worldData.Description;

            IsContentInfoDirty = CheckDirty();
        }

        private void HandleCapacityChange(ChangeEvent<int> evt)
        {
            int clampedValue = Mathf.Clamp(evt.newValue, 1, WORLD_MAX_CAPAITY);
            if (clampedValue != evt.newValue)
            {
                _capacityField.SetValueWithoutNotify(clampedValue);
            }

            _worldData.Capacity = clampedValue;
            if (IsNewWorld)
                WorldBuilderSessionState.WorldCapacity = _worldData.Capacity;

            IsContentInfoDirty = CheckDirty();
        }
        
        private void HandleRecommendedCapacityChange(ChangeEvent<int> evt)
        {
            int clampedValue = Mathf.Clamp(evt.newValue, 1, _worldData.Capacity);
            if (clampedValue != evt.newValue)
            {
                _recommendedCapacityField.SetValueWithoutNotify(clampedValue);
            }

            _worldData.RecommendedCapacity = clampedValue;
            if (IsNewWorld)
                WorldBuilderSessionState.WorldRecommendedCapacity = _worldData.RecommendedCapacity;

            IsContentInfoDirty = CheckDirty();
        }
        
        private void HandleWorldDebuggingChange(ChangeEvent<bool> evt)
        {
            if (_worldData.Tags == null)
            {
                _worldData.Tags = new List<string>();
            }

            if (evt.newValue)
            {
                if (!_worldData.Tags.Contains("debug_allowed"))
                {
                    _worldData.Tags.Add("debug_allowed");
                }
            }
            else
            {
                if (_worldData.Tags.Contains("debug_allowed"))
                {
                    _worldData.Tags.Remove("debug_allowed");
                }
            }

            if (IsNewWorld)
                WorldBuilderSessionState.WorldTags = string.Join("|", _worldData.Tags);

            IsContentInfoDirty = CheckDirty();
        }
        
        private void HandleSelectThumbnailClick()
        {
            var imagePath = EditorUtility.OpenFilePanel("Select thumbnail", "", "png");
            if (string.IsNullOrWhiteSpace(imagePath)) return;

            _newThumbnailImagePath = imagePath;
            if (IsNewWorld)
                WorldBuilderSessionState.WorldThumbPath = _newThumbnailImagePath;

            _thumbnail.SetImage(_newThumbnailImagePath);
            IsContentInfoDirty = CheckDirty();

            ValidateChecklist();
        }

        private void HandleAddTag(object sender, string tag)
        {
            if (_worldData.Tags == null)
                _worldData.Tags = new List<string>();

            var formattedTag = "author_tag_" + tag.ToLowerInvariant().Replace(' ', '_');
            if (_worldData.Tags.Contains(formattedTag)) return;
            
            _worldData.Tags.Add(formattedTag);
            _tagsField.tags = _contentWarningsField.tags = _worldData.Tags;

            if (IsNewWorld)
                WorldBuilderSessionState.WorldTags = string.Join("|", _worldData.Tags);

            IsContentInfoDirty = CheckDirty();
        }

        private void HandleRemoveTag(object sender, string tag)
        {
            if (_worldData.Tags == null)
                _worldData.Tags = new List<string>();

            if (!_worldData.Tags.Contains(tag))
                return;

            _worldData.Tags.Remove(tag);
            _tagsField.tags = _contentWarningsField.tags = _worldData.Tags;

            if (IsNewWorld)
                WorldBuilderSessionState.WorldTags = string.Join("|", _worldData.Tags);

            IsContentInfoDirty = CheckDirty();
        }

        private void HandleToggleTag(object sender, string tag)
        {
            if (_worldData.Tags == null)
                _worldData.Tags = new List<string>();

            if (_worldData.Tags.Contains(tag))
                _worldData.Tags.Remove(tag);
            else
                _worldData.Tags.Add(tag);

            _tagsField.tags = _contentWarningsField.tags = _worldData.Tags;

            if (IsNewWorld)
                WorldBuilderSessionState.WorldTags = string.Join("|", _worldData.Tags);

            IsContentInfoDirty = CheckDirty();
        }
        
        private void HandleThumbnailChanged(object sender, string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) return;
            
            _newThumbnailImagePath = imagePath;
            if (IsNewWorld)
                WorldBuilderSessionState.WorldThumbPath = _newThumbnailImagePath;
            
            _thumbnail.SetImage(_newThumbnailImagePath);
            IsContentInfoDirty = CheckDirty();

            ValidateChecklist();
        }
        
        private async void HandleDiscardChangesClick()
        {
            _worldData = _originalWorldData;
            _worldData.Tags = new List<string>(_originalWorldData.Tags);
            
            _nameField.value = _worldData.Name;
            _descriptionField.value = _worldData.Description;
            _tagsField.tags = _contentWarningsField.tags = _worldData.Tags;
            _lastUpdatedLabel.text = _worldData.UpdatedAt != DateTime.MinValue ? _worldData.UpdatedAt.ToString() : _worldData.CreatedAt.ToString();
            _versionLabel.text = _worldData.Version.ToString();
            _worldDebuggingToggle.value = _worldData.Tags?.Contains("debug_allowed") ?? false;
            _nameField.Reset();
            _descriptionField.Reset();
            _newThumbnailImagePath = null;
            await _thumbnail.SetImageUrl(_worldData.ThumbnailImageUrl);
            IsContentInfoDirty = false;
        }
        
        private async void HandleSaveChangesClick()
        {
            UiEnabled = false;

            if (_nameField.IsPlaceholder() || string.IsNullOrWhiteSpace(_nameField.text))
            {
                Debug.LogError("Name cannot be empty");
                return;
            }

            if (_descriptionField.IsPlaceholder())
            {
                _worldData.Description = "";
            }

            _worldUploadCancellationTokenSource = new CancellationTokenSource();
            _worldUploadCancellationToken = _worldUploadCancellationTokenSource.Token;
            
            if (!string.IsNullOrWhiteSpace(_newThumbnailImagePath))
            {
                _progressBar.style.width = 0f;

                // to avoid loss of exceptions, we hoist it into a local function
                async void Progress(string status, float percentage)
                {
                    // these callbacks can be dispatched off-thread, so we ensure we're main thread pinned
                    await UniTask.SwitchToMainThread();
                    ProgressBarState = new ProgressBarStateData
                    {
                        Visible = true,
                        Progress = percentage * 0.8f,
                        Text = status
                    };
                }
                
                _updateCancelButton.RemoveFromClassList("d-none");

                _newThumbnailImagePath = VRC_EditorTools.CropImage(_newThumbnailImagePath, 800, 600);
                var updatedWorld = await VRCApi.UpdateWorldImage(
                    _worldData.ID,
                    _worldData,
                    _newThumbnailImagePath,
                    Progress, _worldUploadCancellationToken);
                
                // also need to update the base world data
                if (!WorldDataEqual())
                {
                    ProgressBarState = new ProgressBarStateData
                    {
                        Visible = true,
                        Text = "Saving World Changes...",
                        Progress = 1f
                    };
                    updatedWorld = await VRCApi.UpdateWorldInfo(_worldData.ID, _worldData, _worldUploadCancellationToken);
                }
                _worldData = updatedWorld;
                _originalWorldData = updatedWorld;
                await _thumbnail.SetImageUrl(_worldData.ThumbnailImageUrl, _worldUploadCancellationToken);
                _contentWarningsField.originalTags = _originalWorldData.Tags = new List<string>(_worldData.Tags ?? new List<string>());
                _tagsField.tags = _contentWarningsField.tags = _worldData.Tags ?? new List<string>();
                _newThumbnailImagePath = null;
            }
            else
            {
                ProgressBarState = new ProgressBarStateData
                {
                    Visible = true,
                    Text = "Saving World Changes...",
                    Progress = 1f
                };
                var updatedWorld = await VRCApi.UpdateWorldInfo(_worldData.ID, _worldData, _worldUploadCancellationToken);

                var successLabel = new Label("Your world was successfully updated");
                successLabel.AddToClassList("p-2");
                successLabel.AddToClassList("text-center");
                
                _worldData = updatedWorld;
                _originalWorldData = updatedWorld;
                _contentWarningsField.originalTags = _originalWorldData.Tags = new List<string>(_worldData.Tags ?? new List<string>());
                _contentWarningsField.tags = _tagsField.tags = new List<string>(_worldData.Tags ?? new List<string>());

                await _builder.ShowBuilderNotification("World Updated", successLabel, "green", 3000);
            }
            
            _updateCancelButton.AddToClassList("d-none");

            ProgressBarState = false;


            UiEnabled = true;
            _nameField.value = _worldData.Name;
            _descriptionField.value = _worldData.Description;
            _lastUpdatedLabel.text = _worldData.UpdatedAt != DateTime.MinValue ? _worldData.UpdatedAt.ToString() : _worldData.CreatedAt.ToString();
            _versionLabel.text = _worldData.Version.ToString();
            _nameField.Reset();
            _descriptionField.Reset();
            IsContentInfoDirty = false;
        }
        
        private void HandlePublishClick()
        {
            if (_worldData.ReleaseStatus == "private")
            {
                PopupWindow.Show(_publishButton.worldBound, new PublishConfirmationWindow(
                    "Publish to Community Labs",
                    WorldBuilderConstants.PUBLISH_WORLD_COPY,
                    "Yes, do it!",
                    "No, keep the world private",
                    HandlePublishWorldConfirm
                ));
                return;
            }

            PopupWindow.Show(_publishButton.worldBound, new PublishConfirmationWindow(
                "Unpublishing a World",
                WorldBuilderConstants.UNPUBLISH_WORLD_COPY,
                "Yes, do it!",
                "No, keep the world public",
                HandleUnpublishWorldConfirm
            ));
        }

        private async void HandlePublishWorldConfirm()
        {
            try
            {
                await VRCApi.PublishWorld(_worldData.ID);
                await _builder.ShowBuilderNotification("Published world to Community Labs",
                    new GenericBuilderNotification("Your world is now visible to other players!", null,
                        "See it on the VRChat Website",
                        () => { Application.OpenURL($"https://vrchat.com/home/world/{_worldData.ID}"); }),
                    "green", 5000);
                CreateContentInfoGUI(_visualRoot);
            }
            catch (Exception e)
            {
                var message = e is ApiErrorException exception ? exception.ErrorMessage : e.Message;
                await _builder.ShowBuilderNotification("Failed to publish world",
                    new GenericBuilderNotification(
                        "Something went wrong when publishing your world to Community Labs", message),
                    "red", 5000);
                Core.Logger.LogError(e.Message, DebugLevel.API);
            }
        }

        private async void HandleUnpublishWorldConfirm()
        {
            try
            {
                await VRCApi.UnpublishWorld(_worldData.ID);
                await _builder.ShowBuilderNotification("Unpublished world",
                    new GenericBuilderNotification(
                        "Your world is private. You can still access it via direct links", null,
                        "See it on the VRChat Website",
                        () => { Application.OpenURL($"https://vrchat.com/home/world/{_worldData.ID}"); }),
                    "green", 5000);
                CreateContentInfoGUI(_visualRoot);
            }
            catch (Exception e)
            {
                var message = e is ApiErrorException exception ? exception.ErrorMessage : e.Message;
                await _builder.ShowBuilderNotification("Failed to unpublish world",
                    new GenericBuilderNotification("Something went wrong when unpublishing your world",
                        message), "red", 5000);
                Core.Logger.LogError(e.Message, DebugLevel.API);
            }
        }

        #endregion

        private void ValidateChecklist()
        {
            _creationChecklist.MarkItem("name", !string.IsNullOrWhiteSpace(_worldData.Name));
            _creationChecklist.MarkItem("thumbnail", !string.IsNullOrWhiteSpace(_newThumbnailImagePath));
        }

        private void SetThumbnailImage(string imagePath)
        {
            var bytes = File.ReadAllBytes(imagePath);
            var newThumbnail = new Texture2D(2, 2);
            newThumbnail.LoadImage(bytes);
            _thumbnail.SetImage(newThumbnail);
        }

        private bool WorldDataEqual()
        {
            return _worldData.Name.Equals(_originalWorldData.Name) &&
                   _worldData.Description.Equals(_originalWorldData.Description) &&
                   _worldData.Tags.SequenceEqual(_originalWorldData.Tags) &&
                   (_worldData.PreviewYoutubeId == _originalWorldData.PreviewYoutubeId) && // yt id can be null
                   _worldData.Capacity.Equals(_originalWorldData.Capacity) &&
                   _worldData.RecommendedCapacity.Equals(_originalWorldData.RecommendedCapacity);
        }

        private bool CheckDirty()
        {
            // we ignore the diffs for new worlds, since they're not published yet
            if (IsNewWorld) return false;
            if (string.IsNullOrWhiteSpace(_worldData.ID) || string.IsNullOrWhiteSpace(_originalWorldData.ID))
                return false;
            return !WorldDataEqual()|| !string.IsNullOrWhiteSpace(_newThumbnailImagePath);
        }

        private void CheckBlueprintChanges()
        {
            if (!UiEnabled) return;
            if (_pipelineManagers.Length == 0) return;
            var blueprintId = _pipelineManagers[0].blueprintId;
            if (_lastBlueprintId == blueprintId) return;
            CreateContentInfoGUI(_visualRoot);
            _lastBlueprintId = blueprintId;
        }

        private bool _acceptedTerms;
        public virtual void CreateBuildGUI(VisualElement root)
        {
            var tree = Resources.Load<VisualTreeAsset>("VRCSdkWorldBuilderBuildLayout");
            tree.CloneTree(root);
            var styles = Resources.Load<StyleSheet>("VRCSdkWorldBuilderBuildStyles");
            if (!root.styleSheets.Contains(styles))
            {
                root.styleSheets.Add(styles);
            }
            
            root.Q<Button>("show-local-test-help-button").clicked += () =>
            {
                root.Q("local-test-help-text").ToggleInClassList("d-none");
            };
            root.Q<Button>("show-online-publishing-help-button").clicked += () =>
            {
                root.Q("online-publishing-help-text").ToggleInClassList("d-none");
            };

            _testLastBuildButton = root.Q<Button>("test-last-build-button");
            _buildAndTestButton = root.Q<Button>("build-and-test-button");
            var localTestDisabledBlock = root.Q("local-test-disabled-block");
            var localTestDisabledText = root.Q<Label>("local-test-disabled-text");
            _acceptTermsToggle = root.Q<Toggle>("accept-terms-toggle");
            _v3Block = root.Q("v3-block");
            
            _acceptTermsToggle.RegisterValueChangedCallback(evt =>
            {
                _acceptedTerms = evt.newValue;
            });

            var numClientsField = root.Q<IntegerField>("num-clients");
            numClientsField.RegisterValueChangedCallback(evt =>
            {
                VRCSettings.NumClients = Mathf.Clamp(evt.newValue, 0, 8);
                (evt.target as IntegerField)?.SetValueWithoutNotify(VRCSettings.NumClients);
                if (VRCSettings.NumClients == 0)
                {
                    _testLastBuildButton.text = "Reload Last Build";
                    _buildAndTestButton.text = "Build & Reload";
                }
                else
                {
                    _testLastBuildButton.text = "Test Last Build";
                    _buildAndTestButton.text = "Build & Test New Build";
                }
            });
            numClientsField.SetValueWithoutNotify(VRCSettings.NumClients);

            var forceNonVrToggle = root.Q<Toggle>("force-non-vr");
            forceNonVrToggle.RegisterValueChangedCallback(evt =>
            {
                VRCSettings.ForceNoVR = evt.newValue;
            });
            forceNonVrToggle.SetValueWithoutNotify(VRCSettings.ForceNoVR);
            
            var enableWorldReloadToggle = root.Q<Toggle>("enable-world-reload");
            enableWorldReloadToggle.RegisterValueChangedCallback(evt =>
            {
                VRCSettings.WatchWorlds = evt.newValue;
            });
            enableWorldReloadToggle.SetValueWithoutNotify(VRCSettings.WatchWorlds);
            
            _testLastBuildButton.text = VRCSettings.NumClients == 0 ? "Reload Last Build" : "Test Last Build";
            _testLastBuildButton.clicked += async () =>
            {
                if (VRCSettings.NumClients == 0)
                {
                    // Todo: get this from settings or make key a const
                    string path = EditorPrefs.GetString("lastVRCPath");
                    if (File.Exists(path))
                    {
                        File.SetLastWriteTimeUtc(path, DateTime.Now);
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot find last built scene, please Rebuild.");
                    }
                }
                else
                {
                    await TestLastBuild();
                }
            };
            
#if UNITY_ANDROID || UNITY_IOS
            _testLastBuildButton.SetEnabled(false);
            _buildAndTestButton.SetEnabled(false);
            _buildAndTestButton.SetEnabled(false);
            localTestDisabledBlock.RemoveFromClassList("d-none");
            localTestDisabledText.text = "Building and testing on this platform is not supported.";
#endif

            _buildAndTestButton.text = VRCSettings.NumClients == 0 ? "Build & Reload" : "Build & Test New Build";
            _buildAndTestButton.clicked += async () =>
            {
                async void BuildSuccess(object sender, string path)
                {
                    ProgressBarState = new ProgressBarStateData
                    {
                        Visible = true,
                        Text = "World Built",
                        Progress = 1f
                    };
                    
                    await Task.Delay(500);

                    ProgressBarState = false;
                    UiEnabled = true;
                    _thumbnail.Loading = false;
                    RevertThumbnail();

                    ShowBuildSuccessNotification();
                }

                OnSdkBuildStart += BuildStart;
                OnSdkBuildError += BuildError;
                OnSdkBuildSuccess += BuildSuccess;

                try
                {
                    if (VRCSettings.NumClients == 0)
                    {
                        await Build();
                    }
                    else
                    {
                        await BuildAndTest();
                    }
                }
                finally
                {
                    OnSdkBuildStart -= BuildStart;
                    OnSdkBuildError -= BuildError;
                    OnSdkBuildSuccess -= BuildSuccess;
                }
                
            };
            
            _uploadLastBuildButton = root.Q<Button>("upload-last-build-button");
            _buildAndUploadButton = root.Q<Button>("build-and-upload-button");
            var uploadDisabledBlock = root.Q<VisualElement>("build-and-upload-disabled-block");
            var uploadDisabledText = root.Q<Label>("build-and-upload-disabled-text");

            _uploadLastBuildButton.clicked += async () =>
            {
                UiEnabled = false;

                OnSdkUploadStart += UploadStart;
                OnSdkUploadProgress += UploadProgress;
                OnSdkUploadError += UploadError;
                OnSdkUploadSuccess += UploadSuccess;
                OnSdkUploadFinish += UploadFinish;

                _worldUploadCancellationTokenSource = new CancellationTokenSource();

                try
                {
                    await UploadLastBuild(_worldData, _newThumbnailImagePath,
                        _worldUploadCancellationTokenSource.Token);
                }
                finally
                {
                	OnSdkUploadStart -= UploadStart;
                    OnSdkUploadProgress -= UploadProgress;
                    OnSdkUploadError -= UploadError;
                    OnSdkUploadSuccess -= UploadSuccess;
                    OnSdkUploadFinish -= UploadFinish;
                }
                
            };

            _buildAndUploadButton.clicked += async () =>
            {
                UiEnabled = false;

                void BuildSuccess(object sender, string path)
                {
                    ProgressBarState = new ProgressBarStateData
                    {
                        Visible = true,
                        Text = "World Built",
                        Progress = 0.1f,
                    };
                }

                OnSdkBuildStart += BuildStart;
                OnSdkBuildError += BuildError;
                OnSdkBuildSuccess += BuildSuccess;

                OnSdkUploadStart += UploadStart;
                OnSdkUploadProgress += UploadProgress;
                OnSdkUploadError += UploadError;
                OnSdkUploadSuccess += UploadSuccess;
                OnSdkUploadFinish += UploadFinish;

                _worldUploadCancellationTokenSource = new CancellationTokenSource();

                try
                {
                    await BuildAndUpload(_worldData, _newThumbnailImagePath, _worldUploadCancellationTokenSource.Token);
                }
                finally
                {
                    
                    OnSdkBuildStart -= BuildStart;
                    OnSdkBuildError -= BuildError;
                    OnSdkBuildSuccess -= BuildSuccess;

                    OnSdkUploadStart -= UploadStart;
                    OnSdkUploadProgress -= UploadProgress;
                    OnSdkUploadError -= UploadError;
                    OnSdkUploadSuccess -= UploadSuccess;
                    OnSdkUploadFinish -= UploadFinish;
                }
            };

            SetupExtraPanelUI();

            root.schedule.Execute(() =>
            {
                var buildsAllowed = _builder.NoGuiErrorsOrIssues() || APIUser.CurrentUser.developerType == APIUser.DeveloperType.Internal;
                var localBuildsAllowed = (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64) && buildsAllowed;

                localTestDisabledBlock.EnableInClassList("d-none", localBuildsAllowed);
                uploadDisabledBlock.EnableInClassList("d-none", buildsAllowed);
                
                if (localBuildsAllowed)
                {
                    localTestDisabledText.text =
                        "You must fix the issues listed above before you can do an Offline Test";
                }
                
                if (!_acceptedTerms)
                {
                    uploadDisabledText.text = "You must accept the terms above to upload content to VRChat";
                    uploadDisabledBlock.RemoveFromClassList("d-none");
                    return;
                }
                else
                {
                    uploadDisabledBlock.AddToClassList("d-none");
                }
                
                var lastBuildUrl = VRC_SdkBuilder.GetLastUrl();
                
                if (IsNewWorld && (string.IsNullOrWhiteSpace(_worldData.Name) || string.IsNullOrWhiteSpace(_newThumbnailImagePath)))
                {
                    uploadDisabledText.text = "Please set a name and thumbnail before uploading";
                    uploadDisabledBlock.RemoveFromClassList("d-none");
                    return;
                }
                else
                {
                    uploadDisabledText.text = "You must fix the issues listed above before you can Upload a Build";
                }

                if (!UiEnabled) return;
                _testLastBuildButton.SetEnabled(lastBuildUrl != null);
                _testLastBuildButton.tooltip = lastBuildUrl != null ? "" : "No last build found";
                _uploadLastBuildButton.SetEnabled(lastBuildUrl != null);
                _uploadLastBuildButton.tooltip = lastBuildUrl != null ? "" : "No last build found";
            }).Every(1000);
        }

        public virtual void SetupExtraPanelUI()
        {
            
        }

        private async Task<string> Build(bool runAfterBuild)
        {
            var buildBlocked = !VRCBuildPipelineCallbacks.OnVRCSDKBuildRequested(VRCSDKRequestedBuildType.Scene);
            if (buildBlocked)
            {
                throw await HandleBuildError(new BuildBlockedException("Build was blocked by the SDK callback"));
            }
            
            if (!APIUser.CurrentUser.canPublishWorlds)
            {
                VRCSdkControlPanel.ShowContentPublishPermissionsDialog();
                throw await HandleBuildError(new BuildBlockedException("Current User does not have permissions to build and upload worlds"));
            }
            
            if (_builder == null || _scenes == null || _scenes.Length == 0)
            {
                throw await HandleBuildError(new BuilderException("Open the SDK panel to build and upload worlds"));
            }

            _builder.CheckedForIssues = false;
            _builder.ResetIssues();
            OnGUISceneCheck(_scenes[0]);
            _builder.CheckedForIssues = true;
            if (!_builder.NoGuiErrorsOrIssuesForItem(_scenes[0]) || !_builder.NoGuiErrorsOrIssuesForItem(_builder))
            {
                var errorsList = new List<string>();
                errorsList.AddRange(_builder.GetGuiErrorsOrIssuesForItem(_scenes[0]).Select(i => i.issueText));
                errorsList.AddRange(_builder.GetGuiErrorsOrIssuesForItem(_builder).Select(i => i.issueText));
                throw await HandleBuildError(new ValidationException("World validation failed", errorsList));
            }

            EnvConfig.ConfigurePlayerSettings();
            EditorPrefs.SetBool("VRC.SDKBase_StripAllShaders", false);
            AssetExporter.CleanupUnityPackageExport(); // force unity package rebuild on next publish
            VRC_SdkBuilder.shouldBuildUnityPackage = false;
            VRC_SdkBuilder.PreBuildBehaviourPackaging();
            
            VRC_SdkBuilder.ClearCallbacks();
            
            var successTask = new TaskCompletionSource<string>();
            var errorTask = new TaskCompletionSource<string>();
            VRC_SdkBuilder.RegisterBuildProgressCallback((sender, status) =>
            {
                OnSdkBuildProgress?.Invoke(sender, status);
            });
            VRC_SdkBuilder.RegisterBuildErrorCallback((sender, error) =>
            {
                errorTask.TrySetResult(error);
            });
            VRC_SdkBuilder.RegisterBuildSuccessCallback((sender, path) =>
            {
                successTask.TrySetResult(path);
            });
            
            VRC_EditorTools.GetSetPanelBuildingMethod().Invoke(_builder, null);
            OnSdkBuildStart?.Invoke(this, EventArgs.Empty);
            _buildState = SdkBuildState.Building;
            OnSdkBuildStateChange?.Invoke(this, _buildState);
            
            await Task.Delay(100);
            
            if (runAfterBuild && Tools.Platform != "standalonewindows")
            {
                throw new BuilderException("World testing is only supported on Windows");
            }

            if (!runAfterBuild)
            {
                VRC_SdkBuilder.RunExportSceneResource();
            }
            else
            {
                VRC_SdkBuilder.RunExportSceneResourceAndRun();
            }
            
            var result = await Task.WhenAny(successTask.Task, errorTask.Task);

            string bundlePath = null;
            bundlePath = result == successTask.Task ? successTask.Task.Result : null;
            
            VRC_SdkBuilder.ClearCallbacks();

            if (bundlePath == null)
            {
                throw await HandleBuildError(new BuilderException(errorTask.Task.Result));
            }

            _buildState = SdkBuildState.Success;
            OnSdkBuildSuccess?.Invoke(this, bundlePath);
            OnSdkBuildStateChange?.Invoke(this, _buildState);

            await FinishBuild();

            PathToLastBuild = bundlePath;

            return bundlePath;
        }
        
        private async Task FinishBuild()
        {
            await Task.Delay(100);
            _buildState = SdkBuildState.Idle;
            OnSdkBuildFinish?.Invoke(this, "World build finished");
            OnSdkBuildStateChange?.Invoke(this, _buildState);
            VRC_EditorTools.GetSetPanelIdleMethod().Invoke(_builder, null);
        }
        
        private async Task<Exception> HandleBuildError(Exception exception)
        {
            OnSdkBuildError?.Invoke(this, exception.Message);
            _buildState = SdkBuildState.Failure;
            OnSdkBuildStateChange?.Invoke(this, _buildState);

            await FinishBuild();
            return exception;
        }
        
        private async Task Upload(VRCWorld world, string bundlePath, string thumbnailPath = null,
            CancellationToken cancellationToken = default)
        {
            if (cancellationToken == default)
            {
               _worldUploadCancellationTokenSource = new CancellationTokenSource();
               _worldUploadCancellationToken = _worldUploadCancellationTokenSource.Token;
            }
            else
            {
               _worldUploadCancellationToken = cancellationToken;
            }

            if (string.IsNullOrWhiteSpace(bundlePath) || !File.Exists(bundlePath))
            {
                throw await HandleUploadError(new UploadException("Failed to find the built world bundle, the build likely failed"));
            }

            if (!APIUser.CurrentUser.canPublishWorlds)
            {
                VRCSdkControlPanel.ShowContentPublishPermissionsDialog();
                throw await HandleUploadError(new BuildBlockedException("Current User does not have permissions to build and upload worlds"));
            }

            bool mobile = ValidationEditorHelpers.IsMobilePlatform();
            if (ValidationEditorHelpers.CheckIfAssetBundleFileTooLarge(ContentType.World, bundlePath, out int fileSize, mobile))
            {
                var limit = ValidationHelpers.GetAssetBundleSizeLimit(ContentType.World, mobile);
                throw await HandleUploadError(new UploadException(
                    $"World download size is too large for the target platform. {ValidationHelpers.FormatFileSize(fileSize)} > {ValidationHelpers.FormatFileSize(limit)}"));
            }
            
            if (ValidationEditorHelpers.CheckIfUncompressedAssetBundleFileTooLarge(ContentType.World, out int fileSizeUncompressed, mobile))
            {
                var limit = ValidationHelpers.GetAssetBundleSizeLimit(ContentType.World, mobile, false);
                throw await HandleUploadError(new UploadException(
                    $"World uncompressed size is too large for the target platform. {ValidationHelpers.FormatFileSize(fileSizeUncompressed)} > {ValidationHelpers.FormatFileSize(limit)}"));
            }

            VRC_EditorTools.GetSetPanelUploadingMethod().Invoke(_builder, null);
            _uploadState = SdkUploadState.Uploading;
            OnSdkUploadStateChange?.Invoke(this, _uploadState);
            OnSdkUploadStart?.Invoke(this, EventArgs.Empty);

            try
            {
                await Task.Delay(100, _worldUploadCancellationToken);
            }
            catch (TaskCanceledException)
            {
                throw await HandleUploadError(new UploadException("Upload Was Canceled"));
            }
            
            var pms = Tools.FindSceneObjectsOfTypeAll<PipelineManager>();
            if (pms.Length == 0)
            {
                throw await HandleUploadError(new UploadException("The scene does not have a PipelineManager component present, make sure to add a SceneDescriptor before building and uploading"));
            }

            var pM = pms[0];

            var creatingNewWorld = string.IsNullOrWhiteSpace(pM.blueprintId) || string.IsNullOrWhiteSpace(world.ID);

            if (creatingNewWorld && (string.IsNullOrWhiteSpace(thumbnailPath) || !File.Exists(thumbnailPath)))
            {
                throw await HandleUploadError(new UploadException("You must provide a path to the thumbnail image when creating a new world"));
            }

            if (!creatingNewWorld)
            {
                var remoteData = await VRCApi.GetWorld(world.ID, cancellationToken: _worldUploadCancellationToken);
                if (APIUser.CurrentUser == null || remoteData.AuthorId != APIUser.CurrentUser?.id)
                {
                    throw await HandleUploadError(new OwnershipException("World's current ID belongs to a different user, assign a different ID"));
                }
            }

            if (string.IsNullOrWhiteSpace(pM.blueprintId))
            {
                Undo.RecordObject(pM, "Assigning a new ID");
                pM.AssignId();
            }

            try
            {
                if (creatingNewWorld)
                {
                    thumbnailPath = VRC_EditorTools.CropImage(thumbnailPath, 800, 600);
                    _worldData = await VRCApi.CreateNewWorld(pM.blueprintId, world, bundlePath,
                        thumbnailPath,
                        (status, percentage) => { OnSdkUploadProgress?.Invoke(this, (status, percentage)); },
                        _worldUploadCancellationToken);
                }
                else
                {
                    _worldData = await VRCApi.UpdateWorldBundle(pM.blueprintId, world, bundlePath,
                        (status, percentage) => { OnSdkUploadProgress?.Invoke(this, (status, percentage)); },
                        _worldUploadCancellationToken);
                }
                
                _uploadState = SdkUploadState.Success;
                OnSdkUploadSuccess?.Invoke(this, _worldData.ID);

                await FinishUpload();
            }
            catch (TaskCanceledException e)
            {
                AnalyticsSDK.WorldUploadFailed(pM.blueprintId, !creatingNewWorld);
                if (cancellationToken.IsCancellationRequested)
                {
                    Core.Logger.LogError("Request cancelled", DebugLevel.API);
                    throw await HandleUploadError(new UploadException("Request Cancelled", e));
                }
            }
            catch (ApiErrorException e)
            {
                AnalyticsSDK.WorldUploadFailed(pM.blueprintId, !creatingNewWorld);
                throw await HandleUploadError(new UploadException(e.ErrorMessage, e));
            }
            catch (Exception e)
            {
                AnalyticsSDK.WorldUploadFailed(pM.blueprintId, !creatingNewWorld);
                throw await HandleUploadError(new UploadException(e.Message, e));
            }
        }
        
        private async Task FinishUpload()
        {
            await Task.Delay(100, _worldUploadCancellationToken);
            _uploadState = SdkUploadState.Idle;
            OnSdkUploadFinish?.Invoke(this, "World upload finished");
            OnSdkUploadStateChange?.Invoke(this, _uploadState);
            VRC_EditorTools.GetSetPanelIdleMethod().Invoke(_builder, null);
            VRC_EditorTools.ToggleSdkTabsEnabled(_builder, true);
            _worldUploadCancellationToken = default;
        }
        
        private async Task<Exception> HandleUploadError(Exception exception)
        {
            OnSdkUploadError?.Invoke(this, exception.Message);
            _uploadState = SdkUploadState.Failure;
            OnSdkUploadStateChange?.Invoke(this, _uploadState);

            await FinishUpload();
            return exception;
        }

        

        #region Build Callbacks
        
        private async void BuildStart(object sender, object target)
        {
            UiEnabled = false;
            _thumbnail.Loading = true;
            _thumbnail.ClearImage();
            
            if (IsNewWorld)
            {
                _creationChecklist.MarkItem("build", true);
                await Task.Delay(100);
            }
            
            ProgressBarState = new ProgressBarStateData
            {
                Visible = true,
                Text = "Building World",
                Progress = 0.0f
            };
        }
        private async void BuildError(object sender, string error)
        {
            Core.Logger.Log("Failed to build a world!");
            Core.Logger.LogError(error);


            await Task.Delay(100);
            ProgressBarState = false;
            UiEnabled = true;
            _thumbnail.Loading = false;
            RevertThumbnail();
            
            await _builder.ShowBuilderNotification(
                "Build Failed",
                new WorldUploadErrorNotification(error),
                "red"
            );
        }

        private async void RevertThumbnail()
        {
            if (IsNewWorld)
            {
                _thumbnail.SetImage(_newThumbnailImagePath);
            }
            else
            {
                await _thumbnail.SetImageUrl(_worldData.ThumbnailImageUrl);
            }
        }

        private void UploadStart(object sender, EventArgs e)
        {
            _thumbnail.ClearImage();
            _thumbnail.Loading = true;
            UpdateCancelEnabled = true;
            _updateCancelButton.clicked += CancelUpload;
            VRC_EditorTools.ToggleSdkTabsEnabled(_builder, false);
        }

        private async void UploadProgress(object sender, (string status, float percentage) progress)
        {
            await UniTask.SwitchToMainThread();
            ProgressBarState = new ProgressBarStateData
            {
                Visible = true,
                Text = progress.status,
                Progress = 0.2f + progress.percentage * 0.8f
            };
            _progressBlock.MarkDirtyRepaint();
        }
        private async void UploadSuccess(object sender, string worldId)
        {
            await Task.Delay(100);
            UpdateCancelEnabled = false;
            ProgressBarState = false;
            UiEnabled = true;

            _originalWorldData = _worldData;
            _originalWorldData.Tags = new List<string>(_worldData.Tags ?? new List<string>());
            _newThumbnailImagePath = null;

            await _builder.ShowBuilderNotification(
                "Upload Succeeded!",
                new WorldUploadSuccessNotification(worldId),
                "green"
            );
            
            CreateContentInfoGUI(_visualRoot);
        }
        private async void UploadError(object sender, string error)
        {
            Core.Logger.Log("Failed to upload a world!");
            Core.Logger.LogError(error);
            
            await Task.Delay(100);
            UpdateCancelEnabled = false;
            ProgressBarState = false;
            UiEnabled = true;
            _thumbnail.Loading = false;
            RevertThumbnail();
            
            await _builder.ShowBuilderNotification(
                "Upload Failed",
                new WorldUploadErrorNotification(error),
                "red"
            );
        }
        
        private void UploadFinish(object sender, string message)
        {
            _updateCancelButton.clicked -= CancelUpload;
        }

        private async void ShowBuildSuccessNotification()
        {
            await _builder.ShowBuilderNotification(
                "Build Succeeded!",
                new WorldBuildSuccessNotification(),
                "green"
            );
        }

        #endregion

        #endregion

        #region Public API Backing

        private SdkBuildState _buildState;
        private SdkUploadState _uploadState;

        private static CancellationTokenSource _worldUploadCancellationTokenSource;
        private CancellationToken _worldUploadCancellationToken;
        
        private static string PathToLastBuild
        {
            get => SessionState.GetString("VRC.SDK3.Editor_patToLastBuild", null);
            set => SessionState.SetString("VRC.SDK3.Editor_patToLastBuild", value);
        }

        #endregion

        #region Public API
        
        public event EventHandler<object> OnSdkBuildStart;
        public event EventHandler<string> OnSdkBuildProgress;
        public event EventHandler<string> OnSdkBuildFinish;
        public event EventHandler<string> OnSdkBuildSuccess;
        public event EventHandler<string> OnSdkBuildError;
        public event EventHandler<SdkBuildState> OnSdkBuildStateChange;
        public SdkBuildState BuildState => _buildState;
        
        public event EventHandler OnSdkUploadStart;
        public event EventHandler<(string status, float percentage)> OnSdkUploadProgress;
        public event EventHandler<string> OnSdkUploadFinish;
        public event EventHandler<string> OnSdkUploadSuccess;
        public event EventHandler<string> OnSdkUploadError;
        public event EventHandler<SdkUploadState> OnSdkUploadStateChange;
        public SdkUploadState UploadState => _uploadState;
        
        public async Task<string> Build()
        {
            return await Build(false);
        }

        public async Task BuildAndUpload(VRCWorld world, string thumbnailPath = null,
            CancellationToken cancellationToken = default)
        {
            var bundlePath = await Build();
            world.UdonProducts = _scenes[0].udonProducts;
            await Upload(world, bundlePath, thumbnailPath, cancellationToken);
        }

        public async Task UploadLastBuild(VRCWorld world, string thumbnailPath = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(PathToLastBuild))
            {
                OnSdkUploadError?.Invoke(this, "No last build found, you must build first");
                _uploadState = SdkUploadState.Failure;
                OnSdkUploadStateChange?.Invoke(this, _uploadState);
                await FinishUpload();
                throw new UploadException("No last build found, you must build first");
            }

            if (!File.Exists(PathToLastBuild))
            {
                PathToLastBuild = null;
                OnSdkUploadError?.Invoke(this, "No last build found, you must build first");
                _uploadState = SdkUploadState.Failure;
                OnSdkUploadStateChange?.Invoke(this, _uploadState);
                await FinishUpload();
                throw new UploadException("No last build found, you must build first");
            }

            await Upload(world, PathToLastBuild, thumbnailPath, cancellationToken);
        }

        public async Task BuildAndTest()
        {
            await Build(true);
        }

        public Task TestLastBuild()
        {
            if (string.IsNullOrWhiteSpace(PathToLastBuild))
            {
                Core.Logger.LogError("No last build found, you must build first");
                return Task.CompletedTask;
            }

            if (!File.Exists(PathToLastBuild))
            {
                PathToLastBuild = null;
                Core.Logger.LogError("No last build found, you must build first");
                return Task.CompletedTask;
            }
            VRC_SdkBuilder.RunLastExportedSceneResource();
            return Task.CompletedTask;
        }

        public void CancelUpload()
        {
            VRC_EditorTools.GetSetPanelIdleMethod().Invoke(_builder, null);
            if (_worldUploadCancellationToken != default)
            {
                _worldUploadCancellationTokenSource.Cancel();
                Core.Logger.Log("World upload canceled");
                return;
            }
            
            Core.Logger.LogError("Custom cancellation token passed, you should cancel via its token source instead");
        }

        #endregion
    }
}
