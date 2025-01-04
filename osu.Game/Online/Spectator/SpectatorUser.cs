// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;

namespace osu.Game.Online.Spectator
{
    /// <summary>
    /// A user spectating another user.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public class SpectatorUser : IEquatable<SpectatorUser>
    {
        /// <summary>
        /// The online ID of the user.
        /// </summary>
        [Key(0)]
        public readonly int UserID;

        /// <summary>
        /// Whether this user has its player loaded.
        /// </summary>
        [Key(1)]
        public bool HasLoaded { get; set; }

        /// <summary>
        /// The beatmap availability of this user.
        /// </summary>
        [Key(2)]
        public BeatmapAvailability BeatmapAvailability { get; set; } = BeatmapAvailability.Unknown();

        [IgnoreMember]
        public APIUser? User { get; set; }

        public SpectatorUser(int userID)
        {
            UserID = userID;
        }

        public bool Equals(SpectatorUser? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return UserID == other.UserID;
        }

        public override string ToString() => $"user: {UserID}, loaded: {HasLoaded}, availability: ({BeatmapAvailability})";
    }
}
