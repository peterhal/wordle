using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace wordle {
    class Program {
        static bool MatchesTemplate(string template, string value) {
            if (template.Length != value.Length) {
                return false;
            }
            return Enumerable.Zip(template, value).All(pair => pair.First == pair.Second || pair.First == '_');
        }

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
        static List<Predicate<char>> compileTemplate(string template) {
            var result = new List<Predicate<char>>();
            for (var index = 0; index < template.Length; index++) {
                var ch = template[index];
                Predicate<char> predicate;
                if (ch == '_') {
                    predicate = c => true;
                } else if (ch == '[') {
                    index++;
                    var excludes = new List<char>();
                    while (template[index] != ']') {
                        excludes.Add(template[index]);
                        index++;
                    }
                    predicate = c => !excludes.Contains(c);
                } else {
                    predicate = c => c == ch;
                }
                result.Add(predicate);
            }
            return result;
        }

        static int Main(string[] args) {
            if (args.Length != 4) {
                Console.Error.WriteLine("Usage: wordle <dictionary> <template> <excludes> <includes>");
                return 1;
            }

            var words = File.ReadAllLines(args[0]);
            var template = args[1];
            var compiledTemplate = compileTemplate(template);
            var excludes = new HashSet<char>(args[2]);
            var includes = args.Length >= 4 ? new List<char>(args[3]) : new List<char>();
            var results = words.Where(word => MatchesCompiledTemplate(compiledTemplate, word) && !ContainsAny(excludes, word) && ContainsAll(includes, word)).ToArray();
            foreach (var word in results) {
                Console.WriteLine(word);
            }
            Console.WriteLine(results.Length);
            return 0;
        }
    }
}
