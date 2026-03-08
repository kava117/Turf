# Turf — Implementation Plan

## Assumptions & Clarifications (locked in)
- Human player gets **1 action (tile claim) per turn** by default; may increase via perks later.
- **Full Fog of War**: all tiles are hidden at match start and are only revealed by owned tiles' visibility ranges.
- **Perks** are offered every **3 matches** within a run (not every 3 turns).
- Players start with **1 pre-owned tile**, placed as far apart from each other as the board allows.
- A player may claim any tile adjacent to **any** of their owned tiles (not just the most recent).
- **Barbarians** unown tiles belonging to **all** players when they charge.
- All code goes **fresh** into `turf/Assets/Scripts/` — old scripts folder is reference only.
- **Placeholder art** is acceptable; assets will be swapped later.

---

## Phase 1 — Core Infrastructure

These classes form the backbone everything else depends on. Build and test them before moving on.

### Step 1.1 — EventManager
**File:** `Scripts/Core/EventManager.cs`

- Static class, no MonoBehaviour, no GameObject required.
- Define all events as static `Action` delegates:
  - `OnTileCaptured` — `Action<Vector3Int, BaseMatchProfile>`
  - `OnTurnStarted` — `Action<BaseMatchProfile>`
  - `OnTurnEnded` — `Action<BaseMatchProfile>`
  - `OnNumActionsChanged` — `Action<int>`
  - `OnGameOver` — `Action<BaseMatchProfile>`
  - `OnPerkSelected` — `Action<PerkData>`
- No logic — only event declarations.
- **Test:** Another class subscribes, fires the event, and confirms the handler is called.

### Step 1.2 — IGameState Interface
**File:** `Scripts/Core/IGameState.cs`

- Plain C# interface with three methods: `Enter()`, `Tick()`, `Exit()`.
- No implementation here.

### Step 1.3 — GameManager
**File:** `Scripts/Core/GameManager.cs`

- MonoBehaviour singleton (destroys duplicate on Awake).
- Holds references to all four state instances: `PlayerTurnState`, `EnemyTurnState`, `CheckWinState`, `GameOverState`.
- Tracks `_currentState` (IGameState) and `ActivePlayer` (BaseMatchProfile).
- `Init(PlayerMatchData player, AIMatchData ai, BoardData board)` — stores references, instantiates all states.
- `TransitionTo(IGameState newState)` — calls `Exit()` on current, sets new, calls `Enter()`.
- `Update()` — calls `_currentState.Tick()`.
- Does **not** know game rules; only drives the state machine.

### Step 1.4 — GameBootstrapper
**File:** `Scripts/Core/GameBootstrapper.cs`

- MonoBehaviour on a GameObject in GameScene.
- `Start()` runs the boot sequence in this exact order:
  1. Load `PlayerAccountProfile` from disk via `SaveManager`.
  2. Read `PlayerRunData` from a static `SessionData` holder (passed from MetaScene).
  3. Create `PlayerMatchData` and `AIMatchData`.
  4. Generate `BoardData` via `BoardGenerator`.
  5. Assign starting tiles to each player via `BoardData.AssignStartingTiles()`.
  6. Render board via `BoardRenderer.Render(boardData)`.
  7. Call `GameManager.Init(player, ai, board)`.
  8. Call `GameManager.TransitionTo(PlayerTurnState)`.

### Step 1.5 — SessionData
**File:** `Scripts/Core/SessionData.cs`

- Static class that acts as a scene-to-scene data bus.
- Holds `PlayerRunData ActiveRun` and any settings needed at boot.
- Set before loading GameScene; read by GameBootstrapper; cleared on run end.

---

## Phase 2 — Player & Account Data

### Step 2.1 — PlayerAccountProfile
**File:** `Scripts/Persistence/PlayerAccountProfile.cs`

- Plain C# class, permanent lifetime (saved to disk).
- Fields: `TotalWins`, `TotalLosses`, `TotalTilesCaptured`, `List<PerkData> UnlockedPerks`.
- Methods: `RecordMatchResult(bool won, int tilesCaptured)`.

### Step 2.2 — PlayerRunData
**File:** `Scripts/Persistence/PlayerRunData.cs`

- Plain C# class, lives in memory for one run only.
- Fields: `LivesRemaining` (starts at 3), `MatchesPlayed`, `List<PerkData> ActivePerks`.
- Methods: `AddPerk(PerkData)`, `LoseLife()`, `IsRunOver()` (LivesRemaining <= 0).

### Step 2.3 — BaseMatchProfile
**File:** `Scripts/Players/BaseMatchProfile.cs`

- Abstract plain C# class (no MonoBehaviour).
- Fields: `string DisplayName`, `int ActionsPerTurn`, `int ActionsRemaining`, `List<Vector3Int> OwnedTiles`.
- Methods:
  - `ClaimTile(Vector3Int cell, BoardData board)` — adds cell to OwnedTiles, calls `board.SetOwner()`, fires `EventManager.OnTileCaptured`.
  - `SpendAction()` — decrements ActionsRemaining, fires `EventManager.OnNumActionsChanged`.
  - `ResetActions()` — resets ActionsRemaining to ActionsPerTurn.
  - `abstract List<Vector3Int> GetClaimableRange(BoardData board)` — each subclass computes valid target tiles.

### Step 2.4 — PlayerMatchData
**File:** `Scripts/Players/PlayerMatchData.cs`

- Extends `BaseMatchProfile`.
- Constructor takes a `PlayerRunData` reference.
- Default `ActionsPerTurn = 1`.
- `GetClaimableRange` returns all unclaimed, non-Mountain, non-Desolate tiles adjacent to any owned tile, filtered through Fog of War rules.

### Step 2.5 — AIMatchData
**File:** `Scripts/Players/AIMatchData.cs`

- Extends `BaseMatchProfile`.
- Constructor takes `AIDifficulty` enum (Easy/Medium/Hard).
- `ActionsPerTurn` is set from difficulty: Easy=2, Medium=3, Hard=4.
- No upward reference to run or account data.
- `GetClaimableRange` — same adjacency logic as PlayerMatchData.

### Step 2.6 — SaveManager
**File:** `Scripts/Persistence/SaveManager.cs`

- Static class.
- `Save(PlayerAccountProfile)` — serializes to JSON, writes to `Application.persistentDataPath`.
- `Load()` — reads JSON, deserializes; returns new empty profile if none exists.

---

## Phase 3 — Board System

### Step 3.1 — TileType Enum
**File:** `Scripts/Board/TileType.cs`

```
Forest, Plains, Tower, Cave, Mountain, Wizard, Barbarian, Desolate
```

### Step 3.2 — TileData
**File:** `Scripts/Board/TileData.cs`

- Plain C# class. One instance per cell.
- Fields: `TileType Type`, `BaseMatchProfile Owner` (null = unclaimed), `bool IsRevealed`, `bool CaveUsed`, `bool WizardUsed`, `bool BarbarianCharged`.
- Methods:
  - `SetOwner(BaseMatchProfile newOwner)` — updates Owner.
  - `Reveal()` — sets IsRevealed = true.
  - `CanBeClaimed()` — returns false for Mountain and Desolate.

### Step 3.3 — BoardSettings
**File:** `Scripts/Board/BoardSettings.cs`

- Plain C# class, created before each match.
- Fields: `int Width`, `int Height`, `Dictionary<TileType, float> TileProbabilities`.
- Static factory: `BoardSettings.ForMatch(int matchNumber)` — scales board size and special tile probability based on run progression (early matches: mostly Plains; later matches: more special tiles).

### Step 3.4 — BoardData
**File:** `Scripts/Board/BoardData.cs`

- Plain C# class, source of truth for all tile state.
- Core: `Dictionary<Vector3Int, TileData> _tiles`.
- Methods:
  - `SetTile(Vector3Int, TileData)`, `GetTile(Vector3Int)` — basic access.
  - `AllCells()` — iterates all cells.
  - `GetAdjacentCells(Vector3Int)` — returns the 4 cardinal neighbors that exist on the board.
  - `AssignStartingTiles(List<BaseMatchProfile> players)` — finds plain tiles as far apart as possible, calls `tile.SetOwner(player)` for each.
  - `GetClaimableTilesFor(BaseMatchProfile player)` — computes valid claim targets based on owned tiles + special tile rules (see Phase 7).
  - `GetVisibleTilesFor(BaseMatchProfile player)` — computes all tiles revealed by the player's owned tiles (used for Fog of War).

### Step 3.5 — BoardGenerator
**File:** `Scripts/Board/BoardGenerator.cs`

- Plain C# class, no Unity dependencies.
- `Generate(BoardSettings settings)` → returns a `BoardData`.
- Uses weighted random selection from `settings.TileProbabilities` to assign each cell a `TileType`.
- Ensures Mountains and Desolate tiles are placed reasonably (not blocking the entire board).
- Does not assign owners or reveal tiles — that happens in GameBootstrapper.

---

## Phase 4 — Rendering & Input

### Step 4.1 — Unity Scene Setup (GameScene)
- Create a `Grid` GameObject in the scene hierarchy.
- Add four child `Tilemap` GameObjects in this order (bottom to top):
  1. `TerrainTilemap`
  2. `OwnershipTilemap`
  3. `HighlightTilemap`
  4. `UnitsTilemap` (empty/reserved)
- Assign placeholder `Tile` assets (solid colored sprites) for each tile type and owner color.

### Step 4.2 — BoardRenderer
**File:** `Scripts/Board/BoardRenderer.cs`

- MonoBehaviour, attached to the Grid GameObject.
- Holds serialized references to all four Tilemaps.
- `Render(BoardData board)` — initial paint: sets TerrainTilemap sprites from TileData.Type; applies fog (hides unrevealed tiles).
- Subscribes to `EventManager.OnTileCaptured` → repaints OwnershipTilemap for that cell.
- `UpdateHighlights(List<Vector3Int> claimable)` — clears HighlightTilemap, paints highlight sprites on valid targets.
- `ClearHighlights()` — clears HighlightTilemap.
- `UpdateFog(List<Vector3Int> revealedCells)` — updates TerrainTilemap to show newly revealed tiles.
- Never reads from Tilemaps for logic — only writes to them.

### Step 4.3 — BoardInput
**File:** `Scripts/Input/BoardInput.cs`

- MonoBehaviour.
- Detects mouse click on the grid via `Camera.main.ScreenToWorldPoint` + `Tilemap.WorldToCell`.
- On hover: fires an internal event with the hovered cell.
- On click: calls `PlayerTurnState.TryClaimTile(cell)` if it's the player's turn.
- Does not contain game logic; only translates input to game requests.

---

## Phase 5 — Game States

### Step 5.1 — PlayerTurnState
**File:** `Scripts/States/PlayerTurnState.cs`

- Implements `IGameState`. Plain C# (no MonoBehaviour).
- Constructor takes `GameManager`, `PlayerMatchData`, `BoardData`.
- `Enter()`:
  - Calls `player.ResetActions()`.
  - Fires `EventManager.OnTurnStarted`.
  - Computes claimable tiles via `board.GetClaimableTilesFor(player)`.
  - Fires highlight update to `BoardRenderer`.
  - Enables `BoardInput`.
- `Tick()`: Checks `player.ActionsRemaining <= 0` → calls `GameManager.TransitionTo(CheckWinState)`.
- `TryClaimTile(Vector3Int cell)`:
  - Validates cell is in claimable range.
  - Calls `player.ClaimTile(cell, board)`.
  - Calls `player.SpendAction()`.
  - Handles special tile behavior (see Phase 7).
  - Recomputes claimable tiles and updates highlights.
- `Exit()`:
  - Fires `EventManager.OnTurnEnded`.
  - Clears highlights.
  - Disables `BoardInput`.

### Step 5.2 — EnemyTurnState
**File:** `Scripts/States/EnemyTurnState.cs`

- Implements `IGameState`. Plain C# (no MonoBehaviour).
- Constructor takes `GameManager`, `AIMatchData`, `AIController`, `BoardData`.
- `Enter()`:
  - Calls `ai.ResetActions()`.
  - Fires `EventManager.OnTurnStarted`.
  - Calls `AIController.TakeTurn(ai, board)` (synchronous for now; can add delay via coroutine later for visual pacing).
  - After AI is done, calls `GameManager.TransitionTo(CheckWinState)`.
- `Tick()`: No-op (AI acts synchronously in Enter for now).
- `Exit()`: Fires `EventManager.OnTurnEnded`.

### Step 5.3 — CheckWinState
**File:** `Scripts/States/CheckWinState.cs`

- Implements `IGameState`. Plain C# (no MonoBehaviour).
- `Enter()`:
  - Counts all claimable tiles (non-Mountain, non-Desolate).
  - Counts how many each player owns.
  - If all claimable tiles are owned → determine winner by majority → `GameManager.TransitionTo(GameOverState)`.
  - Otherwise: if last active player was the human → `TransitionTo(EnemyTurnState)`.
  - If last active player was AI → `TransitionTo(PlayerTurnState)`.
- `Tick()`: No-op.
- `Exit()`: No-op.

### Step 5.4 — GameOverState
**File:** `Scripts/States/GameOverState.cs`

- Implements `IGameState`. Plain C# (no MonoBehaviour).
- Constructor takes `GameManager`, `PlayerMatchData`, `PlayerRunData`, `PlayerAccountProfile`.
- `Enter()`:
  - Determines win/loss for the human player.
  - If loss: calls `playerRunData.LoseLife()`.
  - Updates `PlayerAccountProfile` stats.
  - Saves via `SaveManager.Save(accountProfile)`.
  - Checks achievements via `AchievementManager`.
  - If `playerRunData.IsRunOver()` → load RunEndScene.
  - Otherwise → load MetaScene (every match) or directly to next GameScene (if MetaScene logic says no perk this match).
- `Tick()` / `Exit()`: No-op.

---

## Phase 6 — AI System

### Step 6.1 — AIController
**File:** `Scripts/Players/AIController.cs`

- Plain C# class. Receives `AIMatchData` and `BoardData`; no other dependencies.
- `TakeTurn(AIMatchData ai, BoardData board)`:
  - Loops while `ai.ActionsRemaining > 0`.
  - Calls the appropriate strategy method based on `ai.Difficulty`.
- **Easy** — `GreedyMove()`: score each claimable tile by number of tiles it would add to the AI's adjacent frontier; claim the highest scorer.
- **Medium** — `GreedyDefensiveMove()`: same as Easy but also scores tiles that block the player's highest-value expansion.
- **Hard** — `LookaheadMove()`: evaluates sequences of 2 moves, picks the first move of the best sequence. Prioritizes map control percentage.

---

## Phase 7 — Special Tile Behaviors

All special tile logic lives in `BoardData.GetClaimableTilesFor()` and `BoardData.GetVisibleTilesFor()`, plus a `TileEffectHandler` for special behaviors that trigger on claim.

### Step 7.1 — Visibility Rules (integrated into GetVisibleTilesFor)
For each tile a player owns, add cells to the revealed set based on type:

| Tile | Reveals |
|---|---|
| Forest | 1 tile in each cardinal direction |
| Plains | 2 tiles in each cardinal direction + 4 diagonal corners |
| Tower | 3 tiles in each cardinal direction + diamond-edge tiles |
| Cave | 1 tile in each cardinal direction + all other Caves on the map |
| Mountain | Nothing (unclaimed) |
| Wizard | 1 tile in each cardinal direction |
| Barbarian | 1 tile in each cardinal direction (before charging) |

### Step 7.2 — Claim Range Rules (integrated into GetClaimableTilesFor)
| Tile | Can Claim |
|---|---|
| Forest | 1 tile in any cardinal direction |
| Plains | 2 tiles in one cardinal direction, OR 1 cardinal + 1 adjacent diagonal |
| Tower | Only the 3rd tile in a cardinal direction, OR edge tiles of the revealed diamond |
| Cave | 1 tile in any cardinal direction + teleport to any unclaimed Cave (if CaveUsed=false) |
| Wizard | 1 tile in any cardinal direction + any unclaimed tile anywhere (if WizardUsed=false) |
| Barbarian | 1 tile in any cardinal direction |

### Step 7.3 — TileEffectHandler
**File:** `Scripts/Board/TileEffectHandler.cs`

- Static class called by `PlayerTurnState.TryClaimTile()` after a tile is claimed.
- `HandleClaimEffect(TileData tile, BoardData board)`:
  - **Cave**: if player used cave teleport, sets `CaveUsed = true` on both origin and destination cave.
  - **Wizard**: if player used global claim, sets `WizardUsed = true` on the wizard tile.
  - **Barbarian**: if tile was just revealed (first claim of an adjacent tile), triggers `BarbarianCharge(cell, board)`.
- `BarbarianCharge(Vector3Int cell, BoardData board)`:
  - Determines longer board axis (horizontal or vertical).
  - Iterates all tiles in that row/column.
  - Calls `tile.SetOwner(null)` on each (unclaims for all players), fires `OnTileCaptured` for each.
  - Sets `BarbarianCharged = true` on the barbarian tile.

---

## Phase 8 — UI

### Step 8.1 — BaseButton
**File:** `Scripts/UI/BaseButton.cs`

- Abstract MonoBehaviour implementing `IPointerEnterHandler`, `IPointerExitHandler`, `IPointerClickHandler`.
- Protected virtual methods: `OnHoverEnter()`, `OnHoverExit()`, `OnClick()`.
- `SetInteractable(bool)` — enables/disables interaction and updates visual state.

### Step 8.2 — EndTurnButton
**File:** `Scripts/UI/EndTurnButton.cs`

- Extends `BaseButton`.
- `OnClick()` → calls `GameManager.Instance.TransitionTo(CheckWinState)`.
- Only interactable during `PlayerTurnState`.

### Step 8.3 — CaptureTileButton (optional, if click-to-confirm flow is used)
**File:** `Scripts/UI/CaptureTileButton.cs`

- Extends `BaseButton`.
- Displays selected tile info.
- `OnClick()` → opens confirm dialog. On confirm, fires `EventManager.OnTileCaptured` with the pending cell.

### Step 8.4 — PerkButton
**File:** `Scripts/UI/PerkButton.cs`

- Extends `BaseButton`.
- Holds a `PerkData` reference; displays name and description via TextMeshPro.
- `OnClick()` → fires `EventManager.OnPerkSelected(perkData)`.

### Step 8.5 — UIManager
**File:** `Scripts/UI/UIManager.cs`

- MonoBehaviour singleton.
- Subscribes to `EventManager.OnNumActionsChanged` → updates action counter display.
- Subscribes to `EventManager.OnGameOver` → shows game over panel.
- Manages showing/hiding: action counter, end turn button, confirm dialog, game over screen.

---

## Phase 9 — Roguelite Meta Layer

### Step 9.1 — PerkData
**File:** `Scripts/Meta/PerkData.cs`

- ScriptableObject.
- Fields: `string DisplayName`, `string Description`, `int BonusActionsPerTurn`, `int BonusStartingTiles`, (other modifiers as needed).
- Create at least 6 placeholder perks as `.asset` files.

### Step 9.2 — PerkLibrary
**File:** `Scripts/Meta/PerkLibrary.cs`

- ScriptableObject (or plain static class with a serialized list).
- Holds `List<PerkData> AllPerks`.
- `GetRandomOffer(int count, List<PerkData> exclude)` — returns N perks not already held.

### Step 9.3 — MetaScene
- Create `MetaScene.unity` in `Assets/Scenes/`.
- MetaScene controller MonoBehaviour:
  - On load, reads `SessionData.ActiveRun`.
  - Calls `PerkLibrary.GetRandomOffer(3, currentPerks)`.
  - Instantiates 3 `PerkButton`s with the offered perks.
  - On `EventManager.OnPerkSelected`: adds perk to `PlayerRunData.ActivePerks`, loads GameScene.
  - Shows current run stats (lives remaining, match number, perks held).

### Step 9.4 — Perk Application
- In `GameBootstrapper`, after creating `PlayerMatchData`, apply active perks from `PlayerRunData.ActivePerks`:
  - `BonusActionsPerTurn` → add to `player.ActionsPerTurn`.
  - Other modifiers as perks are designed.
- This keeps perk logic out of the state machine.

---

## Phase 10 — Scenes & Navigation

### Step 10.1 — MainMenuScene
- Buttons: New Run, Continue (if save exists), Settings, Quit.
- Continue loads `PlayerAccountProfile` and creates a fresh `PlayerRunData`, setting `SessionData.ActiveRun`.

### Step 10.2 — GameSelectionScene
- Allows choosing campaign / endless / challenge.
- Sets difficulty on `SessionData` for `AIMatchData` constructor.
- For now, implement Campaign only; stub out others.

### Step 10.3 — Scene Load Sequence
```
MainMenuScene
  → (New Run) → GameSelectionScene → GameScene
  → (Match end) → MetaScene (if match % 3 != 0, just show stats)
                → MetaScene with perk offer (every 3 matches)
  → (Run over) → RunEndScene (simple win/loss screen, back to MainMenu)
```

---

## Phase 11 — Polish & Secondary Systems

These are deferred until the core loop is playable.

### Step 11.1 — AchievementManager
**File:** `Scripts/Persistence/AchievementManager.cs`

- Static class.
- `Check(PlayerAccountProfile profile)` — evaluates achievement conditions, unlocks new achievements, adds to `profile.UnlockedPerks` if applicable.
- Start with 3 placeholder achievements.

### Step 11.2 — AnimationSystem
**File:** `Scripts/Visuals/AnimationSystem.cs`

- MonoBehaviour.
- Subscribes to `OnTileCaptured` → plays a simple tile-flash animation.
- Keep it fully decoupled from logic; only reacts to events.

### Step 11.3 — SoundManager
**File:** `Scripts/Visuals/SoundManager.cs`

- MonoBehaviour singleton.
- Subscribes to events and plays AudioClips.
- Placeholder: log "SFX: [event name]" until real audio assets are provided.

### Step 11.4 — Board Scaling
- Update `BoardSettings.ForMatch(matchNumber)` to increase Width/Height and shift `TileProbabilities` toward special tiles as `matchNumber` increases.
- Tune weights after the core loop is playable.

---

## Build Order Summary

| Phase | What Gets Built | Playable Milestone |
|---|---|---|
| 1 | EventManager, IGameState, GameManager, GameBootstrapper, SessionData | — |
| 2 | All player/account data classes, SaveManager | — |
| 3 | TileData, BoardSettings, BoardData, BoardGenerator | — |
| 4 | Scene setup, BoardRenderer, BoardInput | Board renders on screen |
| 5 | All four game states | **Full match loop runs** |
| 6 | AIController | AI takes turns |
| 7 | Special tile logic, TileEffectHandler | All tile types work |
| 8 | All UI classes | UI is functional |
| 9 | Perk system, MetaScene | **Full run loop works** |
| 10 | All scenes wired up | **Game is end-to-end playable** |
| 11 | Achievements, animation, sound, board scaling | Polished experience |
