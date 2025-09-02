using Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Client
{
    public class TilemapData
    {
        public Dictionary<Point, Tile> Tilemap { get; set; }
        public Dictionary<Point, Tile> InteractiveTilemap { get; set; }
        public TilemapTags Tags { get; set; }
    }

    public class PointConverter : JsonConverter<Point>
    {
        public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected StartObject Token");

            int x = 0;
            int y = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new Point(x, y);

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();

                    reader.Read();

                    switch (propertyName)
                    {
                        case "X":
                            x = reader.GetInt32();
                            break;
                        case "Y":
                            y = reader.GetInt32();
                            break;
                    }
                }
            }

            throw new JsonException("Invalid Point");
        }

        public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteEndObject();
        }
    }

    public class TileConverter : JsonConverter<Tile>
    {
        public override Tile Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            TileType type = TileType.Floor;
            Point position = new Point();
            TileTags tags = TileTags.None;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return new Tile()
                    {
                        Position = position,
                        Type = type,
                        Tags = tags
                    };

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "Position":
                            position = JsonSerializer.Deserialize<Point>(ref reader, options);
                            break;
                        case "Type":
                            type = JsonSerializer.Deserialize<TileType>(ref reader, options);
                            break;
                        case "Tags":
                            tags = (TileTags)JsonSerializer.Deserialize<int>(ref reader, options);
                            break;

                    }
                }
            }

            throw new Exception("Invalid JSON for Tile");
        }

        public override void Write(Utf8JsonWriter writer, Tile value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Position");
            JsonSerializer.Serialize(writer, value.Position, options);
            writer.WritePropertyName("Type");
            JsonSerializer.Serialize(writer, value.Type, options);
            writer.WritePropertyName("Tags");
            JsonSerializer.Serialize(writer, value.Tags, options);
            writer.WriteEndObject();
        }
    }

    public class TilemapConverter : JsonConverter<TilemapData>
    {
        public override TilemapData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            TilemapData tilemapData = new TilemapData();

            using (var doc = JsonDocument.ParseValue(ref reader))
            {
                if (doc.RootElement.TryGetProperty("Tilemap", out JsonElement tilemapElement))
                    tilemapData.Tilemap = DeserializeMap(tilemapElement, options);

                if (doc.RootElement.TryGetProperty("InteractiveTilemap", out JsonElement interactiveTilemapElement))
                    tilemapData.InteractiveTilemap = DeserializeMap(interactiveTilemapElement, options);
                if (doc.RootElement.TryGetProperty("Tags", out JsonElement tagsElement))
                    tilemapData.Tags = (TilemapTags)JsonSerializer.Deserialize<int>(tagsElement, options);
            }

            return tilemapData;
        }

        public Dictionary<Point, Tile> DeserializeMap(JsonElement element, JsonSerializerOptions options)
        {
            Dictionary<Point, Tile> dict = new Dictionary<Point, Tile>();

            foreach (var item in element.EnumerateArray())
            {
                Point key = JsonSerializer.Deserialize<Point>(item.GetProperty("Key").GetRawText(), options);
                Tile value = JsonSerializer.Deserialize<Tile>(item.GetProperty("Value").GetRawText(), options);
                dict[key] = value;
            }
            return dict;

        }

        public override void Write(Utf8JsonWriter writer, TilemapData value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Tilemap");
            SerializeMap(writer, value.Tilemap, options);
            writer.WritePropertyName("InteractiveTilemap");
            SerializeMap(writer, value.InteractiveTilemap, options);
            writer.WritePropertyName("Tags");
            JsonSerializer.Serialize(writer, (int)value.Tags, options);
            writer.WriteEndObject();
        }

        public void SerializeMap(Utf8JsonWriter writer, Dictionary<Point, Tile> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var pair in value)
            {
                writer.WriteStartObject();

                writer.WritePropertyName("Key");
                JsonSerializer.Serialize(writer, pair.Key, options);

                writer.WritePropertyName("Value");
                JsonSerializer.Serialize(writer, pair.Value, options);

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }
    }
}
