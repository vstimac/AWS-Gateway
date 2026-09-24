namespace AWSGateway.Helpers
{
    public static class ThreadManager
    {
        // izvodi akciju na UI dretvi kontrole; ne radi ništa ako kontrola još nema handle ili je već zatvorena
        public static void SafeInvoke(Control control, Action action)
        {
            if (control.IsHandleCreated && control.IsDisposed == false)
            {
                if (control.InvokeRequired)
                {
                    control.Invoke(action);
                }
                else
                {
                    action();
                }
            }
        }
    }
}