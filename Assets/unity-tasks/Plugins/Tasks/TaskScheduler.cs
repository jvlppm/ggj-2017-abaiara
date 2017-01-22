using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
	public abstract class TaskScheduler {
		static TaskScheduler _current;
		public static TaskScheduler Current {
			get {
				if (_current == null) {
					_current = FromCurrentSynchronizationContext();
				}
				return _current;
			}
			set {
				_current = value;
			}
		}

		public static TaskScheduler FromSynchronizationContext(SynchronizationContextHelper context) {
			if (context == null) {
				throw new ArgumentNullException("context");
			}
			return new CoroutineTaskScheduler(context, context.ThreadContext);
		}

		public static TaskScheduler FromCurrentSynchronizationContext() {
			var context = SynchronizationContextHelper.Current;
			if (context == null) {
				throw new InvalidOperationException("No current SynchronizationContext");
			}
			return FromSynchronizationContext(context);
		}

		public static TaskScheduler FromMonoBehaviour(MonoBehaviour behaviour, SynchronizationContext context = null) {
			return new CoroutineTaskScheduler(behaviour, context);
		}

		public abstract void QueueTask(Task task);
		public abstract void DequeueTask(Task task);

		class CoroutineTaskScheduler : TaskScheduler {
			readonly MonoBehaviour _behaviour;
			readonly SynchronizationContext _context;

			bool PostRequired {
				get { return _context != null && _context != SynchronizationContext.Current; }
			}

			public CoroutineTaskScheduler(MonoBehaviour behaviour, SynchronizationContext context) {
				this._context = context == null? SynchronizationContext.Current : context;
				this._behaviour = behaviour;
			}

			public override void QueueTask(Task task) {
				if (task.schedulerCoroutine != null) {
					throw new InvalidOperationException ("Task already scheduled");
				}
				task.scheduler = this;

				var tcs = task.source;
				((ITaskCompletionSourceController)tcs).TrySetRunning();
				task.schedulerCoroutine = RunTaskCR(tcs);

				if (task.cancellation.CanBeCanceled) {
					var reg = task.cancellation.Register(delegate {
						tcs.TrySetCanceled();
						DequeueTask(tcs.Task);
					});
					tcs.Task.ContinueWith(delegate {
						reg.Dispose();
					});
				}

				if (PostRequired) {
					_context.Post(s => {
						_behaviour.StartCoroutine((IEnumerator)s);
					}, task.schedulerCoroutine);
				}
				else {
					_behaviour.StartCoroutine(task.schedulerCoroutine);
				}
			}

			public override void DequeueTask(Task task) {
				if (task.scheduler != this) {
					throw new ArgumentException("Task is not scheduled", "task");
				}
				_behaviour.StopCoroutine(task.schedulerCoroutine);
				task.schedulerCoroutine = null;
			}

			IEnumerator RunTaskCR(TaskCompletionSource task) {
				object current;
				while (StepTask(task, out current)) {
					yield return current;
				}
			}

			bool StepTask(TaskCompletionSource tcs, out object current) {
				current = null;
				var oldScheduler = _current;
				try {
					_current = this;
					var finished = !tcs.Task.userCoroutine.MoveNext();
					_current = oldScheduler;
					if (finished) {
						if (tcs.Task.autoComplete) {
							tcs.TrySetCompleted();
						}
						return false;
					}
				}
				catch (OperationCanceledException ex) {
					_current = oldScheduler;
					tcs.TrySetCanceled(ex);
					return false;
				}
				catch (Exception ex) {
					_current = oldScheduler;
					tcs.TrySetException(ex);
					return false;
				}


				Action resume = delegate {
					if (!tcs.Task.IsCompleted) {
						_behaviour.StartCoroutine(RunTaskCR(tcs));
					}
				};

				var t = tcs.Task.userCoroutine.Current as Task;
				if (t != null) {
					t.ContinueWith(delegate {
						resume();
					}, this);
					return false;
				}
				var cr = tcs.Task.userCoroutine.Current as IEnumerator;
				if (cr != null) {
					Task.Factory.StartNew(cr, this).ContinueWith(runningTask => {
						if (!runningTask.SetFailure(tcs)) {
							resume();
						}
					}, this);
					return false;
				}

				current = tcs.Task.userCoroutine.Current;
				return true;
			}
		}
	}
}
