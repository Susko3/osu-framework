// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace osu.Framework.Localisation
{
    public partial class LocalisationManager : IDisposable
    {
        public IBindable<LocalisationParameters> CurrentParameters => currentParameters;

        private readonly Bindable<LocalisationParameters> currentParameters = new Bindable<LocalisationParameters>(LocalisationParameters.Default);

        private readonly Dictionary<string, LocaleMapping> locales = new Dictionary<string, LocaleMapping>();

        /// <summary>
        /// The first locale added to <see cref="locales"/>.
        /// Used as the fallback locale if there are no matches.
        /// </summary>
        private LocaleMapping? firstLocale;

        private LocaleMapping? systemDefaultLocaleMapping;

        /// <summary>
        /// The <see cref="LocaleMapping"/> that most closely matches <see cref="GameHost.UserUICulturePriority"/>, or null iff <see cref="locales"/> is empty.
        /// </summary>
        /// <remarks>This property is cached.</remarks>
        public LocaleMapping? SystemDefaultLocaleMapping => systemDefaultLocaleMapping ??= getSystemDefaultLocaleMapping();

        private readonly GameHost host;

        private readonly Bindable<string> configLocale = new Bindable<string>();
        private readonly Bindable<bool> configPreferUnicode = new BindableBool();

        public LocalisationManager(GameHost host, FrameworkConfigManager config)
        {
            this.host = host;
            setupConfig(config);
        }

        /// <summary>
        /// Add multiple locale mappings. Should be used to add all available languages at initialisation.
        /// </summary>
        /// <param name="mappings">All available locale mappings.</param>
        public void AddLocaleMappings(IEnumerable<LocaleMapping> mappings)
        {
            foreach (var mapping in mappings)
            {
                locales.Add(mapping.Name, mapping);
                firstLocale ??= mapping;
            }

            systemDefaultLocaleMapping = null; // invalidate stored default as there could be a better match.

            configLocale.TriggerChange();
        }

        /// <summary>
        /// Add a single language to this manager.
        /// </summary>
        /// <remarks>
        /// Use <see cref="AddLocaleMappings"/> as a more efficient way of bootstrapping all available locales.</remarks>
        /// <param name="language">The culture name to be added. Generally should match <see cref="CultureInfo.Name"/>.</param>
        /// <param name="storage">A storage providing localisations for the specified language.</param>
        public void AddLanguage(string language, ILocalisationStore storage)
        {
            var mapping = new LocaleMapping(language, storage);

            locales.Add(language, mapping);
            firstLocale ??= mapping;

            systemDefaultLocaleMapping = null; // invalidate stored default as there could be a better match.

            configLocale.TriggerChange();
        }

        /// <summary>
        /// Returns the appropriate <see cref="string"/> value for a <see cref="LocalisableString"/> given the currently valid <see cref="LocalisationParameters"/>.
        /// </summary>
        /// <remarks>
        /// The returned value is only valid until the next change to <see cref="CurrentParameters"/>.
        /// To facilitate tracking changes to the localised value across <see cref="CurrentParameters"/> changes, use <see cref="GetLocalisedBindableString"/>
        /// and subscribe to its <see cref="Bindable{T}.ValueChanged"/> instead.
        /// </remarks>
        internal string GetLocalisedString(LocalisableString text)
        {
            switch (text.Data)
            {
                case string plain:
                    return plain;

                case ILocalisableStringData data:
                    return data.GetLocalised(currentParameters.Value);

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Creates an <see cref="ILocalisedBindableString"/> which automatically updates its text according to information provided in <see cref="ILocalisedBindableString.Text"/>.
        /// </summary>
        /// <returns>The <see cref="ILocalisedBindableString"/>.</returns>
        public ILocalisedBindableString GetLocalisedBindableString(LocalisableString original) => new LocalisedBindableString(original, this);

        private void setupConfig(FrameworkConfigManager config)
        {
            config.BindWith(FrameworkSetting.Locale, configLocale);
            configLocale.BindValueChanged(updateLocale);

            config.BindWith(FrameworkSetting.ShowUnicode, configPreferUnicode);
            configPreferUnicode.BindValueChanged(preferUnicode =>
            {
                // optimized path, only PreferOriginalScript changed.
                currentParameters.Value = currentParameters.Value.With(preferOriginalScript: preferUnicode.NewValue);
            }, true);
        }

        /// <summary>
        /// Bound to <see cref="FrameworkSetting.Locale"/>.
        /// </summary>
        private void updateLocale(ValueChangedEvent<string> locale)
        {
            if (locales.Count == 0)
                return;

            if (!updateParameters())
            {
                if (locale.OldValue == locale.NewValue)
                    // equal values mean invalid locale on startup, no real way to recover other than to set to default.
                    configLocale.SetDefault();
                else
                    // revert to the old locale if the new one is invalid.
                    configLocale.Value = locale.OldValue;
            }

            bool updateParameters()
            {
                LocaleMapping? localeMapping;

                if (string.IsNullOrEmpty(locale.NewValue))
                {
                    localeMapping = SystemDefaultLocaleMapping;
                }
                else
                {
                    if (!locales.TryGetValue(locale.NewValue, out localeMapping))
                        return false;
                }

                Debug.Assert(localeMapping != null);

                var culture = (CultureInfo)getSpecificCultureFor(localeMapping.Storage.UICulture).Clone(); // clone to make the culture writeable.
                CustomiseCulture(culture);

                setParameters(new LocalisationParameters(localeMapping.Storage, configPreferUnicode.Value, culture, localeMapping.Storage.UICulture));
                return true;
            }
        }

        /// <summary>
        /// Sets <see cref="CurrentParameters"/> and updates <see cref="GameHost"/> culture with the provided <see cref="LocalisationParameters"/>.
        /// </summary>
        private void setParameters(LocalisationParameters parameters)
        {
            host.SetCulture(parameters.Culture, parameters.UICulture);
            currentParameters.Value = parameters;
        }

        /// <summary>
        /// Applies customization to the <see cref="CultureInfo"/> used for culture-specific string formatting.
        /// </summary>
        /// <param name="culture">Writable <see cref="CultureInfo"/> to be used for <see cref="LocalisationParameters.Culture"/>.</param>
        protected virtual void CustomiseCulture(CultureInfo culture)
        {
        }

        #region CultureInfo/LocaleMapping helpers

        /// <summary>
        /// Gets the <see cref="LocaleMapping"/> from <see cref="locales"/> that most closely matches <see cref="GameHost.UserUICulturePriority"/>,
        /// or <see cref="firstLocale"/> if none match.
        /// </summary>
        /// <returns>A <see cref="LocaleMapping"/> from <see cref="locales"/>, or <c>null</c> iff <see cref="locales"/> is empty.</returns>
        private LocaleMapping? getSystemDefaultLocaleMapping()
        {
            foreach (var uiCulture in host.UserUICulturePriority)
            {
                // also look for any parent (country-neutral) cultures that match. eg. `en-GB` will match `en` LocaleMapping.
                foreach (var c in uiCulture.EnumerateParentCultures())
                {
                    if (locales.TryGetValue(c.Name, out var localeMapping))
                        return localeMapping;
                }
            }

            return firstLocale;
        }

        /// <summary>
        /// Gets the most specific <see cref="CultureInfo"/> matching the provided <paramref name="storeUICulture"/>.
        /// </summary>
        /// <param name="storeUICulture"><see cref="ILocalisationStore.UICulture"/> of the current <see cref="ILocalisationStore"/>.</param>
        /// <returns>The appropriate non-UI culture based on the provided <paramref name="storeUICulture"/>.</returns>
        private CultureInfo getSpecificCultureFor(CultureInfo storeUICulture)
        {
            if (!storeUICulture.IsNeutralCulture)
                return storeUICulture;

            // try to find a more specific culture from user preference. eg. `en` LocaleMapping will match `en-GB` culture.
            foreach (var culture in host.UserCulturePriority)
            {
                if (culture.EnumerateParentCultures().Any(c => c.Name == storeUICulture.Name))
                    return culture;
            }

            return storeUICulture;
        }

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            currentParameters.UnbindAll();
            configLocale.UnbindAll();
            configPreferUnicode.UnbindAll();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
