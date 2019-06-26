// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Physics;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Tests
{
    public class TestPointer : IMixedRealityPointer
    {
        IMixedRealityController IMixedRealityPointer.Controller { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        uint IMixedRealityPointer.PointerId => throw new System.NotImplementedException();

        string IMixedRealityPointer.PointerName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        IMixedRealityInputSource IMixedRealityPointer.InputSourceParent => throw new System.NotImplementedException();

        IMixedRealityCursor IMixedRealityPointer.BaseCursor { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        ICursorModifier IMixedRealityPointer.CursorModifier { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        bool IMixedRealityPointer.IsInteractionEnabled => throw new System.NotImplementedException();

        bool IMixedRealityPointer.IsActive { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        bool IMixedRealityPointer.IsFocusLocked { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        bool IMixedRealityPointer.IsTargetPositionLockedOnFocusLock { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        RayStep[] IMixedRealityPointer.Rays => throw new System.NotImplementedException();

        LayerMask[] IMixedRealityPointer.PrioritizedLayerMasksOverride { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        IMixedRealityFocusHandler IMixedRealityPointer.FocusTarget { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        IPointerResult IMixedRealityPointer.Result { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        SceneQueryType IMixedRealityPointer.SceneQueryType { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        float IMixedRealityPointer.SphereCastRadius { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        Vector3 IMixedRealityPointer.Position => throw new System.NotImplementedException();

        Quaternion IMixedRealityPointer.Rotation => throw new System.NotImplementedException();

        bool IEqualityComparer.Equals(object x, object y)
        {
            throw new System.NotImplementedException();
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            throw new System.NotImplementedException();
        }

        void IMixedRealityPointer.OnPostSceneQuery()
        {
            throw new System.NotImplementedException();
        }

        void IMixedRealityPointer.OnPreCurrentPointerTargetChange()
        {
            throw new System.NotImplementedException();
        }

        void IMixedRealityPointer.OnPreSceneQuery()
        {
            throw new System.NotImplementedException();
        }
    }
}
