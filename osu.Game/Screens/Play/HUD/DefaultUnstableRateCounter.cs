// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public class DefaultUnstableRateCounter : GameplayUnstableRateCounter
    {
        private const float alpha_when_invalid = 0.3f;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueLighter;
        }

        protected override void LoadComplete()
        {
            Valid.BindValueChanged(e =>
                DrawableCount.FadeTo(e.NewValue ? 1 : alpha_when_invalid, 1000, Easing.OutQuint));
            base.LoadComplete();
        }

        protected override IHasText CreateText() => new DefaultTextWithUnitComponent(@"UR")
        {
            Alpha = alpha_when_invalid,
        };
    }
}
