﻿// Credit: Eeny

using System;
using System.Threading;
using System.Threading.Tasks;
using robotManager.Helpful;
using robotManager.Products;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;
using Timer = robotManager.Helpful.Timer;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using robotManager;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public class Main : ICustomClass
{
    public float Range
    {
        get
        {
            return 5.0f;
        }
    }

    private bool _isLaunched;
    private ulong _lastTarget;
    private ulong _currentTarget;
    private uint _target;
    uint oldTarget;

    public void Initialize() // When product started, initialize and launch Fightclass
    {
        _isLaunched = true;
        Logging.Write("Shaman FC Is initialized.");
        Rotation();
    }

    public void Dispose() // When product stopped
    {
        _isLaunched = false;
        Logging.Write("Shaman Stop in progress.");
    }

    public void ShowConfiguration() // When use click on Fight class settings
    {

    }


    // SPELLS:
    public Spell Ghostwolf = new Spell("Ghost Wolf");

    // Buff:


    // Close Combat:
    public Spell HealingWave = new Spell("Healing Wave");
    public Spell LesserHeal = new Spell("Lesser Healing Wave");
    public WoWSpell RockbiterWeapon = new WoWSpell("Rockbiter Weapon", 120000);
    public WoWSpell WindfuryWeapon = new WoWSpell("Windfury Weapon", 120000);
    public Spell EarthShock = new Spell("Earth Shock");
    public Spell FlameShock = new Spell("Flame Shock");
    public Spell LightningShield = new Spell("Lightning Shield");
    public Spell StormStrike = new Spell("StormStrike");
    public WoWSpell SearingTotem = new WoWSpell("Searing Totem", 20000);
    public Spell StoneskinTotem = new Spell("Stoneskin Totem");
    public Spell ManaSpringTotem = new Spell("Mana Spring Totem");
	public Spell Rockbite = new Spell("Rockbiter Weapon");
    public Spell Windfury = new Spell("Windfury Weapon");

    internal void Rotation()
    {
        Logging.Write("Shaman FC started.");
        while (_isLaunched)
        {
            try
            {
                if (!Products.InPause)
                {
                    if (!ObjectManager.Me.IsDeadMe)
                    {
                        Buff();
						UseScroll();
                        if (Fight.InFight && ObjectManager.Me.Target > 0 && ObjectManager.Target.IsAttackable)
                        {
                            CombatRotation();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.WriteError("Shaman FC  ERROR: " + e);
            }

            Thread.Sleep(100); // Pause 10 ms to reduce the CPU usage.
        }
        Logging.Write("Shaman FC  Is now stopped.");
    }
	
    public static int HostileUnitsInRange(float range)
    {
        int hostileUnitsInRange = ObjectManager.GetUnitAttackPlayer().Count(u => u.GetDistance <= range);
        return hostileUnitsInRange;
    }

    public void Buff()
    {
		
        if (ObjectManager.Me.HealthPercent <= 60 && !Fight.InFight)
        {
			MovementManager.StopMoveTo(false, 3000);
            HealingWave.Launch();
			Usefuls.WaitIsCasting();
        }
		
        if (LightningShield.KnownSpell &&ObjectManager.Me.ManaPercentage > 70 && !ObjectManager.Me.HaveBuff("Lightning Shield"))
        {
            LightningShield.Launch();
        }
		
		
		if ((ObjectManager.Target.IsNpcVendor) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Ghost Wolf"))
		{
            Ghostwolf.Launch();
		}	 
		
		// break Wolf for the quest man
 
		 if ((wManager.Wow.ObjectManager.ObjectManager.Target.Reaction == wManager.Wow.Enums.Reaction.Friendly) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Ghost Wolf"))
		 {
            Ghostwolf.Launch();
		 }	
		 if ((wManager.Wow.ObjectManager.ObjectManager.Target.Reaction == wManager.Wow.Enums.Reaction.Honored) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Ghost Wolf"))
		 {
            Ghostwolf.Launch();
		 }	
		 if ((wManager.Wow.ObjectManager.ObjectManager.Target.Reaction == wManager.Wow.Enums.Reaction.Revered) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Ghost Wolf"))
		 {
            Ghostwolf.Launch();
		 }	
		 
		 var nodesNearMe = ObjectManager.GetObjectWoWGameObject().FindAll(p => p.GetDistance <= 8 && p.CanOpen);
		 
		// break wolf for the nodes 
		 if (nodesNearMe.Count > 0 && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Ghost Wolf"))
		 {
            Ghostwolf.Launch();
		 }	 
		// break wolf for the trainer man
		 if ((ObjectManager.Target.IsNpcTrainer) && !(ObjectManager.Me.InCombatFlagOnly) && ObjectManager.Me.HaveBuff("Ghost Wolf"))
		 {
            Ghostwolf.Launch();
		 }	 
    }	
	
    internal void CombatRotation()
    {		
				// auto tag avoid 
         if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && Fight.InFight)
        {
            if (Lua.LuaDoString<bool>(@"return (UnitIsTapped(""target"")) and (not UnitIsTappedByPlayer(""target""));"))
            {
                Fight.StopFight();
                Lua.LuaDoString("ClearTarget();");
                System.Threading.Thread.Sleep(400);
            }
		}
		
		// drop ghost wolf
		
       if (ObjectManager.Me.HaveBuff("Ghost Wolf") && ObjectManager.Me.InCombat)
        {
            Ghostwolf.Launch();
        }
		
		
	    if (!Windfury.KnownSpell)
		{
			this.RockbiterWeapon.Launch();
		}

        if (Windfury.KnownSpell)
		{
			this.WindfuryWeapon.Launch();
		}
        if (FlameShock.KnownSpell && !ObjectManager.Target.HaveBuff("Flame Shock") && ObjectManager.Me.ManaPercentage > 37  && ObjectManager.Target.GetDistance < 17 && !ObjectManager.Target.HaveBuff("Stormstrike"))
        {
            FlameShock.Launch();
        }
						
        if (EarthShock.KnownSpell && ObjectManager.Me.ManaPercentage > 50 && ObjectManager.Me.Level < 12  && ObjectManager.Target.GetDistance < 17)
        {
            EarthShock.Launch();
        }
		
        if (EarthShock.KnownSpell && ObjectManager.Me.ManaPercentage > 50 && ObjectManager.Target.HaveBuff("Stormstrike") && ObjectManager.Target.GetDistance < 17)
        {
            EarthShock.Launch();
        }
		
		 if (SearingTotem.KnownSpell && ObjectManager.Me.ManaPercentage > 40 && ObjectManager.Target.HealthPercent >= 80  && ObjectManager.Target.GetDistance < 9)
        {
            this.SearingTotem.Launch();
        }

		 if (StoneskinTotem.KnownSpell && ObjectManager.Me.ManaPercentage > 10 && !ObjectManager.Me.HaveBuff("Stoneskin") && Methods.HostileUnitsInRange(15.0f) > 1)
        {
            StoneskinTotem.Launch();
        }
		
		 if (ManaSpringTotem.KnownSpell && ObjectManager.Me.ManaPercentage > 10 && !ObjectManager.Me.HaveBuff("Mana Spring") && Methods.HostileUnitsInRange(15.0f) > 1)
        {
            ManaSpringTotem.Launch();
        }
		
		 if (StormStrike.KnownSpell && ObjectManager.Me.ManaPercentage > 65 )
        {
            StormStrike.Launch();
        }
		
        if (ObjectManager.Me.HealthPercent <= 40 && ObjectManager.Me.ManaPercentage > 15)
        {
            LesserHeal.Launch();
        }
        if (!LesserHeal.KnownSpell && ObjectManager.Me.HealthPercent <= 40  && ObjectManager.Me.ManaPercentage > 15)
        {
            HealingWave.Launch();
        }
				
								
    }
internal void UseScroll()
    {
		if (!Fight.InFight)
			{
				// Agi scroll
				if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(3012) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(3012);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1477) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(1477);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4425) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(4425);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10309) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(10309);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27498) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(27498);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33457) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(33457);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(43463) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(43463);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(43464) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(43464);
				}
				
				// int scroll
				
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(955) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(955);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(2290) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(2290);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4419) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(4419);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10308) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(10308);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27499) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(27499);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33458) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(33458);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(37091) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(37091);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(37092) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(37092);
				}
				
				// scroll of protection 
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(3013) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(3013);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1478) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(1478);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4421) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(4421);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10305) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(10305);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27500) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(27500);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33459) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(33459);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(43467) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(43467);
				}
				
				// scroll of spirit
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1181) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(1181);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1712) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(1712);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4424) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(4424);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10306) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(10306);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27501) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(27501);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33460) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(33460);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(37097) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(37097);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(37098) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(37098);
				}
				
				// stamina  buff self
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1180) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(1180);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(1711) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(1711);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4422) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(4422);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10307) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(10307);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27502) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(27502);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33461) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(33461);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(37093) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(37093);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(37094) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(37094);
				}
				
				//strength 
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(954) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(954);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(2289) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(2289);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(4426) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(4426);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(10310) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(10310);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(27503) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(27503);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(33462) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(33462);
				}
				else if (!ObjectManager.Me.IsMounted && ItemsManager.HasItemById(43465) && !ObjectManager.Me.IsDeadMe)
				{
					Interact.InteractGameObject(ObjectManager.Me.GetBaseAddress);
					ItemsManager.UseItem(43465);
				}
			}
    }
	
	
	
    public class WoWSpell : Spell
    {
        private Timer _timer;

        #region Constructor

        /// <summary>
        /// Creates a new instance of the <see cref="WoWSpell"/> class.
        /// </summary>
        /// <param name="spellNameEnglish">The spell name.</param>
        /// <param name="cooldownTimer">The cooldown time.</param>
        public WoWSpell(string spellNameEnglish, double cooldownTimer)
            : base(spellNameEnglish)
        {
            // Set timer
            this._timer = new Timer(cooldownTimer);
        }

        #endregion

        #region Public

        public bool IsReady
        {
            get
            {
                return this._timer.IsReady;
            }
        }

        /// <summary>
        /// Casts the spell if it is ready.
        /// </summary>
        public new void Launch()
        {
            // Is ready?
            if (!this.IsReady)
            {
                // Return
                return;
            }

            // Call launch
            base.Launch();

            // Reset timer
            this._timer.Reset();
        }

        #endregion
    }
	
class Methods
{

    public static int HostileUnitsInRange(float range)
    {
        int hostileUnitsInRange = ObjectManager.GetUnitAttackPlayer().Count(u => u.GetDistance <= range);
        return hostileUnitsInRange;
    }

    // Is Disarmed
    /// <summary>
    /// Determine if the selected unit is Disarmed.
    /// </summary>
    /// <param name="unit">The WoW unit to check.</param>
    /// <returns>True if the selected unit is Disarmed, otherwise false.</returns>

}

}