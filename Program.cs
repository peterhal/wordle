using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace wordle
{
    class Program
    {
        static bool matchesTemplate(string template, string value)
        {
            if (template.Length != value.Length)
            {
                return false;
            }
            foreach (var pair in Enumerable.Zip(template, value))
            {
                if (pair.First != pair.Second && pair.First != '_')
                {
                    return false;
                }
            }
            return true;
        }

        static bool containsAny(HashSet<char> values, string value)
        {
            return value.Any(ch => values.Contains(ch));
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var words = File.ReadAllLines(args[0]);
            var template = args[1];
            var excludes = new HashSet<char>(args[2]);
            foreach (var word in words.Where(word => matchesTemplate(template, word) && !containsAny(excludes, word)))
            {
                Console.WriteLine(word);
            }
        }
    }
}
