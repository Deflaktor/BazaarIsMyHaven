# BazaarIsMyHaven

**server-side mod** - only the host needs it installed.

This is a fork of Lunzir’s excellent [BazaarIsMyHaven](https://thunderstore.io/package/Lunzir2/BazaarIsMyHome/) mod.

## Features

- **Extra Interactables in the Bazaar** (configurable):
  - 3D Printers
  - Additional Cauldrons
  - Scrappers
  - Equipment Terminals
  - Lunar Shop (customizable)
  - Cleansing Pool
  - Shrine of Order
  - Donation Altar

- **Newt Behavior Options**:
  - Stop him from throwing you out.
  - Change what happens after you kill him.
  - Extra dialogue lines.

- **Portal Options**:
  - Spawn a Bazaar portal after *every* teleporter event.

- **Other Tweaks**:
  - Extra decoration in the Bazaar.
  - Settings to gradually increase interactables as you complete more stages during the run.

## Key Settings

In-detail descriptions for some of the settings:

### General - SpawnCountByStage

This settings makes it so that the more stage are completed, the more interactables are spawned in the Bazaar. If you just start the run and go immediately to the Bazaar you will see few interactables. But as you progress further, more and more interactables will get spawned. Up to the configured limit of each respective interactable. The `SpawnCountByStage` setting enables this behavior. There is also the `SpawnCountOffset` which allows you to either add a baseline amount of interactables or make interactables increase even later. Can be both positive or negative. The formula is a follows:

Formula: `Amount of Interactables per Type = Number of Stages Completed + SpawnCountOffset`

### Newt – DeathBehavior

Controls how Newt acts after being killed:

- `Default` → Normal behavior.  
- `Tank` → Newt Health is significantly reduced. Revives with double HP.
- `Ghost` → Newt Health is significantly reduced. Revives as a ghost.
- `Hostile` → Newt Health is significantly reduced. Revives and starts defending himself.

### LunarShop

You can freely configure which items can be bought at the LunarShop. There are two settings to configure this:

- `SequentialItems`:
  - **True** → Items are picked sequentially from the list. As such, if the number of Lunar Shop Terminals and the number of items in the list are the same, you will always find the same items in the Bazaar.
  - **False** → Items are picked at random from the list.

- `ItemList`:
  - A list of items to appear. Must be internal items names. Can use tier or droptables as well, see *Item Keywords* below.
  - Examples:
    - `Tier1, Tier2, Tier3, Lunar, Boss`: If `SequentialItems` is set to `True` and `Amount` to 5, then you will find exactly 1 white item, 1 green item, 1 red item, 1 lunar item and 1 boss item in the shop.
    - `dtLunarChest`: This is the vanilla behavior of the game.
    - `FreeChest, VoidTier1, dtChest2`: 1 Shipping Request Form, one random item of Void Tier 1 and one random item of the droptable of a large chest.

### Donation Altar

The Donate setting spawns a donation box near the Newt. After donating 10 times, the Newt will give you a reward. There are 3 item lists which are selected at random:

- `DonateRewardNormalList`: The common reward list. Contains normal items.
- `DonateRewardEliteList`: Contains the elite equipment items.
- `DonateRewardPeculiarList`: Disabled by default. Contains some unreleased or unfinished items. But can be fully customized.

See the below section "Item Keyword List" on what are valid values.
With the donate reward lists, it is possible to reward multiple items at the same time. Each keyword in the list needs to also have an "=" sign to denote how many of that items shall be given.

Examples:

`RewardNormalList = dtITDefaultWave=5`: The reward will be 5 random items of the droptable of void potentials from the Simulacrum mode.

`RewardNormalList = dtChest1=5, dtChest2=2`: The reward will be either 5 random items of the small chest droptable or 2 random items of the large chest droptable.

## Item Keywords

You can use:
- **Internal item names** (see [R2Wiki - Items-and-Equipments-Data](https://risk-of-thunder.github.io/R2Wiki/Mod-Creation/Developer-Reference/Items-and-Equipments-Data/)).  
- **Item Tier names**: `Tier1`, `Tier2`, `Tier3`, `Lunar`, `Boss`, `VoidTier1`, `VoidTier2`, `VoidTier3`, `VoidBoss`.
- **Droptable names**: e.g. `dtChest1`, `dtLunarChest`, `dtVoidChest`. See below.

### Droptable Names

Here is a list of supported droptables:

|                                 | canDropBeReplaced | requiredItemTags | bannedItemTags | tier1Weight | tier2Weight | tier3Weight | bossWeight | lunarEquipmentWeight | lunarItemWeight | lunarCombinedWeight | equipmentWeight | voidTier1Weight | voidTier2Weight | voidTier3Weight | voidBossWeight |
|---------------------------------|-------------------|------------------|----------------|-------------|-------------|-------------|------------|----------------------|-----------------|---------------------|-----------------|-----------------|-----------------|-----------------|----------------|
| dtMonsterTeamTier1Item | True |  | AIBlacklist, OnKillEffect, EquipmentRelated, SprintRelated | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtMonsterTeamTier2Item | True |  | AIBlacklist, OnKillEffect, EquipmentRelated, SprintRelated | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtMonsterTeamTier3Item | True |  | AIBlacklist, OnKillEffect, EquipmentRelated, SprintRelated | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtSacrificeArtifact | True |  | SacrificeBlacklist | 0.7 | 0.3 | 0.01 | 0 | 0 | 0 | 0 | 0.1 | 0 | 0 | 0 | 0 |
| dtAISafeTier1Item | True |  | AIBlacklist | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtAISafeTier2Item | True |  | AIBlacklist | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtAISafeTier3Item | True |  | AIBlacklist | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtEquipment | True |  |  | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 0 | 0 | 0 |
| dtTier1Item | True |  |  | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtTier2Item | True |  |  | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtTier3Item | True |  |  | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtVoidChest | True |  |  | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 6 | 3 | 1 | 0 |
| dtCasinoChest | True |  |  | 0.7 | 0.3 | 0.01 | 0 | 0 | 0 | 0 | 0.1 | 0 | 0 | 0 | 0 |
| dtSmallChestDamage | True | Damage |  | 0.8 | 0.2 | 0.01 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtSmallChestHealing | True | Healing |  | 0.8 | 0.2 | 0.01 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtSmallChestUtility | True | Utility |  | 0.8 | 0.2 | 0.01 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtChest1 | True |  |  | 0.8 | 0.2 | 0.01 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtChest2 | True |  |  | 0 | 0.8 | 0.2 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtDuplicatorTier1 | True |  | CannotDuplicate | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtDuplicatorTier2 | True |  | CannotDuplicate | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtDuplicatorTier3 | True |  | CannotDuplicate | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtDuplicatorWild | True |  | WorldUnique | 0 | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtGoldChest | True |  |  | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtLunarChest | True |  |  | 0 | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 0 |
| dtShrineChance | True |  |  | 8 | 2 | 0.2 | 0 | 0 | 0 | 0 | 2 | 0 | 0 | 0 | 0 |
| dtLockbox | True |  |  | 0 | 4 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtITBossWave | True |  |  | 0 | 80 | 7.5 | 7.5 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtITDefaultWave | True |  |  | 80 | 10 | 0.25 | 0.25 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtITLunar | True |  |  | 0 | 0 | 0 | 0 | 0 | 0 | 100 | 0 | 0 | 0 | 0 | 0 |
| dtITSpecialBossWave | True |  |  | 0 | 0 | 80 | 20 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtITVoid | True |  |  | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 80 | 20 | 1 | 0 |
| dtCategoryChest2Damage | True | Damage |  | 0 | 0.8 | 0.2 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtCategoryChest2Healing | True | Healing |  | 0 | 0.8 | 0.2 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtCategoryChest2Utility | True | Utility |  | 0 | 0.8 | 0.2 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtVoidCamp | True |  |  | 40 | 40 | 10 | 3 | 0 | 0 | 0 | 0 | 5.714286 | 5.714286 | 1.25 | 0 |
| dtVoidTriple | True |  |  | 0.8 | 0.2 | 0.01 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtVoidLockbox | True |  |  | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 5 | 5 | 2 | 0 |
| AurelioniteHeartPickupDropTable | True |  |  | 0 | 0 | 0.4 | 0.6 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| GeodeRewardDropTable | True |  |  | 0.8 | 0.2 | 0.01 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtShrineHalcyoniteTier1 | True |  |  | 0.65 | 0.3 | 0.05 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtShrineHalcyoniteTier2 | True | HalcyoniteShrine |  | 0.65 | 0.3 | 0.05 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtShrineHalcyoniteTier3 | True |  |  | 0.65 | 0.3 | 0.05 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtChanceDoll | True |  |  | 0 | 0.79 | 0.2 | 0.01 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtSonorousEcho | True |  |  | 0.9 | 0.1 | 0.001 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
| dtCommandChest | True |  | Any, Any, Any, Any, Any | 0.2 | 0.2 | 0.05 | 0.05 | 0 | 0 | 0 | 0.2 | 0.1 | 0.1 | 0.05 | 0.05 |


## Known issues
- Lunar Shop Terminal Price and Equipment Price labels are **not** displayed. This can't be fixed with a server-side mod.
