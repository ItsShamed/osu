// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Menu
{
    public partial class MenuKiaiFountainOverlay : KiaiFountainOverlay
    {
        protected override void LoadFountains(SkinManager skinManager)
        {
            Texture? texture = skinManager.CurrentSkin.Value.GetTexture(@"star2")
                               ?? skinManager.DefaultClassicSkin.GetTexture(@"star2");

            Children = new[]
            {
                LeftFountain = new Fountain(texture)
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f, 1),
                    BasePosition = new Vector2(0.25f, 1.1f),
                    BaseAngle = 180,
                },
                RightFountain = new Fountain(texture)
                {
                    Origin = Anchor.BottomRight,
                    Anchor = Anchor.BottomRight,
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f, 1),
                    BasePosition = new Vector2(0.75f, 1.1f),
                    BaseAngle = 180,
                }
            };
        }

        protected override void Shoot()
        {
            FountainAnimation fountainAnimation = (FountainAnimation)RNG.Next(0, 3);
            LeftFountain.Shoot(fountainAnimation, 1000f);
            RightFountain.Shoot(fountainAnimation.Invert(), 1000f);
        }
    }
}
