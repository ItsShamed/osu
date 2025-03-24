// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.Sprites;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play
{
    public partial class DefaultAccomplishmentSequence : AccomplishmentSequence
    {
        private const int triangles_seed = 170525;

        private readonly OsuSpriteText text;
        private readonly TrianglePieceV2 triangles;
        private readonly Circle glow;

        public DefaultAccomplishmentSequence()
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(1, 0.2f);
            InternalChildren = new Drawable[]
            {
                glow = new Circle
                {
                    Blending = BlendingParameters.Additive,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 0,
                    Size = new Vector2(300, 10),
                    Position = new Vector2(0, 5)
                },
                triangles = new TrianglePieceV2(triangles_seed)
                {
                    Alpha = 0f,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.7f, 1),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    SpawnRatio = 1.5f,
                    Velocity = 3f,
                    Position = new Vector2(-30, 10)
                },
                text = new OsuSpriteText
                {
                    Font = OsuFont.GetFont(size: 70f, weight: FontWeight.SemiBold),
                    Alpha = 0,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(-5f),
                },
            };
        }

        protected override void StartSequence(Accomplishment accomplishment)
        {
            setColours(accomplishment);

            text.Text = accomplishment.GetDescription();

            text.FadeIn().FlashColour(Color4.White, 500).TransformSpacingTo(new Vector2(1f), 1000, Easing.OutQuart).ScaleTo(new Vector2(1.2f), 1000);
            glow.FadeIn().Then().FadeOut(500);

            triangles.Reset();

            triangles.FadeTo(0.7f).TransformTo(nameof(triangles.Velocity), 0.5f, 500);

            using (BeginDelayedSequence(500))
            {
                this.FadeOutFromOne(500, Easing.OutQuad).Then().Expire();
            }
        }

        private void setColours(Accomplishment accomplishment)
        {
            var triangleColour = Color4.White;

            switch (accomplishment)
            {
                case Accomplishment.Perfect:
                    triangleColour = OsuColour.ForRank(ScoreRank.X);
                    text.Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex(@"ffe7a8"), Color4Extensions.FromHex(@"ffb800"));
                    break;

                case Accomplishment.FullCombo:
                    triangleColour = OsuColour.ForRank(ScoreRank.S);
                    text.Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex(@"ffe7a8"), Color4Extensions.FromHex(@"ffb800"));
                    break;

                case Accomplishment.NoBreak:
                    triangleColour = OsuColour.ForRank(ScoreRank.A);
                    text.Colour = ColourInfo.GradientVertical(Color4.White, Color4Extensions.FromHex("afdff0"));
                    break;
            }

            glow.Colour = triangleColour;
            glow.Child.Alpha = 0;
            glow.Child.AlwaysPresent = true;
            glow.EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Glow,
                Colour = triangleColour.Opacity(0.2f),
                Radius = 400f,
            };
            triangles.Colour = ColourInfo.GradientVertical(triangleColour.Lighten(0.5f), triangleColour.Darken(0.05f));
        }

        private partial class TrianglePieceV2 : TrianglesV2
        {
            protected override bool CreateNewTriangles => true;

            public TrianglePieceV2(int? seed = null)
                : base(seed)
            {
                ClampAxes = Axes.None;
            }
        }
    }
}
