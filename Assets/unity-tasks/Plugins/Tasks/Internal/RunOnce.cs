using System.Collections;

namespace System.Threading.Tasks
{
	struct RunOnce : IEnumerator {
		Action action;

		public RunOnce(Action action) {
			this.action = action;
		}

		public bool MoveNext() {
			action();
			return false;
		}

		void IEnumerator.Reset() {
			throw new System.NotImplementedException();
		}

		public object Current { get { return null; } }
	}
}
