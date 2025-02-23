using System;
using UnityEngine.Rendering.HighDefinition;

using Fence =
#if UNITY_2019_1_OR_NEWER
    UnityEngine.Rendering.GraphicsFence;
#else
    UnityEngine.Rendering.GPUFence;
#endif


namespace UnityEngine.Rendering
{
    struct HDGPUAsyncTaskParams
    {
        public ScriptableRenderContext  renderContext;
        public HDCamera                 hdCamera;
        public int                      frameCount;
    }

    struct HDGPUAsyncTask
    {
        private enum AsyncTaskStage
        {
            NotTriggered = 0,
            StartFenceCreated = 1,
            AsyncCmdEnqueued = 2,
            TaskCompleted = 3
        }

        private Fence               m_StartFence;
        private Fence               m_EndFence;

        private string              m_TaskName;
        private ComputeQueueType    m_QueueType;

        private AsyncTaskStage      m_TaskStage;

        public HDGPUAsyncTask(string taskName, ComputeQueueType queueType = ComputeQueueType.Background)
        {
            m_StartFence = new Fence();
            m_EndFence = new Fence();
            m_TaskName = taskName;
            m_QueueType = queueType;
            m_TaskStage = AsyncTaskStage.NotTriggered;
        }

        private void PushStartFenceAndExecuteCmdBuffer(CommandBuffer cmd, ScriptableRenderContext renderContext)
        {
            Debug.Assert(m_TaskStage == AsyncTaskStage.NotTriggered);

            m_StartFence =
#if UNITY_2019_1_OR_NEWER
            cmd.CreateAsyncGraphicsFence();
#else
            cmd.CreateGPUFence();
#endif
            renderContext.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            m_TaskStage = AsyncTaskStage.StartFenceCreated;
        }

        public void Start(CommandBuffer cmd, in HDGPUAsyncTaskParams asyncParams, Action<CommandBuffer, HDGPUAsyncTaskParams> asyncTask, bool pushStartFence = true)
        {
            Debug.Assert(m_TaskStage == AsyncTaskStage.NotTriggered);
            if (pushStartFence)
            {
                PushStartFenceAndExecuteCmdBuffer(cmd, asyncParams.renderContext);
            }

            CommandBuffer asyncCmd = CommandBufferPool.Get(m_TaskName);
#if UNITY_2019_1_OR_NEWER
            asyncCmd.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);
#endif

            if (pushStartFence)
            {
#if UNITY_2019_1_OR_NEWER
                asyncCmd.WaitOnAsyncGraphicsFence(m_StartFence);
#else
                asyncCmd.WaitOnGPUFence(m_StartFence);
#endif
            }

            asyncTask(asyncCmd, asyncParams);

#if UNITY_2019_1_OR_NEWER
            m_EndFence = asyncCmd.CreateAsyncGraphicsFence();
#else
            m_EndFence = asyncCmd.CreateGPUFence();
#endif
            asyncParams.renderContext.ExecuteCommandBufferAsync(asyncCmd, m_QueueType);
            CommandBufferPool.Release(asyncCmd);

            m_TaskStage = AsyncTaskStage.AsyncCmdEnqueued;
        }

        public void EndWithPostWork(CommandBuffer cmd, HDCamera hdCamera, Action<CommandBuffer, HDCamera> postWork)
        {
            Debug.Assert(m_TaskStage == AsyncTaskStage.AsyncCmdEnqueued);

#if UNITY_2019_1_OR_NEWER
            cmd.WaitOnAsyncGraphicsFence(m_EndFence);
#else
            cmd.WaitOnGPUFence(m_EndFence);
#endif
            postWork(cmd, hdCamera);

            m_TaskStage = AsyncTaskStage.TaskCompleted;
        }

        public void End(CommandBuffer cmd, HDCamera hdCamera)
        {
            EndWithPostWork(cmd, hdCamera, (_1, _2) => { });
        }
    }

}
