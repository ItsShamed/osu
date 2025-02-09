// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ExternalEditScreenStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ExternalEditScreen";

        /// <summary>
        /// "Exporting for edit..."
        /// </summary>
        public static LocalisableString ExportingForEdit => new TranslatableString(getKey(@"exporting_for_edit"), @"Exporting for edit...");

        /// <summary>
        /// "Export failed!"
        /// </summary>
        public static LocalisableString ExportFailed => new TranslatableString(getKey(@"export_failed"), @"Export failed!");

        /// <summary>
        /// "Beatmap is mounted externally"
        /// </summary>
        public static LocalisableString BeatmapIsMountedExternally => new TranslatableString(getKey(@"beatmap_is_mounted_externally"), @"Beatmap is mounted externally");

        /// <summary>
        /// "Any changes made to the exported folder will be imported to the game, including file additions, modifications and deletions."
        /// </summary>
        public static LocalisableString ChangesAreImported => new TranslatableString(getKey(@"changes_are_imported"), @"Any changes made to the exported folder will be imported to the game, including file additions, modifications and deletions.");

        /// <summary>
        /// "Finish editing and import changes"
        /// </summary>
        public static LocalisableString FinishAndImport => new TranslatableString(getKey(@"finish_and_import"), @"Finish editing and import changes");

        /// <summary>
        /// "Cleaning up..."
        /// </summary>
        public static LocalisableString CleaningUp => new TranslatableString(getKey(@"cleaning_up"), @"Cleaning up...");

        /// <summary>
        /// "Import failed!"
        /// </summary>
        public static LocalisableString ImportFailed => new TranslatableString(getKey(@"import_failed"), @"Import failed!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
