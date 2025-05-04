using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DinoFractureDemo
{
    [Serializable]
    public class FireCannonBallEvent : UnityEvent<Ray>
    {
    }

    [Serializable]
    public class SliceSceneEvent : UnityEvent<Vector2, Vector2, float, float>
    {
    }

    public class SceneInteractionWidget : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private const float cDragMovemoventThreshold = 70.0f;
        private const float cDragHeldTimeThreshold = 0.2f;
        private const float cSliceStrengthIncreaseRate = 3.0f;
        private const float cSliceStrengthHoldTime = 0.65f;
        private const float cMaxSliceNormalStrength = 60.0f;
        private const float cMaxSliceForwardStrength = cMaxSliceNormalStrength * 1.75f;
        private const float cSliceVisualThickness = 3.0f;

        enum InputState
        {
            Idle,
            PointerDown,
            SliceDragging,
        }

        private static readonly Vector2 cMinPosition = new Vector2(float.MinValue, float.MinValue);

        private InputState _inputState = InputState.Idle;

        private float _dragStartTime;
        private Vector2 _dragStartPos = cMinPosition;
        private Vector2 _dragCurPos = cMinPosition;

        private Image _dragVisual;

        // [0..1]
        private float _sliceStrength;

        private float _timerTime;

        private Canvas _canvas;

        private bool _sliceAvailable = true;

        private bool InSliceDragRange
        {
            get
            {
                bool timeThreshold = (Time.realtimeSinceStartup - _dragStartTime) >= cDragHeldTimeThreshold;
                bool movementThreshold = ((_dragStartPos - _dragCurPos).sqrMagnitude / (_canvas.scaleFactor * _canvas.scaleFactor)) > (cDragMovemoventThreshold * cDragMovemoventThreshold);

                return timeThreshold && movementThreshold;
            }
        }

        public FireCannonBallEvent FireCannonBall;
        public SliceSceneEvent SliceScene;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_inputState == InputState.PointerDown)
            {
                var ray = Camera.main.ScreenPointToRay(eventData.position);
                FireCannonBall.Invoke(ray);
            }
            else if (_inputState == InputState.SliceDragging)
            {
                _dragCurPos = eventData.position;

                // If we released back where we started, don't slice
                if (InSliceDragRange)
                {
                    // Slice the scene
                    SliceScene.Invoke(_dragStartPos, eventData.position, _sliceStrength * cMaxSliceNormalStrength, _sliceStrength * cMaxSliceForwardStrength);
                }
            }

            _inputState = InputState.Idle;

            UpdateSliceVisuals();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (_inputState == InputState.Idle)
                {
                    _inputState = InputState.PointerDown;

                    _dragStartTime = Time.realtimeSinceStartup;
                    _dragStartPos = eventData.position;
                    _dragCurPos = eventData.position;
                    UpdateSliceVisuals();
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right && _sliceAvailable)
            {
                if (_inputState == InputState.Idle)
                {
                    _dragStartPos = eventData.position;
                }

                _sliceStrength = 0.0f;
                _timerTime = cSliceStrengthHoldTime;

                _dragCurPos = eventData.position;
                _inputState = InputState.SliceDragging;
                UpdateSliceVisuals();
            }
        }


        public void OnDrag(PointerEventData eventData)
        {
            _dragCurPos = eventData.position;

            if (_sliceAvailable && _inputState == InputState.PointerDown && InSliceDragRange)
            {
                _sliceStrength = 0.0f;
                _timerTime = cSliceStrengthHoldTime;

                _inputState = InputState.SliceDragging;
            }

            _dragCurPos = eventData.position;
            UpdateSliceVisuals();
        }

        public void OnPointerUp(PointerEventData eventData)
        {

        }

        public void OnCanSliceChanged(bool canSlice)
        {
            _sliceAvailable = canSlice;

            if (_inputState == InputState.SliceDragging)
            {
                _inputState = InputState.PointerDown;
            }
        }

        private void UpdateSliceVisuals()
        {
            if (_inputState == InputState.SliceDragging && InSliceDragRange)
            {
                EnsureDragVisual();

                Vector3 dir = (_dragCurPos - _dragStartPos);
                float len = dir.magnitude / _canvas.scaleFactor;
                dir = dir.normalized;

                float angle = Mathf.Atan2(dir.y, dir.x);

                RectTransform visualTrans = (RectTransform)_dragVisual.transform;
                visualTrans.SetPositionAndRotation(_dragStartPos, Quaternion.Euler(0.0f, 0.0f, angle * Mathf.Rad2Deg));

                visualTrans.sizeDelta = new Vector2(len, cSliceVisualThickness * (_sliceStrength + 1.0f));
            }
            else
            {
                DestroyDragVisual();
            }
        }

        private void EnsureDragVisual()
        {
            if (_dragVisual == null)
            {
                GameObject child = new GameObject("Drag Visual");
                child.transform.SetParent(transform, false);

                _dragVisual = child.AddComponent<Image>();

                RectTransform visualTrans = (RectTransform)child.transform;
                visualTrans.pivot = new Vector2(0.0f, 0.5f);
            }
        }

        private void DestroyDragVisual()
        {
            if (_dragVisual != null)
            {
                Destroy(_dragVisual.gameObject);
                _dragVisual = null;
            }
        }

        private void Update()
        {
            if (_inputState == InputState.SliceDragging)
            {
                if (_timerTime > 0.0f)
                {
                    _timerTime -= Time.deltaTime;
                }
                else
                {
                    _sliceStrength = Mathf.Clamp01(_sliceStrength + cSliceStrengthIncreaseRate * Time.deltaTime);

                    UpdateSliceVisuals();
                }
            }
        }
    }
}