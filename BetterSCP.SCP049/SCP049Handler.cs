// -----------------------------------------------------------------------
// <copyright file="SCP049Handler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Extensions;
using Exiled.API.Features;
using InventorySystem.Disarming;
using MEC;
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
            Exiled.Events.Handlers.Scp049.StartingRecall += this.Scp049_StartingRecall;
            Exiled.Events.Handlers.Server.RestartingRound += this.Server_RestartingRound;
            Exiled.Events.Handlers.Player.Hurting += this.Player_Hurting;

            BetterSCP.SCPGUIHandler.SCPMessages[RoleType.Scp049] = PluginHandler.Instance.Translation.StartMessage;
        }

        public override void OnDisable()
        {
            BetterSCP.SCPGUIHandler.SCPMessages.Remove(RoleType.Scp049);

            Exiled.Events.Handlers.Player.ChangingRole -= this.Player_ChangingRole;
            Exiled.Events.Handlers.Player.Died -= this.Player_Died;
            Exiled.Events.Handlers.Player.Dying -= this.Player_Dying;
            Exiled.Events.Handlers.Scp049.StartingRecall -= this.Scp049_StartingRecall;
            Exiled.Events.Handlers.Server.RestartingRound -= this.Server_RestartingRound;
            Exiled.Events.Handlers.Player.Hurting -= this.Player_Hurting;
        }

        private readonly HashSet<Player> notRecallable = new HashSet<Player>();

        private void Server_RestartingRound()
        {
            Commands.DisarmCommand.DisarmedScps.Clear();
            this.notRecallable.Clear();
        }

        private void Scp049_StartingRecall(Exiled.Events.EventArgs.StartingRecallEventArgs ev)
        {
            if (Commands.DisarmCommand.DisarmedScps.ContainsValue(ev.Scp049))
                ev.IsAllowed = false;
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
            this.RunCoroutine(this.UpdateDisarmed(), "Handler.UpdateDisarmed");
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
                            if (ragdollObj.NetworkInfo.OwnerHub is null)
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
                                message.Add($"<color=yellow>{ragdollObj.NetworkInfo.OwnerHub.name}</color> - <color=yellow>{Mathf.RoundToInt(distance)}</color>m away - <color=yellow>{Mathf.RoundToInt(10f - ragdollObj.NetworkInfo.ExistenceTime)}</color>s");
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
                        scp049.SetGUI("scp049", PseudoGUIPosition.BOTTOM, $"Potential zombies:<br><br>{string.Join("<br>", message)}");
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

        private IEnumerator<float> UpdateDisarmed()
        {
            while (Round.IsStarted)
            {
                foreach (var values in Commands.DisarmCommand.DisarmedScps)
                {
                    if (Vector3.Distance(values.Key.Position, values.Value.Position) >= 30)
                        Commands.DisarmCommand.DisarmedScps.Remove(values.Key);
                }

                yield return Timing.WaitForSeconds(1);
            }
        }
    }
}
