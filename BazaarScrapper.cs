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

        public override void SetupBazaar()
        {
            if(ModConfig.ScrapperSectionEnabled.Value)
            {
                SpawnScrapper();
            }
        }
        private void SpawnScrapper()
        {
            if (ModConfig.ScrapperAmount.Value > 0)
            {
                DicScrapers.Clear();
                SetScraper();
                DoSpawnCard(DicScrapers, iscScrapper, ModConfig.ScrapperAmount.Value);
            }
        }

        private void SetScraper()
        {
            List<int> total = new List<int> { 0, 1, 2, 3 };
            List<int> random = new List<int>();
            while (total.Count > 0)
            {
                int index = RNG.Next(total.Count);
                random.Add(total[index]);
                total.RemoveAt(index);
            }
            DicScrapers.Add(random[0], new SpawnCardStruct(new Vector3(-95.0f, -25.5f, -45.0f), new Vector3(0.0f, 72.0f, 0.0f)));
            DicScrapers.Add(random[1], new SpawnCardStruct(new Vector3(-100.0f, -25.0f, -40.0f), new Vector3(0.0f, 72.0f, 0.0f)));
            DicScrapers.Add(random[2], new SpawnCardStruct(new Vector3(-90.0f, -25.5f, -40.0f), new Vector3(0.0f, 72.0f, 0.0f)));
            DicScrapers.Add(random[3], new SpawnCardStruct(new Vector3(-95.0f, -25.5f, -35.0f), new Vector3(0.0f, 72.0f, 0.0f)));
            //DicScrapers.Add(random[2], new SpawnCardStruct(new Vector3(-105f, -26.0f, -35.0f), new Vector3(0.0f, 72.0f, 0.0f)));
            //DicScrapers.Add(random[5], new SpawnCardInteface(new Vector3(-100.0f, -26.5f, -30.0f), new Vector3(0.0f, 72.0f, 0.0f)));
        }

    }
}
