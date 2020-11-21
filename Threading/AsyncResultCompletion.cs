namespace RemoteController.Threading
{
    /// <summary>
    /// Used with
    /// <see cref="AsyncResult.SetAsCompleted(System.Exception,AsyncResultCompletion)">
    /// AsyncResultNoResult.SetAsCompleted</see> and 
    /// <see cref="AsyncResult{TResult}.SetAsCompleted(TResult, AsyncResultCompletion)">
    /// AsyncResult&lt;TResult&gt;.SetAsCompleted</see>.
    /// </summary>
    internal enum AsyncResultCompletion
    {
        /// <summary>
        /// Equivalent to <c>true</c> for the <see cref="T:System.Boolean"/>
        /// #x201C;completedSynchronously&#x201D; parameter.
        /// </summary>
        IsSync,
        /// <summary>
        /// Equivalent to <c>false</c> for the <see cref="T:System.Boolean"/>
        /// #x201C;completedSynchronously&#x201D; parameter.
        /// </summary>
        IsAsync,
        /// <summary>
        /// Forces the callback to run on a thread-pool thread.
        /// </summary>
        MakeAsync
    }
}
