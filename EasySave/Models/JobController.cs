namespace EasySave.Models
{
    public class JobController
    {
        private readonly CancellationTokenSource _cancelSource = new();
        private readonly ManualResetEventSlim _pauseHandle = new(true);

        public CancellationToken CancelToken => _cancelSource.Token;

        public void Pause() => _pauseHandle.Reset();

        public void Resume() => _pauseHandle.Set();

        public void Stop()
        {
            _pauseHandle.Set(); // débloque si en pause pour que le thread puisse voir l'annulation
            _cancelSource.Cancel();
        }

        // Appelé avant chaque fichier : bloque si en pause, lève OperationCanceledException si stoppé
        public void CheckPoint() => _pauseHandle.Wait(_cancelSource.Token);
    }
}
