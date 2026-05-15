# TrueCraft

A completely [clean-room](https://en.wikipedia.org/wiki/Clean_room_design)
implementation of Minecraft beta 1.7.3 (circa September 2011). No decompiled
code has been used in the development of this software. This is an
**implementation** — not a clone. TrueCraft is compatible with Minecraft beta
1.7.3 clients and servers.

> I miss the old days of Minecraft, when it was a simple game. It was nearly
> perfect. Most of what Mojang has added since beta 1.7.3 is fluff, life
> support for a game that was "done" years ago. This is my attempt to get back
> to the original spirit of Minecraft, before there were things like the End,
> or all-in-one redstone devices, or village gift shops. A simple sandbox where
> you can build and explore and fight with your friends. I miss that.
>
> The goal of this project is effectively to fork Minecraft. Your contribution
> is welcome, but keep in mind that changes will be evaluated against that
> vision. If you like the new Minecraft, please feel free to keep playing it.
> If you miss the old Minecraft, join us.

— *Drew DeVault, original author*

## Repository layout

| Project              | Role                                                                |
|----------------------|---------------------------------------------------------------------|
| `TrueCraft`          | The dedicated server.                                               |
| `TrueCraft.API`      | Public interfaces and value types shared by every other project.    |
| `TrueCraft.Core`     | Block / item / entity logic, terrain generation, world I/O.         |
| `TrueCraft.Client`   | MonoGame-based game client (rendering, input, audio, HUD).          |
| `TrueCraft.Launcher` | MonoGame + [GeonBit.UI](https://github.com/RonenNess/GeonBit.UI) login / world-select shell that spawns the client. |
| `TrueCraft.Nbt`      | Spec-compliant Java NBT library — see [its README](TrueCraft.Nbt/README.md). |
| `Test.*`             | xUnit v3 test suites (`Test.TrueCraft.API`, `.Core`, `.Nbt`, `.Client`). |

## Building

Requires the [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).
The whole solution is SDK-style; everything else is restored from NuGet.

```bash
git clone https://github.com/andreabenetton/TrueCraft.git
cd TrueCraft
dotnet build TrueCraft.sln
```

Run the dedicated server:

```bash
dotnet run --project TrueCraft
```

Run the launcher (which spawns the client when you connect to a world or server):

```bash
dotnet run --project TrueCraft.Launcher
```

Run a specific server directly without the launcher:

```bash
dotnet run --project TrueCraft.Client -- 127.0.0.1:25565 PlayerName -
```

(The three positional args are `<host:port> <username> <sessionId>`. Use `-`
for an offline session token.)

## Testing

```bash
dotnet test TrueCraft.sln
```

xUnit v3 (3.2.2). At the time of writing the suite reports **378 tests** across
four projects (TrueCraft.API, .Core, .Client, .Nbt).

## Contributing

Whether you've ever read the Minecraft source code matters: if you *haven't*,
you are a **clean dev** and may contribute to this repository directly. If you
*have*, you are a **dirty dev** — please limit yourself to surrounding work
(community, website, art, reverse-engineering notes) and never share
decompiled code with a clean dev.

Pull requests are welcome. Prefer small, scoped commits with a clear message;
do not bundle unrelated fixes (see [CLAUDE.md](CLAUDE.md) for the local commit
discipline this repository follows).

## Assets

TrueCraft is compatible with Minecraft beta 1.7.3 texture packs. The default
pack is the Pixeludi Pack by Wojtek Mroczek. The launcher can also download
the official Mojang assets if you accept their asset guidelines.

## Disclaimer

TrueCraft is not associated with Mojang or Minecraft in any official capacity.
