using System;
using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX
{
    public class SetSpawnTime : VFXSpawnerCallbacks
    {
        private static readonly int spawnTimeID = Shader.PropertyToID("spawnTime");

        public sealed override void OnPlay(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {

        }

        public sealed override void OnUpdate(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {
            state.vfxEventAttribute.SetFloat(spawnTimeID, state.totalTime);
        }

        public sealed override void OnStop(VFXSpawnerState state, VFXExpressionValues vfxValues, VisualEffect vfxComponent)
        {

        }
    }
}
