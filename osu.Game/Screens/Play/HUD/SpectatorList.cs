// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Online.Spectator;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class SpectatorList : VisibilityContainer
    {
        private const int max_spectators = 10;

        private readonly Cached sorting = new Cached();

        private readonly OsuSpriteText spectatorHeader;
        private readonly FillFlowContainer<SpectatorListItem> flow;

        private bool populated;

        public bool IgnoreSpectatorLimit { private get; set; }

        [Resolved]
        private SpectatorClient? spectatorClient { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private GameplayState? state { get; set; }

        public SpectatorList()
        {
            AutoSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                spectatorHeader = new OsuSpriteText
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Font = OsuFont.Torus.With(size: 18, weight: FontWeight.SemiBold),
                    Text = SpectatorStrings.SpectatorListTitle(),
                },
                flow = new FillFlowContainer<SpectatorListItem>
                {
                    AutoSizeAxes = Axes.Both,
                    Y = 18,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Direction = FillDirection.Vertical,
                    LayoutDuration = 450,
                    LayoutEasing = Easing.OutQuint,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            spectatorHeader.Colour = colours.Blue0;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (spectatorClient == null)
                return;

            spectatorClient.OnUserBeganWatching += addSpectator;
            spectatorClient.OnUserStoppedWatching += removeSpectator;
            spectatorClient.OnWatchGroupChanged += onWatchGroupChanged;

            flow.Clear();
            populated = false;
            updateState();
        }

        protected override void PopIn()
        {
            this.FadeInFromZero(300);
        }

        protected override void PopOut()
        {
            this.FadeOutFromOne(300);
        }

        protected override void Update()
        {
            base.Update();

            if (!sorting.IsValid)
                sort();
        }

        protected virtual int TrackedUserId => state?.Score.ScoreInfo.User.Id ?? api.LocalUser.Value.Id;

        private void onWatchGroupChanged(SpectatorWatchGroup _) => updateState();

        private void sort()
        {
            if (sorting.IsValid)
                return;

            if (spectatorCount > max_spectators && !IgnoreSpectatorLimit)
                return;

            var orderedItems = flow.OrderByDescending(i => i.BeatmapAvailability.State)
                                   .ThenBy(i => i.UserID)
                                   .ToList();

            for (int i = 0; i < flow.Count; i++)
                flow.SetLayoutPosition(orderedItems[i], i);

            sorting.Validate();
        }

        private void addSpectator(SpectatorUser user, int watched)
        {
            // Let updateState populate initial users
            if (!populated)
                return;

            if (watched != TrackedUserId)
                return;

            if (flow.Any(i => i.UserID == user.UserID))
                return;

            flow.Add(new SpectatorListItem(user, this));
            spectatorHeader.Text = SpectatorStrings.SpectatorListTitle(spectatorCount);

            updateVisibility();
            sorting.Invalidate();
        }

        private void removeSpectator(SpectatorUser user, int watched)
        {
            if (watched != TrackedUserId)
                return;

            flow.SingleOrDefault(i => i.UserID == user.UserID)?.Remove();
            spectatorHeader.Text = SpectatorStrings.SpectatorListTitle(spectatorCount);

            updateVisibility();
            sorting.Invalidate();
        }

        private void updateState()
        {
            if (spectatorClient == null)
                return;

            var watchGroup = spectatorClient.GetSpectators(TrackedUserId);

            // In case of a disconnect, always depopulate
            if (watchGroup == null)
            {
                foreach (var listItem in flow)
                    listItem.Remove();
                spectatorHeader.Text = SpectatorStrings.SpectatorListTitle();
                updateVisibility();
                populated = false;
                return;
            }

            if (populated)
                return;

            foreach (var spectatorUser in watchGroup.Spectators) flow.Add(new SpectatorListItem(spectatorUser, this));

            populated = true;

            spectatorHeader.Text = SpectatorStrings.SpectatorListTitle(watchGroup.Spectators.Count);
            sorting.Invalidate();
            updateVisibility();
        }

        private void updateVisibility()
        {
            if (spectatorCount > 0)
            {
                if (State.Value != Visibility.Visible)
                    Show();
            }
            else
            {
                if (State.Value != Visibility.Hidden)
                    Hide();
            }

            if (!IgnoreSpectatorLimit && spectatorClient?.GetSpectators(TrackedUserId)?.Spectators.Count > max_spectators)
                collapseList();
            else
                expandList();
        }

        private int spectatorCount => spectatorClient?.GetSpectators(TrackedUserId)?.Spectators.Count ?? 0;

        private void collapseList() => flow.FadeOut(100);
        private void expandList() => flow.FadeIn(100);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (spectatorClient == null)
                return;

            spectatorClient.OnUserBeganWatching -= addSpectator;
            spectatorClient.OnUserStoppedWatching -= removeSpectator;
            spectatorClient.OnWatchGroupChanged -= onWatchGroupChanged;
        }

        public partial class SpectatorListItem : CompositeDrawable
        {
            public readonly int UserID;

            public bool HasLoaded { get; private set; }
            public BeatmapAvailability BeatmapAvailability { get; private set; }
            private readonly SpectatorList list;
            private readonly LoadingSpinner spinner;

            private readonly Cached userState = new Cached();

            private string username;

            private readonly OsuSpriteText text;

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            [Resolved]
            private UserLookupCache userLookupCache { get; set; } = null!;

            [Resolved]
            private SpectatorClient? spectatorClient { get; set; }

            public SpectatorListItem(SpectatorUser user, SpectatorList list)
            {
                this.list = list;
                UserID = user.UserID;
                BeatmapAvailability = user.BeatmapAvailability;
                HasLoaded = user.HasLoaded;

                if (user.User == null)
                {
                    username = "loading...";
                    resolveUsername(user.UserID);
                }
                else
                    username = user.User.Username;

                HasLoaded = user.HasLoaded;
                BeatmapAvailability = user.BeatmapAvailability;

                Name = $"Spectator list item \"{username}\" ({UserID})";

                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    Direction = FillDirection.Horizontal,
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(4),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = username,
                        },
                        spinner = new LoadingSpinner
                        {
                            Size = new Vector2(12),
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Padding = new MarginPadding { Bottom = 2f }, // match baseline with text
                            State = { Value = Visibility.Hidden }
                        }
                    },
                };
            }

            public void Remove()
            {
                this.FadeOutFromOne(250f, Easing.OutQuint)
                    .ScaleTo(0.5f, 250f, Easing.OutQuint)
                    .Then().Expire();
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                text.Colour = colours.Gray5;

                if (spectatorClient == null)
                    return;

                spectatorClient.OnUserChangedState += onUserChangedState;

                // By the time we load, the user might have already changed state
                var user = spectatorClient.GetSpectators(list.TrackedUserId)?.Spectators.SingleOrDefault(u => u.UserID == UserID);
                if (user != null)
                    onUserChangedState(user, list.TrackedUserId);
            }

            protected override void Update()
            {
                base.Update();

                if (!userState.IsValid)
                    updateState();
            }

            private void onUserChangedState(SpectatorUser user, int watched)
            {
                if (watched != list.TrackedUserId)
                    return;

                if (user.UserID != UserID)
                    return;

                HasLoaded = user.HasLoaded;
                BeatmapAvailability = user.BeatmapAvailability;

                userState.Invalidate();
                list.sorting.Invalidate();
            }

            private void resolveUsername(int userId) => Scheduler.AddOnce(() =>
            {
                userLookupCache.GetUserAsync(userId).ContinueWith(t =>
                {
                    if (t.IsCompleted)
                    {
                        string? resolvedUsername = t.GetResultSafely()?.Username;

                        if (resolvedUsername != null)
                        {
                            Schedule(() => username = resolvedUsername);
                            return;
                        }
                    }

                    Schedule(() => username = $"[unresolved {userId}]");
                });
            });

            private void updateState()
            {
                const double text_fading_time = 100f;

                switch (BeatmapAvailability.State)
                {
                    case DownloadState.Unknown:
                        text.FadeColour(colours.Gray5, text_fading_time).FadeTo(0.75f, text_fading_time);
                        text.Text = username;
                        break;

                    case DownloadState.NotDownloaded:
                        text.FadeColour(colours.RedLight, text_fading_time).FadeTo(1f, text_fading_time);
                        text.Text = username;
                        break;

                    case DownloadState.Downloading:
                        text.FadeColour(colours.Blue, text_fading_time).FadeTo(0.90f, text_fading_time);
                        text.Text = $"{username} ({BeatmapAvailability.DownloadProgress:0.0%})";
                        break;

                    case DownloadState.Importing:
                        text.FadeColour(colours.Yellow, text_fading_time).FadeTo(1f, text_fading_time);
                        text.Text = username;
                        break;

                    case DownloadState.LocallyAvailable:
                        text.FadeColour(Colour4.White, text_fading_time).FadeTo(1f, text_fading_time);
                        text.Text = username;
                        break;
                }

                if (BeatmapAvailability.State == DownloadState.LocallyAvailable && !HasLoaded)
                    spinner.Show();
                else
                    spinner.Hide();

                userState.Validate();
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);

                if (spectatorClient != null)
                    spectatorClient.OnUserChangedState -= onUserChangedState;
            }
        }
    }
}
