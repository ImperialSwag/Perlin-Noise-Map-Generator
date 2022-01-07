using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
	public enum DrawMode { NoiseMap, ColourMap };
	public DrawMode drawMode;

	public int mapWidth;
	public int mapHeight;
	public float noiseScale;

	public int octaves;
	[Range(-1, 1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public bool autoUpdate;

	public int colorDepth = 1;
	public TerrainType[] regions;
	TerrainType[] tempRegions;

	public int generateAmount = 15;
	public int startIndex = 0;
	public ColorTag colorTag;
	public bool isSeaTexture = false;

	public void GenerateMap()
	{
		float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

		tempRegions = AddRegionDepth(regions, colorDepth);
		Color[] colourMap = new Color[mapWidth * mapHeight];
		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				float currentHeight = noiseMap[x, y];
				for (int i = 0; i < tempRegions.Length; i++)
				{
					if (currentHeight <= tempRegions[i].height)
					{
						colourMap[y * mapWidth + x] = tempRegions[i].color;
						break;
					}
				}
			}
		}
		MapDisplay display = GetComponent<MapDisplay>();

		if (drawMode == DrawMode.NoiseMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
		}
		else if (drawMode == DrawMode.ColourMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromColorMap(colourMap, mapWidth, mapHeight));
		}
	}

	public void SaveTexture()
    {

		MapDisplay display = GetComponent<MapDisplay>();

		byte[] bytes = ((Texture2D)display.textureRender.sharedMaterial.mainTexture).EncodeToPNG();
		var dirPath = Application.dataPath + "/TextureOutput";
		if (!System.IO.Directory.Exists(dirPath))
		{
			System.IO.Directory.CreateDirectory(dirPath);
		}
        switch (colorTag)
        {
			case ColorTag.Red:
				dirPath += "/Red_";
				break;
			case ColorTag.Blue:
				dirPath += "/Blue_";
				break;
			case ColorTag.Green:
				dirPath += "/Green_";
				break;
			case ColorTag.Purple:
				dirPath += "/Purple_";
				break;
			case ColorTag.Gray:
				dirPath += "/Gray_";
				break;
			case ColorTag.Sun:
				dirPath += "/Sun_";
				break;
        }
		if (isSeaTexture)
		{
			System.IO.File.WriteAllBytes(dirPath + seed + "_A.png", bytes);
		} else
		{
			System.IO.File.WriteAllBytes(dirPath + seed + ".png", bytes);
		}
		Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + dirPath);
#if UNITY_EDITOR
		UnityEditor.AssetDatabase.Refresh();
#endif
	}

	public void SaveMultipleTexture(int startIndex)
    {
		for(int i = startIndex; i < startIndex+generateAmount; i++)
        {
			seed = i;
			GenerateMap();
			SaveTexture();
        }
    }

	public void SaveMultipleTexture()
    {
		SaveMultipleTexture(startIndex);
    }

	void OnValidate()
	{
		if (mapWidth < 1) mapWidth = 1;
		if (mapHeight < 1) mapHeight = 1;
		if (lacunarity < 1) lacunarity = 1;
		if (octaves < 0) octaves = 0;
		if (colorDepth < 1) colorDepth = 1;
	}

	TerrainType[] AddRegionDepth(TerrainType[] regions, int depth)
    {
		if (depth == 1) return regions;

		TerrainType[] tempRegions = new TerrainType[(regions.Length*depth)-1];

        int increaseRate = depth - 1;

		for (int a = 0; a < regions.Length; a++)
		{
			if (a + 1 == regions.Length)
			{
				tempRegions[a * depth] = regions[a];
				break;
			}
			else
			{
				for (int b = depth - 1; b >= 0; b--)
				{
					tempRegions[(a * depth) + b] = new TerrainType();
					tempRegions[(a * depth) + b].name = regions[a].name + "_d_" + b;
					float tempF = 1f * b / depth;
					tempRegions[(a * depth) + b].height = Mathf.Lerp(regions[a].height, regions[a + 1].height, tempF);
					tempRegions[(a * depth) + b].color = Color.Lerp(regions[a].color, regions[a + 1].color, tempF);
				}
			}
		}
		/*for (int z = 0; z < regions.Length; z++)
		{
			tempRegions[z * depth] = regions[z];
		}
		int y = 0;
		for(int x = 0; x < tempRegions.Length; x++)
        {
			// 0     1     2     3 
			// 0  1  2  3  4  5  6
			// 0 1 2 3 4 5 6 7 8 9

			if (tempRegions[x].name != null)
			{
				y = 0;
				continue;
			}
			else
			{
				tempRegions[x] = new TerrainType();
				tempRegions[x].name = regions[x - 1].name + "_d_" + y;
				float tempF = 1f / (((float)y + 1f) * (float)depth);
				tempRegions[x].height = Mathf.Lerp(regions[(x / depth) - 1].height, regions[(x / depth)].height, tempF);
				tempRegions[x].color = Color.Lerp(regions[(x / depth) - 1].color, regions[(x / depth)].color, tempF);
				y++;
				int a = 5;
			}
		}*/

		return tempRegions;
    }
}

[System.Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color color;
}

public enum ColorTag
{
	Red, Green, Blue, Purple, Gray, Sun
}