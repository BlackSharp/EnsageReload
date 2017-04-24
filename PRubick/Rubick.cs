using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;


namespace PRubick
{
    internal class Rubick
    {

        #region Fields

        private const string Version = "v1.0.0.0";
        private static bool _loaded;

        private static EzElement _stealIfHave;
        private static EzElement _enabled;
        private static EzElement _lastSpell;

        private static EzGui _gui;
        private static EzElement _spcat;
        private static EzElement _heroes;

        private static Hero _myHero;
        private static Ability _spellSteal;
        private static readonly int[] CastRange = {
			1000, 1400
		};

        private static readonly List<Hero> UiHeroes = new List<Hero>();
        private static readonly List<Hero> LastSpellIsChecked = new List<Hero>();
        private static readonly Dictionary<string, string> AbilitiesFix = new Dictionary<string, string>();
        private static readonly List<string> IncludedAbilities = new List<string>();



        #endregion

        #region Init

        public static void Init()
        {
            Game.OnUpdate += Game_OnUpdate;
            
            // abilitiesFix
            AbilitiesFix.Add("ancient_apparition_ice_blast_release", "ancient_apparition_ice_blast");
        }

        #endregion

        #region Update

        private static void Game_OnUpdate(EventArgs args)
        {
            if (!Game.IsInGame) { _loaded = false; return; }
            if (Game.GameState == GameState.WaitForMapToLoad || Game.GameState == GameState.HeroSelection || Game.GameState == GameState.PreGame || Game.GameState == GameState.WaitForPlayersToLoad || Game.GameState == GameState.PostGame || Game.GameState == GameState.StrategyTime) return;
            #region If assembly not loaded
            if (_loaded == false)
            {
                _gui = new EzGui(Drawing.Width - 350, 60, "PRubick " + Version);
                _enabled = new EzElement(ElementType.Checkbox, "Enabled / Активен", true);
                _spcat = new EzElement(ElementType.Category, "Spell Steal / Кража скиллов", false);
                _stealIfHave = new EzElement(ElementType.Checkbox, "Steal if no cd / Красть если нет кд [if selected]", false);
                _lastSpell = new EzElement(ElementType.Checkbox, "Steal if last spell selected [unstable]", false);
                _gui.AddMainElement(new EzElement(ElementType.Text, "Main / Главная", false));
                _gui.AddMainElement(_enabled);
                _gui.AddMainElement(_stealIfHave);
                _gui.AddMainElement(_lastSpell);
                _gui.AddMainElement(new EzElement(ElementType.Text, "Rubick / Рубик (_heroes appear gradually)", false));
                _gui.AddMainElement(_spcat);
                UiHeroes.Clear();
                _myHero = ObjectManager.LocalHero;
                _spellSteal = _myHero.Spellbook.SpellR;
                _loaded = true;
            }
            #endregion
            if (_myHero.ClassId != ClassId.CDOTA_Unit_Hero_Rubick) return;
            //
            if (!_enabled.IsActive) return;
            var enemies = ObjectManager.GetEntities<Hero>().Where(x => x.Team != _myHero.Team && !x.IsIllusion() && x.IsAlive && x.IsVisible ).ToArray();
            #region GUI Checks
            if (Utils.SleepCheck("GUI_ABILITIES") && _heroes != null)
            {
                foreach (var hero in _heroes.GetElements())
                {
                    foreach (var spell in hero.GetElements())
                    {
                        if (spell.IsActive && !IncludedAbilities.Contains(spell.Content)) IncludedAbilities.Add(spell.Content);
                        if (!spell.IsActive && IncludedAbilities.Contains(spell.Content)) IncludedAbilities.Remove(spell.Content);
                    }
                }
                Utils.Sleep(1000, "GUI_ABILITIES");
            }

            if (Utils.SleepCheck("uiheroesupdate"))
            {
                if (_heroes == null) { _heroes = new EzElement(ElementType.Category, "Heroes / Герои", false); _spcat.AddElement(_heroes); }
                var heroes = ObjectManager.GetEntities<Hero>().Where(p => !p.IsIllusion && p.Team != _myHero.Team).ToList();
                foreach (var enemy in heroes)
                {
                    if (UiHeroes.Contains(enemy)) continue;
                    var hero = new EzElement(ElementType.Category, enemy.Name.Replace("_", "").Replace("npcdotahero", ""), false);
                    foreach (var ability in enemy.Spellbook.Spells)
                    {
                        if (ability.AbilityBehavior == AbilityBehavior.Passive || ability.AbilityType == AbilityType.Attribute) continue;
                        var ac = false;
                        if (ability.AbilityType == AbilityType.Ultimate) { ac = true; IncludedAbilities.Add(ability.Name); }
                        hero.AddElement(new EzElement(ElementType.Checkbox, ability.Name, ac));
                    }
                    _heroes.AddElement(hero);
                    UiHeroes.Add(enemy);
                }
                Utils.Sleep(2000, "uiheroesupdate");
            }
            #endregion
            foreach (var enemy in enemies)
            {
                if (!Utils.SleepCheck(enemy.ClassId.ToString())) continue;
                foreach (var ability in enemy.Spellbook.Spells)
                {
                    if ( LastSpellIsChecked.Contains(enemy) && ( IsCasted(ability) && !IncludedAbilities.Contains(ability.Name) ) && _lastSpell.IsActive ) LastSpellIsChecked.Remove(enemy);
                    if (!IncludedAbilities.Contains(ability.Name) ||
                        !IsCasted(ability) && !LastSpellIsChecked.Contains(enemy) ||
                        SpellOnCooldown(ability.Name) || !CanSteal(enemy) ||
                        _myHero.Spellbook.SpellD.Name == ability.Name || ability.CooldownLength.Equals(0)) continue;
                    if (_stealIfHave.IsActive == false && _myHero.Spellbook.SpellD.Cooldown.Equals(0) && IncludedAbilities.Contains(_myHero.Spellbook.SpellD.Name)) continue;
                    if (_spellSteal.CanBeCasted()) _spellSteal.UseAbility(enemy);
                    else if (_lastSpell.IsActive) LastSpellIsChecked.Add(enemy);
                }
                Utils.Sleep(125, enemy.ClassId.ToString());
            }
        }

        #endregion

        #region Methods

        private static bool IsCasted(Ability ability)
        {
            return (ability.CooldownLength - ability.Cooldown < (float)0.7 + (Game.Ping / 1000));
        }

        private static bool CanSteal(Entity hero)
        {
            if (_myHero.AghanimState())
            {
                if (_myHero.Distance2D(hero) <= CastRange[1]) return true;
            }
            else if (_myHero.AghanimState() == false)
            {
                if (_myHero.Distance2D(hero) <= CastRange[0]) return true;
            }
            return false;
        }

        private static bool SpellOnCooldown(string abilityName)
        {
            if (AbilitiesFix.ContainsKey(abilityName)) abilityName = AbilitiesFix[abilityName];
            var spells = _myHero.Spellbook.Spells.ToArray();
            var spellsF = spells.Where(x => x.Name == abilityName).ToArray();
            if (spellsF.Length <= 0) return false;
            var spellF = spellsF.First();
            return spellF.Cooldown > 10;
        }

        #endregion
    }
}