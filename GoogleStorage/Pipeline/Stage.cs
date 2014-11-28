using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace GoogleStorage.Pipeline
{
    class Stage<TInput, TOutput> : IDisposable
        where TInput : class
        where TOutput : class
    {
        private Func<IEnumerable<TInput>> _producer;

        private Func<TInput, TOutput> _transform;

        public BlockingCollection<TOutput> Output { get; private set; }
        private bool _disposeOutput;

        public Stage(Func<IEnumerable<TInput>> producer)
            : this(producer, input => input as TOutput, new BlockingCollection<TOutput>(), true)
        {
        }

        public Stage(Func<IEnumerable<TInput>> producer, Func<TInput, TOutput> transform)
            : this(producer, transform, new BlockingCollection<TOutput>(), true)
        {
        }

        public Stage(Func<IEnumerable<TInput>> producer, Func<TInput, TOutput> transform, BlockingCollection<TOutput> output)
            : this(producer, transform, output, false)
        {
        }

        private Stage(Func<IEnumerable<TInput>> producer, Func<TInput, TOutput> transform, BlockingCollection<TOutput> output, bool disposeOutput)
        {
            _producer = producer;
            _transform = transform;
            Output = output;
            _disposeOutput = disposeOutput;
        }

        public void Start()
        {
            foreach (var input in _producer())
            {
                Output.Add(_transform(input));
            }

            Output.CompleteAdding();
        }

        public void Dispose()
        {
            if (Output != null && _disposeOutput)
            {
                Output.Dispose();
                Output = null;
            }
        }
    }
}
