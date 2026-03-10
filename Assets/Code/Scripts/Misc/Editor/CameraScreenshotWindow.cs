using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// Takes a screenshot of a <see cref="Camera"/> and saves it to either a PNG or JPG.
/// <summary>
public class CameraScreenshotWindow : EditorWindow
{
    public static string FOLDER = Path.Combine(Application.dataPath.Remove(Application.dataPath.Length - 7), "Screenshots");
    public const int CAMERA_PREVIEW_PIXEL_LIMIT = 2000;
    public const float CAMERA_PREVIEW_MAX_HEIGHT = 200;
    private static GUIStyle _screenshotButtonStyle = null;
    private static GUIStyle _pathLabelStyle = null;

    public Camera CurrentCamera
    {
        get
        {
            switch (mode)
            {
                case Mode.sceneView:

                    if (_sceneViewCamera == null && HasOpenInstances<SceneView>())
                    {
                        _sceneView = null;

                        foreach (SceneView sceneViewIterator in Resources.FindObjectsOfTypeAll<SceneView>())
                        {
                            if (sceneViewIterator.hasFocus)
                            {
                                _sceneView = sceneViewIterator;
                                break;
                            }
                        }

                        if (_sceneView != null)
                        {
                            _sceneViewCamera = _sceneView.camera;
                        }
                    }

                    return _sceneViewCamera;

                default:
                    return camera;
            }
        }
    }
    private SceneView _sceneView;

    public static UniversalRenderPipelineAsset PipelineAsset
    {
        get
        {
            if (_pipelineAsset == null)
            {
                foreach (string guid in AssetDatabase.FindAssets($"t: {nameof(UniversalRenderPipelineAsset)}"))
                {
                    _pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
                    break;
                }
            }

            return _pipelineAsset;
        }
    }
    private static UniversalRenderPipelineAsset _pipelineAsset;

    [SerializeField] private bool initialized;
    [SerializeField] private Mode mode;
    [SerializeField] private Camera camera;
    private Camera _sceneViewCamera;
    [SerializeField] private Vector2Int resolution = new Vector2Int(1920, 1080);
    private Vector2Int? _oldResolution = null;
    [SerializeField] private bool lockAspectRatio;
    [SerializeField] private int renderTextureDepth = 24;
    [SerializeField] private TextureFormat textureFormat = TextureFormat.RGB24;
    [SerializeField] private bool transparentBackground;
    [SerializeField] private string fileName;
    [SerializeField] private ImageFormat imageFormat;
    [SerializeField] private bool renderCameraPreview = false;
    [SerializeField] private float cameraPreviewScale = 1;
    [SerializeField] private float timeBtwCameraPreviewUpdate = 0.5f;

    private RenderTexture _cameraPreviewRenderTexture;
    private Texture2D _cameraPreview;

    private Vector2 _scrollPosition;

    private float _cameraPreviewUpdateTimer;
    private double? _timeSinceStartup = null;

    [MenuItem("Tools/Open Camera Screenshot Tool")]
    public static void Open()
    {
        CameraScreenshotWindow window = GetWindow<CameraScreenshotWindow>();

        window.titleContent = new GUIContent("Camera Screenshot Tool", EditorGUIUtility.IconContent("d_SceneViewCamera").image);
    }

    private void OnEnable()
    {
        ResetCameraPreview();

        _timeSinceStartup = null;
    }

    private void OnDisable()
    {
        _timeSinceStartup = null;
    }

    private void OnFocus()
    {
        ResetCameraPreview();
    }

    private void Update()
    {
        float deltaTime = _timeSinceStartup.HasValue ? (float)(EditorApplication.timeSinceStartup - _timeSinceStartup) : 0;

        _timeSinceStartup = EditorApplication.timeSinceStartup;

        if (CurrentCamera == null || !renderCameraPreview || !EditorApplication.isFocused || timeBtwCameraPreviewUpdate < 0)
        {
            _cameraPreviewUpdateTimer = 0;
            return;
        }

        _cameraPreviewUpdateTimer += deltaTime;

        if (_cameraPreviewUpdateTimer <= timeBtwCameraPreviewUpdate)
        {
            return;
        }

        _cameraPreviewUpdateTimer = 0;

        UpdateCameraPreview();
        Repaint();
    }

    private void ResetCameraPreview()
    {
        DestroyImmediate(_cameraPreviewRenderTexture);
        DestroyImmediate(_cameraPreview);

        Vector2Int resolution = CameraPreviewResolution();

        CreateCameraPreviewRenderTexture(resolution);
        CreateCameraPreviewTexture(resolution);

        UpdateCameraPreview();
        RenderCameraPreview();
    }


    private void AutoSetCamera()
    {
        camera = Camera.current;

        if (camera == null)
        {
            camera = Camera.main;
        }

        if (camera == null)
        {
            camera = FindAnyObjectByType<Camera>();
        }
    }

    private void OnGUI()
    {
        if (!initialized)
        {
            AutoSetCamera();
            fileName = GetAutoScreenshotName();

            initialized = true;

            ClampResolution();
            UpdateCameraPreview();
            RenderCameraPreview();
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        Header("Camera");

        mode = (Mode)EditorGUILayout.EnumPopup("Mode", mode);

        using (new EditorGUI.DisabledScope(mode != Mode.customCamera))
        {
            EditorGUILayout.BeginHorizontal();

            if (GUI.enabled)
            {
                camera = EditorGUILayout.ObjectField("Camera", camera, typeof(Camera), true) as Camera;
            }
            else
            {
                EditorGUILayout.ObjectField("Camera", CurrentCamera, typeof(Camera), false);
            }

            if (GUILayout.Button("Auto", GUILayout.Width(50)))
            {
                AutoSetCamera();
            }

            EditorGUILayout.EndHorizontal();
        }

        if (mode == Mode.sceneView)
        {
            using (new EditorGUI.DisabledScope(_sceneViewCamera == null || _sceneView == null))
            {
                if (GUILayout.Button("Create Camera GameObject from SceneCamera"))
                {
                    mode = Mode.customCamera;

                    GameObject obj = new GameObject("SceneCamera Copy");

                    camera = obj.AddComponent<Camera>();
                    CopyComponent(_sceneViewCamera, camera);

                    Transform transform = obj.transform;

                    transform.position = _sceneView.pivot;
                    transform.rotation = _sceneView.rotation;
                    transform.position -= transform.forward * _sceneView.cameraDistance;

                    void CopyComponent<T>(T from, T to) where T : Component
                    {
                        FieldInfo[] fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        foreach (FieldInfo field in fields)
                        {
                            field.SetValue(to, field.GetValue(from));
                        }

                        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        foreach (PropertyInfo property in properties)
                        {
                            if (property.GetGetMethod() == null || property.GetSetMethod() == null)
                            {
                                continue;
                            }

                            switch (property.Name.ToLower().Trim())
                            {
                                case "cameratype":
                                case "hideflags":
                                case "targettexture":
                                case "rect":
                                case "pixelrect":
                                case "name":
                                    continue;
                            }

                            Debug.Log(property.Name + " | " + property.DeclaringType + " | " + property.GetValue(from));

                            property.SetValue(to, property.GetValue(from));
                        }
                    }

                    Selection.activeGameObject = obj;

                    Undo.RegisterCreatedObjectUndo(obj, "Created Camera GameObject from SceneCamera");
                }
            }
        }

        Header("Output");

        EditorGUILayout.BeginHorizontal();

        GUIContent resolutionLabel = new GUIContent("Resolution");
        resolution = EditorGUILayout.Vector2IntField(resolutionLabel, resolution);

        ClampResolution();

        // Lock Aspect Ratio Button
        Rect rect = EditorGUILayout.GetControlRect(GUILayout.Width(20));

        float height = EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector2Int, resolutionLabel);
        rect.y += height - rect.height;

        lockAspectRatio = GUI.Toggle(rect, lockAspectRatio, new GUIContent("", "Lock Aspect Ratio"), "button");

        Vector2 difference = rect.size;
        rect.width *= 0.65f;
        rect.height *= 0.65f;
        difference -= rect.size;

        rect.position += difference / 2;

        GUI.DrawTexture(rect, EditorGUIUtility.IconContent(lockAspectRatio ? "LockIcon-On" : "LockIcon").image);

        EditorGUILayout.EndHorizontal();

        if (!_oldResolution.HasValue)
        {
            _oldResolution = resolution;
        }

        if (lockAspectRatio)
        {
            if (!EditorGUIUtility.editingTextField)
            {
                Vector2Int oldResolution = _oldResolution.Value;

                if (resolution.x != oldResolution.x)
                {
                    float y = resolution.y;

                    y *= (float)resolution.x / (float)oldResolution.x;

                    resolution.y = Mathf.RoundToInt(y);
                }
                else if (resolution.y != oldResolution.y)
                {
                    float x = resolution.x;

                    x *= (float)resolution.y / (float)oldResolution.y;

                    resolution.x = Mathf.RoundToInt(x);
                }

                ClampResolution();

                _oldResolution = resolution;
            }
        }
        else
        {
            _oldResolution = resolution;
        }

        renderTextureDepth = EditorGUILayout.IntField("Render Texture Depth", renderTextureDepth);

        EditorGUILayout.BeginHorizontal();

        textureFormat = (TextureFormat)EditorGUILayout.EnumPopup("Texture Format", textureFormat);

        if (GUILayout.Button("Reset", GUILayout.Width(60)))
        {
            textureFormat = TextureFormat.RGB24;
        }

        EditorGUILayout.EndHorizontal();

        transparentBackground = EditorGUILayout.Toggle("Transparent Background", transparentBackground);

        if (transparentBackground)
        {
            EditorGUILayout.HelpBox("Note that Transparent Background may cause artifacts to appear if the camera has Post Processing enabled.", MessageType.Info, true);
        }

        Header("File Settings");

        EditorGUILayout.BeginHorizontal();

        fileName = EditorGUILayout.TextField("File Name", fileName);

        if (GUILayout.Button("Auto", GUILayout.Width(40)))
        {
            fileName = GetAutoScreenshotName();
        }

        EditorGUILayout.EndHorizontal();
        imageFormat = (ImageFormat)EditorGUILayout.EnumPopup("Image Format", imageFormat);

        string path = Path.Combine(FOLDER, fileName + "." + imageFormat.ToString());

        if (_pathLabelStyle == null)
        {
            _pathLabelStyle = new GUIStyle(EditorStyles.label);

            Color color = _pathLabelStyle.normal.textColor;
            color.a = 0.5f;
            _pathLabelStyle.normal.textColor = color;

            _pathLabelStyle.fontStyle = FontStyle.Italic;
        }

        EditorGUILayout.LabelField(path.Replace('\\', '/'), _pathLabelStyle);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(CurrentCamera == null))
        {
            if (_screenshotButtonStyle == null)
            {
                _screenshotButtonStyle = new GUIStyle("button");
                _screenshotButtonStyle.fontStyle = FontStyle.Bold;
                _screenshotButtonStyle.fontSize = Mathf.RoundToInt((float)_screenshotButtonStyle.fontSize * 1.5f);
            }

            GUIContent screenshotLabel = EditorGUIUtility.IconContent("SceneViewCamera@2x");
            screenshotLabel.text = " Screenshot!";

            if (GUILayout.Button(screenshotLabel, _screenshotButtonStyle) && (!File.Exists(path) || EditorUtility.DisplayDialog("Replace file at path?", $"There already exists an image with the name \"{fileName}\". \nReplace the file?", "Replace")))
            {
                Screenshot(path);
            }
        }

        if (GUILayout.Button("Open Screenshot Folder"))
        {
            CreateFolderIfNotPresent();

            EditorUtility.RevealInFinder(FOLDER);
        }

        EditorGUILayout.Space();

        bool oldRenderCameraPreview = renderCameraPreview;
        renderCameraPreview = EditorGUILayout.BeginFoldoutHeaderGroup(renderCameraPreview, "Camera Preview");
        EditorGUILayout.EndFoldoutHeaderGroup();

        if (renderCameraPreview)
        {
            if (oldRenderCameraPreview != renderCameraPreview)
            {
                UpdateCameraPreview();
            }

            cameraPreviewScale = EditorGUILayout.FloatField("Camera Preview Scale", cameraPreviewScale);

            if (cameraPreviewScale < 0.1f)
            {
                cameraPreviewScale = 0.1f;
            }

            float aspectRatio = (float)resolution.x / (float)resolution.y;

            Vector2 size;

            float setTo = Mathf.Max(Screen.width - 50, CAMERA_PREVIEW_MAX_HEIGHT);

            if (resolution.x > resolution.y)
            {
                size = new Vector2(
                    setTo,
                    setTo / aspectRatio
                    );
            }
            else if (resolution.x == resolution.y)
            {
                size = new Vector2(
                    setTo,
                    setTo
                    );
            }
            else
            {
                size = new Vector2(
                    setTo * aspectRatio,
                    setTo
                    );
            }

            if (size.y > CAMERA_PREVIEW_MAX_HEIGHT)
            {
                float divider = size.y / CAMERA_PREVIEW_MAX_HEIGHT;

                size.y = CAMERA_PREVIEW_MAX_HEIGHT;
                size.x /= divider;
            }

            size *= cameraPreviewScale;

            rect = EditorGUILayout.GetControlRect(GUILayout.Width(size.x), GUILayout.Height(size.y));

            rect.x += ((float)Screen.width - rect.width) / 2f;

            if (CurrentCamera != null && _cameraPreviewRenderTexture != null && _cameraPreview != null)
            {
                GUI.DrawTexture(rect, _cameraPreview);
            }
            else
            {
                EditorGUI.DrawRect(rect, Color.black);
            }

            timeBtwCameraPreviewUpdate = EditorGUILayout.FloatField(new GUIContent("Time Between Update", "Set to below 0 for no automatic updates at all."), timeBtwCameraPreviewUpdate);

            if (timeBtwCameraPreviewUpdate < 0 && GUILayout.Button("Manual Camera Preview Update"))
            {
                UpdateCameraPreview();
            }
        }

        EditorGUILayout.EndScrollView();

        Undo.RecordObject(this, "Changed Camera Screenshot Tool Settings");
    }

    private void Screenshot(string path)
    {
        // Setup
        Texture2D screenshot = new Texture2D(resolution.x, resolution.y, textureFormat, false);
        RenderTexture renderTexture = new RenderTexture(resolution.x, resolution.y, renderTextureDepth);

        //RenderPipelineSettings? startSettings = null;
        CameraClearFlags? clearFlags = null;
        Color? startBackgroundColor = null;

        if (transparentBackground)
        {
            //startSettings = PipelineAsset.currentPlatformRenderPipelineSettings;
            clearFlags = CurrentCamera.clearFlags;
            startBackgroundColor = CurrentCamera.backgroundColor;
        }

        // Disable motion blur
        List<VolumeComponent> motionBlurComponents = new List<VolumeComponent>();

        foreach (Volume volume in FindObjectsByType<Volume>(FindObjectsSortMode.None))
        {
            VolumeProfile profile = volume.sharedProfile;

            foreach (VolumeComponent volumeComponent in profile.components)
            {
                if (volumeComponent.GetType() == typeof(MotionBlur) && volumeComponent.active)
                {
                    volumeComponent.active = false;
                    motionBlurComponents.Add(volumeComponent);
                }
            }
        }

        // Render to render texture
        RenderTexture startRenderTexture = CurrentCamera.targetTexture;
        CurrentCamera.targetTexture = renderTexture;

        if (transparentBackground)
        {
            CurrentCamera.clearFlags = CameraClearFlags.SolidColor;
            CurrentCamera.backgroundColor = Color.clear;

            /*
            RenderPipelineSettings newSettings = startSettings.Value;
            newSettings.colorBufferFormat = RenderPipelineSettings.ColorBufferFormat.R16G16B16A16;
            PipelineAsset.currentPlatformRenderPipelineSettings = newSettings;
            */
        }

        CurrentCamera.Render();

        // Put pixels onto screenshot Texture2D
        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = renderTexture;
        screenshot.ReadPixels(new Rect(0, 0, resolution.x, resolution.y), 0, 0);
        screenshot.Apply();

        // Reset
        RenderTexture.active = activeRenderTexture;
        CurrentCamera.targetTexture = startRenderTexture;

        if (transparentBackground)
        {
            //PipelineAsset.currentPlatformRenderPipelineSettings = startSettings.Value;
            CurrentCamera.clearFlags = clearFlags.Value;
            CurrentCamera.backgroundColor = startBackgroundColor.Value;
        }

        foreach (VolumeComponent volumeComponent in motionBlurComponents)
        {
            volumeComponent.active = true;
        }

        // Write to file
        byte[] bytes =
            imageFormat switch
            {
                ImageFormat.jpg => screenshot.EncodeToJPG(),
                _ => screenshot.EncodeToPNG(),
            };

        File.WriteAllBytes(path, bytes);

        // Destroy
        DestroyImmediate(screenshot);
        DestroyImmediate(renderTexture);

        Debug.Log($"Took screenshot and saved it at \"{path}\"");
    }

    private void ClampResolution()
    {
        resolution.x = Mathf.Max(resolution.x, 1);
        resolution.y = Mathf.Max(resolution.y, 1);
    }

    private Vector2Int CameraPreviewResolution()
    {
        Vector2Int cameraPreviewResolution = resolution;

        if (resolution.x > CAMERA_PREVIEW_PIXEL_LIMIT || resolution.y > CAMERA_PREVIEW_PIXEL_LIMIT)
        {
            float aspectRatio = (float)resolution.x / (float)resolution.y;

            if (resolution.x > resolution.y)
            {
                cameraPreviewResolution = new Vector2Int(
                    CAMERA_PREVIEW_PIXEL_LIMIT,
                    Mathf.RoundToInt((float)CAMERA_PREVIEW_PIXEL_LIMIT / aspectRatio)
                    );
            }
            else if (resolution.x == resolution.y)
            {
                cameraPreviewResolution = new Vector2Int(CAMERA_PREVIEW_PIXEL_LIMIT, CAMERA_PREVIEW_PIXEL_LIMIT);
            }
            else
            {
                cameraPreviewResolution = new Vector2Int(
                    Mathf.RoundToInt((float)CAMERA_PREVIEW_PIXEL_LIMIT * aspectRatio),
                    CAMERA_PREVIEW_PIXEL_LIMIT
                    );
            }
        }

        cameraPreviewResolution.x = Mathf.Clamp(cameraPreviewResolution.x, 8, CAMERA_PREVIEW_PIXEL_LIMIT);
        cameraPreviewResolution.y = Mathf.Clamp(cameraPreviewResolution.y, 8, CAMERA_PREVIEW_PIXEL_LIMIT);

        return cameraPreviewResolution;
    }

    private static void Header(string text)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
    }

    private static string GetAutoScreenshotName()
    {
        return SceneManager.GetActiveScene().name.Replace(' ', '-') + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    }

    private void UpdateCameraPreview()
    {
        Vector2Int resolution = CameraPreviewResolution();

        if (_cameraPreviewRenderTexture == null)
        {
            CreateCameraPreviewRenderTexture(resolution);
        }
        else
        {
            _cameraPreviewRenderTexture.Release();

            _cameraPreviewRenderTexture.width = resolution.x;
            _cameraPreviewRenderTexture.height = resolution.y;
            _cameraPreviewRenderTexture.depth = renderTextureDepth;

            _cameraPreviewRenderTexture.Create();
        }

        if (_cameraPreview == null || _cameraPreview.width != resolution.x || _cameraPreview.height != resolution.y || _cameraPreview.format != textureFormat)
        {
            DestroyImmediate(_cameraPreview);
            CreateCameraPreviewTexture(resolution);
        }

        RenderCameraPreview();
    }

    private void CreateCameraPreviewRenderTexture(Vector2Int resolution)
    {
        _cameraPreviewRenderTexture = new RenderTexture(resolution.x, resolution.y, renderTextureDepth);
    }

    private void CreateCameraPreviewTexture(Vector2Int resolution)
    {
        _cameraPreview = new Texture2D(resolution.x, resolution.y, textureFormat, false);
    }

    private void RenderCameraPreview()
    {
        if (CurrentCamera == null)
        {
            return;
        }

        RenderTexture startingTargetTexture = CurrentCamera.targetTexture;

        CurrentCamera.targetTexture = _cameraPreviewRenderTexture;
        CurrentCamera.Render();

        CurrentCamera.targetTexture = startingTargetTexture;

        RenderTexture.active = _cameraPreviewRenderTexture;

        _cameraPreview.ReadPixels(new Rect(0, 0, _cameraPreviewRenderTexture.width, _cameraPreviewRenderTexture.height), 0, 0);
        _cameraPreview.Apply();

        RenderTexture.active = null;
    }

    public static void CreateFolderIfNotPresent()
    {
        if (Directory.Exists(FOLDER))
        {
            return;
        }

        Directory.CreateDirectory(FOLDER);
    }

    public enum ImageFormat
    {
        png,
        jpg,
    }

    public enum Mode
    {
        sceneView,
        customCamera,
    }
}