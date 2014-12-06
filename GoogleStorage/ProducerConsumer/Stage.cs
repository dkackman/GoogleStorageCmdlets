using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace GoogleStorage.ProducerConsumer
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

        public void Start(Func<TInput, TOutput> func, CancellationToken cancelToken)
        {
            // this is the delegate that does the downloading
            Action download = () =>
                {
                    foreach (var item in Input.GetConsumingEnumerable(cancelToken))
                    {
                        try
                        {
                            Output.Add(func(item));
                        }
                        catch (AggregateException e)
                        {
                            foreach (var ex in e.InnerExceptions)
                            {
                                Errors.Add(Tuple.Create(item, ex));
                            }
                        }
                        catch (Exception e)
                        {
                            Errors.Add(Tuple.Create(item, e));
                        }
                    }
                };

            // these are the download threads - each download blocks while in progress but the others can work in parallel
            Task.Run(() =>
                {
                    Task[] tasks = new Task[TaskCount];
                    for (int i = 0; i < tasks.Length; i++)
                    {
                        tasks[i] = Task.Run(download, cancelToken);
                    }

                    Task.WaitAll(tasks, cancelToken);
                    Output.CompleteAdding(); // this signals the calling thread that all the work is done
                }, cancelToken);
        }

        public void Dispose()
        {
            Input.Dispose();
            Output.Dispose();
        }
    }
}
