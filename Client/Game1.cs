using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Diagnostics;

namespace Client;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private EventBasedNetListener _listener;
    private NetManager _client;
    private NetPeer _server;

    private string _message = "N/A";

    private float timeForTick;
    private const float SERVER_TICK_RATE = 30.0f;
    

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

        string ip = "localhost";
        int port = 9050;
        string key = "gameKey";
        _listener = new EventBasedNetListener();
        _client = new NetManager(_listener);
        _client.Start();
        _server = _client.Connect(ip, port, key); 

        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {
            int maxMessageLength = 100; // In characters
            _message = dataReader.GetString(maxMessageLength); // gets the message from the server
            DecodeMessage(_message);
            dataReader.Recycle();
        };

        timeForTick = 1f / SERVER_TICK_RATE;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here

        _font = Content.Load<SpriteFont>("Text"); // for displaying text **temporary**
    }

    private void SendMessage(string message)
    {
        // message to server
        NetDataWriter writer = new NetDataWriter();
        writer.Put(message);
        _server.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void DecodeMessage(string message)
    {
        
    }

    protected override void Update(GameTime gameTime)
    {
        // TODO: Add your update logic here

        _client.PollEvents();


        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        _spriteBatch.Begin();

        _spriteBatch.DrawString(_font, _message, new Vector2(50, 50), Color.White);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
