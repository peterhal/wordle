using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace wordle {
    class Program {
        static bool IsValidColorChar(char ch) {
            switch (ch) {
                case 'b': case 'y': case 'g': return true;
                default: return false;
            }
        }

        static int Main(string[] args) {
            // First arg is set of possible words
            var words = File.ReadAllLines(args[0]).ToList();
            // Second arg is set of guesses that are never words
            var validGuesses = File.ReadAllLines(args[1]).ToList();
            var allValidGuesses = Enumerable.Concat(words, validGuesses).ToList();

            // interactive mode
            var guesses = new List<Tuple<string, string>>();
            int wordLength = 5;
            var possibleWords = words.ToList();
            PrintResults(possibleWords);
            PrintBestGuesses(allValidGuesses, possibleWords);
            do {

                Console.WriteLine("Enter your guessed word, followed by the colors returned for your guess.");
                Console.WriteLine("Valid colors are b - black; y - yellow; g - green.");
                Console.Write("> ");
                var input = Console.ReadLine();
                var inputWords = input.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (inputWords.Length != 2) {
                    Console.WriteLine($"Invalid input: Input must have exactly 2 entries; found {inputWords.Length}.");
                    continue;
                } else if (inputWords[0] is var guess && inputWords[1] is var colors && guess.Length != colors.Length) {
                    Console.WriteLine($"Invalid input: Each entry must be same length. Got {guess.Length} and {colors.Length}");
                    continue;
                } else {
                    if (wordLength != guess.Length) {
                        Console.WriteLine($"Invalid guess: Each guess must have the same length. This guess has {guess.Length} chars, previous guess had {wordLength} chars.");
                        continue;
                    } else if (!allValidGuesses.Contains(guess)) {
                        Console.WriteLine($"Invalid guess: Guess word {guess} is not a valid word in the dictionary.");
                        continue;
                    } else if (!colors.All(IsValidColorChar)) {
                        Console.WriteLine($"Invalid color: Second word in input must contain only valid colors (b, y, g). Found '{new String(colors.Where(ch => !IsValidColorChar(ch)).ToArray())}'");
                        continue;
                    } else {
                        // valid input!
                        var filter = FilterOfGuess(guess, colors);
                        possibleWords = GetResults(possibleWords, filter);
                        PrintResults(possibleWords);
                        PrintBestGuesses(allValidGuesses, possibleWords);
                    }

                }

            } while (possibleWords.Count > 1);
            Console.Write("Hit enter to exit.");
            Console.ReadLine();

            return 0;
        }

        private static Func<string, bool> FilterOfGuess(string guess, string colors) {
            return actual => GetColorsOfGuess(guess, actual) == colors;
        }

        private static void PrintResults(List<string> results) {
            foreach (var word in results) {
                Console.WriteLine(word);
            }
            Console.WriteLine(results.Count);
        }

        private static List<string> GetResults(List<string> words, Func<string, bool> filter) {
            return words.Where(filter).ToList();
        }

        private static string GetColorsOfGuess(string guess, string actual) {
            var actualCounts = new Dictionary<char, int>();
            foreach (var pair in Enumerable.Zip(guess, actual)) {
                var ch = pair.Second;
                if (pair.First != ch) {
                    actualCounts.TryGetValue(ch, out int actualCount);
                    actualCount += 1;
                    actualCounts[ch] = actualCount;
                }
            }

            var result = new StringBuilder();
            foreach (var pair in Enumerable.Zip(guess, actual)) {
                var g = pair.First;
                char rch;
                if (g == pair.Second) {
                    rch = 'g';
                } else {
                    if (actualCounts.TryGetValue(g, out int actualCount) && actualCount > 0) {
                        actualCounts[g] = actualCount - 1;
                        rch = 'y';
                    } else {
                        rch = 'b';
                    }
                }
                result.Append(rch);
            }
            return result.ToString();
        }

        // Guess to color to Words
        private static Dictionary<string, Dictionary<string, HashSet<string>>> GetGroups(List<string> dictionary, List<string> words) {
            var guessToColorToWord = new Dictionary<string, Dictionary<string, HashSet<string>>>();
            foreach (var guess in dictionary) {
                var inner = new Dictionary<string, HashSet<string>>();
                guessToColorToWord.Add(guess, inner);
                foreach (var word in words) {
                    var colors = GetColorsOfGuess(guess, word);
                    if (!inner.ContainsKey(colors)) {
                        inner.Add(colors, new HashSet<string>());
                    }
                    inner[colors].Add(word);
                }
            }
            return guessToColorToWord;
        }

        // Number of sets, (guess, color -> possible words, number of sets)
        private static IEnumerable<IGrouping<int, (string First, Dictionary<string, HashSet<string>> Second, int)>> GetBestGuesses(Dictionary<string, Dictionary<string, HashSet<string>>> groups) {
            return groups
                    .Select(pair => (pair.Key, pair.Value, pair.Value.Count))
                    .GroupBy(triple => triple.Item3)
                    .OrderByDescending(grouping => grouping.Key);
        }

        private static IEnumerable<IGrouping<int, (string First, Dictionary<string, HashSet<string>> Second, int)>> GetBestGuess(List<string> dictionary, List<string> words) {
            return GetBestGuesses(GetGroups(dictionary, words));
        }
        
        static void PrintBestGuesses(List<string> allValidGuesses, List<string> possibleWords) {
            var set = possibleWords.ToHashSet();
            var bestGuesses = GetBestGuess(allValidGuesses, possibleWords).First().ToList();
            if (!PrintGroups(bestGuesses.Where(triple => set.Contains(triple.First)))) {
                PrintGroups(bestGuesses);
            }
        }

        // (guess, color -> possible words, Maximum number of groups)
        private static bool PrintGroups(IEnumerable<(string First, Dictionary<string, HashSet<string>> Second, int)> bestGuesses) {
            var result = false;
            foreach (var bestGuess in bestGuesses) {
                Console.WriteLine($"Maximum number of groups {bestGuess.Item3}");
                Console.WriteLine(bestGuess.First);
                foreach (var pair in bestGuess.Second.OrderByDescending(pair => pair.Value.Count)) {
                    Console.WriteLine(pair.Key);
                    foreach (var value in pair.Value.OrderBy(value => value)) {
                        Console.Write("   ");
                        Console.WriteLine(value);
                    }
                }
                result = true;
            }
            return result;
        }
    }
}

