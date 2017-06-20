namespace DotaAllCombo.Service
{
    using System.Security.Permissions;
    using Addons;
    [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
    class AddonsManager
    {
        public static bool IsLoaded { get; private set; }
		//private static AutoDodge autoDodge;
		private static AutoStack autoStack;
		private static CreepControl creepControl;
        private static LastHit lastHit;
        private static OthersAddons othersAddons;
		

		public static void Load()
		{

            autoStack = new AutoStack();
            creepControl = new CreepControl();
            lastHit = new LastHit();
			othersAddons = new OthersAddons();

            lastHit.Load();
            autoStack.Load();
            creepControl.Load();
			//autoDodge.Load();
			othersAddons.Load();
			IsLoaded = true;
		}
        public static void RunAddons()
        {
            if (!IsLoaded) return;

            /*
            Thread autoStackThread = new Thread(autoStack.RunScript);
            autoStackThread.Priority = ThreadPriority.BelowNormal;

            Thread lastHitThread = new Thread(lastHit.RunScript);
            lastHitThread.Priority = ThreadPriority.AboveNormal;

            Thread creepControlThread = new Thread(creepControl.RunScript);
            creepControlThread.Priority = ThreadPriority.BelowNormal;

            Thread othersAddonsThread = new Thread(othersAddons.RunScript);
            othersAddonsThread.Priority = ThreadPriority.Lowest;


            autoStackThread.Start();
            lastHitThread.Start();
            creepControlThread.Start();
            othersAddonsThread.Start();
            
            */
            autoStack.RunScript();
            creepControl.RunScript();
            //autoDodge.RunScript();
            lastHit.RunScript();
            othersAddons.RunScript();

        }
        public static void Unload()
        {
            lastHit.Unload();
            othersAddons.Unload();
			//autoDodge.Unload();
			autoStack.Unload();
			creepControl.Unload();
			IsLoaded = false;
		}
	}
}
