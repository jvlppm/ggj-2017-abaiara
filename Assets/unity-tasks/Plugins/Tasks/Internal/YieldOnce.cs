using System.Collections;

namespace System.Threading.Tasks
{
	struct YieldOnce : IEnumerator {
		object value;
		bool complete;

		public YieldOnce(object value) {
			this.value = value;
			this.complete = false;
		}

		public bool MoveNext() {
			if (!complete) {
				complete = true;
				return true;
			}
			return false;
		}

		void IEnumerator.Reset() {
			throw new System.NotImplementedException();
		}

		public object Current { get { return value; } }
	}
}
