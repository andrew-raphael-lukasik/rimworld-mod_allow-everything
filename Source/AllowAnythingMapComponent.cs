using System;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AllowAnything
{
	public class AllowAnythingMapComponent : MapComponent
	{
		const int k_ticks_threshold = 1000;
		int _ticks = 0;

		/// <remarks> A Ring buffer to remember which corpses were allowed already so we don't do that again (in case player forbidden something again). </remarks>
		RingBufferInt16 _allowedAlready = null;

		AllowAnythingModSettings _settings = null;
		TickManager _tickManager = null;

		public AllowAnythingMapComponent ( Map map )
			: base(map)
		{
			_settings = LoadedModManager
				.GetMod<AllowAnythingMod>()
				.GetSettings<AllowAnythingModSettings>();
			_allowedAlready = new RingBufferInt16( length:256 );
			_tickManager = Find.TickManager;
		}

		public override void MapComponentTick ()
		{
			if( ++_ticks==k_ticks_threshold )
			{
				Task.Run( Routine );
				_ticks = 0;
			}
		}

		/// <remarks> List is NOT thread-safe so EXPECT it can be changed by diffent CPU thread, mid-execution, anytime here.</remarks>
		void Routine ()
		{
			var playerFaction = Faction.OfPlayer;
			int ticksGame = _tickManager.TicksGame;
			
			if( _settings.thingRequestGroupsParsed==null )
				_settings.thingRequestGroupsParsed = AllowAnythingMod.StringToEnumList<ThingRequestGroup>( _settings.thingRequestGroups );
			
			for( int igroup=0 ; igroup<_settings.thingRequestGroupsParsed.Count ; igroup++ )
			{
				ThingRequestGroup group = _settings.thingRequestGroupsParsed[igroup];
				var list = map.listerThings.ThingsInGroup( group );
				for( int i=0 ; i<list.Count ; i++ )
				{
					if( ( list[i] is Thing thing ) && thing.IsForbidden(playerFaction) )
					{
						Int16 hash = (Int16)( thing.GetHashCode() % Int16.MaxValue );
						if( !_allowedAlready.Contains(hash) )
						{ 
							_allowedAlready.Push( hash );

							if( _settings.allow )
								thing.SetForbidden( false );

							if( _settings.notify )
								Messages.Message( text:$"Allowed: {thing.LabelShort}\t(part of {group} group)" , lookTargets:thing , def:MessageTypeDefOf.NeutralEvent );
						}
					}
				}
			}
		}

	}
}
