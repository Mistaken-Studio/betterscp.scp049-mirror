// -----------------------------------------------------------------------
// <copyright file="SCP049Shield.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using UnityEngine;

namespace Mistaken.BetterSCP.SCP049
{
    internal class SCP049Shield : API.Shield.Shield
    {
        protected override float MaxShield => this.localMaxShield;

        protected override float ShieldRechargeRate => 5f;

        protected override float ShieldEffectivnes => .8f;

        protected override float TimeUntilShieldRecharge => 15f;

        protected override float ShieldDropRateOnOverflow => 0f;

        protected override void Start()
        {
            base.Start();

            // this.inRange = API.Components.InRangeBall.Spawn(this.Player.GameObject.transform, Vector3.zero, 10, 5);
            this.InvokeRepeating(nameof(this.UpdateLoop), 5f, 1f);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            this.Player.SetGUI("scp049", PseudoGUIPosition.BOTTOM, null);

            // Destroy(this.inRange);
        }

        private static float SCP0492Regeneration { get; set; } = 1.5f;

        private static int SCP049MaxShieldPerZombie { get; set; } = 100;

        private float localMaxShield = 200;

        // private API.Components.InRangeBall inRange;
        private void UpdateLoop()
        {
            if (this.Player.Role != RoleType.Scp049)
                throw new Exception("Player is not SCP049");

            try
            {
                int maxShield = 200;
                HashSet<Player> inRange = new HashSet<Player>();
                foreach (var zombie in Physics.OverlapSphere(this.Player.Position, 10))
                {
                    try
                    {
                        var zombiePlayer = Player.Get(zombie.transform.root.gameObject);
                        if (inRange.Contains(zombiePlayer))
                            continue;
                        inRange.Add(zombiePlayer);
                        if (zombiePlayer == null || zombiePlayer.Role != RoleType.Scp0492)
                            continue;

                        maxShield += SCP049MaxShieldPerZombie;
                        if (zombiePlayer.MaxHealth > zombiePlayer.Health)
                            zombiePlayer.Health = Math.Min(zombiePlayer.MaxHealth, zombiePlayer.Health + SCP0492Regeneration);
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                    }
                }

                /*foreach (var zombie in this.inRange.ColliderInArea)
                {
                    try
                    {
                        var zombiePlayer = Player.Get(zombie);
                        if (zombiePlayer == null || zombiePlayer.Role != RoleType.Scp0492)
                            continue;

                        maxShield += SCP049MaxShieldPerZombie;
                        if (zombiePlayer.MaxHealth > zombiePlayer.Health)
                            zombiePlayer.Health += SCP0492Regeneration;
                    }
                    catch (System.Exception ex)
                    {
                        Log.Error(ex.Message);
                        Log.Error(ex.StackTrace);
                    }
                }*/

                this.localMaxShield = maxShield;
            }
            catch (System.Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }
        }
    }
}
