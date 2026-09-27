namespace Dotstrap.Integrations
{
    // purely local session-length logging - no network calls, no effect on gameplay
    public class PlaytimeTracker : IDisposable
    {
        private readonly ActivityWatcher _activityWatcher;

        private DateTime _sessionStart;

        public PlaytimeTracker(ActivityWatcher activityWatcher)
        {
            _activityWatcher = activityWatcher;

            _activityWatcher.OnGameJoin += OnGameJoin;
            _activityWatcher.OnGameLeave += OnGameLeave;
        }

        private void OnGameJoin(object? sender, EventArgs e) => _sessionStart = DateTime.Now;

        private void OnGameLeave(object? sender, EventArgs e)
        {
            const string LOG_IDENT = "PlaytimeTracker::OnGameLeave";

            long universeId = _activityWatcher.Data.UniverseId;

            if (universeId == 0 || _sessionStart == default)
                return;

            var elapsed = DateTime.Now - _sessionStart;
            _sessionStart = default;

            if (elapsed <= TimeSpan.Zero)
                return;

            var games = App.PlaytimeState.Prop.Games;

            if (!games.TryGetValue(universeId, out var entry))
            {
                entry = new();
                games[universeId] = entry;
            }

            if (!String.IsNullOrEmpty(_activityWatcher.Data.UniverseDetails?.Data.Name))
                entry.Name = _activityWatcher.Data.UniverseDetails.Data.Name;

            entry.TotalSeconds += (long)elapsed.TotalSeconds;
            entry.SessionCount += 1;
            entry.LastPlayed = DateTime.Now;

            App.PlaytimeState.Save();

            App.Logger.WriteLine(LOG_IDENT, $"Recorded {elapsed.TotalSeconds:0}s of playtime for universe {universeId}");
        }

        public void Dispose()
        {
            _activityWatcher.OnGameJoin -= OnGameJoin;
            _activityWatcher.OnGameLeave -= OnGameLeave;

            GC.SuppressFinalize(this);
        }
    }
}
