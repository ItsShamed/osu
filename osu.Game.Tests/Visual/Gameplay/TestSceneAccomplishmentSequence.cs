// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneAccomplishmentSequence : OsuTestScene
    {
        private ScoreProcessor scoreProcessor = null!;
        private AccomplishmentSequence sequence = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("setup components", () =>
            {
                Children = new Drawable[]
                {
                    scoreProcessor = new ScoreProcessor(new OsuRuleset()),
                    new DependencyProvidingContainer
                    {
                        CachedDependencies = new (Type, object)[] { (typeof(ScoreProcessor), scoreProcessor) },
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            sequence = new DefaultAccomplishmentSequence(),
                        }
                    }
                };
            });
            AddUntilStep("wait for components to load", () => sequence.IsLoaded && scoreProcessor.IsLoaded);
        }

        [Test]
        public void TestAllPerfect()
        {
            populateBeatmap(new[] { new HitObject(), new HitObject() });
            issueResult(HitResult.Perfect);
            issueResult(HitResult.Great);
            AddAssert("sequence is alive", () => sequence.IsAlive);
        }

        [Test]
        public void TestFullCombo()
        {
            populateBeatmap(new[] { new HitObject(), new HitObject(), new HitObject() });
            issueResult(HitResult.Perfect);
            issueResult(HitResult.Great);
            issueResult(HitResult.Good);
            AddAssert("sequence is alive", () => sequence.IsAlive);
        }

        [Test]
        public void TestNoBreak()
        {
            populateBeatmap(new[] { new HitObject(), new HitObject(), new HitObject(), new HitObject() });
            issueResult(HitResult.Perfect);
            issueResult(HitResult.Great);
            issueResult(HitResult.IgnoreMiss);
            issueResult(HitResult.Good);
            AddAssert("sequence is alive", () => sequence.IsAlive);
        }

        [Test]
        public void TestMiss()
        {
            populateBeatmap(new[] { new HitObject(), new HitObject(), new HitObject(), new HitObject(), new HitObject() });
            issueResult(HitResult.Perfect);
            issueResult(HitResult.Great);
            issueResult(HitResult.IgnoreMiss);
            issueResult(HitResult.Good);
            issueResult(HitResult.Miss);
            AddAssert("sequence expired", () => !sequence.IsAlive);
        }

        [Test]
        public void TestSliderBreak()
        {
            populateBeatmap(new[] { new HitObject(), new HitObject(), new HitObject(), new HitObject(), new HitObject() });
            issueResult(HitResult.Perfect);
            issueResult(HitResult.Great);
            issueResult(HitResult.IgnoreMiss);
            issueResult(HitResult.Good);
            issueResult(HitResult.LargeTickMiss);
            AddAssert("sequence expired", () => !sequence.IsAlive);
        }

        private void populateBeatmap(HitObject[] objects) => AddStep($"create beatmap with {objects.Length} objects", () =>
        {
            var beatmap = new Beatmap<HitObject> { HitObjects = objects.ToList() };
            scoreProcessor.ApplyBeatmap(beatmap);
        });

        private void issueResult(HitResult result) => AddStep($"issue {result.GetDescription()} result", () =>
        {
            scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new Judgement()) { Type = result });
        });
    }
}
