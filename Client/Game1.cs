using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LiteNetLib;
using LiteNetLib.Utils;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Client;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private EventBasedNetListener _listener;
    private NetManager _client;
    private NetPeer _server;

    private int clientNum;


    private string _message;

    private SpriteFont _font;

    private Player _player;
    private Dictionary<int, Player> _playerList = new Dictionary<int, Player>(); // the other players

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here
        /* --- Screen --- */
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferHeight = 1000;
        _graphics.PreferredBackBufferWidth = 1000;
        _graphics.ApplyChanges();


        /* --- Server --- */
        _listener = new EventBasedNetListener();
        _client = new NetManager(_listener);
        _client.Start();
        _server = _client.Connect("localhost" /* ip */, 9050 /* port */, "gameKey" /* key */);
        /* Localhost / 127.0.0.1 - this pc
         * Private Ipv4 - LAN
         * Public IP - across networks (ensure port forwarding is on for port 9050)
         */

        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {
            _message = dataReader.GetString(100 /* max length */ ); // gets the message from the server
            DecodeServerMessage(_message);
            dataReader.Recycle();
        };


        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here

        _font = Content.Load<SpriteFont>("Text"); // for displaying text **temporary**
        Globals.PixelTexture = Content.Load<Texture2D>("pixel");

        /* --- player --- */
        int _playerSizeX = 50;
        int _playerSizeY = 50;
        int _playerFramesToMove = 10;
        _player = new Player(Vector2.Zero, _playerSizeX, _playerSizeY, _playerFramesToMove);
    }

    private void SendMessage(string message, NetPeer peer=null)
    {
        // message to server
        if ((_server == null && peer == null) || message == null)
            return;

        NetDataWriter writer = new NetDataWriter();
        writer.Put(message);
        if (peer != null)
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        else if (_server != null)
            _server.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private void DecodeServerMessage(string serverMessage)
    {
        Debug.WriteLine(serverMessage);
        string[] message = serverMessage.Split(' ');
        switch(message[0])
        {
            case "0": // 0 <id>
                clientNum = int.Parse(message[1]);
                break;
            case "1": // 1 <id> <posX> <posY> <velX> <velY>
                if (_playerList.ContainsKey(int.Parse(message[1])))
                    _playerList[int.Parse(message[1])].ReadString(serverMessage);
                else
                    _playerList.Add(int.Parse(message[1]), new Player(Vector2.Zero, 50, 50, serverMessage, 5));
                    break;
            case "2": // 2 <id>
                if (_playerList.ContainsKey(int.Parse(message[1])))
                    _playerList.Remove(int.Parse(message[1]));

                break;
        }
    }
    
    protected override void Update(GameTime gameTime)
    {
        // TODO: Add your update logic here

        _client.PollEvents();

        // removes itself from the list of players
        _playerList.Remove(clientNum);

        InputManager.GetInput();
        _player.Move();
        foreach(var (num, player) in _playerList)
        {
            Debug.WriteLine($"{num} {player.GetString()}");
            player.Move();
        }
        Debug.WriteLine(_player.GetString());

        SendMessage($"1 {clientNum} {_player.GetString()}");

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here
        _spriteBatch.Begin();

        int _textPositionX = 950, _textPositionY = 50;
        _spriteBatch.DrawString(_font, $"{_playerList.Count+1}", new Vector2(_textPositionX, _textPositionY), Color.White);
        _player.Draw(_spriteBatch);

        foreach (var (num, player) in _playerList)
            player.Draw(_spriteBatch);

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
