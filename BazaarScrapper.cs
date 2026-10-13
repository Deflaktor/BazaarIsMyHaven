using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace BazaarIsMyHaven
{
    public class BazaarScrapper : BazaarBase
    {
        AsyncOperationHandle<InteractableSpawnCard> iscScrapper;

        Dictionary<int, SpawnCardStruct> DicScrapers = new Dictionary<int, SpawnCardStruct>();
        DirectorCard card;
        bool spawned;
        public override void Preload()
        {
            iscScrapper = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/Base/Scrapper/iscScrapper.asset");
        }

        public override void Hook()
        {

        }
        public override void RunStart()
        {

        }

        public override void AddDirectorCards(List<DirectorCard> cards)
        {
            card = null;
            if (ModConfig.ScrapperSectionEnabled.Value)
            {
                // one pick spawns all the scrappers
                card = new DirectorCard("Scrappers", ModConfig.ScrapperDirectorCost, ModConfig.ScrapperDirectorWeight, () => ModConfig.ScrapperMaxAmount.Value > 0 ? 1 : 0);
                cards.Add(card);
            }
        }

        public override void SetupBazaar()
        {
            spawned = false;
            SpawnLatePicks();
        }

        public override void SpawnLatePicks()
        {
            if (card == null || card.Count == 0 || spawned)
                return;
            DicScrapers.Clear();
            SetScraper();
            SpawnInSlots(DicScrapers, iscScrapper.WaitForCompletion().prefab, 0, ModConfig.ScrapperMaxAmount.Value);
            spawned = true;
        }

        private void SetScraper()
        {
            List<int> total = new List<int> { 0, 1 };
            List<int> random = new List<int>();
            while (total.Count > 0)
            {
                int index = RNG.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicScrapers.Add(random[0], new SpawnCardStruct(new Vector3(-100.0f, -25.0f, -40.0f), new Vector3(0.0f, 72.0f, 0.0f)));
            DicScrapers.Add(random[1], new SpawnCardStruct(new Vector3(-95.0f, -25.5f, -35.0f), new Vector3(0.0f, 72.0f, 0.0f)));
        }

    }
}
