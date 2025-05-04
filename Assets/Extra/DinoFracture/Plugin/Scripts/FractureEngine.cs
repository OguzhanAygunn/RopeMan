using System.Collections.Generic;
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DinoFracture
{
    /// <summary>
    /// Argument passed to OnFracture message
    /// </summary>
    public sealed class OnFractureEventArgs
    {
        public OnFractureEventArgs(FractureGeometry orig, Bounds origBounds, GameObject root, FractureDetails details)
        {
            OriginalObject = orig;
            OriginalMeshBounds = origBounds;
            FracturePiecesRootObject = root;
            FractureDetails = details;
        }

        public bool IsValid => (FracturePiecesRootObject != null);

        /// <summary>
        /// The object that fractured.
        /// </summary>
        [UnityEngine.Tooltip("The object that fractured.")]
        public FractureGeometry OriginalObject;

        /// <summary>
        /// The bounds of the original mesh
        /// </summary>
        [UnityEngine.Tooltip("The bounds of the original mesh")]
        public Bounds OriginalMeshBounds;

        /// <summary>
        /// The root of the pieces of the resulting fracture.
        /// </summary>
        [UnityEngine.Tooltip("The root of the pieces of the resulting fracture.")]
        public GameObject FracturePiecesRootObject;

        /// <summary>
        /// The parameters used for the fracture.
        /// </summary>
        [UnityEngine.Tooltip("The parameters used for the fracture.")]
        public FractureDetails FractureDetails;

        /// <summary>
        /// Returns an enumerable of just the generated Unity meshes
        /// </summary>
        /// <returns></returns>
        public IEnumerable<UnityEngine.Mesh> GetMeshes()
        {
            if (FracturePiecesRootObject != null)
            {
                for (int i = 0; i < FracturePiecesRootObject.transform.childCount; i++)
                {
                    var child = FracturePiecesRootObject.transform.GetChild(i);
                    var mesh = child.GetComponent<MeshFilter>();
                    if (mesh != null)
                    {
                        yield return mesh.sharedMesh;
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }
    }

    public delegate void OnFractureEventHandler(OnFractureEventArgs args);

    /// <summary>
    /// The result of a fracture.
    /// </summary>
    public sealed class AsyncFractureResult
    {
        private GameObject _callbackObj;
        private OnFractureEventHandler _callbackFunc;

        private OnFractureEventArgs _args;

        private AsyncFractureOperation _fractureOp;

        public event OnFractureEventHandler OnFractureComplete
        {
            add
            {
                if (IsComplete)
                {
                    value(_args);
                }
                else
                {
                    _callbackFunc += value;
                }
            }
            remove { _callbackFunc -= value; }
        }

        /// <summary>
        /// Returns true if the operation has finished; false otherwise.
        /// This value will always be true for synchronous fractures.
        /// </summary>
        public bool IsComplete { get; private set; }

        /// <summary>
        /// Returns true if the operation has finished and returned valid results.
        /// </summary>
        public bool IsSuccessful
        {
            get { return IsComplete && PiecesRoot != null; }
        }

        /// <summary>
        /// The original script that initiated the fracture
        /// </summary>
        public FractureGeometry FractureGeometry { get { return _args?.OriginalObject; } }

        /// <summary>
        /// The root of the pieces of the resulting fracture
        /// </summary>
        public GameObject PiecesRoot { get { return _args?.FracturePiecesRootObject; } }

        /// <summary>
        /// The bounds of the original mesh
        /// </summary>
        public Bounds EntireMeshBounds { get { return (_args != null) ? _args.OriginalMeshBounds : new Bounds(); } }

        internal bool StopRequested { get; private set; }

        internal AsyncFractureOperation FractureOperation { set { _fractureOp = value; } }

        /// <summary>
        /// A number [0..1] denoting the completion percentage of the fracture.
        /// Computed by dividing <see cref="CompletedOperationCount"/> by <see cref="TotalOperationCount"/>.
        /// </summary>
        public float ProgressPercent { get { return (_fractureOp != null) ? _fractureOp.ProgressPercent : 0.0f; } }

        /// <summary>
        /// The number of completed operations / segments in this fracture.
        /// The percent complete is this value divided by <see cref="TotalOperationCount"/>.
        /// </summary>
        public int CompletedOperationCount
        {
            get { return (_fractureOp != null) ? _fractureOp.CompletedOperationCount : 0; }
        }

        /// <summary>
        /// The total number of operations / segments in this fracture.
        /// The percent complete is <see cref="CompletedOperationCount"/> divided by this number.
        /// </summary>
        public int TotalOperationCount
        {
            get { return (_fractureOp != null) ? _fractureOp.TotalOperationCount : 0; }
        }

        internal void SetResult(OnFractureEventArgs args)
        {
            if (IsComplete)
            {
                Logger.Log(LogLevel.Warning, "DinoFracture: Setting AsyncFractureResult's results twice.", args.OriginalObject?.gameObject);
            }
            else
            {
                _args = args;
                IsComplete = true;

                FireCallbacks();
            }
        }

        public void StopFracture()
        {
            StopRequested = true;
        }

        public void SetCallbackObject(GameObject obj)
        {
            _callbackObj = obj;

            if (IsComplete)
            {
                FireCallbackOnCallbackObj();
            }
        }

        public void SetCallbackObject(Component obj)
        {
            _callbackObj = obj?.gameObject;

            if (IsComplete)
            {
                FireCallbackOnCallbackObj();
            }
        }

        private void FireCallbacks()
        {
            FirePieceCallbacks();

            FireCallbackOnCallbackObj();
            FireCallbackOnCallbackFunc();

            FirePiecePostFractureCallbacks();
        }

        private void FirePieceCallbacks()
        {
            FirePieceCallbacks("OnFracture");
        }

        private void FirePiecePostFractureCallbacks()
        {
            FirePieceCallbacks("OnPostFracture");
        }

        private void FirePieceCallbacks(string callbackName)
        {
            if (_args.OriginalObject != null)
            {
                if (Application.isPlaying)
                {
                    _args.OriginalObject.gameObject.SendMessage(callbackName, _args, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    _args.OriginalObject.OnFracture(_args);
                }
            }

            if (_args.FracturePiecesRootObject != null)
            {
                if (Application.isPlaying)
                {
                    Transform trans = _args.FracturePiecesRootObject.transform;
                    for (int i = 0; i < trans.childCount; i++)
                    {
                        trans.GetChild(i).gameObject.SendMessage(callbackName, _args, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }

        private void FireCallbackOnCallbackObj()
        {
            if (_callbackObj != null && _callbackObj != _args.OriginalObject.gameObject)
            {
                _callbackObj.SendMessage("OnFracture", _args, SendMessageOptions.DontRequireReceiver);
            }
        }

        private void FireCallbackOnCallbackFunc()
        {
            if (_callbackFunc != null)
            {
                _callbackFunc.Invoke(_args);
            }
        }
    }

    /// <summary>
    /// This component is created on demand to manage the fracture coroutines.
    /// It is not intended to be added by the user.
    /// </summary>
    public sealed class FractureEngine : FractureEngineBase
    {
        private struct FractureInstance
        {
            public AsyncFractureResult Result;
            public IEnumerator Enumerator;

            public FractureInstance(AsyncFractureResult result, IEnumerator enumerator)
            {
                Result = result;
                Enumerator = enumerator;
            }
        }

        private static FractureEngine _instance;

        [SerializeField] private bool _suspended;

        [SerializeField] private int _maxRunningFractures = 0;

        private List<FractureInstance> _runningFractures = new List<FractureInstance>();
        private List<FractureInstance> _pendingFractures = new List<FractureInstance>();

        private static new FractureEngine Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject inst = new GameObject("Fracture Engine");
                    _instance = inst.AddComponent<FractureEngine>();
                    FractureEngineBase.Instance = _instance;
                }

                return _instance;
            }
        }

        /// <summary>
        /// True if all further fracture operations should be a no-op.
        /// </summary>
        public static bool Suspended
        {
            get { return Instance._suspended; }
            set { Instance._suspended = value; }
        }

        /// <summary>
        /// Returns true if there are fractures currently in progress
        /// </summary>
        public static bool HasFracturesInProgress
        {
            get { return Instance._runningFractures.Count > 0; }
        }

        /// <summary>
        /// The maximum number of async fractures we can process at a time.
        /// If this is set to 0 (default), an unlimited number can be run.
        /// </summary>
        /// <remarks>
        /// NOTE: Synchronous fractures always run immediately
        /// </remarks>
        public static int MaxRunningFractures
        {
            get { return Instance._maxRunningFractures; }
        }

        private static int EffectiveMaxRunningFractures
        {
            get
            {
                if (!Application.isPlaying)
                {
                    return 4;
                }
                else
                {
                    return MaxRunningFractures;
                }
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                FractureEngineBase.Instance = _instance;
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                FractureEngineBase.Instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            // Clear the cache data. This is mostly for stopping play in the
            // editor to get us back to a clean cache state.
            if (_instance == this)
            {
                ClearCachedFractureData();
            }
        }

        /// <summary>
        /// Starts a fracture operation
        /// </summary>
        /// <param name="details">Fracture info</param>
        /// <param name="callback">The object to fracture</param>
        /// <param name="piecesParent">The parent of the resulting fractured pieces root object</param>
        /// <param name="transferMass">True to distribute the original object's mass to the fracture pieces; false otherwise</param>
        /// <param name="hideAfterFracture">True to hide the originating object after fracturing</param>
        /// <returns></returns>
        public static AsyncFractureResult StartFracture(FractureDetails details, FractureGeometry callback, Transform piecesParent, bool transferMass, bool hideAfterFracture)
        {
            AsyncFractureResult res = new AsyncFractureResult();
            if (Suspended)
            {
                res.SetResult(new OnFractureEventArgs(callback, new Bounds(), null, null));
            }
            else
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                FractureBuilder.DisableMultithreading = true;
#endif

                IEnumerator it = Instance.WaitForResults(details, callback, piecesParent, transferMass, hideAfterFracture, res);

                if (details.Asynchronous)
                {
                    if (EffectiveMaxRunningFractures <= 0 || Instance._runningFractures.Count < EffectiveMaxRunningFractures)
                    {
                        if (it.MoveNext())
                        {
#if UNITY_EDITOR
                            if (Instance._runningFractures.Count == 0 && !Application.isPlaying)
                            {
                                EditorApplication.update += Instance.OnEditorUpdate;
                            }
#endif
                            Instance._runningFractures.Add(new FractureInstance(res, it));
                        }
                    }
                    else
                    {
                        Instance._pendingFractures.Add(new FractureInstance(res, it));
                    }
                }
                else
                {
                    // There should only be one iteration
                    while (it.MoveNext())
                    {
                        Logger.Log(LogLevel.Warning, "DinoFracture: Sync fracture taking more than one iteration", callback?.gameObject);
                    }

#if UNITY_EDITOR
                    // Force an update to do any work that was queued
                    if (!Application.isPlaying)
                    {
                        Instance.Update();
                    }
#endif
                }
            }
            return res;
        }

        private void OnEditorUpdate()
        {
            Update();

            if (_runningFractures.Count == 0)
            {
#if UNITY_EDITOR
                EditorApplication.update -= OnEditorUpdate;
#endif
                DestroyImmediate(gameObject);
            }
        }

        protected override void Update()
        {
            base.Update();

            UpdateFractures();
        }

        private void UpdateFractures()
        {
            for (int i = _runningFractures.Count - 1; i >= 0; i--)
            {
                if (_runningFractures[i].Result.StopRequested)
                {
                    _runningFractures.RemoveAt(i);
                }
                else
                {
                    if (!_runningFractures[i].Enumerator.MoveNext())
                    {
                        _runningFractures.RemoveAt(i);
                    }
                }
            }

            for (int i = 0; i < _pendingFractures.Count; i++)
            {
                if (EffectiveMaxRunningFractures <= 0 || _runningFractures.Count < EffectiveMaxRunningFractures)
                {
                    _runningFractures.Add(_pendingFractures[i]);
                    _pendingFractures.RemoveAt(i);
                    i--;
                }
            }
        }

        private IEnumerator WaitForResults(FractureDetails details, FractureGeometry callback, Transform piecesParent, bool transferMass, bool hideAfterFracture, AsyncFractureResult result)
        {
            AsyncFractureOperation operation;
            if (details is ShatterDetails shatterDetails)
            {
                operation = FractureBuilder.Shatter(shatterDetails);
            }
            else if (details is SliceDetails sliceDetails)
            {
                operation = FractureBuilder.Slice(sliceDetails);
            }
            else
            {
                Logger.Log(LogLevel.UserDisplayedError, "Invalid operation type");
                result.SetResult(new OnFractureEventArgs(callback, new Bounds(), null, details));
                yield break;
            }

            result.FractureOperation = operation;

            while (!operation.IsComplete)
            {
                // Async fractures should not happen while in edit mode because game objects don't update too often
                // and the coroutine is not pumped. Sync fractures should not reach this point.
                System.Diagnostics.Debug.Assert(Application.isPlaying && operation.Details.Asynchronous);
                yield return null;
            }

            if (callback == null)
            {
                result.SetResult(new OnFractureEventArgs(callback, new Bounds(), null, details));
                yield break;
            }

            if (operation.Result == null)
            {
                // Something failed catastrophically during fracture

                Logger.Log(LogLevel.UserDisplayedError, "DinoFracture: Fracture failed.", callback.gameObject);
                result.SetResult(new OnFractureEventArgs(callback, new Bounds(), null, details));
                yield break;
            }
            else if (operation.ErrorDuringFracture)
            {
                Logger.Log(LogLevel.UserDisplayedError, "Fracturing failed. Results are likely invalid.", callback.gameObject);
            }

            float totalMass = 0.0f;
            float totalVolume = 0.0f;

            if (transferMass)
            {
                totalMass = FractureUtilities.GetTotalMass(callback.gameObject);
            }
            totalVolume = FractureUtilities.GetTotalVolume(callback.gameObject);

            IReadOnlyList<FracturedMesh> meshes = operation.Result.GetMeshes();

            GameObject rootGO = new GameObject(callback.gameObject.name + " - Fracture Root");
            rootGO.transform.SetParent(piecesParent ?? callback.transform.parent, false);
            rootGO.transform.position = callback.transform.position;
            rootGO.transform.rotation = callback.transform.rotation;

            // It is expected that pieces parent has all the same scaling as the game object being fractured.
            rootGO.transform.localScale = Vector3.one;

            Material[] sharedMaterials = callback.GetComponent<Renderer>().sharedMaterials;

            bool skinnedMeshRendererWarningFired = false;
            for (int i = 0; i < meshes.Count; i++)
            {
                var fractureTemplate = (callback.FractureTemplate != null) ? callback.FractureTemplate : callback.gameObject;

                GameObject go = Instantiate(fractureTemplate);
                go.name = "Fracture Object " + i;
                go.transform.SetParent(rootGO.transform, false);
                go.transform.localPosition = meshes[i].Offset;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                go.SetActive(true);

                meshes[i].Mesh.name = go.name;

                if (go.TryGetComponent(out MeshFilter mf))
                {
                    mf.sharedMesh = meshes[i].Mesh;
                }
                else
                {
                    if (go.TryGetComponent(out SkinnedMeshRenderer smr))
                    {
                        if (!skinnedMeshRendererWarningFired)
                        {
                            skinnedMeshRendererWarningFired = true;
                            Logger.Log(LogLevel.UserDisplayedWarning, "DinoFracture: Setting static mesh onto a skinned renderer on the fracture template. This scenario is not fully supported yet and the mesh will not animate.", callback.gameObject);
                        }

                        smr.sharedMesh = meshes[i].Mesh;
                    }
                }

                // Copy the correct materials to the new mesh.
                // There are some things we need to account for:
                //
                // 1) Not every subMesh in the original mesh will still
                //    exist. It may have no triangles now and we should
                //    skip over those materials.
                // 2) We have added a new submesh for the inside triangles
                //    and need to add the inside material.
                // 3) The original mesh might have more materials than
                //    were subMeshes. In that case, we want to append
                //    the extra materials to the end of our list.
                //
                // The final material list will be:
                // * Used materials from the original mesh
                // * Inside material
                // * Extra materials from the original mesh
                if (go.TryGetComponent(out MeshRenderer meshRenderer))
                {
                    // There is an entry in EmptyTriangles for each subMesh,
                    // including the newly added inside triangles. The last
                    // subMesh is always the inside triangles we created.
                    int numOrigSubMeshes = meshes[i].EmptyTriangles.Count - 1;

                    Material[] materials = new Material[sharedMaterials.Length - meshes[i].EmptyTriangleCount + 1];
                    int matIdx = 0;
                    for (int m = 0; m < numOrigSubMeshes; m++)
                    {
                        if (!meshes[i].EmptyTriangles[m])
                        {
                            materials[matIdx++] = sharedMaterials[m];
                        }
                    }
                    if (!meshes[i].EmptyTriangles[numOrigSubMeshes])
                    {
                        materials[matIdx++] = callback.InsideMaterial;
                    }
                    for (int m = numOrigSubMeshes; m < sharedMaterials.Length; m++)
                    {
                        materials[matIdx++] = sharedMaterials[m];
                    }

                    meshRenderer.sharedMaterials = materials;
                }

                if (go.TryGetComponent(out MeshCollider meshCol))
                {
                    // Check if we have a "bad" mesh.
                    //
                    // A small vertex count can lead to errors being thrown by Unity because
                    // it is not able to generate a mesh collider with > 0 volume.
                    //
                    bool notValidMeshCollider = meshes[i].Flags.HasFlag(FracturedMeshResultFlags.SmallVertexCount) || meshes[i].Flags.HasFlag(FracturedMeshResultFlags.ZeroVolume);
                    if (notValidMeshCollider && operation.Details.IssueResolution != FractureIssueResolution.NoAction)
                    {
                        // Replace the mesh collider with a sphere collider
                        if (operation.Details.IssueResolution == FractureIssueResolution.ReplaceMeshCollider)
                        {
                            if (Application.isPlaying)
                            {
                                Destroy(meshCol);
                            }
                            else
                            {
                                DestroyImmediate(meshCol);
                            }

                            BoxCollider boxCollider = go.AddComponent<BoxCollider>();
                            boxCollider.center = meshes[i].Mesh.bounds.center;
                            boxCollider.size = meshes[i].Mesh.bounds.size;
                        }
                    }
                    else
                    {
                        meshCol.sharedMesh = mf.sharedMesh;
                    }
                }

                float thisMass = 0.0f;
                float thisVolume = FractureUtilities.GetThisVolume(go);
                if (go.TryGetComponent(out Rigidbody rb))
                {
                    if (transferMass)
                    {
                        thisMass = FractureUtilities.GetThisMass(go, totalMass, totalVolume, thisVolume);
                        rb.mass = thisMass;
                    }
                    else
                    {
                        thisMass = rb.mass;
                    }
                }

                {
                    if (!go.TryGetComponent(out FracturedObject fo))
                    {
                        fo = go.AddComponent<FracturedObject>();
                    }

                    fo.TotalMass = totalMass;
                    fo.TotalVolume = totalVolume;
                    fo.ThisMass = thisMass;
                    fo.ThisVolume = thisVolume;
                }

                if (Application.isPlaying)
                {
                    if (go.TryGetComponent(out CleanupMeshOnDestroy cleanupComp))
                    {
                        cleanupComp.SetIsRuntimeAsset();
                    }
                }

                if (go.TryGetComponent(out FractureGeometry fg))
                {
                    fg.InsideMaterial = callback.InsideMaterial;
                    fg.FractureTemplate = callback.FractureTemplate;
                    fg.PiecesParent = callback.PiecesParent;
                    fg.NumGenerations = (callback.NumGenerations > 0) ? callback.NumGenerations - 1 : callback.NumGenerations;
                    fg.DistributeMass = callback.DistributeMass;

                    // It is assumed that any geometry produced by the engine will be valid.
                    // No need to check in the future.
                    fg.ForceValidGeometry();
                }

                // Disable the game object if we have found errors
                if (meshes[i].Flags != FracturedMeshResultFlags.NoIssues &&
                    operation.Details.IssueResolution == FractureIssueResolution.DisableGameObject)
                {
                    go.SetActive(false);
                }
            }

            // The physics engine sometimes has stale data for the rigid body position.
            // Calling this ensures the rigid body is positioned where the game object's transform
            // is right now so that the next physics update does not reposition the game object.
            Physics.SyncTransforms();

            OnFractureEventArgs args = new OnFractureEventArgs(callback, operation.Result.EntireMeshBounds, rootGO, details);

            result.SetResult(args);

            if (hideAfterFracture)
            {
                callback.gameObject.SetActive(false);
            }
        }
    }
}