namespace System.Threading.Tasks
{
#if !UNITY_5_2
    public class TaskYieldInstruction : UnityEngine.CustomYieldInstruction
    {
        Task task;

        public TaskYieldInstruction(Task task) {
            if (task == null) {
                throw new ArgumentNullException("task");
            }
            this.task = task;
        }

        public override bool keepWaiting {
            get { return !isDone; }
        }

        public TaskStatus status {
            get { return task.Status; }
        }

        public System.Exception error {
            get { return task.Error; }
        }

        public bool isCanceled {
            get { return task.IsCanceled; }
        }

        public bool isDone {
            get { return task.IsCompleted; }
        }
    }

    public class TaskYieldInstruction<T> : TaskYieldInstruction
    {
        Task<T> task;

        public TaskYieldInstruction(Task<T> task) : base(task) {
            if (task == null) {
                throw new ArgumentNullException("task");
            }
            this.task = task;
        }

        public T result {
            get {
                return task.Result;
            }
        }
    }
#endif
}
