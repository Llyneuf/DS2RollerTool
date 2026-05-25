using System;
using System.Collections.Generic;

namespace DS2Roller.Logic
{
    public static class Roller
    {
        private static readonly Random Random = new();

        public static string Roll(List<string> list)
        {
            return list[Random.Next(list.Count)];
        }
    }
}