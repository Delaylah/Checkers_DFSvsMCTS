using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckersBoard
{
    public interface IRandomService
    {
        int Next(int maxValue);

        int Next(int start, int end);

        double NextDouble();
    }
    public class RandomService : IRandomService
    {
        private Random rand;

        public RandomService()
        {
            rand = new Random();
        }

        public RandomService(int seed)
        {
            rand = new Random(seed);
        }

        public int Next(int maxValue)
        {
            return rand.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return rand.Next(minValue, maxValue);
        }

        public double NextDouble()
        {
            return rand.NextDouble();
        }
    }
}
