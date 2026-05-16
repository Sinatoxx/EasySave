namespace EasySave.Services
{
    public static class PriorityCoordinator
    {
        private static readonly object _lock = new();
        private static int _pendingPriorityCount = 0;
        private static readonly ManualResetEventSlim _noMorePriority = new(true);

        public static void RegisterPriorityFiles(int count)
        {
            lock (_lock)
            {
                _pendingPriorityCount += count;
                if (_pendingPriorityCount > 0) _noMorePriority.Reset();
            }
        }

        public static void OnePriorityDone()
        {
            lock (_lock)
            {
                _pendingPriorityCount--;
                if (_pendingPriorityCount <= 0)
                {
                    _pendingPriorityCount = 0;
                    _noMorePriority.Set();
                }
            }
        }

        public static void WaitUntilNoPriorityPending(CancellationToken token)
        {
            _noMorePriority.Wait(token);
        }

        public static int GetPendingCount()
        {
            lock (_lock) return _pendingPriorityCount;
        }
    }
}