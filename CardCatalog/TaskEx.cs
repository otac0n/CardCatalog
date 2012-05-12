using System;
using System.Threading;
using System.Threading.Tasks;

namespace CardCatalog
{
    public static class TaskEx
    {
        static readonly Task preCompletedTask = GetCompletedTask();
        static readonly Task preCanceledTask = GetPreCanceledTask();

        public static Task Delay(int dueTimeMs, CancellationToken cancellationToken)
        {
            if (dueTimeMs < -1)
            {
                throw new ArgumentOutOfRangeException("dueTimeMs", "Invalid due time");
            }
            else if (cancellationToken.IsCancellationRequested)
            {
                return preCanceledTask;
            }
            else if (dueTimeMs == 0)
            {
                return preCompletedTask;
            }

            var tcs = new TaskCompletionSource<object>();
            var ctr = new CancellationTokenRegistration();
            var timer = new Timer(self =>
            {
                ctr.Dispose();
                ((Timer)self).Dispose();
                tcs.TrySetResult(null);
            });

            if (cancellationToken.CanBeCanceled)
            {
                ctr = cancellationToken.Register(() =>
                {
                    timer.Dispose();
                    tcs.TrySetCanceled();
                });
            }

            timer.Change(dueTimeMs, -1);
            return tcs.Task;
        }

        private static Task GetPreCanceledTask()
        {
            var source = new TaskCompletionSource<object>();
            source.TrySetCanceled();
            return source.Task;
        }

        private static Task GetCompletedTask()
        {
            var source = new TaskCompletionSource<object>();
            source.TrySetResult(null);
            return source.Task;
        }
    }
}