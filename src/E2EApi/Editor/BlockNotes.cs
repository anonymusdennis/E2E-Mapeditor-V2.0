namespace E2EApi.Editor
{
    /// <summary>
    /// Researched, per-block explanations of what the game actually does with
    /// each placeable. Sources (decompiled Assembly-CSharp):
    /// - GameLevelEditorManager.PlaceObject instantiates each object's REAL
    ///   prefab (m_Prefab) when a custom map is played — so AI_* prefabs are
    ///   literal characters, Escape_* are working escape vehicles, etc.
    /// - Zone identity comes from the block's limitation group, which maps
    ///   1:1 onto ZoneDetailsManager.ZoneTypes (InmateCell, RollCall, …).
    /// - GuardTower.cs: towers run spotlights and snipe (ShootCharacter).
    /// - SpawnPoint.cs / RoomManager.cs: spawn points anchor a character to a
    ///   cell, its bed/desk/toilet and starting items.
    /// - DigFunctionality.cs / ElectricFence.cs: native electric fences shock
    ///   characters that dig or climb at them.
    /// </summary>
    internal static class BlockNotes
    {
        public static string For(BaseBuildingBlock block, string internalName, string prefabName,
            string limitGroup, string description)
        {
            string n = (internalName ?? "").ToLowerInvariant();
            string p = (prefabName ?? "").ToLowerInvariant();
            string both = n + " " + p;
            bool hasDesc = !string.IsNullOrEmpty(description);

            // ---- character spawners (real AI prefabs instantiated in play mode) ----
            if (p.StartsWith("ai_") || n.StartsWith("editor_dog") || n.StartsWith("editor_inmate") ||
                n.StartsWith("editor_medic") || n.StartsWith("editor_riotguard") ||
                n.StartsWith("editor_maintenance"))
            {
                string who = p.StartsWith("ai_") ? prefabName.Substring(3) : internalName.Substring("Editor_".Length);
                string detail;
                switch (who.ToLowerInvariant())
                {
                    case "dog":
                        detail = "Dogs patrol, chase players whose heat is high and attack on contact. " +
                                 "Normally they only exist in maps with a Kennels zone.";
                        break;
                    case "inmate":
                        detail = "This inmate exists on top of the cell-derived inmate count, has no own " +
                                 "cell/bed/spawn point, and just follows the daily routine from here.";
                        break;
                    case "medic":
                        detail = "The medic idles in the infirmary and carries knocked-out characters " +
                                 "there so they can recover.";
                        break;
                    case "riotguard":
                        detail = "Riot guards are the heavily armoured response tier: they spawn during " +
                                 "lockdowns/riots, hit harder than normal guards and cannot be knocked " +
                                 "out permanently.";
                        break;
                    case "maintenanceman":
                        detail = "The maintenance man walks to damaged walls, fences and vents and " +
                                 "repairs them over time. Without one, sabotage stays broken forever.";
                        break;
                    default:
                        detail = "It behaves exactly like its normal AI counterpart.";
                        break;
                }
                return $"Spawns a real {who} character at this tile when the map starts. " + detail +
                       " Because the prefab is the live character, it bypasses the editor's usual " +
                       "guard/inmate budgeting.";
            }

            // ---- the player/inmate spawn anchor ----
            if (both.Contains("spawnpoint") || both.Contains("spawn point"))
            {
                return "Character home anchor (SpawnPoint component). The game assigns one character " +
                       "(the player or an inmate) to each spawn point: that character appears here at " +
                       "level start and after each wake-up, owns the bed/desk/toilet attached to the " +
                       "same cell, and receives the spawn point's starting items. Cells without a spawn " +
                       "point house nobody — place one per cell, plus one for each player.";
            }

            // ---- guard tower ----
            if (both.Contains("guardtower") || both.Contains("guard tower") || both.Contains("guard_tower"))
            {
                return "Working sniper tower (GuardTower component): its spotlight sweeps the yard at " +
                       "night and during lockdown, and the sniper shoots characters who are actively " +
                       "escaping or violent while wanted. Towers also count towards the GuardTower " +
                       "room requirement some routines check.";
            }

            // ---- roll call stand spots ----
            if (both.Contains("rollcall") || both.Contains("roll call") || both.Contains("roll_call"))
            {
                return "Roll-call stand position: during the Roll Call routine every inmate paths to a " +
                       "free position like this inside the RollCall zone and stands on it until the " +
                       "routine ends. Missing positions = inmates that can never attend (causes " +
                       "permanent search-for-you behaviour). '(x1)' means capacity for one inmate.";
            }

            // ---- escape vehicles ----
            if (both.Contains("escape"))
            {
                if (both.Contains("heli"))
                {
                    return "Spawns the working escape helicopter. Reaching and boarding it completes " +
                           "the map with the helicopter-escape ending (normally requires the multi-step " +
                           "helicopter escape route items).";
                }
                if (both.Contains("boat") || both.Contains("jetski"))
                {
                    return "Spawns the working water escape vehicle. Boarding it while escaping ends " +
                           "the map with that escape. Normally placed off-map at the shoreline.";
                }
                if (both.Contains("jeep") || both.Contains("van") || both.Contains("car") ||
                    both.Contains("motorbike") || both.Contains("train"))
                {
                    return "Spawns the working land escape vehicle. Boarding it while escaping ends " +
                           "the map with that vehicle's escape cutscene. Normally placed outside the " +
                           "perimeter wall.";
                }
                return "Escape-route element: part of a themed escape sequence (the scripted endings " +
                       "beyond simply crossing the perimeter). The placed prefab is the real working " +
                       "object, so it functions in custom maps.";
            }

            // ---- routine waypoints (roll call positions etc.) ----
            if (both.Contains("waypoint") || both.Contains("way_point") || both.Contains("wayp"))
            {
                return "Routine stand position (waypoint): during the owning room's routine, one " +
                       "character paths to this exact tile and stands on it — roll call is the main " +
                       "user (one inmate per waypoint). Too few waypoints means inmates that can " +
                       "never attend, which stalls the routine.";
            }

            // ---- cross-layer transition pairs (building entrances) ----
            var obj = block as BuildingBlock_Object;
            if (obj != null &&
                (obj.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Entrance) != 0)
            {
                return "Transition ENTRANCE point: at level start the game pairs it with an Exit " +
                       "point that has the same room number on another layer (LevelSetup_Transistion). " +
                       "Characters walking into it are moved to the paired exit — this is how " +
                       "'entering a building' works (outside layer → interior layer). " +
                       "W2/H2 in the name = 2-tile wide horizontal/vertical doorway; L/R/T/B = side. " +
                       "Both points must sit inside the same room/complex to pair up." +
                       (both.Contains("dontuse") ? " (Prefab is marked DontUse — a deprecated " +
                       "version; prefer the newer Obj_EntPoint variants.)" : "");
            }
            if (obj != null &&
                (obj.m_SpecialFlags & BuildingBlock_Object.SpecialFlagsEnum.Exit) != 0)
            {
                return "Transition EXIT point: the destination half of an entrance/exit pair. The " +
                       "matching Entrance point with the same room number (on another layer) " +
                       "teleports characters here. Up/Down in the name = which way the travel feels; " +
                       "W2/H2 = doorway span; L/R/T/B = side." +
                       (both.Contains("dontuse") ? " (Prefab is marked DontUse — deprecated.)" : "");
            }

            // ---- zone painters: identity = limitation group ----
            // (m_ZoneObject is also set on many interactive objects, so only blocks whose
            //  limitation group is a real zone type are described here; the rest fall
            //  through to the object-specific rules below)
            if (obj != null && obj.m_ZoneObject)
            {
                string zoneNote = ZoneNote(limitGroup);
                if (zoneNote != null)
                {
                    return zoneNote;
                }
            }

            // ---- security & infrastructure ----
            if (both.Contains("cctv") || both.Contains("camera"))
            {
                return "Working security camera: it watches a cone in front of it and raises the " +
                       "player's heat to maximum (instant wanted) when it sees them somewhere they " +
                       "shouldn't be — restricted zones, holding contraband, or during lockdown.";
            }
            if (both.Contains("generator"))
            {
                return "Prison generator: powers the electric perimeter fences and lights. Sabotaging " +
                       "it (a job or contraband action) temporarily disables electric fences until the " +
                       "maintenance man repairs it. The Generators zone marks where it lives.";
            }
            if (both.Contains("vent"))
            {
                return "Vent system element: vent covers can be unscrewed (screwdriver) to enter the " +
                       "crawlable vent layers above the prison. Guards spot open/damaged covers and " +
                       "raise the alarm; the maintenance man re-screws them.";
            }
            if (both.Contains("fence"))
            {
                return "Fence wall. Note: themed prisons attach a native ElectricFence component to " +
                       "perimeter fences — it shocks characters who dig at or climb the fence while " +
                       "the generator is powered (20 damage + knockback in vanilla).";
            }

            // ---- stairs / layer transitions ----
            if (both.Contains("fakestairs"))
            {
                return "Visual-only stairs sprite (4 directional variants). It does NOT create a layer " +
                       "transition — it's decoration for places where real stairs would look right but " +
                       "no floor change should happen.";
            }
            if (both.Contains("stairs_down") || both.Contains("stairs down"))
            {
                return "Real layer transition: walking onto this tile moves the character one building " +
                       "layer DOWN (e.g. ground floor → underground). Must be paired with a matching " +
                       "up-stairs tile on the layer below, in the same grid position.";
            }
            if (both.Contains("stairs_up") || both.Contains("stairs up"))
            {
                return "Real layer transition: walking onto this tile moves the character one building " +
                       "layer UP (e.g. ground floor → first floor). Must be paired with a matching " +
                       "down-stairs tile on the layer above, in the same grid position.";
            }
            if (both.Contains("stairs"))
            {
                return "Stairs element: part of the layer-transition tile family (up/down pairs connect " +
                       "building layers; 'fake' variants are pure decoration).";
            }

            // ---- doors & floors ----
            if (both.Contains("floor_door") || both.Contains("floor door"))
            {
                return "Door-threshold floor tile: the floor piece that sits under a doorway so the " +
                       "flooring continues through the door frame. Horizontal/vertical variants match " +
                       "the door orientation. Visually near-invisible in the editor.";
            }
            if ((obj != null && obj.m_ItsADoor) || n.StartsWith("door"))
            {
                return "Door object: characters open it if their key/permission level allows. Players " +
                       "can lockpick it, cut through it, or slip through behind staff; guards lock it " +
                       "again. The colour in the name is the key tier that opens it (e.g. Cyan/Red = " +
                       "coloured key doors, Guard/Medic/Solitary = staff doors, 'UpOnly' = one-way). " +
                       "H/V = horizontal/vertical wall orientation.";
            }

            // ---- jobs ----
            if (both.Contains("blacksmith") || both.Contains("woodwork") || both.Contains("metalwork") ||
                both.Contains("machine") || both.Contains("materials") || both.Contains("container"))
            {
                return "Job-station element: prison jobs run as a loop of 'take material → process it " +
                       "at the machine → deliver the product to the container'. Inmates assigned to the " +
                       "job (and the player, during job hours) work these stations to earn coins. The " +
                       "matching job zone (e.g. Job_Woodwork) must cover the room for the job to exist.";
            }
            if (both.Contains("job") || both.Contains("desk"))
            {
                return "Work/duty furniture: used by the job and routine systems — inmates and staff " +
                       "path to it and play their work animation here during the matching routine.";
            }

            // ---- usable furniture ----
            if (both.Contains("shower"))
            {
                return "Working shower fixture: characters use it during the morning Shower routine " +
                       "(the Shower zone sends them here). Players can use it any time to wash. " +
                       "Down/Up in the name = which way the sprite faces.";
            }
            if (both.Contains("chair") || both.Contains("seat") || both.Contains("bench") ||
                both.Contains("sofa") || both.Contains("couch"))
            {
                return "Sittable furniture: characters sit on it to regain Energy, and routines that " +
                       "need seats (meal hall, job office, visitation) path their characters to free " +
                       "seats like this one.";
            }

            // ---- infirmary ----
            if (both.Contains("heart") || both.Contains("monitor") || both.Contains("medical"))
            {
                return "Infirmary equipment: knocked-out characters are carried to the infirmary and " +
                       "wake up next to equipment like this with restored health (players lose carried " +
                       "contraband when they wake up here).";
            }

            // ---- seasonal / misc ----
            if (both.Contains("snowman") || both.Contains("santa"))
            {
                return "Santa's Shakedown seasonal prop (DLC theme decoration). No gameplay function.";
            }
            if (both.Contains("tutorial"))
            {
                return "Tutorial-prison content: carries scripted hooks used by the Centre Perks " +
                       "tutorial (highlighted interactions, forced events). In a custom map the scripts " +
                       "never fire, so it acts as plain furniture — but may log errors.";
            }
            if (both.Contains("crack") || both.Contains("damage") || both.Contains("broken"))
            {
                return "Damage-state visual: the cracked/chipped stage a wall or floor goes through " +
                       "while being dug or chipped. Placing it pre-damages the surface — the " +
                       "maintenance man will come and repair it.";
            }
            if (both.Contains("marker") || both.Contains("arrow"))
            {
                return "Developer annotation marker: drawn in the editor only, never instantiated with " +
                       "a gameplay component. Team17 used these to leave notes on their own maps.";
            }
            if (both.Contains("temp"))
            {
                return "Unfinished developer placeholder ('TEMP'). It has placeholder art and no " +
                       "dedicated behaviour — the prefab it spawns is " +
                       (string.IsNullOrEmpty(prefabName) ? "missing entirely" : $"'{prefabName}'") +
                       ", so expect it to render wrong or do nothing. Fine as a building block for " +
                       "texture-less geometry; risky for anything else.";
            }

            // ---- wall posters / signage (EW_* = editor wall decals) ----
            if (n.StartsWith("ew_") || p.StartsWith("ew_"))
            {
                string subject =
                    both.Contains("wanted") ? "a WANTED poster" :
                    both.Contains("elec") ? "an electricity-hazard warning sign" :
                    both.Contains("danger") ? "a danger warning poster" :
                    both.Contains("calm") ? "a 'keep calm' motivational poster" :
                    both.Contains("emer") ? "an emergency-procedure notice" :
                    both.Contains("warn") ? "a warning notice" :
                    both.Contains("chart") || both.Contains("piecha") ? "a chart/statistics noticeboard" :
                    "a poster";
                return "Wall decal: renders " + subject + " on the wall face it is attached to " +
                       "(Front/Left/Right in the name = which wall face the sprite fits). Pure " +
                       "decoration — no gameplay effect, not even for the electricity warning.";
            }

            // ---- window walls ----
            if (both.Contains("window"))
            {
                return "Window wall segment: blocks movement like a wall, but characters and cameras " +
                       "can SEE through it — guards can spot restricted-zone players and contraband " +
                       "through windows. It can also be cut/broken like a weak wall in some themes.";
            }

            // ---- complex stamps ----
            if (block is BuildingBlock_Complex || n.Contains("complex"))
            {
                return "Complex stamp: a saved multi-tile arrangement (walls, floor, objects and zone " +
                       "in one brush) that the editor places in a single click and registers as one " +
                       "room. 'New Complex' entries are empty template slots with no content — placing " +
                       "them does nothing visible.";
            }

            // ---- pure decoration families ----
            if (n.StartsWith("deco") || p.Contains("_deco_") || both.Contains("floorline") ||
                both.Contains("floorsign") || both.Contains("sheetmusic"))
            {
                return "Pure decoration: a static sprite with no collision component and no gameplay " +
                       "behaviour. Floor-line variants are painted markings; the name encodes " +
                       "orientation (90/180/270) and colour. Safe to place anywhere.";
            }

            // ---- generic but still informative fallbacks ----
            if (block != null && block.m_AutomaticBlock)
            {
                return "Automatic block: the editor places this itself as a side effect of other edits " +
                       "(corner pieces, wall caps, auto-floors). Placing it manually works but the " +
                       "editor may replace it when neighbouring tiles change.";
            }
            if (ZoneNote(limitGroup) != null)
            {
                // non-zone block that still counts towards a zone group (beds, benches…)
                return "Counts towards the '" + limitGroup + "' room requirement. " + ZoneNote(limitGroup);
            }
            if (obj != null && obj.m_ZoneObject && !hasDesc)
            {
                return "Zone-flagged object: placing it also stamps its area into the zone/room data " +
                       "(group: " + (string.IsNullOrEmpty(limitGroup) ? "none" : limitGroup) + "). " +
                       "The spawned prefab '" + (prefabName ?? "none") + "' provides the actual " +
                       "in-play behaviour.";
            }
            if (block != null && block.m_EditorOnly)
            {
                return "Dev-only block: present in the game data but hidden from the vanilla spawnlist. " +
                       "In play mode it spawns prefab '" + (prefabName ?? "none") + "' — whatever that " +
                       "prefab contains is exactly what the block does.";
            }
            return null;
        }

        /// <summary>What the game does with each zone / limitation group (ZoneTypes).</summary>
        private static string ZoneNote(string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                return null;
            }
            switch (group)
            {
                case "InmateCell":
                    return "Inmate cell zone: each cell (zone + bed + spawn point) houses one inmate. " +
                           "The map's inmate count is the number of valid cells minus 4 (reserved for " +
                           "players). Inmates sleep here at lights-out; guards check cells during shakedowns.";
                case "RollCall":
                    return "Roll-call zone: inmates muster here during the Roll Call routine, each on a " +
                           "stand position. Missing or unreachable roll-call space breaks the daily routine.";
                case "MealHall":
                    return "Meal-hall zone: inmates path here at breakfast/dinner routines to eat. " +
                           "Eating restores energy; fights here are common heat sources.";
                case "Gym":
                    return "Gym zone: inmates (and the player) train Strength here during free time and " +
                           "the exercise routine, using the gym equipment placed inside.";
                case "Shower":
                    return "Shower zone: morning shower routine destination. Inmates undress here; " +
                           "a classic spot for fights with no guards watching.";
                case "Library":
                    return "Library zone: inmates raise Intellect here during free time using desks and " +
                           "bookshelves.";
                case "Solitary":
                    return "Solitary zone: punishment cells. Characters wanted-for-solitary get carried " +
                           "here by guards and locked in for a timer; their contraband is confiscated.";
                case "Infirmary":
                    return "Infirmary zone: knocked-out characters are carried here by the medic and " +
                           "wake up with restored health; players lose contraband on waking here.";
                case "JobOffice":
                    return "Job-office zone: the job board lives here — inmates and players visit it to " +
                           "take, swap or lose prison jobs; the job officer works from here.";
                case "ControlRoom":
                    return "Control-room zone: staff-only area housing camera monitors; entering it as " +
                           "an inmate is instantly restricted (heat).";
                case "ContrabandRoom":
                    return "Contraband-room zone: confiscated items end up in the storage here — raid " +
                           "it to get your stuff back. Staff-only, heavily restricted.";
                case "Kitchen":
                    return "Kitchen zone: the kitchen job runs here (food prep loop); knives spawn in " +
                           "its containers, making it a contraband hotspot.";
                case "Kennels":
                    return "Kennels zone: home area for guard dogs; dogs return here between patrols. " +
                           "A map with kennels gets dog patrols.";
                case "WardensOffice":
                    return "Warden's-office zone: the warden's desk and unique loot live here; " +
                           "staff-only and heavily restricted.";
                case "GuardQuarters":
                    return "Guard-quarters zone: off-duty guards rest here; staff-only area with guard " +
                           "uniforms and keys as typical loot.";
                case "GuardRoom":
                    return "Guard-room zone: on-duty guard base; guards idle and respawn patrols from " +
                           "here. Staff-only.";
                case "Maintenance":
                    return "Maintenance zone: the maintenance man's base; repair jobs originate here and " +
                           "tools spawn in its containers.";
                case "SocialArea":
                    return "Social-area zone: free-time destination where inmates chat, play and idle " +
                           "(TV rooms, yards, pool tables).";
                case "Generators":
                    return "Generator zone: houses the generator that powers electric fences and lights; " +
                           "sabotage happens here.";
                case "JobRoom":
                    return "Generic job-room zone: marks the work area for a prison job so inmates know " +
                           "where to clock in.";
                case "Job_Woodwork":
                    return "Woodwork-job zone: the woodshop job loop (timber → lathe → crate) runs " +
                           "inside this area.";
                case "Job_Blacksmith":
                    return "Blacksmith/metalshop-job zone: the metal job loop (ingot → forge → crate) " +
                           "runs inside this area. Metal tools are easy to pocket here.";
                case "GuardTower":
                    return "Guard-tower group: counts sniper towers for the map's security setup.";
                case "Inmate":
                    return "Counts towards the map's inmate population.";
                case "Guard":
                    return "Counts towards the map's guard staffing.";
                case "Visitor":
                    return "Visitation area: the visitor routine brings civilians here to meet inmates.";
                case "WasteCollection":
                    return "Waste-collection point: garbage bins/dumpsters — also a classic escape " +
                           "hiding spot (the laundry/garbage truck trick).";
                case "InfirmaryStockRoom":
                    return "Infirmary stock room: medical supplies storage (medkits spawn here).";
                default:
                    return null;
            }
        }
    }
}
