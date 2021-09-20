using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using tiny_tines;
using Assert = NUnit.Framework.Assert;
using CollectionAssert = NUnit.Framework.CollectionAssert;

namespace tiny_tines_tests
{
    [TestClass]
    [TestOf(typeof(Story))]
    public class StoryTests
    {
        [TestMethod]
        public void GetSubstring_Returns_Correct_Substring()
        {
            //Arrange
            string inputstring = "This is a {{message.value}}. }}";
            string expected = "message.value";

            //Act
            var substringList = Story.GetSubstrings(inputstring);

            //Assert
            string actual = substringList[0];
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetSubstring_Returns_Correct_List_Values()
        {
            //Arrange
            string inputString = "This is a {{{message.value}}. This string {{contains}} several }} instances {{ where {{there.are.substrings.to}} be found }}." +
                "There {{are.other} things to test, such} as this.";
            List<string> expected = new List<string> { "message.value", "contains", "there.are.substrings.to" };

            //Act
            var actual = Story.GetSubstrings(inputString);

            //Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetSubstring_Returns_Empty_List()
        {
            //Arrange
            string inputString = "This string has no valid curly brace substring.";

            //Act
            var output = Story.GetSubstrings(inputString);

            //Assert
            Assert.That(output, Is.Null.Or.Empty);
        }

        [TestMethod]
        public void ReplaceSubstrings_Replaces_In_Order()
        {
            //Arrange
            List<string> inputList = new List<string>() { "", "Ireland", "9:00PM" };
            string inputString = "Sunset in {{location.city}}, {{location.country}} is at {{sunset.results.sunset}}.";
            string expected = "Sunset in , Ireland is at 9:00PM.";

            //Act
            var actual = Story.ReplaceSubstrings(inputList, inputString);

            //Assert
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void GetEventValues_Returns_Correct_Nested_Values()
        {
            //Arrange
            Story story = new Story();
            string jsonString = File.ReadAllText(@"testjson.json");
            var jsonDict = JsonSerializer.Deserialize<JsonElement>(jsonString);
            var location = new Dictionary<string, JsonElement>() { { "location", jsonDict } };

            jsonString = File.ReadAllText(@"testjson2.json");
            jsonDict = JsonSerializer.Deserialize<JsonElement>(jsonString);
            var sunset = new Dictionary<string, JsonElement>() { { "sunset", jsonDict } };

            sunset.ToList().ForEach(x => location.Add(x.Key, x.Value));

            var combinedString = JsonSerializer.Serialize(location);
            var combinedElement = JsonSerializer.Deserialize<JsonElement>(combinedString);

            var nestedKeysList = new List<string>() { "location.city", "location.country", "sunset.results.sunset" };
            var expected = new List<string>() { "Dublin", "Ireland", "6:00:14 PM" };

            //Act
            var actual = story.GetEventValues(nestedKeysList, combinedElement);

            //Assert
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetEventValues_Returns_Null()
        {
            //Arrange
            Story story = new Story();
            var nestedKeysList = new List<string>() { "location.city", "sunset.results.sunset" };
            var expected = new List<string>() { "", "" };

            //Act
            var actual = story.GetEventValues(nestedKeysList, null);

            //Assert
            CollectionAssert.AreEqual(expected,actual);
        }
    }
}