using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;

namespace TerrorAutoUlti123
{
    class TerrorAutoUlti
    {
        static readonly Menu Menu = new Menu("TerrorAutoUlti", "TerrorAutoUlti by Black", true, "npc_dota_hero_terrorblade", true);
        static Hero me;
        static System.Collections.Generic.List<Hero> _enemies;
        static bool loaded;
        static void OnLoadEvent(object sender, EventArgs args)
        {
            if (ObjectManager.LocalHero.ClassId != ClassId.CDOTA_Unit_Hero_Terrorblade) return;
            Menu.AddItem(new MenuItem("enabled", "Enabled").SetValue(true));
            Menu.AddItem(new MenuItem("toggle", "Toggle").SetValue(new KeyBind('G', KeyBindType.Toggle)));
            Menu.AddItem(new MenuItem("slider", "Min hp % to swag").SetValue(new Slider(25, 5, 75)));
            Menu.AddToMainMenu();
            Game.OnUpdate += Game_OnUpdate;
        }
        static void OnCloseEvent(object sender, EventArgs args)
        {
            Game.OnUpdate -= Game_OnUpdate;
            Menu.RemoveFromMainMenu();
        }
        static void Main()
        {
            Events.OnLoad += OnLoadEvent;
			Game.OnUpdate += Tick;
            Events.OnClose += OnCloseEvent;
		}
        static void Game_OnUpdate(EventArgs args)
        {
            if (!loaded)
            {
                me = ObjectManager.LocalHero;

                if (!Game.IsInGame || Game.IsWatchingGame || me == null ||
                    me.ClassId != ClassId.CDOTA_Unit_Hero_Terrorblade) return;
                loaded = true;

            }
            if (!Utils.SleepCheck("toggle") || Game.IsChatOpen || Game.IsPaused ||
                Game.IsWatchingGame) return;
            Utils.Sleep(200, "toggle");
        }
        public static void Tick(EventArgs args)
        {
            if (!Game.IsInGame || Game.IsPaused || Game.IsWatchingGame || !Menu.Item("enabled").GetValue<bool>())
                return;
            me = ObjectManager.LocalHero;
            if (me == null)
                return;
            _enemies = ObjectManager.GetEntitiesParallel<Hero>().Where(x => me.Team != x.Team && x.IsValid && !x.IsIllusion && x.IsAlive).ToList();
            if (_enemies == null)
                return;

            var ulti = me.Spellbook.SpellR;
           // var ultiLevel = me.Spellbook.SpellR.Level - 1;

            foreach (var v in _enemies)
            {
                if (!me.IsValid || !v.IsValid || !(me.Distance2D(v) <= ulti.GetCastRange()) || !ulti.CanBeCasted() ||
                    !me.CanCast() || !Utils.SleepCheck("1")) continue;
                if (me.Health / me.MaximumHealth * 100 <= Menu.Item("slider").GetValue<Slider>().Value)
                    continue;
                ulti.UseAbility(v);
                Utils.Sleep(1000, "1");
            }
        }
    }
}