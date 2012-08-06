﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSoft.NFramework.Parallelism.Tools;

namespace NSoft.NFramework.Parallelism.DataStructures {
    /// <summary>
    /// 비동기 <see cref="AutoResetEvent"/>
    /// </summary>
    public class AsyncAutoResetEvent {
        private static readonly Task completedTask = Task.Factory.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> _waits = new Queue<TaskCompletionSource<bool>>();
        private bool _signaled;

        public Task WaitAsync() {
            lock(_waits) {
                if(_signaled) {
                    _signaled = false;
                    return completedTask;
                }

                var tcs = new TaskCompletionSource<bool>();
                _waits.Enqueue(tcs);
                return tcs.Task;
            }
        }

        public void Set() {
            TaskCompletionSource<bool> toRelease = null;

            lock(_waits) {
                if(_waits.Count > 0)
                    toRelease = _waits.Dequeue();
                else if(_signaled == false)
                    _signaled = true;
            }

            if(toRelease != null)
                toRelease.SetResult(true);
        }
    }
}