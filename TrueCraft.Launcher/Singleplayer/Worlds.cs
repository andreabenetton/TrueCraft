using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.TerrainGen;
using TrueCraft.Core.World;

namespace TrueCraft.Launcher.Singleplayer;

public class Worlds
{
    private readonly ILogger<Worlds> Log;
    private readonly ILoggerFactory _loggerFactory;

    public Worlds(ILogger<Worlds> log, ILoggerFactory loggerFactory)
    {
        Log = log;
        _loggerFactory = loggerFactory;
    }

    public static Worlds Local { get; set; }

    public BlockRepository BlockRepository { get; set; }
    public World[] Saves { get; set; }

    public void Load()
    {
        if (!Directory.Exists(Paths.Worlds))
            Directory.CreateDirectory(Paths.Worlds);
        BlockRepository = new BlockRepository();
        BlockRepository.DiscoverBlockProviders();
        var directories = Directory.GetDirectories(Paths.Worlds);
        var saves = new List<World>();
        foreach (var d in directories)
            try
            {
                var w = World.LoadWorld(d, _loggerFactory);
                saves.Add(w);
            }
            catch (Exception e)
            {
                Log.LogError(e, "Failed to load world from {Path}", d);
            }

        Saves = saves.ToArray();
    }

    public World CreateNewWorld(string name, string seed)
    {
        if (!int.TryParse(seed, out var s))
            // TODO: Hash seed string
            s = MathHelper.Random.Next();
        var world = new World(name, s, new StandardGenerator(), _loggerFactory);
        world.BlockRepository = BlockRepository;
        var safeName = name;
        foreach (var c in Path.GetInvalidFileNameChars())
            safeName = safeName.Replace(c.ToString(), "");
        world.Name = name;
        world.Save(Path.Combine(Paths.Worlds, safeName));
        Saves = Saves.Concat(new[] {world}).ToArray();
        return world;
    }
}
