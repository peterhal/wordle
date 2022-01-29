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

        static bool ContainsAny(HashSet<char> values, string value) {
            return value.Any(ch => values.Contains(ch));
        }

        static bool ContainsAll(List<char> values, string value) {
            return values.All(value.Contains);
        }
        static int Main(string[] args) {
            if (args.Length != 4) {
                Console.Error.WriteLine("Usage: wordle <dictionary> <template> <excludes> <includes>");
                return 1;
            }

            var words = File.ReadAllLines(args[0]);
            var template = args[1];
            var excludes = new HashSet<char>(args[2]);
            var includes = args.Length >= 4 ? new List<char>(args[3]) : new List<char>();
            foreach (var word in words.Where(word => MatchesTemplate(template, word) && !ContainsAny(excludes, word) && ContainsAll(includes, word))) {
                Console.WriteLine(word);
            }
            return 0;
        }
    }
}
