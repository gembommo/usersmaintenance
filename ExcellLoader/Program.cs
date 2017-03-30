using System;
using System.Collections.Generic;
using System.IO;
using CommonTools;

namespace ExcellLoader
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new[]
            {
                "EngBlackWords.txt"
            };
            if (args.IsNullOrEmpty())
            {
                return;
            }
            var items = new List<string>();
            var stream = new StreamReader(new FileStream(args[0], FileMode.Open));

            while (!stream.EndOfStream)
            {
                var collection = stream.ReadLine()?.Split(',');
                if (collection != null)
                    items.AddRange(collection);
            }
        }
    }
}