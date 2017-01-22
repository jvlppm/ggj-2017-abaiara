namespace System.Threading.Tasks
{
	public class CustomSynchronizationContext : SynchronizationContext {
	    readonly Action<SendOrPostCallback, object> _post;
	    public readonly int ThreadId;

		public CustomSynchronizationContext(Action<SendOrPostCallback, object> post)
		{
		    _post = post;
		    this.ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
		}

		public override void Send(System.Threading.SendOrPostCallback d, object state) {
			if (Thread.CurrentThread.ManagedThreadId == ThreadId) {
				base.Send(d, state);
			}
			else {
				Semaphore sem = new Semaphore(0, 1);
				_post(s => {
					d(s);
					sem.Release();
				}, state);
				sem.WaitOne();
			}
		}

		public override void Post(SendOrPostCallback d, object state) {
			_post(d, state);
		}
	}
}
