using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using Verse;
using RimWorld;

using Mathf = UnityEngine.Mathf;

namespace AllowEverything
{
    public class AllowEverythingMapComponent : MapComponent
    {
        const int k_num_ticks_delay = 1234;// jobs are scheduled once every this num of ticks
        const int k_items_per_job = 1024;
        int _ticks = 0;
        AllowEverythingModSettings _settings = null;
        TickManager _tickManager = null;
        HashSet<Thing> _allowedAlready;// list items that were allowed already so we don't do that again
        HashSet<Thing> _notifiedAlready;// list items that were notified already so we don't do that again
        ConcurrentBag<Thing> _thingsToAllow;// safe-thread container to get data back from job threads
        ConcurrentBag<Thing> _thingsToNotify;// safe-thread container to get data back from job threads

        public AllowEverythingMapComponent(Map map)
            : base(map)
        {
            _settings = LoadedModManager
                .GetMod<AllowEverythingMod>()
                .GetSettings<AllowEverythingModSettings>();
            _allowedAlready = new HashSet<Thing>();
            _notifiedAlready = new HashSet<Thing>();
            _thingsToAllow = new ConcurrentBag<Thing>();
            _thingsToNotify = new ConcurrentBag<Thing>();
            _tickManager = Find.TickManager;
        }

        public override void MapComponentTick()
        {
            while (_thingsToAllow.TryTake(out Thing thing))
            if (thing != null && !thing.Destroyed)
            {
                _allowedAlready.Add(thing);
                thing.SetForbidden(value: false, warnOnFail: false);
            }

            while (_thingsToNotify.TryTake(out Thing thing))
            if (thing != null && !thing.Destroyed)
            {
                _notifiedAlready.Add(thing);
                Messages.Message(text: $"Spotted: {thing.LabelShort}", lookTargets: thing, def: MessageTypeDefOf.NeutralEvent);
            }

            if (++_ticks == k_num_ticks_delay)
            {
                if (_settings.allowByCategory.Count() + _settings.notifyByCategory.Count() > 0)
                {
                    var haulables = map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
                    int numHaulablesTotal = haulables.Count;
                    int numHaulablesLeft = numHaulablesTotal;
                    int jobCounter = 0;
                    for (int i = 0; i < numHaulablesTotal; i += k_items_per_job)
                    {
                        var job = new SearchJob
                        {
                            haulables = haulables,
                            playerFaction = Faction.OfPlayer,
                            ticksGame = _tickManager.TicksGame,
                            allowedAlready = _allowedAlready,
                            notifiedAlready = _notifiedAlready,
                            categoriesToAllow = _settings.allowByCategory,
                            categoriesToNotify = _settings.notifyByCategory,
                            thingsToAllow = _thingsToAllow,
                            thingsToNotify = _thingsToNotify,

                            startIndex = i,
                            count = Mathf.Min(k_items_per_job, numHaulablesLeft)
                        };
                        Task.Run(job.Execute);
                        jobCounter++;
                        numHaulablesLeft -= job.count;
                    }
                }

                _ticks = 0;
            }
        }

        public override void ExposeData()
        {
            _allowedAlready.RemoveWhere((o) => o == null);
            _notifiedAlready.RemoveWhere((o) => o == null);
            Scribe_Collections.Look(ref _allowedAlready, false, nameof(_allowedAlready), LookMode.Reference);
            Scribe_Collections.Look(ref _notifiedAlready, false, nameof(_notifiedAlready), LookMode.Reference);
            base.ExposeData();
        }

        static bool IsCategorySet(in ThingDef thingDef, in HashSet<string> enabledDefs)
        {
            if (thingDef.thingCategories == null) return false;

            foreach (var cat in thingDef.thingCategories)
            {
                var current = cat;
                while (current != null)
                {
                    if (enabledDefs.Contains(current.defName))
                        return true;
                    current = current.parent;
                }
            }

            return false;
        }

        struct SearchJob
        {
            public List<Thing> haulables;
            public Faction playerFaction;
            public int ticksGame;
            public HashSet<Thing> allowedAlready;// read only
            public HashSet<Thing> notifiedAlready;// read only
            public HashSet<string> categoriesToAllow;// read only
            public HashSet<string> categoriesToNotify;// read only
            public ConcurrentBag<Thing> thingsToAllow;
            public ConcurrentBag<Thing> thingsToNotify;

            public int startIndex, count;

            public void Execute()
            {
                for (int i = startIndex; i < startIndex + count; i++)
                if (
                        (haulables[i] is Thing thing)
                    &&  thing.IsForbidden(playerFaction)
                    &&  !thing.Fogged()
                    &&  (!(thing is Corpse corpse) || ticksGame - corpse.timeOfDeath > k_num_ticks_delay * 2)// fixes: corpses being eaten by predators losing it's allowed status
                )
                {
                    if (IsCategorySet(thing.def, categoriesToAllow) && !allowedAlready.Contains(thing))
                        thingsToAllow.Add(thing);

                    if (IsCategorySet(thing.def, categoriesToNotify) && !notifiedAlready.Contains(thing))
                        thingsToNotify.Add(thing);
                }
            }
        }

    }
}
