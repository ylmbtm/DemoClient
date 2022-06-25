using UnityEngine;
using System.Collections;

namespace MapMagic
{
	public class Noise
	{
		private int iterations;
		public int seedX;
		public int seedZ;
		public int resolution;
		public float size;

		public Noise (float size, int resolution, int seedX, int seedZ)
		{
			this.size = size; this.resolution = resolution;
			this.seedX = seedX % 77777; this.seedZ = seedZ % 73333; //for backwards compatibility
			
			//get number of iterations
			iterations = 1; //max size iteration included
			float tempSize = size;
			for (int i=0; i<100; i++)
			{
				tempSize = tempSize/2;
				if (tempSize<1) break;
				iterations++;
			}
		}
		
		public float Fractal (int x, int z, float detail=0.5f)
		{
			float result = 0.5f;
			float curSize = size;
			float curAmount = 1;

			//making x and z resolution independent
			float rx = 1f*x / resolution * 512;
			float rz = 1f*z / resolution * 512;
				
			//applying noise
			for (int i=0; i<iterations;i++)
			{
				float curSizeBkcw = 8/(curSize*10+1); //for backwards compatibility. Use /curSize to get rid of extra calcualtions
						
				float perlin = Mathf.PerlinNoise(
					(rx + seedX + 1000*(i+1))*curSizeBkcw, 
					(rz + seedZ + 100*i)*curSizeBkcw );
				perlin = (perlin-0.5f)*curAmount + 0.5f;

				//applying overlay
				if (perlin > 0.5f) result = 1 - 2*(1-result)*(1-perlin);
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


