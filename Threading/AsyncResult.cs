using System;
using System.Diagnostics;
using System.Threading;

namespace RemoteController.Threading
{
    //[DebuggerNonUserCode] // alanjmcf
    internal class AsyncResult : IAsyncResult
    {
        // Fields set at construction which never change while operation is pending
        private readonly AsyncCallback m_AsyncCallback;
        private readonly object m_AsyncState;

        // Field set at construction which do change after operation completes
        private const int c_StatePending = 0;
        private const int c_StateCompletedSynchronously = 1;
        private const int c_StateCompletedAsynchronously = 2;
        private int m_CompletedState = c_StatePending;

        // Field that may or may not get set depending on usage
        private ManualResetEvent m_AsyncWaitHandle;

        // Fields set when operation completes
        private Exception m_exception;

        public AsyncResult(AsyncCallback asyncCallback, object state)
        {
            m_AsyncCallback = asyncCallback;
            m_AsyncState = state;
        }

        //[DebuggerNonUserCode] // alanjmcf
        //[Obsolete("temp Call with: Boolean callbackOnNewThread")]
        public void SetAsCompleted(Exception exception, bool completedSynchronously)
        {
            AsyncResultCompletion x = ConvertCompletion(completedSynchronously);
            SetAsCompleted(exception, x);
        }

        protected static AsyncResultCompletion ConvertCompletion(bool completedSynchronously)
        {
            AsyncResultCompletion x;
            if (completedSynchronously)
                x = AsyncResultCompletion.IsSync;
            else
                x = AsyncResultCompletion.IsAsync;
            return x;
        }

        //[DebuggerStepThrough] // alanjmcf
        public void SetAsCompleted(Exception exception, AsyncResultCompletion completion)
        {
            bool completedSynchronously = completion == AsyncResultCompletion.IsSync;
            // Passing null for exception means no error occurred; this is the common case
            m_exception = exception;

            // The m_CompletedState field MUST be set prior calling the callback
            int prevState = Interlocked.Exchange(ref m_CompletedState,
               completedSynchronously ? c_StateCompletedSynchronously : c_StateCompletedAsynchronously);
            if (prevState != c_StatePending)
                throw new InvalidOperationException("You can set a result only once");

            // If the event exists, set it
            if (m_AsyncWaitHandle != null)
                m_AsyncWaitHandle.Set();

            // If a callback method was set, call it
            if (m_AsyncCallback != null)
            {
                if (completion != AsyncResultCompletion.MakeAsync)
                    m_AsyncCallback(this);
                else
                    ThreadPool.QueueUserWorkItem(CallbackRunner);
            }
        }

        void CallbackRunner(object state)
        {
            m_AsyncCallback(this);
        }

        [DebuggerNonUserCode] // alanjmcf
        public void EndInvoke()
        {
            // This method assumes that only 1 thread calls EndInvoke for this object
            if (!IsCompleted)
            {
                // If the operation isn't done, wait for it
                AsyncWaitHandle.WaitOne();
                AsyncWaitHandle.Close();
                m_AsyncWaitHandle = null;  // Allow early GC
            }

            // Operation is done: if an exception occured, throw it
            if (m_exception != null)
                throw m_exception;
        }

        #region Implementation of IAsyncResult
        public object AsyncState { get { return m_AsyncState; } }

        public bool CompletedSynchronously
        {
            get { return m_CompletedState == c_StateCompletedSynchronously; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (m_AsyncWaitHandle == null)
                {
                    bool done = IsCompleted;
                    ManualResetEvent mre = new ManualResetEvent(done);
                    if (Interlocked.CompareExchange(ref m_AsyncWaitHandle, mre, null) != null)
                    {
                        // Another thread created this object's event; dispose the event we just created
                        mre.Close();
                    }
                    else
                    {
                        if (!done && IsCompleted)
                        {
                            // If the operation wasn't done when we created 
                            // the event but now it is done, set the event
                            m_AsyncWaitHandle.Set();
                        }
                    }
                }
                return m_AsyncWaitHandle;
            }
        }

        public bool IsCompleted
        {
            get { return m_CompletedState != c_StatePending; }
        }
        #endregion
    }

    [DebuggerNonUserCode] // alanjmcf
    internal class AsyncResult<TResult> : AsyncResult
    {
        // Field set when operation completes
        private TResult m_result = default(TResult);

        public AsyncResult(AsyncCallback asyncCallback, object state) : base(asyncCallback, state) { }

        [DebuggerNonUserCode] // alanjmcf
        public void SetAsCompleted(TResult result, bool completedSynchronously)
        {
            // Save the asynchronous operation's result
            m_result = result;

            // Tell the base class that the operation completed sucessfully (no exception)
            base.SetAsCompleted(null, completedSynchronously);
        }

        [DebuggerNonUserCode] // alanjmcf
        public void SetAsCompleted(TResult result, AsyncResultCompletion completion)
        {
            // Save the asynchronous operation's result
            m_result = result;

            // Tell the base class that the operation completed sucessfully (no exception)
            base.SetAsCompleted(null, completion);
        }

        [DebuggerNonUserCode] // alanjmcf
        public new TResult EndInvoke()
        {
            base.EndInvoke(); // Wait until operation has completed 
            return m_result;  // Return the result (if above didn't throw)
        }

        #region SetAsCompleted with: Func getResultsOrThrow
        /// <summary>
        /// Get the results of the operation from the specified function
        /// and set the operation as completed,
        /// or if getting the results fails then set the corresponding error
        /// completion.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The pattern that comes to mind when calling
        /// <see cref="M:SetAsCompleted(TResult,AsyncResultCompletion)"/> is
        /// the incorrect:
        /// <code>try {
        ///    var result = SomeStatementsAndFunctionCallsToGetTheResult(...);
        ///    ar.SetAsCompleted(result, false);
        /// } catch (Exception ex) {
        ///    ar.SetAsCompleted(ex, false);
        /// }
        /// </code>
        /// That is wrong because if the user callback fails with an exception
        /// then we'll catch it and try to call SetAsCompleted a second time!
        /// </para>
        /// <para>We need to instead call SetAsCompleted outside of the try
        /// block.  This method provides that pattern.
        /// </para>
        /// </remarks>
        /// -
        /// <param name="getResultsOrThrow">A delegate containing the function
        /// to call to get the result.
        /// It should throw an exception in error cases.
        /// </param>
        /// <param name="completedSynchronously"></param>
        internal void SetAsCompletedWithResultOf(
            Func<TResult> getResultsOrThrow,
            bool completedSynchronously)
        {
            SetAsCompletedWithResultOf(getResultsOrThrow, ConvertCompletion(completedSynchronously));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "SetAsCompleted.")]
        internal void SetAsCompletedWithResultOf(
            Func<TResult> getResultsOrThrow,
            AsyncResultCompletion completion)
        {
            TResult result;
            try
            {
                result = getResultsOrThrow();
            }
            catch (Exception ex)
            {
                SetAsCompleted(ex, completion);
                return;
            }
            SetAsCompleted(result, completion);
        }
        #endregion
    }
}
