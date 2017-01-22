namespace System.Threading.Tasks
{
	/// <summary>
	/// Represents the current stage in the lifecycle of a Task.
	/// </summary>
	public enum TaskStatus {
		
		/// <sumary>
		/// The task has been initialized but has not yet been scheduled.
		/// </sumary>
		Created,
		/// <summary>
		/// The task is running but has not yet completed.
		/// </sumary>
		Running,
		///  <sumary>
		/// The task completed execution successfully.
		///  </sumary>
		RanToCompletion,
		/// <sumary>
		/// The task acknowledged cancellation by throwing an OperationCanceledException.
		/// </sumary>
		Canceled,
		/// <sumary>
		/// The task completed due to an unhandled exception.
		/// </sumary>
		Faulted
	}
}
