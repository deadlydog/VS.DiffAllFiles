
namespace System.Threading.Tasks
{
    /// <summary>
    /// Provides extension methods for the <see cref="Task"/>types.
    /// </summary>
    /// <remarks>
    /// Methods inspired by <see href="https://github.com/VsixCommunity/Community.VisualStudio.Toolkit/blob/26bdbf6ee28d0c706c2ad92fe4f226b9ebbddee5/src/Community.VisualStudio.Toolkit.Shared/ExtensionMethods/TaskExtensions.cs">
    /// Community.VisualStudio.Toolkit</see>
    /// </remarks>
    public static class TaskExtensions
    {
        /// <summary>
        /// Safely ignore the results of a task, and immediately resolve any thrown exceptions by ignoring it.
        /// </summary>
        /// <param name="t"> The <see cref="Task" /> to act on.</param>
        /// <remarks>
        ///    The task itself starts before this extension method is called and only continues in
        ///    this extension method if the original task throws an unhandled exception after the
        ///    first await of a non-completed task.<p/>
        ///    Exceptions that occur inside of <see langword="async"/> code may fire on a separate, non-UI thread. 
        ///    When not awaiting the result of a  <see cref="Task" /> (or otherwise not attaching 
        ///    a continuation to it that handles the faulted case), tasks that fault, depending on 
        ///    the context, can cause exceptions not getting released or getting released at some 
        ///    indeterminate time later - often when the application shuts down. Exceptions get 
        ///    released when the finalizer runs on the wrapping code or task, and in some cases 
        ///    that may never happen until the app shuts down. The end result is that you 
        ///    effectively have a memory leak.<p/>
        ///    This solution attaches a continuation that resolves the exception immediately.
        /// </remarks>
        public static void FireAndForget(this Task t)
        {
            t.ContinueWith(tsk => tsk.Exception, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default).Forget();
        }

        /// <summary>
        /// Consumes a task and doesn't do anything with it.  Useful for fire-and-forget calls to async methods within async methods.
        /// </summary>
        /// <param name="task">The task whose result is to be ignored.</param>
        /// <remarks>From Microsoft.VisualStudio.Threading, class TplExtensions. 
        /// See <see href="https://github.com/microsoft/vs-threading/blob/main/doc/cookbook_vs.md#how-to-write-a-fire-and-forget-method-responsibly">Visual Studio Cookbook</see>. </remarks>
        private static void Forget(this Task task)
        {
        }

#if !VS2012
        /// <summary>
        /// Ignore if the given <see cref="Microsoft.VisualStudio.Threading.JoinableTask" /> faults.
        /// </summary>
        /// <remarks>
        /// This is the JoinableTask equivalent of <see cref="FireAndForget(System.Threading.Tasks.Task)"/>
        /// </remarks>
        public static void FireAndForget(this Microsoft.VisualStudio.Threading.JoinableTask joinableTask)
        {
            FireAndForget(joinableTask.Task);
        }
#endif
    }
}