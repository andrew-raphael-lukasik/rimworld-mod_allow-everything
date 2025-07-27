using Verse;

namespace AllowEverything
{
    public class AllowEverythingModSettings : ModSettings
    {
        
        public bool allow = true;
        public bool notify = false;

        public override void ExposeData ()
        {
            Scribe_Values.Look( ref allow , nameof(allow) , defaultValue:true );
            Scribe_Values.Look( ref notify , nameof(notify) , defaultValue:false );
            base.ExposeData();
        }

    }
}
