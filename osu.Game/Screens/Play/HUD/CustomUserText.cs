// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class CustomUserText : Container, ISkinnableDrawable
    {
        [SettingSource(nameof(Text))]
        public Bindable<string> Text { get; set; } = new Bindable<string>("Sample text");

        [Resolved]
        private GameplayState gameplayState { get; set; } = null!;

        private SpriteText displayText = null!;

        private Regex regex;

        private string title = "";
        private string artist = "";
        private string diff = "";
        private string player = "";
        private string mapper = "";

        public CustomUserText()
        {
            regex = new Regex(@"\{[a-zA-Z]*\}", RegexOptions.Compiled | RegexOptions.Multiline);

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            displayText = new OsuSpriteText
            {
                Font = OsuFont.Torus.With(size: 40f),
            };
            updateDisplay();
            Child = displayText;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            title = gameplayState.Beatmap.Metadata.Title;
            artist = gameplayState.Beatmap.Metadata.Artist;
            diff = gameplayState.Beatmap.BeatmapInfo.DifficultyName;
            player = gameplayState.Score.ScoreInfo.User.Username;
            mapper = gameplayState.Beatmap.Metadata.Author.Username;
        }

        protected override void Update()
        {
            base.Update();

            updateDisplay();
        }

        private void updateDisplay()
        {
            string currentText = Text.Value;

            Type thisType = typeof(CustomUserText);
            var fieldInfos = thisType
                             .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                             .Where(f => f.FieldType == typeof(string));

            foreach (Match match in regex.Matches(currentText))
            {
                string fieldName = match.Value.Replace("{", "").Replace("}", "");

                if (fieldInfos.Any(f => f.Name == fieldName))
                {
                    currentText = currentText
                        .Replace(match.Value, thisType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)?
                            .GetValue(this) as string);
                }
            }

            displayText.Text = currentText;
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
