﻿/**** Git Credential Manager for Windows ****
 *
 * Copyright (c) Atlassian
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the """"Software""""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE."
**/

using System.Threading;
using System.Threading.Tasks;

namespace Bitbucket.Shared.Helpers
{
    public static class TaskExtensions
    {
        public static async Task<TResult> RunWithCancellation<TResult>(this Task<TResult> task, CancellationToken cancellationToken)
        {
            var completedTask = await Task.WhenAny(task, cancellationToken.AsTask());
            if (completedTask == task)
            {
                return await task;  // Very important in order to propagate exceptions
            }
            else
            {
                throw new TaskCanceledException("The operation has been canceled");
            }
        }

        /// <summary>
        /// https://github.com/StephenCleary/AsyncEx
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task AsTask(this CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                //return TaskConstants.Never;
                // TODO should be static?
                return new TaskCompletionSource<bool>().Task;
            }
            if (cancellationToken.IsCancellationRequested)
            {
                //return TaskConstants.Canceled;
                // TODO should be static ?
                TaskCompletionSource<bool> completionSource = new TaskCompletionSource<bool>();
                completionSource.SetCanceled();
                return completionSource.Task;
            }

            var tcs = new TaskCompletionSource<object>();
            cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            return tcs.Task;
        }
    }
}
