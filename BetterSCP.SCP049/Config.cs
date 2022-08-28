﻿// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel;
using Mistaken.Updater.Config;

namespace Mistaken.BetterSCP.SCP049
{
    internal class Config : IAutoUpdatableConfig
    {
        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether debug should be displayed.
        /// </summary>
        [Description("If true then debug will be displayed")]
        public bool VerbouseOutput { get; set; }

        [Description("If true then .disarm command will be enabled and recontamination of SCP-049 will be possible")]
        public bool Allow049Recontainment { get; set; } = false;

        [Description("Sets the amount damage after which SCP-049 will be automatically uncuffed")]
        public float Scp049UncuffDamage { get; set; } = 100f;

        /// <inheritdoc/>
        [Description("Auto Update Settings")]
        public System.Collections.Generic.Dictionary<string, string> AutoUpdateConfig { get; set; }
    }
}
