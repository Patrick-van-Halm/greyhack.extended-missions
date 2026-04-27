# Creating Mission Mods

This guide is for external developers who want to build new Grey Hack mission packs with the Extended Missions API.

## Project Setup

A mission pack is a normal `netstandard2.1` BepInEx plugin project that references the built `ExtendedMissions.dll`.

Use the existing example projects as templates:

- `CustomMissions/WebsiteDefacement/ExtendedMissions.WebsiteDefacement.csproj`
- `CustomMissions/ChangePassword/ExtendedMissions.ChangePassword.csproj`
- `CustomMissions/UploadFile/ExtendedMissions.UploadFile.csproj`
- `CustomMissions/CompromisedMail/ExtendedMissions.CompromisedMail.csproj`
- `CustomMissions/SendMoney/ExtendedMissions.SendMoney.csproj`

The important parts are:

```xml
<TargetFramework>netstandard2.1</TargetFramework>
<Nullable>enable</Nullable>

<Reference Include="ExtendedMissions">
  <HintPath>lib\ExtendedMissions.dll</HintPath>
  <Private>false</Private>
</Reference>

<PackageReference Include="Krafs.Publicizer" Version="2.3.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>

<Publicize Include="Assembly-CSharp"/>
```

You also need references to the Grey Hack and BepInEx assemblies, matching the example projects.

When developing inside this repository, the example projects use a `ProjectReference` to `ExtendedMissions.csproj` so they always build against the local source. External mission packs should instead reference the compiled API assembly, for example `ExtendedMissions/bin/Debug/netstandard2.1/ExtendedMissions.dll` during local development or the `ExtendedMissions.dll` shipped with a release. Copy that DLL into a stable location such as `lib/ExtendedMissions.dll` and reference it from your project.

## Why Publicizer Is Used

Grey Hack's game assembly exposes many useful runtime types, fields, and methods as non-public members. Mission code often needs access to those members to inspect generated computers, users, files, services, bank accounts, mailboxes, and mission state.

`Krafs.Publicizer` generates a publicized reference assembly for `Assembly-CSharp`, which lets your mission pack compile against those otherwise inaccessible Grey Hack APIs. It is a compile-time tool: your plugin still runs against the real game assembly in Grey Hack. Keep `<Private>false</Private>` on game and BepInEx references so your mission pack does not ship duplicate copies of assemblies that the game already provides.

## Plugin Class

Each mission pack needs a BepInEx plugin class. The class does not need to register missions manually. Extended Missions discovers loaded mission classes automatically.

```csharp
using BepInEx;

namespace MyMissionPack
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(ExtendedMissions.Plugin.PluginGuid)]
    public class MyMissionPackPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.example.greyhack.my-mission-pack";
        public const string PluginName = "My Mission Pack";
        public const string PluginVersion = "0.1.0";
    }
}
```

Use a stable plugin GUID. Extended Missions uses the plugin GUID plus the mission class name to reserve mission ids and translation keys.

## Direct Missions

Direct missions appear on the Hackshop or Police board. They inherit from:

```csharp
ExtendedDirectMission<TMission, TData>
```

`TMission` is the concrete mission class. `TData` is the data you want to persist while the mission is active.

Good examples:

- `CustomMissions/ChangePassword/Public/ChangePasswordMission.cs`
- `CustomMissions/UploadFile/Public/UploadFileMission.cs`
- `CustomMissions/SendMoney/Public/SendMoneyMission.cs`
- `CustomMissions/CompromisedMail/Public/CompromisedMailMission.cs`
- `CustomMissions/WebsiteDefacement/Public/*.cs`

### Minimal Direct Mission Shape

```csharp
using System.Collections.Generic;
using ExtendedMissions.Missions;
using MissionConfig;

public class ExampleDirectMission : ExtendedDirectMission<ExampleDirectMission, ExampleDirectMission.Data>
{
    public class Data
    {
        public string TargetValue { get; set; } = string.Empty;
    }

    private const string CONDITION_EXAMPLE = "EXAMPLE";

    protected override string Title => "Example mission";
    protected override string Preview => "Short text shown on the mission board.";
    protected override string Details => "Longer text shown in the board preview.";
    protected override string Mail => "Contract mail body. Target: $TARGET.";
    protected override DirectMissionTarget DirectMissionBoard => DirectMissionTarget.Hackshop;

    protected override Dictionary<string, string>? Conditions => new Dictionary<string, string>
    {
        [CONDITION_EXAMPLE] = "Complete the objective for $TARGET."
    };

    protected override PreparedMission? PrepareMission(
        DirectMission mission,
        string language,
        PlayerMissions playerMissions)
    {
        var random = RandomUtils.CreateRandom();
        var router = SpawnMissionRouter(mission, random);
        var computer = MissionUtils.GetRandomMissionComputer(router);

        var text = BuildMissionText(
            mission,
            language,
            ("$TARGET", router.GetPublicIP()!.BoldString()));

        return new PreparedMission
        {
            TargetComputer = computer,
            KarmaType = KarmaSystem.KarmaType.GREY,
            MissionData = new Data
            {
                TargetValue = computer.GetID()
            },
            Text = text
        };
    }

    protected override bool ValidateMission(
        ActiveMission mission,
        string message,
        FileSystem.Archivo attachment)
    {
        var data = GetData(mission);
        var computer = ServerMap.Singleton.GetRemoteComputer(mission.targetComputerID);
        return computer != null && data.TargetValue == computer.GetID();
    }
}
```

### Direct Mission Lifecycle

`PrepareMission` runs when the player accepts a board mission.

Use it to:

- Spawn or select target infrastructure.
- Choose users, accounts, files, services, or mailboxes.
- Build player-facing contract text with `BuildMissionText`.
- Return `PreparedMission` with `TargetComputer`, `KarmaType`, persisted `MissionData`, and `Text`.

Return `null` if the mission cannot be prepared.

`ValidateMission` runs when the player submits mission completion.

Use it to:

- Load persisted state with `GetData(mission)`.
- Find the target with `ServerMap.Singleton.GetRemoteComputer(mission.targetComputerID)`.
- Inspect the game state and return `true` only when the objective is complete.

### Conditions

Conditions let one mission type produce several variants. Define a tag-to-text dictionary:

```csharp
private const string CONDITION_PRECISE = "PRECISE";
private const string CONDITION_AT_LEAST = "AT_LEAST";

protected override Dictionary<string, string>? Conditions => new Dictionary<string, string>
{
    [CONDITION_PRECISE] = "Transfer exactly $$AMOUNT.",
    [CONDITION_AT_LEAST] = "Transfer at least $$AMOUNT."
};
```

Check the selected condition inside `PrepareMission`:

```csharp
var conditionId = mission.condition.condition.ToExtendedConditionId();
var precise = conditionId == GetConditionId(CONDITION_PRECISE);
```

See `SendMoneyMission` for a complete condition-based validation flow.

### Contract Text

`Title`, `Preview`, `Details`, `Mail`, and `Conditions` can contain tokens like `$PUBLIC_IP`, `$LOCAL_IP`, or `$USERNAME`.

Use `BuildMissionText` to combine the base mail text and selected condition text, then replace tokens:

```csharp
var text = BuildMissionText(
    mission,
    language,
    ("$PUBLIC_IP", router.GetPublicIP()!.BoldString()),
    ("$LOCAL_IP", computer.GetLocalIP()!.BoldString()),
    ("$USERNAME", user.nombreUsuario.BoldString()));
```

The examples use `BoldString()` from `StringExtensions` an ingame class to emphasize important values in emails.

### Persisted Mission Data

Put only the data needed for validation in `TData`. It must be JSON-serializable.

Examples:

- `ChangePasswordMission.Data` stores the target user, original password, and optional required password.
- `UploadFileMission.Data` stores the target path and required file content.
- `SendMoneyMission.Data` stores source and destination bank accounts, amount, precision mode, and last transaction date.
- `CompromisedMailMission.Data` stores sender mail, recipient identity, subject, body, and optional attachment name.

Avoid storing live game objects in `TData`. Store ids, names, paths, and other stable values instead.

### Attachments

Override `GetMailAttachment` when the accepted mission should include a file.

```csharp
public override FileSystem.Archivo? GetMailAttachment(ActiveMission mission)
{
    return GenerationUtils.CreateMailAttachment("payload.bin", "content", true);
}
```

For HTML replacement missions, the Website Defacement examples create a `website.html` attachment and validate that `/Public/htdocs/website.html` exactly matches the expected HTML.

### Soft-Locked Missions

Override `IsSoftLocked` when a mission can become impossible to finish. When validation fails and `IsSoftLocked` returns `true`, the player gets the failed mission text.

`SendMoneyMission` uses this when the source bank account no longer has enough money to complete the required transfer.

## World Missions

World missions are generated into the world mission system instead of being accepted from a direct board.

Use:

- `ExtendedKarmaMission<TMission>` for BlackHat, WhiteHat, or GreyHat world missions. Which you receive randomly while playing the game and get karma related to the target mission.
- `ExtendedHiddenMission<TMission>` for hidden world missions (these are found randomly in the game world).
- `ExtendedWorldMission<TMission>` directly only when you need lower-level control.

`CustomMissions/TestSuite/Public/TestHiddenMission.cs` is the current simple world mission example.

### Hidden Mission Example

```csharp
using ExtendedMissions.Missions;

public class ExampleHiddenMission : ExtendedHiddenMission<ExampleHiddenMission>
{
    protected override int RewardTier => 0;

    public ExampleHiddenMission()
    {
        var npc = AddGeneratedNpc(isGuilty: true);
        var computer = AddComputerMission(cloneNpcIndex: npc);
        AddTextFileMissionItem(computer, "Hidden evidence text", isEvidence: true);
    }
}
```

Useful world mission helpers:

- `AddGeneratedNpc` creates an NPC definition and returns its index.
- `AddComputerMission` creates a generated computer and returns its index.
- `AddTextFileMissionItem` adds a text file.
- `AddChatLogMissionItem` adds a chat log.
- `AddMailMissionItem` adds a mail conversation.
- `AddFirewallMissionItem` adds a firewall rule.
- `AddSystemLogMissionItem` adds a system log.
- `AddServiceMissionItem` adds a service.

## Procedural Missions

Procedural missions inherit from:

```csharp
ExtendedProceduralMission<TMission>
```

The current example is `CustomMissions/TestSuite/Public/TestProceduralMission.cs`.

### Minimal Procedural Mission Shape

```csharp
using System.Collections.Generic;
using ExtendedMissions.Missions;
using MissionConfig;

public class ExampleProceduralMission : ExtendedProceduralMission<ExampleProceduralMission>
{
    protected override string ArcheTypeText => "Example procedural mission";
    protected override string ArcheTypeDetailText => "A short description of the generated mission.";

    public ExampleProceduralMission()
    {
        AddGroup("main");

        AddGroupStarter(
            "main",
            "Find $IP_REMOTE and recover $NEXT_NPC_PASSWORD.",
            new List<EvidenceFunction>
            {
                new EvidenceFunction { type = EvidenceFunctionType.NextNodeRemote },
                new EvidenceFunction { type = EvidenceFunctionType.NextNpcPassword }
            });

        AddFileHeartBeatEntry(
            Verb.Recon,
            "Internal note: $NPC_NAME_GLOBAL reused a weak password.",
            new List<EvidenceFunction>
            {
                new EvidenceFunction { type = EvidenceFunctionType.NpcNameGlobal }
            });

        AddClosureEntry(
            "main",
            ClosureEntryType.File,
            "Closure evidence for $NPC_NAME_GLOBAL.",
            new List<EvidenceFunction>
            {
                new EvidenceFunction { type = EvidenceFunctionType.NpcNameGlobal }
            });
    }
}
```

Procedural missions must have:

- At least one group.
- At least one starter for every group.
- Heart and closure entries that produce enough evidence for the generated mission to be solvable.

`ProceduralMissionConfig` validates that every starter belongs to a known group and every group has a starter.

## Registration and Discovery

Mission classes are discovered from loaded assemblies. A mission class should:

- Be non-abstract.
- Inherit one of the Extended Missions base classes.
- Have a public or accessible parameterless constructor.
- Use the generic singleton pattern required by the base type, for example `ExtendedDirectMission<MyMission, MyMission.Data>`.

You do not call a manual registration method from your plugin.

## Common Helpers

`RandomUtils.CreateRandom()` creates a seeded `Random`.

`MissionUtils.GetRandomMissionComputer(router, includeWithoutServices)` selects a random computer from a router.

`MissionUtils.GetRandomUser(computer, random, includeRoot)` selects a random user.

`GenerationUtils.CreateMailAttachment(name, content, isBinary)` creates a generic mission attachment.

`TypeHelpers` converts between vanilla enum values and raw extended ids:

```csharp
var conditionId = mission.condition.condition.ToExtendedConditionId();
var missionType = MissionTypeId.ToBaseType();
```

## Example Patterns To Copy

Use `ChangePasswordMission` when your objective mutates a user account and validation compares before/after state.

Use `UploadFileMission` when your objective requires a file at a generated path.

Use `SendMoneyMission` when your objective depends on database state and transactions after mission acceptance.

Use `CompromisedMailMission` when your objective requires player-sent mail and optional attachments.

Use the Website Defacement missions when your objective targets a specific service and validates file contents on a web server.

Use `TestHiddenMission` for the smallest world mission pattern.

Use `TestProceduralMission` for the smallest procedural mission pattern.

Do not use `DoxMission` as a reference until it is updated.

## Build and Test

Build your mission pack project:

```powershell
dotnet build CustomMissions\YourMissionPack\YourMissionPack.csproj
```

Then load the plugin with Extended Missions installed. Verify:

- The plugin assembly loads with no BepInEx errors.
- The mission appears on the expected board or generation path.
- The contract text has no unreplaced tokens.
- The target exists and is reachable.
- Completion fails before the player performs the objective.
- Completion succeeds only after the expected objective is complete.
- Saved and loaded active missions still validate correctly.
