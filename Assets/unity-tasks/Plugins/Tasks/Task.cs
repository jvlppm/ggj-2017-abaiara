using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace System.Threading.Tasks
{
	public class Task {
		static Task()
		{
			var tcs = new TaskCompletionSource();
			tcs.TrySetCompleted();
			CompletedTask = tcs.Task;

			CompletedWithNull = Task<object>.FromResult((object)null);
			CompletedWithTrue = Task<bool>.FromResult(true);
			CompletedWithFalse = Task<bool>.FromResult(false);
		}

		// Actual task, can be null.
		internal IEnumerator userCoroutine;
		// Scheduler coroutine, can be null.
		internal IEnumerator schedulerCoroutine;
		// Scheduler that is set to run this coroutine;
		internal TaskScheduler scheduler;
		// Set task as completed after userCoroutine execution.
		internal bool autoComplete = false;
		// Task cancellation token.
		internal CancellationToken cancellation;

		/// <summary>
		/// Creates a Task&lt;TResult&gt; that's completed
		/// successfully with the specified result.
		/// </summary>
		/// <param name="result">The result to store into the completed task. </param>
		/// <returns>The successfully completed task.</returns>
		public static Task<T> FromResult<T>(T result) {
			if (typeof(T) == typeof(object)) {
				if (result == null && CompletedWithNull != null) {
					return (Task<T>)(object)CompletedWithNull;
				}
			}
			if (typeof(T) == typeof(bool)) {
				if ((bool)(object)result && CompletedWithTrue != null) {
					return (Task<T>)(object)CompletedWithTrue;
				}
				else if ((bool)(object)result && CompletedWithFalse != null) {
					return (Task<T>)(object)CompletedWithFalse;
				}
			}

			var t = new TaskCompletionSource<T>();
			t.TrySetResult(result);
			return t.Task;
		}

		/// <summary>
		/// Creates a Task that's completed with a specified exception.
		/// </summary>
		/// <param name="ex">The exception with which to complete the task.</param>
		/// <returns>The faulted task.</returns>
		public static Task FromException(Exception ex) {
			var t = new TaskCompletionSource();
			t.SetException(ex);
			return t.Task;
		}

		/// <summary>
		/// Creates a Task&lt;T&gt; that's completed with a specified exception.
		/// </summary>
		/// <param name="ex">The exception with which to complete the task.</param>
		/// <returns>The faulted task.</returns>
		public static Task<T> FromException<T>(Exception ex) {
			var t = new TaskCompletionSource<T>();
			t.SetException(ex);
			return t.Task;
		}

		public class TaskFactory {
			public Task StartNew(IEnumerator coroutine, TaskScheduler scheduler = null, CancellationToken cancellation = default(CancellationToken)) {
				var tcs = new TaskCompletionSource();
				tcs.Task.userCoroutine = coroutine;
				tcs.Task.autoComplete = true;
				tcs.Task.cancellation = cancellation;

				scheduler = scheduler ?? TaskScheduler.Current;
				scheduler.QueueTask (tcs.Task);

				
				return tcs.Task;
			}

			public Task StartNew(Func<TaskCompletionSource, IEnumerator> asyncMethod, TaskScheduler scheduler = null, CancellationToken cancellation = default(CancellationToken)) {
				var tcs = new TaskCompletionSource ();
				tcs.Task.userCoroutine = asyncMethod(tcs);
				tcs.Task.cancellation = cancellation;
				
				scheduler = scheduler ?? TaskScheduler.Current;
				scheduler.QueueTask (tcs.Task);

				return tcs.Task;
			}

			public Task<TResult> StartNew<TResult>(Func<TaskCompletionSource<TResult>, IEnumerator> asyncMethod, TaskScheduler scheduler = null, CancellationToken cancellation = default(CancellationToken)) {
				var tcs = new TaskCompletionSource<TResult> ();
				tcs.Task.userCoroutine = asyncMethod(tcs);
				tcs.Task.cancellation = cancellation;
				
				scheduler = scheduler ?? TaskScheduler.Current;
				scheduler.QueueTask (tcs.Task);

				return tcs.Task;
			}
		}

		public static TaskFactory Factory = new TaskFactory();

		/// <summary>
		/// Creates a task that completes after a specified time interval.
		/// </summary>
		/// <param name="delay">The time span to wait before completing the returned task, or TimeSpan.FromMilliseconds(-1) to wait indefinitely.</param>
		/// <returns>A task that represents the time delay.</returns>
		public static Task Delay(TimeSpan delay, CancellationToken cancellation = default(CancellationToken)) {
			return Delay((int)delay.TotalMilliseconds, cancellation);
		}

		/// <summary>
		/// Creates a task that completes after a time delay.
		/// </summary>
		/// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned task, or -1 to wait indefinitely.</param>
		/// <returns>A task that represents the time delay.</returns>
		public static Task Delay(int millisecondsDelay, CancellationToken cancellation = default(CancellationToken)) {
			if (millisecondsDelay < -1) {
				throw new ArgumentOutOfRangeException("millisecondsDelay");
			}
			if (millisecondsDelay == -1) {
				var tcs = new TaskCompletionSource();
				return tcs.Task;
			}
			var op = new YieldOnce(new WaitForSeconds(millisecondsDelay / 1000.0f));
			return Task.Factory.StartNew(op);
		}

		/// <summary>
		/// Creates an awaitable task that asynchronously
		/// yields back to the current context when awaited.
		/// </summary>
		/// <returns></returns>
		public static Task Yield() {
			var op = new YieldOnce((object)null);
			return Task.Factory.StartNew(op);
		}

		/// <summary>
		/// Creates a task that will complete when all of the
		/// Task&lt;TResult&gt; objects in an array have completed.
		/// </summary>
		/// <param name="tasks">The tasks to wait on for completion.</param>
		/// <returns>A task that represents the completion of all of the supplied tasks.</returns>
		public static Task<TResult[]> WhenAll<TResult>(params Task<TResult>[] tasks)
		{
			return Task.WhenAll((Task[])tasks).ContinueWith(t => {
			    t.AssertCompletion();
				var res = new TResult[tasks.Length];
				for (int i = 0; i < tasks.Length; i++) {
					res[i] = tasks[i].Result;
				}
				return res;
			});
		}

		/// <summary>
		/// Creates a task that will complete when any of the supplied tasks have completed.
		/// </summary>
		/// <param name="tasks">The tasks to wait on for completion.</param>
		/// <returns>A task that represents the completion of one of the supplied tasks. The return task's Result is the task that completed.</returns>
		public static Task<Task<TResult>> WhenAny<TResult>(params Task<TResult>[] tasks) {
			return WhenAny((Task[])tasks).ContinueWith(t => {
				return (Task<TResult>)t.Result;
			});
		}

		/// <summary>
		/// Creates a task that will complete when any of the supplied tasks have completed.
		/// </summary>
		/// <param name="tasks">The tasks to wait on for completion.</param>
		/// <returns>A task that represents the completion of one of the supplied tasks. The return task's Result is the task that completed.</returns>
		public static Task<Task> WhenAny(params Task[] tasks) {
			if (tasks == null || tasks.Length <= 0 || tasks.Contains(null)) {
				throw new ArgumentException("tasks");
			}

			var tcs = new TaskCompletionSource<Task>();
			Action<Task> onComplete = t => {
				lock (tcs) {
					tcs.TrySetCompleted(t);
				}
			};

			foreach (var t in tasks) {
				t.ContinueWith(onComplete);
			}

			return tcs.Task;
		}

		/// <summary>
		/// Creates a task that will complete when all
		/// of the Task objects in an array have completed.
		/// </summary>
		/// <param name="tasks">The tasks to wait on for completion.</param>
		/// <returns>A task that represents the completion of all of the supplied tasks.</returns>
		public static Task WhenAll(params Task[] tasks)
		{
			if (tasks == null || tasks.Contains(null)) {
				throw new ArgumentException("tasks");
			}

			if (tasks.Length <= 0) {
				return Task.CompletedTask;
			}

		    List<Exception> errors = new List<Exception>();

			int remaining = tasks.Length;
			var tcs = new TaskCompletionSource();
			Action<Task> onComplete = t =>
			{
			    if (t.Status != TaskStatus.RanToCompletion)
			        errors.Add(t.Error);
			    lock (tcs) {
					remaining--;
					if (remaining <= 0)
					{
					    if (errors.Any())
					        tcs.TrySetException(new AggregateException(errors));
					    else
						    tcs.TrySetCompleted();
					}
				}
			};

			foreach (var t in tasks) {
				t.ContinueWith(onComplete);
			}

			return tcs.Task;
		}

		/// <summary>
		/// Gets a task that has already completed successfully.
		/// </summary>
		public static readonly Task CompletedTask;
		readonly static Task<bool> CompletedWithTrue, CompletedWithFalse;
		readonly static Task<object> CompletedWithNull;

		internal readonly TaskCompletionSource source;

		internal Task(TaskCompletionSource source) {
			this.source = source;
		}

		public Task ContinueWith(Action<Task> action, TaskScheduler scheduler = null) {
			return this.ContinueWithCR(t => new RunOnce(() => action(t)), scheduler);
		}

		public Task<T> ContinueWith<T>(Func<Task, T> action, TaskScheduler scheduler = null) {
			return this.ContinueWithCR<T>((tcs, t) => new RunOnce(() => tcs.TrySetResult(action(t))), scheduler);
		}

		public TaskStatus Status {
			get { return ((ITaskCompletionSourceController)source).Status; }
		}

		public Exception Error {
			get { return source.Error; }
		}

		/// <summary>
		/// Gets whether this Task instance has completed execution due to being canceled.
		/// </summary>
		/// <returns>true if the task has completed due to being canceled; otherwise false.</returns>
		public bool IsCanceled {
			get { return source.Status == TaskStatus.Canceled; }
		}

		/// <summary>
		/// Gets whether this Task has completed.
		/// </summary>
		/// <returns>true if the task has completed; otherwise false.</returns>
		public bool IsCompleted {
			get { return source.Status > TaskStatus.Running; }
		}

		/// <summary>
		/// Gets whether the Task completed due to an unhandled exception.
		/// </summary>
		/// <returns>true if the task has thrown an unhandled exception; otherwise false.</returns>
		public bool IsFaulted {
			get { return source.Status == TaskStatus.Faulted; }
		}

		public void AssertCompletion()
		{
			if (Error != null) {
				throw Error;
			}
		}
	}

	public class Task<T> : Task
	{
		internal readonly new TaskCompletionSource<T> source;

		public Task(TaskCompletionSource<T> source) : base(source) {
			this.source = source;
		}

		public T Result {
			get { return source.Result; }
		}

		public Task ContinueWith(Action<Task<T>> action, TaskScheduler scheduler = null) {
			return base.ContinueWith(t => action((Task<T>)t), scheduler);
		}

		public Task<TR> ContinueWith<TR>(Func<Task<T>, TR> action, TaskScheduler scheduler = null) {
			return base.ContinueWith(t => action((Task<T>)t), scheduler);
		}
	}
}
