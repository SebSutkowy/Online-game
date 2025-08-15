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

    private int _clientId;
    private PlayerManager _playerManager;

    private string _message = "N/A";

    private float _timer;
    private int _currentTick = 0;

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

        _playerManager = new PlayerManager();

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
        writer.Put($"3 {_clientId} {inputPayload.Tick} {inputPayload.Input.X} {inputPayload.Input.Y}");
        _server.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void DecodeMessage(string message)
    {
        int playerId,
            tick;
        float X,
              Y;
        string[] code = message.Split(' ');
        if (code.Length == 0)
            return;
        string opcode = code[0];
        playerId = int.Parse(code[1]);
        switch (opcode)
        {
            case "0": // On Join: 0 {id} {tick}
                _clientId = playerId;
                _currentTick = int.Parse(code[2]);
                break;
            case "1": // On Leave: 1 {id}
                _playerManager.Remove(playerId);
                break;
            case "2": // Player state payload: 2 {id} {tick} {posX} {posY}
                tick = int.Parse(code[2]);
                X = float.Parse(code[3]);
                Y = float.Parse(code[4]);
                StatePayload state = CreateStatePayload(tick, X, Y);
                _playerManager.UpdatePlayer(playerId, state);
                break;
            case "3": // Player Input payload: 3 {id} {tick} {dirX} {dirY}
                tick = int.Parse(code[2]);
                X = float.Parse(code[3]);
                Y = float.Parse(code[4]);
                CreateInputPayload(tick, X, Y);
                break;
        }        
    }

    private StatePayload CreateStatePayload(int tick, float posX, float posY) => new StatePayload
    {
        Tick = tick,
        Position = new Vector2(posX, posY)
    };

    private InputPayload CreateInputPayload(int tick, float dirX, float dirY) => new InputPayload
    {
        Tick = tick,
        Input = new Vector2(dirX, dirY)
    };

    private void CheckServerConnection()
    {
        switch (_server.ConnectionState)
        {
            case ConnectionState.Outgoing:
                _message = "Connecting to server...";
                break;
            case ConnectionState.Disconnected:
                _message = "Failed to connect to server";
                if (InputManager.ReceivedInput(Input.RefreshServer))
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

        _timer += (float) gameTime.ElapsedGameTime.TotalSeconds;
        while(_timer >= minTimeBetweenTicks)
        {
            _timer -= minTimeBetweenTicks;
            HandleTick();
            _currentTick++;
        }

        base.Update(gameTime);
    }

    public void HandleTick()
    {
        int bufferIndex = _currentTick % BUFFER_SIZE;
        InputPayload input = new InputPayload
        {
            Tick = _currentTick,
            Input = InputManager.InputDirection
        };
        if (input.Input != Vector2.Zero)
            SendInputPayload(input);
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
