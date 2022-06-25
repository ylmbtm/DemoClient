using UnityEngine;
using System.Collections;

namespace MapMagic 
{
	public class InstanceRandom
	{
		private int seed;
		
		private float[] lut;
		private int current;
		
		public InstanceRandom (int s, int lutLength=100) 
		{ 
			seed = s; 
			lut = new float[lutLength];
			for (int i=0; i<lut.Length; i++) lut[i] = Random();
		}

		public float Random ()
		{ 
			seed = 214013*seed + 2531011; 
			return ((seed>>16)&0x7FFF) / 32768f;
		}

		public float Random (float min, float max)
		{
			seed = 214013*seed + 2531011; 
			float rnd = ((seed>>16)&0x7FFF) / 32768f;
			return rnd*(max-min) + min;
		}

		public float Random (Vector2 scope)
		{
			seed = 214013*seed + 2531011; 
			float rnd = ((seed>>16)&0x7FFF) / 32768f;
			return rnd*(scope.y-scope.x) + scope.x;
		}

		public int RandomToInt (float val)
		{
			int integer = (int)val;
			float remain = val - integer;
			if (remain>Random()) integer++;
			return integer;
		}
		
		//the chance 0-1 iterated steps times
		public float MultipleRandom (int steps)
		{
			float random = Random();
			return (1-Mathf.Pow(random,steps+1)) / (1-random) - 1;
		}

		public float CoordinateRandom (int x)
		{
			current = x % lut.Length;
			return lut[current];
		}

		public float CoordinateRandom (int x, Vector2 scope)
		{
			current = x % lut.Length;
			return lut[current]*(scope.y-scope.x) + scope.x;
		}

		public float CoordinateRandom (int x, int z)
		{
			z+=991; x+=1999;
			current = (x*x)%5453 + Mathf.Abs((z*x)%2677) + (z*z)%1871;
			current = current%lut.Length;
			if (current<0) current = -current;
			return lut[current];
		}

		public float NextCoordinateRandom ()
		{
			current++;
			current = current%lut.Length;
			return lut[current];
		}


		public static float Fractal (int x, int z, float size, float detail=0.5f)
		{
			//x+=1000
		
			float result = 0.5f;
			float curSize = size;
			float curAmount = 1;

			//get number of iterations
			int numIterations = 1; //max size iteration included
			for (int i=0; i<100; i++)
			{
				curSize = curSize/2;
				if (curSize<1) break;
				numIterations++;
			}

			//applying noise
			curSize = size;
			for (int i=0; i<numIterations;i++)
			{	
				float perlin = Mathf.PerlinNoise(x/(curSize+1), z/(curSize+1));
				perlin = (perlin-0.5f)*curAmount + 0.5f;

				//applying overlay
				if (perlin > 0.5f) result = 1 - 2*(1-result)*(1-perlin); //(1 - (1-2*(perlin-0.5f)) * (1-result));
				else result = 2*perlin*result;

				curSize *= 0.5f;
				curAmount *= detail; //detail is 0.5 by default
			}

			if (result < 0) result = 0;
			if (result > 1) result = 1;
				
			return result;
		}
	}
}
