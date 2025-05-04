using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DinoFracture
{
    /// <summary>
    /// Apply this component to any game object you wish to pre-fracture.
    /// Pre-fracturing is a way of baking fracture pieces into the scene.
    /// Each time the object is fractured, the same set of pieces will
    /// activate.  This is very useful when creating a large number of
    /// pieces or high poly meshes, which would be too slow to create at
    /// runtime.  The pieces will be in the scene as a disabled root object
    /// with piece children.  When the object is fractured, those pieces
    /// will activate.
    /// </summary>
    public class PreFracturedGeometry : FractureGeometry
    {
        /// <summary>
        /// A reference to the root of the pre-fractured pieces.
        /// This is not normally set manually.  Instead, you press
        /// the “Create Fractures” button in the inspector window
        /// to generate the fracture immediately.  
        /// </summary>
        /// <remarks>The “Create Fractures” button is only intended to be used in edit mode; not game mode.</remarks>
        [UnityEngine.Tooltip("A reference to the root of the pre-fractured pieces. This is not normally set manually.  Instead, you press the “Create Fractures” button in the inspector window to generate the fracture immediately.  ")]
        public GameObject GeneratedPieces;

        /// <summary>
        /// The encapsulating bounds of the entire set of pieces.  In local space.
        /// </summary>
        [UnityEngine.Tooltip("The encapsulating bounds of the entire set of pieces.  In local space.")]
        public Bounds EntireMeshBounds;

        public PreFracturedGeometry()
        {
            SeparateDisjointPieces = true;
        }

        private void Start()
        {
            Prime();
        }

        /// <summary>
        /// Primes the pre-fractured pieces when the game starts by
        /// activating them and then deactivating them.  This avoids
        /// a large delay on fracture if there are a lot of rigid bodies.
        /// </summary>
        public void Prime()
        {
            if (GeneratedPieces != null)
            {
                bool activeSelf = gameObject.activeSelf;
                gameObject.SetActive(false);

                GeneratedPieces.SetActive(true);
                GeneratedPieces.SetActive(false);

                gameObject.SetActive(activeSelf);
            }
        }

        public AsyncFractureResult GenerateFractureMeshes()
        {
            return GenerateFractureMeshes(Vector3.zero);
        }

        public AsyncFractureResult GenerateFractureMeshes(Vector3 localPoint)
        {
            if (IsProcessingFracture)
            {
                Logger.Log(LogLevel.UserDisplayedError, "DinoFracture: Cannot start a fracture while a fracture is running.", gameObject);
                return null;
            }

            if (Application.isPlaying)
            {
                Logger.Log(LogLevel.UserDisplayedWarning, "DinoFracture: Creating pre-fractured pieces at runtime. This can be slow if there a lot of pieces.", gameObject);
            }

            // Don't clear the pieces when fracturing at edit time. The editor
            // script will handle changing out the piece. This allows us to save
            // the old results on failure and also not mark this game object as
            // dirty if the values don't change.
            if (Application.isPlaying)
            {
                ClearGeneratedPieces();
            }

            AsyncFractureResult ret = null;

            if (FractureType == FractureType.Shatter)
            {
                ShatterDetails details = new ShatterDetails();
                details.NumPieces = NumFracturePieces;
                details.NumIterations = NumIterations;
                details.EvenlySizedPieces = EvenlySizedPieces;
                details.UVScale = UVScale;
                details.UVBounds = UVBounds;
                details.IssueResolution = FractureIssueResolution.ReplaceMeshCollider;
                details.Asynchronous = !Application.isPlaying && !FractureEngineBase.ForceSynchronousPreFractureInEditor;  // Async in editor to prevent hangs, sync while playing
                details.FractureCenter = localPoint;
                details.FractureSize = FractureSize;
                details.SeparateDisjointPieces = SeparateDisjointPieces;
                details.RandomSeed = RandomSeed;

                ret = Fracture(details, false);
            }
            else if (FractureType == FractureType.Slice)
            {
                SliceDetails details = new SliceDetails();
                details.SlicingPlanes.AddRange(SlicePlanes.Select(p => p.ToSlicePlane()));
                details.UVScale = UVScale;
                details.UVBounds = UVBounds;
                details.IssueResolution = FractureIssueResolution.ReplaceMeshCollider;
                details.Asynchronous = !Application.isPlaying && !FractureEngineBase.ForceSynchronousPreFractureInEditor;  // Async in editor to prevent hangs, sync while playing
                details.SeparateDisjointPieces = SeparateDisjointPieces;

                ret = Fracture(details, false);
            }
            else
            {
                Logger.Log(LogLevel.Error, "Invalid fracture type", gameObject);
            }

            return ret;
        }

        public void ClearGeneratedPieces()
        {
            if (GeneratedPieces != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(GeneratedPieces);
                }
                else
                {
                    DestroyImmediate(GeneratedPieces);
                }

                GeneratedPieces = null;
            }
        }

        protected override AsyncFractureResult FractureInternal(Vector3 localPos)
        {
            AsyncFractureResult result;

            if (gameObject.activeSelf)
            {
                if (GeneratedPieces == null)
                {
                    result = GenerateFractureMeshes(localPos);
                }
                else
                {
                    result = new AsyncFractureResult();
                    result.SetResult(new OnFractureEventArgs(this, EntireMeshBounds, GeneratedPieces, null));
                }
            }
            else
            {
                result = new AsyncFractureResult();
                result.SetResult(new OnFractureEventArgs(this, new Bounds(), null, null));
            }

            return result;
        }

        private void EnableFracturePieces()
        {
            GeneratedPieces.SetActive(true);
        }

        internal override void OnFracture(OnFractureEventArgs args)
        {
            if (Application.isPlaying)
            {
                if (args.IsValid)
                {
                    GeneratedPieces = args.FracturePiecesRootObject;
                    EntireMeshBounds = args.OriginalMeshBounds;

                    EnableFracturePieces();
                }

                // Always disable because the game might require
                // the mesh be gone and we don't want to interfere with that.
                gameObject.SetActive(false);
            }
            else
            {
                // The editor script will take care of assignment
                // of the new mesh
            }

            base.OnFracture(args);
        }
    }
}