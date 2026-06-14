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
  canvas.cb_anim { width:64px; height:64px; display:block; image-rendering:pixelated; border-radius:4px; background:#0c0c14; }
  canvas.cb_thumb { width:64px; height:64px; display:block; image-rendering:pixelated; border-radius:4px; background:#0c0c14; }
  canvas.cb_combo { width:64px; height:64px; display:block; image-rendering:pixelated; border-radius:4px; background:#0c0c14; }
  #anim_pick_bar { position:fixed; bottom:0; left:0; right:0; z-index:250; display:none;
    align-items:center; gap:12px; padding:10px 16px; background:#1a1a28; border-top:2px solid #4a4a80;
    box-shadow:0 -4px 24px rgba(0,0,0,.45); }
  #anim_pick_bar span { flex:1; font-size:13px; color:#ccc; }
  #anim_pick_bar button { font-size:12px; padding:6px 14px; }
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
  #combo_overlay, #ct_overlay { position:fixed; inset:0; background:rgba(0,0,0,.82); z-index:320;
                  display:none; align-items:flex-start; justify-content:center; overflow:auto; }
  #combo_overlay.show, #ct_overlay.show { display:flex; }
  #combo_box, #ct_box { background:#181824; border:1px solid #48486e; border-radius:14px;
              padding:16px 18px; width:min(1180px,98vw); margin:16px auto; box-sizing:border-box; }
  .combo-layout { display:grid; grid-template-columns:1fr 260px; gap:12px; align-items:start; }
  .combo-panel { background:#101018; border:1px solid #2b2b3d; border-radius:8px; padding:10px; }
  .combo-grid-wrap { overflow:auto; max-height:70vh; border:1px solid #33334a; background:#090910; padding:8px; }
  #combo_grid { display:grid; gap:1px; width:max-content; }
  .combo-cell { width:34px; height:34px; background:#171722; border:1px solid #242438; box-sizing:border-box;
                cursor:pointer; position:relative; image-rendering:pixelated; }
  .combo-cell:hover { outline:2px solid #7a7af0; z-index:1; }
  .combo-cell.filled { background:#263026; border-color:#557755; }
  .combo-cell canvas { width:32px; height:32px; display:block; image-rendering:pixelated; }
  .combo-cell .badge { position:absolute; right:1px; bottom:0; font-size:8px; color:#eef; background:rgba(0,0,0,.55); }
  .combo-recent { display:flex; gap:6px; overflow:auto; padding:6px; border:1px solid #33334a; background:#080810; margin-bottom:8px; }
  .combo-recent-item { flex:0 0 auto; width:58px; height:74px; border:1px solid #33334a; border-radius:6px; background:#11111b;
    text-align:center; cursor:pointer; font-size:9px; color:#99a; position:relative; }
  .combo-recent-item.active { border-color:#7fd17f; background:#1e2c1e; }
  .combo-recent-item canvas { width:48px; height:48px; margin:4px auto 1px; display:block; image-rendering:pixelated; }
  #combo_pick_bar { position:fixed; bottom:0; left:0; right:0; z-index:330; display:none; align-items:center; gap:12px;
    padding:10px 16px; background:#1a1a28; border-top:2px solid #4a4a80; box-shadow:0 -4px 24px rgba(0,0,0,.45); }
  #combo_pick_bar span { flex:1; font-size:13px; color:#ccc; }
  .combo-toolbar { display:flex; gap:8px; flex-wrap:wrap; align-items:center; margin-bottom:8px; }
  .combo-toolbar input[type=number] { width:56px; background:#0e0e12; border:1px solid #333; color:#eee;
    padding:4px 6px; border-radius:5px; }
  .guide { color:#9aa; font-size:12px; line-height:1.45; }
  .ct-options { display:flex; flex-wrap:wrap; gap:8px; align-items:center; margin-bottom:10px; }
  .ct-options select, .ct-options input { background:#0e0e12; border:1px solid #333; color:#eee;
    padding:5px 8px; border-radius:5px; }
  .ct-source { display:flex; gap:6px; overflow:auto; padding:8px; border:1px solid #33334a;
    background:#080810; margin:8px 0 12px; min-height:76px; }
  .ct-source-tile { width:58px; height:74px; flex:0 0 auto; border:1px solid #33334a; border-radius:6px;
    background:#11111b; cursor:grab; text-align:center; font-size:9px; color:#99a; position:relative; }
  .ct-source-tile.used { opacity:.35; filter:grayscale(1); }
  .ct-source-tile canvas { width:48px; height:48px; margin:4px auto 1px; image-rendering:pixelated; display:block; }
  .ct-main { display:grid; grid-template-columns:minmax(360px,1fr) minmax(360px,1fr); gap:12px; align-items:start; }
  .ct-slot-grid { display:grid; gap:5px; width:max-content; margin:auto; }
  .ct-slot { width:70px; height:82px; border:2px solid #34344c; background:#0e0e18; border-radius:7px;
    position:relative; text-align:center; cursor:pointer; }
  .ct-slot canvas { width:64px; height:64px; margin:3px auto 0; image-rendering:pixelated; display:block;
    background:#191923; border-radius:4px; }
  .ct-slot.empty canvas { opacity:.30; }
  .ct-slot:hover { border-color:#7a9af0; }
  .ct-slot .mask { position:absolute; left:3px; top:2px; background:rgba(0,0,0,.65); color:#dde;
    font-size:9px; padding:0 3px; border-radius:3px; }
  .ct-slot .var { position:absolute; right:3px; top:2px; background:#314b31; color:#dff; font-size:9px;
    padding:0 3px; border-radius:3px; }
  .ct-preview-wrap { position:relative; width:max-content; margin:auto; }
  .ct-preview-grid, #ct_pattern { display:grid; gap:1px; background:#050509; padding:8px; border:1px solid #33334a; width:max-content; }
  .ct-cell { width:34px; height:34px; border:1px solid #242438; background:#151520; cursor:pointer; position:relative; }
  .ct-cell.on { background:#2c522c; border-color:#78b878; }
  .ct-cell canvas { width:32px; height:32px; image-rendering:pixelated; display:block; }
  .ct-cell.ghost canvas { opacity:.35; }
  .ct-full { margin-top:12px; }
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
  .detect-grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(140px,1fr)); gap:8px; margin-top:8px; }
  .detect-grid label { font-size:11px; color:#bbb; display:flex; flex-direction:column; gap:3px; }
  .detect-grid input, .detect-grid select { background:#0e0e12; border:1px solid #333; color:#eee;
    padding:4px 6px; border-radius:5px; font-size:12px; }

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

  /* rainbow feature button */
  @keyframes rainbow-shift {
    0% { background-position: 0% 50%; }
    100% { background-position: 200% 50%; }
  }
  @keyframes rainbow-glow {
    0%, 100% { box-shadow: 0 0 8px rgba(255,100,200,.45), 0 0 18px rgba(100,180,255,.25); }
    50% { box-shadow: 0 0 14px rgba(255,220,100,.55), 0 0 26px rgba(120,255,180,.35); }
  }
  .btn-rainbow {
    background: linear-gradient(90deg,#ff3366,#ff9933,#ffee33,#33ee77,#3399ff,#aa55ff,#ff3366);
    background-size: 200% 100%;
    animation: rainbow-shift 2.8s linear infinite, rainbow-glow 2s ease-in-out infinite;
    border: 2px solid rgba(255,255,255,.35);
    color: #fff;
    font-weight: 700;
    font-size: 13px;
    padding: 8px 16px;
    border-radius: 8px;
    text-shadow: 0 1px 3px rgba(0,0,0,.55);
    letter-spacing: .02em;
  }
  .btn-rainbow:hover { filter: brightness(1.08); transform: translateY(-1px); }

  /* virtual map layer paper stack */
  #layer_overlay { position:fixed; inset:0; background:rgba(0,0,0,.84); z-index:340;
                   display:none; align-items:center; justify-content:center; padding:20px; }
  #layer_overlay.show { display:flex; }
  #layer_box { background:#1a1a22; border:1px solid #3a3a4c; border-radius:14px;
               width:min(900px,96vw); max-height:92vh; overflow:auto; padding:18px 20px;
               box-shadow:0 16px 48px rgba(0,0,0,.55); }
  #layer_box h3 { margin:0; font-size:18px; color:#fff; }
  .layer-header { display:flex; align-items:center; justify-content:space-between; gap:12px; margin-bottom:12px; }
  .layer-warn { font-size:12px; color:#ffb0a0; background:#2a1818; border:1px solid #6a3030;
                border-radius:8px; padding:8px 12px; margin-bottom:10px; display:none; }
  .layer-warn.show { display:block; }
  .layer-bounds { display:flex; flex-wrap:wrap; gap:6px; margin-bottom:10px; align-items:center; }
  .layer-bounds .lbl { font-size:12px; color:#aaa; margin-right:4px; }
  .layer-bounds button { font-size:11px; padding:4px 8px; }
  .layer-add-row { display:flex; flex-wrap:wrap; gap:6px; margin-bottom:16px; }
  .layer-add-row button { font-size:11px; background:#2a3040; border-color:#4a5070; }
  .layer-panels { display:flex; gap:0; align-items:flex-start; }
  .layer-stack { position:relative; flex:1; padding:12px 8px 28px; min-height:120px; }
  .layer-sheet {
    position:relative; margin:0 auto 0; width:100%; max-width:640px;
    background: linear-gradient(145deg, #faf6ec 0%, #ebe4d4 55%, #ddd4c4 100%);
    color:#2a2820; border:1px solid #c8c0b0; border-radius:3px 3px 1px 1px;
    box-shadow: 1px 2px 0 #fff inset, 2px 3px 8px rgba(0,0,0,.28);
    padding:10px 14px 12px; transition: transform .18s ease, box-shadow .18s ease, margin .18s ease;
    cursor:grab;
  }
  .layer-sheet + .layer-sheet { margin-top:-18px; }
  .layer-sheet.selected {
    transform: translateY(-6px) scale(1.015);
    box-shadow: 1px 2px 0 #fff inset, 0 10px 28px rgba(0,0,0,.42);
    outline: 2px solid #7a7af0;
    z-index: 50 !important;
  }
  .layer-sheet.hidden-layer { opacity:0.45; filter:grayscale(60%); }
  .layer-sheet.dragging { opacity:0.35; cursor:grabbing; }
  .layer-sheet.drag-over-top { border-top:3px solid #7a7af0; margin-top:-21px; }
  .layer-sheet.drag-over-bottom { border-bottom:3px solid #7a7af0; }
  .layer-sheet.type-underground { background: linear-gradient(145deg,#e8dcc8,#c8b898); }
  .layer-sheet.type-ground { background: linear-gradient(145deg,#eef4e8,#d4e0c8); }
  .layer-sheet.type-vent { background: linear-gradient(145deg,#e8eef4,#c8d4e0); }
  .layer-sheet.type-roof { background: linear-gradient(145deg,#ece8f0,#ccc8d8); }
  .layer-sheet-head { display:flex; align-items:center; gap:8px; flex-wrap:wrap; margin-bottom:4px; }
  .layer-idx { font-size:11px; color:#666; font-weight:700; min-width:28px; }
  .layer-sheet-head strong { font-size:14px; flex:1; }
  .layer-type-badge { font-size:10px; text-transform:uppercase; letter-spacing:.06em;
                      background:rgba(0,0,0,.08); padding:2px 7px; border-radius:4px; color:#444; }
  .layer-meta { font-size:11px; color:#666; margin-bottom:8px; }
  .layer-actions { display:flex; flex-wrap:wrap; gap:5px; align-items:center; }
  .layer-actions button { font-size:11px; padding:3px 8px; background:#f0ece4; border-color:#bbb;
                          color:#333; }
  .layer-actions button:hover { background:#fff; }
  .layer-actions button.danger { border-color:#a05050; color:#802020; }
  .layer-actions select { font-size:11px; padding:3px 6px; background:#f8f4ec; border:1px solid #bbb;
                          border-radius:4px; color:#333; }
  #trash_panel { width:210px; min-width:180px; flex-shrink:0; background:#16161f;
                 border-left:1px solid #2e2e40; border-radius:0 8px 8px 0; padding:12px 10px;
                 min-height:200px; }
  #trash_panel h4 { margin:0 0 10px; font-size:13px; color:#aaa; text-align:center; }
  #trash_drop_area { min-height:80px; background:#1e1e2a; border:2px dashed #3a3a50;
                     border-radius:8px; padding:8px; display:flex; flex-direction:column; gap:6px; }
  #trash_drop_area.drag-active { border-color:#7a7af0; background:#22223a; }
  #trash_drop_hint { font-size:11px; color:#555; text-align:center; padding:14px 6px; }
  .trash-tile { background:#23232f; border:1px solid #3a3a4c; border-radius:6px;
                padding:7px 9px; cursor:grab; transition:background .15s; }
  .trash-tile:hover { background:#2c2c3c; }
  .trash-tile strong { font-size:12px; color:#ccc; display:block; }
  .trash-tile span { font-size:10px; color:#777; }
  .trash-tile .restore-btn { font-size:10px; padding:2px 6px; margin-top:5px; display:block;
                              background:#2a3040; border:1px solid #4a5070; color:#9ab; cursor:pointer;
                              border-radius:4px; width:100%; text-align:center; }
  .trash-tile .restore-btn:hover { background:#384060; }
  .layer-footer { display:flex; flex-wrap:wrap; gap:10px; align-items:center; margin-top:14px;
                  padding-top:12px; border-top:1px solid #2e2e3c; }
  .layer-hash { font-size:11px; color:#8d8da8; font-family:monospace; }
  /* per-map settings panel */
  .ms-group { margin-bottom:12px; }
  .ms-group-label { font-size:11px; color:#9090b0; text-transform:uppercase; letter-spacing:.05em;
                    margin-bottom:6px; border-bottom:1px solid #2a2a38; padding-bottom:3px; }
  .ms-row { display:flex; flex-wrap:wrap; align-items:center; gap:5px; margin-bottom:5px; }
  .ms-key { font-size:11px; color:#ccd; font-family:monospace; min-width:0; }
  .ms-inp { width:90px; font-size:12px; background:#141420; border:1px solid #3a3a50;
            color:#eee; padding:3px 6px; border-radius:4px; }
  .ms-inp.ms-active { border-color:#7a7af0; background:#1a1a30; }
  .ms-x { font-size:11px; background:none; border:1px solid #503030; color:#c07070; padding:2px 6px; }
  .ms-x:hover { border-color:#a05050; color:#f09090; }
</style>
</head>
<body>
<div id='top'>
  <div id='bar1'>
    <h1>E2E Map Editor</h1>
    <button class='btn-rainbow' onclick='openLayerStack()' title='Configure virtual map layers (100+ floors, expanded bounds)'>🗂 Map Layers</button>
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
    <div class='card'>
      <h2>Custom building block presets</h2>
      <div class='row'>
        <button class='custom' onclick='openComboEditor()'>⊞ Create new combined building block</button>
        <span class='hint'>Build reusable structures from saved custom/animated blocks on the current editor layer.</span>
      </div>
    </div>
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
        <button id='ts_open_ct' onclick='openConnectedTextureEditor()' disabled>▦ Create connecting texture from selection</button>
        <button id='tool_tileerase' onclick='setTool(""tileerase"")'>✕ Erase modded tiles</button>
        <button class='danger' onclick='confirm(""Remove every painted mod tile (static + animated) from this map?"") && post(""/api/tiles/clear"")'>Clear ALL modded tiles</button>
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
        When off, vanilla players see the mod-free extras.</div>
    </div>
    <div class='card' id='ca_card'>
      <h2>Custom assets <span id='ca_count' style='font-weight:normal;font-size:12px;color:#7a7ab0'></span></h2>
      <div class='hint'>Drop Unity 5.5-compatible asset bundles into
        <code>BepInEx/plugins/E2EMapEditor/bundles/</code> then pick a bundle below.
        Place prefabs at the editor cursor tile. Placements are saved in the Level.e2e sidecar.</div>
      <div class='row' style='margin-top:8px;align-items:center;gap:8px;flex-wrap:wrap'>
        <select id='ca_bundle' onchange='loadCaAssets()' style='background:#0e0e12;border:1px solid #333;color:#eee;padding:4px 6px;border-radius:4px;min-width:160px'>
          <option value=''>— pick a bundle —</option>
        </select>
        <button onclick='refreshCaBundles()' title='Refresh bundle list'>⟳</button>
      </div>
      <div id='ca_asset_list' style='display:none;margin-top:8px'>
        <select id='ca_asset' style='background:#0e0e12;border:1px solid #333;color:#eee;padding:4px 6px;border-radius:4px;min-width:200px'>
          <option value=''>— pick an asset —</option>
        </select>
        <div class='row' style='margin-top:6px;gap:6px'>
          <button id='ca_place_btn' onclick='caPlaceCursor()' disabled title='Place selected prefab at the in-game cursor tile'>📦 Place at cursor</button>
          <button id='ca_erase_btn' onclick='caEraseCursor()'>✕ Erase at cursor</button>
          <button class='danger' onclick='caClearAll()'>Clear ALL custom assets</button>
        </div>
      </div>
      <div id='ca_placed' style='margin-top:8px;max-height:140px;overflow-y:auto;display:none'></div>
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
      <h2>Virtual map layers</h2>
      <div class='row'>
        <button class='btn-rainbow' onclick='openLayerStack()'>🗂 Configure map layers</button>
        <span class='hint'>Paper-stack editor: add/reorder virtual floors, set types, expand map bounds beyond 120×120.</span>
      </div>
    </div>
    <div class='card' id='ms_card'>
      <h2>Per-map settings
        <span id='ms_badge' style='display:none;font-size:11px;color:#7af07a;margin-left:8px;font-weight:normal'></span>
      </h2>
      <div class='hint' style='margin-bottom:10px'>
        Settings saved here are stored in the map's Level.e2e sidecar and applied whenever
        the map is played (including play-test). Leave a field blank to use game defaults.
      </div>

      <!-- Time -->
      <div class='ms-group'>
        <div class='ms-group-label'>⏱ Time</div>
        <div class='ms-row'>
          <span class='ms-key'>timeScale</span>
          <input class='ms-inp' id='ms_timeScale' type='number' step='0.1' min='0.1' placeholder='1.0'
            title='Day-cycle speed multiplier. 2.0 = twice as fast.'>
          <button onclick='msSet(""timeScale"",el(""ms_timeScale"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""timeScale"",""ms_timeScale"")'>✕</button>
        </div>
        <div class='ms-row'>
          <span class='ms-key'>startHour</span>
          <input class='ms-inp' id='ms_startHour' type='number' min='0' max='23' placeholder='8'
            title='In-game hour when the map starts (0–23).'>
          <button onclick='msSet(""startHour"",el(""ms_startHour"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""startHour"",""ms_startHour"")'>✕</button>
          <span class='ms-key' style='margin-left:8px'>startMinute</span>
          <input class='ms-inp' id='ms_startMinute' type='number' min='0' max='59' placeholder='0'
            title='In-game minute when the map starts (0–59).'>
          <button onclick='msSet(""startMinute"",el(""ms_startMinute"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""startMinute"",""ms_startMinute"")'>✕</button>
        </div>
        <div class='ms-row'>
          <span class='ms-key'>timedPrison</span>
          <input class='ms-inp' id='ms_timedPrison' type='text' placeholder='48h'
            title='Escape deadline. Formats: 48h, 2h30m, 90m.'>
          <button onclick='msSet(""timedPrison"",el(""ms_timedPrison"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""timedPrison"",""ms_timedPrison"")'>✕</button>
        </div>
      </div>

      <!-- Audio -->
      <div class='ms-group'>
        <div class='ms-group-label'>🔊 Audio</div>
        <div class='ms-row'>
          <span class='ms-key'>ambience</span>
          <input class='ms-inp' id='ms_ambience' type='text' placeholder='Play_Prison_05_Ambience_General'
            title='Wwise ambience event name (see AUTOGEN_T17Wwise_Enums.Events for the full list).'>
          <button onclick='msSet(""ambience"",el(""ms_ambience"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""ambience"",""ms_ambience"")'>✕</button>
        </div>
        <div class='ms-row'>
          <span class='ms-key'>spotlightHours</span>
          <input class='ms-inp' id='ms_spotlightHours' type='text' placeholder='18:30-06:30'
            title='Spotlight on/off window (HH:MM-HH:MM).'>
          <button onclick='msSet(""spotlightHours"",el(""ms_spotlightHours"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""spotlightHours"",""ms_spotlightHours"")'>✕</button>
        </div>
      </div>

      <!-- Player stats -->
      <div class='ms-group'>
        <div class='ms-group-label'>🧍 Player stats</div>
        <div class='ms-row'>
          <span class='ms-key'>playerMoney</span>
          <input class='ms-inp' id='ms_playerMoney' type='number' min='0' placeholder='100'
            title='Starting money (in-game currency).'>
          <button onclick='msSet(""playerMoney"",el(""ms_playerMoney"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""playerMoney"",""ms_playerMoney"")'>✕</button>
        </div>
        <div class='ms-row'>
          <span class='ms-key'>healthRegen</span>
          <input class='ms-inp' id='ms_healthRegen' type='number' step='0.001' placeholder='default'
            title='Health restore rate per second (0 = no regen, hardcore).'>
          <button onclick='msSet(""healthRegen"",el(""ms_healthRegen"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""healthRegen"",""ms_healthRegen"")'>✕</button>
          <span class='ms-key' style='margin-left:8px'>energyRegen</span>
          <input class='ms-inp' id='ms_energyRegen' type='number' step='0.001' placeholder='default'
            title='Energy restore rate per second.'>
          <button onclick='msSet(""energyRegen"",el(""ms_energyRegen"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""energyRegen"",""ms_energyRegen"")'>✕</button>
        </div>
        <div class='ms-row'>
          <span class='ms-key'>heatDecay</span>
          <input class='ms-inp' id='ms_heatDecay' type='number' step='0.001' placeholder='default'
            title='Heat decay rate (0 = guards never forget).'>
          <button onclick='msSet(""heatDecay"",el(""ms_heatDecay"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""heatDecay"",""ms_heatDecay"")'>✕</button>
        </div>
      </div>

      <!-- Security hardware -->
      <div class='ms-group'>
        <div class='ms-group-label'>🔒 Security hardware</div>
        <div class='ms-row'>
          <span class='ms-key'>generatorDowntime</span>
          <input class='ms-inp' id='ms_generatorDowntime' type='number' min='0' placeholder='30'
            title='Seconds a generator stays off after being cut (default 30).'>
          <button onclick='msSet(""generatorDowntime"",el(""ms_generatorDowntime"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""generatorDowntime"",""ms_generatorDowntime"")'>✕</button>
        </div>
        <div class='ms-row'>
          <span class='ms-key'>cctvSpeed</span>
          <input class='ms-inp' id='ms_cctvSpeed' type='number' step='0.1' min='0' placeholder='1.0'
            title='CCTV camera sweep speed multiplier.'>
          <button onclick='msSet(""cctvSpeed"",el(""ms_cctvSpeed"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""cctvSpeed"",""ms_cctvSpeed"")'>✕</button>
        </div>
        <div class='ms-row'>
          <span class='ms-key'>sniperDamage</span>
          <input class='ms-inp' id='ms_sniperDamage' type='number' min='0' placeholder='40'
            title='Guard-tower sniper damage per shot (default 40).'>
          <button onclick='msSet(""sniperDamage"",el(""ms_sniperDamage"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""sniperDamage"",""ms_sniperDamage"")'>✕</button>
          <span class='ms-key' style='margin-left:8px'>sniperHeatThreshold</span>
          <input class='ms-inp' id='ms_sniperHeatThreshold' type='number' min='0' max='100' placeholder='70'
            title='Heat at which snipers open fire (0–100, default 70).'>
          <button onclick='msSet(""sniperHeatThreshold"",el(""ms_sniperHeatThreshold"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""sniperHeatThreshold"",""ms_sniperHeatThreshold"")'>✕</button>
        </div>
        <div class='ms-row'>
          <span class='ms-key'>startingAlertness</span>
          <input class='ms-inp' id='ms_startingAlertness' type='number' min='0' max='10' placeholder='0'
            title='Initial alertness star rating at map start (0–10).'>
          <button onclick='msSet(""startingAlertness"",el(""ms_startingAlertness"").value)'>Set</button>
          <button class='ms-x' onclick='msUnset(""startingAlertness"",""ms_startingAlertness"")'>✕</button>
        </div>
      </div>

      <div class='row' style='margin-top:12px'>
        <button onclick='msLoad()'>↻ Reload from map</button>
        <button class='danger' onclick='msClearAll()'>✕ Clear all settings</button>
      </div>
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
      <div class='row' id='virtualLayerBtns'></div>
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
        <div class='frame-add-btn' onclick='beginPickNextFrame()' title='Minimize editor and pick next frame from tilesets'>＋</div>
      </div>
      <div class='anim-btns' style='margin-top:6px'>
        <button class='anim-detect-btn' onclick='autoDetectFrames()' id='anim_detect_btn' title='Auto-scan this atlas for repeating same-size tiles along horizontal or vertical strips'>🔍 Detect frames automatically</button>
        <button onclick='addCurrentSelectionAsFrame()' id='anim_add_sel_btn' disabled title='Add current tileset selection as next frame'>⊕ Add current selection</button>
        <button onclick='clearAnimFrames()' style='background:#2a1a1a;border-color:#6a2a2a'>✕ Clear all frames</button>
      </div>
      <div class='detect-grid'>
        <label>Mode
          <select id='anim_detect_mode'>
            <option value='auto'>Auto best strip</option>
            <option value='area'>Selected area grid</option>
            <option value='right'>Horizontal right</option>
            <option value='left'>Horizontal left</option>
            <option value='down'>Vertical down</option>
            <option value='up'>Vertical up</option>
            <option value='bothh'>Horizontal both ways</option>
            <option value='bothv'>Vertical both ways</option>
          </select>
        </label>
        <label>Frame W tiles <input id='anim_detect_w' type='number' min='1' max='16' value='1'></label>
        <label>Frame H tiles <input id='anim_detect_h' type='number' min='1' max='16' value='1'></label>
        <label>Max frames <input id='anim_detect_max' type='number' min='2' max='256' value='128'></label>
        <label>Allowed empty gaps <input id='anim_detect_gaps' type='number' min='0' max='8' value='0'></label>
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
            <label class='anim-chk'><input type='checkbox' id='anim_loop' checked onchange='restartPreviewIfRunning()'>Loop</label>
            <label class='anim-chk'><input type='checkbox' id='anim_pingpong' onchange='restartPreviewIfRunning()'>Ping-pong</label>
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
<div id='anim_pick_bar'>
  <span id='anim_pick_status'>Pick next frame — select a region in Tilesets, then click Add frame</span>
  <button onclick='confirmPickFrame()' style='background:#264026;border-color:#3a6a3a'>✓ Add frame</button>
  <button onclick='cancelPickFrame()'>Cancel</button>
</div>
<div id='combo_overlay' onclick='if(event.target===this)closeComboEditor()'>
  <div id='combo_box'>
    <h3 style='margin:0 0 10px'>⊞ Combined building block preset</h3>
    <div class='combo-toolbar'>
      <span class='num'>Name <input id='combo_name' type='text' value='Combined block' style='width:180px'></span>
      <span class='num'>Width <input id='combo_w' type='number' min='1' max='64' value='16' onchange='resizeComboGrid()'></span>
      <span class='num'>Height <input id='combo_h' type='number' min='1' max='64' value='16' onchange='resizeComboGrid()'></span>
      <button onclick='saveComboPreset()' style='background:#264026;border-color:#3a6a3a'>Save preset</button>
      <button onclick='closeComboEditor()'>Close</button>
    </div>
    <div class='combo-panel'>
      <h4>Last used tiles</h4>
      <div class='guide'>Right-click an empty grid cell to temporarily minimize this window and pick a saved block. Left-click empty cells reuses the last picked tile. Use Eraser to delete.</div>
      <div id='combo_recent' class='combo-recent'></div>
      <button onclick='comboBrush=null;comboEraser=true;renderComboRecent()'>Eraser brush</button>
    </div>
    <div class='combo-layout'>
      <div class='combo-panel'>
        <div class='combo-toolbar'>
          <button onclick='comboImportStart()'>Select ingame area</button>
          <button onclick='comboClearGrid()' class='danger'>Clear grid</button>
          <span class='hint' id='combo_status'>16×16 grid</span>
        </div>
        <div class='combo-grid-wrap'><div id='combo_grid'></div></div>
      </div>
      <div class='combo-panel'>
        <h4>Import from game</h4>
        <div class='guide'>
          Enter a region from the editor map to import already painted modded or animated tiles from the current editor layer.
          Vanilla game blocks are not exposed by the current API yet.
        </div>
        <div class='combo-toolbar'>
          <span class='num'>x <input id='combo_ix' type='number' value='0'></span>
          <span class='num'>y <input id='combo_iy' type='number' value='0'></span>
          <span class='num'>w <input id='combo_iw' type='number' value='8'></span>
          <span class='num'>h <input id='combo_ih' type='number' value='8'></span>
        </div>
        <button onclick='comboImportRegion()'>Import region</button>
      </div>
    </div>
  </div>
</div>
<div id='combo_pick_bar'>
  <span id='combo_pick_status'>Pick a saved custom/animated/combined block from the Blocks tab.</span>
  <button onclick='cancelComboPick()'>Cancel</button>
</div>
<div id='ct_overlay' onclick='if(event.target===this)closeConnectedTextureEditor()'>
  <div id='ct_box'>
    <h3 style='margin:0 0 10px'>▦ Connected texture mapper</h3>
    <div class='combo-panel'>
      <div class='ct-options'>
        <label>Mode
          <select id='ct_mode' onchange='ctSwitchMode(this.value)'>
            <option value='floor'>Floor mode (16 side masks)</option>
            <option value='wall'>Wall mode (edge/wall masks)</option>
            <option value='other'>Other / manual mode</option>
          </select>
        </label>
        <input id='ct_name' type='text' value='Connected texture' style='width:220px'>
        <button onclick='ctAutoDetect()'>Detect and configure automatically</button>
        <button onclick='ctSelectMore()'>Select another area additionally to currently selected area</button>
        <button onclick='ctSave()' style='background:#264026;border-color:#3a6a3a'>Save connected texture</button>
        <button onclick='closeConnectedTextureEditor()'>Close</button>
      </div>
      <div class='guide' id='ct_mode_help'></div>
    </div>
    <div class='combo-panel'>
      <h4>Current selection / available tiles</h4>
      <div class='guide'>Drag source tiles onto the known-layout slots or directly onto the preview. Used tiles are greyed out; they can still be reused for variations.</div>
      <div id='ct_source' class='ct-source'></div>
    </div>
    <div class='ct-main'>
      <div class='combo-panel'>
        <h4>Known output layout / configuration</h4>
        <div class='guide'>Drop onto the matching known tile. If a slot is already configured, choose replace or add variation. Right-click configured slots to remove variants.</div>
        <div id='ct_slots' class='ct-slot-grid'></div>
      </div>
      <div class='combo-panel'>
        <h4>Live output preview</h4>
        <div class='guide'>Shows what your current config produces. Missing masks use a ghost of the known example.</div>
        <div class='ct-preview-wrap'><div id='ct_preview' class='ct-preview-grid'></div></div>
      </div>
    </div>
    <div class='combo-panel ct-full'>
      <div class='ct-options'>
        <button onclick='ctClearPattern()' class='danger'>Clear below area</button>
        <button onclick='ctDefaultShape();renderCtPattern();renderCtPreview()'>Reset example pattern</button>
        <span id='ct_status' class='guide'></span>
      </div>
      <h4>Editable stress-test pattern</h4>
      <div class='guide'>Click to draw/delete. This pattern tries to cover ends, corners, blobs, lines, and holes.</div>
      <div id='ct_pattern'></div>
    </div>
  </div>
</div>
<div id='layer_overlay' onclick='if(event.target===this)closeLayerStack()'>
  <div id='layer_box'>
    <div class='layer-header'>
      <h3>🗂 Virtual Map Layers</h3>
      <button onclick='closeLayerStack()'>Close</button>
    </div>
    <div class='hint' style='margin-bottom:10px'>Stack of logical floors stored in Level.e2e. Drag to reorder. Right-click a floor in-game to hide/show it. Drag floors to 🗑 Deleted to remove them.</div>
    <div id='layer_warning' class='layer-warn'></div>
    <div class='layer-bounds'>
      <span class='lbl'>Bounds:</span>
      <span id='layer_bounds_lbl' class='hint'></span>
      <button onclick='geoBoundsDelta(""width"",-16)'>W −16</button>
      <button onclick='geoBoundsDelta(""width"",16)'>W +16</button>
      <button onclick='geoBoundsDelta(""height"",-16)'>H −16</button>
      <button onclick='geoBoundsDelta(""height"",16)'>H +16</button>
      <button onclick='geoBoundsDelta(""originX"",-16)'>X −16</button>
      <button onclick='geoBoundsDelta(""originX"",16)'>X +16</button>
      <button onclick='geoBoundsDelta(""originY"",-16)'>Y −16</button>
      <button onclick='geoBoundsDelta(""originY"",16)'>Y +16</button>
    </div>
    <div class='layer-add-row'>
      <span class='hint' style='margin-right:6px'>Add layer:</span>
      <button onclick='geoAdd(""Underground"")'>+ Underground</button>
      <button onclick='geoAdd(""Ground"")'>+ Ground</button>
      <button onclick='geoAdd(""Vent"")'>+ Vent</button>
      <button onclick='geoAdd(""Roof"")'>+ Roof</button>
    </div>
    <div class='layer-panels'>
      <div id='layer_stack' class='layer-stack'
           ondragover='event.preventDefault()' ondrop='onDropTrashRestore(event)'></div>
      <div id='trash_panel'>
        <h4>🗑 Deleted Floors</h4>
        <div id='trash_drop_area'
             ondragover='onTrashDragOver(event)' ondragleave='onTrashDragLeave(event)'
             ondrop='onDropToTrash(event)'>
          <div id='trash_drop_hint'>Drag floors here to delete</div>
        </div>
      </div>
    </div>
    <div class='layer-footer'>
      <button class='danger' onclick='geoReset()'>Reset vanilla 6-layer layout</button>
      <span id='layer_hash' class='layer-hash'></span>
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
let prefs = { customFilters: [], order: [], customBlocks: [], customCategoryName: 'Custom', animatedDefs: [], compositeDefs: [], connectedTextures: [] };
let orderRank = {};                        // block id -> user rank
let dragId = null;
let curFloor = -1;
let curVirtualLayer = -1;
let floorsKnown = '';
let mapTileSize = 8; // px per tile in the map texture (game generates 8)
let lastState = null;

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
  if (page === 'tools') { refreshCaBundles(); refreshCaPlaced(); }
  if (page === 'settings') msLoad();
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
  if (customActive(c)) {
    el('q').value = '';
    activeFilters.clear();
    renderFilters(); render();
    msg('filter cleared');
    return;
  }
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
  if (comboPickMode) {
    comboFinishPick({ kind:'vanilla', id });
    return;
  }
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

/* ---- per-map settings ---- */
const MS_KEYS = [
  'timeScale','startHour','startMinute','timedPrison',
  'ambience','spotlightHours',
  'playerMoney','healthRegen','energyRegen','heatDecay',
  'generatorDowntime','cctvSpeed','sniperDamage','sniperHeatThreshold','startingAlertness'
];

async function msLoad() {
  try {
    const d = await fetch('/api/map-settings').then(r => r.json());
    const s = d.settings || {};
    for (const k of MS_KEYS) {
      const inp = el('ms_' + k);
      if (!inp) continue;
      if (s[k] !== undefined) {
        inp.value = s[k];
        inp.classList.add('ms-active');
      } else {
        inp.value = '';
        inp.classList.remove('ms-active');
      }
    }
    const n = Object.keys(s).length;
    const badge = el('ms_badge');
    if (badge) {
      badge.style.display = n > 0 ? '' : 'none';
      badge.textContent = n + (n === 1 ? ' setting' : ' settings') + ' active';
    }
  } catch(e) { console.warn('msLoad', e); }
}

async function msSet(key, value) {
  value = (value || '').trim();
  if (!value) { await msUnset(key, 'ms_' + key); return; }
  await fetch(`/api/map-settings/set?key=${encodeURIComponent(key)}&value=${encodeURIComponent(value)}`, {method:'POST'});
  const inp = el('ms_' + key);
  if (inp) inp.classList.add('ms-active');
  await msLoad();
}

async function msUnset(key, inputId) {
  await fetch(`/api/map-settings/unset?key=${encodeURIComponent(key)}`, {method:'POST'});
  if (inputId) {
    const inp = el(inputId);
    if (inp) { inp.value = ''; inp.classList.remove('ms-active'); }
  }
  await msLoad();
}

async function msClearAll() {
  if (!confirm('Clear all per-map settings for this map?')) return;
  await fetch('/api/map-settings/clear', {method:'POST'});
  await msLoad();
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
  el('ts_open_anim').disabled = true;
  el('ts_open_ct').disabled = true;
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
  el('ts_open_anim').disabled = false;
  el('ts_open_ct').disabled = false;
  // update anim editor
  el('anim_src_sel').textContent = `${tsSel.cw}×${tsSel.ch} @ (${tsSel.cx},${tsSel.cy})`;
  if (el('anim_add_sel_btn')) el('anim_add_sel_btn').disabled = false;
  if (animPickMode && el('anim_pick_status')) {
    el('anim_pick_status').textContent =
      `Selection ready (${tsSel.cw}×${tsSel.ch} @ ${tsSel.cx},${tsSel.cy}) — release mouse to add it`;
  }
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
  document.addEventListener('mouseup', () => {
    const wasDragging = !!tsDrag;
    tsDrag = null;
    if (wasDragging && animPickMode && tsSel) {
      confirmPickFrame();
    }
  });
})();

  /* ---- custom assets tab (Tools page card) ---- */
  async function refreshCaBundles() {
    try {
      const bundles = await (await fetch('/api/custom-assets/bundles')).json();
      const sel = el('ca_bundle');
      const prev = sel.value;
      sel.innerHTML = '<option value=\"\">— pick a bundle —</option>' +
        bundles.map(b => `<option value=\"${esc(b)}\"${b === prev ? ' selected' : ''}>${esc(b)}</option>`).join('');
      if (prev && bundles.includes(prev)) {
        loadCaAssets();
      } else {
        el('ca_asset_list').style.display = 'none';
      }
    } catch(e) {
      msg('Could not load bundle list: ' + e);
    }
  }

  async function loadCaAssets() {
    const bundle = el('ca_bundle').value;
    if (!bundle) { el('ca_asset_list').style.display = 'none'; return; }
    try {
      const assets = await (await fetch('/api/custom-assets/list?bundle=' + encodeURIComponent(bundle))).json();
      const sel = el('ca_asset');
      sel.innerHTML = '<option value=\"\">— pick an asset —</option>' +
        assets.map(a => `<option value=\"${esc(a)}\">${esc(a)}</option>`).join('');
      el('ca_asset_list').style.display = '';
      el('ca_place_btn').disabled = assets.length === 0;
    } catch(e) {
      msg('Could not list assets: ' + e);
    }
  }

  async function caPlaceCursor() {
    const bundle = el('ca_bundle').value;
    const asset  = el('ca_asset').value;
    if (!bundle || !asset) { msg('Select a bundle and asset first'); return; }
    const r = await post(`/api/custom-assets/place-cursor?bundle=${encodeURIComponent(bundle)}&asset=${encodeURIComponent(asset)}`);
    if (r.ok) {
      msg(`Placed ${asset} @ (${r.x},${r.y},${r.layer}) — ${r.count} total`);
      refreshCaPlaced();
    }
  }

  async function caEraseCursor() {
    const r = await post('/api/custom-assets/erase-cursor');
    if (r.ok) {
      msg(r.removed ? `Erased ${r.removed} custom asset(s) — ${r.count} remain` : 'No custom asset at cursor');
      refreshCaPlaced();
    }
  }

  async function caClearAll() {
    if (!confirm('Remove ALL custom asset placements from this map?')) return;
    await post('/api/custom-assets/clear');
    refreshCaPlaced();
    msg('All custom asset placements cleared');
  }

  async function refreshCaPlaced() {
    try {
      const list = await (await fetch('/api/custom-assets')).json();
      const div = el('ca_placed');
      if (list.length === 0) {
        div.style.display = 'none';
        div.innerHTML = '';
      } else {
        div.style.display = '';
        div.innerHTML = '<table style=\"width:100%;border-collapse:collapse;font-size:11px\">' +
          '<tr><th style=\"text-align:left;color:#7a7ab0\">Bundle</th>' +
          '<th style=\"text-align:left;color:#7a7ab0\">Asset</th>' +
          '<th style=\"text-align:left;color:#7a7ab0\">Tile</th></tr>' +
          list.map(p =>
            `<tr><td>${esc(p.bundle)}</td><td>${esc(p.asset)}</td>` +
            `<td>(${p.x},${p.y},L${p.layer})</td></tr>`
          ).join('') + '</table>';
      }
      const cnt = el('ca_count');
      if (cnt) cnt.textContent = list.length ? `(${list.length} placed)` : '';
    } catch(e) {}
  }

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
    // Virtual layer buttons (only shown when sidecar layers exist)
    try {
      const geom = await (await fetch('/api/layers')).json();
      if (geom && geom.layers && geom.layers.length > 0) {
        const vbEl = el('virtualLayerBtns');
        if (vbEl) {
          vbEl.innerHTML = geom.layers.map((lyr, vi) =>
            `<button id='vl_${vi}' onclick='pickVirtualLayer(${vi},${lyr.backingLayer})'>` +
            `#${vi} ${esc(lyr.name)}</button>`).join('');
        }
      }
    } catch (_) { /* no virtual layers */ }
  } catch (e) { /* not reachable */ }
  refreshPlayer();
}

function pickFloor(i) {
  curFloor = i;
  curVirtualLayer = -1;
  document.querySelectorAll(""[id^='fl_']"").forEach(b =>
    b.classList.toggle('active', b.id === 'fl_' + i));
  document.querySelectorAll(""[id^='vl_']"").forEach(b => b.classList.remove('active'));
  el('mapimg').src = '/api/map/' + i + '.png?t=' + Date.now();
}

function pickVirtualLayer(vi, backingFloor) {
  curVirtualLayer = vi;
  curFloor = backingFloor;
  document.querySelectorAll(""[id^='vl_']"").forEach(b =>
    b.classList.toggle('active', b.id === 'vl_' + vi));
  document.querySelectorAll(""[id^='fl_']"").forEach(b => b.classList.remove('active'));
  el('mapimg').src = '/api/map/v/' + vi + '.png?t=' + Date.now();
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
      (p.tile ? `<span>tile <b>(${p.tile.x},${p.tile.y})</b> floor <b>${p.tile.floor}</b>` +
        (p.tile.virtualLayer >= 0 ? ` vLayer <b>${p.tile.virtualLayer}</b>` : '') +
        `</span>` : '');
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
  mark.style.top = ((1 + (tile.y + 0.5) * mapTileSize) * sy) + 'px';
  mark.style.display = 'block';
}

async function mapClick(ev) {
  const img = el('mapimg');
  if (!img.naturalWidth || curFloor < 0) return;
  const r = img.getBoundingClientRect();
  const px = (ev.clientX - r.left) * (img.naturalWidth / r.width);
  const py = (ev.clientY - r.top) * (img.naturalHeight / r.height);
  const geom = lastState && lastState.mapGeometry ? lastState.mapGeometry : { width:120, height:120, originX:0, originY:0 };
  const maxX = geom.originX + geom.width - 1;
  const maxY = geom.originY + geom.height - 1;
  const tx = Math.max(geom.originX, Math.min(maxX, Math.floor((px - 1) / mapTileSize)));
  const ty = Math.max(geom.originY, Math.min(maxY, Math.floor((img.naturalHeight - py - 1) / mapTileSize)));
  const url = curVirtualLayer >= 0
    ? `/api/teleport?x=${tx}&y=${ty}&virtualLayer=${curVirtualLayer}`
    : `/api/teleport?x=${tx}&y=${ty}&floor=${curFloor}`;
  await post(url);
  refreshPlayer();
}

/* ---- virtual map layer stack ---- */
const LAYER_TYPES = ['Underground','Ground','Vent','Roof'];

function layerEsc(s) {
  return (s || '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/\x22/g,'&quot;');
}

function openLayerStack() {
  if (!lastState || !lastState.inEditor) {
    msg('open the level editor first');
    return;
  }
  el('layer_overlay').classList.add('show');
  renderLayerStack();
}

function closeLayerStack() { el('layer_overlay').classList.remove('show'); }

let _dragSrcIndex = null;   // index being dragged from layer stack
let _dragSrcTrash = null;   // trashIndex being dragged from trash bin

function renderLayerStack() {
  const geom = lastState && lastState.mapGeometry;
  if (!geom || !geom.layers) {
    el('layer_stack').innerHTML = '<div class=""hint"">no geometry data — reload editor</div>';
    return;
  }
  const warn = el('layer_warning');
  if (geom.warning) {
    warn.textContent = geom.warning;
    warn.classList.add('show');
  } else if (!geom.nativeCompatible) {
    warn.textContent = 'Custom geometry requires matching Level.e2e on every multiplayer client.';
    warn.classList.add('show');
  } else {
    warn.textContent = '';
    warn.classList.remove('show');
  }
  el('layer_bounds_lbl').textContent =
    `${geom.width}×${geom.height} origin (${geom.originX},${geom.originY})`;
  el('layer_hash').textContent = 'hash ' + (geom.hash || '?');
  const stack = el('layer_stack');
  stack.innerHTML = '';
  const selected = typeof geom.selected === 'number' ? geom.selected : 0;
  geom.layers.forEach((layer, i) => {
    const sheet = document.createElement('div');
    const typeKey = (layer.type || 'Ground').toLowerCase();
    const isHidden = !!layer.hidden;
    let cls = 'layer-sheet type-' + typeKey;
    if (i === selected) cls += ' selected';
    if (isHidden) cls += ' hidden-layer';
    sheet.className = cls;
    sheet.style.zIndex = (i + 1).toString();
    sheet.draggable = true;
    sheet.dataset.layerIndex = i;
    const opts = LAYER_TYPES.map(t =>
      `<option value=""${t}"" ${layer.type === t ? 'selected' : ''}>${t}</option>`).join('');
    const eyeIcon = isHidden ? '🙈' : '👁';
    const eyeTitle = isHidden ? 'Show in game' : 'Hide from game';
    sheet.innerHTML =
      `<div class=""layer-sheet-head"">
        <span class=""layer-idx"">#${i}</span>
        <strong>${layerEsc(layer.name)}</strong>
        <span class=""layer-type-badge"">${layerEsc(layer.type)}</span>
      </div>
      <div class=""layer-meta"">Native backing layer ${layer.backingLayer}</div>
      <div class=""layer-actions"">
        <button onclick=""geoSelect(${i})"">${i === selected ? '● Selected' : 'Select'}</button>
        <button onclick=""geoHide(${i},${!isHidden})"" title=""${eyeTitle}"">${eyeIcon}</button>
        <button onclick=""geoDuplicate(${i})"">Duplicate</button>
        <button class=""danger"" onclick=""geoRemove(${i})"">🗑 Delete</button>
        <select onchange=""geoType(${i},this.value)"">${opts}</select>
      </div>`;
    // drag-and-drop: reorder
    sheet.addEventListener('dragstart', e => {
      _dragSrcIndex = i;
      _dragSrcTrash = null;
      e.dataTransfer.effectAllowed = 'move';
      e.dataTransfer.setData('text/plain', 'layer:' + i);
      setTimeout(() => sheet.classList.add('dragging'), 0);
    });
    sheet.addEventListener('dragend', () => {
      sheet.classList.remove('dragging');
      document.querySelectorAll('.drag-over-top,.drag-over-bottom')
        .forEach(n => n.classList.remove('drag-over-top','drag-over-bottom'));
    });
    sheet.addEventListener('dragover', e => {
      e.preventDefault();
      e.dataTransfer.dropEffect = 'move';
      const rect = sheet.getBoundingClientRect();
      const mid = rect.top + rect.height / 2;
      document.querySelectorAll('.drag-over-top,.drag-over-bottom')
        .forEach(n => n.classList.remove('drag-over-top','drag-over-bottom'));
      if (e.clientY < mid) sheet.classList.add('drag-over-top');
      else sheet.classList.add('drag-over-bottom');
    });
    sheet.addEventListener('dragleave', () => {
      sheet.classList.remove('drag-over-top','drag-over-bottom');
    });
    sheet.addEventListener('drop', async e => {
      e.preventDefault();
      sheet.classList.remove('drag-over-top','drag-over-bottom');
      if (_dragSrcTrash !== null) {
        // restore from trash into position i
        await geoApply(await post('/api/geometry/restore?trashIndex=' + _dragSrcTrash));
        _dragSrcTrash = null;
        return;
      }
      if (_dragSrcIndex === null || _dragSrcIndex === i) return;
      const delta = i - _dragSrcIndex;
      await geoApply(await post('/api/geometry/move?index=' + _dragSrcIndex + '&delta=' + delta));
      _dragSrcIndex = null;
    });
    stack.appendChild(sheet);
  });
  renderTrashBin(geom.trash || []);
}

function renderTrashBin(trashItems) {
  const area = el('trash_drop_area');
  // keep the drop hint but remove old tiles
  const hint = el('trash_drop_hint');
  area.innerHTML = '';
  area.appendChild(hint);
  hint.style.display = trashItems.length ? 'none' : 'block';
  trashItems.forEach((layer, ti) => {
    const tile = document.createElement('div');
    tile.className = 'trash-tile';
    tile.draggable = true;
    tile.dataset.trashIndex = ti;
    tile.innerHTML =
      `<strong>${layerEsc(layer.name)}</strong>
       <span>${layerEsc(layer.type)}</span>
       <button class=""restore-btn"" onclick=""geoRestore(${layer.trashIndex})"">↩ Restore</button>`;
    tile.addEventListener('dragstart', e => {
      _dragSrcTrash = ti;
      _dragSrcIndex = null;
      e.dataTransfer.effectAllowed = 'move';
      e.dataTransfer.setData('text/plain', 'trash:' + ti);
    });
    area.appendChild(tile);
  });
}

// Drop onto main layer area from trash (restore)
async function onDropTrashRestore(e) {
  e.preventDefault();
  if (_dragSrcTrash === null) return;
  await geoApply(await post('/api/geometry/restore?trashIndex=' + _dragSrcTrash));
  _dragSrcTrash = null;
}

function onTrashDragOver(e) {
  e.preventDefault();
  e.dataTransfer.dropEffect = 'move';
  el('trash_drop_area').classList.add('drag-active');
}
function onTrashDragLeave() { el('trash_drop_area').classList.remove('drag-active'); }
async function onDropToTrash(e) {
  e.preventDefault();
  el('trash_drop_area').classList.remove('drag-active');
  if (_dragSrcIndex === null) return;
  await geoApply(await post('/api/geometry/remove?index=' + _dragSrcIndex));
  _dragSrcIndex = null;
}

async function geoApply(r) {
  if (r && r.mapGeometry && lastState) {
    lastState.mapGeometry = r.mapGeometry;
    renderLayerStack();
    msg('map layers updated');
  }
}

async function geoSelect(i) { await geoApply(await post('/api/geometry/select?index=' + i)); }
async function geoAdd(type) { await geoApply(await post('/api/geometry/add?type=' + type)); }
async function geoRemove(i) {
  if (!confirm('Remove virtual layer #' + i + '?')) return;
  await geoApply(await post('/api/geometry/remove?index=' + i));
}
async function geoMove(i, delta) { await geoApply(await post('/api/geometry/move?index=' + i + '&delta=' + delta)); }
async function geoDuplicate(i) { await geoApply(await post('/api/geometry/duplicate?index=' + i)); }
async function geoType(i, type) { await geoApply(await post('/api/geometry/type?index=' + i + '&type=' + type)); }
async function geoHide(i, hidden) { await geoApply(await post('/api/geometry/hide?index=' + i + '&hidden=' + hidden)); }
async function geoRestore(ti) { await geoApply(await post('/api/geometry/restore?trashIndex=' + ti)); }
async function geoBoundsDelta(field, delta) {
  await geoApply(await post('/api/geometry/bounds-delta?field=' + field + '&delta=' + delta));
}
async function geoReset() {
  if (!confirm('Reset to vanilla 6-layer 120×120 layout?')) return;
  await geoApply(await post('/api/geometry/reset'));
}

/* ---- status poll ---- */
async function poll() {
  try {
    const s = await (await fetch('/api/state')).json();
    lastState = s;
    el('status').textContent =
      (s.inEditor ? 'EDITOR' : 'menu/play') +
      (s.cursor ? ` — cursor (${s.cursor.x},${s.cursor.y})` : '') +
      ` — fences ${s.fences}, links ${s.triggers}, tiles ${s.modTiles}` +
      (s.animatedTiles ? `, animated ${s.animatedTiles}` : '') +
      (s.customAssets ? `, custom ${s.customAssets}` : '') +
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
    if (el('layer_overlay').classList.contains('show')) renderLayerStack();
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
  if (ev.key === 'Escape' && el('layer_overlay').classList.contains('show')) closeLayerStack();
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
  if (comboPickMode) {
    comboFinishPick({ kind:'static', i });
    return;
  }
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

async function drawCustomBlockThumb(i, cb) {
  const cv = el('cb_thumb_' + i);
  if (!cv || !cb) return;
  await drawFrameToCanvas(cv, {
    atlas: cb.atlas, atlasH: cb.atlasH,
    x: cb.x, y: cb.y, w: cb.w, h: cb.h
  }, Math.min(64 / Math.max(cb.w, 1), 64 / Math.max(cb.h, 1), 8));
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

/* ---- combined building block presets ---- */
let comboW = 16, comboH = 16;
let comboCells = {}; // x,y -> [parts]
let comboBrush = null;
let comboEraser = false;
let comboRecent = [];
let comboPickTarget = null;
let comboPickMode = false;
let comboZoom = 1;
let comboPanDrag = null;

function comboKey(x, y) { return `${x},${y}`; }
function comboAllParts() {
  const out = [];
  for (const k in comboCells) {
    (comboCells[k] || []).forEach(p => out.push({...p}));
  }
  return out;
}
function openComboEditor(existing) {
  comboW = 16; comboH = 16; comboCells = {}; comboBrush = null; comboEraser = false;
  comboPickMode = false; comboPickTarget = null;
  el('combo_name').value = 'Combined block';
  if (existing) {
    comboW = existing.w || 16; comboH = existing.h || 16;
    el('combo_name').value = existing.name || 'Combined block';
    (existing.parts || []).forEach(p => {
      const k = comboKey(p.dx || 0, p.dy || 0);
      if (!comboCells[k]) comboCells[k] = [];
      comboCells[k].push({...p, layer:-1});
    });
  }
  el('combo_w').value = comboW; el('combo_h').value = comboH;
  el('combo_overlay').classList.add('show');
  el('combo_pick_bar').style.display = 'none';
  bindComboPanZoom();
  renderComboRecent();
  renderComboGrid();
}
function closeComboEditor() { el('combo_overlay').classList.remove('show'); }
function resizeComboGrid() {
  comboW = Math.max(1, Math.min(64, +el('combo_w').value || comboW));
  comboH = Math.max(1, Math.min(64, +el('combo_h').value || comboH));
  el('combo_w').value = comboW; el('combo_h').value = comboH;
  renderComboGrid();
}
function bindComboPanZoom() {
  const wrap = document.querySelector('#combo_overlay .combo-grid-wrap');
  if (!wrap || wrap.dataset.bound) return;
  wrap.dataset.bound = '1';
  wrap.addEventListener('wheel', ev => {
    ev.preventDefault();
    comboZoom = Math.max(0.5, Math.min(3, comboZoom * (ev.deltaY < 0 ? 1.1 : 0.9)));
    renderComboGrid();
  }, { passive:false });
  wrap.addEventListener('mousedown', ev => {
    if (ev.button !== 1) return;
    ev.preventDefault();
    comboPanDrag = { x: ev.clientX, y: ev.clientY, left: wrap.scrollLeft, top: wrap.scrollTop };
  });
  document.addEventListener('mousemove', ev => {
    if (!comboPanDrag) return;
    wrap.scrollLeft = comboPanDrag.left - (ev.clientX - comboPanDrag.x);
    wrap.scrollTop = comboPanDrag.top - (ev.clientY - comboPanDrag.y);
  });
  document.addEventListener('mouseup', ev => {
    if (ev.button === 1) comboPanDrag = null;
  });
}
function comboBrushId(brush) { return brush ? brush.kind + ':' + (brush.kind === 'vanilla' ? brush.id : brush.i) : ''; }
function comboDefForBrush(brush) {
  if (!brush) return null;
  if (brush.kind === 'static') return (prefs.customBlocks || [])[brush.i];
  if (brush.kind === 'animated') return (prefs.animatedDefs || [])[brush.i];
  if (brush.kind === 'combo') return (prefs.compositeDefs || [])[brush.i];
  if (brush.kind === 'vanilla') return blocks.find(b => b.id === brush.id);
  return null;
}
function comboRememberBrush(brush) {
  if (!brush) return;
  comboBrush = brush; comboEraser = false;
  const id = comboBrushId(brush);
  comboRecent = comboRecent.filter(b => comboBrushId(b) !== id);
  comboRecent.unshift({...brush});
  comboRecent = comboRecent.slice(0, 400);
  renderComboRecent();
}
function renderComboRecent() {
  const all = comboRecent.filter(b => comboDefForBrush(b));
  comboRecent = all;
  el('combo_recent').innerHTML = all.map((b, i) => {
    const def = comboDefForBrush(b);
    return `<div class='combo-recent-item ${comboBrushId(comboBrush)===comboBrushId(b) && !comboEraser?'active':''}'
      onclick='comboRememberBrush(comboRecent[${i}])' title='${esc(def.name || 'tile')}'>
      <canvas id='combo_recent_${i}' width='48' height='48'></canvas><div>${esc(def.name || b.kind)}</div></div>`;
  }).join('') || '<span class=""hint"">Right-click an empty grid cell and pick a block; up to 400 last-used tiles stay here.</span>';
  all.forEach((b, i) => drawComboBrushPreview('combo_recent_' + i, b));
}
function drawComboBrushPreview(canvasId, brushOrPart) {
  const cv = el(canvasId);
  if (!cv || !brushOrPart) return;
  let frame = null;
  if (brushOrPart.kind === 'static') {
    const cb = brushOrPart.atlas ? brushOrPart : comboDefForBrush(brushOrPart);
    if (cb) frame = { atlas: cb.atlas, atlasH: cb.atlasH, x: cb.x, y: cb.y, w: cb.w, h: cb.h };
  } else if (brushOrPart.kind === 'animated') {
    const def = brushOrPart.frames ? brushOrPart : comboDefForBrush(brushOrPart);
    if (def && def.frames && def.frames.length) frame = def.frames[0];
  } else if (brushOrPart.kind === 'combo') {
    const def = comboDefForBrush(brushOrPart);
    if (def && def.parts && def.parts.length) return drawComboBrushPreview(canvasId, def.parts[0]);
  } else if (brushOrPart.kind === 'vanilla') {
    const id = brushOrPart.id || brushOrPart.blockId;
    const b = blocks.find(x => x.id === id);
    if (b && b.hasIcon) {
      const img = new Image();
      img.onload = () => {
        const ctx = cv.getContext('2d');
        ctx.clearRect(0,0,cv.width,cv.height);
        ctx.imageSmoothingEnabled = false;
        ctx.drawImage(img, 0, 0, cv.width, cv.height);
      };
      img.src = '/api/icon/' + id + '.png';
      return;
    }
  }
  if (frame) drawFrameToCanvas(cv, frame, Math.min(32 / Math.max(frame.w,1), 32 / Math.max(frame.h,1), 1));
}
function renderComboGrid() {
  comboW = Math.max(1, Math.min(64, +el('combo_w').value || comboW));
  comboH = Math.max(1, Math.min(64, +el('combo_h').value || comboH));
  const grid = el('combo_grid');
  const cellSize = Math.round(34 * comboZoom);
  grid.style.gridTemplateColumns = `repeat(${comboW},${cellSize}px)`;
  let html = '';
  for (let y = comboH - 1; y >= 0; y--) {
    for (let x = 0; x < comboW; x++) {
      const parts = comboCells[comboKey(x, y)] || [];
      html += `<div class='combo-cell ${parts.length?'filled':''}' style='width:${cellSize}px;height:${cellSize}px'
        oncontextmenu='comboRightClickCell(event,${x},${y})'
        onclick='comboPaintCell(${x},${y})' title='${x},${y}'>` +
        (parts.length ? `<canvas id='combo_cell_${x}_${y}' width='32' height='32' style='width:${Math.max(8,cellSize-2)}px;height:${Math.max(8,cellSize-2)}px'></canvas><span class='badge'>${parts.length}</span>` : '') +
        `</div>`;
    }
  }
  grid.innerHTML = html;
  for (let y = 0; y < comboH; y++) for (let x = 0; x < comboW; x++) {
    const parts = comboCells[comboKey(x, y)] || [];
    if (parts.length) drawComboBrushPreview(`combo_cell_${x}_${y}`, parts[parts.length - 1]);
  }
  el('combo_status').textContent = `${comboW}×${comboH}, ${comboAllParts().length} part(s); uses current editor layer`;
}
function comboPartFromBrush(x, y) {
  if (!comboBrush) return null;
  if (comboBrush.kind === 'static') {
    const cb = (prefs.customBlocks || [])[comboBrush.i];
    if (!cb) return null;
    return { kind:'static', dx:x, dy:y, layer:-1, decor:!!cb.decor, atlas:cb.atlas,
      atlasH:cb.atlasH, x:cb.x, y:cb.y, w:cb.w, h:cb.h };
  }
  const def = (prefs.animatedDefs || [])[comboBrush.i];
  if (!def) return null;
  return { kind:'animated', dx:x, dy:y, layer:-1, decor:!!def.decor, fps:def.fps || 4,
    loop:def.loop !== false, pingpong:!!def.pingpong,
    frames:(def.frames || []).map(f => ({...f})) };
}
function comboPartsFromBrush(x, y, brush) {
  brush = brush || comboBrush;
  if (!brush) return [];
  if (brush.kind === 'vanilla') {
    return [{ kind:'vanilla', dx:x, dy:y, layer:-1, blockId: brush.id }];
  }
  if (brush.kind === 'combo') {
    const def = comboDefForBrush(brush);
    if (!def) return [];
    return (def.parts || []).map(p => ({ ...p, dx: x + (p.dx || 0), dy: y + (p.dy || 0), layer:-1 }));
  }
  const old = comboBrush;
  comboBrush = brush;
  const part = comboPartFromBrush(x, y);
  comboBrush = old;
  return part ? [part] : [];
}
function comboPaintCell(x, y) {
  const k = comboKey(x, y);
  if (comboEraser) { delete comboCells[k]; renderComboGrid(); return; }
  if (!comboBrush) return;
  const parts = comboPartsFromBrush(x, y, comboBrush);
  if (!parts.length) return;
  parts.forEach(p => {
    const pk = comboKey(p.dx, p.dy);
    comboCells[pk] = [p];
  });
  renderComboGrid();
}
function comboRightClickCell(ev, x, y) {
  ev.preventDefault();
  const k = comboKey(x, y);
  if ((comboCells[k] || []).length) {
    comboEraseCell(ev, x, y);
    return;
  }
  comboPickTarget = { x, y };
  comboPickMode = true;
  el('combo_overlay').classList.remove('show');
  el('combo_pick_bar').style.display = 'flex';
  const tab = document.querySelector('.tab[data-page=""blocks""]');
  if (tab) showTab(tab);
  msg('Pick a custom, animated, or combined block for the empty cell');
}
function cancelComboPick() {
  comboPickMode = false;
  comboPickTarget = null;
  el('combo_pick_bar').style.display = 'none';
  el('combo_overlay').classList.add('show');
}
function comboFinishPick(brush) {
  comboRememberBrush(brush);
  if (comboPickTarget) {
    const parts = comboPartsFromBrush(comboPickTarget.x, comboPickTarget.y, brush);
    parts.forEach(p => {
      const pk = comboKey(p.dx, p.dy);
      comboCells[pk] = [p];
    });
  }
  comboPickMode = false;
  comboPickTarget = null;
  el('combo_pick_bar').style.display = 'none';
  el('combo_overlay').classList.add('show');
  renderComboRecent(); renderComboGrid();
}
function comboEraseCell(ev, x, y) {
  ev.preventDefault();
  delete comboCells[comboKey(x, y)];
  renderComboGrid();
}
function comboClearGrid() {
  if (comboAllParts().length && !confirm('Clear combined block grid?')) return;
  comboCells = {};
  renderComboGrid();
}
function serializeCompositeParts(parts) {
  return parts.map(p => {
    if (p.kind === 'vanilla') {
      return `${p.dx},${p.dy},-1,f,v,0|${p.blockId}`;
    }
    if (p.kind === 'animated') {
      const frames = (p.frames || []).map(f => `${f.atlas}:${f.x}:${f.y}:${f.w}:${f.h}`).join(';');
      return `${p.dx},${p.dy},-1,${p.decor?'d':'f'},a,${p.fps||4},${p.loop!==false?'1':'0'},${p.pingpong?'p':'n'}|${frames}`;
    }
    return `${p.dx},${p.dy},-1,${p.decor?'d':'f'},s,0|${p.atlas}:${p.x}:${p.y}:${p.w}:${p.h}`;
  }).join('\n');
}
async function armCompositePreset(i) {
  const def = (prefs.compositeDefs || [])[i];
  if (!def) return;
  if (comboPickMode) {
    comboFinishPick({ kind:'combo', i });
    return;
  }
  const res = await fetch(`/api/composite/arm?name=${encodeURIComponent(def.name)}&w=${def.w}&h=${def.h}`, {
    method:'POST',
    headers:{'Content-Type':'text/plain; charset=utf-8'},
    body: serializeCompositeParts(def.parts || [])
  });
  const r = await res.json();
  msg(r.msg || (r.ok ? `armed ${def.name}` : 'failed to arm combined block'));
}
function saveComboPreset() {
  const name = el('combo_name').value.trim() || 'Combined block';
  const parts = comboAllParts();
  if (parts.length === 0) { msg('Add at least one block to the grid first'); return; }
  prefs.compositeDefs = prefs.compositeDefs || [];
  prefs.compositeDefs.push({ id: Date.now().toString(36), name, category:'combined', w:comboW, h:comboH, parts });
  savePrefs();
  renderCustomBlocks();
  closeComboEditor();
  msg(`Saved combined block ""${name}""`);
}
function deleteCompositePreset(i) {
  const def = (prefs.compositeDefs || [])[i];
  if (!def || !confirm(`Delete combined block ""${def.name}""?`)) return;
  prefs.compositeDefs.splice(i, 1);
  savePrefs();
  renderCustomBlocks();
}
function editCompositePreset(i) { openComboEditor((prefs.compositeDefs || [])[i]); }
function comboImportStart() {
  if (lastState && lastState.cursor) {
    el('combo_ix').value = lastState.cursor.x;
    el('combo_iy').value = lastState.cursor.y;
  }
  msg('Move cursor/enter region, then click Import region in the combined editor');
}
async function comboImportRegion() {
  const x = +el('combo_ix').value || 0, y = +el('combo_iy').value || 0;
  const w = Math.max(1, +el('combo_iw').value || 1), h = Math.max(1, +el('combo_ih').value || 1);
  const r = await (await fetch(`/api/composite/import?x=${x}&y=${y}&w=${w}&h=${h}`)).json();
  if (!r.ok) { msg(r.msg || 'import failed'); return; }
  if ((r.w > comboW || r.h > comboH) && confirm('Make grid larger to fit pasted selection?')) {
    comboW = Math.max(comboW, r.w); comboH = Math.max(comboH, r.h);
    el('combo_w').value = comboW; el('combo_h').value = comboH;
  }
  (r.parts || []).forEach(p => {
    const k = comboKey(p.dx, p.dy);
    comboCells[k] = [{...p, layer:-1}];
  });
  renderComboRecent(); renderComboGrid();
  msg(`Imported ${(r.parts || []).length} modded tile(s)`);
}

/* ---- connected texture helper ---- */
let ctShape = [];
let ctConfig = null;
let ctSourceTiles = [];
let ctDraggedTile = null;
let ctVariationTick = 0;
const CT_LABELS = ['solo','N','E','NE','S','NS','ES','NES','W','NW','EW','NEW','SW','NSW','ESW','NESW'];

function ctKnownMask(x, y) {
  return y * 4 + x;
}

function openConnectedTextureEditor() {
  if (!tsAtlas || !tsSel) { msg('Select a tileset region first'); return; }
  if (!el('ct_overlay')) { msg('connected texture UI missing from page; reload the web UI'); return; }
  const base = selToFrame();
  if (!base) { msg('Could not read selected tileset region'); return; }
  if (ctConfig && ctConfig._appendNext) {
    ctConfig._appendNext = false;
    ctBuildSourceFromSelection(true);
    el('ct_overlay').classList.add('show');
    renderCtSource();
    renderCtSlots();
    renderCtPattern();
    renderCtPreview();
    msg('Added selected area to connected texture source tiles');
    return;
  }
  ctConfig = {
    id: Date.now().toString(36),
    name: tsAtlas.name + ' connected',
    mode: 'floor',
    base,
    variants: {}, // mask -> [{ frame, weight }]
    source: [],
  };
  el('ct_name').value = ctConfig.name;
  el('ct_mode').value = 'floor';
  ctBuildSourceFromSelection(false);
  ctDefaultShape();
  el('ct_overlay').classList.add('show');
  renderCtModeHelp();
  renderCtSource();
  renderCtSlots();
  renderCtPattern();
  renderCtPreview();
}
function closeConnectedTextureEditor() { el('ct_overlay').classList.remove('show'); }
function ctSwitchMode(mode) {
  if (!ctConfig) return;
  ctConfig.mode = mode;
  renderCtModeHelp();
  renderCtSlots();
  renderCtPreview();
}
function renderCtModeHelp() {
  const mode = ctConfig ? ctConfig.mode : 'floor';
  const text = mode === 'floor'
    ? '<b>Floor mode:</b> 16 known side masks. N/E/S/W connections pick the tile.'
    : mode === 'wall'
      ? '<b>Wall mode:</b> same 16 masks, labelled for wall/edge pieces. Good for outlines and fence-like tiles.'
      : '<b>Other/manual:</b> still uses the 16 mask board, but you decide what each mask means.';
  el('ct_mode_help').innerHTML = text + ' Drag source tiles onto matching known slots; multiple tiles become weighted variations.';
}
function ctBuildSourceFromSelection(append) {
  if (!tsAtlas || !tsSel || !ctConfig) return;
  if (!append) ctSourceTiles = [];
  const existing = new Set(ctSourceTiles.map(t => `${t.frame.atlas}:${t.frame.x}:${t.frame.y}:${t.frame.w}:${t.frame.h}`));
  for (let cy = tsSel.cy; cy < tsSel.cy + tsSel.ch; cy++) {
    for (let cx = tsSel.cx; cx < tsSel.cx + tsSel.cw; cx++) {
      const frame = {
        atlas: tsAtlas.name, atlasH: tsAtlas.h,
        x: cx * 32,
        y: tsAtlas.h - (cy * 32 + 32),
        w: 32, h: 32
      };
      const key = `${frame.atlas}:${frame.x}:${frame.y}:${frame.w}:${frame.h}`;
      if (existing.has(key)) continue;
      existing.add(key);
      ctSourceTiles.push({ id: 'src_' + ctSourceTiles.length + '_' + Date.now(), frame });
    }
  }
  ctConfig.source = ctSourceTiles.map(t => t.frame);
}
function ctSelectMore() {
  if (!ctConfig) return;
  el('ct_overlay').classList.remove('show');
  msg('Select another tileset area, then click the connected texture button again to append it.');
  ctConfig._appendNext = true;
}
function ctDefaultShape() {
  const w = 15, h = 11;
  ctShape = Array.from({length:h}, (_, y) => Array.from({length:w}, (_, x) =>
    (x >= 2 && x <= 12 && y >= 2 && y <= 8 && !(x >= 7 && x <= 10 && y >= 5 && y <= 7)) ||
    (x >= 1 && x <= 4 && y >= 6) || (x >= 11 && y <= 3) || (x === 7 && y === 1)));
}
function ctMask(x, y) {
  const on = (xx, yy) => yy >= 0 && yy < ctShape.length && xx >= 0 && xx < ctShape[0].length && ctShape[yy][xx];
  let m = 0;
  if (on(x, y + 1)) m |= 1;
  if (on(x + 1, y)) m |= 2;
  if (on(x, y - 1)) m |= 4;
  if (on(x - 1, y)) m |= 8;
  return m;
}
function ctVariantFor(mask) {
  const arr = ctConfig && ctConfig.variants[mask];
  if (arr && arr.length) return arr[ctVariationTick % arr.length];
  return null;
}
function ctUsedSourceKeys() {
  const set = new Set();
  if (!ctConfig) return set;
  Object.keys(ctConfig.variants).forEach(k => (ctConfig.variants[k] || []).forEach(v => {
    const f = v.frame;
    set.add(`${f.atlas}:${f.x}:${f.y}:${f.w}:${f.h}`);
  }));
  return set;
}
function renderCtSource() {
  const used = ctUsedSourceKeys();
  el('ct_source').innerHTML = ctSourceTiles.map((t, i) => {
    const f = t.frame;
    const key = `${f.atlas}:${f.x}:${f.y}:${f.w}:${f.h}`;
    return `<div class='ct-source-tile ${used.has(key)?'used':''}' draggable='true'
      ondragstart='ctDragSource(event,${i})' title='${f.x/32},${(f.atlasH-f.y-f.h)/32}'>
      <canvas id='ct_src_${i}' width='48' height='48'></canvas><div>${i+1}</div></div>`;
  }).join('') || '<span class=""hint"">No source tiles. Select an atlas area first.</span>';
  ctSourceTiles.forEach((t, i) => drawFrameToCanvas(el('ct_src_' + i), t.frame, 1.5));
}
function ctDragSource(ev, i) {
  ctDraggedTile = ctSourceTiles[i] || null;
  if (ev && ev.dataTransfer) ev.dataTransfer.setData('text/plain', '' + i);
}
function ctDropMask(ev, mask) {
  ev.preventDefault();
  if (!ctDraggedTile || !ctConfig) return;
  ctAssignVariant(mask, ctDraggedTile.frame);
}
function ctDropPreview(ev, x, y) {
  ev.preventDefault();
  if (!ctDraggedTile || !ctConfig || !ctShape[y][x]) return;
  ctAssignVariant(ctMask(x, y), ctDraggedTile.frame);
}
function ctAssignVariant(mask, frame) {
  const variants = ctConfig.variants[mask] || [];
  let action = 'replace';
  if (variants.length) {
    action = prompt('Tile already configured. Type replace or variation:', 'variation') || 'cancel';
    action = action.toLowerCase();
    if (action !== 'replace' && action !== 'variation') return;
  }
  const weight = Math.max(1, Math.min(100, +(prompt('Weight percent for this tile:', variants.length ? '100' : '100') || 100)));
  const entry = { frame: {...frame}, weight };
  ctConfig.variants[mask] = action === 'variation' ? [...variants, entry] : [entry];
  renderCtSource(); renderCtSlots(); renderCtPreview();
}
function ctDeleteMask(ev, mask) {
  ev.preventDefault();
  const variants = ctConfig && ctConfig.variants[mask];
  if (!variants || !variants.length) return;
  if (variants.length === 1) delete ctConfig.variants[mask];
  else {
    const choices = variants.map((v,i) => `${i+1}: weight ${v.weight}%`).join('\n');
    const idx = +(prompt('Which variation delete?\n' + choices, '1') || 0) - 1;
    if (idx >= 0 && idx < variants.length) variants.splice(idx, 1);
    if (!variants.length) delete ctConfig.variants[mask];
  }
  renderCtSource(); renderCtSlots(); renderCtPreview();
}
function renderCtSlots() {
  const wrap = el('ct_slots');
  wrap.style.gridTemplateColumns = 'repeat(4,70px)';
  let html = '';
  for (let y = 3; y >= 0; y--) for (let x = 0; x < 4; x++) {
    const mask = ctKnownMask(x, y);
    const v = ctVariantFor(mask);
    html += `<div class='ct-slot ${v?'':'empty'}' ondragover='event.preventDefault()'
      ondrop='ctDropMask(event,${mask})' oncontextmenu='ctDeleteMask(event,${mask})'
      title='mask ${mask}: ${CT_LABELS[mask]}'>
      <span class='mask'>${mask} ${CT_LABELS[mask]}</span>${ctConfig.variants[mask] && ctConfig.variants[mask].length>1 ? `<span class='var'>×${ctConfig.variants[mask].length}</span>` : ''}
      <canvas id='ct_slot_${mask}' width='64' height='64'></canvas></div>`;
  }
  wrap.innerHTML = html;
  for (let mask = 0; mask < 16; mask++) {
    const v = ctVariantFor(mask);
    drawFrameToCanvas(el('ct_slot_' + mask), v ? v.frame : ctConfig.base, 2);
  }
}
function renderCtPreview() {
  const wrap = el('ct_preview');
  if (!ctShape.length) ctDefaultShape();
  wrap.style.gridTemplateColumns = `repeat(${ctShape[0].length},34px)`;
  let html = '';
  for (let y = ctShape.length - 1; y >= 0; y--) {
    for (let x = 0; x < ctShape[0].length; x++) {
      const on = ctShape[y][x];
      const mask = on ? ctMask(x, y) : 0;
      const configured = on && ctVariantFor(mask);
      html += `<div class='ct-cell ${on?'on':''} ${configured?'':'ghost'}'
        ondragover='event.preventDefault()' ondrop='ctDropPreview(event,${x},${y})'
        onclick='ctToggle(${x},${y})' title='${on ? 'mask '+mask+' '+CT_LABELS[mask] : 'empty'}'>` +
        (on ? `<canvas id='ct_${x}_${y}' width='32' height='32'></canvas>` : '') +
        `</div>`;
    }
  }
  wrap.innerHTML = html;
  for (let y = 0; y < ctShape.length; y++) for (let x = 0; x < ctShape[0].length; x++) {
    if (!ctShape[y][x]) continue;
    const v = ctVariantFor(ctMask(x, y));
    const f = v ? v.frame : (ctConfig && ctConfig.base);
    if (f) drawFrameToCanvas(el(`ct_${x}_${y}`), f, 1);
  }
  el('ct_status').textContent = `${Object.keys(ctConfig.variants).length}/16 masks configured`;
}
function renderCtPattern() {
  const wrap = el('ct_pattern');
  if (!ctShape.length) ctDefaultShape();
  wrap.style.gridTemplateColumns = `repeat(${ctShape[0].length},34px)`;
  let html = '';
  for (let y = ctShape.length - 1; y >= 0; y--) {
    for (let x = 0; x < ctShape[0].length; x++) {
      html += `<div class='ct-cell ${ctShape[y][x]?'on':''}' onclick='ctToggle(${x},${y})'></div>`;
    }
  }
  wrap.innerHTML = html;
}
function ctToggle(x, y) {
  ctShape[y][x] = !ctShape[y][x];
  renderCtPattern();
  renderCtPreview();
}
function ctClearPattern() {
  ctShape = Array.from({length:11}, () => Array.from({length:15}, () => false));
  renderCtPattern(); renderCtPreview();
}
function ctAutoDetect() {
  if (!ctConfig || !tsAtlas || !tsSel) return;
  const startCx = tsSel.cx, startCy = tsSel.cy;
  for (let i = 0; i < 16; i++) {
    const cx = startCx + (i % 4) * tsSel.cw;
    const cy = startCy + Math.floor(i / 4) * tsSel.ch;
    if ((cx + tsSel.cw) * 32 <= tsAtlas.w && (cy + tsSel.ch) * 32 <= tsAtlas.h) {
      const w = tsSel.cw * 32, h = tsSel.ch * 32;
      const frame = {
        atlas: tsAtlas.name, atlasH: tsAtlas.h,
        x: cx * 32,
        y: tsAtlas.h - (cy * 32 + h),
        w, h
      };
      ctConfig.variants[i] = [{ frame, weight: 100 }];
    }
  }
  renderCtSource(); renderCtSlots(); renderCtPreview();
  el('ct_status').textContent = 'Auto-filled 16 known-layout slots from a 4×4 source layout.';
}
function ctSave() {
  if (!ctConfig) return;
  const name = prompt('Name this connected texture:', el('ct_name').value.trim() || ctConfig.name);
  if (!name) return;
  ctConfig.name = name;
  ctConfig.mode = el('ct_mode').value || ctConfig.mode;
  const previewNote = prompt('Preview drawing label/notes (the visible pattern is saved too):', 'default stress pattern') || '';
  ctConfig.previewNote = previewNote;
  ctConfig.previewShape = ctShape.map(row => row.slice());
  ctConfig.savedAt = Date.now();
  prefs.connectedTextures = prefs.connectedTextures || [];
  prefs.connectedTextures.push(JSON.parse(JSON.stringify(ctConfig)));
  savePrefs();
  renderCustomBlocks();
  el('ct_status').textContent = 'Saved connected texture config.';
  msg('connected texture config saved');
}

function serializeConnected(def) {
  return Object.keys(def.variants || {}).map(mask => {
    const vars = def.variants[mask] || [];
    const line = vars.map(v => {
      const f = v.frame || v;
      return `${f.atlas}:${f.x}:${f.y}:${f.w}:${f.h}:${v.weight || 100}`;
    }).join(';');
    return `${mask}|${line}`;
  }).join('\n');
}
async function armConnectedTexture(i) {
  const def = (prefs.connectedTextures || [])[i];
  if (!def) return;
  const url = `/api/connected/arm?id=${encodeURIComponent(def.id||'')}&name=${encodeURIComponent(def.name)}` +
    `&mode=${encodeURIComponent(def.mode||'floor')}&color=1` +
    `&decor=${def.mode==='wall'?1:0}&collide=0&destructible=0&damaging=0`;
  const r = await (await fetch(url, { method:'POST', headers:{'Content-Type':'text/plain'}, body: serializeConnected(def) })).json();
  msg(r.msg || (r.ok ? `armed ${def.name}` : 'failed to arm connected texture'));
}
function deleteConnectedTexture(i) {
  const def = (prefs.connectedTextures || [])[i];
  if (!def || !confirm(`Delete connected texture ""${def.name}""?`)) return;
  prefs.connectedTextures.splice(i, 1);
  savePrefs();
  renderCustomBlocks();
}
function drawConnectedThumb(id, def) {
  const cv = el(id);
  if (!cv || !def) return;
  let f = null;
  const keys = Object.keys(def.variants || {});
  if (keys.length && def.variants[keys[0]] && def.variants[keys[0]].length) {
    f = def.variants[keys[0]][0].frame;
  } else {
    f = def.base;
  }
  if (f) drawFrameToCanvas(cv, f, Math.min(64 / Math.max(f.w,1), 64 / Math.max(f.h,1), 8));
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
  const comboDefs = prefs.compositeDefs || [];
  const ctDefs = prefs.connectedTextures || [];
  const section = el('cb_section');
  if (staticBlocks.length === 0 && animDefs.length === 0 && comboDefs.length === 0 && ctDefs.length === 0) { section.style.display = 'none'; return; }
  section.style.display = '';
  const q = (el('q') ? el('q').value : '').toLowerCase();

  // gather all categories
  const catMap = {}; // cat -> { static, anim, combo, ct }
  staticBlocks.forEach((cb, i) => {
    const cat = cb.category || prefs.customCategoryName || 'Custom';
    if (!catMap[cat]) catMap[cat] = { static: [], anim: [], combo: [], ct: [] };
    if (!q || cb.name.toLowerCase().includes(q)) catMap[cat].static.push({ i, cb });
  });
  animDefs.forEach((def, i) => {
    const cat = def.category || 'animated';
    if (!catMap[cat]) catMap[cat] = { static: [], anim: [], combo: [], ct: [] };
    if (!q || def.name.toLowerCase().includes(q)) catMap[cat].anim.push({ i, def });
  });
  comboDefs.forEach((def, i) => {
    const cat = def.category || 'combined';
    if (!catMap[cat]) catMap[cat] = { static: [], anim: [], combo: [], ct: [] };
    if (!q || def.name.toLowerCase().includes(q)) catMap[cat].combo.push({ i, def });
  });
  ctDefs.forEach((def, i) => {
    const cat = def.mode === 'wall' ? 'Connected Walls' : 'Connected Floors';
    if (!catMap[cat]) catMap[cat] = { static: [], anim: [], combo: [], ct: [] };
    if (!q || def.name.toLowerCase().includes(q)) catMap[cat].ct.push({ i, def });
  });

  const cats = Object.keys(catMap);
  el('cb_cats').innerHTML = cats.map(cat => {
    const items = catMap[cat];
    const staticHtml = items.static.map(({ i, cb }) => `
      <div class='cb_cell ${i === customSelectedId ? ""sel"" : """"}' onclick='brushCustomBlock(${i})'
           onmouseenter='cbTipShow(event,${i})' onmousemove='tipMove(event)' onmouseleave='tipHide()'>
        <span class='cb_del' onclick='event.stopPropagation();deleteCustomBlock(${i})'>×</span>
        <canvas class='cb_thumb' id='cb_thumb_${i}' width='64' height='64' title='${esc(cb.name)}'></canvas>
        <div class='nm'>${esc(cb.name)}</div>
      </div>`).join('');
    const animHtml = items.anim.map(({ i, def }) => `
      <div class='cb_cell anim ${i === animSelectedId ? ""sel"" : """"}' onclick='brushAnimBlock(${i})'
           onmouseenter='animTipShow(event,${i})' onmousemove='tipMove(event)' onmouseleave='tipHide()'>
        <span class='cb_del' onclick='event.stopPropagation();deleteAnimBlock(${i})'>×</span>
        <canvas class='cb_anim' id='cb_anim_${i}' width='64' height='64' title='${esc(def.name)}'></canvas>
        <div class='nm'>${esc(def.name)}</div>
        <div style='font-size:9px;color:#6688aa'>${def.frames.length}f@${def.fps}fps</div>
      </div>`).join('');
    const comboHtml = items.combo.map(({ i, def }) => `
      <div class='cb_cell ${i === -999 ? ""sel"" : """"}' onclick='armCompositePreset(${i})'
           title='${esc(def.parts.length + "" part(s), "" + def.w + ""×"" + def.h)}'>
        <span class='cb_del' onclick='event.stopPropagation();deleteCompositePreset(${i})'>×</span>
        <span onclick='event.stopPropagation();editCompositePreset(${i})' style='position:absolute;left:5px;top:3px;color:#9ab;cursor:pointer;font-size:11px'>✎</span>
        <canvas class='cb_combo' id='cb_combo_${i}' width='64' height='64' title='${esc(def.name)}'></canvas>
        <div class='nm'>${esc(def.name)}</div>
        <div style='font-size:9px;color:#8aa'>${def.w}×${def.h}, ${def.parts.length} parts</div>
      </div>`).join('');
    const ctHtml = items.ct.map(({ i, def }) => `
      <div class='cb_cell ${def.mode==='wall'?'anim':''}' onclick='armConnectedTexture(${i})'
           title='connected ${esc(def.mode || 'floor')}'>
        <span class='cb_del' onclick='event.stopPropagation();deleteConnectedTexture(${i})'>×</span>
        <canvas class='cb_combo' id='cb_ct_${i}' width='64' height='64' title='${esc(def.name)}'></canvas>
        <div class='nm'>${esc(def.name)}</div>
        <div style='font-size:9px;color:#8aa'>${esc(def.mode || 'floor')} CT</div>
      </div>`).join('');
    return `<div class='cat-section'>
      <div class='cat-hdr'>
        <h2>${esc(cat)}</h2>
        <button onclick='renameCategoryPrompt(""${esc(cat)}"")' title='Rename category' style='padding:2px 7px;font-size:11px'>✎</button>
      </div>
      <div class='cb_grid'>${staticHtml}${animHtml}${comboHtml}${ctHtml}</div>
    </div>`;
  }).join('');
  staticBlocks.forEach((cb, i) => drawCustomBlockThumb(i, cb));
  comboDefs.forEach((def, i) => {
    if (def.parts && def.parts.length) drawComboBrushPreview('cb_combo_' + i, def.parts[0]);
  });
  ctDefs.forEach((def, i) => drawConnectedThumb('cb_ct_' + i, def));
  startCbAnimPreviews();
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
let animPickMode = false;
const atlasImgCache = {};
const cbAnimTimers = {};

function loadAtlasImage(name) {
  const hit = atlasImgCache[name];
  if (hit && hit.complete && hit.naturalWidth) return Promise.resolve(hit);
  return new Promise((res, rej) => {
    const img = new Image();
    img.crossOrigin = 'anonymous';
    img.onload = () => { atlasImgCache[name] = img; res(img); };
    img.onerror = rej;
    img.src = '/api/tilesets/atlas.png?name=' + encodeURIComponent(name) + '&t=' + Date.now();
  });
}

async function preloadAnimFrames() {
  const names = [...new Set(animFrames.map(f => f.atlas))];
  await Promise.all(names.map(n => loadAtlasImage(n).catch(() => null)));
}

function drawFrameToCanvasSync(canvas, frame, scale) {
  if (!canvas || !frame) return;
  if (scale == null) scale = Math.max(1, Math.min(8, +el('anim_preview_scale').value || 2));
  const w = Math.max(1, Math.round(frame.w * scale));
  const h = Math.max(1, Math.round(frame.h * scale));
  if (canvas.width !== w) canvas.width = w;
  if (canvas.height !== h) canvas.height = h;
  const ctx = canvas.getContext('2d');
  ctx.clearRect(0, 0, w, h);
  const img = atlasImgCache[frame.atlas];
  if (!img || !img.complete || !img.naturalWidth) {
    ctx.fillStyle = '#334';
    ctx.fillRect(0, 0, w, h);
    return;
  }
  const atlasH = frame.atlasH || img.naturalHeight;
  const srcY = atlasH - frame.y - frame.h;
  ctx.imageSmoothingEnabled = false;
  ctx.drawImage(img, frame.x, srcY, frame.w, frame.h, 0, 0, w, h);
}

function stopCbAnimPreviews() {
  for (const k in cbAnimTimers) { clearInterval(cbAnimTimers[k]); delete cbAnimTimers[k]; }
}

async function startCbAnimPreviews() {
  stopCbAnimPreviews();
  const animDefs = prefs.animatedDefs || [];
  for (let i = 0; i < animDefs.length; i++) {
    const def = animDefs[i];
    if (!def.frames || def.frames.length === 0) continue;
    const cv = el('cb_anim_' + i);
    if (!cv) continue;
    for (const f of def.frames) await loadAtlasImage(f.atlas).catch(() => null);
    let idx = 0, dir = 1;
    const f0 = def.frames[0];
    const scale = Math.min(64 / Math.max(f0.w, 1), 64 / Math.max(f0.h, 1), 8);
    const draw = () => drawFrameToCanvasSync(cv, def.frames[idx], scale);
    draw();
    if (def.frames.length <= 1) continue;
    const ms = 1000 / Math.max(1, def.fps || 4);
    cbAnimTimers['cb_' + i] = setInterval(() => {
      const loop = def.loop !== false;
      const pingpong = !!def.pingpong;
      if (pingpong) {
        idx += dir;
        if (idx >= def.frames.length - 1) { idx = def.frames.length - 1; dir = -1; }
        else if (idx <= 0) { idx = 0; dir = 1; }
      } else if (loop) {
        idx = (idx + 1) % def.frames.length;
      } else if (idx < def.frames.length - 1) {
        idx++;
      }
      draw();
    }, ms);
  }
}

function advanceAnimPreview(count, loop, pingpong) {
  if (pingpong) {
    animPreviewIdx += animPreviewDir;
    if (animPreviewIdx >= count - 1) { animPreviewIdx = count - 1; animPreviewDir = -1; }
    else if (animPreviewIdx <= 0) { animPreviewIdx = 0; animPreviewDir = 1; }
    return false;
  }
  if (loop) {
    animPreviewIdx = (animPreviewIdx + 1) % count;
    return false;
  }
  if (animPreviewIdx >= count - 1) return true;
  animPreviewIdx++;
  return false;
}

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

function beginPickNextFrame() {
  animPickMode = true;
  el('anim_overlay').classList.remove('show');
  el('anim_pick_bar').style.display = 'flex';
  el('anim_pick_status').textContent = tsSel
    ? `Selection ready (${tsSel.cw}×${tsSel.ch} @ ${tsSel.cx},${tsSel.cy}) — click Add frame or pick another`
    : 'Pick next frame — open Tilesets, drag a selection, then click Add frame';
  const tab = document.querySelector('.tab[data-page=""tilesets""]');
  if (tab) showTab(tab);
}

function confirmPickFrame() {
  if (!animPickMode) return;
  const frame = selToFrame();
  if (!frame) { msg('Select a region in a tileset atlas first'); return; }
  animFrames.push(frame);
  animPickMode = false;
  el('anim_pick_bar').style.display = 'none';
  renderAnimFrames();
  restartPreviewIfRunning();
  el('anim_overlay').classList.add('show');
  msg(`Frame ${animFrames.length} added`);
}

function cancelPickFrame() {
  animPickMode = false;
  el('anim_pick_bar').style.display = 'none';
  el('anim_overlay').classList.add('show');
}

function addCurrentSelectionAsFrame() {
  const frame = selToFrame();
  if (!frame) { msg('Select a region in a tileset atlas first'); return; }
  animFrames.push(frame);
  renderAnimFrames();
  animTimingHint();
  restartPreviewIfRunning();
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
  if (animPreviewTimer) { restartPreviewIfRunning(); return; }
  loadAtlasImage(animFrames[0].atlas).then(() => {
    drawFrameToCanvasSync(el('anim_preview_canvas'), animFrames[0]);
  }).catch(() => {});
}

function restartPreviewIfRunning() {
  if (animPreviewTimer) { stopAnimPreview(); previewAnimEditor(); }
  else animTimingHint();
}

async function drawFrameToCanvas(canvas, frame, scale) {
  if (scale == null) {
    scale = Math.min(64 / Math.max(frame.w, 1), 64 / Math.max(frame.h, 1), 8);
  }
  try { await loadAtlasImage(frame.atlas); } catch (e) {}
  drawFrameToCanvasSync(canvas, frame, scale);
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
  html += `<div class='frame-add-btn' onclick='beginPickNextFrame()' title='Minimize editor and pick next frame from tilesets'>＋</div>`;
  strip.innerHTML = html;
  // draw each frame asynchronously
  animFrames.forEach((f, i) => {
    const cv = el('frame_canvas_' + i);
    if (cv) drawFrameToCanvas(cv, f);
  });
  animTimingHint();
  if (!animPreviewTimer && count > 0) {
    loadAtlasImage(animFrames[0].atlas).then(() => {
      drawFrameToCanvasSync(el('anim_preview_canvas'), animFrames[0]);
    }).catch(() => {});
  }
  el('anim_frame_disp').textContent = `Frame ${count > 0 ? animPreviewIdx+1 : 0}/${count}`;
}

function previewAnimEditor() {
  if (animFrames.length === 0) { msg('Add at least one frame first'); return; }
  stopAnimPreview();
  preloadAnimFrames().then(() => {
    animPreviewIdx = 0;
    animPreviewDir = 1;
    const fps = Math.max(1, Math.min(60, +el('anim_fps').value || 4));
    const loop = el('anim_loop').checked;
    const pingpong = el('anim_pingpong').checked;
    function tick() {
      drawFrameToCanvasSync(el('anim_preview_canvas'), animFrames[animPreviewIdx]);
      el('anim_frame_disp').textContent = `Frame ${animPreviewIdx + 1}/${animFrames.length}`;
      if (advanceAnimPreview(animFrames.length, loop, pingpong)) {
        stopAnimPreview();
        return;
      }
      animPreviewTimer = setTimeout(tick, 1000 / fps);
    }
    tick();
  });
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
    const img = await loadAtlasImage(tsAtlas.name);
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
    const fwTiles = Math.max(1, Math.min(16, +el('anim_detect_w').value || tsSel.cw || 1));
    const fhTiles = Math.max(1, Math.min(16, +el('anim_detect_h').value || tsSel.ch || 1));
    const maxFrames = Math.max(2, Math.min(256, +el('anim_detect_max').value || 128));
    const allowedGaps = Math.max(0, Math.min(8, +el('anim_detect_gaps').value || 0));
    const mode = el('anim_detect_mode').value || 'auto';
    const sw = fwTiles * 32, sh = fhTiles * 32;
    const start = { cx: tsSel.cx, cy: tsSel.cy };
    function validCell(cx, cy) {
      return cx >= 0 && cy >= 0 &&
        (cx + fwTiles) * 32 <= cv.width && (cy + fhTiles) * 32 <= cv.height;
    }
    function hasCell(cx, cy) {
      return validCell(cx, cy) && regionHasContent(cx * 32, cy * 32, sw, sh);
    }
    function toFrame(cell) {
      return {
        atlas: tsAtlas.name, atlasH: tsAtlas.h,
        x: cell.cx * 32,
        y: tsAtlas.h - (cell.cy * 32 + sh),
        w: sw, h: sh
      };
    }
    function scan(dx, dy) {
      const out = [];
      let cx = start.cx, cy = start.cy, gaps = 0;
      while (validCell(cx, cy) && out.length < maxFrames) {
        if (hasCell(cx, cy)) {
          out.push({ cx, cy });
          gaps = 0;
        } else if (gaps++ >= allowedGaps) break;
        cx += dx * fwTiles;
        cy += dy * fhTiles;
      }
      return out;
    }
    function scanReverse(dx, dy) {
      const out = [];
      let cx = start.cx - dx * fwTiles, cy = start.cy - dy * fhTiles, gaps = 0;
      while (validCell(cx, cy) && out.length < maxFrames) {
        if (hasCell(cx, cy)) {
          out.unshift({ cx, cy });
          gaps = 0;
        } else if (gaps++ >= allowedGaps) break;
        cx -= dx * fwTiles;
        cy -= dy * fhTiles;
      }
      return out;
    }
    function scanArea() {
      const out = [];
      const endCx = tsSel.cx + tsSel.cw;
      const endCy = tsSel.cy + tsSel.ch;
      for (let cy = tsSel.cy; cy + fwTiles <= endCy && out.length < maxFrames; cy += fhTiles) {
        for (let cx = tsSel.cx; cx + fwTiles <= endCx && out.length < maxFrames; cx += fwTiles) {
          if (hasCell(cx, cy)) out.push({ cx, cy });
        }
      }
      return out;
    }
    const candidates = [];
    const right = scan(1, 0), left = scanReverse(1, 0);
    const down = scan(0, 1), up = scanReverse(0, 1);
    candidates.push({ label:'horizontal right', frames:right });
    candidates.push({ label:'horizontal left', frames:[...left, start] });
    candidates.push({ label:'vertical down', frames:down });
    candidates.push({ label:'vertical up', frames:[...up, start] });
    candidates.push({ label:'horizontal both ways', frames:[...left, ...right] });
    candidates.push({ label:'vertical both ways', frames:[...up, ...down] });
    candidates.push({ label:'selected area grid', frames:scanArea() });
    let chosen;
    if (mode === 'right') chosen = candidates[0];
    else if (mode === 'left') chosen = candidates[1];
    else if (mode === 'down') chosen = candidates[2];
    else if (mode === 'up') chosen = candidates[3];
    else if (mode === 'bothh') chosen = candidates[4];
    else if (mode === 'bothv') chosen = candidates[5];
    else if (mode === 'area') chosen = candidates[6];
    else chosen = candidates.slice().sort((a,b) => b.frames.length - a.frames.length)[0];
    const best = chosen.frames.slice(0, maxFrames);
    if (best.length < 2) {
      el('anim_detect_status').textContent =
        `⚠ Found ${best.length} frame. Try smaller frame size, selected-area mode, or allow gaps.`;
      el('anim_detect_btn').disabled = false;
      return;
    }
    const newFrames = best.map(toFrame);
    animFrames = newFrames;
    renderAnimFrames();
    el('anim_detect_status').textContent =
      `✅ Found ${newFrames.length} frames using ${chosen.label}; frame ${fwTiles}×${fhTiles} tile(s), max ${maxFrames}, gaps ${allowedGaps}.`;
    msg(`Auto-detected ${newFrames.length} frames (${chosen.label})`);
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
  if (comboPickMode) {
    comboFinishPick({ kind:'animated', i });
    return;
  }
  // serialize frames for the API: atlas:rx:ry:rw:rh;...
  const framesParam = def.frames.map(f => `${f.atlas}:${f.x}:${f.y}:${f.w}:${f.h}`).join(';');
  const r = await (await fetch(
    `/api/tilesets/arm-animated?name=${encodeURIComponent(def.name)}&fps=${def.fps}&loop=${def.loop?1:0}&pingpong=${def.pingpong?1:0}&decor=${def.decor?1:0}&frames=${encodeURIComponent(framesParam)}`,
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
refreshCaBundles();
refreshCaPlaced();
setInterval(() => {
  if (el('page_gameplay').classList.contains('on')) refreshGameplay();
  if (el('page_tilesets').classList.contains('on')) refreshTilesets();
  if (el('page_tools').classList.contains('on')) refreshCaPlaced();
}, 3000);
</script>
</body>
</html>";
    }
}
