using System.Threading.Tasks;
using UnityEngine;

namespace Nextension
{
    public class WaitTask : CustomYieldInstruction
    {
        private Task _task;
        private WaitTask(Task task)
        {
            if (_task.Status == TaskStatus.Created)
            {
                _task.Start();
            }
            _task = task;
        }
        public override bool keepWaiting => !_task.IsCompleted;

        public static implicit operator WaitTask(Task task) => new(task);
    }
}
