namespace System.Threading
{
	/// <summary>
	/// Propagates notification that operations should be canceled.
	/// </summary>
	public struct CancellationToken {
		/// <summary>
		/// Returns an empty CancellationToken value.
		/// This cancellation token can never be cancelled.
		/// </summary>
		/// <param name="CancellationToken"></param>
		/// <returns>An empty cancellation token.</returns>
		public readonly static CancellationToken None = default(CancellationToken);

		CancellationTokenSource _source;

		internal CancellationToken(CancellationTokenSource source) {
			this._source = source;
		}

		/// <summary>
		/// Gets whether this token is capable of being in the canceled state.
		/// </summary>
		/// <returns>true if this token is capable of being in the canceled state; otherwise, false.</returns>
		public bool CanBeCanceled {
			get { return _source != null; }
		}

		/// <summary>
		/// Gets whether cancellation has been requested for this token.
		/// </summary>
		/// <returns>true if cancellation has been requested for this token; otherwise, false.</returns>
		public bool IsCancellationRequested {
			get { return _source != null && _source.IsCancellationRequested; }
		}

		/// <summary>
		/// Throws a OperationCanceledException if this token has had cancellation requested.
		/// </summary>
		public void ThrowIfCancellationRequested()
		{
			if (IsCancellationRequested) {
				throw new OperationCanceledException("The operation was canceled.");
			}
		}

		/// <summary>
        /// Registers a delegate that will be called when this CancellationToken is canceled.
        /// </summary>
        /// <param name="callback">The delegate to be executed when the CancellationToken is canceled.</param>
        /// <returns>The CancellationTokenRegistration instance that can be used to deregister the callback.</returns>
		public CancellationTokenRegistration Register(System.Action callback) {
			if (_source == null) {
				return default(CancellationTokenRegistration);
			}
			return _source.Register(callback);
		}
	}
}
