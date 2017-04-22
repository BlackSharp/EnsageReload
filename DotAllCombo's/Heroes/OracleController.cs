using DotaAllCombo.Extensions;

namespace DotaAllCombo.Heroes
{
	using System; using System.Runtime; using System.Runtime;
	using System.Collections.Generic;
	using System.Linq;
	using Ensage;
	using Ensage.Common;
	using Ensage.Common.Extensions;
	using Ensage.Common.Menu;
	using Service.Debug;

	internal class OracleController : Variables, IHeroController
	{
		private Ability _q, _w, _e, _r;
		private Item _urn, _ethereal, _dagon, _halberd, _mjollnir, _orchid, _abyssal, _shiva, _mail, _bkb, _satanic, _medall, _glimmer, _manta, _pipe, _guardian, _sphere;

	    public OracleController(Item urn)
	    {
	        _urn = urn;
	    }


	    public void Combo()
		{
			if (!Menu.Item("enabled").IsActive())
				return;
			Active = Game.IsKeyDown(Menu.Item("keyBind").GetValue<KeyBind>().Key);

			_q = Me.Spellbook.SpellQ;
			_w = Me.Spellbook.SpellW;
			_e = Me.Spellbook.SpellE;
			_r = Me.Spellbook.SpellR;
            
			_glimmer = Me.FindItem("item_glimmer_cape");
			_manta = Me.FindItem("item_manta");
			_pipe = Me.FindItem("item_pipe");
			_guardian = Me.FindItem("item_guardian_greaves") ?? Me.FindItem("item_mekansm");
			_sphere = Me.FindItem("item_sphere");
			_dagon = Me.Inventory.Items.FirstOrDefault(x => x.Name.Contains("item_dagon"));
			_ethereal = Me.FindItem("item_ethereal_blade");
			_halberd = Me.FindItem("item_heavens_halberd");
			_mjollnir = Me.FindItem("item_mjollnir");
			_orchid = Me.FindItem("item_orchid") ?? Me.FindItem("item_bloodthorn");
			_abyssal = Me.FindItem("item_abyssal_blade");
			_mail = Me.FindItem("item_blade_mail");
			_bkb = Me.FindItem("item_black_king_bar");
			_satanic = Me.FindItem("item_satanic");
			_medall = Me.FindItem("item_medallion_of_courage") ?? Me.FindItem("item_solar_crest");
			_shiva = Me.FindItem("item_shivas_guard");

			var modifInv =
				Me.Modifiers.All(
					x =>
						x.Name == "modifier_item_silver_edge_windwalk" || x.Name == "modifier_item_edge_windwalk" ||
						x.Name == "modifier_treant_natures_guise" || x.Name == "modifier_rune_invis");
			var v =
				   ObjectManager.GetEntities<Hero>()
					   .Where(x => x.Team != Me.Team && x.IsAlive && x.IsVisible && !x.IsIllusion)
					   .ToList();

            E = Toolset.ClosestToMouse(Me);
            if (E == null) return;
            if (Active && Me.Distance2D(E) <= 1400 && E.IsAlive && !modifInv && Utils.SleepCheck("Combo"))
            {
                if ( // 
                        _e != null
                        && _e.CanBeCasted()
                        && Me.CanCast()
                        && !E.HasModifier("oracle_fates_edict")
                        && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_e.Name)
                        && Utils.SleepCheck("E")
                        && Me.Distance2D(E) <= _e.GetCastRange() + 400
                        )
                {
                    _e.UseAbility(E);
                    Utils.Sleep(250, "E");
                }

                if ( // 
                    _q != null
                    && _q.CanBeCasted()
                    &&
                    (_e == null || !_e.CanBeCasted() || !Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_e.Name))
                    && Me.CanCast()
                    && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_q.Name)
                    && !E.HasModifier("oracle_fates_edict")
                    && Me.Distance2D(E) <= _q.GetCastRange() + 400
                    && Utils.SleepCheck("Q")
                    )
                {
                    _q.UseAbility(E);
                    Utils.Sleep(250, "Q");
                }
                
                if (_q != null && ((!Menu.Item("Q spell").GetValue<bool>() || _q == null || !Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_q.Name)) && (!_q.IsInAbilityPhase && !_q.IsChanneling && !Me.IsChanneling())))
                {
                    
                    if (Menu.Item("orbwalk").GetValue<bool>() && Me.Distance2D(E) <= 1900 && Utils.SleepCheck("Orbwalk"))
                    {
                        Orbwalking.Orbwalk(E, 0, 1600, true, true);
                        Utils.Sleep(150, "Orbwalk");
                    }
                    if ( // Mjollnir
                        _mjollnir != null
                        && _mjollnir.CanBeCasted()
                        && Me.CanCast()
                        && !E.IsMagicImmune()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_mjollnir.Name)
                        && Utils.SleepCheck("mjollnir")
                        && Me.Distance2D(E) <= 900
                        )
                    {
                        _mjollnir.UseAbility(Me);
                        Utils.Sleep(250, "mjollnir");
                    } // Mjollnir Item end
                    if ( // Medall
                        _medall != null
                        && _medall.CanBeCasted()
                        && Utils.SleepCheck("Medall")
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_medall.Name)
                        && Me.Distance2D(E) <= 700
                        )
                    {
                        _medall.UseAbility(E);
                        Utils.Sleep(250, "Medall");
                    } // Medall Item end
                    if (_orchid != null && _orchid.CanBeCasted() && Me.Distance2D(E) <= 900
                        && !E.HasModifier("oracle_fates_edict")
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_orchid.Name) && Utils.SleepCheck("orchid"))
                    {
                        _orchid.UseAbility(E);
                        Utils.Sleep(100, "orchid");
                    }

                    if (_shiva != null && _shiva.CanBeCasted() && Me.Distance2D(E) <= 600
                        && !E.HasModifier("oracle_fates_edict")
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_shiva.Name)
                        && !E.IsMagicImmune() && Utils.SleepCheck("Shiva"))
                    {
                        _shiva.UseAbility();
                        Utils.Sleep(100, "Shiva");
                    }

                    if (_ethereal != null && _ethereal.CanBeCasted()
                        && Me.Distance2D(E) <= 700 && Me.Distance2D(E) <= 400
                        && !E.HasModifier("oracle_fates_edict")
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_ethereal.Name) &&
                        Utils.SleepCheck("ethereal"))
                    {
                        _ethereal.UseAbility(E);
                        Utils.Sleep(100, "ethereal");
                    }


                    if ( // Dagon
                        Me.CanCast()
                        && _dagon != null
                        && (_ethereal == null
                            || (Me.HasModifier("modifier_item_ethereal_blade_slow")
                                || _ethereal.Cooldown < 17))
                        && !E.IsLinkensProtected()
                        && _dagon.CanBeCasted()
                        && !E.HasModifier("oracle_fates_edict")
                        && !E.IsMagicImmune()
                        && Utils.SleepCheck("dagon")
                        )
                    {
                        _dagon.UseAbility(E);
                        Utils.Sleep(200, "dagon");
                    } // Dagon Item end
                    if ( // Abyssal Blade
                        _abyssal != null
                        && _abyssal.CanBeCasted()
                        && Me.CanCast()
                        && !E.IsStunned()
                        && !E.IsHexed()
                        && Utils.SleepCheck("abyssal")
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_abyssal.Name)
                        && Me.Distance2D(E) <= 400
                        )
                    {
                        _abyssal.UseAbility(E);
                        Utils.Sleep(250, "abyssal");
                    } // Abyssal Item end
                    if (_urn != null && _urn.CanBeCasted() && !E.HasModifier("oracle_fates_edict") && _urn.CurrentCharges > 0 && Me.Distance2D(E) <= 400
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_urn.Name) && Utils.SleepCheck("urn"))
                    {
                        _urn.UseAbility(E);
                        Utils.Sleep(240, "urn");
                    }
                    if ( // Hellbard
                        _halberd != null
                        && _halberd.CanBeCasted()
                        && Me.CanCast()
                        && !E.IsMagicImmune()
                        && !E.HasModifier("oracle_fates_edict")
                        && (E.NetworkActivity == NetworkActivity.Attack
                            || E.NetworkActivity == NetworkActivity.Crit
                            || E.NetworkActivity == NetworkActivity.Attack2)
                        && Utils.SleepCheck("halberd")
                        && Me.Distance2D(E) <= 700
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_halberd.Name)
                        )
                    {
                        _halberd.UseAbility(E);
                        Utils.Sleep(250, "halberd");
                    }
                    if ( // Satanic 
                        _satanic != null &&
                        Me.Health <= (Me.MaximumHealth * 0.3) &&
                        _satanic.CanBeCasted() &&
                        Me.Distance2D(E) <= Me.AttackRange + 50
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_satanic.Name)
                        && Utils.SleepCheck("satanic")
                        )
                    {
                        _satanic.UseAbility();
                        Utils.Sleep(240, "satanic");
                    } // Satanic Item end
                    if (_mail != null && _mail.CanBeCasted() && (v.Count(x => x.Distance2D(Me) <= 650) >=
                                                               (Menu.Item("Heelm").GetValue<Slider>().Value)) &&
                        Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_mail.Name) && Utils.SleepCheck("mail"))
                    {
                        _mail.UseAbility();
                        Utils.Sleep(100, "mail");
                    }
                    if (_bkb != null && _bkb.CanBeCasted() && (v.Count(x => x.Distance2D(Me) <= 650) >=
                                                             (Menu.Item("Heel").GetValue<Slider>().Value)) &&
                        Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_bkb.Name) && Utils.SleepCheck("bkb"))
                    {
                        _bkb.UseAbility();
                        Utils.Sleep(100, "bkb");
                    }
                }
                Utils.Sleep(250, "Combo");
            }
            A();
        }

		private void A()
		{
			var ally = ObjectManager.GetEntities<Hero>()
									.Where(x => x.Team == Me.Team && x.IsAlive && x.IsVisible && !x.IsIllusion).ToList();
			var v =
				   ObjectManager.GetEntities<Hero>()
					   .Where(x => x.Team != Me.Team && x.IsAlive && x.IsVisible && !x.IsIllusion)
					   .ToList();
			var countAlly = ally.Count();
			var countV = v.Count();
            double[] penitence = { 0, 1.15, 1.2, 1.25, 1.3 };
            double[] bloodrage = { 0, 1.15, 1.2, 1.25, 1.3 };
            double[] soul = { 0, 1.2, 1.3, 1.4, 1.5 };
            for (int i = 0; i < countAlly; ++i)
			{
				if (countV <= 0) return;
				for (int z = 0; z < countV; ++z)
				{

					if (!Me.IsInvisible())
					{
						/*if (
							W != null && W.CanBeCasted() && Me.Distance2D(Ally[i]) <= W.GetCastRange()+50
							&& Ally[i].Health <= (Me.MaximumHealth * 0.6) && !Q.CanBeCasted()
							&& Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(W.Name)
							&& Utils.SleepCheck("W")
							)
						{
							W.UseAbility(Ally[i]);
							Utils.Sleep(200, "W");
						}
						*/
						if (
							_w != null && _w.CanBeCasted()
							&& !v[z].IsMagicImmune()
							&& Me.Distance2D(v[z]) <= v[z].AttackRange + v[z].HullRadius + 50
							&& v[z].NetworkActivity == NetworkActivity.Attack
							&& v[z].Level >= 8
							&& Menu.Item("SkillsAutoTarget").GetValue<AbilityToggler>().IsEnabled(_w.Name)
							&& Utils.SleepCheck("W")
							)
						{
							_w.UseAbility(v[z]);
							Utils.Sleep(200, "W");
						}

						if (_e != null && _e.CanBeCasted()
						   && Me.Distance2D(ally[i]) <= _e.GetCastRange() + 50
						   && !ally[i].IsMagicImmune()
						   && ally[i].HasModifier("modifier_oracle_false_promise")
						   && Menu.Item("SkillsAutoAlly").GetValue<AbilityToggler>().IsEnabled(_e.Name)
						   && Utils.SleepCheck("E")
						   )
						{
							_e.UseAbility(ally[i]);
							Utils.Sleep(150, "E");
						}

						if (
							_guardian != null && _guardian.CanBeCasted()
							&& Me.Distance2D(ally[i]) <= _guardian.GetCastRange()
							&& (v.Count(x => x.Distance2D(Me) <= _guardian.GetCastRange()) >= (Menu.Item("healsetTarget").GetValue<Slider>().Value))
							&& (ally.Count(x => x.Distance2D(Me) <= _guardian.GetCastRange()) >= (Menu.Item("healsetAlly").GetValue<Slider>().Value))
							&& ally[i].Health <= (ally[i].MaximumHealth / 100 * (Menu.Item("HealhHeal").GetValue<Slider>().Value))
							&& Menu.Item("ItemsS").GetValue<AbilityToggler>().IsEnabled(_guardian.Name)
							&& Utils.SleepCheck("guardian")
							)
						{
							_guardian.UseAbility();
							Utils.Sleep(200, "guardian");
						}

						if (
							_r != null && _r.CanBeCasted()
							&& Me.Distance2D(ally[i]) <= _r.GetCastRange() + 100
							&& ally[i].Health <= (ally[i].MaximumHealth / 100 * (Menu.Item("HealhHealUlt").GetValue<Slider>().Value))
							&& ally[i].Distance2D(v[z]) <= 700
							&& Menu.Item("SkillsAutoAlly").GetValue<AbilityToggler>().IsEnabled(_r.Name)
							&& Utils.SleepCheck("R")
							)
						{
							_r.UseAbility(ally[i]);
							Utils.Sleep(200, "R");
						}

						uint[] eDmg = { 0, 90, 180, 270, 360 };

						var damage = Math.Floor(eDmg[_e.Level] * (1 - v[z].MagicDamageResist));
						var lens = Me.HasModifier("modifier_item_aether_lens");
						var spellamplymult = 1 + (Me.TotalIntelligence / 16 / 100);
						if (v[z].NetworkName == "CDOTA_Unit_Hero_Spectre" && v[z].Spellbook.Spell3.Level > 0)
						{
							damage =
								Math.Floor(eDmg[_e.Level - 1] *
										   (1 - (0.10 + v[z].Spellbook.Spell3.Level * 0.04)) * (1 - v[z].MagicDamageResist));
						}
						if (lens) damage = damage * 1.08;
						if (v[z].HasModifier("modifier_kunkka_ghost_ship_damage_absorb")) damage = damage * 0.5;
						if (v[z].HasModifier("modifier_item_mask_of_madness_berserk")) damage = damage * 1.3;

                        if (v[z].HasModifier("modifier_bloodseeker_bloodrage"))
                        {
                            var blood =
                                ObjectManager.GetEntities<Hero>()
                                    .FirstOrDefault(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Bloodseeker);
                            if (blood != null)
                                damage = damage * bloodrage[blood.Spellbook.Spell1.Level];
                            else
                                damage = damage * 1.4;
                        }
                        
                        if (v[z].HasModifier("modifier_chen_penitence"))
                        {
                            var chen =
                                ObjectManager.GetEntities<Hero>()
                                    .FirstOrDefault(x => x.Team == Me.Team && x.ClassId == ClassId.CDOTA_Unit_Hero_Chen);
                            if (chen != null)
                                damage = damage * penitence[chen.Spellbook.Spell1.Level];
                        }
                        
                        if (v[z].HasModifier("modifier_shadow_demon_soul_catcher"))
                        {
                            var demon =
                                ObjectManager.GetEntities<Hero>()
                                    .FirstOrDefault(x => x.Team == Me.Team && x.ClassId == ClassId.CDOTA_Unit_Hero_Shadow_Demon);
                            if (demon != null)
                                damage = damage * soul[demon.Spellbook.Spell2.Level];
                        }

                        damage = damage * spellamplymult;

						if (_e != null && _e.CanBeCasted()
							&& !v[z].HasModifier("modifier_tusk_snowball_movement")
							&& !v[z].HasModifier("modifier_snowball_movement_friendly")
							&& !v[z].HasModifier("modifier_templar_assassin_refraction_absorb")
							&& !v[z].HasModifier("modifier_ember_spirit_flame_guard")
							&& !v[z].HasModifier("modifier_ember_spirit_sleight_of_fist_caster_invulnerability")
							&& !v[z].HasModifier("modifier_obsidian_destroyer_astral_imprisonment_prison")
							&& !v[z].HasModifier("modifier_puck_phase_shift")
							&& !v[z].HasModifier("modifier_eul_cyclone")
							&& !v[z].HasModifier("modifier_dazzle_shallow_grave")
							&& !v[z].HasModifier("modifier_shadow_demon_disruption")
							&& !v[z].HasModifier("modifier_necrolyte_reapers_scythe")
							&& !v[z].HasModifier("modifier_necrolyte_reapers_scythe")
							&& !v[z].HasModifier("modifier_storm_spirit_ball_lightning")
							&& !v[z].HasModifier("modifier_ember_spirit_fire_remnant")
							&& !v[z].HasModifier("modifier_nyx_assassin_spiked_carapace")
							&& !v[z].HasModifier("modifier_phantom_lancer_doppelwalk_phase")
							&& !v[z].HasModifier("oracle_fates_edict")
							&& !v[z].HasModifier("modifier_abaddon_borrowed_time_damage_redirect")
							&& !v[z].IsMagicImmune()
							&& Menu.Item("SkillsAutoTarget").GetValue<AbilityToggler>().IsEnabled(_e.Name)
							&& v[z].Health <= damage - 5
							&& Me.Distance2D(v[z]) <= _e.GetCastRange() + 10
							&& Utils.SleepCheck(E.Handle.ToString()))
						{
							_e.UseAbility(v[z]);
							Utils.Sleep(150, E.Handle.ToString());
						}


						if (_w != null && _w.CanBeCasted()
							&& Me.Distance2D(ally[i]) <= _w.GetCastRange() + 50
							&& !ally[i].IsMagicImmune()
							&& ally[i].HasModifier("modifier_oracle_false_promise")
							&& Menu.Item("SkillsAutoAlly").GetValue<AbilityToggler>().IsEnabled(_w.Name)
							&& Utils.SleepCheck("W")
							)
						{
							_w.UseAbility(ally[i]);
							Utils.Sleep(200, "W");
						}
						var spray = ally[i].Modifiers.FirstOrDefault(y => y.Name == "modifier_bristleback_quill_spray_stack");
						var napalm = ally[i].Modifiers.FirstOrDefault(y => y.Name == "modifier_batrider_sticky_napalm");

						if (_q != null && _q.CanBeCasted() && Me.Distance2D(ally[i]) <= _q.GetCastRange() + 50
							&& (ally[i].IsSilenced()
							|| ally[i].IsHexed()
							|| ally[i].HasModifier("modifier_item_diffusal_blade")
							|| ally[i].HasModifier("modifier_slardar_amplify_damage")
							|| ally[i].HasModifier("modifier_invoker_cold_snap_freeze")
							|| ally[i].HasModifier("modifier_item_urn_damage")
							|| ally[i].HasModifier("modifier_rod_of_atos_debuff")
							|| ally[i].HasModifier("modifier_brewmaster_thunder_clap")
							|| ally[i].HasModifier("modifier_brewmaster_drunken_haze")
							|| ally[i].HasModifier("modifier_ursa_earthshock")
							|| ally[i].HasModifier("modifier_night_stalker_void")
							|| ally[i].HasModifier("modifier_ogre_magi_ignite")
							|| (spray != null && spray.StackCount >= 3)
							|| (napalm != null && napalm.StackCount >= 4)
							)
							&& !ally[i].HasModifier("modifier_legion_commander_duel")
							&& !ally[i].HasModifier("modifier_riki_smoke_screen")
							&& !ally[i].HasModifier("modifier_disruptor_static_storm")
							&& !ally[i].IsMagicImmune()
							&& Menu.Item("SkillsAutoAlly").GetValue<AbilityToggler>().IsEnabled(_q.Name)
							&& Utils.SleepCheck("Q")
							)
						{
							_q.UseAbility(ally[i]);
							Me.Stop();
							Utils.Sleep(200, "Q");
						}


						if (_q != null && _q.CanBeCasted() && Me.Distance2D(v[z]) <= _q.GetCastRange() + 50 &&
							Menu.Item("SkillsAutoTarget").GetValue<AbilityToggler>().IsEnabled(_q.Name)
							&& (v[z].HasModifier("modifier_rune_haste")
							|| v[z].HasModifier("modifier_rune_regen")
							|| v[z].HasModifier("modifier_rune_doubledamage")
							|| v[z].HasModifier("modifier_legion_commander_press_the_attack")
							|| v[z].HasModifier("modifier_ogre_magi_bloodlust")
							)
							&& !v[z].IsMagicImmune()
							&& Utils.SleepCheck("Q")
							)
						{
							_q.UseAbility(v[z]);
							Me.Stop();
							Utils.Sleep(200, "Q");
						}
						var modi = ally[i].Modifiers.FirstOrDefault(x => x.Name == "modifier_stunned");
						var mod = ally[i].Modifiers.FirstOrDefault(x => x.Name == "modifier_stun");
						if (_r != null && _r.CanBeCasted() && Me.Distance2D(ally[i]) <= _r.GetCastRange() + 50
							&& ((mod != null && mod.RemainingTime >= 1.6 + Game.Ping)
							|| (modi != null && mod.RemainingTime >= 1.6 + Game.Ping)
							|| ally[i].HasModifier("modifier_batrider_flaming_lasso")
							)
							&& Menu.Item("SkillsAutoAlly").GetValue<AbilityToggler>().IsEnabled(_r.Name)
							&& Utils.SleepCheck("R")
							)
						{
							_r.UseAbility(ally[i]);
							Utils.Sleep(200, "R");
						}
						if (
						   _manta != null && _manta.CanBeCasted()
						   && (Me.Distance2D(v[z]) <= Me.AttackRange + Me.HullRadius + 10)
						   || (Me.Distance2D(v[z]) <= v[z].AttackRange + Me.HullRadius + 10)
						   && Menu.Item("ItemsS").GetValue<AbilityToggler>().IsEnabled(_manta.Name)
						   && Utils.SleepCheck("manta")
						   )
						{
							_manta.UseAbility();
							Utils.Sleep(200, "manta");
						}
						if (
						 _pipe != null && _pipe.CanBeCasted()
						 && Me.Distance2D(ally[i]) <= _pipe.GetCastRange()
						 && (v.Count(x => x.Distance2D(Me) <= _pipe.GetCastRange()) >= (Menu.Item("pipesetTarget").GetValue<Slider>().Value))
						 && (ally.Count(x => x.Distance2D(Me) <= _pipe.GetCastRange()) >= (Menu.Item("pipesetAlly").GetValue<Slider>().Value))
						 && Menu.Item("ItemsS").GetValue<AbilityToggler>().IsEnabled(_pipe.Name)
						 && Utils.SleepCheck("pipe")
						 )
						{
							_pipe.UseAbility();
							Utils.Sleep(200, "pipe");
						}

						if (
							_sphere != null && _sphere.CanBeCasted() && Me.Distance2D(ally[i]) <= _sphere.GetCastRange() + 50
							&& !ally[i].IsMagicImmune()
							&& ((ally[i].Distance2D(v[z]) <= ally[i].AttackRange + ally[i].HullRadius + 10)
							|| (ally[i].Distance2D(v[z]) <= v[z].AttackRange + ally[i].HullRadius + 10)
							|| ally[i].Health <= (Me.MaximumHealth * 0.5))
							&& Menu.Item("ItemsS").GetValue<AbilityToggler>().IsEnabled(_sphere.Name)
							&& Utils.SleepCheck("sphere")
							)
						{
							_sphere.UseAbility(ally[i]);
							Utils.Sleep(200, "sphere");
						}

						if (
							_glimmer != null && _glimmer.CanBeCasted() && Me.Distance2D(ally[i]) <= _glimmer.GetCastRange() + 50
							&& ally[i].Health <= (Me.MaximumHealth * 0.5)
							&& Menu.Item("ItemsS").GetValue<AbilityToggler>().IsEnabled(_glimmer.Name)
							&& Utils.SleepCheck("glimmer")
							)
						{
							_glimmer.UseAbility(ally[i]);
							Utils.Sleep(200, "glimmer");
						}
						if (_w != null && _w.CanBeCasted() && Me.Distance2D(v[z]) <= _w.GetCastRange() + 50
						   && !v[z].IsMagicImmune() &&
						   (v[z].HasModifier("modifier_sven_gods_strength")
						   || v[z].HasModifier("modifier_rune_doubledamage")
						   || ((v[z].ClassId == ClassId.CDOTA_Unit_Hero_Legion_Commander && v[z].FindSpell("legion_commander_duel").Cooldown <= 0 && ally[i].Distance2D(v[z]) <= 500)
						   || (v[z].ClassId == ClassId.CDOTA_Unit_Hero_Sniper && v[z].Level >= 8)
						   || (v[z].ClassId == ClassId.CDOTA_Unit_Hero_DrowRanger && v[z].Level >= 8)
						   || (v[z].ClassId == ClassId.CDOTA_Unit_Hero_Ursa && v[z].Distance2D(ally[i]) <= 300 && v[z].NetworkActivity == NetworkActivity.Attack)
						   || (v[z].ClassId == ClassId.CDOTA_Unit_Hero_TemplarAssassin && v[z].Distance2D(ally[i]) <= 300 && v[z].IsAttacking()))
						   )
						   && Menu.Item("SkillsAutoTarget").GetValue<AbilityToggler>().IsEnabled(_w.Name)
						   && Utils.SleepCheck("W")
						   )
						{
							_w.UseAbility(v[z]);
							Utils.Sleep(200, "W");
						}
					}
				}
			}
		}
		public void OnLoadEvent()
		{
			AssemblyExtensions.InitAssembly("VickTheRock", "0.1b");

			Print.LogMessage.Success("I Safe All Life, My Freand's!");

			Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
			Menu.AddItem(new MenuItem("orbwalk", "orbwalk").SetValue(true));
            Menu.AddItem(new MenuItem("Q spell", "Wait time chaneling Fortunes End(Q spell)").SetValue(true));
            Menu.AddItem(new MenuItem("keyBind", "Combo key").SetValue(new KeyBind('D', KeyBindType.Press)));

		    Menu items = new Menu("Items And Spel's Combo", "Items");
			Menu heal = new Menu("Healing Items Settings", "Heal");
			Menu spell = new Menu("Auto Use Spell Q+W+E+R Logic", "AutoSpell");
			Menu ally = new Menu("Auto Healing | Purge Logic", "Autoally");
			Menu enemy = new Menu("Auto Debuf | KillSteal Q+W+E Logic", "Autoenemy");
			items.AddItem(
				new MenuItem("Skills", "Skills").SetValue(new AbilityToggler(new Dictionary<string, bool>
				{
				    {"oracle_purifying_flames", true},
				    {"oracle_fortunes_end", true},
				    {"oracle_fates_edict", true},
				    {"oracle_false_promise", true}
				})));
			items.AddItem(
				new MenuItem("Items", "Items:").SetValue(new AbilityToggler(new Dictionary<string, bool>
				{
				    {"item_heavens_halberd", true},
				    {"item_orchid", true},
					{"item_bloodthorn", true},
				    {"item_mjollnir", true},
				    {"item_urn_of_shadows", true},
				    {"item_ethereal_blade", true},
				    {"item_abyssal_blade", true},
				    {"item_shivas_guard", true},
				    {"item_blade_mail", true},
				    {"item_black_king_bar", true},
				    {"item_satanic", true},
				    {"item_medallion_of_courage", true},
				    {"item_solar_crest", true}
				})));
			items.AddItem(
				new MenuItem("ItemsS", "Items:").SetValue(new AbilityToggler(new Dictionary<string, bool>
				{
				    {"item_manta", true},
				    {"item_mekansm", true},
				    {"item_pipe", true},
				    {"item_guardian_greaves", true},
				    {"item_sphere", true},
				    {"item_glimmer_cape", true}
				})));
			Menu.AddItem(new MenuItem("Heel", "Min Target's to BKB").SetValue(new Slider(2, 1, 5)));
			Menu.AddItem(new MenuItem("Heelm", "Min Target's to BladeMail").SetValue(new Slider(2, 1, 5)));
			heal.AddItem(new MenuItem("pipesetTarget", "Min Target's to Pipe").SetValue(new Slider(2, 1, 5)));
			heal.AddItem(new MenuItem("pipesetAlly", "Min Ally to Pipe").SetValue(new Slider(2, 1, 5)));
			heal.AddItem(new MenuItem("healsetTarget", "Min Target's to Meka | Guardian").SetValue(new Slider(2, 1, 5)));
			heal.AddItem(new MenuItem("healsetAlly", "Min Ally to Meka | Guardian").SetValue(new Slider(2, 1, 5)));
			heal.AddItem(new MenuItem("HealhHeal", "Min healh % to item's Heal").SetValue(new Slider(35, 10, 70))); // x/ 10%
			ally.AddItem(
				new MenuItem("SkillsAutoAlly", "Skills").SetValue(new AbilityToggler(new Dictionary<string, bool>
				{
				    {"oracle_purifying_flames", true},
				    {"oracle_fortunes_end", true},
				    {"oracle_fates_edict", true},
				    {"oracle_false_promise", true}
				})));
			ally.AddItem(new MenuItem("HealhHealUlt", "Min Ally Healh % to Ult").SetValue(new Slider(35, 10, 70)));
			enemy.AddItem(
				new MenuItem("SkillsAutoTarget", "Skills").SetValue(new AbilityToggler(new Dictionary<string, bool>
				{
				    {"oracle_purifying_flames", true},
				    {"oracle_fortunes_end", true},
				    {"oracle_fates_edict", true},
				})));

			Menu.AddSubMenu(items);
			Menu.AddSubMenu(heal);
			Menu.AddSubMenu(spell);
			spell.AddSubMenu(ally);
			spell.AddSubMenu(enemy);
		}

		public void OnCloseEvent()
		{
			
		}
	}
}
 