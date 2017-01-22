using UnityEngine;
using System.Collections.Generic;

namespace System.Threading.Tasks
{
	public class SynchronizationContextHelper : MonoBehaviour {
		public System.Threading.SynchronizationContext MainContext = new CustomSynchronizationContext(PostToDefault);

		/// <summary>
		/// Gets the synchronization context for the current thread.
		/// </summary>
		public static SynchronizationContextHelper Current = null;

		public static SynchronizationContextHelper Default { get; private set; }

		public System.Threading.SynchronizationContext ThreadContext { get; private set; }

		[SerializeField] bool _default = SynchronizationContextHelper.Current == null;

		struct PendingOperation {
			public System.Threading.SendOrPostCallback callback;
			public object state;
		}

		readonly Queue<PendingOperation> pendingOperations = new Queue<PendingOperation>();
		static readonly Queue<PendingOperation> defaultPendingOperations = new Queue<PendingOperation>();

		void Awake() {
		    ThreadContext = new CustomSynchronizationContext(Post);
		    if (_default) {
				if (Current != null) {
					_default = false;
					return;
				}
				Default = this;
				Current = this;
				System.Threading.SynchronizationContext.SetSynchronizationContext(ThreadContext);
			}
		}

		void OnDestroy() {
			if (_default) {
				if (Current == this) {
					Current = null;
				}
				Default = null;
				System.Threading.SynchronizationContext.SetSynchronizationContext(null);
			}
		}
		
		// Update is called once per frame
		void Update () {
			ProcessOperations(this.pendingOperations);
			if (this._default) {
				ProcessOperations(defaultPendingOperations);
			}
		}

		static void ProcessOperations(Queue<PendingOperation> pendingOperations) {
			while (pendingOperations.Count > 0) {
				var op = pendingOperations.Dequeue();
				op.callback(op.state);
			}
		}

		public static void PostToDefault(System.Threading.SendOrPostCallback d, object state) {
			defaultPendingOperations.Enqueue(new PendingOperation { callback = d, state = state });
		}

		public virtual void Post(System.Threading.SendOrPostCallback d, object state) {
			pendingOperations.Enqueue(new PendingOperation { callback = d, state = state });
		}
	}
}
