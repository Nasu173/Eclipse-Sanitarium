using UnityEngine;

namespace EclipseSanitarium.TaskSystem
{
    public class TaskTrigger : MonoBehaviour
    {
        public enum TriggerType { Collision, Interaction, Custom }

        [Header("触发配置")]
        public TriggerType type = TriggerType.Interaction;
        public string targetTaskId;
        public int progressAmount = 1;
        
        [Tooltip("是否只能触发一次")]
        public bool triggerOnce = true;
        private bool hasTriggered = false;

        public void Trigger()
        {
            if (triggerOnce && hasTriggered) return;

            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.UpdateProgress(targetTaskId, progressAmount);
                hasTriggered = true;
                Debug.Log($"触发器已启动: 目标任务 {targetTaskId}");
            }
        }

        // 碰撞触发
        private void OnTriggerEnter(Collider other)
        {
            if (type == TriggerType.Collision && other.CompareTag("Player"))
            {
                Trigger();
            }
        }

        // 交互触发 (预留给交互系统调用)
        public void OnInteract()
        {
            if (type == TriggerType.Interaction)
            {
                Trigger();
            }
        }
    }
}
