# Possible per-map settings (`[mapSettings]` in Level.e2e)

Research result: **how much custom behaviour can we attach to a custom map?**
Answer: a *lot*. The game funnels almost all per-prison tuning through a small
number of objects that exist exactly once per loaded level and are populated at
load time:

| Object | What it owns | Where custom maps get it |
|---|---|---|
| `RoutineManager` (singleton) | clock speed, elapsed time, current routine, calendar | scene component; `StartInit()` pulls `RoutineConfig` from `ConfigManager` |
| `RoutinesData` (ScriptableObject) | routine list, ambience event, sunrise/sunset, purple-door windows, container refresh hour, timed-prison mode | built fresh for custom maps in `LevelSetup_RoutineManager.Setup()` from the editor's 24-slot schedule |
| `PrisonConfig` (ScriptableObject) | hub: `CharacterConfig` ×5, `AIConfig`, `JobConfig`, `VendorConfig`, `OpinionConfig`, `QuestConfig`, desk `ItemContainerConfig`s, item overrides | `GlobalStart` (case `WAIT_FOR_OTHER_SCENES_IGM`, `m_CustomLevel` branch) assembles a `PrisonData` clone and sets `LevelScript.m_LevelSetup`; `ConfigManager.SetActiveConfig()` selects it |
| Scene components | generators, CCTV, contraband detectors, guard towers, lighting, solitary | spawned while the instruction log replays |

Our sidecar (`Level.e2e`, see `src/E2EApi/Persistence/ModExtras.cs`) is already
loaded in a postfix on `SaveManager.LoadTheLevel` — i.e. **before** any of the
above run. So the implementation pattern for everything below is:

1. Map author sets values in our editor UI → saved as `key=value` lines under a
   `[mapSettings]` section in `Level.e2e`.
2. At play time, Harmony postfixes on the listed setup methods read
   `ModExtras.Current` and overwrite the listed fields.

**One important caveat (applies to every `*Config` setting):** in the
non-preview play path, `GlobalStart` does
`prisonData.m_Configs.AddRange(LevelDataManager.m_CustomPrisonConfigs)` —
it does **not** `Instantiate()` the configs (only the preview path does). The
ScriptableObject assets are shared, so any mutation survives until the assets
are reloaded. Our patcher must either clone the config before mutating
(mirroring the preview path) or snapshot + restore original values on level
unload. With that handled, everything below is safe.

Confidence legend: **verified** = read the decompiled code end-to-end;
**likely** = fields found, consumer partially traced; **speculative** = idea
plausible but unproven.

---

## 1. High confidence, easy wins

### 1.1 Time & clock

The entire in-game clock is one accumulator in `RoutineManager.ProcessTime()`:
`m_ElapsedInGameSeconds += UpdateManager.deltaTime * GetCurrentGameSecondsPerRealSecond()`.

`GetCurrentGameSecondsPerRealSecond()` returns
`m_GameSecondsPerRealSecond * GetFastForwardFactor()`, and
`m_GameSecondsPerRealSecond` is computed **once** in `RoutineManager.Awake()` as
`1f / (m_RealLifeSecondPerGameMinute / 60f)` (default `m_RealLifeSecondPerGameMinute = 5f`,
i.e. 12 game-seconds per real second).

| Setting | Class / field | Patch | Sidecar key | Confidence |
|---|---|---|---|---|
| Day-cycle speed | `RoutineManager.m_RealLifeSecondPerGameMinute` (+ recompute private `m_GameSecondsPerRealSecond`) | postfix `RoutineManager.Awake`, set both fields (Traverse/AccessTools for the private one). Alternative: postfix `GetCurrentGameSecondsPerRealSecond` and multiply the result — catches every consumer | `timeScale=2.0` (multiplier) or `secondsPerGameMinute=2.5` | **verified** |
| Sleep fast-forward factor | `RoutineManager.m_AllSleepingFastForwardFactor` (default 100, public) | set in postfix on `RoutineManager.StartInit` | `sleepFastForward=200` | **verified** |
| Debug-style fast forward | `RoutineManager.m_bFastForward` / `m_FastForwardFactor` (public) | runtime toggle, e.g. for a "hyper mode" map | `fastForwardFactor=…` | **verified** |
| Freeze time | `RoutineManager.m_bFreezeTime` (private; `SetTimeFrozenRPC(bool)` exists and syncs MP) | call `SetTimeFrozenRPC(true)` after load — frozen-clock puzzle maps | `freezeTime=true` | **verified** |
| Start time of day | `RoutinesData.m_StartOfTheDayHour/­Minutes` — `SetInitialTime()` uses it on first load (custom maps hardcode `:45` of the hour before first non-lights-out routine in `LevelSetup_RoutineManager.Setup`) | postfix `LevelSetup_RoutineManager.Setup`, rewrite the two fields before `StartInit` consumes them | `startHour=22`, `startMinute=0` (night-start map!) | **verified** |
| Timed prison (escape deadline) | `RoutinesData.m_bIsTimedPrison`, `m_TimedHoursDuration`, `m_TimedMinutesDuration` → `SetupTimedPrisonCallback()` ends the level via `m_TimesUpCutscene` | set the three fields in `LevelSetup_RoutineManager.Setup` postfix (must run before `RoutineManager.StartInit`; the LevelSetup runs at `SetupPriority.Priority_9`, which precedes manager init) | `timedPrison=48h` | **verified** |
| Calendar events | `RoutineManager.m_CalendarEvents` — `DayType[30]`: `NormalDay/GarbageDay/HelicopterDay/BoatDay`; `GetCurrentDayType()` = `day % 30` | overwrite array in `StartInit` postfix — schedule garbage-truck or helicopter escape days on custom maps | `calendar=N,N,G,N,H,…` | **verified** (array + getter; consumers of DayType not fully traced → escape-day behaviour itself **likely**) |

### 1.2 Ambient sound & music (the big question)

All audio is **Wwise** (`AkSoundEngine`), addressed by *event name strings* from
the `AUTOGEN_T17Wwise_Enums.Events` enum (~1,300 events — every prison's
ambience + 7 routine-music slots per prison theme, transports, DLC themes,
stingers).

How a map's audio is chosen (verified end-to-end):

- **Ambience**: `RoutinesData.m_Ambience` (e.g. `Play_Prison_01_Ambience_General`).
  `RoutineManager.SetInitialTime()` → `AudioController.PlayPrisonAmbience(m_RoutinesData.m_Ambience)`;
  stopped in `OnDestroy`.
- **Music**: each `RoutinesData.Routine` has `m_RoutineMusic` (an `Events`
  value). On routine change, `RoutineManager.UpdateRoutine()` pauses the old
  event and `AudioController.PlayRoutineMusic(newEvent)` (the controller
  derives `Pause_`/`Resume_`/`Stop_` names by string replace).
- **Custom maps**: the editor already stores a per-map `m_MusicType`
  (`PRISON_ENUM`, serialized flag 20 in Level.dat) and
  `LevelSetup_RoutineManager.GetCorrectMusic()` remaps the Prison_01 defaults —
  but vanilla only ever maps to Prison_01 or Prison_03!

| Setting | Patch | Sidecar key | Confidence |
|---|---|---|---|
| Per-map ambience (any of the ~20 `*_Ambience_*` events: jungle POW camp, space station hum, oil rig, train…) | postfix `LevelSetup_RoutineManager.Setup`: set `routineManager.m_RoutinesData.m_Ambience` to any `Events` value (parse from string name) | `ambience=Play_Prison_05_Ambience_General` | **verified** |
| Per-routine music tracks | same postfix: rewrite `m_RoutineMusic` on each `RoutinesData.Routine` (and `m_SaveLoadRoutineMusic`); any prison theme, any routine slot — e.g. Area 17 escape music during free time | `routineMusic=FreeTime:Play_Music_Prison_04_Routine_C` | **verified** |
| Whole music theme beyond the two vanilla choices | bypass `GetCorrectMusic()`'s hardcoded 01→03 mapping; substitute any `Prison_0X`/transport/DLC family | `musicTheme=Prison_05` | **verified** |
| One-shot Wwise events as map triggers (sirens, stingers, dog barks) — pairs with our existing trigger links | `AudioController.SendEvent(SOUND_AREA.SA_INGAME, "<eventName>", obj)` accepts **raw strings**, so any of the ~1,300 events is playable on demand | `[triggers] … action=sound:Play_Alarm_…` | **verified** (API); event list audit pending |
| **Custom audio files (mp3/ogg from disk)** | **Not possible through Wwise** — events must exist in the shipped soundbanks; `AkSoundEngine` has no API to register loose files. Workaround: mod-side `UnityEngine.AudioSource` + `WWW`/`AudioClip` loading, played outside Wwise (ignores the game's volume sliders unless we mirror `AudioController.m_MusicVolume`). Ship the file in the map's save folder next to Level.e2e. | `customMusic=mysong.ogg` | **verified impossible in Wwise**; Unity-side workaround **likely** (standard Unity 5.5 API, not yet prototyped) |

### 1.3 Routine schedule & lockdown

Custom maps currently get a coarse 24×1-hour schedule. We can post-process the
generated `RoutinesData` for much finer control.

| Setting | Class / field | Sidecar key | Confidence |
|---|---|---|---|
| Minute-resolution routines (e.g. 7:15–7:45 rollcall) | `RoutinesData.Routine.m_StartHour/m_StartMinutes/m_EndHour/m_EndMinutes` | `routine=RollCall:07:15-07:45` | **verified** |
| Heat/alertness penalty for missing a routine | `Routine.m_AddedHeatWhenMissed` (0–100), `m_AddedAlertnessWhenMissed` (0–11), `m_PostLockdownAlertness` | `missedRoutineHeat=…` | **verified** (fields; consumers in AI event code **likely**) |
| Grace period to reach a routine | `Routine.m_TimeToGetToRoutine` (default 12 game-min) | `routineGraceMinutes=5` | **verified** |
| Purple-door open windows | `RoutinesData.m_PurpleDoorControllers` (list of time ranges; custom maps auto-generate one per non-lights-out routine) | `purpleDoors=08:00-18:00` | **verified** |
| Lockdown duration | `SolitaryManager.m_LockdownDuration` (180 game-min) / `m_MiniLockdownDuration` (45) | `lockdownMinutes=300` | **verified** |
| Solitary sentence lengths | `SolitaryManager.m_SolitarySetupInfo` — list of `{TimesSentToSolitary, Duration}` (escalating); `m_TaskCompleteReduction` (potato-mash time credit) | `solitary=1:30,2:60,3:120` | **verified** |
| Starting alertness ("star rating") | `PrisonAlertnessManager.m_StartingAlertness`, plus `m_MorningRollCallReduction` (how fast it cools) | `startingAlertness=3` | **verified** |
| Desk/container restock time | `RoutinesData.m_ItemContainerRefreshHour/­Minute` → daily `AllItemContainerRefresh()` | `containerRefreshHour=3` | **verified** |

### 1.4 Player / NPC stats (per-role)

`Character` init resolves a `CharacterConfig` per role from the active
`PrisonConfig` (`m_PlayerConfig`, `m_InmateConfig`, `m_GuardConfig`,
`m_RiotGuardConfig` — used at 10+ alertness, `m_DogConfig`) and calls
`CharacterStats.ApplyCharacterConfig`. Mutating those configs at load time
re-tunes every spawn.

| Setting | Field (on `CharacterConfig`) | Sidecar key | Confidence |
|---|---|---|---|
| Starting money | `m_MoneyBaseLine` | `playerMoney=500` | **verified** |
| Starting health/strength/cardio/intellect/energy/heat | `m_*BaseLine` | `playerStats=hp:100,str:80,…` | **verified** |
| Sentence length (days shown on HUD) | `m_SentenceBaseLine` | `sentenceDays=999` | **verified** |
| Health regen rate | `m_HealthRestoreRate` (per second) | `healthRegen=0` (hardcore!) | **verified** |
| Stamina (energy) regen, incl. while blocking | `m_EnergyRestoreRate`, `m_EnergyRestoreRateBlocking` | `energyRegen=…` | **verified** |
| Stat decay (gym/library grind decay) | `m_StrengthDecayRate`, `m_IntellectDecayRate`, `m_CardioDecayRate` | `statDecay=0` | **verified** |
| Heat decay (how fast guards forget) | `m_HeatDecayRate` | `heatDecay=0.1` | **verified** |
| Separate values for inmates / guards / riot guards / dogs | same fields on the other four configs | `guard.healthRegen=…` etc. | **verified** |
| Movement speed | `CharacterMovement.m_fMaxSpeed` (5), `m_fMaxSpeedDashing` (8), `m_fMaxSpeedBlocking` (1) — per character component, not config | postfix character spawn (we already hook spawns for fences) or iterate `Character` list on `GameEvents.LevelLoaded` | `moveSpeed=1.5x` | **verified** (fields; full speed pipeline incl. `CharacterStats.m_SpeedMod`/cardio boost **likely**) |
| NPC vision range | `Character.m_fVisionDistance` (10), `m_fTouchingVisionRadius` (0.7) — used by `CharacterUtil` vision cone | same spawn hook | `guardVision=6` (stealth maps) | **verified** |

### 1.5 Guard AI / heat / detection (per-map `AIConfig`)

All on the active `PrisonConfig.m_AIConfig` (private serialized fields →
AccessTools):

| Setting | Field | Sidecar key | Confidence |
|---|---|---|---|
| Heat for being seen suspicious / wanted | `m_GuardSuspiciousHeat`, `m_GuardWantedHeat` | `suspiciousHeat=…` | **verified** |
| Guard follow/ignore times when suspicious | `m_GuardSuspiciousFollowMin/MaxTime`, `…IgnoreMin/MaxTime` | `guardFollowTime=…` | **verified** |
| Rollcall desk searches | `ChanceToSearchPlayerDesk` (0.05), `NumberOfDesksToSearch` (2) | `deskSearchChance=0.5` | **verified** |
| Lights-out punishment | `AlertnessIncreaseAtLightsout`, `HeatIncreaseAtLightsout` (100) | `lightsOutHeat=20` | **verified** |
| Dog patrol behaviour | `MaxDogPauseTime`, `DogPauseReduction`, `DogLoveOpinion` (60), `DogContrabandDetectionHeat` (100) | `dogContrabandHeat=…` | **verified** |
| Inmate snitching | `InmateSnitchLikeOpinion` (75), `InmateSnitchToGuardMaxDistance` (20) | `snitchDistance=0` (no snitches) | **verified** |
| Disguise strictness | `DisguiseableEvents` list, `DisguiseBreakHeat` (80) | `disguiseBreakHeat=…` | **verified** |
| Bed-sheet-rope takedown alertness | `TakeDownBedSheetAlertness` | `bedsheetAlertness=…` | **verified** |
| NPC random fight frequency | `RandomAttackMinTime`/`MaxTime` (per-personality `FloatSetting`) | `npcAggression=2x` | **verified** (fields; per-personality scaling adds complexity) |

### 1.6 Economy, jobs, shops, favours

| Setting | Class / field | Sidecar key | Confidence |
|---|---|---|---|
| Job pay | `JobConfig.m_MoneyReward` (10) | `jobPay=25` | **verified** |
| Job lateness window + missed-job heat | `JobConfig.m_CharacterLateTime` (30), `m_MissedJobOfficerHeatIncrease` (10) | `jobLateMinutes=…` | **verified** |
| Shop (vendor) prices | `VendorConfig.m_ItemCostModifier` (0–2×) | `shopPriceModifier=0.5` | **verified** |
| Shop stock size & quality | `m_MinItems`/`m_MaxItems` (0–12), `m_PossibleItemSets` (weighted item groups → full shop inventory control) | `shopMinItems=…`, `shopItems=…` | **verified** (fields; item-set construction needs item-ID plumbing → that part medium) |
| Vendor availability | `m_RequiredOpinion`, `m_MinVendorDuration`/`m_MaxVendorDuration` (game-min), `m_MaxVendors` (vanilla custom maps force `inmates/2`) | `maxVendors=10` | **verified** |
| Favour/quest density | `QuestConfig.m_MaxPercentageQuestGivers` (20%), `m_TimeInHoursBeforeInmatesHaveQuests` (1h), `m_bAllowSpecificQuests` | `questGiverPercent=50` | **verified** |
| Quest pool override | `QuestConfig.m_OverrideQuests` + `PrisonConfig.m_QuestOverrides` (list of `QuestManager.QuestType`) | `questPool=…` | **likely** (lists verified, QuestManager consumption not fully traced) |
| Opinion system tuning | `OpinionConfig.m_LowOpinionThreshold`/`m_HighOpinionThreshold`, `m_ItemGiftValueModifier`, `m_InitialOpinionOfPlayers` | `giftValueModifier=2.0` | **verified** |

### 1.7 Security hardware (scene-object sweep at level load)

These are per-instance components — patch pattern: on `GameEvents.LevelLoaded`,
`Object.FindObjectsOfType<T>()` and overwrite fields (same approach our
electric-fence feature already uses).

| Setting | Class / field | Sidecar key | Confidence |
|---|---|---|---|
| Generator reset time (power-cut duration) | `Generator.m_InactiveTime` (30s real time) | `generatorDowntime=120` | **verified** |
| Electric fence damage/knockback | `ElectricFence.m_fDamage` (20), `m_fKnockBack` (5), anim timers | `fenceDamage=50` | **verified** (we already manage these objects) |
| Contraband detector alert hold | `ContrabandDetector.m_StayAlertedTime` (5s) + alert light colours | `detectorAlertTime=…` | **verified** |
| Contraband pouch immunity | `ContrabandDetector.m_RegularContrabandPouch` / `m_DurableContrabandPouch` (which items bypass it) | `detectorIgnoresPouch=false` | **verified** (fields; swap logic medium) |
| CCTV sweep behaviour | `CCTVCamera.m_Speed` (1), `m_RestTime` (3), `m_Fov`, `m_VisionDistance`, `m_MaxRotationAngle`, `m_BlindspotDistance` (1), `m_HeatIncrease`, `m_AlertnessIncrease` | `cctvSpeed=2`, `cctvHeat=…` | **verified** |
| Guard-tower snipers | `GuardTowerManager.m_TimeBetweenShots` (3s), `m_DamagePerShot` (40), `m_ShootingHeatTolerance` (70 — heat at which they open fire) | `sniperDamage=100`, `sniperHeatThreshold=40` | **verified** |
| Tower spotlight | `GuardTowerSpotlight.m_Size` (50), `m_Intensity`, `m_Colour`, `m_PatrolSpeed` (1), `m_FollowSpeed` (5) | `spotlightSpeed=…` | **verified** |
| Spotlight hours | `RoutinesData.m_SpotlightsStartHour/…EndHour` | `spotlightHours=18:30-06:30` | **verified** |

### 1.8 Combat

Active `PrisonConfig.m_CombatConfig` (`GlobalCombatConfig`):

| Setting | Field | Sidecar key | Confidence |
|---|---|---|---|
| Unarmed damage profile | `m_UnarmedCombatConfig` (`Item_Combat`) | `unarmedDamage=…` | **likely** (Item_Combat internals not fully read) |
| Hit ranges | `m_fCombatNearHitDistance` (0.9), `m_fCombatDoggieNearHitDistance` (3) | `meleeRange=…` | **verified** |
| Charge-attack timings | `m_fSmashAttackFullChargeTime`, `…DashTime`, `…AttackTime`, `…CommitTime` | `smashChargeTime=…` | **verified** |
| Knockback/stun | `m_fKnockBackStunTime` (0.2), `m_fKnockBackPowerOnDamage` (4), `…OnBlock` (2) | `knockback=…` | **verified** |

---

## 2. Medium effort

### 2.1 Day/night visuals & lighting

`LightingManager` holds six `LightingPeriod` structs (`m_DawnLight`,
`m_DayLight`, `m_DuskLight`, `m_NightLight`, `m_UnderGroundLight`,
`m_VentsLight`), each with **indoor/outdoor ambient color + intensity,
directional light color/angle/height, shadow color, fog color/density**. It
interpolates between them using `RoutinesData`'s sunrise/sunset windows.

- Sunrise/sunset/spotlight times: `RoutinesData.m_SunRiseStartHour…m_SunSetEndMinutes`
  → trivially patchable in the `LevelSetup_RoutineManager.Setup` postfix.
  `sunset=15:30-18:30` — **verified**.
- Permanent night / blood-red sky / horror fog: overwrite `LightingPeriod`
  members on `GameEvents.LevelLoaded`. Colors serialize fine as `r,g,b`.
  `nightAmbient=0.1,0.1,0.3`, `fogDensity=0.05` — **verified fields**, but the
  exact look needs in-game iteration, and `OnLightingPreCalc` event suggests a
  cleaner hook → medium.
- Per-light-group schedules: `LightingManager.LightGroup.m_Times`
  (`TimeOnOff` windows, `m_bAlwaysOn`) — lets a map turn whole light groups on
  a custom clock. **verified**, medium (need stable group IDs per map).

### 2.2 Weather / snow

`WeatherEffectManager` (scene singleton) renders up to 5 fullscreen tiled
effects (`WeatherEffectData`: texture, scroll curves, alpha curve, scale,
audio on/off events, rumble). This is how snow/rain/sandstorm themes work in
the themed prisons.

- The custom-prison scene includes the manager but (presumably) no enabled
  effects; `m_EffectEnabled` is a plain bool per slot.
- Plan: at level load, enable slot 0 and populate `m_Texture` +
  curves — either copied from a shipped effect (load the relevant prison's
  assets?) or generated (simple snow-dot `Texture2D` created at runtime works
  on Unity 5.5).
- `snow=true`, `weatherAudio=Play_Amb_Wind…`
- Confidence: **likely** for enabling/feeding the renderer (renderer code
  `WeatherObjectRenderer` reads `WeatherEffectManager.Instance.GetEffectData()`
  every frame — verified); **speculative** for whether the custom-map scene
  actually carries the manager prefab — needs an in-game check, else we
  instantiate it ourselves.

### 2.3 Population & roles

Guard/inmate counts are vanilla per-map settings (sbyte each, serialized flags
12/13), consumed in `GlobalStart` → `prisonData.m_CustomisableRoles[2]`.

- Exceed the editor's slider limits (or set asymmetric extremes): postfix the
  `GlobalStart` block (or `LevelDetailsManager.GetNumberOfGuards/Inmates`) and
  return sidecar values. `guards=40`, `inmates=2` — **likely** (consumption
  verified; NPC spawner ceilings / seat counts like `EMPTY_SEAT_COUNT = 30`
  in `NPCManager` may clamp in practice — needs testing).
- **Dog patrols on custom maps**: `m_DogConfig` and dog AI all exist
  (`AICharacter_Dog`, `CanineJobInit`); whether custom maps can spawn dogs
  depends on dog spawn markers/jobs existing in the block set → tied to our
  block-unlock work. **speculative**.
- **Robinson (story favour-giver)**: `GlobalStart` hardcodes
  `prisonData.m_bAddRobinsonCharacter = false` for custom maps — a one-line
  postfix could re-enable him (`robinson=true`). Untested knock-on effects on
  quest lines. **speculative**.

### 2.4 Items & loot

- **Desk loot tables**: `GlobalStart` builds a fresh `ItemContainerConfig` for
  custom maps from `DifficultySettings` + the 5 chosen `ItemGroupSetting`s.
  Postfix it to inject arbitrary `m_StartingItems` / `m_RandomGroups` /
  `m_RandomPercentages` → full control of what spawns in inmate/guard desks.
  `deskLoot=…` — **verified** structure; item-ID plumbing = the work.
- **Per-map item overrides**: `PrisonConfig.m_ItemDataOverrides`
  (`ItemDataConfig`: `m_ItemValue`, `m_ItemHealth`, **`m_bIsContraband`**,
  combat data, armour, outfit). Make guard outfits non-contraband on a
  social-stealth map, make a wrench a super-weapon, re-price everything.
  Applied via `ConfigManager.GetItemOverrideConfig` → `ItemData` (verified
  consumer in `ItemData` line ~408). `itemOverride=205:value=50,contraband=false`
  — **verified**.
- **Crafting intellect requirements**: `CraftManager` recipes carry
  `m_Intellect` (default 10) per recipe (`GetIntellectRequiredForRecipe`).
  Patch the live recipe list at load. `craftIntellect=*:0` — **verified**
  fields, medium (recipe list mutation timing).
- **AI event overrides**: `PrisonConfig.m_AIEventOverrides`
  (`AIEventData` per `AIEvent.EventType`) — per-map repurposing of AI event
  responses (applied via `ConfigManager.ApplyAIEventOverride`, verified). The
  authoring surface is big → medium. `aiEventOverride=…`.

### 2.5 Escape & objectives

- `PrisonEscapeCollider.m_EscapeMethod` + `m_EscapeCutscene` per escape zone;
  `EscapePrisonFunctionality` handles the rest. We could re-type escape zones
  (e.g. require the helicopter-day calendar event we schedule above) or gate
  them with our existing trigger-link system. `escapeMethod=…` —
  **likely** (fields verified; interplay with zone validation untested).
- Versus/co-op round duration: `PrisonConfig.m_VersusDays/Hours/Minutes` +
  `RoutineManager.SetupVersusTimeoutCallback` — for custom versus maps.
  `versusDuration=0d2h0m` — **verified**.

---

## 3. Hard / speculative

| Idea | Why it's hard | Verdict |
|---|---|---|
| Truly custom music **through Wwise** | Events must exist in shipped `.bnk` soundbanks; no runtime registration API in this Wwise version | **Impossible** as asked; use the Unity `AudioSource` side-channel from §1.2 instead |
| Gravity | There is no gameplay gravity — it's a top-down game; `CharacterMovement` is velocity-driven (`m_bUsePhysics` exists but is off and untested) | **Not applicable** — `moveSpeed`/dash settings are the real equivalent |
| New visual tile themes (beyond `m_OutfitType` enum) | Block sets/textures are baked into the building-block scene; swapping needs asset bundle injection | speculative, big effort (`LevelSetup_MapTextures` exists — worth a later look) |
| Per-map player gravity… er, *knockback physics* on thrown items, projectiles | scattered per-item `Item_Combat` data; doable via `m_ItemDataOverrides` but needs a full item audit | speculative |
| Custom cutscenes on escape/intro | `CutsceneManager` plays authored prefab cutscenes; injecting new ones = asset work | speculative |
| Re-skinning NPC outfits per map beyond `PRISON_ENUM` outfit | `CustomisationConfig`/`m_RoleStartingOutfitData` on `PrisonData` — fields verified, pipeline deep | speculative/medium |
| MP sync of all the above | Most fields are read locally on each client; as long as both clients run the mod and the same sidecar, values agree (same pattern as our fence feature). Clock itself is master-synced via `RPC_SyncTime` — our `timeScale` must be identical on all clients or drift-correction fights us | design constraint, not a blocker |

---

## Suggested `[mapSettings]` starter set (implementation order)

1. `timeScale` — one postfix, huge gameplay impact, zero risk.
2. `ambience` + `routineMusic` + `musicTheme` — one postfix on
   `LevelSetup_RoutineManager.Setup`, immediately makes maps feel custom.
3. `startHour`, `sunset`/`sunrise`, `spotlightHours` — same postfix, free.
4. `timedPrison` — escape-deadline maps, same postfix.
5. `playerMoney`, `healthRegen`, `energyRegen`, `heatDecay` — config mutation
   (with the clone-don't-mutate caveat from the intro).
6. `lockdownMinutes`, `solitary`, `startingAlertness` — scene sweep.
7. `generatorDowntime`, `cctvSpeed`, `sniperDamage`, `detectorAlertTime`,
   `fenceDamage` — scene sweep, pairs with existing fence feature.
8. `shopPriceModifier`, `jobPay`, `questGiverPercent` — config mutation.
9. `guards`/`inmates` beyond slider, `calendar` — needs testing.
10. `snow`, lighting overrides — visual polish tier.

That's **45+ concrete settings**, of which ~35 are verified against the
decompile and reachable from two patch points (`LevelSetup_RoutineManager.Setup`
postfix + a `GameEvents.LevelLoaded` scene sweep) plus careful `PrisonConfig`
mutation.
