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
        _screenDimensions = new Point(768, 512);
        _graphics.PreferredBackBufferWidth = _screenDimensions.X;
        _graphics.PreferredBackBufferHeight = _screenDimensions.Y;
        _graphics.IsFullScreen = false;
        _graphics.ApplyChanges();


        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here

        _font = Content.Load<SpriteFont>("Text");
        serverTexture = new Texture2D(GraphicsDevice, 1, 1);
        serverTexture.SetData(new[] { new Color(50, 230, 50, 128) });

        clientTexture = new Texture2D(GraphicsDevice, 1, 1);
        clientTexture.SetData(new[] { new Color(230, 32, 32, 128)});

        Server.SetPlayerTexture(serverTexture);
        Client.SetPlayerTexture(clientTexture);

        Console.Initialize(_font, _screenDimensions);
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
            displayConsole = true;
            Debug.WriteLine("Switched to Server");
        }
        else if (InputManager.ReceivedPressedInput(Input.SwitchToClient))
        {
            NetworkMode = Mode.Client;
            Debug.WriteLine("Switched to Client");
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

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        // TODO: Add your drawing code here
        _spriteBatch.Begin();

        
        switch(NetworkMode)
        {
            case Mode.Client:
                Client.PlayerManager.DrawPlayers(_spriteBatch);
                _spriteBatch.DrawString(_font, Client.GetMostRecentMessage(), new Vector2(50, 50), Color.White);
                if (InputManager.ReceivedPressedInput(Input.DisplayConsole))
                    displayConsole = !displayConsole;
                break;
        }

        if (displayConsole)
            Console.DisplayConsole(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
