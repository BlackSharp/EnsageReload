using Ensage.Common.Menu;

namespace SkyWrathReload
{
    internal class Options : Variables
    {
        public static void MenuInit()
        {
            HeroName = "npc_dota_hero_skywrath_mage";
            Menu = new Menu(AssemblyName, AssemblyName, true, HeroName, true);
            ComboKey = new MenuItem("comboKey", "Combo Key").SetValue(new KeyBind(70, KeyBindType.Press));
            HarassKey = new MenuItem("harassKey", "Harass Key").SetValue(new KeyBind(68, KeyBindType.Press));
            useBlink = new MenuItem("useBlink", "Use Blink Dagger").SetValue(false).SetTooltip("Auto-blink to target when Combo key is pressed.");
            SoulRing = new MenuItem("soulRing", "Soulring").SetValue(true).SetTooltip("Use soulring before use the combo if your HP is greater than 150.");
            BladeMail = new MenuItem("bladeMail", "Check for BladeMail").SetValue(false);
            drawTarget = new MenuItem("drawTarget", "Target indicator").SetValue(true);
            MoveMode = new MenuItem("moveMode", "Orbwalk").SetValue(true);
            PredictionType = new MenuItem("predictionType", "Ultimate prediction").SetValue(new StringList(new[] { "InFront", "By MS/Direction"}));
            EzKillCheck = new MenuItem("ezKillCheck", "Check for EZ Kill").SetValue(true).SetTooltip("Check if an enemy is ez-killable (low-mana costs and the fastest way to slay an enemy).");
            EzKillStyle = new MenuItem("ezKillIndicator", "Indicator style").SetValue(new StringList(new[] {"Icon", "Text"}));
            StraightTimeCheck = new MenuItem("straightTimeCheck", "Straight time before ulti").SetValue(new Slider(0, 0, 2)).SetTooltip("At least time enemy's moving in straight before casting ulti.");
            ClosestToMouseRange = new MenuItem("ClosestToMouseRange", "Closest to mouse range").SetValue(new Slider(600, 500, 1200)).SetTooltip("Range that makes assembly checking for enemy in selected range.");
            Nocastulti = new MenuItem("noCastUlti", "Do not use ulti if % of enemy's HP is below: ").SetValue(new Slider(35));


            NoCastUlti = new Menu("Ultimate usage", "Ultimate usage");
            MagicItems = new Menu("Magic Damage Items", "Magic Damage Items");
            PopLinkensItems = new Menu("Pop Linkens Items", "Pop Linkens Items");
            abilities = new Menu("Abilities", "Abilities");
            Ezkillmenu = new Menu("EZkill Menu", "ezkillmenu");
            
            Menu.AddItem(ComboKey);
            Menu.AddItem(HarassKey);
            Menu.AddItem(useBlink);
            Menu.AddItem(SoulRing);
            Menu.AddItem(BladeMail);
            Menu.AddItem(drawTarget);
            Menu.AddItem(MoveMode);
            Menu.AddItem(PredictionType);
            Menu.AddItem(StraightTimeCheck);
            Menu.AddItem(ClosestToMouseRange);

            Menu.AddSubMenu(MagicItems);
            Menu.AddSubMenu(PopLinkensItems);
            Menu.AddSubMenu(abilities);
            Menu.AddSubMenu(NoCastUlti);
            Menu.AddSubMenu(Ezkillmenu);

            MagicItems.AddItem(
                new MenuItem("magicItems", "Magic Damage").SetValue(
                    new AbilityToggler(MagicItemsDictionary)));
            PopLinkensItems.AddItem(
                new MenuItem("popLinkensItems", "Pop Linken's Items").SetValue(
                    new AbilityToggler(PopLinkensDictionary)));
            abilities.AddItem(new MenuItem("abilities", "Abilities").SetValue(
                new AbilityToggler(AbilitiesDictionary)));

            NoCastUlti.AddItem(Nocastulti);

            Ezkillmenu.AddItem(EzKillCheck);
            Ezkillmenu.AddItem(EzKillStyle);

            Menu.AddToMainMenu();
        }

    }
}