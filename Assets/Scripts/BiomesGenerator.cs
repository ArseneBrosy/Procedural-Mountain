using UnityEngine;

public static class BiomesGenerator
{
    public static BiomeData[] biomes = new BiomeData[2];

    public static void GenerateBiomes() {
        biomes[0] = new BiomeData(new Color(1f, 1f, 1f));
        biomes[1] = new BiomeData(new Color(0.1f, 0.47f, 0.1f));
        biomes[2] = new BiomeData(new Color(0.27f, 0.15f, 0.15f));
    }                                                    

    public static int[,] GenerateBiomeMap(float[,] noiseMap, float forestMaxHeight, float forestMaxGradient) {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        int[,] biomeMap = new int[width, height];

        // Generate the gradient map
        Vector2[,] gradientMap = new Vector2[width, height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                float dx = 0f;
                float dy = 0f;

                // X derivative
                if (x < width - 1)
                    dx = noiseMap[x + 1, y] - noiseMap[x, y];

                // Y derivative
                if (y < height - 1)
                    dy = noiseMap[x, y + 1] - noiseMap[x, y];

                // Update the gradient map
                Vector2 gradient = new Vector2(dx, dy);
                gradientMap[x, y] = gradient;
            }
        }

        // Scan the noise map
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                biomeMap[x, y] = 0;
                if (noiseMap[x, y] < forestMaxHeight) {
                    biomeMap[x, y] = 2;
                    if (gradientMap[x, y].magnitude < forestMaxGradient) {
                        biomeMap[x, y] = 1;
                    }
                }
            }
        }

        return biomeMap;
    }
}

public class BiomeData {
    public string name;
    public Color color;

    public BiomeData(Color _color) {
        color = _color;
    }
}
