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
    private NetPeer? _server;

    private string _message = "N/A";

    private float timer;
    private int currentTick = 0;

    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 30.0f;
    private const int BUFFER_SIZE = 1024;

    private const string ip = "localhost";
    private const int port = 9050;
    private const string key = "gameKey";

    private SpriteFont _font;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    private NetPeer ConnectToServer(NetManager client, string ip, int port, string key) => client.Connect(ip, port, key); 

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

        
        _listener = new EventBasedNetListener();
        _client = new NetManager(_listener);
        _client.Start();
        _server = ConnectToServer(_client, ip, port, key); 

        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {
            int maxMessageLength = 100; // In characters
            _message = dataReader.GetString(maxMessageLength); // gets the message from the server
            DecodeMessage(_message);
            dataReader.Recycle();
        };

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
    
    private void SendInputPayload(InputPayload inputPayload)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put($"2 {inputPayload.Tick} {inputPayload.Input.X} {inputPayload.Input.Y}");
        _server.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void DecodeMessage(string message)
    {
        
    }

    private void CheckServerConnection()
    {
        switch (_server.ConnectionState)
        {
            case ConnectionState.Outgoing:
                _message = "Connecting to server...";
                break;
            case ConnectionState.Disconnected:
                _message = "Failed to connect to server";
                if (InputManager.IsInputPresent(Input.RefreshServer))
                    _server = ConnectToServer(_client, ip, port, key);
                break;
            default:
                _message = "Connected to server";
                break;
        }
    }

    protected override void Update(GameTime gameTime)
    {
        // TODO: Add your update logic here
        InputManager.Update();

        _client.PollEvents();
        CheckServerConnection();

        timer += (float) gameTime.ElapsedGameTime.TotalSeconds;
        while(timer >= minTimeBetweenTicks)
        {
            timer -= minTimeBetweenTicks;
            HandleTick();
            currentTick++;
        }

        base.Update(gameTime);
    }

    public void HandleTick()
    {
        int bufferIndex = currentTick % BUFFER_SIZE;
        InputPayload input = new InputPayload
        {
            Tick = currentTick,
            Input = InputManager.InputDirection
        };

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
