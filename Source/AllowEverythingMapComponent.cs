using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Verse;
using RimWorld;
using Mathf = UnityEngine.Mathf;

namespace AllowEverything
{
	public class AllowEverythingMapComponent : MapComponent
	{
		const int k_num_ticks_delay = 1234;// jobs are scheduled once every this num of ticks
		const int k_items_per_job = 512;
		int _ticks = 0;

		/// <remarks> A HashSet to list items that were allowed already so we don't do that again. </remarks>
		HashSet<Thing> _allowedAlready;
		ConcurrentBag<Thing> _thingsToAllow;// safe-thread container to get data back from job threads

		AllowEverythingModSettings _settings = null;
		TickManager _tickManager = null;

		public AllowEverythingMapComponent ( Map map )
			: base(map)
		{
			_settings = LoadedModManager
				.GetMod<AllowEverythingMod>()
				.GetSettings<AllowEverythingModSettings>();
			_allowedAlready = new HashSet<Thing>();
			_thingsToAllow = new ConcurrentBag<Thing>();
			_tickManager = Find.TickManager;
		}

		public override void MapComponentTick ()
		{
			while( _thingsToAllow.TryTake( out Thing thing ) )
			if( thing!=null && !thing.Destroyed )
			{
				_allowedAlready.Add( thing );

				if( _settings.allow )
					thing.SetForbidden( value:false , warnOnFail:false );

				if( _settings.notify )
					Messages.Message( text:$"Allowed: {thing.LabelShort}" , lookTargets:thing , def:MessageTypeDefOf.NeutralEvent );
			}

			if( ++_ticks==k_num_ticks_delay )
			{
				var haulables = map.listerThings.ThingsInGroup( ThingRequestGroup.HaulableEver );
				int numHaulablesTotal = haulables.Count;
				int numHaulablesLeft = numHaulablesTotal;
				int jobCounter = 0;
				for( int i=0 ; i<numHaulablesTotal ; i+=k_items_per_job )
				{
					var job = new AllowJob{
						haulables = haulables,
						playerFaction = Faction.OfPlayer,
						ticksGame = _tickManager.TicksGame,
						allowedAlready = _allowedAlready,
						thingsToAllow = _thingsToAllow,

						startIndex = i,
						count = Mathf.Min( k_items_per_job , numHaulablesLeft )
					};
					Task.Run( job.Execute );
					jobCounter++;
					numHaulablesLeft -= job.count;
				}

				_ticks = 0;
			}
		}

		public override void ExposeData ()
		{
			_allowedAlready.RemoveWhere( (o) => o==null );
			Scribe_Collections.Look( ref _allowedAlready , false , nameof(_allowedAlready) , LookMode.Reference );
			base.ExposeData();
		}

		struct AllowJob
		{
			public List<Thing> haulables;
			public Faction playerFaction;
			public int ticksGame;
			public HashSet<Thing> allowedAlready;// read only
			public ConcurrentBag<Thing> thingsToAllow;// thread safe

			public int startIndex, count;

            public void Execute ()
            {
                for( int i=startIndex ; i<startIndex+count ; i++ )
				{
					if(
							( haulables[i] is Thing thing )
						&&	thing.IsForbidden(playerFaction)
						&&	!thing.Fogged()
						&&	( !(thing is Corpse corpse) || ticksGame-corpse.timeOfDeath>k_num_ticks_delay*2 )// fixes: corpses being eaten by predators losing it's allowed status
						&&	!allowedAlready.Contains(thing)
					)
					{
						thingsToAllow.Add( thing );
					}
				}
            }
		}

	}
}
