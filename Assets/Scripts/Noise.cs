using UnityEngine;
using System.Collections;

public static class Noise {

	static float[,] noiseMap;

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, float gradientIntensity, float centerIntensity) {
		noiseMap = new float[mapWidth, mapHeight];
		Vector2[,] gradientMap = new Vector2[mapWidth, mapHeight];

		// Generate the random offset for each octave based on the seed
		System.Random prng = new System.Random(seed);
		Vector2[] octaveOffsets = new Vector2[octaves];
		for (int i = 0; i < octaves; i++) {
			float offsetX = prng.Next(-100000, 100000) + offset.x;
			float offsetY = prng.Next(-100000, 100000) + offset.y;
			octaveOffsets[i] = new Vector2(offsetX, offsetY);
		}

		// Keep the scale over 0
		if (scale <= 0) {
			scale = 0.0001f;
		}

		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;

		float[,,] octavesMaps = new float[octaves, mapWidth, mapHeight];

		// Create a blank gradient map
		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				gradientMap[x, y] = Vector2.zero;
			}
		}

		// Set the base amplitude and frequency
		float amplitude = 1;
		float frequency = 1;

		// Generate all octaves
		for (int i = 0; i < octaves; i++) {

			// Generate the Noisemap for this octave
			for (int y = 0; y < mapHeight; y++) {
				for (int x = 0; x < mapWidth; x++) {

					// Get the octave noise value at sample point
					float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
					float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
					octavesMaps[i, x, y] = perlinValue * amplitude;
				}
			}

			// Generate the gradient map
			for (int y = 0; y < mapHeight; y++) {
				for (int x = 0; x < mapWidth; x++) {
					float dx = 0f;
					float dy = 0f;

					// X derivative
					if (x < mapWidth - 1)
						dx = octavesMaps[i, x + 1, y] - octavesMaps[i, x, y];

					// Y derivative
					if (y < mapHeight - 1)
						dy = octavesMaps[i, x, y + 1] - octavesMaps[i, x, y];

					// Update the combined gradient map
					Vector2 gradient = new Vector2(dx, dy);
					gradientMap[x, y] += gradient;

					// Apply the gradient trick to errode this octave
					octavesMaps[i, x, y] *= 1.0f / (1.0f + gradientMap[x, y].magnitude * gradientIntensity);
				}
			}

			// Get the next amplitude and frequency
			amplitude *= persistance;
			frequency *= lacunarity;
		}

		// Combinate octaves into one noiseMap
		float maxNoiseHeight = float.MinValue;
		float minNoiseHeight = float.MaxValue;

		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {

				// Add all octaves
				float noiseHeight = 0;
				for (int i = 0; i < octaves; i++) {
					noiseHeight += octavesMaps[i, x, y];
				}

				// Get the min and max values of the noise map
				if (noiseHeight > maxNoiseHeight) {
					maxNoiseHeight = noiseHeight;
				} else if (noiseHeight < minNoiseHeight) {
					minNoiseHeight = noiseHeight;
				}

				noiseMap[x, y] = noiseHeight;
			}
		}

		// Normalize the Noisemap
		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				noiseMap[x, y] = 1 - Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
			}
		}

		// Calculate the distance form the center
		float[,] centerMap = new float[mapWidth, mapHeight];

		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				float distanceToCenter = Mathf.Sqrt(Mathf.Pow(halfWidth - x, 2) + Mathf.Pow(halfHeight - y, 2));
				distanceToCenter /= Mathf.Sqrt(Mathf.Pow(halfWidth, 2) + Mathf.Pow(halfWidth, 2));

				centerMap[x, y] = Mathf.Pow(centerIntensity, distanceToCenter * distanceToCenter * -1f);
			}
		}

		// Intensify the center of the map
		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				noiseMap[x, y] *= centerMap[x, y];
			}
		}

		return noiseMap;
	}

	public static float[,] GenerateChunkNoiseMap(int chunkX, int chunkY, int chunkSize, int chunkWidth) {
		float[,] chunkNoiseMap = new float[chunkWidth, chunkWidth];

		int startX = chunkX * chunkSize;
		int startY = chunkY * chunkSize;

		for (int y = 0; y < chunkWidth; y++) {
			for (int x = 0; x < chunkWidth; x++) {
				float noiseMapX = startX + x * (float)chunkSize / chunkWidth;
				float noiseMapY = startY + y * (float)chunkSize / chunkWidth;
				int noiseMapLeft = Mathf.FloorToInt(noiseMapX);
				int noiseMapTop = Mathf.FloorToInt(noiseMapY);

				float height = BilinearInterpolation(
					noiseMap[noiseMapLeft, noiseMapTop + 1],
					noiseMap[noiseMapLeft + 1, noiseMapTop + 1],
					noiseMap[noiseMapLeft, noiseMapTop],
					noiseMap[noiseMapLeft + 1, noiseMapTop],
					noiseMapX - noiseMapLeft,
					noiseMapY - noiseMapTop
				);

				chunkNoiseMap[x, y] = height;
            }
        }

		return chunkNoiseMap;
    }

	public static float BilinearInterpolation(float f00, float f10, float f01, float f11, float x, float y) {
		float r1 = f00 + x * (f10 - f00);
		float r2 = f01 + x * (f11 - f01);

		return r2 + y * (r1 - r2);
	}
}