using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace AllowAnything
{
	public class AllowAnythingMapComponent : MapComponent
	{
		const int k_ticks_threshold = 1234;
		int _ticks = 0;

		/// <remarks> A HashSet to list items that were allowed already so we don't do that again. </remarks>
		HashSet<Thing> _allowedAlready;

		AllowAnythingModSettings _settings = null;
		TickManager _tickManager = null;

		public AllowAnythingMapComponent ( Map map )
			: base(map)
		{
			_settings = LoadedModManager
				.GetMod<AllowAnythingMod>()
				.GetSettings<AllowAnythingModSettings>();
			_allowedAlready = new HashSet<Thing>();
			_tickManager = Find.TickManager;
		}

		public override void MapComponentTick ()
		{
			if( ++_ticks==k_ticks_threshold )
			{
				Task.Run( AllowAllHaulableThings );
				_ticks = 0;
			}
		}

		/// <remarks> List is NOT thread-safe so EXPECT it can be changed by diffent CPU thread, mid-execution, anytime here.</remarks>
		void AllowAllHaulableThings ()
		{
			Faction playerFaction = Faction.OfPlayer;
			bool allow = _settings.allow;
			bool notify = _settings.notify;
			
			var list = map.listerThings.ThingsInGroup( ThingRequestGroup.HaulableEver );
			for( int i=list.Count-1 ; i!=-1 ; i-- )
			{
				if(
						( list[i] is Thing thing )
					&&	thing.IsForbidden(playerFaction)
					&&	!thing.Fogged()
				)
				{
					if( !_allowedAlready.Contains(thing) )
					{
						_allowedAlready.Add(thing);

						if( allow )
							thing.SetForbidden( false );

						if( notify )
							Messages.Message( text:$"Allowed: {thing.LabelShort}" , lookTargets:thing , def:MessageTypeDefOf.NeutralEvent );
					}
				}
			}
		}

		public override void ExposeData ()
		{
			Scribe_Collections.Look( ref _allowedAlready , false , nameof(_allowedAlready) , LookMode.Reference );
			base.ExposeData();
		}

	}
}
