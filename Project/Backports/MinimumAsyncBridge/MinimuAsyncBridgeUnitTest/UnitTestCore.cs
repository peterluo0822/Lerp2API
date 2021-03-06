﻿//using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Assertions;

namespace MinimuAsyncBridgeUnitTest
{
    public class UnitTestCore
    {
        
        public void TestCompletedTask()
        {
            AwaitCompletedTaskShouldBeExecutedSynchronously().Wait();
        }

        private async Task AwaitCompletedTaskShouldBeExecutedSynchronously()
        {
            var tid0 = Thread.CurrentThread.ManagedThreadId;

            {
                var t = DateTime.Now;
                await Task.CompletedTask;
                var elapsed = (DateTime.Now) - t;
                Assert.IsTrue(elapsed.TotalSeconds < 20);
                var tid = Thread.CurrentThread.ManagedThreadId;
                Assert.AreEqual(tid0, tid);
            }
            {
                var t = DateTime.Now;
                await Task.FromResult(10);
                var elapsed = (DateTime.Now) - t;
                Assert.IsTrue(elapsed.TotalSeconds < 20);
                var tid = Thread.CurrentThread.ManagedThreadId;
                Assert.AreEqual(tid0, tid);
            }
            {
                var t = DateTime.Now;
                await Task.FromResult("");
                var elapsed = (DateTime.Now) - t;
                Assert.IsTrue(elapsed.TotalSeconds < 20);
                var tid = Thread.CurrentThread.ManagedThreadId;
                Assert.AreEqual(tid0, tid);
            }
        }

        
        public void TestFromException()
        {
            AwaitOnTaskFromExceptionShouldThrow().Wait();
        }

        private async Task AwaitOnTaskFromExceptionShouldThrow()
        {
            try
            {
                await Task.FromException<int>(new InvalidOperationException());
            }
            catch (InvalidOperationException)
            {
                return;
            }

            //Assert.Fail();
            Assert.raiseExceptions = true;
        }

        
        public void TestCancellationTokenSource()
        {
            CancellationTokenSource_CancelShouldWorkIfFirstTime();
        }

        private void CancellationTokenSource_CancelShouldWorkIfFirstTime()
        {
            var cancelCount = 0;
            var cts = new CancellationTokenSource();
            cts.Token.Register(() => { cancelCount++; cts.Cancel(); });
            cts.Cancel();

            Assert.IsTrue(cancelCount == 1);
        }

        
        public void TestCancellationTokenSourceWithTimeout() => TestCancellationTokenSourceWithTimeoutAsync().Wait();
        public async Task TestCancellationTokenSourceWithTimeoutAsync()
        {
            var cts = new CancellationTokenSource(1000);
            var ct = cts.Token;

            Assert.IsFalse(ct.IsCancellationRequested);

            await Task.Delay(510);
            Assert.IsFalse(ct.IsCancellationRequested);

            await Task.Delay(510);
            Assert.IsTrue(ct.IsCancellationRequested);
        }

        
        public void TestContinueWith()
        {
            ContinueWithShouldHasExceptionIfContinuationActionGetException().Wait();
            ContinueWithShouldHasExceptionIfContinuationActionGetExceptionWithResult().Wait();
        }

        private async Task ContinueWithShouldHasExceptionIfContinuationActionGetException()
        {
            var exceptionCount = 0;
            var ex = new Exception("ex1");
            try
            {
                await Task.Delay(1).ContinueWith(_ => { throw ex; });
            }
            catch (Exception e)
            {
                AssertExt.AreSame(ex, e);
                exceptionCount++;
            }
            Assert.AreEqual(1, exceptionCount);
        }

        private async Task ContinueWithShouldHasExceptionIfContinuationActionGetExceptionWithResult()
        {
            var exceptionCount = 0;
            var ex = new Exception("ex1");
            try
            {
                await Task.Delay(1).ContinueWith(_ => 1).ContinueWith(_ => { throw ex; });
            }
            catch (Exception e)
            {
                AssertExt.AreSame(ex, e);
                exceptionCount++;
            }
            Assert.AreEqual(1, exceptionCount);
        }
    }
}
