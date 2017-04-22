namespace TinyReload
{
    using System;
	using System.Linq;
	using System.Collections.Generic;
	using Ensage;
	using SharpDX;
	using Ensage.Common.Extensions;
	using Ensage.Common;
	using Ensage.Common.Menu;

	internal class Program
	{
		private static readonly Menu Menu = new Menu("Tiny", "Tiny by Vick", true, "npc_dota_hero_tiny", true);
		private static Ability _q, _w;
		private static bool _active;
		private static Hero _me, _e;
		private static Item _urn, _ethereal, _dagon, _halberd, _vail, _mjollnir, _orchid, _abyssal, _mom, _shiva, _mail, _bkb, _satanic, _medall, _blink;
		
		private static void OnLoadEvent(object sender, EventArgs args)
		{
			if (ObjectManager.LocalHero.ClassId != ClassId.CDOTA_Unit_Hero_Tiny) return;

			Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
			Menu.AddItem(new MenuItem("orbwalk", "orbwalk").SetValue(true));
			Menu.AddItem(new MenuItem("keyBind", "Combo key").SetValue(new KeyBind('D', KeyBindType.Press)));

			Menu.AddItem(
				new MenuItem("Skills", "Skills").SetValue(new AbilityToggler(new Dictionary<string, bool>
				{
					{"tiny_avalanche", true},
					{"tiny_toss", true}
				})));
			Menu.AddItem(
				new MenuItem("Items", "Items:").SetValue(new AbilityToggler(new Dictionary<string, bool>
				{
					{"item_blink", true},
					{"item_mask_of_madness", true},
					{"item_heavens_halberd", true},
					{"item_orchid", true},
					{ "item_bloodthorn", true},
					{"item_mjollnir", true},
					{"item_dagon", true},
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
		}


		private static void Main()
		{
			Events.OnLoad += OnLoadEvent;
			Events.OnClose += OnCloseEvent;
		}

		private static void Game_OnUpdate(EventArgs args)
		{
			_me = ObjectManager.LocalHero;
			if (!Game.IsInGame || Game.IsWatchingGame || _me==null || _me.ClassId != ClassId.CDOTA_Unit_Hero_Tiny) return;

			if (!Menu.Item("enabled").IsActive())
				return;

			_e = _me.ClosestToMouseTarget(1800);
			if (_e == null)
				return;
			_active = Game.IsKeyDown(Menu.Item("keyBind").GetValue<KeyBind>().Key);

			_q = _me.Spellbook.SpellQ;
			_w = _me.Spellbook.SpellW;

			_blink = _me.FindItem("item_blink");
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
			_vail = _me.FindItem("item_veil_of_discord");

			var modifEther = _e.Modifiers.Any(y => y.Name == "modifier_item_ethereal_blade_slow");
			var stoneModif = _e.Modifiers.Any(y => y.Name == "modifier_medusa_stone_gaze_stone");
			var v =
				ObjectManager.GetEntities<Hero>()
					.Where(x => x.Team != _me.Team && x.IsAlive && x.IsVisible && !x.IsIllusion && !x.IsMagicImmune())
					.ToList();
			var isInvisible =
				_me.IsInvisible();
			if (_active)
			{
				if (Menu.Item("orbwalk").GetValue<bool>() && _me.Distance2D(_e) <= 1900)
				{
					Orbwalking.Orbwalk(_e, 0, 1600, true, true);
				}
			}
			if (_active && _me.Distance2D(_e) <= 1400 && _e.IsAlive && !isInvisible)
			{
				float angle = _me.FindAngleBetween(_e.Position, true);
				Vector3 pos = new Vector3((float)(_e.Position.X - 100 * Math.Cos(angle)), (float)(_e.Position.Y - 100 * Math.Sin(angle)), 0);
				if (
					_blink != null
					&& _q.CanBeCasted()
					&& _me.CanCast()
					&& _blink.CanBeCasted()
					&& _me.Distance2D(_e) >= _me.GetAttackRange()+ _me.HullRadius+150
					&& _me.Distance2D(pos) <= 1180
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_blink.Name)
					&& Utils.SleepCheck("blink")
					)
				{
					_blink.UseAbility(pos);
					Utils.Sleep(250, "blink");
				}
				if (_orchid != null && _orchid.CanBeCasted() && _me.Distance2D(_e) <= 900 &&
					   Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_orchid.Name) && Utils.SleepCheck("orchid"))
				{
					_orchid.UseAbility(_e);
					Utils.Sleep(100, "orchid");
				}
				if ( // vail
				_vail != null
				&& _vail.CanBeCasted()
				&& _me.CanCast()
				&& !_e.IsMagicImmune()
				&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_vail.Name)
				&& _me.Distance2D(_e) <= 1500
				&& Utils.SleepCheck("vail")
				)
				{
					_vail.UseAbility(_e.Position);
					Utils.Sleep(250, "vail");
				} // orchid Item end
				if (_ethereal != null && _ethereal.CanBeCasted()
					   && _me.Distance2D(_e) <= 700 && _me.Distance2D(_e) <= 400
					   && Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_ethereal.Name) &&
					   Utils.SleepCheck("ethereal"))
				{
					_ethereal.UseAbility(_e);
					Utils.Sleep(100, "ethereal");
				}

				if ( // Dagon
					_me.CanCast()
					&& _dagon != null
					&& (_ethereal == null
						|| (modifEther
							|| _ethereal.Cooldown < 17))
					&& !_e.IsLinkensProtected()
					&& _dagon.CanBeCasted()
					&& _me.Distance2D(_e) <= 1400
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled("item_dagon")
					&& !_e.IsMagicImmune()
					&& !stoneModif
					&& Utils.SleepCheck("dagon")
					)
				{
					_dagon.UseAbility(_e);
					Utils.Sleep(200, "dagon");
				} // Dagon Item end
				if (
					_q != null && _q.CanBeCasted() && _me.Distance2D(_e) <= 1500
					&& Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_q.Name)
					&& Utils.SleepCheck("Q")
					)
				{
					_q.UseAbility(_e.Predict(400));
					Utils.Sleep(200, "Q");
				}
				if (
					_w != null && _w.CanBeCasted() && _me.Distance2D(_e) <= 1500
					&& Menu.Item("Skills").GetValue<AbilityToggler>().IsEnabled(_w.Name)
					&& Utils.SleepCheck("W")
					)
				{
					_w.UseAbility(_e);
					Utils.Sleep(200, "W");
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


				if (_shiva != null && _shiva.CanBeCasted() && _me.Distance2D(_e) <= 600
					&& Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_shiva.Name)
					&& !_e.IsMagicImmune() && Utils.SleepCheck("Shiva"))
				{
					_shiva.UseAbility();
					Utils.Sleep(100, "Shiva");
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
				if (_mail != null && _mail.CanBeCasted() && (v.Count(x => x.Distance2D(_me) <= 650) >=
														   (Menu.Item("Heelm").GetValue<Slider>().Value)) &&
					Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_mail.Name) && Utils.SleepCheck("mail"))
				{
					_mail.UseAbility();
					Utils.Sleep(100, "mail");
				}
				if (_bkb != null && _bkb.CanBeCasted() && (v.Count(x => x.Distance2D(_me) <= 650) >=
														 (Menu.Item("Heel").GetValue<Slider>().Value)) &&
					Menu.Item("Items").GetValue<AbilityToggler>().IsEnabled(_bkb.Name) && Utils.SleepCheck("bkb"))
				{
					_bkb.UseAbility();
					Utils.Sleep(100, "bkb");
				}
			}
		}
	}
}