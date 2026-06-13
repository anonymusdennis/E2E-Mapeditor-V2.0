namespace MapEditorMod.WebUi
{
    /// <summary>The single-page web UI, embedded as a string.</summary>
    internal static class WebUiPage
    {
        public const string Html = @"<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='utf-8'>
<title>E2E Map Editor</title>
<style>
  :root { color-scheme: dark; }
  * { box-sizing: border-box; }
  body { margin:0; font-family: system-ui, sans-serif; background:#16161c; color:#eee; }

  /* fixed chrome: header with tabs + per-tab subbar */
  #top { position:fixed; top:0; left:0; right:0; z-index:20; background:#20202a;
         box-shadow:0 2px 10px rgba(0,0,0,.45); }
  #bar1 { display:flex; gap:10px; align-items:center; padding:8px 14px; flex-wrap:wrap; }
  #bar1 h1 { font-size:16px; margin:0 8px 0 0; font-weight:600; }
  .tab { background:none; border:none; color:#9a9ab0; padding:7px 14px; font-size:13px;
         cursor:pointer; border-radius:8px 8px 0 0; border-bottom:2px solid transparent; }
  .tab:hover { color:#fff; }
  .tab.on { color:#fff; border-bottom-color:#7a7af0; background:#26263200; font-weight:600; }
  #status { font-size:12px; color:#9a9ab0; margin-left:auto; }
  #msg { font-size:12px; color:#7fd17f; min-height:14px; max-width:40vw;
         overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
  #bar2 { display:flex; gap:8px; align-items:center; padding:6px 14px 8px; flex-wrap:wrap;
          border-top:1px solid #2a2a38; }

  input[type=search], input[type=number] { background:#0e0e12; border:1px solid #333;
         color:#eee; padding:6px 10px; border-radius:6px; }
  input[type=search] { width:200px; }
  input[type=number] { width:80px; }
  button { background:#2a2a38; border:1px solid #3a3a4c; color:#eee; padding:6px 10px;
           border-radius:6px; cursor:pointer; font-size:12px; }
  button:hover { background:#34344a; }
  button.active { background:#46467a; border-color:#6a6ab0; }
  button.danger { border-color:#7a3a3a; }

  main { padding:12px 14px; }
  .page { display:none; }
  .page.on { display:block; }

  #grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(86px,1fr)); gap:8px; }
  .cell { background:#20202a; border:1px solid #2e2e3c; border-radius:8px; padding:6px;
          text-align:center; cursor:pointer; }
  .cell:hover { border-color:#6a6ab0; }
  .cell.dev { background:#2c2026; }
  .cell.sel { border-color:#7fd17f; background:#1f2a1f;
              box-shadow:0 0 0 2px rgba(127,209,127,.45), 0 0 14px rgba(127,209,127,.35); }
  .cell.dragover { border-color:#e8c66a; box-shadow:0 0 0 2px rgba(232,198,106,.5); }
  .cell img { width:64px; height:64px; object-fit:contain; image-rendering:pixelated; }
  .cell .noicon { width:64px; height:64px; display:inline-block; line-height:64px; color:#555; }
  .cell .nm { font-size:10px; color:#bbb; word-break:break-word; margin-top:2px; }
  .chipx { margin-left:6px; color:#e09a9a; font-weight:700; cursor:pointer; }
  .chipx:hover { color:#ff6a6a; }
  button.custom { border-color:#5a5a8c; }

  .card { background:#1c1c24; border:1px solid #2e2e3c; border-radius:10px;
          padding:12px 16px; margin-bottom:14px; }
  .card h2 { font-size:13px; margin:0 0 10px; color:#bdbdd6; text-transform:uppercase;
             letter-spacing:.06em; }
  .row { display:flex; gap:10px; flex-wrap:wrap; align-items:center; margin-bottom:8px; }
  .row:last-child { margin-bottom:0; }
  .chk { display:flex; align-items:center; gap:6px; font-size:13px; color:#ccc;
         background:#22222c; border:1px solid #30303e; padding:6px 10px; border-radius:6px;
         cursor:pointer; user-select:none; }
  .chk input { cursor:pointer; }
  .num { display:flex; align-items:center; gap:6px; font-size:13px; color:#ccc; }
  .hint { font-size:12px; color:#8d8da8; line-height:1.5; }

  #mapwrap { position:relative; display:inline-block; max-width:100%; }
  #mapimg { display:block; width:min(820px, 92vw); image-rendering:pixelated;
            border:1px solid #3a3a4c; border-radius:8px; cursor:crosshair; background:#0c0c12; }
  #pmark { position:absolute; width:14px; height:14px; margin:-7px 0 0 -7px; border-radius:50%;
           background:#ff4d4d; border:2px solid #fff; box-shadow:0 0 8px #000;
           pointer-events:none; display:none; }
  #pstats span { display:inline-block; margin-right:16px; font-size:13px; }
  #pstats b { color:#fff; }

  #tip { position:fixed; z-index:50; max-width:360px; background:#0c0c12; border:1px solid #44446a;
          border-radius:8px; padding:10px 12px; font-size:12px; line-height:1.45; color:#ddd;
          pointer-events:none; display:none; box-shadow:0 6px 24px rgba(0,0,0,.6); }
  #tip b { color:#fff; font-size:13px; }
  #tip .meta { color:#8d8da8; }
  #tip .notes { color:#c9e3a8; margin-top:6px; display:block; }

  /* custom building blocks section */
  #cb_section { margin-bottom:10px; }
  .cat-section { margin-bottom:12px; }
  .cat-hdr { display:flex; gap:8px; align-items:center; margin-bottom:6px; }
  .cat-hdr h2 { font-size:13px; margin:0; color:#bdbdd6; text-transform:uppercase; letter-spacing:.06em; }
  #cb_hdr { display:flex; gap:8px; align-items:center; margin-bottom:8px; }
  #cb_hdr h2 { font-size:13px; margin:0; color:#bdbdd6; text-transform:uppercase; letter-spacing:.06em; }
  #cb_grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(86px,1fr)); gap:8px; }
  .cb_grid { display:grid; grid-template-columns:repeat(auto-fill,minmax(86px,1fr)); gap:8px; }
  .cb_cell { background:#1c2030; border:1px solid #3a3a5c; border-radius:8px; padding:6px;
             text-align:center; cursor:pointer; position:relative; }
  .cb_cell:hover { border-color:#7a7af0; }
  .cb_cell.sel { border-color:#7fd17f; background:#1f2a1f;
                 box-shadow:0 0 0 2px rgba(127,209,127,.45), 0 0 14px rgba(127,209,127,.35); }
  .cb_cell.anim { border-color:#3a4a6c; }
  .cb_cell.anim:hover { border-color:#7a9af0; }
  .cb_cell.anim.sel { border-color:#7fd1ff; background:#1a2030; }
  .cb_noicon { width:64px; height:64px; display:inline-block; line-height:64px; color:#555; font-size:26px; }
  .cb_del { position:absolute; top:3px; right:5px; color:#e09a9a; font-weight:700; cursor:pointer; font-size:11px; }
  .cb_del:hover { color:#ff6a6a; }
  .cb_sep { border:none; border-top:1px solid #2a2a38; margin:10px 0 0; }

  /* animation editor overlay */
  #anim_overlay { position:fixed; inset:0; background:rgba(0,0,0,.8); z-index:300;
                  display:none; align-items:flex-start; justify-content:center; overflow-y:auto; }
  #anim_overlay.show { display:flex; }
  #anim_box { background:#1a1a26; border:1px solid #44446a; border-radius:14px;
              padding:20px 22px; width:min(860px,96vw); margin:18px auto; }
  #anim_box h3 { font-size:15px; margin:0 0 14px; color:#eee; display:flex; align-items:center; gap:8px; }
  #anim_box h3 span.anim-badge { font-size:11px; background:#3a3a6a; color:#9a9ae8;
    padding:2px 8px; border-radius:6px; font-weight:normal; letter-spacing:.04em; }
  .anim-section { background:#111118; border:1px solid #2a2a3a; border-radius:8px;
                  padding:10px 12px; margin-bottom:10px; }
  .anim-section h4 { font-size:11px; color:#7a7ab0; text-transform:uppercase; letter-spacing:.06em;
                     margin:0 0 8px; }
  .frame-strip { display:flex; gap:6px; padding:6px 0; overflow-x:auto; min-height:76px;
                 align-items:center; flex-wrap:nowrap; }
  .frame-strip::-webkit-scrollbar { height:4px; }
  .frame-strip::-webkit-scrollbar-track { background:#111; }
  .frame-strip::-webkit-scrollbar-thumb { background:#444; border-radius:2px; }
  .frame-item { position:relative; border:2px solid #333345; border-radius:6px;
                cursor:pointer; flex-shrink:0; background:#0c0c14; }
  .frame-item:hover { border-color:#7a7af0; }
  .frame-item.sel { border-color:#7fd17f; }
  .frame-item canvas { display:block; border-radius:4px; }
  .frame-idx { position:absolute; bottom:1px; left:3px; font-size:9px; color:#888;
               background:rgba(0,0,0,.6); padding:0 3px; border-radius:2px; }
  .frame-del { position:absolute; top:1px; right:2px; color:#e09a9a; cursor:pointer;
               font-size:10px; font-weight:700; line-height:1; padding:1px 3px;
               background:rgba(0,0,0,.5); border-radius:2px; }
  .frame-del:hover { color:#ff6a6a; }
  .frame-add-btn { width:60px; height:60px; border:2px dashed #334; border-radius:6px;
                   display:flex; align-items:center; justify-content:center; cursor:pointer;
                   font-size:22px; color:#446; flex-shrink:0; }
  .frame-add-btn:hover { border-color:#6a6ab0; color:#9a9af0; }
  #anim_preview_canvas { border:1px solid #334; border-radius:6px; background:#0c0c14;
                         image-rendering:pixelated; }
  .anim-controls { display:flex; gap:12px; flex-wrap:wrap; align-items:center; }
  .anim-num { display:flex; align-items:center; gap:5px; font-size:12px; color:#ccc; }
  .anim-num input[type=number] { width:64px; background:#0e0e12; border:1px solid #333;
    color:#eee; padding:4px 8px; border-radius:5px; font-size:12px; }
  .anim-chk { display:flex; align-items:center; gap:5px; font-size:12px; color:#ccc; cursor:pointer; }
  .anim-chk input { cursor:pointer; }
  #anim_name_field, #anim_cat_field { width:100%; background:#0e0e14; border:1px solid #333;
    color:#eee; padding:7px 10px; border-radius:6px; font-size:13px; font-family:inherit;
    margin-bottom:8px; display:block; box-sizing:border-box; }
  #anim_desc_field { width:100%; background:#0e0e14; border:1px solid #333; color:#eee;
    padding:7px 10px; border-radius:6px; font-size:13px; font-family:inherit;
    resize:vertical; min-height:50px; display:block; box-sizing:border-box; }
  .anim-btns { display:flex; gap:8px; flex-wrap:wrap; }
  .anim-btns button { font-size:12px; }
  .anim-detect-btn { background:#26264a; border-color:#4a4a80; }
  .anim-detect-btn:hover { background:#32327a; }
  .anim-save-btn { background:#264026; border-color:#3a6a3a; font-weight:600; }
  .anim-save-btn:hover { background:#306030; }

  /* modal dialog */
  #modal_overlay { position:fixed; inset:0; background:rgba(0,0,0,.7); z-index:200;
                   display:none; align-items:center; justify-content:center; }
  #modal_overlay.show { display:flex; }
  #modal_box { background:#1c1c28; border:1px solid #3e3e5c; border-radius:12px;
               padding:22px 26px; min-width:320px; max-width:460px; width:90vw; }
  #modal_box h3 { font-size:14px; margin:0 0 14px; color:#eee; }
  #modal_box input[type=text], #modal_box textarea {
    width:100%; background:#0e0e14; border:1px solid #333; color:#eee;
    padding:8px 10px; border-radius:6px; margin-bottom:10px;
    font-size:13px; font-family:inherit; display:block; }
  #modal_box input[type=text].error { border-color:#c04040; }
  #modal_err { font-size:12px; color:#e09a9a; margin:-6px 0 8px; display:none; }
  #modal_box textarea { resize:vertical; min-height:60px; }
  #modal_btns { display:flex; gap:8px; justify-content:flex-end; margin-top:4px; }
  #modal_box.nameonly textarea { display:none; margin-bottom:0; }
</style>
</head>
<body>
<div id='top'>
  <div id='bar1'>
    <h1>E2E Map Editor</h1>
    <button class='tab on' data-page='blocks' onclick='showTab(this)'>Blocks</button>
    <button class='tab' data-page='tilesets' onclick='showTab(this)'>Tilesets</button>
    <button class='tab' data-page='tools' onclick='showTab(this)'>Tools</button>
    <button class='tab' data-page='settings' onclick='showTab(this)'>Settings</button>
    <button class='tab' data-page='gameplay' onclick='showTab(this)'>Gameplay</button>
    <span id='msg'></span>
    <span id='status'>connecting…</span>
  </div>
  <div id='bar2'>
    <input id='q' type='search' placeholder='search blocks…'>
    <span id='filters'></span>
    <span id='cfilters'></span>
    <button onclick='saveCustomFilter()' title='save the current search + filter combination as a chip'>★ Save filter</button>
    <button onclick='loadBlocks()'>Reload</button>
  </div>
</div>

<main>
  <div class='page on' id='page_blocks'>
    <div id='cb_section' style='display:none'>
      <div id='cb_cats'></div>
      <hr class='cb_sep'>
    </div>
    <div id='grid'></div>
  </div>

  <div class='page' id='page_tilesets'>
    <div class='card'>
      <h2>Content sets <span id='ts_status' style='text-transform:none;color:#8d8da8'></span>
        <button onclick='openAnimEditorEmpty()' style='margin-left:auto;font-size:11px;padding:4px 10px'
          title='Create animated tile from scratch'>🎬 Animate</button>
      </h2>
      <div class='row' id='ts_sets'><span class='hint'>loading…</span></div>
      <div class='row'>
        <button onclick='harvest(""all"")'>Harvest all installed</button>
        <button onclick='refreshTilesets()'>Refresh</button>
      </div>
      <div class='hint'>Harvesting loads each prison scene once (main menu only, a few
        seconds per set) and caches its tile/decor art on disk. Afterwards the art is
        browsable here and paintable onto any custom map. Maps remember art by asset
        name — other modded players just harvest the same sets to see it.</div>
    </div>
    <div class='card' id='ts_atlcard' style='display:none'>
      <h2>Atlases — <span id='ts_setname' style='text-transform:none'></span></h2>
      <div id='ts_atlases' style='display:grid;grid-template-columns:repeat(auto-fill,minmax(150px,1fr));gap:8px'></div>
    </div>
    <div class='card' id='ts_pickcard' style='display:none'>
      <h2>Pick a region — <span id='ts_atlasname' style='text-transform:none'></span></h2>
      <div class='row'>
        <label class='chk'><input type='radio' name='ts_mode' id='ts_mode_floor' checked>
          Floor (under characters)</label>
        <label class='chk'><input type='radio' name='ts_mode' id='ts_mode_decor'>
          Decor (over characters)</label>
        <span class='num'>Zoom <input type='number' id='ts_zoom' min='1' max='4' value='2'
          onchange='atlasZoom()'></span>
        <span id='ts_sel' class='hint'>drag on the image to select tiles (32 px = 1 tile)</span>
      </div>
      <div class='row'>
        <button id='ts_arm' onclick='armStamp()' disabled>🖌 Paint this stamp</button>
        <button id='ts_save_block' onclick='saveAsCustomBlock()' disabled>⊕ Save as building block</button>
        <button id='ts_open_anim' onclick='openAnimEditorFromSelection()' disabled>🎬 Create animated tile</button>
        <button id='tool_tileerase' onclick='setTool(""tileerase"")'>✕ Erase modded tiles</button>
        <button class='danger' onclick='confirm(""Do you really want to delete all custom tilesets?"") && post(""/api/tiles/clear"")'>Clear ALL modded tiles</button>
        <button class='danger' onclick='openAnimEditorEmpty()' title='Open animation editor'>🎬 Animate tileset</button>
      </div>
      <div style='overflow:auto;max-height:70vh;border:1px solid #3a3a4c;border-radius:8px'>
        <div id='ts_wrap' style='position:relative;display:inline-block;line-height:0'>
          <img id='ts_img' style='image-rendering:pixelated;display:block;cursor:crosshair'
               draggable='false'>
          <div id='ts_grid' style='position:absolute;inset:0;pointer-events:none;
               background-image:linear-gradient(to right,rgba(255,255,255,.13) 1px,transparent 1px),
               linear-gradient(to bottom,rgba(255,255,255,.13) 1px,transparent 1px)'></div>
          <div id='ts_rect' style='position:absolute;border:2px solid #7fd17f;
               background:rgba(127,209,127,.18);pointer-events:none;display:none'></div>
        </div>
      </div>
    </div>
  </div>

  <div class='page' id='page_tools'>
    <div class='card'>
      <h2>Electricity &amp; logic tools</h2>
      <div class='row'>
        <button id='tool_paint' onclick='setTool(""paint"")'>⚡ Paint electricity</button>
        <button id='tool_erase' onclick='setTool(""erase"")'>✕ Remove electricity</button>
        <button id='tool_link' onclick='setTool(""link"")'>🔗 Link fence → switch</button>
      </div>
      <div class='hint'>A tool takes over the left mouse button inside the game:
        the vanilla editor stops placing/selecting while it is active, and a colored
        tile cursor shows what you are aiming at. Paint/erase work by holding LMB and
        dragging. Esc-equivalent: click the active tool button again to turn it off.</div>
      <div class='row' style='margin-top:8px'>
        <button onclick='post(""/api/fence/cursor"")'>Toggle fence @ cursor (F6)</button>
        <button onclick='post(""/api/trigger/cursor"")'>Link trigger @ cursor (F7, 2 clicks)</button>
        <button class='danger' onclick='post(""/api/clear-extras"")'>Clear ALL fences + links</button>
      </div>
    </div>
    <div class='card'>
      <h2>Overlays</h2>
      <div class='row'>
        <label class='chk'><input type='checkbox' id='s_fenceOverlay'
            onchange='setSetting(""fenceOverlay"",this.checked,false)'>Electric fence overlay</label>
        <label class='chk'><input type='checkbox' id='s_arrows'
            onchange='setSetting(""arrows"",this.checked,false)'>Logic connection arrows</label>
        <label class='chk'><input type='checkbox' id='s_xray'
            onchange='setSetting(""xray"",this.checked,false)'>X-ray hidden blocks</label>
      </div>
    </div>
    <div class='card'>
      <h2>Camera</h2>
      <div class='row'>
        <label class='chk'><input type='checkbox' id='s_cameraLock'
            onchange='setSetting(""cameraLock"",this.checked,false)'><b>Lock camera</b>
            — no edge auto-pan, WASD/arrows only</label>
      </div>
    </div>
    <div class='card'>
      <h2>Map flags</h2>
      <div class='row'>
        <label class='chk'><input type='checkbox' id='requiresMod'
            onchange='post(""/api/requiresmod?value=""+this.checked)'>This map requires the mod</label>
      </div>
      <div class='hint'>Set automatically as soon as the map contains fences, trigger
        links or modded tiles. Stored in the Level.e2e sidecar next to Level.dat.</div>
      <div class='row' style='margin-top:8px'>
        <label class='chk'><input type='checkbox' id='s_vanillaFallback'
            onchange='setSetting(""vanillaFallback"",this.checked,false)'>Vanilla fallback
            disclaimer map</label>
      </div>
      <div class='hint'>When on (recommended), publishing a map that needs the mod writes a
        vanilla-safe Level_Finished.dat with ""NEEDS E2E MAPEDITOR MOD"" painted on the ground;
        the real map travels in the sidecar and is restored automatically on modded clients.
        When off, vanilla players see the map without the modded extras.</div>
    </div>
  </div>

  <div class='page' id='page_settings'>
    <div class='card'>
      <h2>Editor unlocks</h2>
      <div class='row'>
        <label class='chk'><input type='checkbox' id='s_devBlocks'
            onchange='setSetting(""devBlocks"",this.checked,true)'>Dev-only blocks</label>
        <label class='chk' title='Items + craft recipes of every installed DLC (and of all base-game prisons) become available in custom maps. Not-owned DLC stays locked.'>
            <input type='checkbox' id='s_dlcBlocks'
            onchange='setSetting(""dlcBlocks"",this.checked,true)'>Installed-DLC content</label>
        <label class='chk'><input type='checkbox' id='s_layers'
            onchange='setSetting(""layers"",this.checked,true)'>Ignore layer restrictions</label>
        <label class='chk'><input type='checkbox' id='s_completion'
            onchange='setSetting(""completion"",this.checked,true)'>Incomplete blocks</label>
        <label class='chk' title='Place anything anywhere; save, upload and play unfinished maps'>
            <input type='checkbox' id='s_restrictions'
            onchange='setSetting(""restrictions"",this.checked,true)'><b>Ignore ALL restrictions</b></label>
      </div>
    </div>
    <div class='card'>
      <h2>Editor numbers</h2>
      <div class='row'>
        <span class='num'>Guard/inmate cap <input type='number' id='n_guardCap' min='0' max='99'
            onchange='setNum(""guardCap"",this.value)'></span>
        <span class='num'>Extra zoom-in steps <input type='number' id='n_zoomSteps' min='0' max='6'
            onchange='setNum(""zoomSteps"",this.value)'></span>
        <span class='num'>Fence damage/tick <input type='number' id='n_fenceDamage' min='0' max='100'
            onchange='setNum(""fenceDamage"",this.value)'></span>
      </div>
      <div class='hint'>Zoom steps apply the next time you enter the editor.</div>
    </div>
    <div class='card'>
      <h2>Display &amp; web UI</h2>
      <div class='row'>
        <label class='chk'><input type='checkbox' id='s_forceWindowed'
            onchange='setSetting(""forceWindowed"",this.checked,false)'>Force windowed (next launch)</label>
        <span class='num'>Window <input type='number' id='n_windowWidth' min='0' max='7680'
            onchange='setNum(""windowWidth"",this.value)'> ×
            <input type='number' id='n_windowHeight' min='0' max='4320'
            onchange='setNum(""windowHeight"",this.value)'></span>
        <label class='chk'><input type='checkbox' id='s_autoOpen'
            onchange='setSetting(""autoOpen"",this.checked,false)'>Auto-open this page</label>
      </div>
    </div>
    <div class='card'>
      <h2>Per-map settings (coming)</h2>
      <div class='hint'>Custom per-map rules — time speed, ambient sound/music, routine
        times, guard counts, lighting and more — are being catalogued in
        <b>possible_settings.md</b> and will be stored in the map's Level.e2e sidecar.</div>
    </div>
  </div>

  <div class='page' id='page_gameplay'>
    <div class='card'>
      <h2>Player</h2>
      <div class='row'>
        <button onclick='post(""/api/dev/playtest"")'>▶ Play-test the open map</button>
        <span class='hint'>switches the editor into the game's preview mode</span>
      </div>
      <div id='pstats' class='row'><span class='hint'>no player — start/test a map first</span></div>
      <div class='row'>
        <button onclick='cheat(""heal"")'>❤ Full health</button>
        <button onclick='cheat(""energy"")'>⚡ Full energy</button>
        <button onclick='cheat(""money"")'>$ Max money</button>
        <button onclick='cheat(""stealth"")'>👻 Clear heat/wanted</button>
        <button onclick='cheat(""ko-guards"")'>💤 KO all guards</button>
        <button onclick='cheat(""ko-dogs"")'>🐕 KO all dogs</button>
        <label class='chk'><input type='checkbox' id='s_infiniteEnergy'
            onchange='setSetting(""infiniteEnergy"",this.checked,false)'>Infinite energy</label>
      </div>
    </div>
    <div class='card'>
      <h2>Teleport — click the map</h2>
      <div class='row' id='floorBtns'><span class='hint'>floors appear in play mode</span></div>
      <div id='mapwrap'>
        <img id='mapimg' alt='floor map (play mode only)'
             onclick='mapClick(event)' onerror='mapError()'>
        <div id='pmark'></div>
      </div>
      <div class='hint' style='margin-top:6px'>The red dot is your test player. Click
        anywhere to teleport there on the shown floor — great for checking far corners
        of a map without walking.</div>
    </div>
  </div>
</main>
<div id='anim_overlay' onclick='if(event.target===this)closeAnimEditor()'>
  <div id='anim_box'>
    <h3>🎬 Animation Editor <span class='anim-badge'>Animated Tile Creator</span></h3>

    <div class='anim-section'>
      <h4>Frames <span id='anim_frame_count' style='color:#667;font-size:10px'>(0 frames)</span></h4>
      <div class='frame-strip' id='anim_frame_strip'>
        <div class='frame-add-btn' onclick='addCurrentSelectionAsFrame()' title='Add current atlas selection as frame'>＋</div>
      </div>
      <div class='anim-btns' style='margin-top:6px'>
        <button class='anim-detect-btn' onclick='autoDetectFrames()' id='anim_detect_btn' title='Auto-scan this atlas for repeating same-size tiles along horizontal or vertical strips'>🔍 Detect frames automatically</button>
        <button onclick='addCurrentSelectionAsFrame()' id='anim_add_sel_btn' disabled title='Add current tileset selection as next frame'>⊕ Add current selection</button>
        <button onclick='clearAnimFrames()' style='background:#2a1a1a;border-color:#6a2a2a'>✕ Clear all frames</button>
      </div>
      <div id='anim_detect_status' style='font-size:11px;color:#7a9a7a;margin-top:4px;display:none'></div>
    </div>

    <div class='anim-section'>
      <h4>Preview</h4>
      <div style='display:flex;gap:12px;align-items:flex-start;flex-wrap:wrap'>
        <div>
          <canvas id='anim_preview_canvas' width='64' height='64'></canvas>
          <div style='margin-top:4px;display:flex;gap:6px'>
            <button onclick='previewAnimEditor()' id='anim_play_btn' style='font-size:11px;padding:3px 8px'>▶ Play</button>
            <button onclick='stopAnimPreview()' id='anim_stop_btn' style='font-size:11px;padding:3px 8px'>⏹ Stop</button>
            <span id='anim_frame_disp' style='font-size:11px;color:#777;line-height:28px'>Frame 0/0</span>
          </div>
        </div>
        <div style='flex:1;min-width:200px'>
          <div class='anim-controls'>
            <div class='anim-num'><label>FPS</label>
              <input type='number' id='anim_fps' min='1' max='60' value='4'
                oninput='restartPreviewIfRunning()'>
            </div>
            <div class='anim-num'><label>Scale preview</label>
              <input type='number' id='anim_preview_scale' min='1' max='8' value='2'
                oninput='updatePreviewCanvasSize()'>
            </div>
          </div>
          <div class='anim-controls' style='margin-top:8px'>
            <label class='anim-chk'><input type='checkbox' id='anim_loop' checked>Loop</label>
            <label class='anim-chk'><input type='checkbox' id='anim_pingpong'>Ping-pong</label>
            <label class='anim-chk'><input type='radio' name='anim_layer' id='anim_layer_floor' checked>Floor</label>
            <label class='anim-chk'><input type='radio' name='anim_layer' id='anim_layer_decor'>Decor</label>
          </div>
          <div style='margin-top:8px;font-size:11px;color:#667;font-style:italic' id='anim_timing_hint'></div>
        </div>
      </div>
    </div>

    <div class='anim-section'>
      <h4>Atlas source for auto-detect</h4>
      <div style='display:flex;gap:8px;align-items:center;flex-wrap:wrap'>
        <span style='font-size:12px;color:#ccc'>Current atlas:</span>
        <span id='anim_src_atlas' style='font-size:12px;color:#9a9af0;font-style:italic'>none selected</span>
        <span style='font-size:12px;color:#777'>|</span>
        <span style='font-size:12px;color:#ccc'>Selection:</span>
        <span id='anim_src_sel' style='font-size:12px;color:#7fd17f;font-style:italic'>none</span>
      </div>
      <div style='font-size:11px;color:#556;margin-top:4px'>
        Auto-detect scans the atlas image for non-transparent tile-sized regions adjacent to the current selection.
        It searches in all 4 directions and adds each found frame automatically.
      </div>
    </div>

    <div class='anim-section'>
      <h4>Save as animated building block</h4>
      <input type='text' id='anim_name_field' placeholder='Block name…' maxlength='80'>
      <input type='text' id='anim_cat_field' placeholder='Category (e.g. animated, effects, custom…)' maxlength='60' value='animated'>
      <textarea id='anim_desc_field' placeholder='Description (optional)…' maxlength='400'></textarea>
      <div class='anim-btns'>
        <button class='anim-save-btn' onclick='saveAnimBlock()'>💾 Save animated tile</button>
        <button onclick='closeAnimEditor()' style='background:#1a1a2a'>Cancel</button>
      </div>
      <div id='anim_save_err' style='font-size:12px;color:#e09a9a;margin-top:4px;display:none'></div>
    </div>
  </div>
</div>
<div id='modal_overlay' onclick='if(event.target===this)cancelModal()'>
  <div id='modal_box'>
    <h3 id='modal_title'>Add custom building block</h3>
    <input type='text' id='modal_name' placeholder='Name…' maxlength='80'
           onkeydown='if(event.key===""Enter"")confirmModal()'>
    <div id='modal_err'>Name is required.</div>
    <textarea id='modal_desc' placeholder='Description (optional)…' maxlength='400'></textarea>
    <div id='modal_btns'>
      <button onclick='cancelModal()'>Cancel</button>
      <button onclick='confirmModal()' class='custom' style='background:#36366a'>Save</button>
    </div>
  </div>
</div>
<div id='tip'></div>

<script>
let blocks = [];
let tool = 'none';
let selectedId = -1;                       // block currently on the editor brush
let customSelectedId = -1;                 // index in prefs.customBlocks for active custom stamp
let modalCallback = null;
let activeFilters = new Set();             // ids of active built-in filter chips
let prefs = { customFilters: [], order: [], customBlocks: [], customCategoryName: 'Custom', animatedDefs: [] };
let orderRank = {};                        // block id -> user rank
let dragId = null;
let curFloor = -1;
let floorsKnown = '';
let mapTileSize = 8; // px per tile in the map texture (game generates 8)

function el(id) { return document.getElementById(id); }
function msg(t) { el('msg').textContent = t; }

/* ---- tabs ---- */
function showTab(btn) {
  document.querySelectorAll('.tab').forEach(b => b.classList.toggle('on', b === btn));
  const page = btn.dataset.page;
  document.querySelectorAll('.page').forEach(p =>
    p.classList.toggle('on', p.id === 'page_' + page));
  el('bar2').style.display = page === 'blocks' ? 'flex' : 'none';
  padMain();
  if (page === 'gameplay') refreshGameplay();
  if (page === 'tilesets') refreshTilesets();
}
function padMain() {
  document.querySelector('main').style.paddingTop = (el('top').offsetHeight + 10) + 'px';
}
window.addEventListener('resize', padMain);

/* ---- blocks tab: filters ---- */
function blockText(b) {
  return ((b.name||'')+' '+(b.internalName||'')+' '+(b.prefab||'')+' '
    +(b.className||'')).toLowerCase();
}
function reHit(b, words) { return new RegExp(words).test(blockText(b)); }

const FILTERS = [
  { id:'floors',   label:'Floors',     fn:b => b.kind === 'Tile' },
  { id:'walls',    label:'Walls',      fn:b => b.kind === 'Wall' },
  { id:'doors',    label:'Doors',      fn:b => reHit(b,'door|gate|hatch|barred') },
  { id:'furniture',label:'Furniture',  fn:b => reHit(b,'bed|chair|table|desk|sofa|couch|bench|shelf|locker|cabinet|drawer|stool|wardrobe|bookcase') },
  { id:'security', label:'Security',   fn:b => reHit(b,'camera|guard|alarm|detector|spotlight|searchlight|fence|sniper|dog|siren|scanner|barbed') },
  { id:'lights',   label:'Lights',     fn:b => reHit(b,'light|lamp|torch|lantern|candle') },
  { id:'utility',  label:'Utilities',  fn:b => reHit(b,'generator|electric|power|switch|water|pipe|sink|toilet|shower|laundry|boiler|furnace|radiator|bin') },
  { id:'nature',   label:'Nature',     fn:b => reHit(b,'tree|bush|plant|flower|rock|grass|hedge|log|stump|snow|sand') },
  { id:'vents',    label:'Vents',      fn:b => reHit(b,'vent|duct') || (b.layers||'').toLowerCase().includes('vent') },
  { id:'jobs',     label:'Job objects',fn:b => reHit(b,'job|kitchen|workshop|mail|metal|wood|janitor|deliver|crate|press') || (b.purpose||'').toLowerCase().includes('job') },
  { id:'rooms',    label:'Rooms',      fn:b => b.kind === 'Room' },
  { id:'deco',     label:'Decoration', fn:b => b.kind === 'Decoration' },
  { id:'zones',    label:'Zones',      fn:b => b.zone },
  { id:'dev',      label:'Dev-only',   fn:b => b.editorOnly },
];

function renderFilters() {
  el('filters').innerHTML =
    `<button class='${activeFilters.size===0?""active"":""""}' onclick='clearFilters()'>All</button>` +
    FILTERS.map(f =>
      `<button class='${activeFilters.has(f.id)?""active"":""""}' onclick='toggleFilter(""${f.id}"")'>${f.label}</button>`
    ).join('');
  el('cfilters').innerHTML = (prefs.customFilters||[]).map((c,i) =>
    `<button class='custom ${customActive(c)?""active"":""""}' onclick='applyCustom(${i})'
       title='${esc(c.q||'')}${(c.filters||[]).length?' +'+(c.filters||[]).join('+'):''}'>${esc(c.name)}` +
    `<span class='chipx' onclick='delCustom(event,${i})'>×</span></button>`
  ).join('');
}

function toggleFilter(id) {
  if (activeFilters.has(id)) activeFilters.delete(id); else activeFilters.add(id);
  renderFilters(); render();
}
function clearFilters() { activeFilters.clear(); renderFilters(); render(); }

function matchesFilters(b) {
  if (activeFilters.size === 0) return true;
  for (const id of activeFilters) {
    const f = FILTERS.find(x => x.id === id);
    if (f && f.fn(b)) return true;
  }
  return false;
}

/* custom (user-saved) filters */
function saveCustomFilter() {
  const q = el('q').value.trim();
  if (!q && activeFilters.size === 0) { msg('type a search or activate filters first'); return; }
  const name = prompt('Name this filter:', q || [...activeFilters].join('+'));
  if (!name) return;
  prefs.customFilters = prefs.customFilters || [];
  prefs.customFilters.push({ name: name, q: q, filters: [...activeFilters] });
  savePrefs(); renderFilters();
  msg(`saved filter “${name}”`);
}
function applyCustom(i) {
  const c = (prefs.customFilters||[])[i];
  if (!c) return;
  el('q').value = c.q || '';
  activeFilters = new Set(c.filters || []);
  renderFilters(); render();
}
function delCustom(ev, i) {
  ev.stopPropagation();
  prefs.customFilters.splice(i, 1);
  savePrefs(); renderFilters();
}
function customActive(c) {
  return (el('q').value.trim() === (c.q||'')) &&
    JSON.stringify([...activeFilters].sort()) ===
    JSON.stringify((c.filters||[]).slice().sort());
}

/* ---- blocks tab: user arrangement + prefs persistence ---- */
function applyOrder() {
  orderRank = {};
  (prefs.order||[]).forEach((id, i) => { orderRank[id] = i; });
}
function sortedBlocks() {
  return blocks.slice().sort((a, b) => {
    const ra = orderRank[a.id], rb = orderRank[b.id];
    if (ra !== undefined && rb !== undefined) return ra - rb;
    if (ra !== undefined) return -1;
    if (rb !== undefined) return 1;
    return 0; // stable sort keeps the natural order for unranked blocks
  });
}

async function loadPrefs() {
  try {
    const p = await (await fetch('/api/prefs')).json();
    prefs = Object.assign({ customFilters: [], order: [], customBlocks: [], customCategoryName: 'Custom', animatedDefs: [] }, p);
  } catch (e) { /* server unreachable — keep defaults */ }
  applyOrder(); renderFilters(); render();
  renderCustomBlocks();
}
let prefsTimer = null;
function savePrefs() {
  clearTimeout(prefsTimer);
  prefsTimer = setTimeout(() =>
    fetch('/api/prefs', { method:'POST', body: JSON.stringify(prefs) }), 400);
}

function dragStart(ev, id) {
  dragId = id;
  ev.dataTransfer.effectAllowed = 'move';
  ev.dataTransfer.setData('text/plain', String(id)); // Firefox needs data to start a drag
  tipHide();
}
function dragOver(ev) {
  if (dragId === null) return;
  ev.preventDefault();
  ev.currentTarget.classList.add('dragover');
}
function dragLeave(ev) { ev.currentTarget.classList.remove('dragover'); }
function dropOn(ev, targetId) {
  ev.preventDefault();
  ev.currentTarget.classList.remove('dragover');
  if (dragId === null || dragId === targetId) { dragId = null; return; }
  const ids = sortedBlocks().map(b => b.id);
  ids.splice(ids.indexOf(dragId), 1);
  ids.splice(ids.indexOf(targetId), 0, dragId);
  prefs.order = ids;
  applyOrder(); savePrefs(); render();
  dragId = null;
}

function render() {
  const q = el('q').value.toLowerCase();
  const show = sortedBlocks().filter(b =>
    matchesFilters(b) &&
    (!q || b.name.toLowerCase().includes(q) || (b.internalName||'').toLowerCase().includes(q)));
  el('grid').innerHTML = show.map(b => `
    <div class='cell ${b.editorOnly?""dev"":""""} ${b.id===selectedId?""sel"":""""}' data-id='${b.id}'
         draggable='true' onclick='brush(${b.id})'
         ondragstart='dragStart(event,${b.id})' ondragover='dragOver(event)'
         ondragleave='dragLeave(event)' ondrop='dropOn(event,${b.id})'
         onmouseenter='tipShow(event,${b.id})' onmousemove='tipMove(event)' onmouseleave='tipHide()'>
      ${b.hasIcon ? `<img loading='lazy' src='/api/icon/${b.id}.png'>` : `<span class='noicon'>?</span>`}
      <div class='nm'>${b.name||('#'+b.id)}</div>
    </div>`).join('');
  msg(`${show.length}/${blocks.length} blocks`);
  renderCustomBlocks();
}

function updateSel() {
  document.querySelectorAll('#grid .cell').forEach(c =>
    c.classList.toggle('sel', +c.dataset.id === selectedId));
}

function esc(s) {
  return (s||'').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;')
                .replace(/'/g,'&#39;').replace(/""/g,'&quot;');
}

function tipShow(ev, id) {
  const b = blocks.find(x => x.id === id);
  if (!b) return;
  const rows = [];
  rows.push(`<b>${esc(b.name)||'#'+b.id}</b> <span class='meta'>#${b.id} — ${b.kind}` +
    (b.editorOnly?' — dev-only':'') + (b.zone?' — zone':'') + '</span>');
  if (b.internalName) rows.push(`<span class='meta'>internal:</span> ${esc(b.internalName)}`);
  if (b.prefab) rows.push(`<span class='meta'>prefab:</span> ${esc(b.prefab)}`);
  if (b.className) rows.push(`<span class='meta'>class:</span> ${esc(b.className)}`);
  if (b.layers) rows.push(`<span class='meta'>layers:</span> ${esc(b.layers)}`);
  if (b.limitGroup) rows.push(`<span class='meta'>group:</span> ${esc(b.limitGroup)}`);
  if (b.themes) rows.push(`<span class='meta'>themes:</span> ${esc(b.themes)}`);
  if (b.purpose) rows.push(`<span class='meta'>purpose:</span> ${esc(b.purpose)}`);
  if (b.desc) rows.push(esc(b.desc));
  if (b.notes) rows.push(`<span class='notes'>${esc(b.notes)}</span>`);
  const tip = el('tip');
  tip.innerHTML = rows.join('<br>');
  tip.style.display = 'block';
  tipMove(ev);
}

function tipMove(ev) {
  const tip = el('tip');
  if (tip.style.display === 'none') return;
  const pad = 14;
  let x = ev.clientX + pad, y = ev.clientY + pad;
  const r = tip.getBoundingClientRect();
  if (x + r.width > innerWidth - 8) x = ev.clientX - r.width - pad;
  if (y + r.height > innerHeight - 8) y = ev.clientY - r.height - pad;
  tip.style.left = x + 'px';
  tip.style.top = y + 'px';
}

function tipHide() { el('tip').style.display = 'none'; }

async function loadBlocks() {
  msg('loading…');
  try {
    blocks = await (await fetch('/api/blocks')).json();
    render();
  } catch (e) { msg('load failed: ' + e); }
}

async function brush(id) {
  const target = id === selectedId ? -1 : id; // click the selected tile again to deselect
  const r = await (await fetch('/api/brush/' + target, {method:'POST'})).json();
  if (r.ok) {
    selectedId = target;
    customSelectedId = -1;
    updateSel();
    renderCustomBlocks();
    msg(target < 0 ? 'brush cleared' : 'brush set: #' + id);
  } else {
    msg('editor not open');
  }
}

/* ---- shared ---- */
async function post(url) {
  const r = await (await fetch(url, {method:'POST'})).json();
  if (r.msg) msg(r.msg);
  return r;
}

async function setTool(name) {
  await post('/api/tool?name=' + (tool === name ? 'none' : name));
}

function setSetting(name, value, reloadList) {
  fetch(`/api/setting?name=${name}&value=${value}`, {method:'POST'})
    .then(() => { if (reloadList) loadBlocks(); });
}

function setNum(name, value) {
  fetch(`/api/numsetting?name=${name}&value=${value}`, {method:'POST'});
}

async function cheat(name) {
  await post('/api/cheat?name=' + name);
  refreshGameplay();
}

/* ---- tilesets tab ---- */
let tsAtlas = null;   // { name, w, h } of the atlas open in the picker
let tsSel = null;     // selection in 32px cells: {cx, cy, cw, ch} (top-left origin)
let tsDrag = null;

async function refreshTilesets() {
  try {
    const t = await (await fetch('/api/tilesets')).json();
    el('ts_status').textContent = t.busy ? '— ' + t.status
      : (t.atFrontend ? '' : '— harvest available from the main menu only');
    el('ts_sets').innerHTML = t.sets.map(s => {
      const state = s.cached ? `${s.atlases} atlases` : (s.installed ? 'not harvested' : 'DLC not owned');
      const open = s.cached ? `<button onclick='openSet(""${s.id}"",""${esc(s.name)}"")'>📂 ${esc(s.name)}</button>` :
        `<button ${s.installed?'':'disabled'} onclick='harvest(""${s.id}"")'>${esc(s.name)}</button>`;
      return `<span class='num'>${open}<span class='hint'>${state}</span></span>`;
    }).join('') || ""<span class='hint'>no prison data yet — wait for the main menu</span>"";
  } catch (e) { el('ts_status').textContent = '— unreachable'; }
}

async function harvest(set) {
  const r = await (await fetch('/api/tilesets/harvest?set=' + set, {method:'POST'})).json();
  msg(r.msg);
  setTimeout(refreshTilesets, 1500);
}

async function openSet(id, name) {
  el('ts_setname').textContent = name;
  el('ts_atlcard').style.display = 'block';
  const a = await (await fetch('/api/tilesets/atlases?set=' + id)).json();
  el('ts_atlases').innerHTML = a.atlases.map(x => `
    <div class='cell' onclick='openAtlas(""${x.name}"",${x.w},${x.h})'>
      <img loading='lazy' src='/api/tilesets/atlas.png?name=${encodeURIComponent(x.name)}'
           style='width:100%;height:90px;object-fit:contain'>
      <div class='nm'>${esc(x.name)}<br><span class='hint'>${x.w}×${x.h}</span></div>
    </div>`).join('') || ""<span class='hint'>no atlases in this set</span>"";
}

function openAtlas(name, w, h) {
  tsAtlas = { name, w, h };
  tsSel = null;
  el('ts_atlasname').textContent = `${name} (${w}×${h})`;
  el('ts_pickcard').style.display = 'block';
  el('ts_rect').style.display = 'none';
  el('ts_arm').disabled = true;
  el('ts_save_block').disabled = true;
  el('ts_open_anim').disabled = false;
  el('ts_img').src = '/api/tilesets/atlas.png?name=' + encodeURIComponent(name);
  // update anim editor source if it's open
  el('anim_src_atlas').textContent = name;
  el('anim_src_sel').textContent = 'none';
  if (el('anim_add_sel_btn')) el('anim_add_sel_btn').disabled = true;
  atlasZoom();
  el('ts_pickcard').scrollIntoView({behavior:'smooth'});
}

function atlasZoom() {
  if (!tsAtlas) return;
  const z = Math.max(1, Math.min(4, +el('ts_zoom').value || 2));
  el('ts_img').style.width = (tsAtlas.w * z) + 'px';
  el('ts_img').style.height = (tsAtlas.h * z) + 'px';
  el('ts_grid').style.backgroundSize = (32*z) + 'px ' + (32*z) + 'px';
  drawSel();
}

function cellFromEvent(ev) {
  const img = el('ts_img');
  const r = img.getBoundingClientRect();
  if (!r.width || !r.height) return { cx: 0, cy: 0 };
  const px = (ev.clientX - r.left) * (tsAtlas.w / r.width);
  const py = (ev.clientY - r.top) * (tsAtlas.h / r.height);
  return {
    cx: Math.max(0, Math.min(Math.floor(tsAtlas.w/32) - 1, Math.floor(px / 32))),
    cy: Math.max(0, Math.min(Math.floor(tsAtlas.h/32) - 1, Math.floor(py / 32))),
  };
}

function drawSel() {
  const rect = el('ts_rect');
  if (!tsAtlas || !tsSel) { rect.style.display = 'none'; return; }
  const z = el('ts_img').getBoundingClientRect().width / tsAtlas.w;
  rect.style.left = (tsSel.cx * 32 * z) + 'px';
  rect.style.top = (tsSel.cy * 32 * z) + 'px';
  rect.style.width = (tsSel.cw * 32 * z) + 'px';
  rect.style.height = (tsSel.ch * 32 * z) + 'px';
  rect.style.display = 'block';
  el('ts_sel').textContent =
    `selection: ${tsSel.cw}×${tsSel.ch} tile(s) @ cell (${tsSel.cx},${tsSel.cy})`;
  el('ts_arm').disabled = false;
  el('ts_save_block').disabled = false;
  // update anim editor
  el('anim_src_sel').textContent = `${tsSel.cw}×${tsSel.ch} @ (${tsSel.cx},${tsSel.cy})`;
  if (el('anim_add_sel_btn')) el('anim_add_sel_btn').disabled = false;
}

async function armStamp() {
  if (!tsAtlas || !tsSel) return;
  // convert top-left cell selection to a bottom-left pixel rect
  const x = tsSel.cx * 32;
  const h = tsSel.ch * 32;
  const y = tsAtlas.h - (tsSel.cy * 32 + h);
  const w = tsSel.cw * 32;
  const decor = el('ts_mode_decor').checked;
  await post(`/api/tilesets/stamp?atlas=${encodeURIComponent(tsAtlas.name)}` +
    `&x=${x}&y=${y}&w=${w}&h=${h}&decor=${decor}`);
}

(function bindAtlasPicker() {
  document.addEventListener('mousedown', ev => {
    if (!tsAtlas || !ev.target.closest('#ts_wrap')) return;
    ev.preventDefault();
    tsDrag = cellFromEvent(ev);
    tsSel = { cx: tsDrag.cx, cy: tsDrag.cy, cw: 1, ch: 1 };
    drawSel();
  });
  document.addEventListener('mousemove', ev => {
    if (!tsDrag) return;
    const c = cellFromEvent(ev);
    tsSel = {
      cx: Math.min(tsDrag.cx, c.cx), cy: Math.min(tsDrag.cy, c.cy),
      cw: Math.abs(c.cx - tsDrag.cx) + 1, ch: Math.abs(c.cy - tsDrag.cy) + 1,
    };
    drawSel();
  });
  document.addEventListener('mouseup', () => { tsDrag = null; });
})();

/* ---- gameplay tab ---- */
async function refreshGameplay() {
  try {
    const f = await (await fetch('/api/floors')).json();
    const key = JSON.stringify(f.floors.map(x => x.index + x.name));
    if (key !== floorsKnown) {
      floorsKnown = key;
      if (f.floors.length === 0) {
        el('floorBtns').innerHTML = ""<span class='hint'>floors appear in play mode</span>"";
      } else {
        el('floorBtns').innerHTML = f.floors.map(x =>
          `<button id='fl_${x.index}' onclick='pickFloor(${x.index})'>` +
          `${esc(x.name)}${x.start?' ★':''}</button>`).join('');
        if (curFloor < 0 || !f.floors.some(x => x.index === curFloor)) {
          const start = f.floors.find(x => x.start) || f.floors[0];
          pickFloor(start.index);
        }
      }
    }
  } catch (e) { /* not reachable */ }
  refreshPlayer();
}

function pickFloor(i) {
  curFloor = i;
  document.querySelectorAll(""[id^='fl_']"").forEach(b =>
    b.classList.toggle('active', b.id === 'fl_' + i));
  el('mapimg').src = '/api/map/' + i + '.png?t=' + Date.now();
}

function mapError() {
  el('pmark').style.display = 'none';
}

async function refreshPlayer() {
  try {
    const p = await (await fetch('/api/player')).json();
    if (!p.present) {
      el('pstats').innerHTML = ""<span class='hint'>no player — start/test a map first</span>"";
      el('pmark').style.display = 'none';
      return;
    }
    el('pstats').innerHTML =
      `<span><b>${esc(p.name)}</b></span>` +
      `<span>❤ <b>${p.health}</b></span><span>⚡ <b>${p.energy}</b></span>` +
      `<span>$ <b>${p.money}</b></span><span>🔥 heat <b>${p.heat}</b></span>` +
      (p.tile ? `<span>tile <b>(${p.tile.x},${p.tile.y})</b> floor <b>${p.tile.floor}</b></span>` : '');
    el('s_infiniteEnergy').checked = p.infiniteEnergy;
    placeMarker(p.tile);
  } catch (e) { /* ignore */ }
}

function placeMarker(tile) {
  const img = el('mapimg'), mark = el('pmark');
  if (!tile || tile.floor !== curFloor || !img.naturalWidth) {
    mark.style.display = 'none';
    return;
  }
  const sx = img.clientWidth / img.naturalWidth;
  const sy = img.clientHeight / img.naturalHeight;
  mark.style.left = ((1 + (tile.x + 0.5) * mapTileSize) * sx) + 'px';
  mark.style.top = (img.clientHeight - (1 + (tile.y + 0.5) * mapTileSize) * sy) + 'px';
  mark.style.display = 'block';
}

async function mapClick(ev) {
  const img = el('mapimg');
  if (!img.naturalWidth || curFloor < 0) return;
  const r = img.getBoundingClientRect();
  const px = (ev.clientX - r.left) * (img.naturalWidth / r.width);
  const py = (ev.clientY - r.top) * (img.naturalHeight / r.height);
  const tx = Math.max(0, Math.min(119, Math.floor((px - 1) / mapTileSize)));
  const ty = Math.max(0, Math.min(119, Math.floor((img.naturalHeight - py - 1) / mapTileSize)));
  await post(`/api/teleport?x=${tx}&y=${ty}&floor=${curFloor}`);
  refreshPlayer();
}

/* ---- status poll ---- */
async function poll() {
  try {
    const s = await (await fetch('/api/state')).json();
    el('status').textContent =
      (s.inEditor ? 'EDITOR' : 'menu/play') +
      (s.cursor ? ` — cursor (${s.cursor.x},${s.cursor.y})` : '') +
      ` — fences ${s.fences}, links ${s.triggers}, tiles ${s.modTiles}` +
      (s.animatedTiles ? `, animated ${s.animatedTiles}` : '') +
      (s.missingAtlases && s.missingAtlases.length
        ? ` — ⚠ missing atlases: ${s.missingAtlases.join(', ')} (harvest their sets!)` : '') +
      (s.tool && s.tool !== 'none' ? ` — TOOL ${s.tool}: ${s.toolHint}` : '');
    tool = s.tool || 'none';
    ['paint','erase','link','tileerase'].forEach(t =>
      el('tool_' + t).classList.toggle('active', tool === t));
    el('ts_arm').classList.toggle('active', tool === 'tilepaint');
    el('requiresMod').checked = s.requiresMod;
    el('s_devBlocks').checked = s.settings.devBlocks;
    el('s_dlcBlocks').checked = s.settings.dlcBlocks;
    el('s_layers').checked = s.settings.layers;
    el('s_completion').checked = s.settings.completion;
    el('s_restrictions').checked = s.settings.restrictions;
    el('s_xray').checked = s.settings.xray;
    el('s_fenceOverlay').checked = s.settings.fenceOverlay;
    el('s_arrows').checked = s.settings.arrows;
    el('s_cameraLock').checked = s.settings.cameraLock;
    el('s_forceWindowed').checked = s.settings.forceWindowed;
    el('s_autoOpen').checked = s.settings.autoOpen;
    el('s_vanillaFallback').checked = s.settings.vanillaFallback;
    el('s_infiniteEnergy').checked = s.settings.infiniteEnergy;
    for (const k of ['guardCap','zoomSteps','fenceDamage','windowWidth','windowHeight']) {
      const box = el('n_' + k);
      if (box && document.activeElement !== box) box.value = s.numbers[k];
    }
    if (typeof s.brush === 'number' && s.brush !== selectedId) {
      selectedId = s.brush;  // selection changed from inside the game
      updateSel();
    }
    if (s.inEditor && blocks.length === 0) loadBlocks();
    if (el('page_gameplay').classList.contains('on')) refreshPlayer();
  } catch (e) {
    el('status').textContent = 'game not reachable';
  }
  setTimeout(poll, 1000);
}

/* ---- modal ---- */
function openModal(title, nameVal, descVal, callback, nameOnly) {
  el('modal_title').textContent = title;
  el('modal_name').value = nameVal || '';
  el('modal_name').classList.remove('error');
  el('modal_err').style.display = 'none';
  el('modal_desc').value = descVal || '';
  el('modal_box').classList.toggle('nameonly', !!nameOnly);
  modalCallback = callback;
  el('modal_overlay').classList.add('show');
  el('modal_name').focus();
}
function cancelModal() {
  el('modal_overlay').classList.remove('show');
  modalCallback = null;
}
function confirmModal() {
  const name = el('modal_name').value.trim();
  if (!name) {
    el('modal_name').classList.add('error');
    el('modal_err').style.display = 'block';
    el('modal_name').focus();
    return;
  }
  const desc = el('modal_desc').value.trim();
  if (modalCallback) modalCallback(name, desc);
  cancelModal();
}
el('modal_name').addEventListener('input', () => {
  if (el('modal_name').value.trim()) {
    el('modal_name').classList.remove('error');
    el('modal_err').style.display = 'none';
  }
});
document.addEventListener('keydown', ev => {
  if (ev.key === 'Escape' && el('modal_overlay').classList.contains('show')) cancelModal();
});

/* ---- custom building blocks ---- */
function saveAsCustomBlock() {
  if (!tsAtlas || !tsSel) return;
  openModal('Add custom building block', '', '', (name, desc) => {
    const x = tsSel.cx * 32;
    const h = tsSel.ch * 32;
    const y = tsAtlas.h - (tsSel.cy * 32 + h);
    const w = tsSel.cw * 32;
    const decor = el('ts_mode_decor').checked;
    const category = prefs.customCategoryName || 'Custom';
    prefs.customBlocks = prefs.customBlocks || [];
    prefs.customBlocks.push({
      name, desc, category,
      atlas: tsAtlas.name, atlasH: tsAtlas.h,
      x, y, w, h, decor
    });
    savePrefs();
    renderCustomBlocks();
    msg(`saved custom block ""${name}""`);
  });
}

async function brushCustomBlock(i) {
  const cb = (prefs.customBlocks || [])[i];
  if (!cb) return;
  const r = await post(
    `/api/tilesets/stamp?atlas=${encodeURIComponent(cb.atlas)}&x=${cb.x}&y=${cb.y}&w=${cb.w}&h=${cb.h}&decor=${cb.decor}`
  );
  if (r.ok || r.msg) {
    customSelectedId = i;
    selectedId = -1;
    updateSel();
    renderCustomBlocks();
    msg(r.msg || `armed: ${cb.name}`);
  }
}

function deleteCustomBlock(i) {
  const cb = (prefs.customBlocks || [])[i];
  if (!cb) return;
  if (!confirm(`Delete custom block ""${cb.name}""?`)) return;
  prefs.customBlocks.splice(i, 1);
  if (customSelectedId === i) customSelectedId = -1;
  else if (customSelectedId > i) customSelectedId--;
  // safety clamp: ensure selection is within bounds
  if (customSelectedId >= prefs.customBlocks.length) customSelectedId = -1;
  savePrefs();
  renderCustomBlocks();
}

function renameCategoryPrompt(catName) {
  const cur = catName || 'Custom';
  openModal('Rename category', cur, '', (name) => {
    // rename in static blocks
    (prefs.customBlocks || []).forEach(cb => {
      if ((cb.category || prefs.customCategoryName || 'Custom') === cur) cb.category = name;
    });
    // rename in animated defs
    (prefs.animatedDefs || []).forEach(def => {
      if ((def.category || 'animated') === cur) def.category = name;
    });
    if ((prefs.customCategoryName || 'Custom') === cur) prefs.customCategoryName = name;
    savePrefs();
    renderCustomBlocks();
  }, true);
}

function renderCustomBlocks() {
  const staticBlocks = prefs.customBlocks || [];
  const animDefs = prefs.animatedDefs || [];
  const section = el('cb_section');
  if (staticBlocks.length === 0 && animDefs.length === 0) { section.style.display = 'none'; return; }
  section.style.display = '';
  const q = (el('q') ? el('q').value : '').toLowerCase();

  // gather all categories
  const catMap = {}; // cat -> { static: [{i, cb}], anim: [{i, def}] }
  staticBlocks.forEach((cb, i) => {
    const cat = cb.category || prefs.customCategoryName || 'Custom';
    if (!catMap[cat]) catMap[cat] = { static: [], anim: [] };
    if (!q || cb.name.toLowerCase().includes(q)) catMap[cat].static.push({ i, cb });
  });
  animDefs.forEach((def, i) => {
    const cat = def.category || 'animated';
    if (!catMap[cat]) catMap[cat] = { static: [], anim: [] };
    if (!q || def.name.toLowerCase().includes(q)) catMap[cat].anim.push({ i, def });
  });

  const cats = Object.keys(catMap);
  el('cb_cats').innerHTML = cats.map(cat => {
    const items = catMap[cat];
    const staticHtml = items.static.map(({ i, cb }) => `
      <div class='cb_cell ${i === customSelectedId ? ""sel"" : """"}' onclick='brushCustomBlock(${i})'
           onmouseenter='cbTipShow(event,${i})' onmousemove='tipMove(event)' onmouseleave='tipHide()'>
        <span class='cb_del' onclick='event.stopPropagation();deleteCustomBlock(${i})'>×</span>
        <span class='cb_noicon'>🧱</span>
        <div class='nm'>${esc(cb.name)}</div>
      </div>`).join('');
    const animHtml = items.anim.map(({ i, def }) => `
      <div class='cb_cell anim ${i === animSelectedId ? ""sel"" : """"}' onclick='brushAnimBlock(${i})'
           onmouseenter='animTipShow(event,${i})' onmousemove='tipMove(event)' onmouseleave='tipHide()'>
        <span class='cb_del' onclick='event.stopPropagation();deleteAnimBlock(${i})'>×</span>
        <span class='cb_noicon'>🎬</span>
        <div class='nm'>${esc(def.name)}</div>
        <div style='font-size:9px;color:#6688aa'>${def.frames.length}f@${def.fps}fps</div>
      </div>`).join('');
    return `<div class='cat-section'>
      <div class='cat-hdr'>
        <h2>${esc(cat)}</h2>
        <button onclick='renameCategoryPrompt(""${esc(cat)}"")' title='Rename category' style='padding:2px 7px;font-size:11px'>✎</button>
      </div>
      <div class='cb_grid'>${staticHtml}${animHtml}</div>
    </div>`;
  }).join('');
}

function cbTipShow(ev, i) {
  const cb = (prefs.customBlocks || [])[i];
  if (!cb) return;
  const rows = [];
  rows.push(`<b>${esc(cb.name)}</b> <span class='meta'>custom block</span>`);
  rows.push(`<span class='meta'>atlas:</span> ${esc(cb.atlas)}`);
  rows.push(`<span class='meta'>region:</span> ${cb.w/32}×${cb.h/32} tile(s) — ${cb.decor?'decor (over characters)':'floor (under characters)'}`);
  if (cb.desc) rows.push(esc(cb.desc));
  const tip = el('tip');
  tip.innerHTML = rows.join('<br>');
  tip.style.display = 'block';
  tipMove(ev);
}

function animTipShow(ev, i) {
  const def = (prefs.animatedDefs || [])[i];
  if (!def) return;
  const rows = [];
  rows.push(`<b>${esc(def.name)}</b> <span class='meta'>animated tile</span>`);
  rows.push(`<span class='meta'>frames:</span> ${def.frames.length} @ ${def.fps} fps${def.pingpong?' (ping-pong)':''}`);
  rows.push(`<span class='meta'>layer:</span> ${def.decor?'decor':'floor'}`);
  if (def.desc) rows.push(esc(def.desc));
  const tip = el('tip');
  tip.innerHTML = rows.join('<br>');
  tip.style.display = 'block';
  tipMove(ev);
}

/* ---- animation editor state ---- */
let animFrames = [];        // [{atlas, atlasH, x, y, w, h}] pixel coords, bottom-left origin
let animPreviewTimer = null;
let animPreviewIdx = 0;
let animPreviewDir = 1;
let animSelectedId = -1;    // index in prefs.animatedDefs for active animated stamp

/* helpers */
function selToFrame() {
  if (!tsAtlas || !tsSel) return null;
  const w = tsSel.cw * 32, h = tsSel.ch * 32;
  const x = tsSel.cx * 32;
  const y = tsAtlas.h - (tsSel.cy * 32 + h);  // bottom-left
  return { atlas: tsAtlas.name, atlasH: tsAtlas.h, x, y, w, h };
}

function animTimingHint() {
  const fps = Math.max(1, +el('anim_fps').value || 4);
  const n = animFrames.length;
  if (n === 0) { el('anim_timing_hint').textContent = ''; return; }
  const dur = (n / fps).toFixed(2);
  el('anim_timing_hint').textContent = `${n} frame${n===1?'':'s'} × ${(1000/fps).toFixed(0)}ms = ${dur}s per cycle`;
}

function openAnimEditorEmpty() {
  animFrames = [];
  el('anim_name_field').value = '';
  el('anim_cat_field').value = 'animated';
  el('anim_desc_field').value = '';
  el('anim_fps').value = '4';
  el('anim_loop').checked = true;
  el('anim_pingpong').checked = false;
  el('anim_layer_floor').checked = true;
  el('anim_save_err').style.display = 'none';
  el('anim_detect_status').style.display = 'none';
  if (tsAtlas) el('anim_src_atlas').textContent = tsAtlas.name;
  else el('anim_src_atlas').textContent = 'none selected';
  el('anim_src_sel').textContent = tsSel ? `${tsSel.cw}×${tsSel.ch} @ (${tsSel.cx},${tsSel.cy})` : 'none';
  if (el('anim_add_sel_btn')) el('anim_add_sel_btn').disabled = !tsSel;
  stopAnimPreview();
  renderAnimFrames();
  el('anim_overlay').classList.add('show');
}

function openAnimEditorFromSelection() {
  openAnimEditorEmpty();
  if (tsAtlas && tsSel) {
    const frame = selToFrame();
    if (frame) { animFrames = [frame]; renderAnimFrames(); }
  }
}

function closeAnimEditor() {
  stopAnimPreview();
  el('anim_overlay').classList.remove('show');
}

function addCurrentSelectionAsFrame() {
  const frame = selToFrame();
  if (!frame) { msg('Select a region in a tileset atlas first'); return; }
  animFrames.push(frame);
  renderAnimFrames();
  animTimingHint();
}

function removeAnimFrame(i) {
  animFrames.splice(i, 1);
  if (animPreviewIdx >= animFrames.length) animPreviewIdx = 0;
  renderAnimFrames();
  animTimingHint();
}

function moveAnimFrame(i, delta) {
  const j = i + delta;
  if (j < 0 || j >= animFrames.length) return;
  [animFrames[i], animFrames[j]] = [animFrames[j], animFrames[i]];
  renderAnimFrames();
}

function clearAnimFrames() {
  if (animFrames.length > 0 && !confirm('Clear all frames?')) return;
  animFrames = [];
  stopAnimPreview();
  renderAnimFrames();
  animTimingHint();
}

function updatePreviewCanvasSize() {
  if (animFrames.length === 0) return;
  const f = animFrames[0];
  const scale = Math.max(1, Math.min(8, +el('anim_preview_scale').value || 2));
  el('anim_preview_canvas').width = f.w * scale;
  el('anim_preview_canvas').height = f.h * scale;
}

function restartPreviewIfRunning() {
  if (animPreviewTimer) { stopAnimPreview(); previewAnimEditor(); }
  animTimingHint();
}

async function drawFrameToCanvas(canvas, frame) {
  const scale = Math.max(1, Math.min(8, +el('anim_preview_scale').value || 2));
  canvas.width = frame.w * scale;
  canvas.height = frame.h * scale;
  const ctx = canvas.getContext('2d');
  ctx.clearRect(0, 0, canvas.width, canvas.height);
  // load the atlas image and draw the region
  const img = new Image();
  img.src = '/api/tilesets/atlas.png?name=' + encodeURIComponent(frame.atlas);
  await new Promise(res => { img.onload = res; img.onerror = res; });
  if (!img.complete || img.naturalWidth === 0) {
    ctx.fillStyle = '#334';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = '#aaa';
    ctx.font = '10px sans-serif';
    ctx.fillText('?', 4, 14);
    return;
  }
  // frame coords are bottom-left; convert to top-left for canvas
  const atlasH = frame.atlasH || img.naturalHeight;
  const srcY = atlasH - frame.y - frame.h;
  ctx.imageSmoothingEnabled = false;
  ctx.drawImage(img, frame.x, srcY, frame.w, frame.h, 0, 0, frame.w * scale, frame.h * scale);
}

function renderAnimFrames() {
  const strip = el('anim_frame_strip');
  const count = animFrames.length;
  el('anim_frame_count').textContent = `(${count} frame${count===1?'':'s'})`;
  // render frame thumbnails
  let html = '';
  animFrames.forEach((f, i) => {
    html += `<div class='frame-item' id='frame_item_${i}'>
      <canvas id='frame_canvas_${i}' width='64' height='64' title='Frame ${i+1}: ${f.atlas} ${f.w}×${f.h}'></canvas>
      <span class='frame-idx'>${i+1}</span>
      <span class='frame-del' onclick='removeAnimFrame(${i})' title='Remove frame'>×</span>
      <div style='display:flex;justify-content:space-between;padding:1px 2px'>
        <span onclick='moveAnimFrame(${i},-1)' style='cursor:pointer;color:#668;font-size:9px' title='Move left'>◀</span>
        <span onclick='moveAnimFrame(${i},1)' style='cursor:pointer;color:#668;font-size:9px' title='Move right'>▶</span>
      </div>
    </div>`;
  });
  html += `<div class='frame-add-btn' onclick='addCurrentSelectionAsFrame()' title='Add current atlas selection as frame'>＋</div>`;
  strip.innerHTML = html;
  // draw each frame asynchronously
  animFrames.forEach((f, i) => {
    const cv = el('frame_canvas_' + i);
    if (cv) drawFrameToCanvas(cv, f);
  });
  animTimingHint();
  // update preview canvas size if no preview running
  if (!animPreviewTimer && count > 0) {
    const scale = Math.max(1, Math.min(8, +el('anim_preview_scale').value || 2));
    el('anim_preview_canvas').width = animFrames[0].w * scale;
    el('anim_preview_canvas').height = animFrames[0].h * scale;
  }
  el('anim_frame_disp').textContent = `Frame ${count > 0 ? animPreviewIdx+1 : 0}/${count}`;
}

function previewAnimEditor() {
  if (animFrames.length === 0) { msg('Add at least one frame first'); return; }
  stopAnimPreview();
  animPreviewIdx = 0;
  animPreviewDir = 1;
  const fps = Math.max(1, Math.min(60, +el('anim_fps').value || 4));
  const loop = el('anim_loop').checked;
  const pingpong = el('anim_pingpong').checked;
  function tick() {
    drawFrameToCanvas(el('anim_preview_canvas'), animFrames[animPreviewIdx]);
    el('anim_frame_disp').textContent = `Frame ${animPreviewIdx+1}/${animFrames.length}`;
    if (pingpong) {
      animPreviewIdx += animPreviewDir;
      if (animPreviewIdx >= animFrames.length - 1) { animPreviewIdx = animFrames.length - 1; animPreviewDir = -1; }
      if (animPreviewIdx <= 0) { animPreviewIdx = 0; animPreviewDir = 1; if (!loop) { stopAnimPreview(); return; } }
    } else {
      animPreviewIdx = (animPreviewIdx + 1);
      if (animPreviewIdx >= animFrames.length) {
        if (!loop) { animPreviewIdx = animFrames.length - 1; stopAnimPreview(); return; }
        animPreviewIdx = 0;
      }
    }
    animPreviewTimer = setTimeout(tick, 1000 / fps);
  }
  tick();
}

function stopAnimPreview() {
  if (animPreviewTimer) { clearTimeout(animPreviewTimer); animPreviewTimer = null; }
}

async function autoDetectFrames() {
  if (!tsAtlas || !tsSel) { msg('Select a region in a tileset atlas first to use as template frame'); return; }
  el('anim_detect_status').style.display = 'block';
  el('anim_detect_status').textContent = '🔍 Loading atlas image…';
  el('anim_detect_btn').disabled = true;
  try {
    const img = new Image();
    img.crossOrigin = 'anonymous';
    img.src = '/api/tilesets/atlas.png?name=' + encodeURIComponent(tsAtlas.name) + '&t=' + Date.now();
    await new Promise((res, rej) => { img.onload = res; img.onerror = rej; });
    // draw to offscreen canvas to read pixels
    const cv = document.createElement('canvas');
    cv.width = img.naturalWidth; cv.height = img.naturalHeight;
    const ctx = cv.getContext('2d');
    ctx.drawImage(img, 0, 0);
    function regionHasContent(x, y, w, h) {
      if (x < 0 || y < 0 || x + w > cv.width || y + h > cv.height) return false;
      const d = ctx.getImageData(x, y, w, h).data;
      // alpha threshold: >10/255 (~0.04), matching RegionHasContent in TileSets.cs
      for (let i = 3; i < d.length; i += 4) if (d[i] > 10) return true;
      return false;
    }
    // selection in top-left canvas coords
    const sw = tsSel.cw * 32, sh = tsSel.ch * 32;
    const sx = tsSel.cx * 32, sy = tsSel.cy * 32;
    const found = [];
    // include the original selection as frame 0
    found.push({ cx: tsSel.cx, cy: tsSel.cy });
    // scan horizontally right
    let cx = tsSel.cx + tsSel.cw, maxScan = Math.floor(cv.width / 32);
    while (cx + tsSel.cw <= Math.floor(cv.width / 32) && found.length < 128) {
      if (!regionHasContent(cx * 32, sy, sw, sh)) break;
      found.push({ cx, cy: tsSel.cy });
      cx += tsSel.cw;
    }
    // scan horizontally left (insert at beginning)
    cx = tsSel.cx - tsSel.cw;
    const leftFrames = [];
    while (cx >= 0 && leftFrames.length < 64) {
      if (!regionHasContent(cx * 32, sy, sw, sh)) break;
      leftFrames.unshift({ cx, cy: tsSel.cy });
      cx -= tsSel.cw;
    }
    // scan vertically down
    let cy = tsSel.cy + tsSel.ch;
    const downFrames = [];
    while (cy + tsSel.ch <= Math.floor(cv.height / 32) && downFrames.length < 64) {
      if (!regionHasContent(sx, cy * 32, sw, sh)) break;
      downFrames.push({ cx: tsSel.cx, cy });
      cy += tsSel.ch;
    }
    // scan vertically up
    cy = tsSel.cy - tsSel.ch;
    const upFrames = [];
    while (cy >= 0 && upFrames.length < 64) {
      if (!regionHasContent(sx, cy * 32, sw, sh)) break;
      upFrames.unshift({ cx: tsSel.cx, cy });
      cy -= tsSel.ch;
    }
    // pick best direction (most frames found)
    const horiz = [...leftFrames, ...found];
    const vert = [...upFrames, { cx: tsSel.cx, cy: tsSel.cy }, ...downFrames];
    const best = horiz.length >= vert.length ? horiz : vert;
    if (best.length < 2) {
      el('anim_detect_status').textContent = '⚠ Only 1 frame found — try a different selection or direction.';
      el('anim_detect_btn').disabled = false;
      return;
    }
    // convert to frame objects
    const newFrames = best.map(({ cx: fcx, cy: fcy }) => {
      const fw = tsSel.cw * 32, fh = tsSel.ch * 32;
      const fx = fcx * 32;
      const fy = tsAtlas.h - (fcy * 32 + fh);  // bottom-left
      return { atlas: tsAtlas.name, atlasH: tsAtlas.h, x: fx, y: fy, w: fw, h: fh };
    });
    animFrames = newFrames;
    renderAnimFrames();
    el('anim_detect_status').textContent = `✅ Found ${newFrames.length} frames (${best === horiz ? 'horizontal' : 'vertical'} strip).`;
    msg(`Auto-detected ${newFrames.length} animation frames`);
  } catch (e) {
    el('anim_detect_status').textContent = '❌ Error: ' + e.message;
  }
  el('anim_detect_btn').disabled = false;
}

function saveAnimBlock() {
  const name = el('anim_name_field').value.trim();
  el('anim_save_err').style.display = 'none';
  if (!name) {
    el('anim_save_err').textContent = 'Name is required.';
    el('anim_save_err').style.display = 'block';
    el('anim_name_field').focus();
    return;
  }
  if (animFrames.length === 0) {
    el('anim_save_err').textContent = 'Add at least one frame before saving.';
    el('anim_save_err').style.display = 'block';
    return;
  }
  const fps = Math.max(1, Math.min(60, +el('anim_fps').value || 4));
  const loop = el('anim_loop').checked;
  const pingpong = el('anim_pingpong').checked;
  const decor = el('anim_layer_decor').checked;
  const cat = el('anim_cat_field').value.trim() || 'animated';
  const desc = el('anim_desc_field').value.trim();
  const id = Date.now().toString(36) + Math.random().toString(36).slice(2, 6);
  prefs.animatedDefs = prefs.animatedDefs || [];
  prefs.animatedDefs.push({ id, name, desc, category: cat, fps, loop, pingpong, decor, frames: animFrames.map(f => ({...f})) });
  savePrefs();
  renderCustomBlocks();
  msg(`Saved animated tile ""${name}"" to category ""${cat}""`);
  closeAnimEditor();
}

async function brushAnimBlock(i) {
  const def = (prefs.animatedDefs || [])[i];
  if (!def || def.frames.length === 0) return;
  // serialize frames for the API: atlas:rx:ry:rw:rh;...
  const framesParam = def.frames.map(f => `${f.atlas}:${f.x}:${f.y}:${f.w}:${f.h}`).join(';');
  const r = await (await fetch(
    `/api/tilesets/arm-animated?fps=${def.fps}&loop=${def.loop?1:0}&pingpong=${def.pingpong?1:0}&decor=${def.decor?1:0}&frames=${encodeURIComponent(framesParam)}`,
    {method:'POST'}
  )).json();
  if (r.ok || r.msg) {
    animSelectedId = i;
    customSelectedId = -1;
    selectedId = -1;
    updateSel();
    renderCustomBlocks();
    msg(r.msg || `armed animated: ${def.name}`);
  }
}

function deleteAnimBlock(i) {
  const def = (prefs.animatedDefs || [])[i];
  if (!def) return;
  if (!confirm(`Delete animated tile ""${def.name}""?`)) return;
  prefs.animatedDefs.splice(i, 1);
  if (animSelectedId === i) animSelectedId = -1;
  else if (animSelectedId > i) animSelectedId--;
  savePrefs();
  renderCustomBlocks();
}

document.addEventListener('keydown', ev => {
  if (ev.key === 'Escape' && el('anim_overlay').classList.contains('show')) closeAnimEditor();
});

el('q').addEventListener('input', () => { renderFilters(); render(); });
renderFilters();
padMain();
poll();
loadPrefs();
loadBlocks();
setInterval(() => {
  if (el('page_gameplay').classList.contains('on')) refreshGameplay();
  if (el('page_tilesets').classList.contains('on')) refreshTilesets();
}, 3000);
</script>
</body>
</html>";
    }
}
