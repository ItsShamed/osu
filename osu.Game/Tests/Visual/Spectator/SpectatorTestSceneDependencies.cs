// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Spectator;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Spectator
{
    public class SpectatorTestSceneDependencies : OnlinePlayTestSceneDependencies, ISpectatorTestSceneDependencies
    {
        public TestSpectatorClient SpectatorClient { get; }

        public SpectatorTestSceneDependencies()
        {
            SpectatorClient = new TestSpectatorClient();

            CacheAs<SpectatorClient>(SpectatorClient);
        }
    }
}
