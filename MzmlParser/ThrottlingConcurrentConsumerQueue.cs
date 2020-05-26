#nullable enable

using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MzmlParser
{
    /// <summary>
    /// A Task-based queue that allows a single producer, multiple consumers, and limits the queue depth.
    /// All queue activity is run on the enqueuing thread, which blocks when maximumQueueDepth is reached.
    /// </summary>
    /// <remarks>
    /// This assumes that the producer tends to be over-eager and hence need throttling, as it will only schedule
    /// more tasks when the producer enqueues an item or when it is waiting for all tasks to complete.
    /// It's probably unwise to use this with a bursty producer; it might pause with items in the queue
    /// but nothing running.
    /// </remarks>
    class ThrottlingConcurrentConsumerQueue<T>
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private Action<T> Consumer { get; }
        private int MaximumConsumers { get; }
        private int MaximumQueueDepth { get; }
        private readonly IList<Task> tasks = new List<Task>();
        private readonly Queue<T> items = new Queue<T>();

        public ThrottlingConcurrentConsumerQueue(Action<T> consumer, int maximumConsumers, int maximumQueueDepth)
        {
            Consumer = consumer;
            MaximumConsumers = Math.Max(maximumConsumers, 1);
            MaximumQueueDepth = Math.Max(maximumQueueDepth, 1);
        }

        public void Enqueue(T item)
        {
            // Ensure Tasks is as full as possible, maximising the chances of not having to wait for a queue slot.
            PumpTasks(false);
            if (items.Count > MaximumQueueDepth)
                CompleteOneTask().Wait();
            items.Enqueue(item);
            // We may have just allowed a consumer to start - check!
            PumpTasks(false);
        }

        public void WaitForAllTasksToComplete()
        {
            while (tasks.Count > 0 || items.Count > 0)
                PumpTasks(true);
        }

        private async Task CompleteOneTask()
        {
            try
            {
                Task completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
                await completedTask;
            }
            catch (Exception ex)
            {
                LOGGER.Warn(ex);
            }
        }

        private void PumpTasks(bool waitForAtLeastOneTaskToComplete)
        {
            if (waitForAtLeastOneTaskToComplete)
                CompleteOneTask().Wait();
            while (tasks.Any(task => task.IsCompleted))
                CompleteOneTask().Wait();
            FillTasks();
        }

        private void FillTasks()
        {
            while (items.Count > 0 && tasks.Count < MaximumConsumers)
            {
                T item = items.Dequeue();
                tasks.Add(Task.Run(() => Consumer(item)));
            }
        }
    }
}
