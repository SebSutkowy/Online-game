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

    protected override void Update(GameTime gameTime)
    {
        // TODO: Add your update logic here
        InputManager.Update();

        NetworkManager.Update(gameTime);

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
        Camera.ToDraw += () =>
        {
            Tilemap.Draw();
            NetworkManager.Draw();
            DrawCursor();
        };

        Camera.Display(_spriteBatch);

        base.Draw(gameTime);
    }
}
