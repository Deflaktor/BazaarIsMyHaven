using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace BazaarIsMyHaven
{
    // One kind of interactable the director can buy, like a card of the game's scene director
    public class DirectorCard
    {
        public readonly string name;
        // late picks are made in the final pass, after the section spawned its first picks
        public readonly bool allowLatePicks;
        public int Count { get; private set; }
        public int Spent { get; private set; }

        private readonly ConfigEntry<int> cost;
        private readonly ConfigEntry<float> weight;
        private readonly Func<int> maxCount;
        private readonly Func<bool> isAvailable;

        public DirectorCard(string name, ConfigEntry<int> cost, ConfigEntry<float> weight, Func<int> maxCount, Func<bool> isAvailable = null, bool allowLatePicks = true)
        {
            this.name = name;
            this.cost = cost;
            this.weight = weight;
            this.maxCount = maxCount;
            this.isAvailable = isAvailable;
            this.allowLatePicks = allowLatePicks;
        }

        public virtual float Weight => Mathf.Max(0f, weight.Value);

        public bool CanPick(int credits)
        {
            return Weight > 0f && Count < maxCount() && (isAvailable == null || isAvailable()) && CheapestCost() <= credits;
        }

        // Returns what the pick cost
        public int Pick(int credits)
        {
            int paid = PickCost(credits);
            Count++;
            Spent += paid;
            return paid;
        }

        protected virtual int CheapestCost()
        {
            return Math.Max(0, cost.Value);
        }

        // Chooses what this pick spawns; credits are at least CheapestCost()
        protected virtual int PickCost(int credits)
        {
            return CheapestCost();
        }
    }

    // Spends credits on interactables at random by weight, like the game's scene director.
    // Its final pass runs a frame after the Bazaar was set up, once the shops rolled their items.
    public class BazaarDirector : MonoBehaviour
    {
        private static readonly System.Random random = new System.Random();

        private List<BazaarBase> sections;
        private List<DirectorCard> cards;
        private int credits;

        public static int Credits
        {
            get
            {
                int credits = ModConfig.DirectorExtraCredits.Value + ModConfig.DirectorCreditsPerStage.Value * Run.instance.stageClearCount;
                return ModConfig.DirectorCreditsCap.Value >= 0 ? Math.Min(credits, ModConfig.DirectorCreditsCap.Value) : credits;
            }
        }

        // Returns the credits left over
        public static int Spend(int credits, IEnumerable<DirectorCard> cards, string round)
        {
            var candidates = cards.ToList();
            var picks = new Dictionary<DirectorCard, int>();
            int startCredits = credits;
            while (true)
            {
                var affordable = candidates.Where(card => card.CanPick(credits)).ToList();
                if (affordable.Count == 0)
                {
                    break;
                }

                double roll = random.NextDouble() * affordable.Sum(card => card.Weight);
                var chosen = affordable[affordable.Count - 1];
                foreach (var card in affordable)
                {
                    if (roll < card.Weight)
                    {
                        chosen = card;
                        break;
                    }
                    roll -= card.Weight;
                }
                credits -= chosen.Pick(credits);
                picks[chosen] = picks.GetValueOrDefault(chosen) + 1;
            }

            string bought = picks.Count == 0 ? "nothing" : string.Join(", ", picks.Select(pick => $"{pick.Value}x {pick.Key.name}"));
            Log.LogInfo($"Director ({round}): {startCredits} credits bought {bought}; {credits} left.");
            return credits;
        }

        public static void Create(List<BazaarBase> sections, List<DirectorCard> cards, int credits)
        {
            var director = new GameObject("BazaarDirector").AddComponent<BazaarDirector>();
            director.sections = sections;
            director.cards = cards;
            director.credits = credits;
        }

        private IEnumerator Start()
        {
            // shops roll their items in their own Start
            yield return null;
            if (!NetworkServer.active)
            {
                yield break;
            }

            int refund = 0;
            foreach (var section in sections)
            {
                try
                {
                    refund += section.CheckAfterSetup();
                }
                catch (Exception exception)
                {
                    Log.LogError($"Could not check {section.GetType().Name} after setup: {exception}");
                }
            }

            if (refund > 0)
            {
                Spend(credits + refund, cards.Where(card => card.allowLatePicks), "late");
                foreach (var section in sections)
                {
                    try
                    {
                        section.SpawnLatePicks();
                    }
                    catch (Exception exception)
                    {
                        Log.LogError($"Could not spawn the late picks of {section.GetType().Name}: {exception}");
                    }
                }
            }
            Destroy(gameObject);
        }
    }
}
