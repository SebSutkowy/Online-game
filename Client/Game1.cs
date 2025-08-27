using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace Client;

public enum Mode : ushort
{
    None = 1,
    Server = 2,
    Client = 3,
    Hybrid = 6
}

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Texture2D serverTexture, clientTexture, cursorTexture;

    private SpriteFont _font;
    private Point _screenDimensions;

    private Mode NetworkMode = Mode.None;
    private bool displayConsole = false;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        _graphics.PreferredBackBufferWidth = Camera.WIDTH;
        _graphics.PreferredBackBufferHeight = Camera.HEIGHT;
        _graphics.IsFullScreen = false;
        IsMouseVisible = false;
        _graphics.ApplyChanges();


        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here

        Tilemap.ImportTexture(TileType.Floor, Content.Load<Texture2D>("FloorTile"));
        Tilemap.ImportTexture(TileType.Wall, Content.Load<Texture2D>("WallTile"));
        Tilemap.ImportTexture(TileType.Trap, Content.Load<Texture2D>("TrapTile"));
        Tilemap.ImportTexture(TileType.Chest, Content.Load<Texture2D>("ChestTile"));
        Tilemap.ImportTexture(TileType.ActiveTrap, Content.Load<Texture2D>("ActivatedTrapTile"));


        _font = Content.Load<SpriteFont>("Text");
        serverTexture = new Texture2D(GraphicsDevice, 1, 1);
        serverTexture.SetData(new[] { new Color(50, 230, 50, 128) });

        clientTexture = new Texture2D(GraphicsDevice, 1, 1);
        clientTexture.SetData(new[] { new Color(230, 32, 32, 128)});

        cursorTexture = Content.Load<Texture2D>("cursor");
        Server.SetPlayerTexture(serverTexture);
        Client.SetPlayerTexture(clientTexture);

        Console.Initialize(_font);
        Camera.AddFont(_font);
    }

    public void ChangeNetworkMode()
    {
        if (NetworkMode != Mode.None)
            return; // Can only change when no mode is selected
        if (InputManager.ReceivedPressedInput(Input.SwitchToHybrid))
        {
            NetworkMode = Mode.Hybrid;
            Debug.WriteLine("Switched to Hybrid");
        }
        else if (InputManager.ReceivedPressedInput(Input.SwitchToServer))
        {
            NetworkMode = Mode.Server;
            if(!Console.Display)
                Console.ToggleVisibility();
            Debug.WriteLine("Switched to Server");
            Tilemap.ImportFrom("Presets/SampleMap1.json");
        }
        else if (InputManager.ReceivedPressedInput(Input.SwitchToClient))
        {
            NetworkMode = Mode.Client;
            if (Console.Display)
                Console.ToggleVisibility();
            Debug.WriteLine("Switched to Client");
            Tilemap.ImportFrom("Presets/SampleMap1.json");
        }
    }

    protected override void Update(GameTime gameTime)
    {
        // TODO: Add your update logic here
        InputManager.Update();

        ChangeNetworkMode();
        int port = 9050;
        switch (NetworkMode)
        {
            case Mode.Server:
                if(!Server.IsRunning)
                    Server.StartServer(port);
                else
                    Server.Update(gameTime);
                break;
            case Mode.Client:
                if (!Client.IsRunning)
                    Client.Initialize();
                else
                    Client.Update(gameTime);
                break;
        }



        base.Update(gameTime);
    }

    public void DrawCursor()
    {
        Point pos = InputManager.GetMousePos();
        pos.X -= cursorTexture.Width / 2;
        pos.Y -= cursorTexture.Height / 2;
        pos = Camera.AccountForOffset(pos);
        Rectangle cursor = new Rectangle(pos.X, pos.Y, cursorTexture.Width, cursorTexture.Height);
        Camera.Draw(cursorTexture, cursor, Color.White);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code 
        Camera.ToDraw += () => Tilemap.Draw();

        
        switch(NetworkMode)
        {
            case Mode.Client:
                Camera.ToDraw += () => Client.PlayerManager.DrawPlayers();
                break;
            case Mode.Server:
                Camera.ToDraw += () => Server.PlayerManager.DrawPlayers();
                break;
        }

        if (InputManager.ReceivedPressedInput(Input.DisplayConsole))
            Console.ToggleVisibility();
        Camera.ToDraw += () => Console.DisplayConsole();
        Camera.ToDraw += () => DrawCursor();

        Camera.Display(_spriteBatch);

        base.Draw(gameTime);
    }
}
