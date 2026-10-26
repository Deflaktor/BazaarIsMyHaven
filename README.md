# BazaarIsMyHaven

**Server-side mod**: only the host needs it installed.

Adds printers, cauldrons, shops and more to the Bazaar Between Time, all configurable. A fork of Lunzir's excellent [BazaarIsMyHome](https://thunderstore.io/package/Lunzir2/BazaarIsMyHome/).

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/E1E71PHUJ2)

## Features

- **More in the Bazaar**, each of which can be turned off:
  - 3D Printers
  - Cauldrons, also Transcendent Cauldrons (with Hallowed Concepts)
  - Scrappers
  - Equipment Terminals
  - A Lunar Shop with your own item list
  - Cleansing Pool
  - Shrine of Order
  - Donation Altar: donate for a reward
  - Command Pedestal: pick any item of its tier
  - Wandering CHEF, drone stations and a Triple Drone Shop (with the Alloyed Collective). Your drones come along into the Bazaar.
- **The further into the run, the more the Bazaar offers.**
- **Instanced Purchases**: every player can buy from each Equipment Terminal, Lunar Bud, Command Pedestal and Triple Drone Shop.
- **Buy to Inventory**: purchases go straight into the buyer's inventory.
- **Newt**: stop him from kicking you out, change what happens after you kill him, and let him greet you and comment on your purchases.
- **Portals**: spawn a Bazaar portal after every teleporter event, and keep your gold when you enter the Bazaar to spend it there.
- **Other tweaks**:
  - Drifter can't bag things, farm Junk or use Tinker in the Bazaar.
  - Captain's hacking beacon makes only one purchase free (requires a mod which allows him to use hacking in the bazaar).
  - Config changes apply from the next run, without restarting the game.

## Compatibility

Works with
- [BiggerBazaar](https://thunderstore.io/package/MagnusMagnuson/BiggerBazaar/)
- [ShareSuite](https://thunderstore.io/package/FunkFrog-and-Sipondo/ShareSuite/)
- [QoLChests](https://thunderstore.io/c/riskofrain2/p/Faustvii/QoLChests/)
- [InLobbyConfig](https://thunderstore.io/package/KingEnderBrine/InLobbyConfig/)
- [SuppressorInBazaar](https://thunderstore.io/c/riskofrain2/p/TheBugReport/SuppressorInBazaar/)
- [ProperSave](https://thunderstore.io/c/riskofrain2/p/KingEnderBrine/ProperSave/)
- [Quality](https://thunderstore.io/c/riskofrain2/p/Goorakh/Quality/): the Command Pedestal's item can have a quality, with the chance set in Quality's config.

Not compatible with
- BazaarLunarForEveryone

## How it works

The Bazaar is split into sections, one for each kind of thing this mod adds: printers, cauldrons, the Donation Altar and so on. Each has its own part of the config, starting with `SectionEnabled` if you don't want it at all.

Most of it doesn't just always appear. Every time you enter the Bazaar, a director gets some credits and spends them on what the sections offer, a bit like the game's own director does with chests on a stage. The further you are into the run, the more it has to spend:

`Credits = DirectorExtraCredits + DirectorCreditsPerStage x stages cleared`, capped at `DirectorCreditsCap` (-1 for no cap)

Everything it can buy has a `DirectorCost` (how many credits it takes) and a `DirectorWeight` (how often it gets picked over the rest). `MaxAmount` caps how many of one kind you can get. Anything that costs 0 always shows up, which is how the Lunar Buds and the Cleansing Pool work by default.

Some settings come up in several sections:

- `Cost` and `CostType`: the price and what you pay with, e.g. `Money`, `LunarCoin`, `WhiteItem` or `PercentHealth`. Money prices scale with difficulty if `ScaleMoneyCosts` is on.
- `InstancedPurchases`: every player can buy from it once, without taking it away from the others.
- `BuyToInventory`: what you buy goes straight into your inventory. Equipment you swap out stays in the shop, and swapping back there is free.

Item lists, like the Lunar Shop's `ItemList` or the Donation Altar's rewards, use the [ItemStringParser](https://thunderstore.io/package/Def/ItemStringParser/) format. Its page lists the item names and keywords. A few examples:

- `Tier1 | Tier2 | Tier3 | Lunar | Boss`: one item of a random tier
- `dtLunarChest`: the same items the game's own Lunar Buds have
- `5xdtChest1 | 2xdtChest2`: either 5 small chest items or 2 large chest items

## Sections

### Newt

- `NoKickFromShop`: he won't kick you out when you hit him.
- `DeathBehavior`: with `Tank`, `Ghost` or `Hostile` he has much less health and comes back after you kill him: with double health, as a ghost, or fighting back.
- `Dialogue`: he talks in chat. `Default` uses the game's own unused Newt lines, in each player's language, `BazaarIsMyHome` the original mod's lines, and `Off` keeps him quiet.
- `KarmaTrigger`: when angering him counts toward the run's karma.

### Printers

3D printers for items. The tier weights (`Tier1Weight`, `Tier2Weight`, ...) decide what a printer prints, and `DirectorCostWhite`, `DirectorCostGreen` and so on what each tier costs the director. Void printers exist too, but they're rare.

### Cauldrons

Trade items into another tier.

- `WhiteToGreenWeight` and the others pick the kind of cauldron.
- `WhiteToGreenCost` and the others set how many items it takes.
- `TranscendentWeight` adds Transcendent Cauldrons, with Hallowed Concepts.

### Scrappers

The director buys them once, and then all of them spawn. `MaxAmount` sets how many.

### Equipment Terminals

Sell equipment. `ReplaceWithEliteChance` sometimes offers an elite aspect instead, and `ReplaceLunarSeersWithEquipment` puts terminals where the Lunar Seers stand.

### Lunar Shop

Lunar Buds that sell from your own `ItemList`.

- `BuyLimit`: caps purchases per visit.
- `SequentialItems`: goes through the list in order instead of randomly, so `Tier1 | Tier2 | Tier3 | Lunar | Boss` on 5 buds gives one item of each tier.
- `AmountDependingOnCharacter`: gives some survivors more or fewer buds.

### Lunar Recycler

Rerolls the Lunar Buds. The first reroll costs `Cost`, each one after that `CostMultiplier` times more, and `RerollLimit` caps them per visit.

### Cleansing Pool

Like the normal one, but with `RewardsLunarCoin` you get a Lunar Coin back instead of a Pearl. Idea is to give a way out for accidental Lunar purchases.

### Lunar Seers

The two Seer Stations that pick where the next stage takes you. Turn them off with `Available` or change their `Cost`.

### Shrine of Order

Turns all your items of a tier into one random item of that tier, for a Lunar Coin. `UseLimit` caps uses per visit.

### Donation Altar

The director can place an altar next to Newt, and you donate to it for a reward: by default 5 Lunar Coins (`Cost`, `CostType`) for one reward per run. Each player donates on their own, and above the altar you see an example of what you could get. Once you've reached your limit (`RewardLimitPerVisit`, `RewardLimitPerRun`), the altar looks used to you, but the others can still donate.

What you get depends on your survivor:

- Every survivor has a reward of their own that fits their kit, e.g. 5 Sticky Bombs for Commando or 3 Medkits for REX. You can change each one (`RewardListCommando` and so on).
- `RewardListAvailableCharacters` sets which survivors have their own. Everyone else gets `RewardListCharacterDefault`, 3 green scrap by default. Survivors from other mods that give them a reward are added to it once, and stay out if you remove them.
- Rewards can also be a choice, e.g. `Tier3 | 2xTier2` gives one red item or two greens.

For more variety, `RewardList1` to `RewardList3` are extra reward lists, off by default. Give them a weight (`RewardList1Weight` and so on) and each donation picks a list by weight, the survivor's list included (`RewardListCharacterWeight`). With `SequentialRewardLists` on, they take turns instead, from the highest weight down. That way you can set it up that the more you donate the higher tier rewards you get.

### Command Pedestal

Pick any item of a tier for a price. Each tier has its own director cost, weight and price, e.g. `Tier2DirectorCost`, `Tier2Weight` and `Tier2Cost`.

### Wandering CHEF

Needs the Alloyed Collective. He cooks you a meal from one recipe, by default one you can actually make from your inventory and what the Bazaar sells (`CraftableRecipesOnly`). With `UnrestrictedCrafting` you can cook anything, which is the vanilla behavior.

### Drones

Needs the Alloyed Collective. A Drone Combiner Station, Drone Upgrade Station, Drone Scrapper and Triple Drone Shop, each with its own director cost and weight. `TripleDroneShopBuyAll` lets you take all three drones from the shop.

## For mod authors

You can hook your own mod into the Bazaar with `BazaarAPI`. Add `[BepInDependency(BazaarIsMyHaven.Main.PluginGUID)]` to your plugin and call it in `Awake`.

### Adding your own interactables

Everything this mod spawns in the Bazaar comes from a section (the printers, the Donation Altar, and so on). Each visit, a director gets some credits and buys from the sections' director cards, and then every section spawns whatever was bought for it.

To add your own, subclass `BazaarBase` and pass it to `BazaarAPI.AddSection`. Your cards go into the same pool as the built-in ones, so they compete for the same credits. Here's a section that sometimes adds a Shrine of Chance:

```csharp
public class ShrineSection : BazaarBase
{
    private AsyncOperationHandle<GameObject> shrine;
    private DirectorCard card;

    public override void Preload()
    {
        shrine = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ShrineChance/ShrineChance.prefab");
    }

    // on every visit: cost 5 credits, weight 1, at most 1
    public override void AddDirectorCards(List<DirectorCard> cards)
    {
        card = new DirectorCard("Shrine of Chance", () => 5, () => 1f, () => 1);
        cards.Add(card);
    }

    // spawns as many as the director bought
    public override void SetupBazaar()
    {
        for (int i = 0; i < card.Count; i++)
        {
            var instance = Object.Instantiate(shrine.WaitForCompletion(), new Vector3(-100f, -24f, -40f), Quaternion.identity);
            NetworkServer.Spawn(instance);
        }
    }
}

BazaarAPI.AddSection(new ShrineSection());
```

A few things worth knowing:

- Cost and weight can be plain numbers like above, or your own config entries: `new DirectorCard("Shrine of Chance", myCost, myWeight, () => 1)`.
- If your shop turns out to have nothing to sell, you can give the credits back in `CheckAfterSetup`. The director spends them again a frame later, and you spawn those in `SpawnLatePicks`.
- Apart from `Preload` and `Hook`, your section only runs on the host. Clients don't have your mod, so stick to prefabs the game already knows.

### Donation Altar rewards for your survivor

If your mod adds a survivor, you can give it its own Donation Altar reward:

```csharp
BazaarAPI.SetSurvivorDonationReward("MySurvivor", "3xFeather | Tier3");
```

The reward uses the [ItemStringParser](https://thunderstore.io/package/Def/ItemStringParser/) format. It only sets the default of the survivor's `RewardList<name>` setting, so players can still change it. The first time, usually because their config is older than your mod, your survivor is added to the player's `RewardListAvailableCharacters`. If they take it out again, it stays out.

## Credits

Thanks to Lunzir for [BazaarIsMyHome](https://thunderstore.io/package/Lunzir2/BazaarIsMyHome/), the mod this one is a fork of.

Thanks to [WeeabooGod](https://github.com/WeeabooGod) for a large contribution to this mod:

- Making unmodded clients see the Lunar Buds at the right size.
- The price signs above Equipment Terminals.
- Compatibility with QoLChests.
- Reloading the config at the start of each run.
- Various small fixes for the Donation Altar, Lunar Buds, cauldrons, printers, the Wandering CHEF and ShareSuite.
