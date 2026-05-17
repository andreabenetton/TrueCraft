using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TrueCraft.API;
using TrueCraft.API.Logic;
using TrueCraft.Client.Input;
using TrueCraft.Client.Modules;
using TrueCraft.Client.Rendering;
using TrueCraft.Core;
using TrueCraft.Core.Logic;
using TrueCraft.Core.Networking.Packets;
using TVector3 = TrueCraft.API.Vector3;

namespace TrueCraft.Client;

public class TrueCraftGame : Game
{
    public static readonly int Reach = 3;

    public TrueCraftGame(MultiplayerClient client, IPEndPoint endPoint)
    {
        Window.Title = "TrueCraft";
        Content.RootDirectory = "Content";
        Graphics = new GraphicsDeviceManager(this)
        {
            SynchronizeWithVerticalRetrace = false,
            IsFullScreen = UserSettings.Local.IsFullscreen,
            PreferredBackBufferWidth = UserSettings.Local.WindowResolution.Width,
            PreferredBackBufferHeight = UserSettings.Local.WindowResolution.Height
        };
        Graphics.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
        Graphics.ApplyChanges();

        Window.ClientSizeChanged += Window_ClientSizeChanged;
        Client = client;
        EndPoint = endPoint;
        LastPhysicsUpdate = DateTime.MinValue;
        NextPhysicsUpdate = DateTime.MinValue;
        PendingMainThreadActions = new ConcurrentBag<Action>();
        MouseCaptured = true;
        Bobbing = 0;

        KeyboardComponent = new KeyboardHandler(this);
        Components.Add(KeyboardComponent);

        MouseComponent = new MouseHandler(this);
        Components.Add(MouseComponent);

        GamePadComponent = new GamePadHandler(this);
        Components.Add(GamePadComponent);
    }

    public MultiplayerClient Client { get; }
    public GraphicsDeviceManager Graphics { get; }
    public TextureMapper TextureMapper { get; private set; }
    public Camera Camera { get; private set; }
    public ConcurrentBag<Action> PendingMainThreadActions { get; set; }
    public double Bobbing { get; set; }
    public ChunkModule ChunkModule { get; set; }
    public ChatModule ChatModule { get; set; }
    public float ScaleFactor { get; set; }
    public Coordinates3D HighlightedBlock { get; set; }
    public BlockFace HighlightedBlockFace { get; set; }
    public DateTime StartDigging { get; set; }
    public DateTime EndDigging { get; set; }
    public Coordinates3D TargetBlock { get; set; }
    public AudioManager Audio { get; set; }
    public Texture2D White1x1 { get; set; }
    public PlayerControlModule ControlModule { get; set; }
    public SkyModule SkyModule { get; set; }

    private List<IInputModule> InputModules { get; set; }
    private List<IGraphicalModule> GraphicalModules { get; set; }
    private KeyboardHandler KeyboardComponent { get; }
    private MouseHandler MouseComponent { get; }
    private GamePadHandler GamePadComponent { get; }
    private int ThreadID { get; set; }

    private FontRenderer Pixel { get; set; }
    private IPEndPoint EndPoint { get; }
    private DateTime LastPhysicsUpdate { get; }
    private DateTime NextPhysicsUpdate { get; set; }
    private bool MouseCaptured { get; }
    private GameTime GameTime { get; set; }
    private DebugInfoModule DebugInfoModule { get; set; }

    public IBlockRepository BlockRepository => Client.World.World.BlockRepository;

    public IItemRepository ItemRepository { get; set; }

    private void Graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
    {
        e.GraphicsDeviceInformation.GraphicsProfile = GraphicsProfile.HiDef;
    }

    private void Window_ClientSizeChanged(object sender, EventArgs e)
    {
        if (GraphicsDevice.Viewport.Width < 640 || GraphicsDevice.Viewport.Height < 480)
            ScaleFactor = 0.5f;
        else if (GraphicsDevice.Viewport.Width < 978 || GraphicsDevice.Viewport.Height < 720)
            ScaleFactor = 1.0f;
        else
            ScaleFactor = 1.5f;
        IconRenderer.PrepareEffects(this);
        UpdateCamera();
    }

    protected override void Initialize()
    {
        InputModules = new List<IInputModule>();
        GraphicalModules = new List<IGraphicalModule>();

        base.Initialize(); // (calls LoadContent)

        Camera = new Camera(GraphicsDevice.Viewport.AspectRatio, 70.0f, 0.1f, 1000.0f);
        UpdateCamera();

        White1x1 = new Texture2D(GraphicsDevice, 1, 1);
        White1x1.SetData(new[] {Color.White});

        Audio = new AudioManager();
        Audio.LoadDefaultPacks(Content);

        SkyModule = new SkyModule(this);
        ChunkModule = new ChunkModule(this);
        DebugInfoModule = new DebugInfoModule(this, Pixel);
        ChatModule = new ChatModule(this, Pixel);
        var hud = new HUDModule(this, Pixel);
        var windowModule = new WindowModule(this, Pixel);

        GraphicalModules.Add(SkyModule);
        GraphicalModules.Add(ChunkModule);
        GraphicalModules.Add(new HighlightModule(this));
        GraphicalModules.Add(hud);
        GraphicalModules.Add(ChatModule);
        GraphicalModules.Add(windowModule);
        GraphicalModules.Add(DebugInfoModule);

        InputModules.Add(windowModule);
        InputModules.Add(DebugInfoModule);
        InputModules.Add(ChatModule);
        InputModules.Add(ControlModule = new PlayerControlModule(this));

        Client.MainThreadInvoke = Invoke;
        Client.PropertyChanged += HandleClientPropertyChanged;
        Client.Connect(EndPoint);

        BlockProvider.BlockRepository = BlockRepository;
        var itemRepository = new ItemRepository();
        itemRepository.DiscoverItemProviders();
        ItemRepository = itemRepository;
        BlockProvider.ItemRepository = ItemRepository;

        IconRenderer.CreateBlocks(this, BlockRepository);

        var centerX = GraphicsDevice.Viewport.Width / 2;
        var centerY = GraphicsDevice.Viewport.Height / 2;
        Mouse.SetPosition(centerX, centerY);

        MouseComponent.Scroll += OnMouseComponentScroll;
        MouseComponent.Move += OnMouseComponentMove;
        MouseComponent.ButtonDown += OnMouseComponentButtonDown;
        MouseComponent.ButtonUp += OnMouseComponentButtonUp;
        KeyboardComponent.KeyDown += OnKeyboardKeyDown;
        KeyboardComponent.KeyUp += OnKeyboardKeyUp;
        GamePadComponent.ButtonDown += OnGamePadButtonDown;
        GamePadComponent.ButtonUp += OnGamePadButtonUp;

        // Window.ClientSizeChanged only fires when the OS reports a
        // resize, not on initial show. Without this seed call,
        // ScaleFactor stays at its default 0f and every HUD/inventory
        // SpriteBatch.Draw(..., scale: ScaleFactor*2, …) draws at scale
        // zero — i.e. invisible, while the dim overlay (no scale) still
        // covers the screen.
        Window_ClientSizeChanged(null, EventArgs.Empty);

        ThreadID = Thread.CurrentThread.ManagedThreadId;
    }

    public void Invoke(Action action)
    {
        if (ThreadID == Thread.CurrentThread.ManagedThreadId)
            action();
        else
            PendingMainThreadActions.Add(action);
    }

    private void HandleClientPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case "Position":
                UpdateCamera();
                break;
        }
    }

    protected override void LoadContent()
    {
        // Ensure we have default textures loaded.
        TextureMapper.LoadDefaults(GraphicsDevice);

        // Load any custom textures if needed.
        TextureMapper = ActivatorUtilities.CreateInstance<TextureMapper>(App.Services, GraphicsDevice);
        if (UserSettings.Local.SelectedTexturePack != TexturePack.Default.Name)
            TextureMapper.AddTexturePack(TexturePack.FromArchive(Path.Combine(Paths.TexturePacks,
                UserSettings.Local.SelectedTexturePack)));

        Pixel = new FontRenderer(
            new Font(GraphicsDevice, Content, "Fonts/Pixel"),
            new Font(GraphicsDevice, Content, "Fonts/Pixel", FontStyle.Bold), null, null,
            new Font(GraphicsDevice, Content, "Fonts/Pixel", FontStyle.Italic));

        base.LoadContent();
    }

    private void OnKeyboardKeyDown(object sender, KeyboardKeyEventArgs e)
    {
        foreach (var module in InputModules)
            if (module.KeyDown(GameTime, e))
                break;
    }

    private void OnKeyboardKeyUp(object sender, KeyboardKeyEventArgs e)
    {
        foreach (var module in InputModules)
            if (module.KeyUp(GameTime, e))
                break;
    }

    private void OnGamePadButtonUp(object sender, GamePadButtonEventArgs e)
    {
        foreach (var module in InputModules)
            if (module.GamePadButtonUp(GameTime, e))
                break;
    }

    private void OnGamePadButtonDown(object sender, GamePadButtonEventArgs e)
    {
        foreach (var module in InputModules)
            if (module.GamePadButtonDown(GameTime, e))
                break;
    }

    private void OnMouseComponentScroll(object sender, MouseScrollEventArgs e)
    {
        foreach (var module in InputModules)
            if (module.MouseScroll(GameTime, e))
                break;
    }

    private void OnMouseComponentButtonDown(object sender, MouseButtonEventArgs e)
    {
        foreach (var module in InputModules)
            if (module.MouseButtonDown(GameTime, e))
                break;
    }

    private void OnMouseComponentButtonUp(object sender, MouseButtonEventArgs e)
    {
        foreach (var module in InputModules)
            if (module.MouseButtonUp(GameTime, e))
                break;
    }

    private void OnMouseComponentMove(object sender, MouseMoveEventArgs e)
    {
        foreach (var module in InputModules)
            if (module.MouseMove(GameTime, e))
                break;
    }

    public void TakeScreenshot()
    {
        var path = Path.Combine(Paths.Screenshots, DateTime.Now.ToString("yyyy-MM-dd_H.mm.ss") + ".png");
        if (!Directory.Exists(Path.GetDirectoryName(path)))
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        var width = GraphicsDevice.PresentationParameters.BackBufferWidth;
        var height = GraphicsDevice.PresentationParameters.BackBufferHeight;
        var data = new Color[width * height];
        GraphicsDevice.GetBackBufferData(data);
        using var texture = new Texture2D(GraphicsDevice, width, height);
        texture.SetData(data);
        using (var stream = File.OpenWrite(path))
            texture.SaveAsPng(stream, width, height);

        ChatModule.AddMessage("Screenshot saved to " + Path.GetFileName(path));
    }

    public void FlushMainThreadActions()
    {
        while (PendingMainThreadActions.TryTake(out var action))
            action();
    }

    protected override void Update(GameTime gameTime)
    {
        GameTime = gameTime;

        FlushMainThreadActions();

        var adjusted = Client.World.World.FindBlockPosition(
            new Coordinates3D((int) Client.Position.X, 0, (int) Client.Position.Z), out var chunk);
        if (chunk is not null && Client.LoggedIn)
            if (chunk.GetHeight((byte) adjusted.X, (byte) adjusted.Z) != 0)
                Client.Physics.Update(gameTime.ElapsedGameTime);
        if (NextPhysicsUpdate < DateTime.UtcNow && Client.LoggedIn)
        {
            // NOTE: This is to make the vanilla server send us chunk packets
            // We should eventually make some means of detecing that we're on a vanilla server to enable this
            // It's a waste of bandwidth to do it on a TrueCraft server
            Client.QueuePacket(new PlayerGroundedPacket {OnGround = true});
            NextPhysicsUpdate = DateTime.UtcNow.AddMilliseconds(50);
        }

        foreach (var module in InputModules)
            module.Update(gameTime);
        foreach (var module in GraphicalModules)
            module.Update(gameTime);

        UpdateCamera();

        base.Update(gameTime);
    }

    private void UpdateCamera()
    {
        const double bobbingMultiplier = 0.05;

        var bobbing = Bobbing * 1.5;
        var xbob = Math.Cos(bobbing + Math.PI / 2) * bobbingMultiplier;
        var ybob = Math.Sin(Math.PI / 2 - 2 * bobbing) * bobbingMultiplier;

        Camera.Position = new TVector3(
            Client.Position.X + xbob, Client.Position.Y + Client.Size.Height + ybob, Client.Position.Z);

        Camera.Pitch = Client.Pitch;
        Camera.Yaw = Client.Yaw;
    }

    protected override void Draw(GameTime gameTime)
    {
        // Previously rendered into a same-sized RenderTarget2D then blit to
        // the backbuffer; the intermediate texture had no consumer so the
        // blit was pure overhead. The RT was created with
        // RenderTargetUsage.DiscardContents, which gave us a free per-frame
        // clear. Render straight to the backbuffer and clear explicitly so
        // the depth buffer doesn't carry over between frames.
        GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
            Color.Black, 1f, 0);
        GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

        GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

        Mesh.ResetStats();
        foreach (var module in GraphicalModules)
            module.Draw(gameTime);

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            KeyboardComponent.Dispose();
            MouseComponent.Dispose();
            Audio?.Dispose();
        }

        base.Dispose(disposing);
    }
}
