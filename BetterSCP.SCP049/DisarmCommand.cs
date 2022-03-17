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
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class DisarmCommand : IBetterCommand
    {
        /// <summary>
        /// Hashset of disarmed SCP 049s.
        /// </summary>
        public static readonly Dictionary<Player, Player> DisarmedScps = new Dictionary<Player, Player>();

        /// <summary>
        /// Gets or sets a SCP 049 cuffing action.
        /// </summary>
        public static Action<(Player, Player)> Cuffing049 { get; set; }

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
            if (!PluginHandler.Instance.Config.Allow049Disarming)
                return new string[] { "This command is disabled on this server" };
            var player = sender.GetPlayer();
            if (player.Side != Exiled.API.Enums.Side.Mtf && player.Team != Team.CHI)
                return new string[] { "Only Foundation Personnel(MTF, Guards, Sciencists) can use this command" };
            if (this.GetCuffingLimit(player) <= this.GetCuffedPlayers(player).Count() + (DisarmedScps.ContainsKey(player) ? 1 : 0))
                return new string[] { "You have reached your cuffing limit" };
            var scps = RealPlayers.List.Where(p => p.Role == RoleType.Scp049 && Vector3.Distance(p.Position, player.Position) <= 4).ToList();
            if (scps.Count == 0)
                return new string[] { "There is no SCP-049 nearby" };
            if (DisarmedScps.TryGetValue(player, out Player scp))
            {
                DisarmedScps.Remove(player);
                success = true;
                return new string[] { "Uncuffed nearby SCP 049" };
            }

            if (alreadyRunning)
                return new string[] { "SCP-049 is already in disarming process" };
            alreadyRunning = true;
            foreach (var scp049 in scps)
                Module.RunSafeCoroutine(this.ExecuteDisarming(scp049, player), "Disarm.ExecuteDisarming");
            success = true;
            return new string[] { "In progress" };
        }

        private static bool alreadyRunning;

        private IEnumerator<float> ExecuteDisarming(Player scp049, Player disarmer)
        {
            scp049.SetGUI("disarm049", PseudoGUIPosition.MIDDLE, "<color=red><size=150%>You are being disarmed</size></color><br>Stand still for <color=yellow>3</color>s", 5);
            yield return Timing.WaitForSeconds(1);
            Vector3 pos = scp049.Position;
            for (int i = 4; i >= 0; i--)
            {
                if (!scp049.IsConnected)
                    break;
                if (pos != scp049.Position)
                {
                    scp049.SetGUI("disarm049", PseudoGUIPosition.MIDDLE, $"<color=red><size=150%>Disarming canceled</size></color>", 5);
                    disarmer.SetGUI("disarm049", PseudoGUIPosition.MIDDLE, $"<color=red><size=150%>Disarming canceled</size></color><br>SCP-049 <color=yellow>moved</color>", 5);
                    alreadyRunning = false;
                    yield break;
                }

                scp049.SetGUI("disarm049", PseudoGUIPosition.MIDDLE, $"<color=red><size=150%>You are being disarmed</size></color><br>Stand still for <color=yellow>{i}</color>s");
                yield return Timing.WaitForSeconds(0.5f);
            }

            DisarmedScps.Add(disarmer, scp049);
            Cuffing049.Invoke((disarmer, scp049));
            alreadyRunning = false;
            scp049.SetGUI("disarm049", PseudoGUIPosition.MIDDLE, $"<color=red><size=150%>Disarming successfull</size></color>", 5);
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
