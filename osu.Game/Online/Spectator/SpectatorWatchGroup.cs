// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using osu.Game.Utils;

namespace osu.Game.Online.Spectator
{
    /// <summary>
    /// A group of users watching the same user.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public class SpectatorWatchGroup : IDeepCloneable<SpectatorWatchGroup>
    {
        /// <summary>
        /// The ID of the user who is being watched.
        /// </summary>
        [Key(0)]
        public readonly int UserID;

        /// <summary>
        /// The list of <see cref="SpectatorUser"/>s in this group.
        /// </summary>
        [Key(1)]
        public IList<SpectatorUser> Spectators { get; set; } = new List<SpectatorUser>();

        public SpectatorWatchGroup(int userID)
        {
            UserID = userID;
        }

        public SpectatorWatchGroup DeepClone() => new SpectatorWatchGroup(UserID)
        {
            Spectators = new List<SpectatorUser>(Spectators.Select(s => s.DeepClone()))
        };
    }
}
