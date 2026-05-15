using System.Diagnostics;

namespace EasySave.Services
{
    public class BusinessAppWatcher
    {
        private readonly BusinessAppService _businessAppService;
        private readonly Action<bool> _onStateChanged;
        private CancellationTokenSource? _cts;
        private bool _wasRunning;

        public BusinessAppWatcher(BusinessAppService businessAppService, Action<bool> onStateChanged)
        {
            _businessAppService = businessAppService;
            _onStateChanged = onStateChanged;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    bool running = _businessAppService.IsBusinessAppRunning();
                    if (running != _wasRunning)
                    {
                        _wasRunning = running;
                        _onStateChanged(running);
                    }
                    try { await Task.Delay(1000, _cts.Token); }
                    catch (TaskCanceledException) { break; }
                }
            });
        }

        public void Stop() => _cts?.Cancel();
    }
}