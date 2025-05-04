// Enable this to print out stats related to the generated mesh volumes.
// This can be useful to compare results when turning on "EvenlySizedPieces".
//#define PRINT_VOLUME_STATS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if !UNITY_2021_2_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#else
using UnityEditor.SceneManagement;
#endif

namespace DinoFracture.Editor
{
    [CustomEditor(typeof(PreFracturedGeometry))]
    [CanEditMultipleObjects()]
    public class PreFracturedGeometryEditor : FractureGeometryEditor
    {
        private const string cGeneratedFractureMeshesPrefabFolder = "FractureMeshes";

        private bool _waitForClick = false;

        private GUIStyle _centerStyle;
        private GUIStyle _buttonStyle;

        private PreFracturedGeometryEditorFractureProgress _progress;
        private static PreFracturedGeometryEditorFractureData _fractureData;

        private static Dictionary<string, PropertyName> _sPreFractureCommonPropertyNames;

        static PreFracturedGeometryEditor()
        {
            _sPreFractureCommonPropertyNames = new Dictionary<string, PropertyName>();
            AddPropertyName(_sPreFractureCommonPropertyNames, "GeneratedPieces");
            AddPropertyName(_sPreFractureCommonPropertyNames, "EntireMeshBounds");
        }

        public override void OnInspectorGUI()
        {
            EnsureProgressData();

            DrawCommonFractureProperties();

            Space(10);

            EditorGUILayout.LabelField("Fracture Results", _cHeaderTextStyle);
            DrawFractureProperties(_sPreFractureCommonPropertyNames);

            Space(10);

            DrawFractureEventProperties();

            Space(10);

            if (!IsRunningFractures())
            {
                if (GUILayout.Button("Create Fractures"))
                {
                    CreateFractureData();
                    GenerateFractures();
                }

                EditorUtility.ClearProgressBar();
            }
            else
            {
                _progress.DisplayGui();

#if !UNITY_2020_1_OR_NEWER
                // Forces the progress bar to continually update
                Repaint();
#endif
            }

            if (_waitForClick)
            {
                if (_buttonStyle == null)
                {
                    _buttonStyle = new GUIStyle(GUI.skin.button);
                    _buttonStyle.normal.textColor = Color.white;
                }

                Color color = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("Click on the Object", _buttonStyle))
                {
                    _waitForClick = false;
                }
                GUI.backgroundColor = color;
            }
            else
            {
                if (!IsRunningFractures())
                {
                    if (GUILayout.Button("Create Fractures at Point"))
                    {
                        CreateFractureData();
                        _waitForClick = true;
                    }
                }
            }

            Space(10);

            if (!IsRunningFractures())
            {
                if (GUILayout.Button("Delete Generated Pieces"))
                {
                    CreateFractureData();
                    RemoveFracturesFromScene();
                }
            }

            Space(10);

            if (Application.isPlaying)
            {
                if (GUILayout.Button("Crumble"))
                {
                    CreateFractureData();
                    GenerateFractures();
                }
            }
        }

        private void EnsureProgressData()
        {
            if (_progress == null)
            {
                _progress = new PreFracturedGeometryEditorFractureProgress();
                _progress.OnCanceled += StopRunningFractures;
            }
        }

        private void CreateFractureData()
        {
            _fractureData = new PreFracturedGeometryEditorFractureData();

            foreach (PreFracturedGeometry geom in targets)
            {
                _fractureData.GeomList.Add(geom);
            }

            _fractureData.FinalizeList();
        }

        private void ClearFractureData()
        {
            _fractureData = null;
        }

        private bool IsRunningFractures()
        {
            if (_fractureData == null)
            {
                return false;
            }

            foreach (PreFracturedGeometry geom in _fractureData.GeomList)
            {
                if (geom.IsProcessingFracture)
                {
                    return true;
                }
            }

            return false;
        }

        private void GenerateFractures(Vector3 localPoint = default)
        {
            if (Application.isPlaying)
            {
                foreach (PreFracturedGeometry geom in _fractureData.GeomList)
                {
                    geom.Fracture();
                }
            }
            else
            {
                _progress.FractureData = _fractureData;
                _progress.OnFracturesStarted();

                foreach (PreFracturedGeometry geom in _fractureData.GeomList)
                {
                    var result = geom.GenerateFractureMeshes(localPoint);
                    if (result != null)
                    {
                        _fractureData.FractureResults.Add(result);

                        result.OnFractureComplete += (args) =>
                        {
                            PrintStats(args, _fractureData);
                            SaveToDisk(args, _fractureData);
                        };
                    }
                }
            }
        }

        private void RemoveFracturesFromScene()
        {
            _waitForClick = false;

            foreach (PreFracturedGeometry geom in _fractureData.GeomList)
            {
                var generatedPiece = geom.GeneratedPieces;
                if (generatedPiece != null)
                {
                    // Delete the prefab
                    string instPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(generatedPiece);
                    if (!string.IsNullOrEmpty(instPath))
                    {
                        AssetDatabase.MoveAssetToTrash(instPath);
                    }

                    // Remove the fracture output directory if empty
                    try
                    {
                        string dirPath = Path.GetDirectoryName(instPath);
                        if (dirPath.EndsWith(cGeneratedFractureMeshesPrefabFolder))
                        {
                            dirPath = dirPath.Substring("Assets/".Length).Replace('\\', '/');

                            string absPath = Path.Combine(Application.dataPath, dirPath);
                            if (Directory.EnumerateFileSystemEntries(absPath).Count() == 0)
                            {
                                string projectRelDirPath = "Assets/" + dirPath;
                                FileUtil.DeleteFileOrDirectory(projectRelDirPath);
                                FileUtil.DeleteFileOrDirectory(projectRelDirPath + ".meta");

                                AssetDatabase.Refresh();
                            }
                        }
                    }
                    catch (Exception) { }
                }

                geom.ClearGeneratedPieces();
                MarkDirty(geom);
            }
        }

        private void StopRunningFractures()
        {
            if (_fractureData != null)
            {
                foreach (PreFracturedGeometry geom in _fractureData.GeomList)
                {
                    geom.StopRunningFracture();
                }

                ClearFractureData();
            }
        }

        protected override void OnSceneGUI()
        {
            base.OnSceneGUI();

            if (_waitForClick)
            {
                Vector2 mousePos = Event.current.mousePosition;

                if (_centerStyle == null)
                {
                    _centerStyle = new GUIStyle(GUI.skin.label);
                    _centerStyle.alignment = TextAnchor.UpperCenter;
                    _centerStyle.normal.textColor = Color.white;
                    _centerStyle.active.textColor = Color.white;
                    _centerStyle.hover.textColor = Color.white;
                }

                Handles.BeginGUI();
                GUI.Label(new Rect(mousePos.x - 80.0f, mousePos.y - 45.0f, 160.0f, 17.0f),
                    "Click on the object to", _centerStyle);
                GUI.Label(new Rect(mousePos.x - 80.0f, mousePos.y - 28.0f, 160.0f, 17.0f),
                    "create the fracture pieces.", _centerStyle);
                Handles.EndGUI();

                if (Event.current.type == EventType.Layout)
                {
                    HandleUtility.AddDefaultControl(0);
                }

                foreach (PreFracturedGeometry geom in _fractureData.GeomList)
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        _waitForClick = false;
                        Collider collider = geom.GetComponent<Collider>();
                        if (collider != null)
                        {
                            RaycastHit hit;
                            if (collider.Raycast(ray, out hit, 1000000000.0f))
                            {
                                Vector3 localPoint = geom.transform.worldToLocalMatrix.MultiplyPoint(hit.point);

                                GenerateFractures(localPoint);

                                break;
                            }
                        }
                    }
                    else if (Event.current.type == EventType.MouseMove)
                    {
                        SceneView.RepaintAll();
                    }
                }
            }
        }

        private void PrintStats(OnFractureEventArgs args, PreFracturedGeometryEditorFractureData data)
        {
#if PRINT_VOLUME_STATS
            Utilities.PrintStats("Fracture Mesh Volumes", args.GetMeshes(), (mesh) => mesh.Volume());
            Utilities.PrintStats("Fracture Mesh Bounds Volumes", args.GetMeshes(), (mesh) => mesh.BoundsVolume());
#endif
        }

        private void SaveToDisk(OnFractureEventArgs args, PreFracturedGeometryEditorFractureData data)
        {
            UpdatedProgressOnFractureCompleted(data);

            if (!args.IsValid)
            {
                return;
            }

            PreFracturedGeometry geomComp = args.OriginalObject as PreFracturedGeometry;
            if (!geomComp)
            {
                Debug.LogError("Failed to get prefractured geometry component");
            }

            string prefabPath;
            var prefabRoot = FindExistingFracturedAsset(geomComp);
            bool usingExistingPrefab = (prefabRoot != null);

            if (prefabRoot)
            {
                prefabPath = AssetDatabase.GetAssetPath(prefabRoot);

                // Remove all the existing data in the prefab
                foreach (var subAsset in AssetDatabase.LoadAllAssetRepresentationsAtPath(prefabPath))
                {
                    if ((subAsset is UnityEngine.Mesh) || (subAsset is FractureMeshesMetadata))
                    {
                        AssetDatabase.RemoveObjectFromAsset(subAsset);
                        DestroyImmediate(subAsset);
                    }
                }

                prefabRoot = PrefabUtility.SaveAsPrefabAssetAndConnect(args.FracturePiecesRootObject, prefabPath, InteractionMode.AutomatedAction);
                args.FracturePiecesRootObject.SetActive(false);

                if (geomComp.GeneratedPieces == null || geomComp.GeneratedPieces.transform.localToWorldMatrix != args.FracturePiecesRootObject.transform.localToWorldMatrix)
                {
                    geomComp.ClearGeneratedPieces();
                    geomComp.GeneratedPieces = args.FracturePiecesRootObject;
                    usingExistingPrefab = false;

                    MarkDirty(geomComp);
                }

                if (geomComp.EntireMeshBounds != args.OriginalMeshBounds)
                {
                    geomComp.EntireMeshBounds = args.OriginalMeshBounds;
                    MarkDirty(geomComp);
                }
            }
            else
            {
                prefabPath = GeneratePrefabPath(geomComp.gameObject);
                EnsureDirectory(prefabPath);

                prefabRoot = PrefabUtility.SaveAsPrefabAssetAndConnect(args.FracturePiecesRootObject, prefabPath, InteractionMode.AutomatedAction);

                geomComp.ClearGeneratedPieces();

                geomComp.GeneratedPieces = args.FracturePiecesRootObject;
                geomComp.GeneratedPieces.SetActive(false);
                geomComp.EntireMeshBounds = args.OriginalMeshBounds;

                MarkDirty(geomComp);
            }

            // Add the metadata
            FractureMeshesMetadata metadata = CreateInstance<FractureMeshesMetadata>();
            metadata.name = "Metadata";
            metadata.UniqueId = geomComp.UniqueId;
            metadata.ScenePath = GetScenePath(geomComp);
            AssetDatabase.AddObjectToAsset(metadata, prefabRoot);

            // Bake the meshes into the prefab
            for (int m = 0; m < args.FracturePiecesRootObject.transform.childCount; m++)
            {
                var generatedGameObject = args.FracturePiecesRootObject.transform.GetChild(m).gameObject;

                MeshFilter mf = generatedGameObject.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    var sharedMesh = mf.sharedMesh;
                    sharedMesh.name = $"Generated Mesh {m}";
                    AssetDatabase.AddObjectToAsset(sharedMesh, prefabRoot);
                }
            }

            PrefabUtility.ApplyPrefabInstance(args.FracturePiecesRootObject, InteractionMode.AutomatedAction);

#if UNITY_2021_1_OR_NEWER
            AssetDatabase.SaveAssetIfDirty(prefabRoot);
#else
            AssetDatabase.SaveAssets();
#endif

            AssetDatabase.ImportAsset(prefabPath);

            if (usingExistingPrefab)
            {
                DestroyImmediate(args.FracturePiecesRootObject);
                args.FracturePiecesRootObject = geomComp.GeneratedPieces;
            }
        }

        private void UpdatedProgressOnFractureCompleted(PreFracturedGeometryEditorFractureData data)
        {
            bool complete = data.OnComplete();

            _progress.OnFractureComplete();

            if (complete)
            {
                _progress.Hide();

                ClearFractureData();

                FractureEngine.ClearCachedFractureData();
            }
        }

        private string GetScenePath(PreFracturedGeometry geomComp)
        {
            List<string> parts = new List<string>();

            GameObject go = geomComp.gameObject;
            while (go != null)
            {
                parts.Add(go.name);

                var parent = go.transform.parent;
                go = (parent != null) ? parent.gameObject : null;
            }

            parts.Add(geomComp.gameObject.scene.name);

            parts.Reverse();
            return string.Join("/", parts);
        }

        private void EnsureDirectory(string path)
        {
            var fullPath = Path.Combine(Application.dataPath, Path.GetDirectoryName(path).Substring(7));

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        private void MarkDirty(PreFracturedGeometry geomComp)
        {
            EditorUtility.SetDirty(geomComp);
        }

        private GameObject FindExistingFracturedAsset(PreFracturedGeometry geomComp)
        {
            string[] blankObjGuids = AssetDatabase.FindAssets("t:DinoFracture.FractureMeshesMetadata");
            for (int j = 0; j < blankObjGuids.Length; j++)
            {
                var path = AssetDatabase.GUIDToAssetPath(blankObjGuids[j]);

                FractureMeshesMetadata obj = AssetDatabase.LoadAssetAtPath<FractureMeshesMetadata>(path);
                if (obj != null && obj.UniqueId == geomComp.UniqueId)
                {
                    return (GameObject)AssetDatabase.LoadMainAssetAtPath(path);
                }
            }

            return null;
        }

        private string GeneratePrefabPath(GameObject rootObject)
        {
            string baseDir = "Assets";

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
#if UNITY_2020_1_OR_NEWER
                var prefabDir = Path.GetDirectoryName(prefabStage.assetPath);
#else
                var prefabDir = Path.GetDirectoryName(prefabStage.prefabAssetPath);
#endif
                baseDir = prefabDir;
            }
            else
            {
                var scene = rootObject.scene;
                if (scene != null && !string.IsNullOrEmpty(scene.path))
                {
                    var sceneDir = Path.GetDirectoryName(scene.path).Replace('\\', '/');

                    baseDir = sceneDir;
                }
            }

            return GetUniqueFileName($"{baseDir}/{cGeneratedFractureMeshesPrefabFolder}/{rootObject.name}.prefab");
        }

        private string GetUniqueFileName(string filePath)
        {
            if (File.Exists(filePath))
            {
                return AssetDatabase.GenerateUniqueAssetPath(filePath);
            }
            return filePath;
        }
    }

    class PreFracturedGeometryEditorFractureData
    {
        public readonly List<PreFracturedGeometry> GeomList = new List<PreFracturedGeometry>();
        public readonly List<AsyncFractureResult> FractureResults = new List<AsyncFractureResult>();

        private int _countLeft;

        public int ActiveCount
        {
            get { return _countLeft; }
        }

        public void FinalizeList()
        {
            _countLeft = GeomList.Count;
        }

        public bool OnComplete()
        {
            return System.Threading.Interlocked.Decrement(ref _countLeft) == 0;
        }
    }

    class PreFracturedGeometryEditorFractureProgress
    {
        public event Action OnCanceled;

        public PreFracturedGeometryEditorFractureData FractureData;

#if UNITY_2020_1_OR_NEWER
        public int _progressId;
#endif

        private void Cancel()
        {
            if (OnCanceled != null)
            {
                OnCanceled();
            }

            Hide();
        }

        public void DisplayGui()
        {
#if UNITY_2020_1_OR_NEWER
            Color color = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;

            if (GUILayout.Button("Stop Fracturing"))
            {
                Cancel();
            }
            GUI.backgroundColor = color;
#else
            UpdateProgress();
#endif
        }

        public void OnFracturesStarted()
        {
#if UNITY_2020_1_OR_NEWER
            _progressId = Progress.Start("Fracturing Objects", null, Progress.Options.None);
            EditorApplication.update += OnEditorUpdate;
#endif
        }

        private void OnEditorUpdate()
        {
#if UNITY_2020_1_OR_NEWER
            UpdateProgress();
#endif
        }

        public void OnFractureComplete()
        {
#if UNITY_2020_1_OR_NEWER
            UpdateProgress();
#endif
        }

        public void Hide()
        {
#if UNITY_2020_1_OR_NEWER
            EditorApplication.update -= OnEditorUpdate;
            Progress.Remove(_progressId);
#endif
        }

        void UpdateProgress()
        {
            string text = string.Empty;
            float percent = 0.0f;

            if (FractureData != null)
            {
                float sumProgress = 0.0f;
                for (int i = 0; i < FractureData.FractureResults.Count; i++)
                {
                    var result = FractureData.FractureResults[i];
                    if (result.IsComplete)
                    {
                        sumProgress += 1.0f;
                    }
                    else
                    {
                        sumProgress += result.ProgressPercent;
                    }
                }

                percent = sumProgress / FractureData.FractureResults.Count;

                int numFractureCompleted = FractureData.GeomList.Count - FractureData.ActiveCount;
                text = $"Completed ({numFractureCompleted} / {FractureData.GeomList.Count})";
            }

#if UNITY_2020_1_OR_NEWER
            Progress.Report(_progressId, percent, text);
#else
            if (EditorUtility.DisplayCancelableProgressBar("Fracturing Objects", text, percent))
            {
                Cancel();
            }
#endif
        }
    }
}
