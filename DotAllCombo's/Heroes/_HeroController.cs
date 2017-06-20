using System.Security.Permissions;

namespace DotaAllCombo.Heroes
{
    using Ensage;
    using Ensage.Common.Menu;
    using Ensage.Common.Objects.UtilityObjects;
    using Ensage.Common.Extensions;
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    public class Variables
	{
		protected Hero e;
		public Hero me;
		public Menu Menu;
        public bool Active, CastW, CastE, Push, CastQ;
        public  MultiSleeper spellSleeper;
        public Sleeper comboSleep;
        public int GetAbilityDelay(Unit target, Ability ability)
        {
            return (int)((ability.FindCastPoint() + me.GetTurnTime(target)) * 1000.0 + Game.Ping);
        }
    }

	interface IHeroController
	{
		void Combo();
		void OnLoadEvent();
		void OnCloseEvent();
	}
}
