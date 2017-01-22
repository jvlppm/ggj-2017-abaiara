namespace System.Threading
{
    public struct CancellationTokenRegistration : System.IDisposable
    {
        CancellationTokenSource source;
        System.Action handler;

        internal CancellationTokenRegistration(CancellationTokenSource source, System.Action handler) {
            this.source = source;
            this.handler = handler;
            if (this.source != null && this.handler != null) {
                this.source.onCancel += this.handler;
            }
        }

        public void Dispose()
        {
            if (this.source != null && this.handler != null) {
                this.source.onCancel -= this.handler;
            }
            this.source = null;
            this.handler = null;
        }
    }
}
