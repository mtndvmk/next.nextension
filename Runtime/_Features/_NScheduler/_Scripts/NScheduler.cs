using System.Collections.Generic;

namespace Nextension
{

    public class NScheduler<TSchedulable> where TSchedulable : NOperation, ISchedulable
    {
        public NScheduler(int maxSchedulableAtOnce = 3)
        {
            MaxSchedulableAtOnce = maxSchedulableAtOnce;
        }

        protected NBList<TSchedulable, int> _pending;
        protected List<TSchedulable> _executing;

        public NBList<TSchedulable, int> Pending => _pending ??= new(item => item.Priority);
        public List<TSchedulable> Executing => _executing ??= new();

        public int MaxSchedulableAtOnce { get; set; }

        public void schedule(TSchedulable task)
        {
            Pending.AddAndSort(task);
            executeNext();
        }
        public void cancelPending()
        {
            foreach (var schedule in _pending.AsSpan())
            {
                schedule.onCanceled();
            }
            _pending.Clear();
        }

        private void executeNext()
        {
            if (_executing == null || _executing.Count < MaxSchedulableAtOnce)
            {
                __executeNext().forget();
            }
        }

        private async NTaskVoid __executeNext()
        {
            var schedulable = _pending.takeAndRemoveLast();
            Executing.Add(schedulable);
            schedulable.onStartExecute();
            await schedulable;
            _executing.Remove(schedulable);
            if (_pending.Count == 0) return;
            executeNext();
        }
    }
}
