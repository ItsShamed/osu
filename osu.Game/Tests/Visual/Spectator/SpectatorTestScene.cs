// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Online.Rooms;
using osu.Game.Online.Spectator;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Spectator
{
    public abstract partial class SpectatorTestScene : OnlinePlayTestScene, ISpectatorTestSceneDependencies
    {
        protected const int SPECTATED_ID = 10;
        public TestSpectatorClient SpectatorClient => OnlinePlayDependencies.SpectatorClient;

        public new SpectatorTestSceneDependencies OnlinePlayDependencies => (SpectatorTestSceneDependencies)base.OnlinePlayDependencies;

        protected override OnlinePlayTestSceneDependencies CreateOnlinePlayDependencies() => new SpectatorTestSceneDependencies();

        protected void AddSpectator(int userId) => AddStep($"add spectator {userId}", () => SpectatorClient.AddSpectator(userId, SPECTATED_ID));

        protected void AddSpectatorsRange(int minId, int maxId) => AddStep($"add spectators {minId} to {maxId}", () =>
        {
            for (int i = minId; i < maxId; i++)
                SpectatorClient.AddSpectator(i, SPECTATED_ID);
        });

        protected void RemoveSpectator(int userId) => AddStep($"remove spectator {userId}", () => SpectatorClient.RemoveSpectator(userId, SPECTATED_ID));

        protected void SetLoadingState(int userId, bool hasLoaded)
            => AddStep($"make spectator {userId} {(hasLoaded ? "loaded" : "not loaded")}", () => SpectatorClient.ChangeSpectatorLoadingState(userId, SPECTATED_ID, hasLoaded));

        protected void SetBeatmapAvailability(int userId, BeatmapAvailability availability)
            => AddStep($"change spectator {userId} beatmap availability to {availability}",
                () => SpectatorClient.ChangeSpectatorBeatmapAvailability(userId, SPECTATED_ID, availability));

        protected void Watch() => AddStep("start watching", () => SpectatorClient.WatchUser(SPECTATED_ID));
        protected void StopWatching() => AddStep("stop watching", () => SpectatorClient.StopWatchingUser(SPECTATED_ID));

        protected void CheckSpectatorCount(int count) =>
            AddAssert($"spectator count is {count}", () => GetSpectators(SPECTATED_ID)?.Count ?? 0, () => Is.EqualTo(count));

        protected void CheckSpectatorContains(int spectator) =>
            AddAssert($"spectators contains user {spectator}", () => GetSpectators(SPECTATED_ID)?.Any(u => u.UserID == spectator) ?? false);

        protected void CheckSpectatorNotContains(int spectator) =>
            AddAssert($"spectators does not contains user {spectator}", () => GetSpectators(SPECTATED_ID)?.All(u => u.UserID != spectator) ?? false);

        protected void CheckLoadingState(int spectator, bool hasLoaded) =>
            AddAssert($"spectator {spectator} has {(hasLoaded ? "loaded" : "not loaded")}",
                () => GetSpectators(SPECTATED_ID)?.SingleOrDefault(s => s.UserID == spectator)?.HasLoaded, () => hasLoaded ? Is.True : Is.False);

        protected void CheckBeatmapAvailability(int spectator, BeatmapAvailability availability) =>
            AddAssert($"beatmap availability of spectator {spectator} is {availability}",
                () => GetSpectators(SPECTATED_ID)?.SingleOrDefault(s => s.UserID == spectator)?.BeatmapAvailability, () => Is.EqualTo(availability));

        protected IList<SpectatorUser>? GetSpectators(int userId) => SpectatorClient.GetSpectators(userId)?.Spectators;
    }
}
