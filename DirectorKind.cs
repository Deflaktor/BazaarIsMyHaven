using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BazaarIsMyHaven
{
    // One kind of interactable a card can roll, e.g. a printer tier or a cauldron type; other mods add their own through BazaarAPI
    internal class DirectorKind
    {
        public string name;
        public Func<int> cost;
        public Func<float> weight;
        public Func<bool> isAvailable;
        public Action<SpawnCardStruct> spawn;

        // The kinds with a weight that are available on this visit; one of another mod that throws is left out
        public static List<DirectorKind> Available(IEnumerable<DirectorKind> kinds)
        {
            var available = new List<DirectorKind>();
            foreach (var kind in kinds)
            {
                try
                {
                    if (kind.weight() > 0f && (kind.isAvailable == null || kind.isAvailable()))
                        available.Add(kind);
                }
                catch (Exception exception)
                {
                    Log.LogError($"Could not check {kind.name}: {exception}");
                }
            }
            return available;
        }
    }

    // Every pick rolls one of the kinds the director can afford, by weight, since kinds can cost differently
    internal class KindCard : DirectorCard
    {
        private readonly List<DirectorKind> kinds;
        public readonly List<DirectorKind> picks = new List<DirectorKind>();

        public KindCard(string name, List<DirectorKind> kinds, Func<float> weight, Func<int> maxCount) : base(name, (Func<int>)null, weight, maxCount)
        {
            this.kinds = kinds;
        }

        protected override int CheapestCost()
        {
            return kinds.Min(kind => Math.Max(0, kind.cost()));
        }

        protected override int PickCost(int credits)
        {
            WeightedSelection<DirectorKind> weightedSelection = new WeightedSelection<DirectorKind>();
            foreach (var kind in kinds)
            {
                if (Math.Max(0, kind.cost()) <= credits)
                    weightedSelection.AddChoice(kind, kind.weight());
            }
            DirectorKind pick = weightedSelection.Evaluate(UnityEngine.Random.value);
            picks.Add(pick);
            return Math.Max(0, pick.cost());
        }
    }
}
