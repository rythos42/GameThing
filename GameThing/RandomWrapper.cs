using System;

namespace GameThing
{
	public interface IRandomWrapper
	{
		double NextDouble();
		int Next(int minValue, int maxValue);
		int Next(int maxValue);
	}

	// Wrapper to support unit testing
	public class RandomWrapper : IRandomWrapper
	{
		private readonly Random random = new Random();

		public double NextDouble()
		{
			return random.NextDouble();
		}

		public int Next(int minValue, int maxValue)
		{
			return random.Next(minValue, maxValue);
		}

		public int Next(int maxValue)
		{
			return random.Next(maxValue);
		}
	}
}
