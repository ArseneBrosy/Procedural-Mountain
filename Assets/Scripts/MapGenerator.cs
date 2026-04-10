using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode { NoiseMap, ColourMap, Mesh };
	public DrawMode drawMode;

	public int mapWidth;
	public int mapHeight;
	public float noiseScale;

	public int octaves;
	[Range(0, 1)]
	public float persistance;
	public float lacunarity;

	[Min(0)]
	public float gradientIntensity;
	[Min(1)]
	public float centerIntensity;

	public float meshHeight = 1f;

	public int seed;
	public Vector2 offset;

	[Space]
	public int chunkSize;
	public int chunkWidth;
	public int chunkX;
	public int chunkY;

	[Space]
	[Range(0, 1)] public float forestMaxHeight;
	[Min(0)] public float forestMaxGradient;

	public bool autoUpdate;

	public TerrainType[] regions;

	float[,] noiseMap;

	public void GenerateMap() {
		noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset, gradientIntensity, centerIntensity);

		Color[] colourMap = new Color[mapWidth * mapHeight];
		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				float currentHeight = noiseMap[x, y];
				for (int i = 0; i < regions.Length; i++) {
					if (currentHeight <= regions[i].height) {
						colourMap[y * mapWidth + x] = regions[i].colour;
						break;
					}
				}
			}
		}

		MapDisplay display = FindFirstObjectByType<MapDisplay>();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
		} else if (drawMode == DrawMode.ColourMap) {
			display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
		} else if (drawMode == DrawMode.Mesh) {
			int width = noiseMap.GetLength(0);
			int height = noiseMap.GetLength(1);
			int[,] biomeMap = BiomesGenerator.GenerateBiomeMap(noiseMap, forestMaxHeight, forestMaxGradient);
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeight, (width - 1) / -2f, (height - 1) / 2f), TextureGenerator.TextureFromBiomeMap(biomeMap));
		}
	}

	public void GenerateChunk(int x, int y) {
		float[,] chunkNoiseMap = Noise.GenerateChunkNoiseMap(x, y, chunkSize, chunkWidth);

		MapDisplay display = FindFirstObjectByType<MapDisplay>();
		int width = noiseMap.GetLength(0);
		int height = noiseMap.GetLength(1);
		display.DrawChunkMesh(MeshGenerator.GenerateTerrainMesh(chunkNoiseMap, meshHeight, (width - 1) / -2f + chunkSize * x, (height - 1) / 2f - chunkSize * y, (float)chunkSize / chunkWidth), TextureGenerator.TextureFromHeightMap(chunkNoiseMap));
	}

	void OnValidate() {
		if (mapWidth < 1) {
			mapWidth = 1;
		}
		if (mapHeight < 1) {
			mapHeight = 1;
		}
		if (lacunarity < 1) {
			lacunarity = 1;
		}
		if (octaves < 0) {
			octaves = 0;
		}
	}
}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public Color colour;
}