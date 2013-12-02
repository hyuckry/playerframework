﻿// <copyright file="CaptionSettingsPluginBase.WP8.cs" company="Microsoft Corporation">
// Copyright (c) 2013 Microsoft Corporation All Rights Reserved
// </copyright>
// <author>Michael S. Scherotter</author>
// <email>mischero@microsoft.com</email>
// <date>2013-11-14</date>
// <summary>CaptionSettingsPluginBase partial class for Windows Phone 8</summary>

namespace Microsoft.PlayerFramework.CaptionSettings
{
    using System;
    using System.Globalization;
    using System.IO.IsolatedStorage;
    using System.Windows.Media;
    using System.Windows.Navigation;
    using Microsoft.PlayerFramework.CaptionSettings.Model;

    /// <summary>
    /// Windows Phone Caption Settings UI
    /// </summary>
    public partial class CaptionSettingsPluginBase
    {
        #region Fields
        /// <summary>
        /// the isolated storage settings key for the caption settings
        /// </summary>
        private const string LocalSettingsKey = "Microsoft.PlayerFramework.CaptionSettings";

        /// <summary>
        /// the caption settings plug-in
        /// </summary>
        private CaptionSettingsPluginBase plugin;

        /// <summary>
        /// the save caption settings event handler
        /// </summary>
        private EventHandler<CustomCaptionSettingsEventArgs> saveCaptionSettings;

        /// <summary>
        /// the load caption settings event handler
        /// </summary>
        private EventHandler<CustomCaptionSettingsEventArgs> loadCaptionSettings;
        #endregion

        #region Methods

        /// <summary>
        /// Show the settings page if there is a CaptionsPlugin.
        /// </summary>
        /// <param name="service">the navigation service</param>
        /// <example>
        /// This is the handler for an app bar menu button click event:
        /// <code>
        /// private void CaptionSettings_Click(object sender, System.EventArgs e)
        /// {
        ///     this.captionSettingsPlugin.ShowSettingsPage(this.NavigationService);
        /// }
        /// </code>
        /// </example>
        public void ShowSettingsPage(NavigationService service)
        {
            if (this.plugin == null)
            {
                System.Diagnostics.Debug.WriteLine("No CaptionsPlugin is in the MediaPlayer's Plugin collection.");

                return;
            }

            bool isEnabled = false;

            object value;

            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(CaptionSettingsPage.OverrideDefaultKey, out value))
            {
                isEnabled = (bool)value;
            }

            ////var control = new CaptionSettingsControl();

            ////control.Width = layoutRoot.ActualWidth;
            ////control.Height = layoutRoot.ActualHeight;

            ////control.Settings = this.plugin.Settings;

            ////control.ApplyCaptionSettings = this.plugin.ApplyCaptionSettings;

            ////var popup = new Popup();
            ////popup.Child = control;

            ////layoutRoot.Children.Add(popup);

            ////popup.IsOpen = true;

            var assembly = typeof(CaptionSettingsPage).Assembly;

            var assemblyName = System.IO.Path.GetFileNameWithoutExtension(assembly.ManifestModule.Name);

            var uriString = string.Format(
                CultureInfo.InvariantCulture,
                "/{0};component/CaptionSettingsPage.xaml?IsEnabled={1}",
                assemblyName,
                isEnabled);

            var source = new Uri(uriString, UriKind.Relative);

            if (service.Navigate(source))
            {
                CaptionSettingsPage.Settings = this.plugin.Settings;
                CaptionSettingsPage.ApplyCaptionSettings = this.plugin.ApplyCaptionSettings;
            }
        }

        /// <summary>
        /// Activate the caption settings UI
        /// </summary>
        /// <param name="plugin">the plug-in</param>
        /// <param name="loadCaptionSettings">the load caption settings event handler</param>
        /// <param name="saveCaptionSettings">the save caption settings event handler</param>
        internal void Activate(
            CaptionSettingsPluginBase plugin,
            EventHandler<CustomCaptionSettingsEventArgs> loadCaptionSettings,
            EventHandler<CustomCaptionSettingsEventArgs> saveCaptionSettings)
        {
            this.plugin = plugin;
            this.loadCaptionSettings = loadCaptionSettings;
            this.saveCaptionSettings = saveCaptionSettings;

            bool isCustomCaptionSettings = false;

            object value;

            if (IsolatedStorageSettings.ApplicationSettings.TryGetValue(CaptionSettingsPage.OverrideDefaultKey, out value))
            {
                isCustomCaptionSettings = (bool)value;
            }

            if (isCustomCaptionSettings && IsolatedStorageSettings.ApplicationSettings.TryGetValue(LocalSettingsKey, out value))
            {
                var xml = value.ToString();

                this.plugin.Settings = CustomCaptionSettings.FromString(xml);
            }
            else
            {
                this.plugin.Settings = new CustomCaptionSettings
                {
                    FontColor = Colors.White.ToCaptionSettingsColor()
                };
            }
        }

        /// <summary>
        /// Deactivate the Windows Phone UI
        /// </summary>
        internal void Deactivate()
        {
            this.plugin = null;
            this.saveCaptionSettings = null;
            this.loadCaptionSettings = null;
        }

        /// <summary>
        /// Save to Isolated storage
        /// </summary>
        /// <param name="captionSettings">the caption settings</param>
        internal void Save(CustomCaptionSettings captionSettings)
        {
            IsolatedStorageSettings.ApplicationSettings[LocalSettingsKey] = captionSettings.ToXmlString();
        }
        #endregion
    }
}