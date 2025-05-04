#define LOOKUP_BY_NAME

using System;
using System.Collections.Generic;
using UnityEngine;

namespace DinoFracture
{
    public class ChipOnFracture : MonoBehaviour
    {
        struct ListReference<T> : IDisposable
        {
            private List<T> _ref;

            public void Dispose()
            {
                _ref.Clear();
            }

            public static ListReference<T> Get(ref List<T> item)
            {
                ListReference<T> ret = new ListReference<T>();
                ret._ref = item;
                return ret;
            }
        }

        private static List<FracturedObject> _sChippedObjectList = new List<FracturedObject>();
        private static List<FracturedObject> _sUnchippedObjectList = new List<FracturedObject>();
        private static List<FracturedObject> _sRootObjectList = new List<FracturedObject>();

        /// <summary>
        /// The radius, in world space units, around the point of fracture that
        /// we will consider fracture pieces to be chipped off.
        /// </summary>
        /// <remarks>
        /// A fracture piece must be fully contained within the radius to be
        /// considered chipped.
        /// 
        /// If this is set to <= 0.0, the FractureSize on the FractureGeometry will
        /// be used.
        /// </remarks>
        [UnityEngine.Tooltip("The radius, in world space units, around the point of fracture that we will consider fracture pieces to be chipped off.\r\n\r\nA fracture piece must be fully contained within the radius to be considered chipped.\r\n\r\nIf this is set to <= 0.0, the FractureSize on the FractureGeometry will be used.")]
        public float Radius;

        /// <summary>
        /// Add this component to child items if not already present in the FractureTemplate.
        /// It is recommended to keep this true unless you explicitly add it to the FractureTemplate.
        /// </summary>
        [UnityEngine.Tooltip("Add this component to child items if not already present in the FractureTemplate. It is recommended to keep this true unless you explicitly add it to the FractureTemplate.")]
        public bool EnsureChildComponents = true;

        public void CopyFrom(ChipOnFracture other)
        {
            Radius = other.Radius;
        }

        /// <summary>
        /// OnPostFracture is called by the fracture engine after all the regular OnFracture
        /// callbacks have fired. Because this method changes the generated fracture tree,
        /// it's important not to intefere with callbacks that might assume as certain
        /// tree structure.
        /// </summary>
        /// <param name="args"></param>
        private void OnPostFracture(OnFractureEventArgs args)
        {
            if (args.OriginalObject.gameObject == gameObject)
            {
                using (ListReference<FracturedObject>.Get(ref _sUnchippedObjectList))
                using (ListReference<FracturedObject>.Get(ref _sChippedObjectList))
                {
                    if (args.FractureDetails is ShatterDetails shatterDetails)
                    {
                        GetChippedObjects(args, shatterDetails);

                        ProcessChippedObjects(args, shatterDetails);
                    }
                    else if (args.FractureDetails is SliceDetails sliceDetails)
                    {
                        GetChippedObjects(args, sliceDetails);

                        ProcessChippedObjects(args, sliceDetails);
                    }

                    if (args.FracturePiecesRootObject.transform.childCount == 0)
                    {
                        Destroy(args.FracturePiecesRootObject);
                    }
                }
            }
        }

        #region Shatter

        private void GetChippedObjects(OnFractureEventArgs args, ShatterDetails shatterDetails)
        {
            if (shatterDetails.FractureSize.Value > 0)
            {
                Vector3 fractureCenterWS = args.FracturePiecesRootObject.transform.TransformPoint(shatterDetails.FractureCenter);
                float radius = (Radius > 0) ? Radius : shatterDetails.FractureSize.GetWorldSpaceSize(args.OriginalMeshBounds.size).magnitude * 0.5f;

                for (int i = 0; i < args.FracturePiecesRootObject.transform.childCount; i++)
                {
                    var childTrans = args.FracturePiecesRootObject.transform.GetChild(i);
                    if (childTrans.TryGetComponent(out FracturedObject fo))
                    {
                        if (childTrans.TryGetComponent(out Collider childCollider))
                        {
                            Vector3 colliderCenter = childTrans.position;
                            float colliderRadius = childCollider.bounds.extents.magnitude;

                            if ((fractureCenterWS - colliderCenter).magnitude + colliderRadius > radius)
                            {
                                _sUnchippedObjectList.Add(fo);
                                continue;
                            }
                        }

                        // Chipped
                        _sChippedObjectList.Add(fo);
                    }
                }
            }
        }

        private void ProcessChippedObjects(OnFractureEventArgs args, ShatterDetails shatterDetails)
        {
            if (_sUnchippedObjectList.Count == 0)
            {
                return;
            }

            ChippedFractureRoot root = GetComponentInParent<ChippedFractureRoot>();
            if (root == null)
            {
                root = CreateNewRoot(null, 0, args);
            }

            for (int i = 0; i < _sUnchippedObjectList.Count; i++)
            {
                FracturedObject fractureChild = _sUnchippedObjectList[i];

                AddChildToRoot(fractureChild, root, args);
            }

            RecalculateRootProperties(root.GetComponent<FracturedObject>());
        }

        #endregion

        #region Slice

        private void GetChippedObjects(OnFractureEventArgs args, SliceDetails sliceDetails)
        {
            // Add all the newly sliced objects
            for (int i = 0; i < args.FracturePiecesRootObject.transform.childCount; i++)
            {
                FracturedObject fo = args.FracturePiecesRootObject.transform.GetChild(i).GetComponent<FracturedObject>();
                if (fo != null)
                {
                    _sUnchippedObjectList.Add(fo);
                }
            }

            // We also need to re-evaluate the existing pieces that have not been fractured
            ChippedFractureRoot curRoot = GetComponentInParent<ChippedFractureRoot>();
            if (curRoot != null)
            {
                for (int i = 0; i < curRoot.transform.childCount; i++)
                {
                    var child = curRoot.transform.GetChild(i);
                    if (child.gameObject.activeSelf && child.gameObject != gameObject)
                    {
                        FracturedObject childFracturedObject = child.GetComponent<FracturedObject>();
                        if (childFracturedObject != null)
                        {
                            _sUnchippedObjectList.Add(childFracturedObject);
                        }
                    }
                }
            }
        }

        private void ProcessChippedObjects(OnFractureEventArgs args, SliceDetails sliceDetails)
        {
            if (_sUnchippedObjectList.Count == 0)
            {
                return;
            }

            using (ListReference<FracturedObject>.Get(ref _sRootObjectList))
            {
                ChippedFractureRoot curRoot = GetComponentInParent<ChippedFractureRoot>();

                for (int i = 0; i < _sUnchippedObjectList.Count; i++)
                {
                    FracturedObject fractureChild = _sUnchippedObjectList[i];
                    int childId = GenerateFracturedChildSubId(fractureChild, sliceDetails);

                    ChippedFractureRoot root = GetChipRoot(curRoot, sliceDetails.FractureFrame, childId);
                    if (root == null)
                    {
                        root = CreateNewRoot(curRoot, childId, args);
                    }

                    AddChildToRoot(fractureChild, root, args);

                    FracturedObject rootFracturedObj = root.GetComponent<FracturedObject>();
                    if (!_sRootObjectList.Contains(rootFracturedObj))
                    {
                        _sRootObjectList.Add(rootFracturedObj);
                    }
                }

                for (int i = 0; i < _sRootObjectList.Count; i++)
                {
                    RecalculateRootProperties(_sRootObjectList[i]);
                }
            }
        }

        private int GenerateFracturedChildSubId(FracturedObject fractureChild, SliceDetails sliceDetails)
        {
            // Create an ID based on which side of the planes we are on
            int id = 0;
            for (int i = 0; i < sliceDetails.SlicingPlanes.Count; i++)
            {
                var localSlicePlane = sliceDetails.SlicingPlanes[i].ToPlane();
                var worldSlicePlane = transform.localToWorldMatrix.TransformPlane(localSlicePlane);

                bool inFront = worldSlicePlane.GetSide(fractureChild.transform.position);
                if (inFront)
                {
                    id |= (1 << i);
                }
            }
            return id;
        }

        private ChippedFractureRoot GetChipRoot(ChippedFractureRoot curRoot, int id, int subId)
        {
            if (curRoot == null)
            {
                return null;
            }

            if (subId == 0 && curRoot.Id != id)
            {
                curRoot.Id = id;
                curRoot.SubId = 0;
                curRoot.name = GetRootName(curRoot, id, 0);
                return curRoot;
            }

#if LOOKUP_BY_NAME
            var curRootParent = curRoot.transform.parent;
            if (curRootParent == null)
            {
                var foundChild = curRootParent.Find(GetRootName(curRoot, id, subId));
                if (foundChild != null)
                {
                    return foundChild.GetComponent<ChippedFractureRoot>();
                }
            }
            else
            {
                var foundGO = GameObject.Find(GetRootName(curRoot, id, subId));
                if (foundGO != null)
                {
                    return foundGO.GetComponent<ChippedFractureRoot>();
                }
            }
#else
            // Find sibilings of the cur root that have the same id
            var curRootParent = curRoot.transform.parent;
            if (curRootParent != null)
            {
                for (int i = 0; i < curRootParent.childCount; i++)
                {
                    ChippedFractureRoot root = curRootParent.GetChild(i).GetComponent<ChippedFractureRoot>();
                    if (root != null && root.Id == id && root.SubId == subId)
                    {
                        return root;
                    }
                }
            }
            else
            {
                var roots = FindObjectsOfType<ChippedFractureRoot>(true);
                for (int i = 0; i < roots.Length; i++)
                {
                    if (roots[i].transform.parent == null && roots[i].Id == id && roots[i].SubId == subId)
                    {
                        return roots[i];
                    }
                }
            }
#endif

            return null;
        }

        #endregion

        #region Common

        private void AddChildToRoot(FracturedObject unchippedChild, ChippedFractureRoot root, OnFractureEventArgs args)
        {
            var childGO = unchippedChild.gameObject;
            childGO.transform.SetParent(root.transform, true);
            Destroy(childGO.GetComponent<Rigidbody>());

            RuntimeFracturedGeometry fractureGeom = unchippedChild.GetComponent<RuntimeFracturedGeometry>();
            if (fractureGeom == null && EnsureChildComponents)
            {
                fractureGeom = childGO.AddComponent<RuntimeFracturedGeometry>();
                fractureGeom.CopyFrom(args.OriginalObject);
                fractureGeom.ForceValidGeometry();
            }
            else if (fractureGeom != null)
            {
                // Allow us to fracture again as we haven't actually been removed
                fractureGeom.NumGenerations = args.OriginalObject.NumGenerations;

                fractureGeom.FractureSize = args.OriginalObject.FractureSize;
                fractureGeom.NumFracturePieces = args.OriginalObject.NumFracturePieces;
                fractureGeom.NumIterations = args.OriginalObject.NumIterations;

                fractureGeom.ForceValidGeometry();
            }

            if (EnsureChildComponents)
            {
                if (TryGetComponent(out FractureOnCollision thisColComp) && childGO.GetComponent<FractureOnCollision>() == null)
                {
                    FractureOnCollision fractureComp = childGO.AddComponent<FractureOnCollision>();
                    fractureComp.CopyFrom(thisColComp);
                }

                if (TryGetComponent(out FractureOnParticleCollision thisParticleColComp) && childGO.GetComponent<FractureOnParticleCollision>() == null)
                {
                    FractureOnParticleCollision childComp = childGO.AddComponent<FractureOnParticleCollision>();
                    childComp.CopyFrom(thisParticleColComp);
                }

                if (childGO.GetComponent<ChipOnFracture>() == null)
                {
                    ChipOnFracture chipComp = childGO.AddComponent<ChipOnFracture>();
                    chipComp.CopyFrom(this);
                }
            }
        }

        private void RecalculateRootProperties(FracturedObject rootFracturedObject)
        {
            int childCount = 0;
            float totalMass = 0.0f;
            float totalVolume = 0.0f;
            Vector3 centerOfMass = Vector3.zero;

            for (int i = 0; i < rootFracturedObject.transform.childCount; i++)
            {
                var child = rootFracturedObject.transform.GetChild(i);
                if (child.gameObject.activeSelf && child.gameObject != gameObject)
                {
                    FracturedObject childFracturedObject = child.GetComponent<FracturedObject>();

                    float childVolume = childFracturedObject.ThisMass;
                    float childMass = childFracturedObject.ThisVolume;

                    totalMass += childMass;
                    totalVolume += childVolume;

                    centerOfMass += child.localPosition * childMass;

                    childCount++;
                }
            }

            if (childCount > 0)
            {
                centerOfMass *= (1.0f / totalMass);

                Rigidbody rootRigidBody = rootFracturedObject.GetComponent<Rigidbody>();
                rootRigidBody.mass = totalMass;
                rootRigidBody.centerOfMass = centerOfMass;

                rootFracturedObject.TotalMass = totalMass;
                rootFracturedObject.ThisMass = totalMass;
                rootFracturedObject.TotalVolume = totalVolume;
                rootFracturedObject.ThisVolume = totalVolume;
            }
        }

        private ChippedFractureRoot CreateNewRoot(ChippedFractureRoot curRoot, int subId, OnFractureEventArgs args)
        {
            Transform newRootParent;

            if (curRoot == null)
            {
                newRootParent = args.FracturePiecesRootObject.transform;
            }
            else
            {
                newRootParent = curRoot.transform.parent;
            }

            GameObject unchippedGO = new GameObject(GetRootName(curRoot, args.FractureDetails.FractureFrame, subId));
            unchippedGO.transform.SetParent(newRootParent, false);

            unchippedGO.transform.position = transform.position;
            unchippedGO.transform.localRotation = Quaternion.identity;
            unchippedGO.transform.localScale = Vector3.one;

            var rootRigidBody = unchippedGO.AddComponent<Rigidbody>();

            // Keep us frozen if the original object was
            var origRigidBody = args.OriginalObject.GetComponent<Rigidbody>();
            if (origRigidBody != null)
            {
                rootRigidBody.isKinematic = origRigidBody.isKinematic;
            }

            var chippedRoot = unchippedGO.AddComponent<ChippedFractureRoot>();
            chippedRoot.Id = args.FractureDetails.FractureFrame;
            chippedRoot.SubId = subId;

            unchippedGO.AddComponent<FracturedObject>();

            return chippedRoot;
        }

        private string GetRootName(ChippedFractureRoot curRoot, int id, int subId)
        {
            if (curRoot == null)
            {
                return $"UnchippedRoot 0 {id} {subId}";
            }
            return $"UnchippedRoot {curRoot.GetInstanceID()} {id} {subId}";
        }

        #endregion
    }
}