// -----------------------------------------------------------------------
// <copyright file="ContainCommand.cs" company="Mistaken">
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
    public class ContainCommand : IBetterCommand
    {
        /// <inheritdoc/>
        public override string Command => "contain049";

        /// <inheritdoc/>
        public override string[] Aliases => new string[] { "contain" };

        /// <inheritdoc/>
        public override string Description => "Recontain SCP 049";

        /// <inheritdoc/>
        public override string[] Execute(ICommandSender sender, string[] args, out bool success)
        {
            success = false;
            if (!PluginHandler.Instance.Config.Allow049Recontainment)
                return new string[] { "This command is disabled on this server" };
            var player = sender.GetPlayer();
            if (player.Role.Side != Exiled.API.Enums.Side.Mtf && player.Role.Team != Team.CHI)
                return new string[] { "Only Foundation Personnel(MTF, Guards, Sciencists) can use this command" };
            if (!(player.Position.y > -800 && player.Position.y < -700))
                return new string[] { "You have to be in SCP-049 containment chamber" };
            var scps = RealPlayers.List.Where(p => p.Role == RoleType.Scp049 && (player.Position.y > -800 && player.Position.y < -700));
            if (scps.Count() == 0)
                return new string[] { "There is no SCP-049 in SCP-049 containment chamber" };
            if (alreadyRunning)
                return new string[] { "SCP-049 is already in recontainment process" };
            alreadyRunning = true;
            foreach (var scp049 in scps)
                Module.RunSafeCoroutine(this.ExecuteRecontainment(scp049, player), "Contain.ExecuteRecontainment");
            return new string[] { "In progress" };
        }

        private static bool alreadyRunning = false;

        private IEnumerator<float> ExecuteRecontainment(Player scp049, Player recontainer)
        {
            scp049.SetGUI("contain049", PseudoGUIPosition.MIDDLE, "<color=red><size=150%>You are being recontained</size></color><br>Stand still for <color=yellow>5</color>s", 5);
            yield return Timing.WaitForSeconds(1);
            Vector3 pos = scp049.Position;
            for (int i = 4; i >= 0; i--)
            {
                if (!scp049.IsConnected)
                    break;
                if (pos != scp049.Position)
                {
                    scp049.SetGUI("contain049", PseudoGUIPosition.MIDDLE, $"<color=red><size=150%>Recontainment canceled</size></color>", 5);
                    recontainer.SetGUI("contain049", PseudoGUIPosition.MIDDLE, $"<color=red><size=150%>Recontainment canceled</size></color><br>SCP-049 <color=yellow>moved</color>", 5);
                    alreadyRunning = false;
                    yield break;
                }

                scp049.SetGUI("contain049", PseudoGUIPosition.MIDDLE, $"<color=red><size=150%>You are being recontained</size></color><br>Stand still for <color=yellow>{i}</color>s");
                yield return Timing.WaitForSeconds(1);
            }

            alreadyRunning = false;
            scp049.SetRole(recontainer.Role.Team == Team.CHI ? RoleType.ChaosConscript : RoleType.NtfSpecialist, reason: Exiled.API.Enums.SpawnReason.Escaped, lite: true);
            scp049.SetGUI("contain049", PseudoGUIPosition.MIDDLE, $"<color=red><size=150%>Recontainment successfull</size></color>", 5);
            string recontainerName;
            if (recontainer.Role.Team == Team.CHI)
                recontainerName = "CHAOS INSURGENCY";
            else if (recontainer.Role == RoleType.Scientist)
                recontainerName = "SCIENCE PERSONNEL";
            else
            {
                string unit = recontainer.ReferenceHub.characterClassManager.CurUnitName;
                if (unit.StartsWith("<color="))
                    unit = unit.Split('>')[1].Split('<')[0].Trim();
                try
                {
                    string[] array = unit.Split('-');
                    recontainerName = "UNIT NATO_" + array[0][0].ToString() + " " + array[1];
                }
                catch
                {
                    global::ServerConsole.AddLog("Error, couldn't convert '" + unit + "' into a CASSIE-readable form.", ConsoleColor.Gray);
                    recontainerName = "UNKNOWN";
                }
            }

            Cassie.Message($"SCP 0 4 9 recontained successfully by {recontainerName}");
        }
    }
}
