// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Game.Online.Rooms;

namespace osu.Game.Online.Spectator
{
    /// <summary>
    /// An interface defining a spectator client instance.
    /// </summary>
    public interface ISpectatorClient : IStatefulUserHubClient
    {
        /// <summary>
        /// Signals that a user has begun a new play session.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="state">The state of gameplay.</param>
        Task UserBeganPlaying(int userId, SpectatorState state);

        /// <summary>
        /// Signals that a user has finished a play session.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="state">The state of gameplay.</param>
        Task UserFinishedPlaying(int userId, SpectatorState state);

        /// <summary>
        /// Called when new frames are available for a subscribed user's play session.
        /// </summary>
        /// <param name="userId">The user.</param>
        /// <param name="data">The frame data.</param>
        Task UserSentFrames(int userId, FrameDataBundle data);

        /// <summary>
        /// Signals that a user's submitted score was fully processed.
        /// </summary>
        /// <param name="userId">The ID of the user who achieved the score.</param>
        /// <param name="scoreId">The ID of the score.</param>
        Task UserScoreProcessed(int userId, long scoreId);

        /// <summary>
        /// Signals that a user has begun spectating another user.
        /// </summary>
        /// <param name="user">The user who is watching.</param>
        /// <param name="watchedUserId">The user who is being watched.</param>
        /// <remarks>Called when already spectating someone or when playing.</remarks>
        Task UserBeganWatching(SpectatorUser user, int watchedUserId);

        /// <summary>
        /// Signals that a user has stopped spectating another user.
        /// </summary>
        /// <param name="user">The user who was watching.</param>
        /// <param name="watchedUserId">The user who was being watched.</param>
        /// <remarks>Called when already spectating someone or when playing.</remarks>
        Task UserStoppedWatching(SpectatorUser user, int watchedUserId);

        /// <summary>
        /// Signals that a user spectating someone changed their beatmap availability state.
        /// </summary>
        /// <param name="userId">The user whose beatmap availability state changed.</param>
        /// <param name="watchedUserId">The user who is being watched.</param>
        /// <param name="availability">The new beatmap availability state of the user.</param>
        /// <returns></returns>
        Task UserBeatmapAvailabilityChanged(int userId, int watchedUserId, BeatmapAvailability availability);

        /// <summary>
        /// Signals that a user spectating someone changed their loading state.
        /// </summary>
        /// <param name="userId">The user whose loading state changed.</param>
        /// <param name="watchedUserId">The user who is being watched.</param>
        /// <param name="isLoaded">If the user loaded the player.</param>
        Task UserLoadingStateChanged(int userId, int watchedUserId, bool isLoaded);
    }
}
