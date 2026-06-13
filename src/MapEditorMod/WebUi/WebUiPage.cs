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
    <div id='grid'></div>
  </div>

  <div class='page' id='page_tilesets'>
    <div class='card'>
      <h2>Content sets <span id='ts_status' style='text-transform:none;color:#8d8da8'></span></h2>
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
        <button id='tool_tileerase' onclick='setTool(""tileerase"")'>✕ Erase modded tiles</button>
        <button class='danger' onclick='post(""/api/tiles/clear"")'>Clear ALL modded tiles</button>
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
<div id='tip'></div>

<script>
let blocks = [];
let tool = 'none';
let selectedId = -1;                       // block currently on the editor brush
let activeFilters = new Set();             // ids of active built-in filter chips
let prefs = { customFilters: [], order: [] };
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
    prefs = Object.assign({ customFilters: [], order: [] }, p);
  } catch (e) { /* server unreachable — keep defaults */ }
  applyOrder(); renderFilters(); render();
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
    updateSel();
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
  el('ts_img').src = '/api/tilesets/atlas.png?name=' + encodeURIComponent(name);
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
    if (ev.target.id !== 'ts_img' || !tsAtlas) return;
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
