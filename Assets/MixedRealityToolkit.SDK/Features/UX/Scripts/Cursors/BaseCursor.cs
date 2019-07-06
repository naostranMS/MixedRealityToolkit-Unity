﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Physics;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Object that represents a cursor in 3D space.
    /// </summary>
    public class BaseCursor : MonoBehaviour, IMixedRealityCursor
    {
        private IMixedRealityInputSystem inputSystem = null;

        /// <summary>
        /// The active instance of the input system.
        /// </summary>
        protected IMixedRealityInputSystem InputSystem
        {
            get
            {
                if (inputSystem == null)
                {
                    MixedRealityServiceRegistry.TryGetService<IMixedRealityInputSystem>(out inputSystem);
                }
                return inputSystem;
            }
        }

        public CursorStateEnum CursorState { get; private set; } = CursorStateEnum.None;

        public CursorContextEnum CursorContext { get; private set; } = CursorContextEnum.None;

        /// <summary>
        /// Surface distance to place the cursor off of the surface at
        /// </summary>
        [SerializeField]
        [Tooltip("The distance from the hit surface to place the cursor")]
        private float surfaceCursorDistance = 0.02f;

        public float SurfaceCursorDistance
        {
            get { return surfaceCursorDistance; }
        }

        /// <summary>
        /// When lerping, use unscaled time. This is useful for games that have a pause mechanism or otherwise adjust the game timescale.
        /// </summary>
        public bool UseUnscaledTime
        {
            get { return useUnscaledTime; }
            set { useUnscaledTime = value; }
        }

        [Header("Motion")]
        [SerializeField]
        [Tooltip("When lerping, use unscaled time. This is useful for games that have a pause mechanism or otherwise adjust the game timescale.")]
        private bool useUnscaledTime = true;

        /// <summary>
        /// Blend value for surface normal to user facing lerp.
        /// </summary>
        public float PositionLerpTime
        {
            get { return positionLerpTime; }
            set { positionLerpTime = value; }
        }

        [SerializeField]
        [Tooltip("Blend value for surface normal to user facing lerp")]
        private float positionLerpTime = 0.01f;

        /// <summary>
        /// Blend value for surface normal to user facing lerp.
        /// </summary>
        public float ScaleLerpTime
        {
            get { return scaleLerpTime; }
            set { scaleLerpTime = value; }
        }

        [SerializeField]
        [Tooltip("Blend value for surface normal to user facing lerp")]
        private float scaleLerpTime = 0.01f;

        /// <summary>
        /// Blend value for surface normal to user facing lerp.
        /// </summary>
        public float RotationLerpTime
        {
            get { return rotationLerpTime; }
            set { rotationLerpTime = value; }
        }

        [SerializeField]
        [Tooltip("Blend value for surface normal to user facing lerp")]
        private float rotationLerpTime = 0.01f;

        /// <summary>
        /// Blend value for surface normal to user facing lerp.
        /// </summary>
        public float LookRotationBlend
        {
            get { return lookRotationBlend; }
            set { lookRotationBlend = value; }
        }

        [Range(0, 1)]
        [SerializeField]
        [Tooltip("Blend value for surface normal to user facing lerp")]
        private float lookRotationBlend = 0.5f;

        [Header("Transform References")]
        [SerializeField]
        [Tooltip("Visual that is displayed when cursor is active normally")]
        protected Transform PrimaryCursorVisual = null;

        protected bool IsSourceDetected => visibleSourcesCount > 0;

        public List<uint> SourceDownIds = new List<uint>();
        public bool IsPointerDown => SourceDownIds.Count > 0;

        protected GameObject TargetedObject = null;

        private uint visibleSourcesCount = 0;
        public uint VisibleSourcesCount
        {
            get { return visibleSourcesCount; }
            set { visibleSourcesCount = value; }
        }

        protected Vector3 targetPosition;
        protected Vector3 targetScale;
        protected Quaternion targetRotation;

        #region IMixedRealityCursor Implementation

        /// <inheritdoc />
        public virtual IMixedRealityPointer Pointer
        {
            get { return pointer; }
            set
            {
                if (ReferenceEquals(pointer?.BaseCursor, this))
                {
                    pointer.BaseCursor = null;
                }

                pointer = value;
                if (pointer != null)
                {
                    pointer.BaseCursor = this;
                }

                ResetInputSourceState();
            }
        }

        private IMixedRealityPointer pointer;

        /// <inheritdoc />
        public float DefaultCursorDistance
        {
            get { return defaultCursorDistance; }
            set { defaultCursorDistance = value; }
        }

        [SerializeField]
        [Tooltip("The maximum distance the cursor can be with nothing hit")]
        private float defaultCursorDistance = 2.0f;

        /// <inheritdoc />
        public virtual Vector3 Position => transform.position;

        /// <inheritdoc />
        public virtual Quaternion Rotation => transform.rotation;

        /// <inheritdoc />
        public virtual Vector3 LocalScale => transform.localScale;

        public virtual void SetVisibility(bool visible)
        {
            if (PrimaryCursorVisual != null &&
                PrimaryCursorVisual.gameObject.activeInHierarchy != visible)
            {
                PrimaryCursorVisual.gameObject.SetActive(visible);
            }
        }

        /// <inheritdoc />
        public bool IsVisible => PrimaryCursorVisual != null ? PrimaryCursorVisual.gameObject.activeInHierarchy : gameObject.activeInHierarchy;

        /// <inheritdoc />
        public bool SetVisibilityOnSourceDetected { get; set; } = false;

        /// <inheritdoc />
        public GameObject GameObjectReference => gameObject;

        private FocusDetails focusDetails;

        #endregion IMixedRealityCursor Implementation

        #region IMixedRealitySourceStateHandler Implementation

        /// <inheritdoc />
        public virtual void OnSourceDetected(SourceStateEventData eventData)
        {
            if (Pointer != null && eventData.Controller != null)
            {
                for (int i = 0; i < eventData.InputSource.Pointers.Length; i++)
                {
                    // If a source is detected that's using this cursor's pointer, we increment the count to set the cursor state properly.
                    if (eventData.InputSource.Pointers[i].PointerId == Pointer.PointerId)
                    {
                        visibleSourcesCount++;

                        if (SetVisibilityOnSourceDetected && visibleSourcesCount == 1)
                        {
                            SetVisibility(true);
                        }

                        return;
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual void OnSourceLost(SourceStateEventData eventData)
        {
            if (Pointer != null && eventData.Controller != null)
            {
                for (int i = 0; i < eventData.InputSource.Pointers.Length; i++)
                {
                    // If a source is lost that's using this cursor's pointer, we decrement the count to set the cursor state properly.
                    if (eventData.InputSource.Pointers[i].PointerId == Pointer.PointerId)
                    {
                        visibleSourcesCount--;
                        break;
                    }
                }
            }

                SourceDownIds.Remove(eventData.SourceId);

            if (!IsSourceDetected && SetVisibilityOnSourceDetected)
                {
                    SetVisibility(false);
                }
            }

        #endregion IMixedRealitySourceStateHandler Implementation

        #region IMixedRealityFocusChangedHandler Implementation

        /// <inheritdoc />
        public virtual void OnBeforeFocusChange(FocusEventData eventData)
        {
            if (Pointer != null && Pointer.PointerId == eventData.Pointer.PointerId)
            {
                TargetedObject = eventData.NewFocusedObject;
            }
        }

        /// <inheritdoc />
        public virtual void OnFocusChanged(FocusEventData eventData) { }

        #endregion IMixedRealityFocusChangedHandler Implementation

        #region IMixedRealityPointerHandler Implementation

        /// <inheritdoc />
        public virtual void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            if (Pointer != null)
            {
                foreach (var sourcePointer in eventData.InputSource.Pointers)
                {
                    if (sourcePointer.PointerId == Pointer.PointerId)
                    {
                        SourceDownIds.Add(eventData.SourceId);
                        return;
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual void OnPointerDragged(MixedRealityPointerEventData eventData) { }

        /// <inheritdoc />
        public virtual void OnPointerClicked(MixedRealityPointerEventData eventData) { }

        /// <inheritdoc />
        public virtual void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            if (Pointer != null)
            {
                foreach (var sourcePointer in eventData.InputSource.Pointers)
                {
                    if (sourcePointer.PointerId == Pointer.PointerId)
                    {
                        SourceDownIds.Remove(eventData.SourceId);
                        return;
                    }
                }
            }
        }

        #endregion IMixedRealityPointerHandler Implementation

        #region MonoBehaviour Implementation
        protected virtual void Start()
        {
            RegisterManagers();
        }

        private void Update()
        {
            if (!InputSystem.FocusProvider.TryGetFocusDetails(Pointer, out focusDetails))
            {
                if (InputSystem.FocusProvider.IsPointerRegistered(Pointer))
                {
                    Debug.LogError($"{name}: Unable to get focus details for {pointer.GetType().Name}!");
                }
            }

            UpdateCursorState();
            UpdateCursorTransform();
        }

        protected virtual void OnEnable()
        {
            OnCursorStateChange(CursorStateEnum.None);
            ResetInputSourceState();
        }

        protected virtual void OnDisable()
        {
            TargetedObject = null;
            visibleSourcesCount = 0;
            OnCursorStateChange(CursorStateEnum.Contextual);
        }

        private void OnDestroy()
        {
            UnregisterManagers();
        }

        #endregion MonoBehaviour Implementation

        /// <summary>
        /// Register to events from the managers the cursor needs.
        /// </summary>
        protected virtual void RegisterManagers()
        {
            // Register the cursor as a listener, so that it can always get input events it cares about
            InputSystem.RegisterHandler<IMixedRealityCursor>(this);

            // Setup the cursor to be able to respond to input being globally enabled / disabled
            if (InputSystem.IsInputEnabled)
            {
                OnInputEnabled();
            }
            else
            {
                OnInputDisabled();
            }

            InputSystem.InputEnabled += OnInputEnabled;
            InputSystem.InputDisabled += OnInputDisabled;
        }

        /// <summary>
        /// Unregister from events from the managers the cursor needs.
        /// </summary>
        protected virtual void UnregisterManagers()
        {
            if (InputSystem != null)
            {
                InputSystem.InputEnabled -= OnInputEnabled;
                InputSystem.InputDisabled -= OnInputDisabled;
                InputSystem.UnregisterHandler<IMixedRealityCursor>(this);
            }
        }

        /// <summary>
        /// Update the cursor's transform
        /// </summary>
        protected virtual void UpdateCursorTransform()
        {
            if (Pointer == null)
            {
                Debug.LogError($"[BaseCursor.{name}] No Pointer has been assigned!");
                return;
            }

            TargetedObject = InputSystem.FocusProvider.GetFocusedObject(Pointer);

            // Normalize scale on before update
            targetScale = Vector3.one;

            if (Pointer.CursorModifier != null)
            {
                Pointer.CursorModifier.GetModifiedTransform(this, out targetPosition, out targetRotation, out targetScale);
            }
            else
            {
                Vector3 lookForward;
                if (TargetedObject == null)
                {
                    // If no game object is hit, put the cursor at the default distance
                    targetPosition = RayStep.GetPointByDistance(Pointer.Rays, defaultCursorDistance);
                    lookForward = -RayStep.GetDirectionByDistance(Pointer.Rays, defaultCursorDistance);
                }
                else
                {
                    // If no modifier is on the target, just use the hit result to set cursor position
                    // Get the look forward by using distance between pointer origin and target position
                    // (This may not be strictly accurate for extremely wobbly pointers, but it should produce usable results)
                    lookForward = -RayStep.GetDirectionByDistance(Pointer.Rays, focusDetails.RayDistance);
                    targetPosition = focusDetails.Point + (lookForward * surfaceCursorDistance);
                    Vector3 blendedLookForward = Vector3.Slerp(focusDetails.Normal, lookForward, lookRotationBlend);
                    if (blendedLookForward != Vector3.zero)
                    {
                        lookForward = blendedLookForward;
                    }
                }

                targetRotation = lookForward.magnitude > 0.0f ? Quaternion.LookRotation(lookForward, Vector3.up) : transform.rotation;
            }

            LerpToTargetTransform();
        }

        protected void LerpToTargetTransform()
        {
            float deltaTime = useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            // Use the lerp times to blend the position to the target position
            transform.position = Vector3.Lerp(transform.position, targetPosition, deltaTime / positionLerpTime);
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, deltaTime / scaleLerpTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, deltaTime / rotationLerpTime);
        }

        protected void SnapToTargetTransform()
        {
            transform.position = targetPosition;
            transform.localScale = targetScale;
            transform.rotation = targetRotation;
        }

        /// <summary>
        /// Disable input and set to contextual to override input
        /// </summary>
        public virtual void OnInputDisabled()
        {
            // Reset visible hands on disable
            visibleSourcesCount = 0;

            OnCursorStateChange(CursorStateEnum.Contextual);
        }

        /// <summary>
        /// Enable input and set to none to reset cursor
        /// </summary>
        public virtual void OnInputEnabled()
        {
            OnCursorStateChange(CursorStateEnum.None);
            ResetInputSourceState();
        }

        /// <summary>
        /// Update visibleSourcesCount (and correspondingly IsSourceDetected) by looking at all input sources
        /// registered with the input system (DetectedInputSources). This is useful for cases where the cursor
        /// has not been listening for SourceDetected events (or the events have been disabled) and so the
        /// count may have gotten out of sync.
        /// It will also clear SourceDownIds (which will make IsPointerDown false, regardless of the underlying
        /// input source state) - so it should really *only* be used in cases where the source state hadn't been
        /// updating (for whatever reason).
        /// </summary>
        private void ResetInputSourceState()
        {
            SourceDownIds.Clear();
            visibleSourcesCount = 0;
            if (Pointer != null)
            {
                uint cursorPointerId = Pointer.PointerId;
                foreach (IMixedRealityInputSource inputSource in InputSystem.DetectedInputSources)
                {
                    if (inputSource.SourceType != InputSourceType.Head && inputSource.SourceType != InputSourceType.Eyes)
                    {
                        foreach (IMixedRealityPointer inputSourcePointer in inputSource.Pointers)
                        {
                            if (inputSourcePointer.PointerId == cursorPointerId)
                            {
                                ++visibleSourcesCount;
                                break;
                            }
                        }
                    }
                }
            }

            if (SetVisibilityOnSourceDetected)
            {
                SetVisibility(IsSourceDetected);
            }
        }

        /// <summary>
        /// Internal update to check for cursor state changes
        /// </summary>
        private void UpdateCursorState()
        {
            CursorStateEnum newState = CheckCursorState();
            if (CursorState != newState)
            {
                OnCursorStateChange(newState);
            }

            CursorContextEnum newContext = CheckCursorContext();
            if (CursorContext != newContext)
            {
                OnCursorContextChange(newContext);
            }
        }

        /// <summary>
        /// Virtual function for checking state changes.
        /// </summary>
        public virtual CursorStateEnum CheckCursorState()
        {
            if (CursorState != CursorStateEnum.Contextual)
            {
                if (IsPointerDown)
                {
                    return CursorStateEnum.Select;
                }

                if (CursorState == CursorStateEnum.Select)
                {
                    return CursorStateEnum.Release;
                }

                if (IsSourceDetected)
                {
                    return TargetedObject != null ? CursorStateEnum.InteractHover : CursorStateEnum.Interact;
                }

                return TargetedObject != null ? CursorStateEnum.ObserveHover : CursorStateEnum.Observe;
            }

            return CursorStateEnum.Contextual;
        }

        /// <summary>
        /// Gets three axes where the forward is as close to the provided normal as
        /// possible but where the axes are aligned to the TargetObject's transform
        /// </summary>
        private bool GetCursorTargetAxes(Vector3 normal, ref Vector3 right, ref Vector3 up, ref Vector3 forward)
        {
            if (TargetedObject)
            {
                Vector3 objRight = TargetedObject.transform.TransformDirection(Vector3.right);
                Vector3 objUp = TargetedObject.transform.TransformDirection(Vector3.up);
                Vector3 objForward = TargetedObject.transform.TransformDirection(Vector3.forward);

                float dotRight = Vector3.Dot(normal, objRight);
                float dotUp = Vector3.Dot(normal, objUp);
                float dotForward = Vector3.Dot(normal, objForward);

                if (Math.Abs(dotRight) > Math.Abs(dotUp) && 
                    Math.Abs(dotRight) > Math.Abs(dotForward))
                {
                    forward = (dotRight > 0 ? objRight : -objRight).normalized;
                }
                else if (Math.Abs(dotUp) > Math.Abs(dotForward))
                {
                    forward = (dotUp > 0 ? objUp : -objUp).normalized;
                }
                else
                {
                    forward = (dotForward > 0 ? objForward : -objForward).normalized;
                }

                right = Vector3.Cross(Vector3.up, forward).normalized;
                if (right == Vector3.zero)
                {
                    right = Vector3.Cross(objForward, forward).normalized;
                }
                up = Vector3.Cross(forward, right).normalized;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Virtual function for checking cursor context changes.
        /// </summary>
        public virtual CursorContextEnum CheckCursorContext()
        {
            if (CursorContext != CursorContextEnum.Contextual)
            {
                var cursorAction = CursorContextInfo.CursorAction.None;
                Transform contextCenter = null;
                if (TargetedObject)
                {
                    var contextInfo = TargetedObject.GetComponent<CursorContextInfo>();
                    if (contextInfo != null)
                    {
                        cursorAction = contextInfo.CurrentCursorAction;
                        contextCenter = contextInfo.ObjectCenter;
                    }
                }

                Vector3 right = Vector3.zero;
                Vector3 up = Vector3.zero;
                Vector3 forward = Vector3.zero;
                if (!GetCursorTargetAxes(focusDetails.Normal, ref right, ref up, ref forward))
                {
                    return CursorContextEnum.None;
                }

                if (cursorAction == CursorContextInfo.CursorAction.Move)
                {
                    return CursorContextEnum.MoveCross;
                }
                else if (cursorAction == CursorContextInfo.CursorAction.Scale)
                {
                    if (contextCenter != null)
                    {
                        Vector3 adjustedCursorPos = Position - contextCenter.position;

                        if (Vector3.Dot(adjustedCursorPos, up) * Vector3.Dot(adjustedCursorPos, right) > 0) // quadrant 1 and 3
                        {
                            return CursorContextEnum.MoveNorthwestSoutheast;
                        }
                        else // quadrant 2 and 4
                        {
                            return CursorContextEnum.MoveNortheastSouthwest;
                        }
                    }
                }
                else if (cursorAction == CursorContextInfo.CursorAction.Rotate)
                {
                    if (contextCenter != null)
                    {
                        Vector3 adjustedCursorPos = Position - contextCenter.position;

                        if (Math.Abs(Vector3.Dot(adjustedCursorPos, right)) > 
                            Math.Abs(Vector3.Dot(adjustedCursorPos, up)))
                        {
                            return CursorContextEnum.RotateEastWest;
                        }
                        else
                        {
                            return CursorContextEnum.RotateNorthSouth;
                        }
                    }
                }
                return CursorContextEnum.None;
            }
            return CursorContextEnum.Contextual;
        }

        /// <summary>
        /// Change the cursor state to the new state.  Override in cursor implementations.
        /// </summary>
        /// <param name="state"></param>
        public virtual void OnCursorStateChange(CursorStateEnum state)
        {
            CursorState = state;
        }

        /// <summary>
        /// Change the cursor context state to the new context.  Override in cursor implementations.
        /// </summary>
        /// <param name="context"></param>
        public virtual void OnCursorContextChange(CursorContextEnum context)
        {
            CursorContext = context;
        }
    }
}