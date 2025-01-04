// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Spectator
{
    /// <summary>
    /// Interface that defines the required dependencies for spectator test scenes.
    /// </summary>
    public interface ISpectatorTestSceneDependencies : IOnlinePlayTestSceneDependencies
    {
        /// <summary>
        /// The cached <see cref="SpectatorClient"/>
        /// </summary>
        TestSpectatorClient SpectatorClient { get; }
    }
}
