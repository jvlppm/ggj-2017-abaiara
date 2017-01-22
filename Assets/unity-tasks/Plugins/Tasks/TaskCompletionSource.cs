namespace System.Threading.Tasks
{
	/// <summary>
	/// Represents the producer side of a Task unbound to a delegate,
	/// providing access to the consumer side through the Task property.
	/// </summary>
	public class TaskCompletionSource : ITaskCompletionSourceController {
		/// <summary>
		/// Gets the Task created by this TaskCompletionSource.
		/// </summary>
		public readonly Task Task; 

		internal Exception Error { get; private set; }

		internal object syncRoot = new object();
		TaskStatus _state;
		Action<TaskCompletionSource> _onComplete;

		/// <summary>
		/// Creates a TaskCompletionSource.
		/// </summary>
		public TaskCompletionSource() {
			Task = GenerateDefaultTask();
		}

		protected virtual Task GenerateDefaultTask() {
			return new Task(this);
		}

		internal TaskStatus Status {
			get { return _state; }
			set {
				_state = value;
				if (value >= TaskStatus.RanToCompletion && _onComplete != null) {
					_onComplete(this);
					_onComplete = null;
				}
			}
		}

		/// <summary>
		/// Transitions the underlying Task into the Canceled state.
		/// </summary>
		public void SetCanceled(OperationCanceledException ex = null) {
			if (!TrySetCanceled(ex)) {
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the underlying Task
		/// into the TaskStatus.Canceled state.
		/// </summary>
		/// <param name="ex">Optional cancellation exception.</param>
		/// <returns>true if the operation is successful; otherwise, false.</returns>
		public bool TrySetCanceled(OperationCanceledException ex = null)
		{
			lock (syncRoot) {
				if (Status > TaskStatus.Running) {
					return false;
				}
				Error = ex ?? new OperationCanceledException();
				Status = TaskStatus.Canceled;
			}
			return true;
		}

		/// <summary>
		/// Transitions the underlying Task into the RanToCompletion state.
		/// </summary>
		public void SetCompleted() {
			if (!TrySetCompleted()) {
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the underlying Task into the RanToCompletion state.
		/// </summary>
		/// <returns>True if the operation was successful; otherwise, false.</returns>
		public bool TrySetCompleted() {
			lock (syncRoot) {
				if (Status > TaskStatus.Running) {
					return false;
				}

				Status = TaskStatus.RanToCompletion;
			}
			return true;
		}

		/// <summary>
		/// Transitions the underlying Task into the Faulted
		/// state and binds it to a specified exception.
		/// </summary>
		public void SetException(Exception error) {
			if (!TrySetException(error)) {
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the underlying Task into
		/// the Faulted state and binds it to a specified exception.
		/// </summary>
		/// <param name="error">The exception to bind to this Task.</param>
		/// <returns>True if the operation was successful; otherwise, false.</returns>
		public bool TrySetException(Exception error) {
			lock (syncRoot) {
				if (Status > TaskStatus.Running) {
					return false;
				}

				Error = error;
				Status = TaskStatus.Faulted;
			}
			return true;
		}

		internal object Current { get; set; }

		TaskStatus ITaskCompletionSourceController.Status {
			get { return _state; }
		}

		bool ITaskCompletionSourceController.TrySetRunning() {
			lock (syncRoot) {
				if (Status != TaskStatus.Created) {
					return false;
				}
				Status = TaskStatus.Running;
			}
			return true;
		}

		void ITaskCompletionSourceController.ContinueWith(Action<TaskCompletionSource> action) {
			if (Status > TaskStatus.Running) {
				action(this);
			}
			else {
				_onComplete += action;
			}
		}
	}

	public class TaskCompletionSource<TResult> : TaskCompletionSource
	{
		/// <summary>
		/// Gets the Task<TResult> created by this TaskCompletionSource<TResult>.
		/// </summary>
		/// <returns></returns>
		public new Task<TResult> Task { get { return (Task<TResult>)base.Task; } }

		protected override Task GenerateDefaultTask() {
			return new Task<TResult>(this);
		}

		TResult _result;
		internal TResult Result {
			get
			{
				if (Status == TaskStatus.Faulted) {
					throw Error;
				}
				if (Status < TaskStatus.Running) {
					throw new InvalidOperationException("Task did not complete yet", Error);
				}
				if (Status != TaskStatus.RanToCompletion) {
					throw new InvalidOperationException("Task did not complete successfully", Error);
				}
				return _result;
			}
			private set {
				_result = value;
			}
		}

		public new bool TrySetCompleted() {
			throw new InvalidOperationException ("Use TrySetResult instead");
		}

		public new void SetCompleted() {
			throw new InvalidOperationException ("Use TrySetResult instead");
		}

		/// <summary>
		/// Transitions the underlying Task&lt;TResult&gt; into the RanToCompletion state.
		/// </summary>
		/// <param name="result">The result value to bind to this Task.</param>
		public void SetResult(TResult result) {
			if (!TrySetResult(result)) {
				throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Attempts to transition the underlying Task into the RanToCompletion state.
		/// </summary>
		/// <param name="result">The result value to bind to this Task&lt;TResult&gt;.</param>
		public bool TrySetCompleted(TResult result = default(TResult)) {
			return TrySetResult (result);
		}

		/// <summary>
		/// Attempts to transition the underlying Task&lt;TResult&gt; into the RanToCompletion state.
		/// </summary>
		/// <param name="result">The result value to bind to this Task&lt;TResult&gt;.</param>
		public bool TrySetResult(TResult result) {
			lock (syncRoot) {
				if (Status > TaskStatus.Running) {
					return false;
				}

				Result = result;
				base.TrySetCompleted();
			}
			return true;
		}
	}

	internal interface ITaskCompletionSourceController {
		TaskStatus Status { get; }
		void ContinueWith(Action<TaskCompletionSource> action);
		bool TrySetRunning();
	}
}
