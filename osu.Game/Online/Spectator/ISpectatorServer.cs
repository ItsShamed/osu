// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Game.Online.Rooms;
using osu.Game.Screens.Play;

namespace osu.Game.Online.Spectator
{
    /// <summary>
    /// An interface defining the spectator server instance.
    /// </summary>
    public interface ISpectatorServer
    {
        /// <summary>
        /// Signal the start of a new play session.
        /// </summary>
        /// <param name="scoreToken">The score submission token.</param>
        /// <param name="state">The state of gameplay.</param>
        Task BeginPlaySession(long? scoreToken, SpectatorState state);

        /// <summary>
        /// Send a bundle of frame data for the current play session.
        /// </summary>
        /// <param name="data">The frame data.</param>
        Task SendFrameData(FrameDataBundle data);

        /// <summary>
        /// Signal the end of a play session.
        /// </summary>
        /// <param name="state">The state of gameplay.</param>
        Task EndPlaySession(SpectatorState state);

        /// <summary>
        /// Request spectating data for the specified user. May be called on multiple users and offline users.
        /// For offline users, a subscription will be created and data will begin streaming on next play.
        /// </summary>
        /// <param name="userId">The user to subscribe to.</param>
        /// <returns>The watch group of that user.</returns>
        /// <remarks>May be <c>null</c> if this is invoked on an old server that is not returning any watch group.</remarks>
        Task<SpectatorWatchGroup?> StartWatchingUser(int userId); // nullability can be removed 20250703

        /// <summary>
        /// Announce to spectators and specified spectated user, if the local user has loaded into a <see cref="SpectatorPlayer"/> and can see the gameplay.
        /// </summary>
        /// <param name="userId">The spectated user.</param>
        /// <param name="hasLoaded">If the current user see the gameplay.</param>
        /// <returns></returns>
        Task SendLoadingState(int userId, bool hasLoaded);

        /// <summary>
        /// Announce to spectators and specified spectated user a new beatmap availability.
        /// </summary>
        /// <param name="userId">The spectated user.</param>
        /// <param name="beatmapAvailability">The new beatmap availability</param>
        /// <returns></returns>
        Task SendBeatmapAvailability(int userId, BeatmapAvailability beatmapAvailability);

        /// <summary>
        /// Stop requesting spectating data for the specified user. Unsubscribes from receiving further data.
        /// </summary>
        /// <param name="userId">The user to unsubscribe from.</param>
        Task EndWatchingUser(int userId);
    }
}
