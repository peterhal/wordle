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
            foreach (var pair in Enumerable.Zip(template, value)) {
                if (pair.First != pair.Second && pair.First != '_') {
                    return false;
                }
            }
            return true;
        }

        static bool ContainsAny(HashSet<char> values, string value) {
            return value.Any(ch => values.Contains(ch));
        }

        static bool ContainsAll(List<char> values, string value) {
            return values.All(value.Contains);
        }
        static void Main(string[] args) {
            var words = File.ReadAllLines(args[0]);
            var template = args[1];
            var excludes = new HashSet<char>(args[2]);
            var includes = args.Length >= 4 ? new List<char>(args[3]) : new List<char>();
            foreach (var word in words.Where(word => MatchesTemplate(template, word) && !ContainsAny(excludes, word) && ContainsAll(includes, word))) {
                Console.WriteLine(word);
            }
        }
    }
}
