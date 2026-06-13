# Dev Setup (Linux)

## Toolchain

- .NET SDK 8 in `~/.dotnet` (plus .NET 6 runtime for ilspycmd)
- `ilspycmd` 8.2 (`~/.dotnet/tools`), needs `DOTNET_ROOT=$HOME/.dotnet`
- AssetRipper GUI in `~/tools/assetripper/`
- UnityPy venv in `~/tools/unitypy-venv/` (scripted asset dumps)

## Game + loader

- Game: `~/.local/share/Steam/steamapps/common/The Escapists 2`
  (Unity 5.5.0p4, Mono, native Linux build, appid 641990)
- **BepInEx 5.4.21 (unix build)** installed in the game dir.
  - 5.4.23.x does NOT work: its preloader crashes in MonoMod `NativeDetour`
    on this game (`preloader_*.log`: NullReferenceException in
    `MonoMod.RuntimeDetour.NativeDetour..ctor`).
  - **Required fix** (applied in `run_bepinex.sh`): the game's mono directory
    must be on `LD_LIBRARY_PATH`, otherwise MonoMod cannot dlopen `libmono.so`
    and every Harmony patch dies with the same NativeDetour NRE:

    ```sh
    export LD_LIBRARY_PATH="${doorstop_libs}":"${BASEDIR}/TheEscapists2_Data/Mono/x86_64":${LD_LIBRARY_PATH}
    ```

## Launching

- Direct (dev): from the game dir

  ```sh
  SteamAppId=641990 SteamGameId=641990 ./run_bepinex.sh
  ```

  (`SteamAppId` prevents the game from relaunching itself through Steam,
  which would drop the doorstop injection.)
- Via Steam (normal play): set launch options for The Escapists 2 to

  ```
  ./run_bepinex.sh %command%
  ```

## Build + deploy

```sh
bash tools/build.sh        # builds src/, copies DLLs to BepInEx/plugins/E2EMapEditor
```

- `src/Directory.Build.props`: net35 target, `GamePath` property (overridable
  with `-p:GamePath=...`), reference assemblies + BepInEx.AssemblyPublicizer.
- Game DLL references are publicized at build time (`Publicize="true"`),
  so private members compile; at runtime the original game DLLs are used.

## Logs

- `BepInEx/LogOutput.log` in the game dir — plugin + chainloader log
- `~/.config/unity3d/Team 17 Digital ltd_/The Escapists 2/Player.log` — Unity
- `preloader_*.log` in the game dir — only appears when the preloader crashes
