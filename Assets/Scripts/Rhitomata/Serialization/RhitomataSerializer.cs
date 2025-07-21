using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rhitomata {
    public static class RhitomataSerializer {
        private static readonly List<JsonConverter> converters = new()
        {
            new Vector2Converter(),
            new Vector3Converter(),
            new Vector4Converter(),
            new ColorConverter(),
            new QuaternionConverter(),
        };

        private static JsonSerializerSettings Settings => new() { Converters = converters, MissingMemberHandling = MissingMemberHandling.Ignore };
        public static string Serialize(object obj, bool pretty = false) {
            var settings = new JsonSerializerSettings(Settings) { Formatting = pretty ? Formatting.Indented : Formatting.None };
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static string Serialize(object obj, Formatting formatting) {
            var settings = new JsonSerializerSettings(Settings) { Formatting = formatting, DefaultValueHandling = DefaultValueHandling.Ignore };
            return JsonConvert.SerializeObject(obj, settings);
        }

        public static T Deserialize<T>(string json) =>
            JsonConvert.DeserializeObject<T>(json);

        private class Vector2Converter : JsonConverter<Vector2> {
            public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer) {
                var jo = new JObject
                {
                    { "x", value.x },
                    { "y", value.y },
                };
                jo.WriteTo(writer);
            }

            public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue,
                JsonSerializer serializer) {
                var jo = JObject.Load(reader);
                var x = jo["x"]?.Value<float>() ?? 0f;
                var y = jo["y"]?.Value<float>() ?? 0f;
                return new Vector2(x, y);
            }
        }

        private class Vector3Converter : JsonConverter<Vector3> {
            public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer) {
                var jo = new JObject
                {
                    { "x", value.x },
                    { "y", value.y },
                    { "z", value.z },
                };
                jo.WriteTo(writer);
            }

            public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue,
                JsonSerializer serializer) {
                var jo = JObject.Load(reader);
                var x = jo["x"]?.Value<float>() ?? 0f;
                var y = jo["y"]?.Value<float>() ?? 0f;
                var z = jo["z"]?.Value<float>() ?? 0f;
                return new Vector3(x, y, z);
            }
        }

        private class Vector4Converter : JsonConverter<Vector4> {
            public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer) {
                var jo = new JObject
                {
                    { "x", value.x },
                    { "y", value.y },
                    { "z", value.z },
                    { "w", value.w }
                };
                jo.WriteTo(writer);
            }

            public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer) {
                var jo = JObject.Load(reader);
                var x = jo["x"]?.Value<float>() ?? 0f;
                var y = jo["y"]?.Value<float>() ?? 0f;
                var z = jo["z"]?.Value<float>() ?? 0f;
                var w = jo["w"]?.Value<float>() ?? 0f;
                return new Vector4(x, y, z, w);
            }
        }

        private class QuaternionConverter : JsonConverter<Quaternion> {
            public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer) {
                var jo = new JObject
                {
                    { "x", value.x },
                    { "y", value.y },
                    { "z", value.z },
                    { "w", value.w }
                };
                jo.WriteTo(writer);
            }

            public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer) {
                var jo = JObject.Load(reader);
                var x = jo["x"]?.Value<float>() ?? 0f;
                var y = jo["y"]?.Value<float>() ?? 0f;
                var z = jo["z"]?.Value<float>() ?? 0f;
                var w = jo["w"]?.Value<float>() ?? 1f;
                return new Quaternion(x, y, z, w);
            }
        }

        private class ColorConverter : JsonConverter<Color> {
            public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer) {
                var jo = new JObject
                {
                    { "r", value.r },
                    { "g", value.g },
                    { "b", value.b },
                    { "a", value.a }
                };
                jo.WriteTo(writer);
            }

            public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer) {
                var jo = JObject.Load(reader);
                var r = jo["r"]?.Value<float>() ?? 0f;
                var g = jo["g"]?.Value<float>() ?? 0f;
                var b = jo["b"]?.Value<float>() ?? 0f;
                var a = jo["a"]?.Value<float>() ?? 1f;
                return new Color(r, g, b, a);
            }
        }
    }
}