// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tests.Gameplay;
using osu.Game.Tests.Visual.Spectator;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSpectatorList : SpectatorTestScene
    {
        private SpectatorList spectatorList = null!;

        protected override Container<Drawable> Content { get; }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public TestSceneSpectatorList()
        {
            var score = new Score
            {
                ScoreInfo =
                {
                    User = new APIUser
                    {
                        Id = SPECTATED_ID,
                        Username = "Spectated user"
                    }
                }
            };

            var gameplayState1 = TestGameplayState.Create(new OsuRuleset(), score: score);
            base.Content.Add(new DependencyProvidingContainer
            {
                CachedDependencies = [(typeof(GameplayState), gameplayState1)],
                RelativeSizeAxes = Axes.Both,
                Child = Content = new Container
                {
                    RelativeSizeAxes = Axes.Both
                }
            });
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("setup components", () => Child = spectatorList = new SpectatorList
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        [Test]
        public void TestFirstWatcherSingleUser()
        {
            Watch();

            AddSpectator(42);

            SetBeatmapAvailability(42, BeatmapAvailability.NotDownloaded());
            checkItemColour(42, colours.RedLight);

            SetBeatmapAvailability(42, BeatmapAvailability.Downloading(0.32f));
            checkItemColour(42, colours.Blue);
            checkItemSpinner(42, Visibility.Hidden);

            SetBeatmapAvailability(42, BeatmapAvailability.Importing());
            checkItemColour(42, colours.Yellow);

            SetBeatmapAvailability(42, BeatmapAvailability.LocallyAvailable());
            checkItemColour(42, Colour4.White);
            checkItemSpinner(42);

            SetLoadingState(42, true);
            checkItemSpinner(42, Visibility.Hidden);

            StopWatching();
        }

        [Test]
        public void TestFirstWatcherSorting()
        {
            ignoreSpectatorLimit();
            Watch();

            AddSpectatorsRange(20, 45);

            AddStep("change states", () =>
            {
                for (int i = 20; i < 45; i++)
                {
                    switch (i % 5)
                    {
                        case 1:
                            SpectatorClient.ChangeSpectatorBeatmapAvailability(i, SPECTATED_ID, BeatmapAvailability.NotDownloaded());
                            break;

                        case 2:
                            SpectatorClient.ChangeSpectatorBeatmapAvailability(i, SPECTATED_ID, BeatmapAvailability.Downloading(RNG.NextSingle(0.99f)));
                            break;

                        case 3:
                            SpectatorClient.ChangeSpectatorBeatmapAvailability(i, SPECTATED_ID, BeatmapAvailability.Importing());
                            break;

                        case 4:
                            SpectatorClient.ChangeSpectatorBeatmapAvailability(i, SPECTATED_ID, BeatmapAvailability.LocallyAvailable());
                            break;
                    }
                }
            });

            checkSorted();

            StopWatching();
        }

        [Test]
        public void TestPreSeededWatchGroupSingleUser()
        {
            AddSpectator(20);
            SetBeatmapAvailability(20, BeatmapAvailability.NotDownloaded());

            Watch();

            checkItemPresent(20);
            checkItemColour(20, colours.RedLight);

            StopWatching();
        }

        [Test]
        public void TestSpectatorLimit()
        {
            AddSpectatorsRange(20, 30);

            Watch();

            AddAssert("flow is visible", () => getItems().IsPresent, () => Is.True);

            AddSpectator(31);
            AddUntilStep("flow is not visible", () => getItems().IsPresent, () => Is.False);

            RemoveSpectator(31);
            AddUntilStep("flow is visible", () => getItems().IsPresent, () => Is.True);

            StopWatching();
        }

        private void ignoreSpectatorLimit() => AddStep("ignore spectator limit", () => spectatorList.IgnoreSpectatorLimit = true);

        private void checkSorted() => AddAssert("users are sorted", () =>
        {
            var flow = getItems();
            var groups = flow.GroupBy(si => si.BeatmapAvailability.State)
                             .OrderBy(g => g.Key)
                             .ToList();

            for (int i = 0; i < groups.Count; i++)
            {
                var layoutOrderedItems = groups[i].OrderBy(si => getItems().GetLayoutPosition(si)).ToList();
                var idOrderedItems = groups[i].OrderBy(si => si.UserID).ToList();

                // Layout should have the same ordering as `OrderBy(userId)`
                if (!layoutOrderedItems.SequenceEqual(idOrderedItems))
                    return false;

                if (i + 1 >= groups.Count)
                    continue;

                var itemLayoutPositions = groups[i].Select(si => flow.GetLayoutPosition(si)).ToList();
                var nextItemLayoutPositions = groups[i + 1].Select(si => flow.GetLayoutPosition(si)).ToList();

                // Any random item from each group should respect the ordering by state
                return itemLayoutPositions[RNG.Next(itemLayoutPositions.Count)] > nextItemLayoutPositions[RNG.Next(nextItemLayoutPositions.Count)];
            }

            return true;
        });

        private void checkItemSpinner(int userId, Visibility visibility = Visibility.Visible)
            => AddAssert($"spinner is {visibility} for user {userId}",
                () => getItemOf(userId)?.ChildrenOfType<LoadingSpinner>().Single().State.Value, () => Is.EqualTo(visibility));

        private void checkItemColour(int userId, ColourInfo colour)
            => AddAssert($"user {userId} has colour {colour}",
                () => getItemOf(userId)?.ChildrenOfType<OsuSpriteText>().Single().Colour, () => Is.EqualTo(colour));

        private void checkItemPresent(int userId)
            => AddAssert($"user {userId} is present", () => getItemOf(userId), () => Is.Not.Null);

        private FillFlowContainer<SpectatorList.SpectatorListItem> getItems()
            => spectatorList.Children.OfType<FillFlowContainer<SpectatorList.SpectatorListItem>>().Single();

        private SpectatorList.SpectatorListItem? getItemOf(int userId)
            => getItems().SingleOrDefault(i => i.UserID == userId);
    }
}
