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
- **Instanced Purchases**: every player can buy from each Equipment Terminal, Lunar Bud and Command Pedestal.
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

Not compatible with
- BazaarLunarForEveryone

## Settings

The config file explains every setting. A few need more:

### Director

On every visit, a director spends credits on interactables, like the game's own scene director:

`Credits = DirectorExtraCredits + DirectorCreditsPerStage x stages cleared`, at most `DirectorCreditsCap` (-1 for no cap)

Each interactable has a `DirectorCost` and a `DirectorWeight`, which sets how often it gets picked. `MaxAmount` limits how many of a kind appear. Lunar Buds and the Cleansing Pool cost nothing, so they always appear.

### Shops

- `InstancedPurchases`: each player can buy from a shop once, without taking it away from the others.
- `BuyToInventory`: purchases go straight into the buyer's inventory. Equipment you replace stays in the shop, and swapping back there is free for you.
- `CostType`: what a price is paid in, e.g. `Money`, `LunarCoin`, `WhiteItem` or `PercentHealth`.

### Item lists

The Lunar Shop's `ItemList` and the Donation Altar's reward lists use the [ItemStringParser](https://thunderstore.io/package/Def/ItemStringParser/) format. Its page lists the item names and keywords. Examples:

- `Tier1 | Tier2 | Tier3 | Lunar | Boss`: one item of a random tier. With `SequentialItems` on, the Lunar Shop takes them in order, so 5 buds offer one item of each tier.
- `dtLunarChest`: the game's own Lunar Bud items.
- `5xdtChest1 | 2xdtChest2`: a reward of either 5 small chest items or 2 large chest items.

### Donation Altar

A donation gives a reward from one of the reward lists, picked by their weights:

- `RewardList<Character>`, e.g. `RewardListCommando`: a reward for each survivor, used by default. `RewardListAvailableCharacters` sets which characters have one, by survivor name or body name. Everyone else gets `RewardListCharacterDefault`.
- `RewardList1` to `RewardList3`: more lists, off by default.

`RewardLimitPerVisit` and `RewardLimitPerRun` limit how many rewards each player gets.

### Newt

- `Dialogue`: `Off`, `Default` (the game's own unused Newt lines, in each player's language) or `BazaarIsMyHome` (the original mod's lines).
- `KarmaTrigger`: in runs with karma, angering Newt makes the final fight harder. `OnDamage` raises karma when he's hit, like the game does; `OnKill` only when he dies.
- `DeathBehavior`: with `Tank`, `Ghost` or `Hostile`, Newt has much less health and comes back after you kill him: with double health, as a ghost, or fighting back.

## Credits

Thanks to Lunzir for [BazaarIsMyHome](https://thunderstore.io/package/Lunzir2/BazaarIsMyHome/), the mod this one is a fork of.

Thanks to [WeeabooGod](https://github.com/WeeabooGod) for a large contribution to this mod:

- Making unmodded clients see the Lunar Buds at the right size.
- The price signs above Equipment Terminals.
- Compatibility with QoLChests.
- Reloading the config at the start of each run.
- Various small fixes for the Donation Altar, Lunar Buds, cauldrons, printers, the Wandering CHEF and ShareSuite.
