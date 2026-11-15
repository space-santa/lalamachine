using System.Collections.Concurrent;
using System.Threading;

namespace Lalamachine.ViewModel.Unittests
{
    // What is posted to it waits until the test runs it, like on a UI thread.
    internal sealed class PostedActions : SynchronizationContext
    {
        private readonly ConcurrentQueue<(SendOrPostCallback Callback, object? State)> _posted = new();

        public override void Post(SendOrPostCallback d, object? state) => _posted.Enqueue((d, state));

        public void Run()
        {
            while (_posted.TryDequeue(out var posted))
            {
                posted.Callback(posted.State);
            }
        }
    }
}
