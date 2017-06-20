namespace DotaAllCombo.Addons
{
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using System.Security.Permissions;
	using Ensage.Common.Menu;
	using Ensage;
	using Ensage.Common;
	using Ensage.Common.Extensions;
	using System;
	using System.Linq;
	using Service;

	[PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    public class LastHit : IAddon
	{
		private Hero me;

        private bool Active;
        private double apoint;
        private float checkDamage;

	    private readonly HashSet<float> damageSet = new HashSet<float>();
		private float predictDamage;

        private bool tideBringer;
		private float attackRange, bonus; // TODO: Неиспользованная переменная
		private Item qblade, bfury;
        private float pred;

        private Unit enemyUnit;

		

		public void RunScript()
        {
			if (!MainMenu.LastHitMenu.Item("LastOn").GetValue<bool>()) return;

			// Main Initialization
			Active = MainMenu.LastHitMenu.Item("LastHitKey").GetValue<KeyBind>().Active;
            me = ObjectManager.LocalHero;
		    bonus = MainMenu.LastHitMenu.Item("bonusRange").GetValue<Slider>().Value;
            // Items Initialization
            qblade = me.FindItem("item_quelling_blade");
            bfury = me.FindItem("item_bfury");

            // Attack Fields Initialization
            attackRange = me.GetAttackRange() + me.HullRadius + 24;

            // Unit Initialization
            enemyUnit = ObjectManager.GetEntities<Unit>()
                        .Where(x =>
                        (x.ClassId == ClassId.CDOTA_BaseNPC_Tower ||
                        x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Lane
                        || x.ClassId == ClassId.CDOTA_BaseNPC_Creep
                        || x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Neutral
                        || x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Siege
                        || x.ClassId == ClassId.CDOTA_BaseNPC_Additive
                        || x.ClassId == ClassId.CDOTA_BaseNPC_Barracks
                        || x.ClassId == ClassId.CDOTA_BaseNPC_Building
                        || x.ClassId == ClassId.CDOTA_BaseNPC_Creature) && x.IsAlive && x.IsVisible && x.IsSpawned
                        && x.Team != me.Team && x.Distance2D(me) < attackRange + bonus)
                        .OrderBy(creep => creep.Health)
                        .DefaultIfEmpty(null)
                        .FirstOrDefault();

            
            // var orbwalkAbilities = me.GetOrbwalkAbilities().ToList();

            if (me.ClassId == ClassId.CDOTA_Unit_Hero_Kunkka)
			{
				var tide = me.Spellbook.SpellW;

				if (tide.AbilityState != AbilityState.NotLearned && tide.Level > 0)
					tideBringer = tide.Cooldown <= 0;
			}

			double damage = me.DamageAverage + me.BonusDamage;

			if (enemyUnit == null || !enemyUnit.IsValid || !enemyUnit.IsAlive || !enemyUnit.IsVisible) return;

			if ((enemyUnit.ClassId == ClassId.CDOTA_BaseNPC_Creep_Lane ||
			     enemyUnit.ClassId == ClassId.CDOTA_BaseNPC_Creep_Siege ||
			     enemyUnit.ClassId == ClassId.CDOTA_BaseNPC_Tower))
			{
				if (qblade != null && (!(bfury != null || tideBringer || enemyUnit.ClassId == ClassId.CDOTA_BaseNPC_Tower)))
					damage = me.DamageAverage * (attackRange > 195 ? 1.15 : 1.40) + me.BonusDamage;
					
				if (bfury != null && !(tideBringer || enemyUnit.ClassId == ClassId.CDOTA_BaseNPC_Tower))
					damage = me.DamageAverage * (attackRange > 195 ? 1.25 : 1.60) + me.BonusDamage;
					
				if (me.ClassId == ClassId.CDOTA_Unit_Hero_AntiMage)
				{
					var manabreak = me.Spellbook.SpellQ;
					var manaburn = new[] { 28, 40, 52, 64 };

					if (manabreak.AbilityState != AbilityState.NotLearned && manabreak.Level > 0 &&
					    enemyUnit.MaximumMana > 0 && enemyUnit.Mana > 10 &&
					    enemyUnit.Team != me.Team)
					{
						damage += manaburn[manabreak.Level - 1] * 0.60;
					}
				}

				if (me.ClassId == ClassId.CDOTA_Unit_Hero_Viper)
				{
					var nethertoxin = me.Spellbook.SpellW;
					var toxinDamage = new[] { 2.2, 4.7, 7.2, 9.7 };

					if (nethertoxin.AbilityState != AbilityState.NotLearned && nethertoxin.Level > 0 &&
					    enemyUnit.Team != me.Team)
					{
						var HPcent = enemyUnit.Health / enemyUnit.MaximumHealth * 100;

						double netherdamage = 0;
						if (HPcent > 80 && HPcent <= 100)
							netherdamage = toxinDamage[nethertoxin.Level - 1] * 0.5;
						else if (HPcent > 60 && HPcent <= 80)
							netherdamage = toxinDamage[nethertoxin.Level - 1];
						else if (HPcent > 40 && HPcent <= 60)
							netherdamage = toxinDamage[nethertoxin.Level - 1] * 2;
						else if (HPcent > 20 && HPcent <= 40)
							netherdamage = toxinDamage[nethertoxin.Level - 1] * 4;
						else if (HPcent > 0 && HPcent <= 20)
							netherdamage = toxinDamage[nethertoxin.Level - 1] * 8;

						damage += netherdamage;
					}
				}

				if (me.ClassId == ClassId.CDOTA_Unit_Hero_Ursa &&
				    (enemyUnit.ClassId != ClassId.CDOTA_BaseNPC_Tower))
				{
					var furyswipes = me.Spellbook.SpellE;
					var furybuff = enemyUnit.Modifiers
						.Where(x => x.Name == "modifier_ursa_fury_swipes_damage_increase").ToList();
					var furydamage = new[] { 15, 20, 25, 30 };

					if (furyswipes.Level > 0 && enemyUnit.Team != me.Team)
					{
						if (furybuff.Any())
							damage += furydamage[furyswipes.Level - 1] * (furybuff.Count);
						else
							damage += furydamage[furyswipes.Level - 1];
					}
				}

				if (me.ClassId == ClassId.CDOTA_Unit_Hero_BountyHunter &&
				    (enemyUnit.ClassId != ClassId.CDOTA_BaseNPC_Tower))
				{
					var jinada = me.Spellbook.SpellW;
					var jinadaDamage = new[] { 1.5, 1.75, 2, 2.25 };

					if (jinada.AbilityState != AbilityState.NotLearned && jinada.Level > 0 &&
					    jinada.Cooldown <= 0 && enemyUnit.Team != me.Team)
					{
						damage *= jinadaDamage[jinada.Level - 1];
					}
				}

				if (me.ClassId == ClassId.CDOTA_Unit_Hero_Weaver)
				{
					var gem = me.Spellbook.SpellE;

					if (gem.AbilityState != AbilityState.NotLearned && gem.Level > 0 && gem.Cooldown <= 0)
						damage *= 1.8;
				}

				if ((me.ClassId == ClassId.CDOTA_Unit_Hero_SkeletonKing || me.ClassId == ClassId.CDOTA_Unit_Hero_ChaosKnight)
				    && me.NetworkActivity == NetworkActivity.Crit
				    && enemyUnit.ClassId != ClassId.CDOTA_BaseNPC_Tower)
				{
					var critabil = me.Spellbook.SpellE;
					var critmult = new[] { 1.5, 2, 2.5, 3 };

					if (critabil.AbilityState != AbilityState.NotLearned && critabil.Level > 0 && enemyUnit.Team != me.Team)
						damage *= critmult[critabil.Level - 1];
				}

				if ((me.ClassId == ClassId.CDOTA_Unit_Hero_Juggernaut ||
				     me.ClassId == ClassId.CDOTA_Unit_Hero_Brewmaster) &&
				    me.NetworkActivity == NetworkActivity.Crit &&
				    enemyUnit.ClassId != ClassId.CDOTA_BaseNPC_Tower)
				{
					var jugcrit = me.Spellbook.SpellE;
						
					if (jugcrit.AbilityState != AbilityState.NotLearned && jugcrit.Level > 0 && enemyUnit.Team != me.Team)
						damage *= 2;
				}

				if (me.ClassId == ClassId.CDOTA_Unit_Hero_Riki &&
				    (enemyUnit.ClassId != ClassId.CDOTA_BaseNPC_Tower))
				{
					if ((me.Rotation + 180 > enemyUnit.Rotation + 180 - 110 &&
					     me.Rotation + 180 < enemyUnit.Rotation + 180 + 110) &&
					    me.Spellbook.SpellE.Level > 0)
					{
						damage += me.TotalAgility * (me.Spellbook.SpellE.Level * 0.25 + 0.25);
					}
				}
	                
				if (enemyUnit.ClassId == ClassId.CDOTA_BaseNPC_Tower)
					damage -=  enemyUnit.DamageResist;

                if (enemyUnit.ClassId == ClassId.CDOTA_BaseNPC_Creep_Siege)
                    damage *= 0.8;
                
                var aPoint = me.AttackSpeedValue;
                var unit = ObjectManager.GetEntities<Unit>()
                    .Where(x =>
                        (x.ClassId == ClassId.CDOTA_BaseNPC_Tower ||
                         x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Lane
                         || x.ClassId == ClassId.CDOTA_BaseNPC_Creep
                         || x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Neutral
                         || x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Siege
                         || x.ClassId == ClassId.CDOTA_BaseNPC_Additive
                         || x.ClassId == ClassId.CDOTA_BaseNPC_Barracks
                         || x.ClassId == ClassId.CDOTA_BaseNPC_Building
                         || x.ClassId == ClassId.CDOTA_BaseNPC_Creature) && x.IsAlive && x.IsVisible && x.IsSpawned
                        && x.Team != me.Team && x.Distance2D(me) < attackRange + bonus)
                    .OrderBy(creep => creep.Health)
                    .DefaultIfEmpty(null)
                    .ToList();
                var ally = ObjectManager.GetEntities<Unit>()
                    .Where(x =>
                        (x.ClassId == ClassId.CDOTA_BaseNPC_Creep_Lane
                         || x.ClassId == ClassId.CDOTA_BaseNPC_Creep) && x.IsAlive && x.IsVisible && x.IsSpawned
                        && x.Team == me.Team).ToList();
						
				float apoint = me.BaseAttackTime;
                float time = me.IsRanged
                    ? apoint / 1000 + (float)me.GetTurnTime(enemyUnit) + (me.Distance2D(enemyUnit) /
                              UnitDatabase.GetByName(me.Name).ProjectileSpeed)
                    : apoint / 1000 + (float)enemyUnit.GetTurnTime(me);

                
                time *= 1000;
                foreach (var i in unit)
                {
                    uint now = i.Health;
                    Task.Delay((int)time).ContinueWith(_ =>
                    {
                        uint back = i.Health;
                        uint pDmg = now - back;

                        if (i.Handle == enemyUnit.Handle)
                        {
                            if (pDmg <= 0) pDmg = 0;
                            damageSet.Add(pDmg);
                            predictDamage = damageSet.Average(); // TODO: Затестить :D
                            damageSet.Clear();
                            //checkDamage = (now - back);
                            //if (checkDamage <= 0) checkDamage = 0;
                        }
                    });
                    Task.Delay(2000).ContinueWith(_ =>
                    {
                        uint back = i.Health;
                        if (i.Handle == enemyUnit.Handle)
                        {
                            checkDamage = (now - back);
                            if (checkDamage <= 0) checkDamage = 0;
                        }
                    });
                }
                if (MainMenu.LastHitMenu.Item("PredDamage").GetValue<bool>())
			    {
			        if (Game.IsPaused) return;

			        //Console.WriteLine("ProjectileSpeed " + UnitDatabase.GetByName(me.Name).ProjectileSpeed);


			        if (me.IsRanged && ally.Count(x => x.Distance2D(enemyUnit) < 600) > 1 ||
			            ally.Count(x => x.Distance2D(enemyUnit) < 150) > 3)
			        { 
							pred = time/ predictDamage;

                       if (damage < MainMenu.LastHitMenu.Item("maxDamage").GetValue<Slider>().Value)
			           {
			                    damage += pred;
			           }
			        }
                    List<TrackingProjectile> projectiles =
               ObjectManager.TrackingProjectiles.Where(x => x.Target.Handle == enemyUnit.Handle && x.Target.Distance2D(enemyUnit) <=1000).ToList();
                    var projectleTower = ObjectManager.GetEntities<Unit>()
                       .Where(x =>x.ClassId == ClassId.CDOTA_BaseNPC_Tower && x.IsAlive && x.IsVisible && x.IsSpawned
                            ).OrderBy(creep => creep.Distance2D(enemyUnit))
                    .DefaultIfEmpty(null)
                    .ToList();
                    if (projectiles.Count > 0)
                    {
                        for (int i = 0; i < projectiles.Count; ++i)
                        {
                            if (projectiles[i].Source.ClassId == ClassId.CDOTA_BaseNPC_Tower)
                            {
                                Unit tover = Toolset.GetClosestToUTarget(projectleTower, enemyUnit.Position);
                                //Game.PrintMessage("distance " + me.Distance2D(tover));
                                var timeToCreep = projectiles[i].Distance2D(enemyUnit) / projectiles[i].Speed * 1000;
                                //Game.PrintMessage("enemyUnit.Handle " + enemyUnit.Handle);
                                //Game.PrintMessage("proj.Handle " + projectiles[i].Target.Handle);
                                if (enemyUnit.Handle == projectiles[i].Target.Handle)
                                {
                                    
                                    if (timeToCreep < time)
                                    {
                                        //Game.PrintMessage("damageOlD = " + damage);
                                        //Game.PrintMessage("TowDamage " + (tover.DamageAverage));
                                        damage += (tover.DamageAverage);
                                        //Game.PrintMessage("damageNEW = " + damage);
                                    }
                                }
                            }
                        }
                    }
                    /*if (Utils.SleepCheck("Game"))
                                        {
                                            Game.PrintMessage("tow damage " + (tover.DamageAverage));
                                            Game.PrintMessage("projSpeed " + projectiles[i].Distance2D(enemyUnit) / projectiles[i].Speed * 1000);
                                            Game.PrintMessage("timeSpeed " + time);
                                            Game.PrintMessage("damage = " + damage);
                                            Utils.Sleep(20, "Game");
                                        }
                                        */
                }


                // Game.PrintMessage("Damage " + (int)damage);

                Game.PrintMessage("damageFull = " + damage);
                if (!Active || Game.IsPaused || Game.IsChatOpen || !(me.Distance2D(enemyUnit) < attackRange + 500)) return;

				if (enemyUnit.Health < (enemyUnit).DamageTaken((float)damage*2, DamageType.Physical, me)
				    && enemyUnit.Health > (enemyUnit).DamageTaken((float)damage, DamageType.Physical, me)
				    && me.Distance2D(enemyUnit) > attackRange
				    && Utils.SleepCheck("Move"))
				{
					me.Move(enemyUnit.Position);
					Utils.Sleep(150, "Attack");
				}
				if(enemyUnit.Health < (enemyUnit).DamageTaken((float)damage, DamageType.Physical, me) && me.Distance2D(enemyUnit) < attackRange && Utils.SleepCheck("Attack"))
				{
					me.Attack(enemyUnit);
					Utils.Sleep(150, "Attack");
				}
                        
				if ((enemyUnit.ClassId == ClassId.CDOTA_BaseNPC_Creep_Lane 
				     || enemyUnit.ClassId == ClassId.CDOTA_BaseNPC_Creep_Siege
				     || enemyUnit.ClassId == ClassId.CDOTA_BaseNPC_Tower)
				    && enemyUnit.Health < (enemyUnit).DamageTaken((float)(damage * 2), DamageType.Physical, me)
				    && enemyUnit.Health > (enemyUnit).DamageTaken((float)damage, DamageType.Physical, me)
				    && me.Distance2D(enemyUnit) < attackRange)
				{
					if (!Utils.SleepCheck("stop")) return;

					if (ally.Count(x => x.Distance2D(me) < attackRange + 500) > 0)
					{
						if (checkDamage <= 0) return;
						me.Stop();
					}
					me.Attack(enemyUnit);

					if (me.ClassId == ClassId.CDOTA_Unit_Hero_Bristleback)
						Utils.Sleep(aPoint * 0.75, "stop");
					else
						Utils.Sleep(aPoint*0.80, "stop");
				}
			} // if (ClassId)::END
			/*
			switch (me.NetworkName)
			{
				case "CDOTA_Unit_Hero_TemplarAssassin":
					break;

				case "CDOTA_Unit_Hero_Kunkka":
					break;

				default:
					if (me.IsMelee)
					{
						// TODO: Логика для ближников
					}
					else
					{
						// TODO: Логика для дальников
					}
					break;
			}*/
                } // RunScript::END


        public void Load()
		{
			OnLoadMessage();
		}

		public void Unload()
		{
		    me = null;
		    enemyUnit = null;

		}

        private void OnLoadMessage()
		{
			Game.PrintMessage("<font face='verdana' color='#ffa420'>@addon LastHit is Loaded!</font>");
			Service.Debug.Print.ConsoleMessage.Encolored("@addon LastHit is Loaded!", ConsoleColor.Yellow);
		}
	}

}
//static class LastHitExtensions
//{
//	public static IEnumerable<Ability> GetOrbwalkAbilities(this Hero hero, bool forTowers = false)
//	{
//		if (forTowers)
//		{
//			string[] abilities =
//			{
//					"jakiro_liquid_fire",

//				};

//			return hero.Spellbook.Spells.Where(x => abilities.Contains(x.Name));
//		}
//		else
//		{
//			string[] abilities =
//			{
//					"jakiro_liquid_fire",
//					"obsidian_destroyer_arcane_orb",
//					"clinkz_searing_arrows",

//				};

//			return hero.Spellbook.Spells.Where(x => abilities.Contains(x.Name));
//		}
//	}

//	private static double CalculateDamage(this Unit unit, double bonusdamage)
//	{
//		var me = ObjectManager.LocalHero;
//		var quellingBlade = me.FindItem("item_quelling_blade");
//		double modif = 1;
//		double magicdamage = 0;
//		double physDamage = me.MinimumDamage + me.BonusDamage;
//		if (quellingBlade != null && unit.Team != me.Team)
//		{
//			if (me.IsRanged)
//			{
//				physDamage = me.MinimumDamage * 1.15 + me.BonusDamage;
//			}
//			else
//			{
//				physDamage = me.MinimumDamage * 1.4 + me.BonusDamage;
//			}
//		}
//		double bonusdamage2 = 0;
//		switch (me.ClassId)
//		{
//			case ClassId.CDOTA_Unit_Hero_AntiMage:
//				if (unit.MaximumMana > 0 && unit.Mana > 0 && _q.Level > 0 && unit.Team != me.Team)
//					bonusdamage2 = (_q.Level - 1) * 12 + 28 * 0.6;
//				break;
//			case ClassId.CDOTA_Unit_Hero_Viper:
//				if (_w.Level > 0 && unit.Team != me.Team)
//				{
//					var nethertoxindmg = _w.Level * 2.5;
//					//var percent = Math.Floor((double) unit.Health / unit.MaximumHealth * 100);
//					//if (percent > 80 && percent <= 100)
//					// bonusdamage2 = nethertoxindmg * 0.5;
//					//else if (percent > 60 && percent <= 80)
//					// bonusdamage2 = nethertoxindmg * 1;
//					//else if (percent > 40 && percent <= 60)
//					// bonusdamage2 = nethertoxindmg * 2;
//					//else if (percent > 20 && percent <= 40)
//					// bonusdamage2 = nethertoxindmg * 4;
//					//else if (percent > 0 && percent <= 20)
//					// bonusdamage2 = nethertoxindmg * 8;
//				}
//				break;
//			case ClassId.CDOTA_Unit_Hero_Ursa:
//				var furymodif = 0;
//				if (me.Modifiers.Any(x => x.Name == "modifier_ursa_fury_swipes_damage_increase"))
//					furymodif =
//					unit.Modifiers.Find(x => x.Name == "modifier_ursa_fury_swipes_damage_increase").StackCount;
//				if (_e.Level > 0)
//				{
//					if (furymodif > 0)
//						bonusdamage2 = furymodif * ((_e.Level - 1) * 5 + 15);
//					else
//						bonusdamage2 = (_e.Level - 1) * 5 + 15;
//				}
//				break;
//			case ClassId.CDOTA_Unit_Hero_BountyHunter:
//				if (_w.Level > 0 && _w.AbilityState == AbilityState.Ready)
//					bonusdamage2 = physDamage * ((_w.Level - 1) * 0.25 + 0.50);
//				break;
//			case ClassId.CDOTA_Unit_Hero_Weaver:
//				if (_e.Level > 0 && _e.AbilityState == AbilityState.Ready)
//					bonusdamage2 = physDamage;
//				break;
//			case ClassId.CDOTA_Unit_Hero_Kunkka:
//				if (_w.Level > 0 && _w.AbilityState == AbilityState.Ready && _w.IsAutoCastEnabled)
//					bonusdamage2 = (_w.Level - 1) * 15 + 25;
//				break;
//			case ClassId.CDOTA_Unit_Hero_Juggernaut:
//				if (_e.Level > 0)
//					if (me.NetworkActivity == NetworkActivity.Crit)
//						bonusdamage2 = physDamage;
//				break;
//			case ClassId.CDOTA_Unit_Hero_Brewmaster:
//				if (_e.Level > 0)
//					if (me.NetworkActivity == NetworkActivity.Crit)
//						bonusdamage2 = physDamage;
//				break;
//			case ClassId.CDOTA_Unit_Hero_ChaosKnight:
//				if (_e.Level > 0)
//					if (me.NetworkActivity == NetworkActivity.Crit)
//						bonusdamage2 = physDamage * ((_e.Level - 1) * 0.5 + 0.25);
//				break;
//			case ClassId.CDOTA_Unit_Hero_SkeletonKing:
//				if (_e.Level > 0)
//					if (me.NetworkActivity == NetworkActivity.Crit)
//						bonusdamage2 = physDamage * ((_e.Level - 1) * 0.5 + 0.5);
//				break;
//			case ClassId.CDOTA_Unit_Hero_Life_Stealer:
//				if (_w.Level > 0)
//					bonusdamage2 = unit.Health * ((_w.Level - 1) * 0.01 + 0.045);
//				break;
//		}

//		if (me.Modifiers.Any(x => x.Name == "modifier_bloodseeker_bloodrage"))
//		{
//			modif = modif *
//			(ObjectManager.GetEntities<Hero>()
//			.First(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Bloodseeker)
//			.Spellbook.Spell1.Level - 1) * 0.05 + 1.25;
//		}
//		if (me.Modifiers.Any(x => x.Name == "modifier_storm_spirit_overload"))
//		{
//			magicdamage = magicdamage + (_e.Level - 1) * 20 + 30;
//		}
//		if (me.Modifiers.Any(x => x.Name == "modifier_chilling_touch"))
//		{
//			magicdamage = magicdamage + (_e.Level - 1) * 20 + 50;
//		}
//		if (me.ClassId == ClassId.CDOTA_Unit_Hero_Pudge && _w.Level > 0 && Menu.Item("usespell").GetValue<bool>() &&
//		me.Distance2D(unit) <= _attackRange)
//		{
//			magicdamage = magicdamage + (_w.Level - 1) * 6 + 6;
//		}

//		bonusdamage = bonusdamage + bonusdamage2;
//		var damageMp = 1 - 0.06 * unit.Armor / (1 + 0.06 * Math.Abs(unit.Armor));
//		magicdamage = magicdamage * (1 - unit.MagicDamageResist);

//		var realDamage = ((bonusdamage + physDamage) * damageMp + magicdamage) * modif;
//		if (unit.ClassId == ClassId.CDOTA_BaseNPC_Creep_Siege ||
//		unit.ClassId ==
//		ClassId.CDOTA_BaseNPC_Tower)
//		{
//			realDamage = realDamage / 2;
//		}
//		if (realDamage > unit.MaximumHealth)
//			realDamage = unit.Health + 10;

//		return realDamage;
//	}
