using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Parsing
{
    public static class JsonParser
    {
        public static IEnumerable<string> GetValuesByKey(string json, string path)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            if (path == null)
                throw new ArgumentNullException(nameof(path));

            JContainer container;

            if (json.StartsWith('{'))
                container = JObject.Parse(json);

            else if (json.StartsWith('['))
                container = JArray.Parse(json);

            else
                throw new ArgumentException("The provided json is not a valid object or array");

            return container.SelectTokens(path, false)
                    .Select(token => ConvertToken(token));
        }

        private static string ConvertToken(JToken token)
        {
            if (token.Type == JTokenType.Float)
            {
                return token.ToObject<double>().ToString(CultureInfo.InvariantCulture);
            }

            return token.ToString();
        }
    }
}