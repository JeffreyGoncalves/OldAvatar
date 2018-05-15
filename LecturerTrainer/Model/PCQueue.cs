using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LecturerTrainer.Model
{
    /// <summary>
    /// This class uses the producer/consumer queue algorithm to perform a task in parallel
    /// on a every item of a list.
    /// It allows to start a task in parallel and keep it running until we want it to end, via the BlockingCollection
    /// This collection, while it is not completeAdding, will pause the thread while it has to item   
    /// </summary>
    /// <typeparam name="V">The type of the item you want to interact with</typeparam>
    /// <remarks>
    /// This class is a modified version of the one found here :
    /// http://www.albahari.com/threading/part5.aspx#_BlockingCollectionT
    /// I would advise any reader wanting to use multithreading/parallelism mechanics to read this site
    /// It has plenty of useful information.
    /// Example: use PCQueue<Bitmap> if you want to do a specific action on a Bitmap list.
    /// </remarks>
    /// <author> Amirali Ghazi </author>
    class PCQueue<V> : IDisposable
    {
        class WorkItem<T>
        {
            public readonly TaskCompletionSource<object> TaskSource;
            public readonly T Item;
            public readonly CancellationToken? CancelToken;

            public WorkItem(TaskCompletionSource<object> taskSource, T item, CancellationToken? cancelToken)
            {
                TaskSource = taskSource;
                Item = item;
                CancelToken = cancelToken;
            }
        }

        private Action<V> recordingAction;
        private Action closingAction;
        private BlockingCollection<WorkItem<V>> _itemQueue = new BlockingCollection<WorkItem<V>>();
        private Task task;


        /// <summary>
        /// Instantiate a Producer Consumer queue with the action ra and the name taskName
        /// </summary>
        /// <param name="ra">The action it has to perform on every item</param>
        /// <param name="taskName">The name of the task, useful only for debugging</param>
        public PCQueue(Action<V> ra, string taskName)
        {
            recordingAction = ra;
            task = Task.Factory.StartNew(state => Consume(), taskName, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Instantiate a Producer Consumer queue with the action ra, the closing action ca and the name taskName
        /// </summary>
        /// <param name="ra">The action it has to perform on every item</param>
        /// <param name="ca">The action it has to perform before quitting the thread (closing a stream for instance)</param>
        /// <param name="taskName">The name of the task, useful only for debugging</param>
        public PCQueue(Action<V> ra, Action ca, string taskName)
        {
            recordingAction = ra;
            closingAction = ca;
            task = Task.Factory.StartNew(state => Consume(), taskName, TaskCreationOptions.LongRunning);
        }
        /// <summary>
        /// Inform the collection that no more item will be added to the queue
        /// </summary>
        public void Dispose()
        {
            _itemQueue.CompleteAdding();
        }

        /// <summary>
        /// Add an item to the BlockingCollection
        /// </summary>
        /// <param name="item">The item to add</param>
        public void EnqueueItem(V item)
        {
            EnqueueItem(item, null);
        }

        /// <summary>
        /// Add an item to the BlockingCollection and an CancellationToken if need be
        /// </summary>
        /// <param name="item"></param>
        /// <param name="cancelToken"></param>
        /// <remarks> The '?' token is a shortcut for Nullable<CancellationToken> </remarks>
        public void EnqueueItem(V item, CancellationToken? cancelToken)
        {
            var tcs = new TaskCompletionSource<object>();
            _itemQueue.Add(new WorkItem<V>(tcs, item, cancelToken));
        }

        /// <summary>
        /// Main method which perform the actions done in the constructor
        /// </summary>
        private void Consume()
        {
            foreach (WorkItem<V> workItem in _itemQueue.GetConsumingEnumerable())
            {
                if (workItem.CancelToken.HasValue && workItem.CancelToken.Value.IsCancellationRequested)
                {
                    workItem.TaskSource.SetCanceled();
                }
                else
                {
                    try
                    {
                        recordingAction(workItem.Item);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (ex.CancellationToken == workItem.CancelToken)
                            workItem.TaskSource.SetCanceled();
                        else
                            workItem.TaskSource.SetResult(null);
                    }
                    catch (Exception ex)
                    {
                        workItem.TaskSource.SetException(ex);
                    }
                }
            }
            // We get out of the loop when the IsCompleted property of _taskQueue is set as true
            // (meaning the collection has been marked as complete for adding via the Dispose() method and is empty)
            // Therefore here the collection is empty and the task associated with it is done
            // We can close the recording if need be.
            closingAction?.Invoke();
        }

    }
}
