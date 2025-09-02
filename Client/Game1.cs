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

    private Texture2D serverTexture, clientTexture;

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

        Tilemap.ImportTextures(Content);

        UI.ImportTextures(Content);


        _font = Content.Load<SpriteFont>("Text");
        serverTexture = new Texture2D(GraphicsDevice, 1, 1);
        serverTexture.SetData(new[] { new Color(50, 230, 50, 128) });

        clientTexture = new Texture2D(GraphicsDevice, 1, 1);
        clientTexture.SetData(new[] { new Color(230, 32, 32, 128)});

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
        UI.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code 
        Camera.ToDraw += () =>
        {
            Tilemap.Draw();
            NetworkManager.Draw();

            UI.Draw();
        };

        Camera.Display(_spriteBatch);

        base.Draw(gameTime);
    }
}
