// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Online.Spectator;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Tests.Visual.Spectator
{
    public partial class TestSpectatorClient : SpectatorClient
    {
        /// <summary>
        /// Maximum number of frames sent per bundle via <see cref="SendFramesFromUser"/>.
        /// </summary>
        public const int FRAME_BUNDLE_SIZE = 10;

        /// <summary>
        /// Whether to force send operations to fail (simulating a network issue).
        /// </summary>
        public bool ShouldFailSendingFrames { get; set; }

        public int FrameSendAttempts { get; private set; }

        public override IBindable<bool> IsConnected => isConnected;
        private readonly BindableBool isConnected = new BindableBool(true);

        public IReadOnlyDictionary<int, ReplayFrame> LastReceivedUserFrames => lastReceivedUserFrames;

        private readonly Dictionary<int, ReplayFrame> lastReceivedUserFrames = new Dictionary<int, ReplayFrame>();

        private readonly Dictionary<int, int> userBeatmapDictionary = new Dictionary<int, int>();
        private readonly Dictionary<int, APIMod[]> userModsDictionary = new Dictionary<int, APIMod[]>();
        private readonly Dictionary<int, int> userNextFrameDictionary = new Dictionary<int, int>();

        private readonly HashSet<int> watchingUsers = new HashSet<int>();
        private readonly Dictionary<int, HashSet<SpectatorUser>> spectatorWaitingLists = new Dictionary<int, HashSet<SpectatorUser>>();

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        public TestSpectatorClient()
        {
            OnNewFrames += (i, bundle) => lastReceivedUserFrames[i] = bundle.Frames[^1];
        }

        /// <summary>
        /// Starts play for an arbitrary user.
        /// </summary>
        /// <param name="userId">The user to start play for.</param>
        /// <param name="beatmapId">The playing beatmap id.</param>
        /// <param name="mods">The mods the user has applied.</param>
        public void SendStartPlay(int userId, int beatmapId, APIMod[]? mods = null)
        {
            userBeatmapDictionary[userId] = beatmapId;
            userModsDictionary[userId] = mods ?? Array.Empty<APIMod>();
            userNextFrameDictionary[userId] = 0;
            sendPlayingState(userId);
        }

        /// <summary>
        /// Ends play for an arbitrary user.
        /// </summary>
        /// <param name="userId">The user to end play for.</param>
        /// <param name="state">The spectator state to end play with.</param>
        public void SendEndPlay(int userId, SpectatedUserState state = SpectatedUserState.Quit)
        {
            if (!userBeatmapDictionary.TryGetValue(userId, out int beatmapId))
                return;

            ((ISpectatorClient)this).UserFinishedPlaying(userId, new SpectatorState
            {
                BeatmapID = beatmapId,
                RulesetID = 0,
                Mods = userModsDictionary[userId],
                State = state
            });

            userBeatmapDictionary.Remove(userId);
            userModsDictionary.Remove(userId);
        }

        /// <summary>
        /// Sends frames for an arbitrary user, in bundles containing 10 frames each.
        /// This bypasses the standard queueing mechanism completely and should only be used to test cases where multiple users need to be sending data.
        /// Importantly, <see cref="ShouldFailSendingFrames"/> will have no effect.
        /// </summary>
        /// <param name="userId">The user to send frames for.</param>
        /// <param name="count">The total number of frames to send.</param>
        /// <param name="startTime">The time to start gameplay frames from.</param>
        /// <param name="initialResultCount">Add a number of misses to frame header data for testing purposes.</param>
        public void SendFramesFromUser(int userId, int count, double startTime = 0, int initialResultCount = 0)
        {
            var frames = new List<LegacyReplayFrame>();

            int currentFrameIndex = userNextFrameDictionary[userId];
            int lastFrameIndex = currentFrameIndex + count - 1;

            var scoreProcessor = new ScoreProcessor(rulesetStore.GetRuleset(0)!.CreateInstance());

            for (int i = 0; i < initialResultCount; i++)
            {
                scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new Judgement())
                {
                    Type = HitResult.Miss,
                });
            }

            for (; currentFrameIndex <= lastFrameIndex; currentFrameIndex++)
            {
                // This is done in the next frame so that currentFrameIndex is updated to the correct value.
                if (frames.Count == FRAME_BUNDLE_SIZE)
                    flush();

                var buttonState = currentFrameIndex == lastFrameIndex ? ReplayButtonState.None : ReplayButtonState.Left1;
                frames.Add(new LegacyReplayFrame(currentFrameIndex * 100 + startTime, RNG.Next(0, 512), RNG.Next(0, 512), buttonState));
            }

            flush();

            userNextFrameDictionary[userId] = currentFrameIndex;

            void flush()
            {
                if (frames.Count == 0)
                    return;

                var bundle = new FrameDataBundle(new ScoreInfo
                {
                    Combo = currentFrameIndex,
                    TotalScore = (long)(currentFrameIndex * 123478 * RNG.NextDouble(0.99, 1.01)),
                    Accuracy = RNG.NextDouble(0.98, 1),
                    Statistics = scoreProcessor.Statistics.ToDictionary(),
                }, scoreProcessor, frames.ToArray());

                if (initialResultCount > 0)
                {
                    foreach (var f in frames)
                        f.Header = bundle.Header;
                }

                scoreProcessor.ResetFromReplayFrame(frames.Last());
                ((ISpectatorClient)this).UserSentFrames(userId, bundle);

                frames.Clear();
            }
        }

        /// <summary>
        /// Adds an arbitrary spectator to an arbitrary user.
        /// </summary>
        /// <param name="userId">The user that is spectating.</param>
        /// <param name="to">The user that is being spectated.</param>
        public void AddSpectator(int userId, int to)
        {
            if (watchingUsers.Contains(to) || to == api.LocalUser.Value.Id)
            {
                ((ISpectatorClient)this).UserBeganWatching(new SpectatorUser(userId), to);

                if (to == api.LocalUser.Value.Id)
                    return;
            }

            getOrCreateWaitingList(to).Add(new SpectatorUser(userId));
        }

        /// <summary>
        /// Remove a previously watching spectator to an arbitrary user.
        /// </summary>
        /// <param name="userId">The user that is spectating.</param>
        /// <param name="from">The user that is being spectated.</param>
        public void RemoveSpectator(int userId, int from)
        {
            if (watchingUsers.Contains(from) || from == api.LocalUser.Value.Id)
            {
                ((ISpectatorClient)this).UserStoppedWatching(new SpectatorUser(userId), from);

                if (from == api.LocalUser.Value.Id)
                    return;
            }

            getOrCreateWaitingList(from).RemoveWhere(s => s.UserID == userId);
        }

        /// <summary>
        /// Change the loading state on behalf of an arbitrary user.
        /// </summary>
        /// <param name="userId">The user that has its state changed.</param>
        /// <param name="watching">The user that is being spectated.</param>
        /// <param name="hasLoaded">The loading of the spectating user.</param>
        public void ChangeSpectatorLoadingState(int userId, int watching, bool hasLoaded)
        {
            if (watchingUsers.Contains(watching) || watching == api.LocalUser.Value.Id)
            {
                ((ISpectatorClient)this).UserLoadingStateChanged(userId, watching, hasLoaded);

                if (watching == api.LocalUser.Value.Id)
                    return;
            }

            var spectator = getOrCreateWaitingList(watching).SingleOrDefault(s => s.UserID == userId);

            if (spectator == null)
                return;

            spectator.HasLoaded = hasLoaded;
        }

        /// <summary>
        /// Change the beatmap availability on behalf of an arbitrary user.
        /// </summary>
        /// <param name="userId">The user that has its state changed.</param>
        /// <param name="watching">The user that is being spectated.</param>
        /// <param name="availability">The beatmap availability of the spectating user.</param>
        public void ChangeSpectatorBeatmapAvailability(int userId, int watching, BeatmapAvailability availability)
        {
            if (watchingUsers.Contains(watching) || watching == api.LocalUser.Value.Id)
            {
                ((ISpectatorClient)this).UserBeatmapAvailabilityChanged(userId, watching, availability);

                if (watching == api.LocalUser.Value.Id)
                    return;
            }

            var spectator = getOrCreateWaitingList(watching).SingleOrDefault(s => s.UserID == userId);

            if (spectator == null)
                return;

            spectator.BeatmapAvailability = availability;
        }

        protected override Task BeginPlayingInternal(long? scoreToken, SpectatorState state)
        {
            // Track the local user's playing beatmap ID.
            Debug.Assert(state.BeatmapID != null);
            userBeatmapDictionary[api.LocalUser.Value.Id] = state.BeatmapID.Value;
            userModsDictionary[api.LocalUser.Value.Id] = state.Mods.ToArray();

            return ((ISpectatorClient)this).UserBeganPlaying(api.LocalUser.Value.Id, state);
        }

        protected override Task SendFramesInternal(FrameDataBundle bundle)
        {
            FrameSendAttempts++;

            if (ShouldFailSendingFrames)
                return Task.FromException(new InvalidOperationException($"Intentional fail via {nameof(ShouldFailSendingFrames)}"));

            return ((ISpectatorClient)this).UserSentFrames(api.LocalUser.Value.Id, bundle);
        }

        protected override Task EndPlayingInternal(SpectatorState state) => ((ISpectatorClient)this).UserFinishedPlaying(api.LocalUser.Value.Id, state);

        protected override Task<SpectatorWatchGroup?> WatchUserInternal(int userId)
        {
            // When newly watching a user, the server sends the playing state immediately.
            if (userBeatmapDictionary.ContainsKey(userId))
                sendPlayingState(userId);

            watchingUsers.Add(userId);

            spectatorWaitingLists.TryGetValue(userId, out var pendingSpectators);

            SpectatorWatchGroup watchGroup = new SpectatorWatchGroup(userId)
            {
                Spectators = pendingSpectators?.ToList() ?? new List<SpectatorUser>()
            };

            return Task.FromResult<SpectatorWatchGroup?>(watchGroup);
        }

        protected override Task StopWatchingUserInternal(int userId)
        {
            watchingUsers.Remove(userId);
            return Task.CompletedTask;
        }

        private HashSet<SpectatorUser> getOrCreateWaitingList(int userId)
        {
            if (!spectatorWaitingLists.TryGetValue(userId, out HashSet<SpectatorUser>? pendingSpectators))
                pendingSpectators = spectatorWaitingLists[userId] = new HashSet<SpectatorUser>();

            Debug.Assert(pendingSpectators != null);

            return pendingSpectators;
        }

        private void sendPlayingState(int userId)
        {
            ((ISpectatorClient)this).UserBeganPlaying(userId, new SpectatorState
            {
                BeatmapID = userBeatmapDictionary[userId],
                RulesetID = 0,
                Mods = userModsDictionary[userId],
                State = SpectatedUserState.Playing
            });
        }

        protected override Task UpdateLoadingStateInternal(int userId, bool hasLoaded) => Task.CompletedTask;

        protected override Task UpdateBeatmapAvailabilityInternal(int userId, BeatmapAvailability beatmapAvailability) => Task.CompletedTask;

        protected override async Task DisconnectInternal()
        {
            await base.DisconnectInternal().ConfigureAwait(false);
            isConnected.Value = false;
        }
    }
}
