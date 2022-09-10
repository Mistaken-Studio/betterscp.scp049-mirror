// -----------------------------------------------------------------------
// <copyright file="SCP049Handler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using Mistaken.API.Components;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using UnityEngine;

namespace Mistaken.BetterSCP.SCP049
{
    internal class SCP049Handler : Module
    {
        public SCP049Handler(PluginHandler p)
            : base(p)
        {
            // plugin.RegisterTranslation("scp049_start_message", "<color=red><b><size=500%>UWAGA</size></b></color><br><br><br><br><br><br><size=90%>Rozgrywka jako <color=red>SCP 049</color> na tym serwerze jest zmodyfikowana, <color=red>SCP 049</color> posiada domyślnie dodatkowe <color=yellow>60</color> ahp, każdy <color=red>SCP 049-2</color> w zasięgu <color=yellow>10</color> metrów dodaje +<color=yellow>100</color> do max ahp, ahp regeneruje się z prędkością <color=yellow>20</color> na sekundę pod warunkiem że jest <color=yellow>bezpieczny</color>(w ciągu ostatnich <color=yellow>10</color> sekund nie otrzymał obrażeń)</size>");
        }

        public override string Name => nameof(SCP049Handler);

        public override void OnEnable()
        {
            Exiled.Events.Handlers.Player.ChangingRole += this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Died += this.Player_Died;
            Exiled.Events.Handlers.Player.Dying += this.Player_Dying;
            Exiled.Events.Handlers.Player.Hurting += this.Player_Hurting;
            Exiled.Events.Handlers.Player.InteractingDoor += this.Player_InteractingDoor;
            Exiled.Events.Handlers.Player.InteractingElevator += this.Player_InteractingElevator;
            Exiled.Events.Handlers.Scp049.StartingRecall += this.Scp049_StartingRecall;
            Exiled.Events.Handlers.Server.RestartingRound += this.Server_RestartingRound;
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;

            BetterSCP.SCPGUIHandler.SCPMessages[RoleType.Scp049] = PluginHandler.Instance.Translation.StartMessage;
        }

        public override void OnDisable()
        {
            BetterSCP.SCPGUIHandler.SCPMessages.Remove(RoleType.Scp049);

            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
            Exiled.Events.Handlers.Player.Hurting -= this.Player_Hurting;
            Exiled.Events.Handlers.Player.InteractingDoor -= this.Player_InteractingDoor;
            Exiled.Events.Handlers.Player.InteractingElevator -= this.Player_InteractingElevator;
            Exiled.Events.Handlers.Scp049.StartingRecall -= this.Scp049_StartingRecall;
            Exiled.Events.Handlers.Server.RestartingRound -= this.Server_RestartingRound;
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
        }

        private readonly HashSet<Player> notRecallable = new HashSet<Player>();
        private readonly Dictionary<Player, float> scp049DamageRecievedWhileCuffed = new Dictionary<Player, float>();

        private void Server_RestartingRound()
        {
            Commands.DisarmCommand.DisarmedScps.Clear();
            this.notRecallable.Clear();
            this.scp049DamageRecievedWhileCuffed.Clear();
        }

        private void Scp049_StartingRecall(Exiled.Events.EventArgs.StartingRecallEventArgs ev)
        {
            if (Commands.DisarmCommand.DisarmedScps.ContainsValue(ev.Scp049))
            {
                ev.IsAllowed = false;
                return;
            }

            if (this.notRecallable.Contains(ev.Target))
                ev.IsAllowed = false;
        }

        private void Player_Dying(Exiled.Events.EventArgs.DyingEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.Target.IsScp)
            {
                this.notRecallable.Add(ev.Target);
                Timing.CallDelayed(30, () => this.notRecallable.Remove(ev.Target));
            }
        }

        private void Server_RoundStarted()
        {
            // Scp049 Recontainment
            InRange inRange = null;
            List<Player> alreadyRunning = new List<Player>();
            IEnumerator<float> Handler(Player player)
            {
                if (alreadyRunning.Contains(player))
                    yield break;
                if (player.Role.Type != RoleType.Scp049 || !Commands.DisarmCommand.DisarmedScps.ContainsValue(player))
                    yield break;
                alreadyRunning.Add(player);
                var cuffer = Commands.DisarmCommand.DisarmedScps.First(x => x.Value == player).Key;
                for (int i = 4; i > 0; i--)
                {
                    if (!player.IsConnected)
                        yield break;

                    if (!inRange.ColliderInArea.Select(x => Player.Get(x)).Contains(player))
                    {
                        alreadyRunning.Remove(player);
                        player.SetGUI("recontain049", PseudoGUIPosition.MIDDLE, PluginHandler.Instance.Translation.ContainingFailedMessage049, 5);
                        cuffer.SetGUI("recontain049", PseudoGUIPosition.MIDDLE, PluginHandler.Instance.Translation.ContainingFailedMessageCuffer, 5);
                        yield break;
                    }

                    player.SetGUI("recontain049", PseudoGUIPosition.MIDDLE, string.Format(PluginHandler.Instance.Translation.ContainingMessage049, i));
                    cuffer.SetGUI("recontain049", PseudoGUIPosition.MIDDLE, string.Format(PluginHandler.Instance.Translation.ContainingMessageCuffer, i));
                    yield return Timing.WaitForSeconds(1);
                }

                if (Commands.DisarmCommand.DisarmedScps.ContainsValue(player))
                    Commands.DisarmCommand.DisarmedScps.Remove(cuffer);

                alreadyRunning.Remove(player);
                player.SetGUI("recontain049", PseudoGUIPosition.MIDDLE, null);
                cuffer.SetGUI("recontain049", PseudoGUIPosition.MIDDLE, null);

                player.SetRole(cuffer.Role.Team == Team.CHI ? RoleType.ChaosConscript : RoleType.NtfSpecialist, SpawnReason.Escaped, true);
                string recontainerName = "Unspecified";
                if (cuffer.Role.Team == Team.CHI)
                    recontainerName = "Chaos Insurgency";
                else if (cuffer.Role.Type == RoleType.Scientist)
                    recontainerName = "Science Personnel";
                else
                {
                    if (Respawning.NamingRules.UnitNamingRules.AllNamingRules.TryGetValue(Respawning.SpawnableTeamType.NineTailedFox, out var rule))
                        recontainerName = $"Unit {rule.GetCassieUnitName(cuffer.UnitName)}";
                }

                Cassie.MessageTranslated($"SCP 0 4 9 RECONTAINED SUCCESSFULLY BY {recontainerName.ToUpper()}", $"SCP-049 recontained successfully by {recontainerName}");
            }

            if (PluginHandler.Instance.Config.Allow049Recontainment)
            {
                var scp049Chamber = Room.List.First(x => x.Type == RoomType.Hcz049).Transform;
                inRange = InRange.Spawn(scp049Chamber, new Vector3(0f, 266.5f, 14f), new Vector3(20f, 5f, 6f), (Player p) => this.RunCoroutine(Handler(p)));
            }
        }

        private void Player_Died(Exiled.Events.EventArgs.DiedEventArgs ev)
        {
            if (ev.Killer?.Role.Type == RoleType.Scp0492)
            {
                ev.Killer.Health += 100;
                ev.Killer.MaxArtificialHealth += 100;
                ev.Killer.ArtificialHealth += 100;
                if (ev.Killer.MaxHealth < ev.Killer.Health)
                    ev.Killer.Health = ev.Killer.MaxHealth;
            }

            Commands.DisarmCommand.DisarmedScps.Remove(ev.Target);

            /*else if (ev.Killer?.Role == RoleType.Scp049)
            {
                ev.Killer.ArtificialHealth += 50;
            }*/
        }

        private void Player_ChangingRole(Exiled.Events.EventArgs.ChangingRoleEventArgs ev)
        {
            if (!ev.IsAllowed)
                return;

            if (ev.NewRole != RoleType.Spectator)
                this.notRecallable.Remove(ev.Player);

            Commands.DisarmCommand.DisarmedScps.Remove(ev.Player);

            if (ev.NewRole == RoleType.Scp049)
            {
                Timing.RunCoroutine(this.UpdateInfo(ev.Player), "Handler.UpdateInfo");
                Timing.CallDelayed(1, () =>
                {
                    if (ev.IsAllowed && ev.NewRole == RoleType.Scp049 && ev.Player.Role.Type == RoleType.Scp049)
                    {
                        SCP049Shield.Ini<SCP049Shield>(ev.Player);
                        ev.Player.ArtificialHealth = 20f;
                    }
                });
            }
        }

        private void Player_Hurting(Exiled.Events.EventArgs.HurtingEventArgs ev)
        {
            if (Commands.DisarmCommand.DisarmedScps.ContainsValue(ev.Attacker))
            {
                ev.IsAllowed = false;
                return;
            }

            if (Commands.DisarmCommand.DisarmedScps.ContainsValue(ev.Target))
            {
                if (!this.scp049DamageRecievedWhileCuffed.ContainsKey(ev.Target))
                    this.scp049DamageRecievedWhileCuffed.Add(ev.Target, 0f);
                this.scp049DamageRecievedWhileCuffed[ev.Target] += ev.Amount;

                if (this.scp049DamageRecievedWhileCuffed[ev.Target] > PluginHandler.Instance.Config.Scp049UncuffDamage)
                {
                    Commands.DisarmCommand.DisarmedScps.Remove(Commands.DisarmCommand.DisarmedScps.First(x => x.Value == ev.Target).Key);
                    this.scp049DamageRecievedWhileCuffed[ev.Target] = 0f;
                }
            }
        }

        private void Player_InteractingElevator(Exiled.Events.EventArgs.InteractingElevatorEventArgs ev)
        {
            if (Commands.DisarmCommand.DisarmedScps.ContainsValue(ev.Player))
                ev.IsAllowed = false;
        }

        private void Player_InteractingDoor(Exiled.Events.EventArgs.InteractingDoorEventArgs ev)
        {
            if (Commands.DisarmCommand.DisarmedScps.ContainsValue(ev.Player))
                ev.IsAllowed = false;
        }

        private IEnumerator<float> UpdateInfo(Player scp049)
        {
            yield return Timing.WaitForSeconds(1);
            while (scp049.IsConnected && scp049.Role.Type == RoleType.Scp049)
            {
                if (Commands.DisarmCommand.DisarmedScps.ContainsValue(scp049))
                {
                    yield return Timing.WaitForSeconds(1f);
                    continue;
                }

                try
                {
                    List<string> message = new List<string>();
                    foreach (var ragdollObj in Map.Ragdolls.ToArray())
                    {
                        try
                        {
                            if (ragdollObj.NetworkInfo.OwnerHub == null)
                                continue;
                            if (ragdollObj.NetworkInfo.RoleType.GetTeam() == Team.SCP)
                                continue;
                            if (ragdollObj.NetworkInfo.ExistenceTime < 10f)
                            {
                                if (ragdollObj.Base == null)
                                    continue;
                                var distance = Vector3.Distance(scp049.Position, ragdollObj.Base.transform.position);

                                if (distance > 10f)
                                     continue;

                                message.Add(string.Format(PluginHandler.Instance.Translation.PotentialZombiesListElement, ragdollObj.Owner.GetDisplayName(), Mathf.RoundToInt(distance), Mathf.RoundToInt(10f - ragdollObj.NetworkInfo.ExistenceTime)));
                            }
                        }
                        catch (System.Exception ex)
                        {
                            this.Log.Error("Internal");
                            this.Log.Error(ex.Message);
                            this.Log.Error(ex.StackTrace);
                        }
                    }

                    if (message.Count != 0)
                        scp049.SetGUI("scp049", PseudoGUIPosition.BOTTOM, string.Format(PluginHandler.Instance.Translation.PotentialZombiesListMessage, string.Join("<br>", message)));
                    else
                        scp049.SetGUI("scp049", PseudoGUIPosition.BOTTOM, null);
                }
                catch (System.Exception ex)
                {
                    this.Log.Error("External");
                    this.Log.Error(ex.Message);
                    this.Log.Error(ex.StackTrace);
                }

                yield return Timing.WaitForSeconds(1);
            }

            scp049.SetGUI("scp049", PseudoGUIPosition.BOTTOM, null);
        }
    }
}
