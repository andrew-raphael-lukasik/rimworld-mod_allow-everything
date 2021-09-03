using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace AllowAnything
{
	public class AllowAnythingMod : Mod
	{

		AllowAnythingModSettings _settings;

		public AllowAnythingMod ( ModContentPack content )
			: base( content )
			=> this._settings = GetSettings<AllowAnythingModSettings>();

		public override string SettingsCategory () => "Allow Anything";

		Vector2 _scrollPos;

		public override void DoSettingsWindowContents ( Rect rect )
		{
			Listing_Standard listing = new Listing_Standard();
			listing.Begin(rect);
			{
				listing.CheckboxLabeled( "Allow" , ref _settings.allow , "Is auto-allow enabled?" );
				listing.CheckboxLabeled( "Notify" , ref _settings.notify , "Are notifications enabled?" );

				ThingRequestGroup[] allThingRequestGroups = (ThingRequestGroup[]) Enum.GetValues( typeof(ThingRequestGroup) );

				listing.GapLine();
				
				listing.Label( "Groups" );
				listing.Indent();
				listing.Label( _settings.thingRequestGroups );
				if( listing.ButtonText("Clear") )
				{
					_settings.thingRequestGroups = "";
					_settings.thingRequestGroupsParsed = new List<ThingRequestGroup>();
				}
				listing.Outdent();

				listing.Indent();
				int height = 400;
				var section = listing.BeginSection(height);
				{
					_scrollPos = GUILayout.BeginScrollView( _scrollPos , GUILayout.Width(section.ColumnWidth-24) , GUILayout.Height(400) );
					{
						var arr = (ThingRequestGroup[])Enum.GetValues(typeof(ThingRequestGroup));
						for( int i=0 ; i<arr.Length ; i++ )
						{
							ThingRequestGroup group = arr[i];
							if(
								// let's ignore the most confusing and useless entries
									group!=ThingRequestGroup.ActionDelay
								&&	group!=ThingRequestGroup.ActiveDropPod
								&&	group!=ThingRequestGroup.AffectedByFacilities
								&&	group!=ThingRequestGroup.AffectsSky
								&&	group!=ThingRequestGroup.AlwaysFlee
								&&	group!=ThingRequestGroup.AttackTarget
								&&	group!=ThingRequestGroup.Bed
								&&	group!=ThingRequestGroup.Blueprint
								&&	group!=ThingRequestGroup.BuildingArtificial
								&&	group!=ThingRequestGroup.BuildingFrame
								&&	group!=ThingRequestGroup.Chunk
								&&	group!=ThingRequestGroup.ConditionCauser
								&&	group!=ThingRequestGroup.Construction
								&&	group!=ThingRequestGroup.CreatesInfestations
								&&	group!=ThingRequestGroup.DryadSpawner
								&&	group!=ThingRequestGroup.Everything
								// &&	group!=ThingRequestGroup.Facility// what are these?
								&&	group!=ThingRequestGroup.Filth
								&&	group!=ThingRequestGroup.Fire
								&&	group!=ThingRequestGroup.FoodDispenser
								&&	group!=ThingRequestGroup.FoodSource
								&&	group!=ThingRequestGroup.Grave
								&&	group!=ThingRequestGroup.HarvestablePlant
								&&	group!=ThingRequestGroup.HasGUIOverlay
								&&	group!=ThingRequestGroup.LongRangeMineralScanner
								&&	group!=ThingRequestGroup.MeditationFocus
								&&	group!=ThingRequestGroup.MusicSource
								&&	group!=ThingRequestGroup.Nothing
								&&	group!=ThingRequestGroup.Pawn
								&&	group!=ThingRequestGroup.Plant
								&&	group!=ThingRequestGroup.PotentialBillGiver
								&&	group!=ThingRequestGroup.Projectile
								&&	group!=ThingRequestGroup.ProjectileInterceptor
								&&	group!=ThingRequestGroup.Refuelable
								&&	group!=ThingRequestGroup.ResearchBench
								&&	group!=ThingRequestGroup.Studiable
								&&	group!=ThingRequestGroup.ThingHolder
								&&	group!=ThingRequestGroup.Throne
								&&	group!=ThingRequestGroup.Transporter
								&&	group!=ThingRequestGroup.WindSource
								&&	group!=ThingRequestGroup.WithCustomRectForSelector
							)
							{
								if( GUILayout.Button(group.ToString()) )
									ToggleThingRequestGroup(group);
							}
						}
					}
					GUILayout.EndScrollView();
				}
				listing.EndSection( section );
				listing.Outdent();
			}
			listing.End();
			base.DoSettingsWindowContents(rect);
		}

		void ToggleThingRequestGroup ( ThingRequestGroup group )
		{
			var groups = StringToEnumList<ThingRequestGroup>( _settings.thingRequestGroups );
			if( groups.RemoveAll(next=>next==group)==0 ) groups.Add(group);
			
			_settings.thingRequestGroups = EnumListToString( groups );
			_settings.thingRequestGroupsParsed = groups;
		}

		public static List<T> StringToEnumList <T> ( string input ) where T : unmanaged, Enum
		{
			var groups = new List<T>(3);
			{
				string[] arr = input.Split(',');
				T parsed;
				for( int i=0 ; i<arr.Length ; i++ )
					if( Enum.TryParse<T>( arr[i] , out parsed ) )
						groups.Add( parsed );
			}
			return groups;
		}
		
		public static string EnumListToString <T> ( List<T> list ) where T : unmanaged, Enum
		{
			var sb = new System.Text.StringBuilder();
			int len = list.Count;
			if( len!=0 )
			{
				sb.AppendFormat( "{0}" , list[0] );
				for( int i=1 ; i<len ; i++ ) sb.AppendFormat( ",{0}" , list[i] );
			}
			return sb.ToString();
		}

	}
}
