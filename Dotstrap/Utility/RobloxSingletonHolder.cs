namespace Dotstrap.Utility
{
    /// <summary>
    /// Holds Roblox's singleton mutex so that newly started Roblox clients open their own window
    /// instead of handing off to the one that's already running (multi-instance launching).
    /// Nothing is done to the Roblox process itself - the mutex just has to be owned before the client starts.
    /// </summary>
    public sealed class RobloxSingletonHolder : IDisposable
    {
        public const string MutexName = "ROBLOX_singletonMutex";

        private readonly ManualResetEventSlim _ready = new(false);
        private readonly ManualResetEventSlim _release = new(false);
        private readonly Thread _thread;

        public bool IsOwned { get; private set; }

        private RobloxSingletonHolder()
        {
            // mutex ownership belongs to a thread, and it gets abandoned (i.e. roblox gets it back) if that thread exits,
            // so it lives on a dedicated thread rather than on whatever thread pool thread an await resumes on
            _thread = new Thread(HoldMutex)
            {
                IsBackground = true,
                Name = "RobloxSingletonHolder"
            };
        }

        /// <summary>
        /// Takes ownership of the singleton mutex. Returns null if another process owns it -
        /// either another Dotstrap instance that's already holding it, or a Roblox client started without it.
        /// </summary>
        public static RobloxSingletonHolder? TryAcquire()
        {
            const string LOG_IDENT = "RobloxSingletonHolder::TryAcquire";

            var holder = new RobloxSingletonHolder();

            holder._thread.Start();
            holder._ready.Wait();

            if (holder.IsOwned)
            {
                App.Logger.WriteLine(LOG_IDENT, "Acquired Roblox singleton mutex");
                return holder;
            }

            App.Logger.WriteLine(LOG_IDENT, "Roblox singleton mutex is owned by another process");
            holder.Dispose();
            return null;
        }

        /// <summary>
        /// Whether any Roblox client is open. Roblox's tray mode keeps a windowless RobloxPlayerBeta running in the background
        /// (and starts it with Windows), which isn't a client and would otherwise keep us waiting forever.
        /// </summary>
        public static bool IsRobloxPlayerRunning()
        {
            var processes = Process.GetProcessesByName(App.RobloxPlayerAppName);

            bool running = processes.Any(IsClientProcess);

            foreach (var process in processes)
                process.Dispose();

            return running;
        }

        private static bool IsClientProcess(Process process)
        {
            try
            {
                // a minimized client still has its window, and a client that's only just started may not have one yet
                return process.MainWindowHandle != IntPtr.Zero || DateTime.Now - process.StartTime < TimeSpan.FromSeconds(30);
            }
            catch
            {
                // exited, or no access - treat it like before and count it
                return true;
            }
        }

        private void HoldMutex()
        {
            using var mutex = new Mutex(false, MutexName);

            try
            {
                IsOwned = mutex.WaitOne(0);
            }
            catch (AbandonedMutexException)
            {
                // left behind by a client that didn't exit cleanly - we own it now
                IsOwned = true;
            }

            _ready.Set();

            if (!IsOwned)
                return;

            _release.Wait();
            mutex.ReleaseMutex();
        }

        public void Dispose()
        {
            _release.Set();
            _thread.Join();

            _ready.Dispose();
            _release.Dispose();
        }
    }
}
