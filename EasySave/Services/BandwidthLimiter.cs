namespace EasySave.Services
{
    public static class BandwidthLimiter
    {
        private static readonly SemaphoreSlim _largeFileSlot = new(1, 1);
        private static long _maxParallelSizeBytes = 1024 * 1024; // 1 Mo par défaut

        public static void Configure(long maxKB)
        {
            _maxParallelSizeBytes = maxKB * 1024;
        }

        public static bool IsLargeFile(long fileSizeBytes) => fileSizeBytes > _maxParallelSizeBytes;

        public static void AcquireSlotForLargeFile(CancellationToken token)
        {
            _largeFileSlot.Wait(token);
        }

        public static void ReleaseSlotForLargeFile()
        {
            _largeFileSlot.Release();
        }
    }
}