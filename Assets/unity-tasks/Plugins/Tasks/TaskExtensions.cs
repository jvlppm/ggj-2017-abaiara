namespace System.Threading.Tasks
{
	public static class TaskExtensions
	{
		public static Task Unwrap(this Task<Task> task) {
			var tcs = new TaskCompletionSource();
			task.ContinueWith(t => {
				if (!t.SetFailure(tcs)) {
					t.Result.ThenComplete(tcs);
				}
			});
			return tcs.Task;
		}
		public static Task<T> Unwrap<T>(this Task<Task<T>> task) {
			var tcs = new TaskCompletionSource<T>();
			task.ContinueWith(t => {
				if (!t.SetFailure(tcs)) {
					t.Result.ThenComplete(tcs);
				}
			});
			return tcs.Task;
		}
	}
}
