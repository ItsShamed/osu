// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ModelManagerStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ModelManager";

        /// <summary>
        /// "No {0}s found to delete!"
        /// </summary>
        /// <remarks>
        /// {0} is a singular noun like "beatmap" or "skin".
        /// </remarks>
        public static LocalisableString NoModelToDelete(string modelName) => new TranslatableString(getKey(@"no_model_to_delete"), @"No {0}s found to delete!", modelName);

        /// <summary>
        /// "Preparing to delete all {0}s..."
        /// </summary>
        /// <remarks>
        /// {0} is a singular noun like "beatmap" or "skin".
        /// </remarks>
        public static LocalisableString PreparingToDelete(string modelName) => new TranslatableString(getKey(@"preparing_to_delete"), @"Preparing to delete all {0}s...", modelName);

        /// <summary>
        /// "Deleted all {0}s!"
        /// </summary>
        /// <remarks>
        /// {0} is a singular noun like "beatmap" or "skin".
        /// </remarks>
        public static LocalisableString AllModelsDeleted(string modelName) => new TranslatableString(getKey(@"all_models_deleted"), @"Deleted all {0}s!", modelName);

        /// <summary>
        /// "Deleting {0}s ({1} of {2})"
        /// </summary>
        /// <remarks>
        /// {0} is a singular noun like "beatmap" or "skin".
        /// </remarks>
        public static LocalisableString DeletionProgress(string modelName, int current, int count) => new TranslatableString(getKey(@"deletion_progress"), @"Deleting {0}s ({1} of {2})", modelName, current, count);

        /// <summary>
        /// "Restored all deleted items!"
        /// </summary>
        public static LocalisableString RestoredAllDeletedItems => new TranslatableString(getKey(@"restored_all_deleted_items"), @"Restored all deleted items!");

        /// <summary>
        /// "Restoring ({0} of {1})"
        /// </summary>
        public static LocalisableString RestorationProgress(int current, int count) => new TranslatableString(getKey(@"restoration_progress"), @"Restoring ({0} of {1})", current, count);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
