// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Resources.Localisation.Web;
using CommonStrings = osu.Game.Localisation.CommonStrings;

namespace osu.Game.Skinning.Components
{
    [UsedImplicitly]
    public partial class CustomText : FontAdjustableSkinComponent
    {
        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Attribute), nameof(BeatmapAttributeTextStrings.AttributeDescription))]
        public Bindable<CustomTextAttribute> Attribute { get; } = new Bindable<CustomTextAttribute>(CustomTextAttribute.StarRating);

        [SettingSource(typeof(BeatmapAttributeTextStrings), nameof(BeatmapAttributeTextStrings.Template), nameof(BeatmapAttributeTextStrings.TemplateDescription))]
        public Bindable<string> Template { get; set; } = new Bindable<string>("{Label}: {Value}");

        private readonly Dictionary<CustomTextAttribute, LocalisableString> valueDictionary = new Dictionary<CustomTextAttribute, LocalisableString>
        {
            [CustomTextAttribute.CircleSize] = CommonStrings.NotAvailableAbbreviation,
            [CustomTextAttribute.CircleSize] = CommonStrings.NotAvailableAbbreviation,
            [CustomTextAttribute.Accuracy] = CommonStrings.NotAvailableAbbreviation,
            [CustomTextAttribute.HPDrain] = CommonStrings.NotAvailableAbbreviation,
            [CustomTextAttribute.ApproachRate] = CommonStrings.NotAvailableAbbreviation,
            [CustomTextAttribute.StarRating] = CommonStrings.NotAvailableAbbreviation,
            [CustomTextAttribute.Title] = CommonStrings.NotAvailableAbbreviation,
            [CustomTextAttribute.DifficultyName] = CommonStrings.NotAvailableAbbreviation,
            [CustomTextAttribute.Creator] = CommonStrings.NotAvailableAbbreviation,
            [CustomTextAttribute.Length] = CommonStrings.NotAvailableAbbreviation,
            [CustomTextAttribute.RankedStatus] = CommonStrings.NotAvailableAbbreviation,
            [CustomTextAttribute.BPM] = CommonStrings.NotAvailableAbbreviation,
        };

        private static readonly ImmutableDictionary<CustomTextAttribute, LocalisableString> label_dictionary = new Dictionary<CustomTextAttribute, LocalisableString>
        {
            [CustomTextAttribute.CircleSize] = BeatmapsetsStrings.ShowStatsCs,
            [CustomTextAttribute.Accuracy] = BeatmapsetsStrings.ShowStatsAccuracy,
            [CustomTextAttribute.HPDrain] = BeatmapsetsStrings.ShowStatsDrain,
            [CustomTextAttribute.ApproachRate] = BeatmapsetsStrings.ShowStatsAr,
            [CustomTextAttribute.StarRating] = BeatmapsetsStrings.ShowStatsStars,
            [CustomTextAttribute.Title] = EditorSetupStrings.Title,
            [CustomTextAttribute.Artist] = EditorSetupStrings.Artist,
            [CustomTextAttribute.DifficultyName] = EditorSetupStrings.DifficultyHeader,
            [CustomTextAttribute.Creator] = EditorSetupStrings.Creator,
            [CustomTextAttribute.Length] = ArtistStrings.TracklistLength.ToTitle(),
            [CustomTextAttribute.RankedStatus] = BeatmapDiscussionsStrings.IndexFormBeatmapsetStatusDefault,
            [CustomTextAttribute.BPM] = BeatmapsetsStrings.ShowStatsBpm,
        }.ToImmutableDictionary();

        private readonly OsuSpriteText text;

        public CustomText()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(IBindable<WorkingBeatmap>? beatmap)
        {
            beatmap?.BindValueChanged(b =>
            {
                updateBeatmapContent(b.NewValue);
                updateLabel();
            }, true);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Attribute.BindValueChanged(_ => updateLabel());
            Template.BindValueChanged(_ => updateLabel());
        }

        private void updateBeatmapContent(WorkingBeatmap workingBeatmap)
        {
            valueDictionary[CustomTextAttribute.Title] = workingBeatmap.BeatmapInfo.Metadata.Title;
            valueDictionary[CustomTextAttribute.Artist] = workingBeatmap.BeatmapInfo.Metadata.Artist;
            valueDictionary[CustomTextAttribute.DifficultyName] = workingBeatmap.BeatmapInfo.DifficultyName;
            valueDictionary[CustomTextAttribute.Creator] = workingBeatmap.BeatmapInfo.Metadata.Author.Username;
            valueDictionary[CustomTextAttribute.Length] = TimeSpan.FromMilliseconds(workingBeatmap.BeatmapInfo.Length).ToFormattedDuration();
            valueDictionary[CustomTextAttribute.RankedStatus] = workingBeatmap.BeatmapInfo.Status.GetLocalisableDescription();
            valueDictionary[CustomTextAttribute.BPM] = workingBeatmap.BeatmapInfo.BPM.ToLocalisableString(@"F2");
            valueDictionary[CustomTextAttribute.CircleSize] = ((double)workingBeatmap.BeatmapInfo.Difficulty.CircleSize).ToLocalisableString(@"F2");
            valueDictionary[CustomTextAttribute.HPDrain] = ((double)workingBeatmap.BeatmapInfo.Difficulty.DrainRate).ToLocalisableString(@"F2");
            valueDictionary[CustomTextAttribute.Accuracy] = ((double)workingBeatmap.BeatmapInfo.Difficulty.OverallDifficulty).ToLocalisableString(@"F2");
            valueDictionary[CustomTextAttribute.ApproachRate] = ((double)workingBeatmap.BeatmapInfo.Difficulty.ApproachRate).ToLocalisableString(@"F2");
            valueDictionary[CustomTextAttribute.StarRating] = workingBeatmap.BeatmapInfo.StarRating.ToLocalisableString(@"F2");
        }

        private void updateLabel()
        {
            string numberedTemplate = Template.Value
                                              .Replace("{", "{{")
                                              .Replace("}", "}}")
                                              .Replace(@"{{Label}}", "{0}")
                                              .Replace(@"{{Value}}", $"{{{1 + (int)Attribute.Value}}}");

            object?[] args = valueDictionary.OrderBy(pair => pair.Key)
                                            .Select(pair => pair.Value)
                                            .Prepend(label_dictionary[Attribute.Value])
                                            .Cast<object?>()
                                            .ToArray();

            foreach (var type in Enum.GetValues<CustomTextAttribute>())
            {
                numberedTemplate = numberedTemplate.Replace($"{{{{{type}}}}}", $"{{{1 + (int)type}}}");
            }

            text.Text = LocalisableString.Format(numberedTemplate, args);
        }

        protected override void SetFont(FontUsage font) => text.Font = font.With(size: 40);
    }

    public enum CustomTextAttribute
    {
        CircleSize,
        HPDrain,
        Accuracy,
        ApproachRate,
        StarRating,
        Title,
        Artist,
        DifficultyName,
        Creator,
        Length,
        RankedStatus,
        BPM,
    }
}
