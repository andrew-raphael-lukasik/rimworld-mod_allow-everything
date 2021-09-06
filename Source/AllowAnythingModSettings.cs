using System.Collections.Generic;
using Verse;

namespace AllowAnything
{
	public class AllowAnythingModSettings : ModSettings
	{
		
		public bool allow = true;
		public bool notify = false;
		// public string thingRequestGroups = k_default_groups;

		// const string k_default_groups = nameof(ThingRequestGroup.Weapon) + "," + nameof(ThingRequestGroup.Apparel) + "," + nameof(ThingRequestGroup.Medicine) + "," + nameof(ThingRequestGroup.Corpse);
		
		// internal List<ThingRequestGroup> thingRequestGroupsParsed;

		public override void ExposeData ()
		{
			Scribe_Values.Look( ref allow , nameof(allow) , defaultValue:true );
			Scribe_Values.Look( ref notify , nameof(notify) , defaultValue:false );
			// Scribe_Values.Look( ref thingRequestGroups , nameof(thingRequestGroups) , defaultValue:k_default_groups );
			base.ExposeData();
		}

	}
}
