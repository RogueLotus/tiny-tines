using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace tiny_tines
{
    /// <summary>
    /// A class which encapsulates all behaviour needed to handle JSON queries.
    /// </summary>
    public class JsonHandler
    {
        /// <summary>
        /// Updates JsonElement to have a root level key. With current element becoming the valu
        /// </summary>
        /// <param name="element">A <see cref="JsonElement"/> object that represents JSON data. </param>
        /// <param name="key">A <see cref="String"/> object that represents the to be key.</param>
        /// <returns>A <see cref="JsonElement"/> object.</returns>
        public JsonElement AddRootKey(JsonElement element, string key)
        {
            var dict = new Dictionary<string, JsonElement>() { { key, element } };
            var dictAsString = Serialize(dict);

            return DeserializeToElement(dictAsString);
        }

        public JsonElement CombineElements(JsonElement firstElement, JsonElement secondElement)

        {
            var firstAsString = Serialize(firstElement);
            var secondAsString = Serialize(secondElement);

            var firstAsDict = DeserializeToDictionary(firstAsString);
            var secondAsDict = DeserializeToDictionary(secondAsString);

            secondAsDict.ToList().ForEach(x => firstAsDict.Add(x.Key, x.Value));

            var combinedString = Serialize(firstAsDict);
            return DeserializeToElement(combinedString);
        }

        public JsonElement.ArrayEnumerator ElementToArray(JsonElement element)
        {
            var array = new JsonElement.ArrayEnumerator();

            if (CompareElementType(element, "array"))
            {
                array = element.EnumerateArray();
            }

            return array;
        }

        public JsonElement DeserializeToElement(string jsonString)
        {
            return JsonSerializer.Deserialize<JsonElement>(jsonString);
        }

        public Dictionary<string, JsonElement> DeserializeToDictionary(string jsonString)
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);
        }

        public string Serialize(JsonElement jsonElement)
        {
            return JsonSerializer.Serialize(jsonElement);
        }

        public string Serialize(Dictionary<string, JsonElement> dict)
        {
            return JsonSerializer.Serialize(dict);
        }

        /// <summary>
        /// Translates a string value, denoting a data type, to a JsonValueKind.
        /// </summary>
        /// <param name="valueType">A <see cref="String"/> object which represents a data type.</param>
        /// <returns>A <see cref="JsonValueKind"/> object parallel to its written data type; otherwise, <see cref="JsonValueKind.Null"/>.</returns>
        public JsonValueKind TranslateToJsonValueKind(string valueType)
        {
            return valueType switch
            {
                "string" => JsonValueKind.String,
                "number" => JsonValueKind.Number,
                "array" => JsonValueKind.Array,
                "object" => JsonValueKind.Object,
                _ => JsonValueKind.Null,
            };
        }

        /// <summary>
        /// Compares a JsonElement's type to a written value type.
        /// </summary>
        /// <param name="element">A <see cref="JsonElement"/> object that represents JSON data.</param>
        /// <param name="expectedType">A <see cref="String"/> object that represents a data type.</param>
        /// <returns> A <see cref="Boolean"/> <c>true</c> if matching types; otherwise, <c>false</c>.</returns>
        public bool CompareElementType(JsonElement element, string expectedType)
        {
            JsonValueKind expectedValueType = TranslateToJsonValueKind(expectedType);
            bool areOfSameType = element.ValueKind == expectedValueType;
            return areOfSameType;
        }

        /// <summary>
        /// Gets a nested value, using a collection of keys, from a JsonElement.
        /// </summary>
        /// <param name="element">A <see cref="JsonElement"/> object representing JSON data.</param>
        /// <param name="keys">A <see cref="List{T}"/> of keys used to find a nested value.</param>
        /// <returns>A <see cref="String"/> representing found value; otherwise, <c>empty</c> string.</returns>
        public string GetNestedValue(JsonElement element, List<string> keys)
        {
            string emptyString = "";

            var currentElement = element;

            foreach (var key in keys)
            {
                JsonElement? output = GetValue(currentElement, key);
                if (output == null) return emptyString;

                bool is_null = CompareElementType((JsonElement)output, "null");
                if (is_null) return emptyString;

                JsonElement outputNotNull = (JsonElement)output;

                bool isString = CompareElementType(outputNotNull, "string");
                bool isNumber = CompareElementType(outputNotNull, "number");
                if (isString || isNumber) return ElementToString(outputNotNull);

                currentElement = outputNotNull;
            }

            return emptyString;
        }

        /// <summary>
        /// Gets a value from an <c>Array</c> or <c>Object</c> type JsonElement.
        /// </summary>
        /// <param name="element">A <see cref="JsonElement"/> object representing JSON data.</param>
        /// <param name="key">A <see cref="String"/> object that represents the key.</param>
        /// <returns>A <see cref="JsonElement"/> object representing the value; otherwise, returns <c>null</c>.</returns>
        public JsonElement? GetValue(JsonElement element, string key)
        {
            bool isArray = CompareElementType(element, "array");

            bool isObject = CompareElementType(element, "object");

            if (isArray == true)
            {
                return GetArrayValue(element, key);
            }
            else if (isObject == true)
            {
                return GetObjectValue(element, key);
            }

            return element;
        }

        /// <summary>
        /// Gets a value from an <c>Array</c> type JsonElement.
        /// </summary>
        /// <param name="element">A <see cref="JsonElement"/> object representing JSON data.</param>
        /// <param name="key">A <see cref="String"/> object that represents the key.</param>
        /// <returns>A <see cref="JsonElement"/> object representing the value; otherwise, returns <c>null</c>.</returns>
        public JsonElement? GetArrayValue(JsonElement element, string key)
        {
            var jsonElementArray = ElementToArray(element);

            foreach (var item in jsonElementArray)
            {
                bool hasValue = item.TryGetProperty(key, out var value);
                if (hasValue) return value;
            }

            return null;
        }

        /// <summary>
        /// Gets a value from an <c>Object</c> type JsonElement.
        /// </summary>
        /// <param name="element">A <see cref="JsonElement"/> object representing JSON data.</param>
        /// <param name="key">A <see cref="String"/> object that represents the key.</param>
        /// <returns>A <see cref="JsonElement"/> object representing the value; otherwise, returns <c>null</c>.</returns>
        public JsonElement? GetObjectValue(JsonElement element, string key)
        {
            bool hasProperty = element.TryGetProperty(key, out var value);

            return hasProperty ? value : null;
        }

        public string ElementToString(JsonElement element)
        {
            return element.ToString();
        }
    }
}