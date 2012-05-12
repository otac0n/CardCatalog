using System;
using System.Threading;
using System.Threading.Tasks;

namespace CardCatalog
{
    public static class RepeatingBackgroundTask
    {
        public static CancellationTokenSource Start(TimeSpan initialDelay, Func<TimeSpan?> repeatedAction)
        {
            var cts = new CancellationTokenSource();

            Action<Task> wrapperAction = null;
            wrapperAction = _ignore =>
            {
                var timeout = repeatedAction();

                if (timeout.HasValue)
                {
                    TaskEx.Delay((int)timeout.Value.TotalMilliseconds, cts.Token).ContinueWith(wrapperAction, cts.Token);
                }
            };

            TaskEx.Delay((int)initialDelay.TotalMilliseconds, cts.Token).ContinueWith(wrapperAction, cts.Token);

            return cts;
        }
    }
}