using System;
using System.Collections.Generic;
using UnityEngine;

using UnityMesh = UnityEngine.Mesh;

namespace DinoFracture
{
    /// <summary>
    /// This is the base class for the PreFractureGeometry and RuntimeFractureGeometry components.
    /// As such, it is not intended to be directly added to any game object even though fracture
    /// initiator components rely on it.
    /// </summary>
    public abstract class FractureGeometry : MonoBehaviour, ISerializationCallbackReceiver
    {
        /// <summary>
        /// OnFracture() Unity event wrapper
        /// </summary>
        [Serializable]
        public class OnFractureEvent : UnityEngine.Events.UnityEvent<OnFractureEventArgs>
        {
        }

        /// <summary>
        /// Unity cannot handle the serializable attribute on types defined in dlls.
        /// So, we have to duplicate the SlicePlane structure here in order to save it.
        /// </summary>
        [Serializable]
        public struct SlicePlaneSerializable
        {
            public static readonly SlicePlaneSerializable Identity = new SlicePlaneSerializable() { Position = Vector3.zero, Rotation = Quaternion.identity, Scale = 1.0f };

            public Vector3 Position;
            public Quaternion Rotation;
            public float Scale;

            /// <summary>
            /// Converts this serialization helper to a normal slice plane
            /// </summary>
            /// <returns></returns>
            public SlicePlane ToSlicePlane()
            {
                var ret = new SlicePlane();
                ret.Position = Position;
                ret.Rotation = Rotation;
                ret.Scale = Scale;
                return ret;
            }
        }

        /// <summary>
        /// Unity cannot handle the serializable attribute on types defined in dlls.
        /// This wraps a DinoFracture.Size type.
        /// </summary>
        [Serializable]
        public struct SizeSerializable
        {
            /// <summary>
            /// What the bounds is relative to.
            /// </summary>
            [UnityEngine.Tooltip("What the bounds is relative to.")]
            public SizeSpace Space;

            /// <summary>
            /// The size of the bounds, in the specified space.
            /// </summary>
            [UnityEngine.Tooltip("The size of the bounds, in the specified space.")]
            public float Value;

            public static implicit operator Size(SizeSerializable s)
            {
                return new Size() { Space = s.Space, Value = s.Value };
            }
        }

        /// <summary>
        /// Unity cannot handle the serializable attribute on types defined in dlls.
        /// This wraps a DinoFracture.UVBounds type.
        /// </summary>
        [Serializable]
        public struct UVBoundsSerializable
        {
            /// <summary>
            /// Constructor taking the bounds
            /// </summary>
            public UVBoundsSerializable(in Vector2 startUV, in Vector2 endUV)
            {
                StartUV = startUV;
                EndUV = endUV;
            }

            /// <summary>
            /// Start UV coordinates. Expected to be in the range [0..1].
            /// </summary>
            /// <remarks>
            /// Coord (0, 0) is the bottom left of the texture.
            /// Coord (1, 1) is the top right of the texture.
            /// </remarks>
            [Tooltip("Start UV coordinates. Expected to be in the range [0..1].\n\nCoord (0, 0) is the bottom left of the texture.\nCoord (1, 1) is the top right of the texture.")]
            public Vector2 StartUV;

            /// <summary>
            /// End UV coordinates. Expected to be in the range [0..1].
            /// </summary>
            /// <remarks>
            /// Coord (0, 0) is the bottom left of the texture.
            /// Coord (1, 1) is the top right of the texture.
            /// </remarks>
            [Tooltip("End UV coordinates. Expected to be in the range [0..1].\n\nCoord (0, 0) is the bottom left of the texture.\nCoord (1, 1) is the top right of the texture.")]
            public Vector2 EndUV;

            public static implicit operator UVBounds(UVBoundsSerializable s)
            {
                return new UVBounds(s.StartUV, s.EndUV);
            }
        }

        /// <summary>
        /// The material assigned to the “inside” triangles of the fracture pieces.
        /// These are the triangles that DinoFracture creates.  The surface triangles 
        /// of the original mesh retain their materials.
        /// </summary>
        [UnityEngine.Tooltip("The material assigned to the “inside” triangles of the fracture pieces. These are the triangles that DinoFracture creates.  The surface triangles of the original mesh retain their materials.")]
        public Material InsideMaterial;

        /// <summary>
        /// If true, newly generated triangles using the "InsideMaterial" will attempt
        /// to be part of the same existing material in th mesh.
        /// 
        /// If false, newly generated triangles will always be put in a new material placed
        /// after all _used_ existing materials. Additionally, new materials will continue to
        /// be appended upon further fractures.
        /// </summary>
        [UnityEngine.Tooltip("If true, newly generated triangles using the \"InsideMaterial\" will attempt to be part of the same existing material in th mesh.\r\n\r\nIf false, newly generated triangles will always be put in a new material placed after all _used_ existing materials. Additionally, new materials will continue to be appended upon further fractures.")]
        public bool OptimizeMaterialUsage = true;

        /// <summary>
        /// This game object will be cloned for each facture piece.  It is required to
        /// have a MeshFilter component.  If a MeshCollider component is added, it will
        /// be assigned the fracture mesh.
        /// </summary>
        [UnityEngine.Tooltip("This game object will be cloned for each facture piece.  It is required to have a MeshFilter component.  If a MeshCollider component is added, it will be assigned the fracture mesh.")]
        public GameObject FractureTemplate;

        /// <summary>
        /// The parent of the generated pieces.  Each fracture produces a root object
        /// with fracture pieces (clones of FractureTemplate) as children.  The root
        /// object is parented to PiecesParent.
        /// </summary>
        [UnityEngine.Tooltip("The parent of the generated pieces.  Each fracture produces a root object with fracture pieces (clones of FractureTemplate) as children.  The root object is parented to PiecesParent.")]
        public Transform PiecesParent;

        /// <summary>
        /// The type of fracture to produce when Fracture() is called.
        /// </summary>
        [UnityEngine.Tooltip("The type of fracture to produce when Fracture() is called.")]
        public FractureType FractureType = FractureType.Shatter;

        /// <summary>
        /// The planes to use when slicing the mesh. Not used when fracturing into pieces.
        /// </summary>
        /// <remarks>
        /// Each slice plane must be in local space. Use FractureGeometry.CreateSlicePlane() to
        /// create a compatible local space plane from a Unity world space plane.
        /// </remarks>
        [UnityEngine.Tooltip("The planes to use when slicing the mesh. Not used when fracturing into pieces.")]
        public SlicePlaneSerializable[] SlicePlanes;

        /// <summary>
        /// The number of fracture pieces generated per iteration.  Fault lines are
        /// spread evenly around the fracture point.  The number of total pieces
        /// generated is NumFracturePieces ^ NumIterations.
        /// </summary>
        [UnityEngine.Tooltip("The number of fracture pieces generated per iteration.  Fault lines are spread evenly around the fracture point.  The number of total pieces generated is NumFracturePieces ^ NumIterations.")]
        public int NumFracturePieces = 5;

        /// <summary>
        /// The number of passes of fracturing.  Using lower piece count with a higher
        /// iteration count is computationally faster than a higher piece count with a lower
        /// iteration count.  Ex: 5 pieces with 2 iterations is faster than 25 pieces and 
        /// 1 iteration.  The downside to using more iterations is fractures can become 
        /// less uniform.  In general, keep this number below 4.  The number of total pieces 
        /// generated is NumFracturePieces ^ NumIterations.    
        /// </summary>
        /// <remarks>It is recommended you use an iteration count of 1 when 0 &lt; FractureRadius &lt; 1.</remarks>
        [UnityEngine.Tooltip("The number of passes of fracturing.  Using lower piece count with a higher iteration count is computationally faster than a higher piece count with a lower iteration count.  Ex: 5 pieces with 2 iterations is faster than 25 pieces and  1 iteration.  The downside to using more iterations is fractures can become  less uniform.  In general, keep this number below 4.  The number of total pieces generated is NumFracturePieces ^ NumIterations.    ")]
        public int NumIterations = 2;

        /// <summary>
        /// If true, the engine will attempt to make all the randomly generated pieces
        /// roughly the same size. This adds a little processing time to the fracture.
        /// </summary>
        /// <remarks>
        /// Do not set this to true if FractureRadius > 0.
        /// </remarks>
        [UnityEngine.Tooltip("If true, the engine will attempt to make all the randomly generated pieces roughly the same size. This adds a little processing time to the fracture.")]
        public bool EvenlySizedPieces = true;

        /// <summary>
        /// To allow for fracture pieces to be further fractured, the FractureTemplate should
        /// have a FractureGeometry component.  NumGenerations dictates how many times the
        /// geometry can be re-fractured.  The count is decremented and passed on to the 
        /// component in each generated piece.  Ex: A value of 2 means this piece can be
        ///  fractured and each generated piece can be fractured.  The second generation 
        /// of fractures cannot be fractured further.  
        /// </summary>
        /// <remarks>Specify a negative value on the main piece to allow for infinite repeated fractures</remarks>
        [UnityEngine.Tooltip("To allow for fracture pieces to be further fractured, the FractureTemplate should have a FractureGeometry component.  NumGenerations dictates how many times the geometry can be re-fractured.  The count is decremented and passed on to the component in each generated piece.  Ex: A value of 2 means this piece can be  fractured and each generated piece can be fractured.  The second generation  of fractures cannot be fractured further.  ")]
        public int NumGenerations = 1;

        /// <summary>
        /// A value between 0 and 1 that indicates how clustered the fracture lines are.  A
        /// value of 0 or 1 means fractures are evenly distributed across the mesh.  A value
        /// between means they are clustered within a percentage of the mesh bounds.
        /// Ex: a value of 0.3 means fractures are clustered around the fracture point in a 
        /// volume 30% the size of the mesh.  Pre-fracture geometry typically has this value
        /// set to 0 or 1 because there isn’t always a pre-determined point of fracture.
        /// </summary>
        [Obsolete("Use FractureSize instead")]
        [UnityEngine.Tooltip("A value between 0 and 1 that indicates how clustered the fracture lines are.  A value of 0 or 1 means fractures are evenly distributed across the mesh.  A value between means they are clustered within a percentage of the mesh bounds. Ex: a value of 0.3 means fractures are clustered around the fracture point in a  volume 30% the size of the mesh.  Pre-fracture geometry typically has this value set to 0 or 1 because there isn’t always a pre-determined point of fracture.")]
        public float FractureRadius;

        /// <summary>
        /// The approximate size of fractures to create during shatter. When combined with a position through a
        /// call to Fracture(), the shatter planes will be clustered within this bounds.
        /// </summary>
        [UnityEngine.Tooltip("The approximate size of fractures to create during shatter. When combined with a position through a call to Fracture(), the shatter planes will be clustered within this bounds.")]
        public SizeSerializable FractureSize;

        /// <summary>
        /// If set to EntireMesh, the UV map for each inside triangle will be mapped to a box 
        /// the size of the original mesh.  If set to piece, inside triangles will be mapped to 
        /// a box the size of the individual fracture piece.
        /// </summary>
        [UnityEngine.Tooltip("If set to EntireMesh, the UV map for each inside triangle will be mapped to a box the size of the original mesh.  If set to piece, inside triangles will be mapped to  a box the size of the individual fracture piece.")]
        public FractureUVScale UVScale = FractureUVScale.Piece;

        /// <summary>
        /// Final 'inside' triangles will be remapped to be within this range. This does not affect the UVs on the
        /// incoming mesh and works with any value set for the UVScale.
        /// 
        /// This can be used with an atlas texture to constrain the generated triangles to use only a specific portion of the texture map.
        /// </summary>
        [UnityEngine.Tooltip("Final 'inside' triangles will be remapped to be within this range. This does not affect the UVs on the incoming mesh and works with any value set for the UVScale.\n\nThis can be used with an atlas texture to constrain the generated triangles to use only a specific portion of the texture map.")]
        public UVBoundsSerializable UVBounds = new UVBoundsSerializable(Vector2.zero, Vector2.one);

        /// <summary>
        /// If true and both this game object and the FractureTemplate have a RigidBody component,
        /// each fracture piece will have a mass set to a value proportional to its volume.
        /// That is, the density of the fracture piece will equal the density of the original mesh.
        /// If false, the mass property goes untouched.
        /// </summary>
        [UnityEngine.Tooltip("If true and both this game object and the FractureTemplate have a RigidBody component, each fracture piece will have a mass set to a value proportional to its volume. That is, the density of the fracture piece will equal the density of the original mesh. If false, the mass property goes untouched.")]
        public bool DistributeMass = true;

        /// <summary>
        /// If true, a final pass will be done to separate out meshes that are not
        /// physically connected. This can only happen when the mesh has concave parts.
        /// </summary>
        /// <remarks>
        /// This process can be slow. It is recommended to be off for runtime fractures
        /// unless there is a good chance of disjoint pieces.
        /// </remarks>
        [UnityEngine.Tooltip("If true, a final pass will be done to separate out meshes that are not physically connected. This can only happen when the mesh has concave parts.")]
        public bool SeparateDisjointPieces = false;

        /// <summary>
        /// The random seed to use when initiating the fracture. If set to zero, then
        /// the system clock will be used to create a random seed.
        /// </summary>
        [UnityEngine.Tooltip("The random seed to use when initiating the fracture. If set to zero, then the system clock will be used to create a random seed.")]
        public int RandomSeed = 0;

        /// <summary>
        /// Unity event that fires whenever a fracture on this object completes
        /// </summary>
        [UnityEngine.Tooltip("Unity event that fires whenever a fracture on this object completes")]
        public OnFractureEvent OnFractureCompleted;

        private AsyncFractureResult _runningFracture = null;

        [SerializeField]
        private string _uniqueId = Guid.NewGuid().ToString("N");

        [SerializeField]
        private MeshValidity _meshValidity = MeshValidity.Unknown;
        private MeshTopologyError _topologyErrors = MeshTopologyError.None;
        private int _lastMeshId = 0;    // Cache so we don't re-validate unnecessarily

        /// <summary>
        /// Not recommended to be set. But if set to true, no mesh cleaning will
        /// occur during the fracture process. This has no effect if the mesh is
        /// already clean.
        /// </summary>
        [UnityEngine.Tooltip("Not recommended to be set. But if set to true, no mesh cleaning will occur during the fracture process. This has no effect if the mesh is already clean.")]
        public bool SkipMeshCleaning = false;

        /// <summary>
        /// Used internally to uniquely identify this object
        /// </summary>
        public string UniqueId => _uniqueId;

        /// <summary>
        /// Used to determine if the mesh is of known good quality to fracture.
        /// </summary>
        public MeshValidity MeshValidity => _meshValidity;

        internal MeshTopologyError MeshTopologyErrors => _topologyErrors;

        /// <summary>
        /// Are we in the middle of computing a fracture for this object?
        /// </summary>
        public bool IsProcessingFracture
        {
            get { return (_runningFracture != null); }
        }

        public virtual void CopyFrom(FractureGeometry other)
        {
            InsideMaterial = other.InsideMaterial;
            OptimizeMaterialUsage = other.OptimizeMaterialUsage;
            FractureTemplate = other.FractureTemplate;
            PiecesParent = other.PiecesParent;
            FractureType = other.FractureType;
            SlicePlanes = other.SlicePlanes;
            NumFracturePieces = other.NumFracturePieces;
            NumIterations = other.NumIterations;
            EvenlySizedPieces = other.EvenlySizedPieces;
            NumGenerations = other.NumGenerations;
            FractureSize = other.FractureSize;
            UVScale = other.UVScale;
            UVBounds = other.UVBounds;
            DistributeMass = other.DistributeMass;
            SeparateDisjointPieces = other.SeparateDisjointPieces;
            RandomSeed = other.RandomSeed;
            OnFractureCompleted = other.OnFractureCompleted;
            SkipMeshCleaning = other.SkipMeshCleaning;
            _meshValidity = other.MeshValidity;
        }

        // Called when the component is added for the first time or manually reset
        protected virtual void Reset()
        {
            ResetUniqueId();
        }

        protected virtual void OnValidate()
        {
            if (Event.current != null && Event.current.type == EventType.ExecuteCommand)
            {
                switch (Event.current.commandName)
                {
                    case "Duplicate":
                    case "Paste":
                        ResetUniqueId();
                        break;

                    default:
                        break;
                }
            }
            else if (Event.current == null)
            {
                // This seems to happen when right clicking in the hierarchy tree and selecting duplicate.
                //
                // However, it is also called when hot-reloading. Therefore, we can't use it to reset
                // the unique id.
            }
        }

        private void ResetUniqueId()
        {
            // Ensure we have a new unique ID if we were copied from somewhere
            _uniqueId = Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// Initiate a fracture at the origin and does not return
        /// a handle to the async operation.
        /// </summary>
        /// <remarks>
        /// The OnFracture() callback will still fire. This method
        /// is compatible with Unity events.
        /// </remarks>
        public void FractureAndForget()
        {
            Fracture();
        }

        /// <summary>
        /// Initiate a fracture at the specified position relative to
        /// this object and does not return a handle to the async operation.
        /// </summary>
        /// <remarks>
        /// The OnFracture() callback will still fire. This method
        /// is compatible with Unity events.
        /// </remarks>
        public void FractureAndForget(Vector3 localPos)
        {
            Fracture(localPos);
        }

        /// <summary>
        /// Initiate a fracture at the origin
        /// </summary>
        /// <returns></returns>
        public AsyncFractureResult Fracture()
        {
            return FractureInternal(Vector3.zero);
        }

        /// <summary>
        /// Initiate a fracture at the specified position relative to this object.
        /// </summary>
        /// <param name="localPos"></param>
        /// <returns></returns>
        public AsyncFractureResult Fracture(Vector3 localPos)
        {
            return FractureInternal(localPos);
        }

        protected AsyncFractureResult Fracture(FractureDetails details, bool hideAfterFracture)
        {
            if (IsProcessingFracture)
            {
                return null;
            }

            if (!FillCommonDetails(details))
            {
                return null;
            }

            if (details is ShatterDetails shatterDetails)
            {
                if (shatterDetails.NumIterations == 0 || shatterDetails.NumPieces == 0)
                {
                    Logger.Log(LogLevel.UserDisplayedError, "Invalid number of pieces or iterations", gameObject);
                    return null;
                }
            }
            else if (details is SliceDetails sliceDetails)
            {
                if (sliceDetails.SlicingPlanes.Count == 0)
                {
                    Logger.Log(LogLevel.UserDisplayedError, "Must have at least one slice plane", gameObject);
                    return null;
                }
            }

            // Unassigned transforms aren't actually assigned to null by Unity, so we need check for it here.
            Transform piecesParent = (PiecesParent == null) ? null : PiecesParent;

            var fractureResult = FractureEngine.StartFracture(details, this, piecesParent, DistributeMass, hideAfterFracture);

            if (!fractureResult.IsComplete)
            {
                _runningFracture = fractureResult;
            }

            return fractureResult;
        }

        private bool FillCommonDetails(FractureDetails details)
        {
            if (FractureTemplate != null && FractureTemplate.GetComponent<MeshFilter>() == null)
            {
                Logger.Log(LogLevel.UserDisplayedError, "DinoFracture: A fracture template with a MeshFilter component is required.", gameObject);
            }

            if (details.Mesh == null)
            {
                details.Validity = _meshValidity;
                details.Mesh = GetMeshOnThisObject();
                if (details.Mesh == null)
                {
                    Logger.Log(LogLevel.UserDisplayedError, "DinoFracture: A mesh filter required if a mesh is not supplied.", gameObject);
                }
            }
            else
            {
                // Assume the user is specifying a valid geometry
                details.Validity = MeshValidity.Valid;
            }

            if (SkipMeshCleaning)
            {
                // Force valid to skip cleaning
                details.Validity = MeshValidity.Valid;
            }

            if (details.MeshScale == Vector3.zero)
            {
                details.MeshScale = transform.localScale;
            }

            // Update the output material
            details.InsideMaterialIndex = (InsideMaterial != null) ? -1 : 0;
            if (OptimizeMaterialUsage)
            {
                var renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        if (renderer.sharedMaterials[i] == InsideMaterial)
                        {
                            details.InsideMaterialIndex = i;
                            break;
                        }
                    }
                }
            }

            return true;
        }

        public void StopRunningFracture()
        {
            if (_runningFracture != null)
            {
                _runningFracture.StopFracture();
                _runningFracture = null;
            }
        }

        protected abstract AsyncFractureResult FractureInternal(Vector3 localPos);

        /// <summary>
        /// Called when this object is fractured or spawned as a result of a fracture
        /// </summary>
        /// <param name="args"></param>
        internal virtual void OnFracture(OnFractureEventArgs args)
        {
            if (args.OriginalObject == this)
            {
                _runningFracture = null;

                // Invoked in editor (via pre-fracture) & runtime
                if (OnFractureCompleted != null)
                {
                    OnFractureCompleted.Invoke(args);
                }
            }
        }

        /// <summary>
        /// This will create a valid slice plane for slicing a mesh.
        /// </summary>
        /// <remarks>
        /// While valid, it is not intended to be displayed in the editor and
        /// is meant for runtime use.
        /// </remarks>
        public static SlicePlaneSerializable CreateSlicePlane(Plane worldPlane, Transform targetGameObject)
        {
            var localPlane = worldPlane;
            if (targetGameObject != null)
            {
                localPlane = targetGameObject.worldToLocalMatrix.TransformPlane(localPlane);
            }

            var normal = localPlane.normal;
            var pos = normal * -localPlane.distance;

            SlicePlaneSerializable ret = new SlicePlaneSerializable();
            ret.Position = pos;
            ret.Rotation = Quaternion.LookRotation(normal);
            ret.Scale = 1.0f;

            return ret;
        }

        /// <summary>
        /// This is called automatically when viewing the component in the inspector.
        /// However, it should be called whenever the mesh changes through other means.
        /// </summary>
        public MeshTopologyError CheckMeshValidity()
        {
#if UNITY_EDITOR
            var oldValidity = _meshValidity;
#endif

            UnityMesh mesh = GetMeshOnThisObject(staticMesh: true);
            if (mesh == null || _lastMeshId != mesh.GetInstanceID())
            {
                _lastMeshId = (mesh != null) ? mesh.GetInstanceID() : 0;
                _meshValidity = FractureBuilder.ValidateMesh(mesh, out _topologyErrors);
            }

#if UNITY_EDITOR
            if (oldValidity != _meshValidity)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
            return _topologyErrors;
        }

        /// <summary>
        /// This will force the geometry to pass all validity checks.
        /// 
        /// This method can be useful to call in very custom scenarios of
        /// fracturing through script when you know the mesh you are
        /// passing in is valid.
        /// </summary>
        public void ForceValidGeometry()
        {
            _meshValidity = MeshValidity.Valid;
        }

        /// <summary>
        /// Returns any bad mesh edges. Used by the editor script for debugging.
        /// </summary>
        public List<EdgeError> GetMeshEdgeErrors()
        {
            return FractureBuilder.GetMeshEdgeErrors(gameObject, GetMeshOnThisObject(staticMesh: true));
        }

        internal UnityMesh GetMeshOnThisObject(bool staticMesh = false)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                return meshFilter.sharedMesh;
            }
            else
            {
                SkinnedMeshRenderer skinnedRenderer = GetComponent<SkinnedMeshRenderer>();
                if (skinnedRenderer != null)
                {
                    if (staticMesh)
                    {
                        return skinnedRenderer.sharedMesh;
                    }
                    else
                    {
                        UnityMesh mesh = new UnityMesh();

                        // We want to use the scaling applied only to the skinned mesh (usually 1x)
                        // and not the "sum" scaling from the parent tree. This ensures we do not
                        // double scale the fracture pieces when the fracture root is under the
                        // scaling root of the skinned mesh.
#if UNITY_2020_2_OR_NEWER
                        skinnedRenderer.BakeMesh(mesh, useScale: true);
#else
                        skinnedRenderer.BakeMesh(mesh);
#endif

                        return mesh;
                    }
                }
            }

            return null;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#pragma warning disable 0618
            if (FractureRadius != float.MinValue)
            {
                FractureSize = new SizeSerializable() { Space = SizeSpace.RelativeToBounds, Value = FractureRadius * 2.0f };
                FractureRadius = float.MinValue;
            }
#pragma warning restore 0618
        }
    }
}
