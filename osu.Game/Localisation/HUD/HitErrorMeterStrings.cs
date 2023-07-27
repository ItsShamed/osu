// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class HitErrorMeterStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HitErrorMeter";

        /// <summary>
        /// "Auto-hide"
        /// </summary>
        public static LocalisableString AutoHide => new TranslatableString(getKey(@"auto_hide"), @"Auto-hide");

        /// <summary>
        /// "Automatically hides the meter when gameplay is idle."
        /// </summary>
        public static LocalisableString AutoHideDescription => new TranslatableString(getKey(@"auto_hide_description"), @"Automatically hides the meter when gameplay is idle.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
