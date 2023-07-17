// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK;
using Fountain = osu.Game.Graphics.Fountain;

namespace osu.Game.Tests.Visual.Menus
{
    public partial class TestSceneFountain : OsuTestScene
    {
        private Fountain fountain = null!;

        [BackgroundDependencyLoader]
        private void load(SkinManager skinManager)
        {
            Children = new Drawable[]
            {
                fountain = new Fountain(skinManager.DefaultClassicSkin.GetTexture(@"star2")!)
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1),
                    BasePosition = new Vector2(0.5f, 1.1f)
                },
            };

            AddSliderStep("Base Angle", 0f, 360f, 180f, value => fountain.BaseAngle = value);
            AddSliderStep("Spread Angle", 0f, 90f, 30f, value => fountain.SweepAngleRange = value);
            AddSliderStep("Angle Variation", 0f, 90f, 10f, value => fountain.SpreadAngle = value);
            AddSliderStep("Reach Radius", 0f, 3f, 1.5f, value => fountain.ReachRadius = value);
            AddSliderStep("Gravity", 0f, 2f, 0.75f, value => fountain.ReachRadius = value);
            AddSliderStep("Base X", 0f, 1f, 0.5f, value => fountain.BasePosition.X = value);
            AddSliderStep("Base Y", 0f, 1f, 0.5f, value => fountain.BasePosition.Y = value);

            AddStep("Clockwise turn", () => fountain.Shoot(FountainAnimation.ClockwiseTurn, 1000f));
            AddStep("Counter-clockwise turn", () => fountain.Shoot(FountainAnimation.CounterClockwiseTurn, 1000f));
            AddStep("Straight up", () => fountain.Shoot(FountainAnimation.Idle, 1000f));
            AddStep("Stop", fountain.Stop);
        }
    }
}
