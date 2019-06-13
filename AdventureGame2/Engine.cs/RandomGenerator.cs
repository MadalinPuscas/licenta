using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public static class RandomGenerator
    {
        private static Random rnd = new Random();

        public static int Number(int minimumValue, int maximumValue)
        {
            return rnd.Next(minimumValue, maximumValue + 1);
        }
    }
}
