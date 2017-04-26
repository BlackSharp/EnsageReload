using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

namespace AutoSunder
{
    internal class Program
    {
        private static readonly Menu Menu = new Menu("AutoSunder", "AutoSunder by Black", true,
            "npc_dota_hero_terrorblade", true);

        private static Hero _me;
        private static List<Hero> _enemies;

        private static void OnLoadEvent(object sender, EventArgs args)
        {
            if (ObjectManager.LocalHero.ClassId != ClassId.CDOTA_Unit_Hero_Terrorblade) return;
            Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("slider", "Min hp % to swap").SetValue(new Slider(25, 10, 75)));
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
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame || !Menu.Item("enabled").GetValue<bool>())
                return;
            _me = ObjectManager.LocalHero;
            if (_me == null)
                return;
            _enemies = ObjectManager.GetEntitiesParallel<Hero>()
                .Where(x => _me.Team != x.Team && x.IsValid && !x.IsIllusion && x.IsAlive)
                .ToList();
            if (_enemies == null)
                return;
            var ulti = _me.Spellbook.SpellR;
            foreach (var v in _enemies)
            {
                if (!_me.IsValid || !v.IsValid || !(_me.Distance2D(v) <= ulti.GetCastRange()) || !ulti.CanBeCasted() ||
                    !_me.CanCast() || !Utils.SleepCheck("1")) continue;
                if (!((double) _me.Health / _me.MaximumHealth * 100D <=
                      Menu.Item("slider").GetValue<Slider>().Value)) continue;
                ulti.UseAbility(v);
                Utils.Sleep(1000, "1");
            }
        }
    }
}