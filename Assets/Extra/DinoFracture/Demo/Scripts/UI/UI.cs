//#define SLICE_FORCE_INV_PROPORTIONAL_TO_MASS
//#define ALLOW_SCREENSHOT_MODE

using DinoFracture;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace DinoFractureDemo
{
    [Serializable]
    public class BoolEvent : UnityEvent<bool>
    { }

    [Serializable]
    public class PanelDescriptionEvent : UnityEvent<PanelDescriptionAsset>
    { }

    [Serializable]
    public class ImportantGameObjectsEvent : UnityEvent<ImportantGameObjectData[]>
    { }

    [Serializable]
    public class PanelData
    {
        public bool CanSlice;

        public PanelDescriptionAsset PanelDescription;

        public ImportantGameObjectData[] ImportantGameObjects;
    }

    public class UI : MonoBehaviour
    {
        public GameObject CannonBallTemplate;

        public PanelData[] PanelData;
        public int StartPanelIndex = 0;

        public float PanelWidth;
        public AnimationCurve MoveCurve;
        public float MoveTime;

        public GameObject WebGLNoThreadsMessage;

        public BoolEvent HaveMorePanelsBackwardChanged;
        public BoolEvent HaveMorePanelsForwardChanged;
        public BoolEvent CanSliceChanged;
        public BoolEvent IsPanelDescriptionVisibleChanged;
        public PanelDescriptionEvent PanelDescriptionChanged;
        public BoolEvent IsImportantGameObjectsVisibleChanged;
        public ImportantGameObjectsEvent ImportantGameObjectsChanged;

        private int _curPanel;

        private void Start()
        {
            _curPanel = StartPanelIndex;

            NotifyCurrentState();

            // Resolve all the game object paths. We do this now because
            // resetting the scene can invalidate the game objects.
            for (int i = 0; i < PanelData.Length; i++)
            {
                if (PanelData[i].ImportantGameObjects != null)
                {
                    for (int j = 0; j < PanelData[i].ImportantGameObjects.Length; j++)
                    {
                        var data = PanelData[i].ImportantGameObjects[j];

                        data.ResolveGameObjectPath();

                        if (string.IsNullOrEmpty(data.Description) && data.GameObject.GetComponent<FractureGeometry>() != null)
                        {
                            data.Description = "Fracturing object";
                        }

                        // Recursively capture fracture templates
                        FractureGeometry fractureGeo = data.GameObject.GetComponent<FractureGeometry>();
                        if (fractureGeo != null && fractureGeo.FractureTemplate != null && fractureGeo.FractureTemplate.name != fractureGeo.name)
                        {
                            var templateItem = new ImportantGameObjectData() { GameObject = fractureGeo.FractureTemplate, Description = "Fracture template object" };
                            List<ImportantGameObjectData> newList = PanelData[i].ImportantGameObjects.ToList();
                            newList.Insert(j + 1, templateItem);
                            PanelData[i].ImportantGameObjects = newList.ToArray();
                        }
                    }
                }
            }
        }

#if ALLOW_SCREENSHOT_MODE
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetPlayground();
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                var canvas = transform.Find("Canvas/VisibleUI");
                if (canvas != null)
                {
                    canvas.gameObject.SetActive(!canvas.gameObject.activeSelf);
                }
            }
        }
#endif

        public void ResetPlayground()
        {
            GameRoot.Instance.Reset();
        }

        public void MoveToPrevPanel()
        {
            if (_curPanel > 0)
            {
                _curPanel--;

                StartMoveCameraToCurrentPanel();
            }
        }

        public void MoveToNextPanel()
        {
            if (_curPanel < PanelData.Length - 1)
            {
                _curPanel++;

                StartMoveCameraToCurrentPanel();
            }
        }

        public void FireCannonBall(Ray ray)
        {
            GameObject cannonBall = Instantiate(CannonBallTemplate, ray.origin + ray.direction.normalized * 0.1f, Quaternion.identity, GameRoot.Instance.Main.transform.Find("CannonBallParent"));
            cannonBall.GetComponent<Rigidbody>().velocity = ray.direction.normalized * 25.0f;
        }

        public void SliceScene(Vector2 startScreenPos, Vector2 endScreenPos, float normalStrength, float forwardStrength)
        {
            Vector2 screenCenterPos = (startScreenPos + endScreenPos) * 0.5f;

            Vector3 startWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(startScreenPos.x, startScreenPos.y, 5.0f));
            Vector3 endWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(endScreenPos.x, endScreenPos.y, 5.0f));

            Ray startRay = Camera.main.ScreenPointToRay(startScreenPos);
            Ray endRay = Camera.main.ScreenPointToRay(endScreenPos);

            Ray centerRay = Camera.main.ScreenPointToRay(screenCenterPos);

            Vector3 normal = Vector3.Cross(startRay.direction, endRay.direction).normalized;
            Quaternion rotation = Quaternion.LookRotation(normal, Vector3.Cross(normal, Camera.main.transform.forward));

            Vector3 boxCastStart = Camera.main.ScreenToWorldPoint(new Vector3(screenCenterPos.x, screenCenterPos.y, 1.0f));

            Vector3 halfBoxSize = new Vector3(0.05f, (endWorldPos - startWorldPos).magnitude * 0.5f, 0.05f);

            Plane slicePlane = new Plane(normal, boxCastStart);

            var rayHits = Physics.BoxCastAll(boxCastStart, halfBoxSize, centerRay.direction, rotation, 20.0f);
            if (rayHits != null)
            {
                for (int i = 0; i < rayHits.Length; i++)
                {
                    FractureGeometry fractureGeo = rayHits[i].collider.GetComponent<FractureGeometry>();
                    if (fractureGeo != null)
                    {
                        fractureGeo.SlicePlanes = new FractureGeometry.SlicePlaneSerializable[]
                        {
                            FractureGeometry.CreateSlicePlane(slicePlane, fractureGeo.transform)
                        };
                        fractureGeo.FractureType = FractureType.Slice;

                        var fractureRes = fractureGeo.Fracture();
                        if (fractureRes != null)
                        {
                            Vector3 hitPoint = rayHits[i].point;
                            fractureRes.OnFractureComplete += (args) => ApplySliceImpulse(args, slicePlane, hitPoint, normalStrength, forwardStrength);
                        }
                    }
                }
            }
        }

        public void ToggleFullScreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
        }

        private void UpdateWebGLNoThreadsMessage()
        {
#if !UNITY_WEBGL
            WebGLNoThreadsMessage.SetActive(false);
#else
            WebGLNoThreadsMessage.SetActive(_curPanel == 0);
#endif
        }

        private void ApplySliceImpulse(OnFractureEventArgs args, in Plane slicePlane, in Vector3 hitPoint, float normalStrength, float forwardStrength)
        {
            if (!args.IsValid)
            {
                return;
            }

#if SLICE_FORCE_INV_PROPORTIONAL_TO_MASS
            Rigidbody originalRB = args.OriginalObject.GetComponent<Rigidbody>();
            float originalMass = (originalRB != null) ? originalRB.mass : 1.0f;
#endif

            Vector3 constantSliceForce = Camera.main.transform.forward * forwardStrength;

            // Because the rigid body was just created, we need to sync
            // the rigid body properties with the object. Failing to call this will result
            // in improper forces being applied because the rigid body will think it's
            // at the world origin.
            Physics.SyncTransforms();

            for (int i = 0; i < args.FracturePiecesRootObject.transform.childCount; i++)
            {
                var child = args.FracturePiecesRootObject.transform.GetChild(i);
                var rb = child.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    Vector3 childPos = rb.worldCenterOfMass;

                    Vector3 closestPointOnPlane = hitPoint;
                    Vector3 force = (childPos - closestPointOnPlane).normalized * normalStrength + constantSliceForce;

#if SLICE_FORCE_INV_PROPORTIONAL_TO_MASS
                    force *= originalMass * 0.5f;
#endif

                    Vector3 forcePos = Vector3.Lerp(closestPointOnPlane, childPos, 0.4f);

#if SLICE_FORCE_INV_PROPORTIONAL_TO_MASS
                    rb.AddForceAtPosition(force, forcePos, ForceMode.Force);
#else
                    rb.AddForceAtPosition(force, forcePos, ForceMode.Acceleration);
#endif
                }
            }
        }

        private void StartMoveCameraToCurrentPanel()
        {
            StopAllCoroutines();

            StartCoroutine(MoveCameraToCurrentPanelCoroutine());
        }

        private IEnumerator MoveCameraToCurrentPanelCoroutine()
        {
            Vector3 startCameraPos = Camera.main.transform.position;
            Vector3 endCameraPos = startCameraPos;
            endCameraPos.x = PanelWidth * _curPanel;

            float time = 0.0f;

            while (time < MoveTime)
            {
                float t = Mathf.Clamp01(time / MoveTime);

                t = MoveCurve.Evaluate(t);

                Camera.main.transform.position = Vector3.Lerp(startCameraPos, endCameraPos, t);

                time += Time.deltaTime;
                yield return null;
            }

            Camera.main.transform.position = endCameraPos;

            NotifyCurrentState();
        }

        private void NotifyCurrentState()
        {
            UpdateWebGLNoThreadsMessage();

            HaveMorePanelsBackwardChanged.Invoke(_curPanel > 0);
            HaveMorePanelsForwardChanged.Invoke(_curPanel < PanelData.Length - 1);
            CanSliceChanged.Invoke(PanelData[_curPanel].CanSlice);
            IsPanelDescriptionVisibleChanged.Invoke(PanelData[_curPanel].PanelDescription != null);
            IsImportantGameObjectsVisibleChanged.Invoke(PanelData[_curPanel].ImportantGameObjects != null && PanelData[_curPanel].ImportantGameObjects.Length > 0);

            if (PanelData[_curPanel].PanelDescription != null)
            {
                PanelDescriptionChanged.Invoke(PanelData[_curPanel].PanelDescription);
            }

            if (PanelData[_curPanel].ImportantGameObjects != null && PanelData[_curPanel].ImportantGameObjects.Length > 0)
            {
                ImportantGameObjectsChanged.Invoke(PanelData[_curPanel].ImportantGameObjects);
            }
        }
    }
}