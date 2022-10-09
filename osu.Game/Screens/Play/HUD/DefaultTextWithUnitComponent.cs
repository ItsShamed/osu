// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public class DefaultTextWithUnitComponent : CompositeDrawable, IHasText
    {
        public LocalisableString Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        public LocalisableString Unit
        {
            get => unitText.Text;
            set => unitText.Text = value;
        }

        private readonly OsuSpriteText text;
        private readonly OsuSpriteText unitText;

        public DefaultTextWithUnitComponent(LocalisableString unit)
        {
            AutoSizeAxes = Axes.Both;

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
                    unitText = new OsuSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Text = unit,
                        Font = OsuFont.Numeric.With(size: 8),
                        Padding = new MarginPadding { Bottom = 1.5f }, // align baseline better
                    }
                }
            };
        }
    }
}
