using System.Collections;
using UnityEngine;

namespace System.Threading.Tasks
{
	public static class Extensions {
		public static Task RunTask(this MonoBehaviour behaviour, IEnumerator coroutine, float delay = 0) {
			var scheduler = TaskScheduler.FromMonoBehaviour(behaviour);
			var cr = delay <= 0? coroutine : RunCoroutineDelayed(coroutine, delay);
			return Task.Factory.StartNew(cr, scheduler);
		}

		static IEnumerator RunCoroutineDelayed(IEnumerator coroutine, float delay) {
			yield return new WaitForSeconds(delay);
			yield return coroutine;
		}

		public static Task RunTask(this MonoBehaviour behaviour, Func<TaskCompletionSource, IEnumerator> coroutine) {
			var scheduler = TaskScheduler.FromMonoBehaviour(behaviour);
			return Task.Factory.StartNew(coroutine, scheduler);
		}

		public static Task<T> RunTask<T>(this MonoBehaviour behaviour, Func<TaskCompletionSource<T>, IEnumerator> coroutine) {
			var scheduler = TaskScheduler.FromMonoBehaviour(behaviour);
			return Task.Factory.StartNew<T>(coroutine, scheduler);
		}

#if !UNITY_5_2
		public static TaskYieldInstruction ToYieldInstruction(this Task task) {
			return new TaskYieldInstruction(task);
		}

		public static TaskYieldInstruction<T> ToYieldInstruction<T>(this Task<T> task) {
			return new TaskYieldInstruction<T>(task);
		}
#endif

		public static IEnumerator ToIEnumerator(this Task task) {
			while (!task.IsCompleted)
				yield return null;
		}

		public static Task ThenComplete(this Task task, TaskCompletionSource tcs) {
			return task.ContinueWith(t => {
				if (t.Status == TaskStatus.RanToCompletion) {
					tcs.TrySetCompleted();
				}
				else if (t.Status == TaskStatus.Canceled) {
					tcs.TrySetCanceled((OperationCanceledException)t.Error);
				}
				else if (t.Status == TaskStatus.Faulted) {
					tcs.TrySetException(t.Error);
				}
				else {
					throw new NotImplementedException();
				}
				t.AssertCompletion();
			});
		}

		public static Task<T> ThenComplete<T>(this Task<T> task, TaskCompletionSource<T> tcs) {
			return task.ContinueWith<T>((Task<T> t) => {
				if (t.Status == TaskStatus.RanToCompletion) {
					tcs.TrySetCompleted(t.Result);
				}
				else if (t.Status == TaskStatus.Canceled) {
					tcs.TrySetCanceled((OperationCanceledException)t.Error);
				}
				else if (t.Status == TaskStatus.Faulted) {
					tcs.TrySetException(t.Error);
				}
				else {
					throw new NotImplementedException();
				}
				return t.Result;
			});
		}

		public static bool SetFailure(this Task task, TaskCompletionSource tcs) {
			if (task.Status == TaskStatus.Canceled) {
				tcs.TrySetCanceled(task.Error as System.OperationCanceledException);
				return true;
			}

			if (task.Status == TaskStatus.Faulted) {
				tcs.TrySetException(task.Error);
				return true;
			}

			return false;
		}

		public static Task ContinueWithCR(this Task task, Func<Task, IEnumerator> sequence, TaskScheduler scheduler = null) {
			var t = new TaskCompletionSource();
			t.Task.autoComplete = true;
			t.Task.scheduler = scheduler ?? task.scheduler ?? TaskScheduler.Current;

			((ITaskCompletionSourceController)task.source).ContinueWith(tcs => {
				t.Task.userCoroutine = sequence(tcs.Task);
				t.Task.scheduler.QueueTask(t.Task);
			});
			return t.Task;
		}

		public static Task ContinueWithCR(this Task task, Func<TaskCompletionSource, Task, IEnumerator> sequence, TaskScheduler scheduler = null) {
			var t = new TaskCompletionSource();
			t.Task.scheduler = scheduler ?? task.scheduler ?? TaskScheduler.Current;

			((ITaskCompletionSourceController)task.source).ContinueWith(tcs => {
				t.Task.userCoroutine = sequence(t, tcs.Task);
				t.Task.scheduler.QueueTask(t.Task);
			});
			return t.Task;
		}

		public static Task<T> ContinueWithCR<T>(this Task task, Func<TaskCompletionSource<T>, Task, IEnumerator> sequence, TaskScheduler scheduler = null) {
			var t = new TaskCompletionSource<T>();
			t.Task.scheduler = scheduler ?? task.scheduler ?? TaskScheduler.Current;

			((ITaskCompletionSourceController)task.source).ContinueWith(tcs => {
				t.Task.userCoroutine = sequence(t, tcs.Task);
				t.Task.scheduler.QueueTask(t.Task);
			});
			return t.Task;
		}

		public static Task ContinueWithCR<T>(this Task<T> task, Func<Task<T>, IEnumerator> sequence, TaskScheduler scheduler = null) {
			return ((Task)task).ContinueWithCR(s => sequence((Task<T>)s), scheduler);
		}

		public static Task ContinueWithCR<T>(this Task<T> task, Func<TaskCompletionSource, Task<T>, IEnumerator> sequence, TaskScheduler scheduler = null) {
			return ((Task)task).ContinueWithCR((s, t) => sequence(s, (Task<T>)t), scheduler);
		}

		public static Task<TR> ContinueWithCR<T, TR>(this Task<T> task, Func<TaskCompletionSource<TR>, Task<T>, IEnumerator> sequence, TaskScheduler scheduler = null) {
			return ((Task)task).ContinueWithCR<TR>((s, t) => sequence(s, (Task<T>)t), scheduler);
		}

		public static IEnumerator Wait(this Task task, string log = null, float timeOut = -1, bool ignoreErrors = false) {
			var toWait = timeOut < 0? task : task.TimeOut(TimeSpan.FromSeconds(timeOut), log + " Timed out");
			if (log != null) {
				toWait = toWait.Log(log);
			}

			yield return toWait;

			if (!ignoreErrors) {
				toWait.AssertCompletion();
			}
		}

		public static Task Log(this Task task, string name) {
			Debug.Log(name + " Started");
			return LogResult(task, name, false);
		}

		public static Task<T> Log<T>(this Task<T> task, string name) {
			Debug.Log(name + " Started");
			return LogResult(task, name, false);
		}

		public static Task LogResult(this Task task, string name, bool errorsOnly = false) {
			return task.ContinueWith(t => {
				if (!errorsOnly || t.Status != TaskStatus.RanToCompletion) {
					LogTaskResult(t, name);
				}
				t.AssertCompletion();
			});
		}

		public static Task<T> LogResult<T>(this Task<T> task, string name, bool errorsOnly = false) {
			return task.ContinueWith<T>(t => {
				if (!errorsOnly || t.Status != TaskStatus.RanToCompletion) {
					LogTaskResult(task, name);
				}
				return t.Result;
			});
		}

		static void LogTaskResult<T>(Task<T> task, string name) {
			if (task.IsFaulted) {
				Debug.LogError(name + " Completed - " + task.Status + " - " + task.Error);
			}
			else {
				Debug.Log(name + " Completed - " + task.Status + " - " + (task.Error != null? task.Error.ToString() : Convert.ToString(task.Result)));
			}
		}

		static void LogTaskResult(Task task, string name) {
			if (task.IsFaulted) {
				Debug.LogError(name + " Completed - " + task.Status + " - " + task.Error);
			}
			else {
				Debug.Log(name + " Completed - " + task.Status + (task.Error != null? " - " + task.Error : ""));
			}
		}

		public static Task TimeOut(this Task t, float seconds, string message = null) {
			return Task.Factory.StartNew(TimeOutCR(t, TimeSpan.FromSeconds(seconds), message));
		}

		public static Task TimeOut(this Task t, TimeSpan duration, string message = null) {
			return Task.Factory.StartNew(TimeOutCR(t, duration, message));
		}

		static IEnumerator TimeOutCR(Task t, TimeSpan duration, string message) {
			var cancellation = new CancellationTokenSource();
			var normalizedDuration = duration < TimeSpan.Zero? TimeSpan.FromMilliseconds(-1) : duration;
			var timeout = Task.Delay(normalizedDuration, cancellation.Token);

			var any = Task.WhenAny(t, timeout);
			yield return any;
			cancellation.Cancel();

			if (any.Result == timeout) {
				throw new TimeoutException(message ?? "Task timed out");
			}
			t.AssertCompletion();
		}
	}
}
