// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ConfirmDiscardChangesDialogStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ConfirmDiscardChangesDialog";

        /// <summary>
        /// "Are you sure you want to go back?"
        /// </summary>
        public static LocalisableString HeaderTitle => new TranslatableString(getKey(@"are_you_sure_you_want"), @"Are you sure you want to go back?");

        /// <summary>
        /// "This will discard any unsaved changes"
        /// </summary>
        public static LocalisableString BodyText => new TranslatableString(getKey(@"this_will_discard_any_unsaved"), @"This will discard any unsaved changes");

        /// <summary>
        /// "No I didn&#39;t mean to"
        /// </summary>
        public static LocalisableString Cancel => new TranslatableString(getKey(@"no_ididnt_mean_to"), @"No I didn't mean to");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
