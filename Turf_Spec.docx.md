  
**Turf**

Game Design & Technical Specification

*Unity 2D  |  Roguelite Territory Control*

Version 0.1  —  Draft

# **1\. Project Overview**

Turf is a Unity 2D, turn-based territory control game with a tabletop aesthetic. The player takes the role of a king competing against one or more AI opponents on a randomly generated tile board. The goal is to claim a majority of board tiles before opponents do.

The game features a roguelite meta-progression layer: between matches within a run the player may select perks and abilities that carry forward, increasing in complexity and strategic depth as runs progress.

## **1.1 High-Level Goals**

* Simple, readable turn structure suitable for a tabletop feel.

* Randomly generated boards that scale in complexity with run progression.

* Clean separation between match state, run state, and persistent account state.

* Extensible architecture that supports new tile types, perks, and AI difficulties without major refactoring.

## **1.2 Target Platform**

* Engine: Unity 2D (LTS recommended)

* Primary platform: PC (Windows / macOS)

* Language: C\#

# **2\. Game Rules**

| *This section is a skeleton for you to fill in. Use the boxes below to define the rules of your game.* |
| :---- |

## **2.1 Win Condition**

| Win Condition — *Once all tiles have been claimed, whoever owns the majority wins.*  |
| :---- |

## **2.2 Turn Structure**

| Describe what a player can do on their turn, how many actions they have, and how a turn ends *A player can choose to claim a tile that is in their current claimable tile range. The turn ends once the player claims their tile.*  |
| :---- |

## **2.3 Tile Capturing**

| Describe how tiles are captured — adjacency rules, action costs, any restrictions *Any tile within a player’s current claimable area can be claimed. Special Tiles alter the claimable range. Mountains and Desolate tiles can never be claimed.*  |
| :---- |

## **2.4 Special Tiles**

| List and describe any special tile types, their effects when captured or occupied, and when they appear *Forest: Visibility \- reveals a single tile in each cardinal direction. Claim Range \- allows claiming of a single tile in each cardinal direction. Special Behavior \- n/a Plains: Visibility \- reveals two tiles in each cardinal direction, and each adjacent corner tile. Claim Range \- allows claiming of two tiles in each cardinal direction, or one tile in a cardinal direction and one adjacent corner tile. Special Behavior \- n/a Tower: Visibility \- reveals three tiles in each cardinal direction, and all corner tiles in the shape of a diamond. Claim Range \- allows claiming ONLY the third tile in each cardinal direction, or one of the tiles on the edge of the diamond shape that gets revealed. Special Behavior \- n/a Cave: Visibility \- reveals a single tile in each cardinal direction, and every other Cave on the map. Claim Range \- allows claiming of a single tile in each cardinal direction Special Behavior \- allows claiming from the claimed Cave to any of the other unclaimed Caves on the map. Once a Cave has been used to travel to another Cave (or has been traveled to), both Caves lose this special behavior. Mountain: Visibility \- can never be claimed, so does not provide visibility. Claim Range \- can never be claimed. Special Behavior \- n/a Wizard: Visibility \- reveals a single tile in each cardinal direction. Claim Range \- allows claiming of a single tile in each cardinal direction. Special Behavior \- allows claiming of any unclaimed tile anywhere across the map. Once this special ability is used, the Wizard loses it.*  *Barbarians: Visibility \- reveals a single tile in each cardinal direction. Claim Range \- allows claiming of a single tile in each cardinal direction. Special Behavior \- once revealed from the Fog of War, the Barbarians will pick a direction (either horizontal or vertical, whichever is longer) and will charge down the entire line of Tiles. Any Tile the Barbarian touches will be unclaimed.*  |
| :---- |

## **2.5 Perks & Abilities**

| Describe how perks are earned, when they are offered, and any limits on how many can be held *Perks will be offered to the player every 3 turns during a special meta-round. The player will be offered 3 Perks that are randomly selected from a greater pool, of which they can choose 1\. The player can have a total of 5 Perks at any given time.*  |
| :---- |

## **2.6 Losing Conditions**

| Describe how a match or run is lost *A match is lost whenever the player is not the winner, which will take away a player’s life.A run is lost whenever the player loses all 3 of their lives.*  |
| :---- |

## **2.7 Additional Rules**

| Any other rules, edge cases, or clarifications *This area is subject to grow as the project develops.*  |
| :---- |

# **3\. Architecture Overview**

The codebase is organized around four core principles: single responsibility per class, event-driven communication between systems, a clear data hierarchy across player lifetime tiers, and a state machine driving the game loop.

## **3.1 Scene Structure**

* MainMenuScene — account loading, credits, new run vs continue, settings.

* GameSelectionScene — game selection (campaign, challenge runs, endless), settings.

* MetaScene — between-match perk selection and run stats. Skipped before the first match.

* GameScene — all active gameplay. Initialized by GameBootstrapper.

## **3.2 Folder Structure**

| */Scripts  /Core         — GameManager, EventManager, GameBootstrapper  /Board        — BoardGenerator, BoardData, BoardRenderer, TileData  /Input        — BoardInput  /Players      — BaseMatchProfile, PlayerMatchData, AIMatchData, AIController  /Persistence  — PlayerAccountProfile, PlayerRunData, SaveManager  /UI           — UIManager, BaseButton and subclasses  /Visuals      — AnimationSystem  /Meta         — PerkLibrary, PerkData, AchievementManager* |
| :---- |

# **4\. Class Reference**

## **4.1 Core Classes**

| Class | Extends | Type | Lifetime | Responsibility |
| :---- | :---- | :---- | :---- | :---- |
| GameManager | MonoBehaviour | Singleton | Scene | Owns state machine, drives Update loop, coordinates system init |
| EventManager | — | Static class | App | Central event bus — all cross-system communication goes through here |
| GameBootstrapper | MonoBehaviour | Scene component | Scene | Initializes all systems in correct order on scene load |

## **4.2 State Classes**

All state classes are plain C\# (no MonoBehaviour). They implement IGameState and are instantiated once by GameManager.

| Class | Extends | Type | Lifetime | Responsibility |
| :---- | :---- | :---- | :---- | :---- |
| IGameState | — | Interface | Match | Contract: Enter(), Tick(), Exit() |
| PlayerTurnState | IGameState | Plain C\# | Match | Handles player input, action spending, highlight management |
| EnemyTurnState | IGameState | Plain C\# | Match | Triggers AI controller, waits for AI to complete turn |
| CheckWinState | IGameState | Plain C\# | Match | Counts owned tiles, determines winner or continues loop |
| GameOverState | IGameState | Plain C\# | Match | Bubbles stats, shows end screen, loads next scene |

## **4.3 Player & Account Classes**

Player data is split across three lifetime tiers. Data only flows upward at tier boundaries — match end updates run data, run end updates account.

| Class | Extends | Type | Lifetime | Responsibility |
| :---- | :---- | :---- | :---- | :---- |
| PlayerAccountProfile | — | Plain C\# | Permanent | Career stats, achievements, all unlocked perks. Saved to disk. |
| PlayerRunData | — | Plain C\# | One run | Active perks, run stats, bonus actions. Not saved. |
| BaseMatchProfile | — | Abstract C\# | One match | Shared in-match data: owned tiles, actions, ClaimTile(), SpendActions() |
| PlayerMatchData | BaseMatchProfile | Plain C\# | One match | Human player match state. Holds reference to PlayerRunData. |
| AIMatchData | BaseMatchProfile | Plain C\# | One match | AI match state. Holds AIDifficulty. No upward references. |
| AIController | — | Plain C\# | One match | AI decision logic. Receives AIMatchData \+ BoardData. Separate from data. |

## **4.4 Board Classes**

| Class | Extends | Type | Lifetime | Responsibility |
| :---- | :---- | :---- | :---- | :---- |
| BoardGenerator | — | Plain C\# | Match | Produces a BoardData from BoardSettings. No Unity dependencies. |
| BoardSettings | — | Plain C\# | Pre-match | Width, height, tile type probabilities. Scales with run progress. |
| BoardData | — | Plain C\# | Match | Dictionary\<Vector3Int, TileData\>. Source of truth for all tile state. |
| TileData | — | Plain C\# | Match | Type, owner, capture cost, SetOwner(). One instance per cell. |
| BoardRenderer | MonoBehaviour | Scene component | Scene | Translates BoardData into Tilemaps. Reacts to EventManager events. |

## **4.5 UI Classes**

| Class | Extends | Type | Lifetime | Responsibility |
| :---- | :---- | :---- | :---- | :---- |
| BaseButton | MonoBehaviour | Abstract | Scene | Shared hover, click, disabled logic. Implements pointer interfaces. |
| EndTurnButton | BaseButton | Concrete | Scene | Transitions to EnemyTurnState on click. |
| CaptureTileButton | BaseButton \+ IConfirmable | Concrete | Scene | Opens confirm dialog, fires EventManager.TileCapture on confirm. |
| PerkButton | BaseButton | Concrete | Meta scene | Displays a PerkData, fires EventManager.PerkSelected on click. |
| UIManager | MonoBehaviour | Singleton | Scene | Coordinates UI panels, confirm dialogs, action counter display. |

# **5\. EventManager Reference**

EventManager is a static class. No GameObject required. Any class subscribes and unsubscribes in OnEnable/OnDisable (MonoBehaviours) or via constructor/destructor (plain C\# classes).

## **5.1 Events**

| Event | Signature | Fired When |
| :---- | :---- | :---- |
| OnTileCaptured | Action\<Vector3Int, BaseMatchProfile\> | A tile's owner changes |
| OnTurnStarted | Action\<BaseMatchProfile\> | A new turn begins for any player |
| OnTurnEnded | Action\<BaseMatchProfile\> | A player's turn ends |
| OnNumActionsChanged | Action\<int\> | Actions remaining changes |
| OnGameOver | Action\<BaseMatchProfile\> | A winner is determined |
| OnPerkSelected | Action\<PerkData\> | Player picks a perk in MetaScene |

## **5.2 Subscription Pattern**

| *Always unsubscribe in OnDisable to prevent null reference errors from destroyed objects still receiving events.void OnEnable()  \=\> EventManager.OnTileCaptured \+= HandleTileCaptured;void OnDisable() \=\> EventManager.OnTileCaptured \-= HandleTileCaptured;* |
| :---- |

# **6\. Board & Tilemap Structure**

## **6.1 Tilemap Layers**

Four Tilemap layers sit on a single Grid GameObject in the GameScene hierarchy, ordered bottom to top:

| Layer | Updates | Purpose |
| :---- | :---- | :---- |
| TerrainTilemap | Once (on render) | Base terrain sprites. Never changes after board generation. |
| OwnershipTilemap | On capture | Semi-transparent color overlay showing current tile owner. |
| HighlightTilemap | Each turn | Valid capture indicators and hover highlights. Cleared on turn end. |
| UnitsTilemap | On move (future) | Reserved for unit pieces if added later. |

## **6.2 BoardData (Parallel Dictionary)**

BoardData is the source of truth. Tilemaps are purely visual — they never drive game logic. Any query about board state reads from BoardData, never from the Tilemap.

* Key: Vector3Int (grid cell coordinates)

* Value: TileData (type, owner, capture cost)

* BoardRenderer subscribes to OnTileCaptured and repaints ownership visuals when the dictionary changes.

## **6.3 Board Generation**

BoardGenerator produces a fresh BoardData from a BoardSettings object each match. Tile type probabilities shift over the course of a run — early matches favor plain tiles, later matches introduce more special tiles.

# **7\. Game Loop**

## **7.1 GameScene Boot Order**

GameBootstrapper.Start() initializes systems in this exact order to avoid null reference errors:

* 1\. Load PlayerAccountProfile from disk via SaveManager.

* 2\. Read PlayerRunData from SessionData (passed from MetaScene).

* 3\. Create PlayerMatchData and AIMatchData.

* 4\. Generate BoardData via BoardGenerator.

* 5\. Assign starting tiles to each player.

* 6\. Render board via BoardRenderer.

* 7\. Call GameManager.Init() with players and board.

* 8\. Call GameManager.TransitionTo(PlayerTurnState).

## **7.2 Match Loop**

Once booted, the match loop runs entirely through the state machine:

| *PlayerTurnState → (actions exhausted) → CheckWinState → (no winner) → EnemyTurnState → (AI done) → CheckWinState → (no winner) → PlayerTurnState → ...* |
| :---- |

## **7.3 Match End & Data Flow**

When CheckWinState finds a winner it transitions to GameOverState, which:

* Updates PlayerRunData with match result.

* Bubbles stats up to PlayerAccountProfile (total wins, tiles captured, etc.).

* Checks achievements via AchievementManager.

* Saves PlayerAccountProfile to disk via SaveManager.

* Loads MetaScene for perk selection, or RunEndScene if the run is over.

# **8\. Roguelite Meta Layer**

## **8.1 Run Structure**

A run is a sequence of matches. Each match the board grows in complexity. Between matches the player visits MetaScene to select one perk from a randomized offer of three.

## **8.2 Perk System**

* PerkData is a ScriptableObject defining a perk's display name, description, and stat modifiers.

* PerkLibrary holds all available perks and handles random selection for offers.

* Selected perks are stored on PlayerRunData.ActivePerks for the remainder of the run.

* Perks affect PlayerMatchData at match start via MatchSettings.FromRunData().

## **8.3 Persistence**

* PlayerAccountProfile is the only object saved to disk.

* PlayerRunData lives in memory only — losing a run discards it.

* Unlocked perks (for future runs) are stored on PlayerAccountProfile.UnlockedPerks.

# **9\. AI System**

## **9.1 AIMatchData vs AIController**

AIMatchData is a data class (extends BaseMatchProfile) and is treated identically to PlayerMatchData by all game systems. AIController is a separate logic class that reads AIMatchData and BoardData to make decisions. The separation means AI behavior can be changed without touching data or event systems.

## **9.2 Difficulty**

| Difficulty | Actions/Turn | Behavior |
| :---- | :---- | :---- |
| Easy | 2 | Greedy — always captures highest value adjacent tile it can afford. |
| Medium | 3 | Greedy \+ defensive — considers blocking player expansion. |
| Hard | 4 | Lookahead — evaluates multi-step sequences, prioritizes map control. |

# **10\. Open Questions & TODO**

| *Track unresolved design decisions and outstanding implementation tasks here.* |
| :---- |

| Open design questions *\[ Fill in here \]*  |
| :---- |

| Outstanding implementation tasks *\[ Fill in here \]*  |
| :---- |

| Known issues / risks *\[ Fill in here \]*  |
| :---- |

