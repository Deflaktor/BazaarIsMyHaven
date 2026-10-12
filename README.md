# BazaarIsMyHaven

**server-side mod** - only the host needs it installed.

This is a fork of Lunzir's excellent [BazaarIsMyHome](https://thunderstore.io/package/Lunzir2/BazaarIsMyHome/) mod.

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/E1E71PHUJ2)

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
  - Wandering Chef
  - Drone stations from the Alloyed Collective

- **Newt Behavior Options**:
  - Stop him from throwing you out.
  - Change what happens after you kill him.
  - Extra dialogue lines.

- **Portal Options**:
  - Spawn a Bazaar portal after *every* teleporter event.

- **Other Tweaks**:
  - A director that decides which interactables appear, with more credits the further you are into the run.
  - Config changes apply from the next run, without restarting the game.

- **Compatiblity with other Mods**
  - [BiggerBazaar](https://thunderstore.io/package/MagnusMagnuson/BiggerBazaar/)
  - [InLobbyConfig](https://thunderstore.io/package/KingEnderBrine/InLobbyConfig/)

## Key Settings

In-detail descriptions for some of the settings:

### General - Director

Like the game's own scene director, a director decides which interactables appear in the Bazaar. On every visit it gets credits:

Formula: `Credits = DirectorExtraCredits + DirectorCreditsPerStage x Number of Stages Completed`

It spends them at random on the interactables of the enabled sections, until it can't afford anything more. Each interactable has a `DirectorCost` and a `DirectorWeight`. The weight sets how likely the director picks it compared to the others, and `MaxAmount` limits how many of a kind can spawn. By default, a printer costs 5 to 15 credits depending on its tier, and Lunar Buds cost nothing, so they always spawn up to their `MaxAmount`.

Two interactables work differently:

- **Scrappers**: the director picks them only once, and then all `MaxAmount` Scrappers spawn.
- **Wandering Chef**: with `CraftableRecipesOnly`, he only offers a recipe that a player can cook from their inventory and what the Bazaar offers. If there is none, he leaves again and the director spends his credits on something else.

### Newt - DeathBehavior

Controls how Newt acts after being killed:

- `Default` -> Normal behavior.  
- `Tank` -> Newt Health is significantly reduced. Revives with double HP.
- `Ghost` -> Newt Health is significantly reduced. Revives as a ghost.
- `Hostile` -> Newt Health is significantly reduced. Revives and starts defending himself.

### Equipment

With a `Cost` above 0, each Equipment Terminal shows its price above it. Clients without the mod see it too. `CostType` sets what the price is paid in, such as money (the default), Lunar Coins or items of a tier.

### LunarShop

The Lunar Shop swaps the Bazaar's Lunar Buds for up to `MaxAmount` Lunar Buds (at most 20), each with its price. `CostType` sets what the price is paid in, such as Lunar Coins (the default), money or items of a tier. Clients without the mod also see them at the right size. The Lunar Recycler rerolls them, except buds a player already bought from.

You can freely configure which items can be bought at the LunarShop. There are two settings to configure this:

- `SequentialItems`:
  - **True** -> Items are drawn in order from the list. So if the number of Lunar Buds matches the number of items in the list, you always find the same items in the Bazaar.
  - **False** -> Items are picked at random from the list.

- `ItemList`:
  - Follows the [ItemStringPaser](https://thunderstore.io/package/Def/ItemStringParser/) format.
  - A list of items to appear. Must be internal items names. Can use tier or droptables as well, see *Item Keywords* below.
  - Examples:
    - `Tier1 | Tier2 | Tier3 | Lunar | Boss`: If `SequentialItems` is set to `True` and `MaxAmount` to 5, then you will find exactly 1 white item, 1 green item, 1 red item, 1 lunar item and 1 boss item in the shop.
    - `dtLunarChest`: This is the vanilla behavior of the game.
    - `FreeChest | VoidTier1 | dtChest2`: 1 Shipping Request Form, one random item of Void Tier 1 and one random item of the droptable of a large chest.
  - The Repeat and Multiplier operators do not play a role here.

### Donation Altar

The Donate section lets the director spawn a Donation Altar near the Newt. Donating Lunar Coins gives you a reward.

Every 2 seconds, each player sees a reward they could get on up to five Lunar Buds above the altar, one bud per item. These are only examples: the reward for a donation is rolled when you donate. Clients need no mod for this.

There are 3 item lists which are selected at random:

- `RewardList1`: By default contains either 5 small chest items or 2 large chest items.
- `RewardList2`: By default contains either 1 legendary item or 1 boss item.
- `RewardList3`: Disabled by default. By default contains some unreleased or unfinished items.

These reward lists can be fully customized. They follow the [ItemStringPaser](https://thunderstore.io/package/Def/ItemStringParser/) format. See the below section *Item Keyword List* on what are valid values. With the donate reward lists, it is possible to reward multiple items at the same time.

Examples:

`RewardList1 = 5xdtITDefaultWave`: The reward will be 5 random items of the droptable of void potentials from the Simulacrum mode.

`RewardList1 = 5xdtChest1 | 2xdtChest2`: The reward will be either 5 random items of the small chest droptable or 2 random items of the large chest droptable.

### Drones

If the Alloyed Collective is enabled for the run, the Drones section lets the director spawn a Drone Combiner Station, a Drone Upgrade Station, a Drone Scrapper and a Triple Drone Shop. Each one has its own settings.

Drones normally stay behind when you enter the Bazaar, but the Drone Combiner Station, the Drone Upgrade Station and the Drone Scrapper only work with drones that are there. So when one of them spawns, the players' drones come along. They wait at one spot without moving or attacking. Drones that show up elsewhere, such as bought or upgraded ones, are moved there after 2 seconds. On the next stage, drones behave as usual again.

## RewardListAvailableCharacters - Valid Keywords

### Survivors

Bandit2
Captain
Commando
Croco
Engi
Heretic
Huntress
Loader
Mage
Merc
Toolbot
Treebot
Railgunner
VoidSurvivor
Chef
FalseSon
Seeker
Drifter
DroneTech

### Other Bodies

AcidLarvaBody
AffixEarthHealerBody
AltarSkeletonBody
AncientWispBody
ArchWispBody
ArtifactShellBody
Assassin2Body
AssassinBody
BackupDroneBody
BackupDroneBodyRemoteOp
BackupDroneOldBody
Bandit2Body
BanditBody
BasePodBody
BasePodBody_NoRevive
BeadProjectileTrackingBomb
BeetleBody
BeetleCrystalBody
BeetleGuardAllyBody
BeetleGuardBody
BeetleGuardCrystalBody
BeetleQueen2Body
BeetleWard
BellBody
BirdsharkBody
BisonBody
BombardmentDroneBody
BombardmentDroneBodyRemoteOp
BomberBody
BrotherBody
BrotherGlassBody
BrotherHauntBody
BrotherHurtBody
CaptainBody
ChefBody
ChildBody
ClayBody
ClayBossBody
ClayBruiserBody
ClayGrenadierBody
CleanupDroneBody
CleanupDroneBodyRemoteOp
CommandoBody
CommandoPerformanceTestBody
CopycatDroneBody
CopycatDroneBodyRemoteOp
CorruptionSpike
CrocoBody
DTGunnerDroneBody
DTGunnerDroneBrokenBody
DTHaulerDroneBody
DTHaulerDroneBrokenBody
DTHealingDroneBody
DTHealingDroneBrokenBody
DeathProjectile
DefectiveUnitBody
DestructibleSpawnerObjectBody
DevotedLemurianBody
DevotedLemurianBruiserBody
DrifterBody
DrifterShieldTank
DrifterThqwib
Drone1Body
Drone1BodyRemoteOp
Drone2Body
Drone2BodyRemoteOp
DroneBallDotZone
DroneBomberBody
DroneCommanderBody
DroneTechBody
DroneTechShield
ElectricWormBody
EmergencyDroneBody
EmergencyDroneBodyRemoteOp
EnforcerBody
EngiBeamTurretBody
EngiBody
EngiTurretBody
EngiWalkerTurretBody
EquipmentDroneBody
EquipmentDroneBodyRemoteOp
ExhaustPortWeakpointBody
ExplosiveJunkBombDestructibleBody
ExplosivePotDestructibleBody
ExtractorUnitBody
FalseSonBody
FalseSonBossBody
FalseSonBossBodyBrokenLunarShard
FalseSonBossBodyLunarShard
FireExtinguisherPodBody
FlameDroneBody
FlameDroneBodyRemoteOp
FlyingVerminBody
FriendUnitBody
FusionCellDestructibleBody
GeepBody
GipBody
GolemBody
GolemBodyInvincible
GrandParentBody
GravekeeperBody
GravekeeperTrackingFireball
GreaterWispBody
GupBody
HANDBody
HalcyoniteBody
HaulerBody
HaulerDroneBody
HeatSinkPodBody
HeaterPodBody
HeaterPodBodyNoRespawn
HeaterPodBodyNoRespawn_Large
HereticBody
HermitCrabBody
HuntressBody
ITBrotherBody
ImpBody
ImpBossBody
IronHaulerBody
JailerDroneBody
JailerDroneBodyRemoteOp
JellyfishBody
JunkCubeConsoleOptPrefabVariant
JunkCubePrefab
JunkDroneBody
JunkDroneBodyRemoteOp
LemurianBody
LemurianBruiserBody
LoaderBody
LunarExploderBody
LunarGolemBody
LunarRain
LunarRain_DistanceTest
LunarWispBody
LunarWispTrackingBomb
MageBody
MagmaWormBody
MajorConstructBody
MegaConstructBody
MegaDroneBody
MegaDroneBodyRemoteOp
MercBody
MinePodBody
MiniGeodeBody
MiniMushroomBody
MiniVoidRaidCrabBodyBase
MiniVoidRaidCrabBodyPhase1
MiniVoidRaidCrabBodyPhase2
MiniVoidRaidCrabBodyPhase3
MinorConstructAttachableBody
MinorConstructBody
MinorConstructOnKillBody
MissileDroneBody
MissileDroneBodyRemoteOp
NullifierAllyBody
NullifierBody
PaladinBody
ParentBody
ParentPodBody
Pot2Body
PotMobile2Body
PotMobileBody
PowerOrbShieldTank
RailgunnerBody
RechargeDroneBody
RechargeDroneBodyRemoteOp
RoboBallBossBody
RoboBallGreenBuddyBody
RoboBallMiniBody
RoboBallRedBuddyBody
SMInfiniteTowerMaulingRockLarge
SMInfiniteTowerMaulingRockMedium
SMInfiniteTowerMaulingRockSmall
SMMaulingRockLarge
SMMaulingRockMedium
SMMaulingRockSmall
ScavBody
ScavLunar1Body
ScavLunar2Body
ScavLunar3Body
ScavLunar4Body
ScavSackProjectile
ScorchlingBody
ScorchlingBombProjectile
SeekerBody
ShopkeeperBody
SniperBody
SolusAmalgamatorBody
SolusAmalgamatorFlamethrowerCannonBody
SolusAmalgamatorMissilePodBody
SolusAmalgamatorThrusterBody
SolusAmalgamatorTrackingBomb
SolusHeartBody
SolusHeartBody_Logbook
SolusHeartBody_Offering
SolusHeart_DDOSProjectile
SolusMineBody
SolusVendorBody
SolusWingBody
SolusWingLogbookBody
SolusWing_LaserBurstBlastProjectile
SpectatorBody
SpectatorSlowBody
SquidTurretBody
SulfurPodBody
SuperRoboBallBossBody
TankerAccelerantPuddleBodyProjectile
TankerBody
TankerLogbookBody
TeleportComboLaserProjectile
TimeCrystalBody
TitanBody
TitanGoldBody
ToolbotBody
TreebotBody
Turret1Body
UnderclockSpawnerProjectile
UrchinTurretBody
VagrantBody
VagrantTrackingBomb
VerminBody
VoidBarnacleBody
VoidBarnacleNoCastBody
VoidInfestorBody
VoidJailerAllyBody
VoidJailerBody
VoidMegaCrabAllyBody
VoidMegaCrabBody
VoidRaidCrabBody
VoidRaidCrabJointBody
VoidSurvivorBody
VultureBody
VultureEggBody
VultureHunterBody
WispBody
WispSoulBody
WorkerUnitBody
