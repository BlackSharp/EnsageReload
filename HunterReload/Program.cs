using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

namespace HunterReload
{
    internal class Program
    {

        private static bool _active, _rActive;
        private static Ability _q, _r;
        private static Hero _me, _e;
        private static Item _urn, _ethereal, _dagon, _halberd, _mjollnir, _orchid, _abyssal, _mom, _shiva, _mail, _bkb, _satanic, _medall;
        private static readonly Menu Menu = new Menu("BountyHunter", "BountyHunter", true, "npc_dota_hero_bounty_hunter");

        private static void OnLoadEvent(object sender, EventArgs args)
        {
            if (ObjectManager.LocalHero.ClassId != ClassId.CDOTA_Unit_Hero_BountyHunter) return;
            Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("orbwalk", "orbwalk").SetValue(true));
            Menu.AddItem(new MenuItem("keyBind", "Combo key").SetValue(new KeyBind('D', KeyBindType.Press)));

            Menu.AddItem(new MenuItem("KeyR", "Use Auto Track").SetValue(new KeyBind('R', KeyBindType.Toggle)));
            Menu.AddItem(
                new MenuItem("Skills", "Skills").SetValue(new AbilityToggler(new Dictionary<string, bool>
                {
                    {"bounty_hunter_track", true},
                    {"bounty_hunter_shuriken_toss", true}
                })));
            Menu.AddItem(
                new MenuItem("Items", "Items:").SetValue(new AbilityToggler(new Dictionary<string, bool>
                {
                    {"item_mask_of_madness", true},
                    {"item_heavens_halberd", true},
                    {"item_mjollnir", true},
                    {"item_orchid", true}, {"item_bloodthorn", true},
                    {"item_urn_of_shadows", true},
                    {"item_ethereal_blade", true},
                    {"item_veil_of_discord", true},
                    {"item_abyssal_blade", true},
                    {"item_shivas_guard", true},
                    {"item_blade_mail", true},
                    {"item_black_king_bar", true},
                    {"item_satanic", true},
                    {"item_medallion_of_courage", true},
                    {"item_solar_crest", true}
                })));
            Menu.AddItem(new MenuItem("Heel", "Min targets to BKB").SetValue(new Slider(2, 1, 5)));
            Menu.AddItem(new MenuItem("Heelm", "Min targets to BladeMail").SetValue(new Slider(2, 1, 5)));
            Menu.AddToMainMenu();
            Game.OnUpdate += Game_OnUpdate;
        }
        private static void OnCloseEvent(object sender, EventArgs args)
        {
            Game.OnUpdate -= Game_OnUpdate;
            Menu.RemoveFromMainMenu();
        } // OnClose
        static void Main()
        {
            Events.OnLoad += OnLoadEvent;
            Events.OnClose += OnCloseEvent;
        }




        public static void Game_OnUpdate(EventArgs args)
        {
            _me = ObjectManager.LocalHero;
            if (!Game.IsInGame || _me == null || _me.ClassId != ClassId.CDOTA_Unit_Hero_BountyHunter) return;
            if (!Menu.Item("enabled").IsActive())
                return;
			
            _active = Game.IsKeyDown(Menu.Item("keyBind").GetValue<KeyBind>().Key);
			
            _rActive = Menu.Item("KeyR").GetValue<KeyBind>().Active;
            _q = _me.Spellbook.SpellQ;
            _r = _me.Spellbook.SpellR;

			

            var enemies = ObjectManager.GetEntities<Hero>()
                .Where(x => x.Team != _me.Team && x.IsAlive && x.IsVisible && !x.IsIllusion && !x.IsMagicImmune())
                .ToList();
            if (_active)
            {
                _e = _me.ClosestToMouseTarget(1800);
                if (_e == null)
                    return;
                _mom = _me.FindItem("item_mask_of_madness");
                _urn = _me.FindItem("item_urn_of_shadows");
                _dagon = _me.Inventory.Items.FirstOrDefault(x => x.Name.Contains("item_dagon"));
                _ethereal = _me.FindItem("item_ethereal_blade");
                _halberd = _me.FindItem("item_heavens_halberd");
                _mjollnir = _me.FindItem("item_mjollnir");
                _orchid = _me.FindItem("item_orchid") ?? _me.FindItem("item_bloodthorn");
                _abyssal = _me.FindItem("item_abyssal_blade");
                _mail = _me.FindItem("item_blade_mail");
                _bkb = _me.FindItem("item_black_king_bar");
                _satanic = _me.FindItem("item_satanic");
                _medall = _me.FindItem("item_medallion_of_courage") ?? _me.FindItem("item_solar_crest");
                _shiva = _me.FindItem("item_shivas_guard");

                if (Menu.Item("orbwalk").GetValue<bool>() && _me.Distance2D(_e) <= 1900)
                {
                    Orbwalking.Orbwalk(_e, 0, 1600, true, true);
                }
                if (_active && _me.Distance2D(_e) <= 1400 && _e != null && _e.IsAlive && !_me.IsInvisible())
                {
                    if (
                        _q != null && _q.CanBeCasted() && _me.Distance2D(_e) <= 1500
                        && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_q.Name)
                        && Utils.SleepCheck("Q")
                    )
                    {
                        _q.UseAbility(_e);
                        Utils.Sleep(200, "Q");
                    }
                    if (
                        _r != null && _r.CanBeCasted() && _me.Distance2D(_e) <= 1500
                        && Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_r.Name)
                        && !_me.HasModifier("modifier_bounty_hunter_wind_walk")
                        && !_me.IsChanneling()
                        && Utils.SleepCheck("R")
                    )
                    {
                        _r.UseAbility(_e);
                        Utils.Sleep(200, "R");
                    }

                    if ( // MOM
                        _mom != null
                        && _mom.CanBeCasted()
                        && _me.CanCast()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_mom.Name)
                        && Utils.SleepCheck("mom")
                        && _me.Distance2D(_e) <= 700
                    )
                    {
                        _mom.UseAbility();
                        Utils.Sleep(250, "mom");
                    }
                    if ( // Mjollnir
                        _mjollnir != null
                        && _mjollnir.CanBeCasted()
                        && _me.CanCast()
                        && !_e.IsMagicImmune()
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_mjollnir.Name)
                        && Utils.SleepCheck("mjollnir")
                        && _me.Distance2D(_e) <= 900
                    )
                    {
                        _mjollnir.UseAbility(_me);
                        Utils.Sleep(250, "mjollnir");
                    } // Mjollnir Item end
                    if ( // Medall
                        _medall != null
                        && _medall.CanBeCasted()
                        && Utils.SleepCheck("Medall")
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_medall.Name)
                        && _me.Distance2D(_e) <= 700
                    )
                    {
                        _medall.UseAbility(_e);
                        Utils.Sleep(250, "Medall");
                    } // Medall Item end
                    if ( // Hellbard
                        _halberd != null
                        && _halberd.CanBeCasted()
                        && _me.CanCast()
                        && !_e.IsMagicImmune()
                        && (_e.NetworkActivity == NetworkActivity.Attack
                            || _e.NetworkActivity == NetworkActivity.Crit
                            || _e.NetworkActivity == NetworkActivity.Attack2)
                        && Utils.SleepCheck("halberd")
                        && _me.Distance2D(_e) <= 700
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_halberd.Name)
                    )
                    {
                        _halberd.UseAbility(_e);
                        Utils.Sleep(250, "halberd");
                    }
                    if (_orchid != null && _orchid.CanBeCasted() && _me.Distance2D(_e) <= 900 &&
                        Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_orchid.Name) && Utils.SleepCheck("orchid"))
                    {
                        _orchid.UseAbility(_e);
                        Utils.Sleep(100, "orchid");
                    }

                    if (_shiva != null && _shiva.CanBeCasted() && _me.Distance2D(_e) <= 600
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_shiva.Name)
                        && !_e.IsMagicImmune() && Utils.SleepCheck("Shiva"))
                    {
                        _shiva.UseAbility();
                        Utils.Sleep(100, "Shiva");
                    }

                    if (_ethereal != null && _ethereal.CanBeCasted()
                        && _me.Distance2D(_e) <= 700 && _me.Distance2D(_e) <= 400
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_ethereal.Name) &&
                        Utils.SleepCheck("ethereal"))
                    {
                        _ethereal.UseAbility(_e);
                        Utils.Sleep(100, "ethereal");
                    }

                    if (_dagon != null
                        && _dagon.CanBeCasted()
                        && _me.Distance2D(_e) <= 500
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled("item_dagon")
                        && Utils.SleepCheck("dagon"))
                    {
                        _dagon.UseAbility(_e);
                        Utils.Sleep(100, "dagon");
                    }
                    if ( // Abyssal Blade
                        _abyssal != null
                        && _abyssal.CanBeCasted()
                        && _me.CanCast()
                        && !_e.IsStunned()
                        && !_e.IsHexed()
                        && Utils.SleepCheck("abyssal")
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_abyssal.Name)
                        && _me.Distance2D(_e) <= 400
                    )
                    {
                        _abyssal.UseAbility(_e);
                        Utils.Sleep(250, "abyssal");
                    } // Abyssal Item end
                    if (_urn != null && _urn.CanBeCasted() && _urn.CurrentCharges > 0 && _me.Distance2D(_e) <= 400
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_urn.Name) && Utils.SleepCheck("urn"))
                    {
                        _urn.UseAbility(_e);
                        Utils.Sleep(240, "urn");
                    }
                    if ( // Satanic 
                        _satanic != null &&
                        _me.Health <= (_me.MaximumHealth * 0.3) &&
                        _satanic.CanBeCasted() &&
                        _me.Distance2D(_e) <= _me.AttackRange + 50
                        && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_satanic.Name)
                        && Utils.SleepCheck("satanic")
                    )
                    {
                        _satanic.UseAbility();
                        Utils.Sleep(240, "satanic");
                    } // Satanic Item end
                    if (_mail != null && _mail.CanBeCasted() && (enemies.Count(x => x.Distance2D(_me) <= 650) >=
                                                                 (Menu.Item("Heelm").GetValue<Slider>().Value)) &&
                        Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_mail.Name) && Utils.SleepCheck("mail"))
                    {
                        _mail.UseAbility();
                        Utils.Sleep(100, "mail");
                    }
                    if (_bkb != null && _bkb.CanBeCasted() && (enemies.Count(x => x.Distance2D(_me) <= 650) >=
                                                               (Menu.Item("Heel").GetValue<Slider>().Value)) &&
                        Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_bkb.Name) && Utils.SleepCheck("bkb"))
                    {
                        _bkb.UseAbility();
                        Utils.Sleep(100, "bkb");
                    }
                }
            }
			
            if (_rActive && _me.IsAlive && _r != null && _r.CanBeCasted())
                if (!_me.HasModifier("modifier_bounty_hunter_wind_walk")|| _me.IsVisibleToEnemies)
                    foreach (var v in enemies)
                    {
                        var checkMod = v.Modifiers.Where(y => y.Name == "modifier_bounty_hunter_track").DefaultIfEmpty(null).FirstOrDefault();
                        var invItem = v.FindItem("item_glimmer_cape")?? v.FindItem("item_invis_sword") ?? v.FindItem("item_silver_edge") ?? v.FindItem("item_glimmer_cape");
                        if (
                            (( v.ClassId == ClassId.CDOTA_Unit_Hero_Riki     || v.ClassId == ClassId.CDOTA_Unit_Hero_Broodmother
                               || v.ClassId == ClassId.CDOTA_Unit_Hero_Clinkz   || v.ClassId == ClassId.CDOTA_Unit_Hero_Invoker
                               || v.ClassId == ClassId.CDOTA_Unit_Hero_SandKing || v.ClassId == ClassId.CDOTA_Unit_Hero_TemplarAssassin
                               || v.ClassId == ClassId.CDOTA_Unit_Hero_Treant   || v.ClassId == ClassId.CDOTA_Unit_Hero_PhantomLancer
                             )
                             || (
                                 v.Modifiers.Any(x =>
                                     (x.Name == "modifier_riki_permanent_invisibility" 
                                      || x.Name == "modifier_mirana_moonlight_shadow" 
                                      || x.Name == "modifier_treant_natures_guise" 
                                      || x.Name == "modifier_weaver_shukuchi" 
                                      || x.Name == "modifier_broodmother_spin_web_invisible_applier" 
                                      || x.Name == "modifier_item_invisibility_edge_windwalk" 
                                      || x.Name == "modifier_rune_invis" 
                                      || x.Name == "modifier_clinkz_wind_walk" 
                                      || x.Name == "modifier_item_shadow_amulet_fade" 
                                      || x.Name == "modifier_item_silver_edge_windwalk" 
                                      || x.Name == "modifier_item_edge_windwalk" 
                                      || x.Name == "modifier_nyx_assassin_vendetta" 
                                      || x.Name == "modifier_invisible" 
                                      || x.Name == "modifier_invoker_ghost_walk_enemy")))
                             ||(invItem!=null && invItem.Cooldown<=0)
                             || v.Health <= (v.MaximumHealth * 0.5)) 
                            && _me.Distance2D(v) <= _r.GetCastRange()+_me.HullRadius 
                            && (!v.HasModifier("modifier_bounty_hunter_track") || checkMod != null && checkMod.RemainingTime <= 2)
                            && Utils.SleepCheck("R"))
                        {
                            _r.UseAbility(v);
                            Utils.Sleep(300, "R");
                        }
                    }
        }
		
    }
}