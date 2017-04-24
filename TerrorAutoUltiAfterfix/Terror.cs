using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;

namespace TerrorAutoUlti
{
    internal class TerrorAutoUlti
    {
        private static readonly Menu Menu = new Menu("TerrorAutoUlti", "TerrorAutoUlti by Black", true, "npc_dota_hero_terrorblade", true);
        private static bool _active = true;
        private static Hero _me;
        private static System.Collections.Generic.List<Hero> _enemies;
        private static bool _loaded;
        private static void OnLoadEvent(object sender, EventArgs args)
        {
            if (ObjectManager.LocalHero.ClassId != ClassId.CDOTA_Unit_Hero_Terrorblade) return;
            Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("Key", "Combo key").SetValue(new KeyBind('D', KeyBindType.Toggle)));
            Menu.AddItem(new MenuItem("Heel", "Min hp to swap").SetValue(new Slider(2, 1, 5)));
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
            if (!_loaded)
            {
                _me = ObjectManager.LocalHero;

                if (!Game.IsInGame || Game.IsWatchingGame || _me == null ||
                    _me.ClassId != ClassId.CDOTA_Unit_Hero_Terrorblade) return;

                if (!Menu.Item("enabled").IsActive())
                    return;
                _loaded = true;
                _active = true;

            }
            if (!Game.FindKeyValues() || !Utils.SleepCheck("toggle") || Game.IsChatOpen || Game.IsPaused ||
                Game.IsWatchingGame) return;
            Utils.Sleep(200, "toggle");
        }
        public static void Tick(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame)
                return;
            _me = ObjectManager.LocalHero;
            if (_me == null)
                return;
            _enemies = ObjectManager.GetEntities<Hero>().Where(x => _me.Team != x.Team && !x.IsIllusion && x.IsAlive).ToList();
            if (_enemies == null)
                return;

            foreach (var v in _enemies)
            {
                if (!_active) continue;
                if (_me.Player.Hero.Level < 11  && Utils.SleepCheck("1") && _me.Player.Hero.IsAlive && _me.ClassId == ClassId.CDOTA_Unit_Hero_Terrorblade)
                {
                    if (_me.Player.Hero.Health < 200)
                    {
                        if (v.Player.Hero.Health > _me.Player.Hero.Health)
                        {
                            _me.Spellbook.SpellR.UseAbility(v.Player.Hero);
                            Utils.Sleep(1000, "1");
                        }
                    }

                }
                if (_me.Player.Hero.Level <= 11 || !Utils.SleepCheck("2") || !_me.Player.Hero.IsAlive ||
                    _me.ClassId != ClassId.CDOTA_Unit_Hero_Terrorblade) continue;
                if (_me.Player.Hero.Health >= 400) continue;
                if (v.Player.Hero.Health <= _me.Player.Hero.Health) continue;
                _me.Spellbook.SpellR.UseAbility(v.Player.Hero);
                Utils.Sleep(1000, "2");
            }
        }
    }
}