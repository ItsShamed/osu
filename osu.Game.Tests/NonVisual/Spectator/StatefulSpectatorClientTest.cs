// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Online.Rooms;
using osu.Game.Tests.Visual.Spectator;

namespace osu.Game.Tests.NonVisual.Spectator
{
    [HeadlessTest]
    public partial class StatefulSpectatorClientTest : SpectatorTestScene
    {
        [Test]
        public void TestFirstWatcherBasic()
        {
            Watch();

            CheckSpectatorCount(0);

            AddSpectator(42);
            CheckSpectatorContains(42);
            CheckBeatmapAvailability(42, BeatmapAvailability.Unknown());
            CheckLoadingState(42, false);

            AddSpectator(28);
            CheckSpectatorContains(28);
            CheckBeatmapAvailability(28, BeatmapAvailability.Unknown());
            CheckLoadingState(42, false);
            CheckSpectatorCount(2);

            RemoveSpectator(42);
            CheckSpectatorCount(1);
            CheckSpectatorNotContains(42);

            StopWatching();
        }

        [Test]
        public void TestFirstWatcherStateModification()
        {
            Watch();

            AddSpectator(42);
            AddSpectator(28);
            AddSpectator(23);
            AddSpectator(21);
            AddSpectator(88);

            SetBeatmapAvailability(42, BeatmapAvailability.NotDownloaded());
            CheckBeatmapAvailability(42, BeatmapAvailability.NotDownloaded());

            SetBeatmapAvailability(28, BeatmapAvailability.Downloading(0.23f));
            CheckBeatmapAvailability(28, BeatmapAvailability.Downloading(0.23f));

            SetBeatmapAvailability(23, BeatmapAvailability.Importing());
            CheckBeatmapAvailability(23, BeatmapAvailability.Importing());

            SetBeatmapAvailability(21, BeatmapAvailability.LocallyAvailable());
            CheckBeatmapAvailability(21, BeatmapAvailability.LocallyAvailable());

            SetLoadingState(88, true);
            CheckLoadingState(88, true);

            StopWatching();
        }

        [Test]
        public void TestLastWatcherBasic()
        {
            AddSpectator(42);
            SetBeatmapAvailability(42, BeatmapAvailability.LocallyAvailable());

            AddSpectator(28);
            SetLoadingState(28, true);

            Watch();

            CheckSpectatorCount(2);
            CheckSpectatorContains(42);
            CheckSpectatorContains(28);

            CheckBeatmapAvailability(42, BeatmapAvailability.LocallyAvailable());
            CheckLoadingState(28, true);

            RemoveSpectator(42);
            CheckSpectatorCount(1);
            CheckSpectatorNotContains(42);

            StopWatching();
        }

        [Test]
        public void TestLastWatcherStateModification()
        {
            AddSpectator(42);
            SetBeatmapAvailability(42, BeatmapAvailability.NotDownloaded());

            AddSpectator(28);
            SetBeatmapAvailability(28, BeatmapAvailability.Importing());

            Watch();

            CheckBeatmapAvailability(42, BeatmapAvailability.NotDownloaded());
            SetBeatmapAvailability(42, BeatmapAvailability.Downloading(0.727f));
            CheckBeatmapAvailability(42, BeatmapAvailability.Downloading(0.727f));

            CheckBeatmapAvailability(28, BeatmapAvailability.Importing());
            CheckLoadingState(28, false);
            SetBeatmapAvailability(28, BeatmapAvailability.LocallyAvailable());
            SetLoadingState(28, true);
            CheckBeatmapAvailability(28, BeatmapAvailability.LocallyAvailable());
            CheckLoadingState(28, true);

            StopWatching();
        }
    }
}
