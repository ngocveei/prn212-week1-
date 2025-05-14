// A utility to analyze text files and provide statistics
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace FileAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("File Analyzer - .NET Core");
            Console.WriteLine("This tool analyzes text files and provides statistics.");
            
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide a file path as a command-line argument.");
                Console.WriteLine("Example: dotnet run myfile.txt");
                return;
            }
            
            string filePath = args[0];
            
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: File '{filePath}' does not exist.");
                return;
            }
            
            try
            {
                Console.WriteLine($"Analyzing file: {filePath}");
                
                // Read the file content
                string content = File.ReadAllText(filePath);

                // TODO: Implement analysis functionality
                // 1. Count words
                string[] words = content
                    .Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                int wordCount = words.Length;
                Console.WriteLine($"Number of words: {wordCount}");

                // 2. Count characters
                int charCountWithSpaces = content.Length;
                int charCountWithoutSpaces = content.Count(c => !char.IsWhiteSpace(c));
                Console.WriteLine($"Number of characters (with spaces): {charCountWithSpaces}");
                Console.WriteLine($"Number of characters (without spaces): {charCountWithoutSpaces}");

                // 3. Count sentences (basic detection using '.', '!', '?')
                char[] sentenceDelimiters = { '.', '!', '?' };
                int sentenceCount = content.Split(sentenceDelimiters, StringSplitOptions.RemoveEmptyEntries).Length;
                Console.WriteLine($"Number of sentences: {sentenceCount}");

                // 4. Most common words (case-insensitive)
                var wordFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var word in words)
                {
                    string cleanWord = new string(word.Where(char.IsLetterOrDigit).ToArray()).ToLower();
                    if (string.IsNullOrEmpty(cleanWord)) continue;

                    if (wordFrequency.ContainsKey(cleanWord))
                        wordFrequency[cleanWord]++;
                    else
                        wordFrequency[cleanWord] = 1;
                }

                var mostCommonWord = wordFrequency.OrderByDescending(kv => kv.Value).FirstOrDefault();
                Console.WriteLine($"Most common word: '{mostCommonWord.Key}' (appears {mostCommonWord.Value} times)");

                // 5. Average word length
                double avgWordLength = words
                    .Where(w => w.Any(char.IsLetterOrDigit))
                    .Select(w => w.Count(char.IsLetterOrDigit))
                    .DefaultIfEmpty(0)
                    .Average();
                Console.WriteLine($"Average word length: {avgWordLength:F2}");


                // Example implementation for counting lines:
                int lineCount = File.ReadAllLines(filePath).Length;
                Console.WriteLine($"Number of lines: {lineCount}");
                
                // TODO: Additional analysis to be implemented
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during file analysis: {ex.Message}");
            }
        }
    }
}