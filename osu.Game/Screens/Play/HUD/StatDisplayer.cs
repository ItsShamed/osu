// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class StatDisplayer : RollingCounter<double>, ISkinnableDrawable
    {
        [SettingSource("Beatmap statistic", "Which statistic of the beatmap to choose.")]
        public Bindable<Stat> BeatmapStat { get; set; } = new Bindable<Stat>(Stat.BPM);

        [SettingSource("Decimal places", "The count of floating point numbers.")]
        public Bindable<DecimalPlaces> FloatingPoints { get; set; } = new Bindable<DecimalPlaces>();

        protected override double RollingDuration => 1000;

        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        [Resolved]
        private IGameplayClock gameplayClock { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private DrawableRuleset? drawableRuleset { get; set; }

        private IGameplayClock playerClock => drawableRuleset?.FrameStableClock ?? gameplayClock;

        private List<TimedDifficultyAttributes>? timedAttributes;

        private readonly CancellationTokenSource loadCancellationSource = new CancellationTokenSource();

        protected override void Update()
        {
            base.Update();

            Current.Value = getValueFromStat(BeatmapStat.Value);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, BeatmapDifficultyCache difficultyCache)
        {
            Colour = colours.BlueLighter;

            var clonedMods = gameplayState.Mods.Select(m => m.DeepClone()).ToArray();
            var gameplayWorkingBeatmap = new PerformancePointsCounter.GameplayWorkingBeatmap(gameplayState.Beatmap);
            difficultyCache.GetTimedDifficultyAttributesAsync(gameplayWorkingBeatmap, gameplayState.Ruleset, clonedMods, loadCancellationSource.Token)
                           .ContinueWith(task => Schedule(() =>
                           {
                               timedAttributes = task.GetResultSafely();
                           }), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private DifficultyAttributes? getAttributeAtTime(double time)
        {
            if (timedAttributes == null || timedAttributes.Count == 0)
                return null;

            int attribIndex = timedAttributes.BinarySearch(new TimedDifficultyAttributes(time, null));
            if (attribIndex < 0)
                attribIndex = ~attribIndex - 1;

            return timedAttributes[Math.Clamp(attribIndex, 0, timedAttributes.Count - 1)].Attributes;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            FloatingPoints.BindValueChanged(e => SetCountWithoutRolling(Current.Value));
        }

        private double getValueFromStat(Stat stat)
        {
            return stat switch
            {
                Stat.AR => gameplayState.Beatmap.Difficulty.ApproachRate,
                Stat.CS => gameplayState.Beatmap.Difficulty.CircleSize,
                Stat.OD => gameplayState.Beatmap.Difficulty.OverallDifficulty,
                Stat.HP => gameplayState.Beatmap.Difficulty.DrainRate,
                Stat.SR => getAttributeAtTime(gameplayClock.CurrentTime)?.StarRating ?? gameplayState.Beatmap.Difficulty.OverallDifficulty,
                Stat.BPM => gameplayState.Beatmap.ControlPointInfo.TimingPointAt(playerClock.CurrentTime).BPM * gameplayClock.GetTrueGameplayRate(),
                _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, $"Unknown stat {nameof(stat)}")
            };
        }

        public enum Stat
        {
            AR,
            CS,
            OD,
            HP,
            SR,
            BPM
        }

        protected override LocalisableString FormatCount(double count)
        {
            return count.ToString($"F{(int)FloatingPoints.Value}");
        }

        public enum DecimalPlaces
        {
            NoDecimalPlaces = 0,
            OneDecimalPlace = 1,
            TwoDecimalPlace = 2
        }

        protected override IHasText CreateText() => new TextComponent(this);

        private class TextComponent : CompositeDrawable, IHasText
        {
            public LocalisableString Text
            {
                get => text.Text;
                set => text.Text = value;
            }

            private readonly OsuSpriteText text;

            public TextComponent(StatDisplayer statDisplayer)
            {
                AutoSizeAxes = Axes.Both;

                OsuSpriteText statSuffix;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(2),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 16, fixedWidth: true)
                        },
                        statSuffix = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Text = statDisplayer.BeatmapStat.Value.ToString(),
                            Font = OsuFont.Numeric.With(size: 8),
                            Padding = new MarginPadding { Bottom = 1.5f }, // align baseline better
                        }
                    }
                };

                statDisplayer.BeatmapStat.BindValueChanged(e =>
                {
                    statSuffix.Text = e.NewValue.ToString();
                });
            }
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
