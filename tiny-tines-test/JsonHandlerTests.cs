using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System.Text.Json;
using tiny_tines;
using Assert = NUnit.Framework.Assert;

namespace tiny_tines_test
{
    [TestClass]
    [TestOf(typeof(JsonHandler))]
    public class JsonHandlerTests
    {
        private JsonHandler jsonHandler = new JsonHandler();

        [TestMethod]
        public void ElementToString_Returns_String()
        {
            string jsonString = @"{""key"":""value""}";

            JsonDocument document = JsonDocument.Parse(jsonString);
            JsonElement element = document.RootElement;

            var string_element = element.GetProperty("key");
            var expected = "value";

            var actual = jsonHandler.ElementToString(string_element);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ElementToString_Returns_Empty()
        {
            //Arrange
            string jsonString = "{\"key\":null}";
            JsonElement element = JsonSerializer.Deserialize<JsonElement>(jsonString);
            var null_element = element.GetProperty("key");

            //Act
            var output = jsonHandler.ElementToString(null_element);

            //Assert
            Assert.IsEmpty(output);
        }

        [TestMethod]
        public void ElementToString_Returns_Json_String()
        {
            //Arrange
            string jsonString = @"{""key"":{""keynested1"":""value1"",""keynested2"":""value2""},""key2"":{""keynestedA"":""valueA"",""keynestedB"":""valueB""}}
            ";
            JsonElement element = JsonSerializer.Deserialize<JsonElement>(jsonString);
            string expected = JsonSerializer.Serialize(element);

            //Act
            var actual = jsonHandler.ElementToString(element);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetObjectValue_Returns_Correct_Object()
        {
            //Arrange
            string jsonString = @"{""key"":{""keynested1"":""value1"",""keynested2"":""value2""},""key2"":{""keynestedA"":""valueA"",""keynestedB"":""valueB""}}
            ";
            JsonDocument document = JsonDocument.Parse(jsonString);
            JsonElement element = document.RootElement;

            string jsonStringExpected = @"{""keynestedA"":""valueA"",""keynestedB"":""valueB""}";
            JsonDocument documenteExpected = JsonDocument.Parse(jsonStringExpected);
            JsonElement expected = documenteExpected.RootElement;

            //Act
            var actual = jsonHandler.GetObjectValue(element, "key2");

            //Assert
            Assert.AreEqual(expected.ToString(), actual.ToString());
        }

        [TestMethod]
        public void GetObjectValue_Returns_Null()
        {
            //Arrange
            string jsonString = @"{""key"":{""keynested1"":""value1"",""keynested2"":""value2""},""key2"":{""keynestedA"":""valueA"",""keynestedB"":""valueB""}}";
            JsonDocument document = JsonDocument.Parse(jsonString);
            JsonElement element = document.RootElement;

            //Act
            var actual = jsonHandler.GetObjectValue(element, "non-existent key");

            //Assert
            Assert.IsNull(actual);
        }

        [DataRow(JsonValueKind.String, "string")]
        [DataRow(JsonValueKind.Number, "number")]
        [DataRow(JsonValueKind.Array, "array")]
        [DataRow(JsonValueKind.Object, "object")]
        [DataRow(JsonValueKind.Null, "null")]
        [DataRow(JsonValueKind.Null, "default-invalid")]
        [DataTestMethod]
        public void TranslateToJsonKind_Returns_Correct_Kind(JsonValueKind expected, string actualType)
        {
            //Act
            var actual = jsonHandler.TranslateToJsonValueKind(actualType);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Serialize_Correctly_To_String()
        {
            //Arrange
            string jsonString = @"{""key"":{""keynested1"":""value1"",""keynested2"":""value2""},""key2"":{""keynestedA"":""valueA"",""keynestedB"":""valueB""}}";
            JsonDocument document = JsonDocument.Parse(jsonString);
            JsonElement element = document.RootElement;

            string expected = JsonSerializer.Serialize(element);

            //Act
            string actual = jsonHandler.Serialize(element);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ElementToArray_Returns_Array()
        {
            //Arrange
            string jsonString = @"[""value0"",""value1"",""value2"",""value3""]";
            JsonDocument document = JsonDocument.Parse(jsonString);
            JsonElement element = document.RootElement;

            var expected = new JsonElement.ArrayEnumerator();

            //Act
            var actual = jsonHandler.ElementToArray(element);

            //Assert
            Assert.AreEqual(expected.GetType(), actual.GetType());
        }

        [TestMethod]
        public void ElementToArray_Returns_Empty()
        {
            //Arrange
            string jsonString = @"{""key"":{""keynested1"":""value1"",""keynested2"":""value2""},""key2"":{""keynestedA"":""valueA"",""keynestedB"":""valueB""}}";
            JsonDocument document = JsonDocument.Parse(jsonString);
            JsonElement element = document.RootElement;

            //Act
            var output = jsonHandler.ElementToArray(element);

            //Assert
            Assert.IsEmpty(output);
        }
    }
}