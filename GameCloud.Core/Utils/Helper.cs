using System;
using System.Threading;
using System.Threading.Tasks;

namespace GameCloud.Core.Utils
{
    public static class Helper
    {
        /// <summary>
        /// Creates a random string of a given length.
        /// Uses a substring of guid
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string CreateRandomString(int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "length cannot be less than zero.");
            return Guid.NewGuid().ToString().Substring(0, length);
        }

        /// <summary>
        /// Runs a task with cancellation token and timeout scenario
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {

                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                }
                else
                {
                    throw new TimeoutException("The operation has timed out.");
                }
            }
        }
    }
}