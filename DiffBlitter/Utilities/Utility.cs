using System.ComponentModel;

namespace DiffBlitter.Utilities
{
    public static class Utility
    {
        // TODO: cancellation token?
        public static BackgroundWorker CreateBackgroundWorker(DoWorkEventHandler workHandler,
            ProgressChangedEventHandler progressHandler = null, RunWorkerCompletedEventHandler completedHandler = null)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += workHandler;
            worker.WorkerReportsProgress = progressHandler != null;
            worker.ProgressChanged += progressHandler;
            worker.RunWorkerCompleted += completedHandler;
            return worker;
        }
    }
}
