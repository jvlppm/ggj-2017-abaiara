namespace System.Threading
{
	/// <summary>
	/// Signals to a CancellationToken that it should be canceled.
	/// </summary>
	public class CancellationTokenSource {
		bool _cancelled;
		internal event System.Action onCancel;

		/// <summary>
		/// Gets whether cancellation has been requested for this CancellationTokenSource.
		/// </summary>
		/// <returns>true if cancellation has been requested for this CancellationTokenSource; otherwise, false.</returns>
		public bool IsCancellationRequested {
			get { return _cancelled; }
		}

		/// <summary>
		/// Gets the CancellationToken associated with this CancellationTokenSource.
		/// </summary>
		/// <returns>The CancellationToken associated with this CancellationTokenSource.</returns>
		public CancellationToken Token {
			get { return new CancellationToken(this); }
		}

		/// <summary>
		/// Communicates a request for cancellation.
		/// </summary>
		public void Cancel() {
			_cancelled = true;
			if (onCancel != null) {
				onCancel();
			}
			onCancel = null;
		}

		internal CancellationTokenRegistration Register (System.Action callback) {
			if (IsCancellationRequested) {
				callback();
				return default(CancellationTokenRegistration);
			}
			return new CancellationTokenRegistration(this, callback);
		}
	}
}
