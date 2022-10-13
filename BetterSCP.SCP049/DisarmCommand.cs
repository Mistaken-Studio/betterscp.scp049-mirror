// -----------------------------------------------------------------------
// <copyright file="DisarmCommand.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CommandSystem;
using Exiled.API.Features;
using MEC;
using Mistaken.API;
using Mistaken.API.Commands;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using UnityEngine;

namespace Mistaken.BetterSCP.SCP049.Commands
{
    /// <inheritdoc/>
    [CommandHandler(typeof(ClientCommandHandler))]
    public class DisarmCommand : IBetterCommand
    {
        /// <summary>
        /// Event that's fired when SCP-049 is getting cuffed;
        /// </summary>
        public static event EventHandler<(Player Cuffer, Player Scp049)> Cuffed049;

        /// <summary>
        /// Gets dictionary of disarmed SCP-049s.
        /// </summary>
        public static Dictionary<Player, Player> DisarmedScps { get; } = new();

        /// <inheritdoc/>
        public override string Command => "disarm049";

        /// <inheritdoc/>
        public override string[] Aliases => new string[] { "disarm" };

        /// <inheritdoc/>
        public override string Description => "Disarm SCP 049";

        /// <inheritdoc/>
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (!PluginHandler.Instance.Config.Allow049Recontainment)
                return new string[] { PluginHandler.Instance.Translation.DisabledCommand };

            var player = Player.Get(sender);
            if (player.Role.Side != Exiled.API.Enums.Side.Mtf && player.Role.Team != Team.CHI)
                return new string[] { PluginHandler.Instance.Translation.WrongSideCommandInfo };

            if (this.GetCuffingLimit(player) <= this.GetCuffedPlayers(player).Count() + (DisarmedScps.ContainsKey(player) ? 1 : 0))
                return new string[] { PluginHandler.Instance.Translation.ExceededCuffingLimit };

            var scps = RealPlayers.List.Where(p => p.Role.Type == RoleType.Scp049 && Vector3.Distance(p.Position, player.Position) <= 4).ToList();
            if (scps.Count == 0)
                return new string[] { PluginHandler.Instance.Translation.NoScpNearby };

            if (DisarmedScps.TryGetValue(player, out Player scp))
            {
                DisarmedScps.Remove(player);
                success = true;
                return new string[] { PluginHandler.Instance.Translation.UncuffedScpCommandInfo };
            }

            if (alreadyRunning)
                return new string[] { PluginHandler.Instance.Translation.AlreadyBeingDisarmed };

            alreadyRunning = true;
            foreach (var scp049 in scps)
                Module.RunSafeCoroutine(this.ExecuteDisarming(scp049, player), "Disarm.ExecuteDisarming");

            success = true;
            return new string[] { PluginHandler.Instance.Translation.InProgressCommandInfo };
        }

        private static bool alreadyRunning;

        private IEnumerator<float> ExecuteDisarming(Player scp049, Player disarmer)
        {
            scp049.SetGUI("disarm049", PseudoGUIPosition.MIDDLE, string.Format(PluginHandler.Instance.Translation.DisarmingMessage049, 3), 5);
            yield return Timing.WaitForSeconds(1);
            Vector3 pos = scp049.Position;

            for (int i = 3; i >= 0; i--)
            {
                if (!scp049.IsConnected())
                    break;

                if (pos != scp049.Position)
                {
                    scp049.SetGUI("disarm049", PseudoGUIPosition.MIDDLE, PluginHandler.Instance.Translation.DisarmingFailedMessage049, 5);
                    disarmer.SetGUI("disarm049", PseudoGUIPosition.MIDDLE, PluginHandler.Instance.Translation.DisarmingFailedMessageCuffer, 5);
                    alreadyRunning = false;
                    yield break;
                }

                scp049.SetGUI("disarm049", PseudoGUIPosition.MIDDLE, string.Format(PluginHandler.Instance.Translation.DisarmingMessage049, i));
                yield return Timing.WaitForSeconds(1f);
            }

            DisarmedScps.Add(disarmer, scp049);
            if (Cuffed049 != null)
                Cuffed049.Invoke(null, (disarmer, scp049));

            alreadyRunning = false;
            disarmer.SetGUI("disarm049", PseudoGUIPosition.MIDDLE, PluginHandler.Instance.Translation.DisarmingSuccessfull, 5);
            scp049.SetGUI("disarm049", PseudoGUIPosition.MIDDLE, null);
            Timing.RunCoroutine(this.UpdateGUI(scp049, disarmer));
        }

        private IEnumerator<float> UpdateGUI(Player player, Player cuffer)
        {
            while (DisarmedScps.ContainsValue(player))
            {
                if (player.IsConnected() && cuffer.IsConnected() && player.Role.Type == RoleType.Scp049)
                {
                    player.SetGUI("disarmed049gui", PseudoGUIPosition.MIDDLE, string.Format(PluginHandler.Instance.Translation.DisarmedInformation049, cuffer.GetDisplayName()));
                    if (Vector3.Distance(player.Position, cuffer.Position) >= 30)
                        DisarmedScps.Remove(cuffer);
                }
                else
                    DisarmedScps.Remove(cuffer);

                yield return Timing.WaitForSeconds(1);
            }

            player.SetGUI("disarmed049gui", PseudoGUIPosition.MIDDLE, null);
        }

        private IEnumerable<Player> GetCuffedPlayers(Player cuffer)
            => RealPlayers.List.Where(x => x.IsAlive && x.Cuffer == cuffer);

        private ushort GetCuffingLimit(Player cuffer)
        {
            ushort limit = 0;

            if (cuffer.HasItem(ItemType.ArmorLight))
                limit = 1;
            else if (cuffer.HasItem(ItemType.ArmorCombat))
                limit = 2;
            else if (cuffer.HasItem(ItemType.ArmorHeavy))
                limit = 4;
            return limit;
        }
    }
}
