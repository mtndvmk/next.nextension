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
            Pending.addAndSort(task);
            executeNext();
        }
        public void cancelPending()
        {
            foreach (var schedule in _pending.asSpan())
            {
                schedule.onCanceled();
            }
            _pending.clear();
        }

        private async void executeNext()
        {
            if (_executing == null || _executing.Count < MaxSchedulableAtOnce)
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
}
