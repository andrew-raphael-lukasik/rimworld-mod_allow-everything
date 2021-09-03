using System.Collections.Generic;
using Verse;

namespace AllowAnything
{
	public class AllowAnythingModSettings : ModSettings
	{
		
		public bool allow = true;
		public bool notify = false;
		public string thingRequestGroups = nameof(ThingRequestGroup.HaulableEver);
		
		internal List<ThingRequestGroup> thingRequestGroupsParsed;

		public override void ExposeData ()
		{
			Scribe_Values.Look( ref allow , nameof(allow) , defaultValue:true );
			Scribe_Values.Look( ref notify , nameof(notify) , defaultValue:false );
			Scribe_Values.Look( ref thingRequestGroups , nameof(thingRequestGroups) , defaultValue:nameof(ThingRequestGroup.HaulableEver) );
			base.ExposeData();
		}

	}
}
