using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace wordle {
    class Program {
        static bool MatchesCompiledTemplate(List<Predicate<char>> template, string value) {
            if (template.Count != value.Length) {
                return false;
            }
            return Enumerable.Zip(template, value).All(pair => pair.First(pair.Second));
        }

        static bool ContainsAny(HashSet<char> values, string value) {
            return value.Any(ch => values.Contains(ch));
        }

        static bool ContainsAll(List<char> values, string value) {
            return values.All(value.Contains);
        }

        // Template syntax:
        //    _ = any character
        //    [<chars>] = not any of <chars>
        //    x = must be char x
        static Func<string, bool> compileTemplate(string template, HashSet<char> excludes, List<char> includes) {
            var result = new List<Predicate<char>>();
            for (var index = 0; index < template.Length; index++) {
                var ch = template[index];
                Predicate<char> predicate;
                if (ch == '_') {
                    predicate = c => true;
                } else if (ch == '[') {
                    index++;
                    var charExcludes = new List<char>();
                    while (template[index] != ']') {
                        charExcludes.Add(template[index]);
                        index++;
                    }
                    predicate = c => !charExcludes.Contains(c);
                } else {
                    predicate = c => c == ch;
                }
                result.Add(predicate);
            }
            return WordMatches(result, excludes, includes);
        }

        static bool IsValidColorChar(char ch) {
            switch (ch) {
                case 'b': case 'y': case 'g': return true;
                default: return false;
            }
        }

        static int Main(string[] args) {
            var words = File.ReadAllLines(args[0]).ToList();

            if (args.Length == 4) {
                var template = args[1];
                var excludes = new HashSet<char>(args[2]);
                var includes = args.Length >= 4 ? new List<char>(args[3]) : new List<char>();
                var filter = compileTemplate(template, excludes, includes);
                var results = GetResults(words, filter);
                PrintResults(results);
            } else if (args.Length == 1) {
                // interactive mode
                var guesses = new List<Tuple<string, string>>();
                int? wordLength = null;
                var allValidGuesses = new List<string>();
                var possibleWords = new List<string>();
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
                        if (!wordLength.HasValue) {
                            wordLength = guess.Length;
                            allValidGuesses = words.Where(word => word.Length == wordLength).ToList();
                            possibleWords = allValidGuesses.ToList();
                        }

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
                            var filter = CompileInteractiveInput(guess, colors);
                            possibleWords = GetResults(possibleWords, filter);
                            PrintResults(possibleWords);
                        }

                    }

                } while (!wordLength.HasValue || possibleWords.Count > 1);

            } else {
                Console.Error.WriteLine("Usage: wordle <dictionary> <template> <excludes> <includes>");
                return 1;
            }
            return 0;
        }

        private static Func<string, bool> CompileInteractiveInput(string guess, string colors) {
            var compiledTemplate = new List<Predicate<char>>();
            var excludes = new HashSet<char>();
            var includes = new List<char>();
            foreach (var pair in Enumerable.Zip(guess, colors)) {
                var ch = pair.First;
                switch (pair.Second) {
                    case 'b':
                        excludes.Add(ch);
                        compiledTemplate.Add(c => true);
                        break;
                    case 'y':
                        compiledTemplate.Add(c => c != ch);
                        // TODO: This condition does not correctly handle the case where
                        // there's also a green 'ch' in the guess
                        // Correct condition is that chars in 'b' guesses must contain ch.
                        includes.Add(ch);
                        break;
                    case 'g':
                        compiledTemplate.Add(c => c == ch);
                        break;
                    default:
                        throw new Exception("Unexpected color");
                }
            }
            return WordMatches(compiledTemplate, excludes, includes);
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

        private static Func<string, bool> WordMatches(List<Predicate<char>> compiledTemplate, HashSet<char> excludes, List<char> includes) {
            return word => MatchesCompiledTemplate(compiledTemplate, word) && !ContainsAny(excludes, word) && ContainsAll(includes, word);
        }
    }
}
