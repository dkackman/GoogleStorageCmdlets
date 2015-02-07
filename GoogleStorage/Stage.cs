using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace GoogleStorage
{
    class Stage<TInput, TOutput> : IDisposable
    {
        public BlockingCollection<TInput> Input { get; private set; }

        public BlockingCollection<TOutput> Output { get; private set; }

        public ConcurrentBag<Tuple<TInput, Exception>> Errors { get; private set; }

        public int TaskCount { get; set; }

        public Stage()
        {
            TaskCount = 4;
            Input = new BlockingCollection<TInput>();
            Output = new BlockingCollection<TOutput>();
            Errors = new ConcurrentBag<Tuple<TInput, Exception>>();
        }

        public async Task Start(Func<TInput, Task<TOutput>> func, CancellationToken cancelToken)
        {
            // func is the delegate that does the work, such as downloading
            // the consumeInput async lambda spins over the input collection,
            // calls the worker func for each item it gets, and waits for the input colleciton to
            // be emptied and closed. This will run on multiple threads
            Func<Task> consumeInput = async () =>
                {
                    foreach (var item in Input.GetConsumingEnumerable(cancelToken))
                    {
                        try
                        {
                            TOutput output = await func(item);
                            Output.Add(output);
                        }
                        catch (OperationCanceledException)
                        {
                            throw; // if something was cancelled rethrow that
                        }
                        catch (AggregateException e)
                        {
                            // if any of the child exceptions indicate cancellation just throw the first one
                            e.ThrowIfCancelled();

                            // everything else, log for upstream inspection
                            foreach (var ex in e.InnerExceptions)
                            {
                                Errors.Add(Tuple.Create(item, ex));
                            }
                        }
                        catch (Exception e)
                        {
                            Errors.Add(Tuple.Create(item, e));
                        }

                        cancelToken.ThrowIfCancellationRequested();
                    }
                };

            Task[] tasks = new Task[TaskCount];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(consumeInput, cancelToken);
            }

            // these are the worker threads 
            await Task.Run(() =>
                {
                    Task.WaitAll(tasks, cancelToken);
                    Output.CompleteAdding(); // this signals the calling thread that all the work is done
                    // because it will stop trying to iterate on the blocking enuemration
                }, cancelToken);
        }

        public void Dispose()
        {
            Input.Dispose();
            Output.Dispose();
        }
    }
}
