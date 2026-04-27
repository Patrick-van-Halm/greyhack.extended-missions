# Extended Missions

Extended Missions is a modular BepInEx plugin suite for Grey Hack. It adds new direct mission types and provides an API that other mod authors can use to build their own mission packs.

## Included Plugins

The release package includes the BepInEx host plugin, the core framework, and these mission packs:

- `ExtendedMissions.BepInEx5.dll` or `ExtendedMissions.BepInEx6.dll` - target-version BepInEx host plugin.
- `ExtendedMissions.dll` - shared mission framework and registration patches.
- `ExtendedMissions.SendMoney.dll` - transfer money from one bank account to another.
- `ExtendedMissions.UploadFile.dll` - upload a supplied file to a target machine.
- `ExtendedMissions.CompromisedMail.dll` - send mail from a compromised mailbox.
- `ExtendedMissions.ChangePassword.dll` - change a target user's password.
- `ExtendedMissions.WebsiteDefacement.dll` - replace or modify target website content.

Each mission pack is a separate BepInEx plugin and depends on the core `ExtendedMissions` plugin.

## Installation

### Thunderstore

Use the Thunderstore package output from `ExtendedMissions_BepInEx5_Thunderstore`. It contains:

- `manifest.json`
- `README.md`
- `CHANGELOG.md`
- `icon.png`
- `BepInEx/plugins/ExtendedMissions/*.dll`

Install it like a normal BepInEx 5 Thunderstore package.

### Manual BepInEx 5

Build `ExtendedMissions_BepInEx5`, then copy the generated `BepInEx/plugins/ExtendedMissions` folder into your Grey Hack install.

Expected layout:

```text
Grey Hack/
  BepInEx/
    plugins/
      ExtendedMissions/
        ExtendedMissions.BepInEx5.dll
        ExtendedMissions.dll
        ExtendedMissions.SendMoney.dll
        ExtendedMissions.UploadFile.dll
        ExtendedMissions.CompromisedMail.dll
        ExtendedMissions.ChangePassword.dll
        ExtendedMissions.WebsiteDefacement.dll
```

### Manual BepInEx 6

Build `ExtendedMissions_BepInEx6`, then copy the generated `BepInEx/plugins/ExtendedMissions` folder into your Grey Hack install.

## Building From Source

Build the core framework:

```powershell
dotnet build ExtendedMissions\ExtendedMissions.csproj
```

Build a package layout:

```powershell
dotnet build ExtendedMissions_BepInEx5\ExtendedMissions_BepInEx5.csproj
dotnet build ExtendedMissions_BepInEx6\ExtendedMissions_BepInEx6.csproj
dotnet build ExtendedMissions_BepInEx5_Thunderstore\ExtendedMissions_BepInEx5_Thunderstore.csproj
```

The packaging projects compile the target-version BepInEx host plugin from the target folder's `Plugin.cs`, using that folder's `lib` references, then copy the host and mission DLLs into the expected `BepInEx/plugins/ExtendedMissions` structure.

## For Mission Developers

Extended Missions exposes base classes for direct, world, hidden, karma, and procedural missions. External mission packs should reference the built `ExtendedMissions.dll` and depend on the core BepInEx plugin:

```csharp
[BepInDependency(ExtendedMissions.Plugin.PluginGuid)]
```

See `docs/CreatingMissionMods.md` for the full developer guide and the current example mission packs under `CustomMissions`.

## Notes

- Mission and condition ids are reserved through registry files so ids remain stable as mission packs change.
- The built-in mission packs currently focus on direct board missions.
- The core framework also contains procedural and world mission extension points for external mission packs.
- `Krafs.Publicizer` is used at build time because Grey Hack exposes many useful game members as non-public API.
