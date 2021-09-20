using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace tiny_tines
{
    /// <summary>
    /// A class for processing a Tiny-Tines Story.
    /// </summary>
    public class Story
    {
        //A constant string variable representing the 'HTTP Request Action' value.
        private const string _httpActionType = "HTTPRequestAction";

        //A constant string variable representing the 'Print Action' value.
        private const string _printActionType = "PrintAction";

        private JsonHandler jsonHandler = new JsonHandler();

        /// <summary>
        /// Handles the Story and the Story's Actions.
        /// </summary>
        /// <param name="jsonString">A <see cref="String"/> object denoting the Story's JSON object data.</param>
        public void HandleStory(string jsonString)
        {
            //Gets an array of all the Actions in the Story.
            JsonElement actionsElement = jsonHandler.DeserializeToElement(jsonString);
            var elementNullable = jsonHandler.GetObjectValue(actionsElement, "actions");
            if (elementNullable is null) return;

            actionsElement = (JsonElement)elementNullable;
            JsonElement.ArrayEnumerator actionsElementArray = jsonHandler.ElementToArray(actionsElement);

            //Event object that is passed as the input-output Event.
            JsonElement? passedEventElement = null;

            foreach (var action in actionsElementArray)
            {
                //Gets the actions keys: type, name, options.
                var actionType = jsonHandler.GetObjectValue(action, "type").ToString();
                var actionName = jsonHandler.GetObjectValue(action, "name").ToString();
                var optionsElement = (JsonElement)jsonHandler.GetObjectValue(action, "options");

                //Tries to get the option values: url and message.
                var url = jsonHandler.GetObjectValue(optionsElement, "url");
                var message = jsonHandler.GetObjectValue(optionsElement, "message");

                //Checks if the Action has an option type.
                bool hasUrlOption = (url != null);
                bool hasMsgOption = (message != null);

                //Action's type must match 'HTTPResonseAction' and must have a 'url' option in order to do the action.
                if (actionType == _httpActionType && hasUrlOption)
                {
                    passedEventElement = HttpGetAction(passedEventElement, actionName, url.ToString());

                    //If there is no repsonse or an error the method will break, then exit back to Program class where it ends.
                    if (passedEventElement == null) break;
                }
                //Action's type must match 'PrintAction' and must have a 'message' option in order to do the action.
                else if (actionType == _printActionType && hasMsgOption)
                {
                    PrintAction(passedEventElement, message.ToString());
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="inputEventElement">The input event represented by a <see cref="JsonElement"/>? denoting JSON data or null object.</param>
        /// <param name="name">The <see cref="String"/> object representing the name of the Action.</param>
        /// <param name="url">The <see cref="String"/> representing the Action's option value.</param>
        /// <returns>A <see cref="JsonElement"/> denoting the response Json data; otherwise; <c>null</c>.</returns>
        public JsonElement? HttpGetAction(JsonElement? inputEventElement, string name, string url)
        {
            //Gets and replaces the url's substrings with proper event values. If applicable.
            var keysList = GetSubstrings(url);
            if (keysList.Count != 0)
            {
                var valueLst = GetEventValues(keysList, inputEventElement);
                url = ReplaceSubstrings(valueLst, url);
            }

            string responseString = null;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception)
            {
                //In case of any error terminates program.
                System.Environment.Exit(1);
            }

            if (responseString is null) return null;

            var responseElement = jsonHandler.DeserializeToElement(responseString);

            //Adds Action's name as the key and the response JsonElement as the value.
            var outputEventElement = jsonHandler.AddRootKey(responseElement, name);

            if (inputEventElement != null)
            {
                outputEventElement = jsonHandler.CombineElements((JsonElement)inputEventElement, outputEventElement);
            }

            return outputEventElement;
        }

        /// <summary>
        /// Adjusts message with the input Event values and writes to console.
        /// </summary>
        /// <param name="inputEventElement">The input event represented by a <see cref="JsonElement"/>? denoting JSON data or null object.</param>
        /// <param name="message">The <see cref="String"/> representing the Action's option value.</param>
        public void PrintAction(JsonElement? inputEventElement, string message) //SHOULD YOU ADD RETURN OUTPUT EVENT
        {
            //Gets and replaces the message's substrings with the proper event values.If applicable.
            var keysList = GetSubstrings(message);

            if (keysList.Count != 0)
            {
                var valueList = GetEventValues(keysList, inputEventElement);
                message = ReplaceSubstrings(valueList, message);
            }

            Console.WriteLine(message);
        }

        /// <summary>
        /// Gets all nested values from an Event JsonElement.
        /// </summary>
        /// <param name="nestedKeysList">A <see cref="List{T}"/> where each item is a <see cref="String"/> object representing nested keys (separating by a . <see cref="char"/>), used to find a value.</param>
        /// <param name="eventElement">A <see cref="JsonElement"/> object representing an Event's JSON data.</param>
        /// <returns>A <see cref="List{T}"/> where each item is the found value; otherwise: empty string List</returns>
        public List<string> GetEventValues(List<string> nestedKeysList, JsonElement? eventElement)
        {
            if (eventElement == null)
            {
                //Event does not exist; replaces every list item with an empty string.
                var emptyStringList = nestedKeysList.Select(y => y = "").ToList();
                return emptyStringList;
            }

            //For each nested keys item
            for (int idx = 0; idx < nestedKeysList.Count; idx++)
            {
                //Gets the current List item, used to find ONE nested value. Eg. item => sunset.results.day_length
                var nestedKeysString = nestedKeysList[idx];
                var splitNestedKeysList = nestedKeysString.Split(".").ToList();

                var value = jsonHandler.GetNestedValue((JsonElement)eventElement, splitNestedKeysList);

                //Replaces, in the List, the key with the found value.
                nestedKeysList[idx] = value;
            }

            return nestedKeysList;
        }

        /// <summary>
        /// Gets all substrings <c>within</c> curly braces.
        /// </summary>
        /// <param name="valueString"></param>
        /// <returns>A <see cref="List{T}"/> of all matched substrings; otherwise, empty List</returns>
        public static List<string> GetSubstrings(string valueString)
        {
            Regex regexGetInbetweenPattern = new Regex(@"{{([^{{}}]+)}}");
            var matchList = (from Match m in regexGetInbetweenPattern.Matches(valueString) select m.Groups[1].Value).ToList();

            return matchList;
        }

        /// <summary>
        /// Replaces all substrings encapsulated by double curly braces by new values.
        /// </summary>
        /// <param name="replacementValueList">A <see cref="List{T}"/> of values to be used as replacements. In order of match.</param>
        /// <param name="valueString">A <see cref="String"/> object containing substrings to be replaced.</param>
        /// <returns>A <see cref="String"/> object with replaced substrings.</returns>
        public static string ReplaceSubstrings(List<string> replacementValueList, string valueString)
        {
            //Regex pattern to match substring including encapsulating double curly braces.
            Regex regexGetAllPattern = new Regex(@"({{[^{{}}]+}})"); //{{.*?}}");

            foreach (var value in replacementValueList)
            {
                valueString = regexGetAllPattern.Replace(valueString, value, 1);
            }

            return valueString;
        }
    }
}