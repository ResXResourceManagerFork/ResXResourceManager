﻿namespace ResXManager.Model
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class SynchronizationContextThrottle
    {
        public static TaskFactory? TaskFactory { get; set; }

        private readonly Action _target;

        private int _counter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationContextThrottle"/> class.
        /// </summary>
        /// <param name="target">The target action to invoke when the throttle condition is hit.</param>
        public SynchronizationContextThrottle(Action target)
        {
            _target = target;

            //try
            //{
            //    _taskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
            //}
            //catch (InvalidOperationException)
            //{
            //    // for scripting deferred notifications are not needed, so if the current thread does not support a synchronization context, just go without.
            //}
        }

        /// <summary>
        /// Ticks this instance to trigger the throttle.
        /// </summary>
        public void Tick()
        {
            var taskFactory = TaskFactory;
            if (taskFactory == null)
                return;

            if (Interlocked.CompareExchange(ref _counter, 1, 0) != 0)
                return;

            taskFactory.StartNew(() =>
            {
                _target();
                Interlocked.Exchange(ref _counter, 0);
            });
        }
    }
}
