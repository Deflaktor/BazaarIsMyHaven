# BazaarIsMyHaven

**server-side mod** - only the host needs it installed.

This is a fork of Lunzir's excellent [BazaarIsMyHome](https://thunderstore.io/package/Lunzir2/BazaarIsMyHome/) mod.

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

- **Compatiblity with other Mods**
  - [BiggerBazaar](https://thunderstore.io/package/MagnusMagnuson/BiggerBazaar/)
  - [InLobbyConfig](https://thunderstore.io/package/KingEnderBrine/InLobbyConfig/)

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
  - Follows the [ItemStringPaser](https://thunderstore.io/package/Def/ItemStringParser/) format.
  - A list of items to appear. Must be internal items names. Can use tier or droptables as well, see *Item Keywords* below.
  - Examples:
    - `Tier1 | Tier2 | Tier3 | Lunar | Boss`: If `SequentialItems` is set to `True` and `Amount` to 5, then you will find exactly 1 white item, 1 green item, 1 red item, 1 lunar item and 1 boss item in the shop.
    - `dtLunarChest`: This is the vanilla behavior of the game.
    - `FreeChest | VoidTier1 | dtChest2`: 1 Shipping Request Form, one random item of Void Tier 1 and one random item of the droptable of a large chest.
  - The Repeat and Multiplier operators do not play a role here.

### Donation Altar

The Donate setting spawns a donation box near the Newt. After donating 10 times, the Newt will give you a reward. There are 3 item lists which are selected at random:

- `RewardList1`: By default contains either 5 small chest items or 2 large chest items.
- `RewardList2`: By default contains either 1 legendary item or 1 boss item.
- `RewardList3`: Disabled by default. By default contains some unreleased or unfinished items.

These reward lists can be fully customized. They follow the [ItemStringPaser](https://thunderstore.io/package/Def/ItemStringParser/) format. See the below section *Item Keyword List* on what are valid values. With the donate reward lists, it is possible to reward multiple items at the same time.

Examples:

`RewardList1 = 5xdtITDefaultWave`: The reward will be 5 random items of the droptable of void potentials from the Simulacrum mode.

`RewardList1 = 5xdtChest1 | 2xdtChest2`: The reward will be either 5 random items of the small chest droptable or 2 random items of the large chest droptable.

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

## Known issues

- Lunar Shop Terminal Price and Equipment Price labels are **not** displayed. This can't be fixed with a server-side mod.
