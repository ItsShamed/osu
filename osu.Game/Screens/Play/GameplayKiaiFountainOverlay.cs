// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play
{
    public partial class GameplayKiaiFountainOverlay : KiaiFountainOverlay
    {
        protected override void LoadFountains(SkinManager skinManager)
        {
            Texture? texture = skinManager.CurrentSkin.Value.GetTexture(@"star2");

            Children = new[]
            {
                LeftFountain = new Fountain(texture)
                {
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f, 1),
                    BasePosition = new Vector2(0, 1f),
                    BaseAngle = 140,
                },
                RightFountain = new Fountain(texture)
                {
                    Origin = Anchor.BottomRight,
                    Anchor = Anchor.BottomRight,
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f, 1),
                    BasePosition = new Vector2(1),
                    BaseAngle = 220,
                }
            };

            skinManager.CurrentSkin.BindValueChanged(e =>
            {
                LeftFountain.Texture = e.NewValue.GetTexture(@"star2");
                RightFountain.Texture = e.NewValue.GetTexture(@"star2");
            });
        }

        protected override void Shoot()
        {
            LeftFountain.Shoot(FountainAnimation.Idle, 1000f);
            RightFountain.Shoot(FountainAnimation.Idle, 1000f);
        }
    }
}
