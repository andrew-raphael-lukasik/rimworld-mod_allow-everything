using UnityEngine;
using Verse;
using RimWorld;

namespace AllowEverything
{
	public class AllowEverythingMod : Mod
	{

		AllowEverythingModSettings _settings;

		public AllowEverythingMod ( ModContentPack content )
			: base( content )
			=> this._settings = GetSettings<AllowEverythingModSettings>();

		public override string SettingsCategory () => "Allow Everything";

		public override void DoSettingsWindowContents ( Rect rect )
		{
			var listing = new Listing_Standard();
			listing.Begin(rect);
			{
				listing.CheckboxLabeled( "Allow" , ref _settings.allow , "Is auto-allow enabled?" );
				listing.CheckboxLabeled( "Notify" , ref _settings.notify , "Are notifications enabled?" );
			}
			listing.End();
			base.DoSettingsWindowContents(rect);
		}

	}
}
