using System;
using System.IO;

namespace tiny_tines
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Checks in place to make sure args are not null and correct.
            if (args.Length == 0)
            {
                Console.WriteLine("Missing Argument: Please include a JSON file.");
                return;
            }

            string jsonFile = args[0];
            string fileExtension = Path.GetExtension(jsonFile);

            if (!File.Exists(jsonFile))
            {
                Console.WriteLine("File Not Found: Please provide a valid file.");
                return;
            }
            else if (fileExtension != ".json")
            {
                Console.WriteLine("Incorrect File Extension: The file must be a JSON file.");
                return;
            }
            else if (new FileInfo(jsonFile).Length == 0)
            {
                Console.WriteLine("Empty File: Please provid a valid non-empty file.");
                return;
            }

            string jsonString = File.ReadAllText(jsonFile);

            Story story = new Story();
            story.HandleStory(jsonString);
        }
    }
}