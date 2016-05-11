using UnityEngine;
using System.Collections;

public class TerrainChange : MonoBehaviour
{
    public DepthWrapper KinectDepth;
	public short minDepthMM = 800;
	public short maxDepthMM = 2000;
	public float depthScale = 1;

    Terrain t; // terrain to modify
     int hmWidth, hmHeight; // heightmap width
    
     int alphaW, alphaH;
    
    int depthW = 320;
    int depthH = 240;

    float[,,] map;
    float[,] heights;

    void Start()
    {
        t = Terrain.activeTerrain;
        hmWidth = t.terrainData.heightmapWidth;
        hmHeight = t.terrainData.heightmapHeight;
        Terrain.activeTerrain.heightmapMaximumLOD = 0;

        heights = t.terrainData.GetHeights(0, 0, hmWidth, hmHeight);
        
        alphaW = t.terrainData.alphamapWidth;
        alphaH = t.terrainData.alphamapHeight;
		map = new float[alphaW, alphaH, t.terrainData.alphamapLayers];
    }

    float getHeight(short[] depthImage, int x, int y)
    {
		short depth = depthImage[y * 320 + x];
        if (depth == 0) return 0;
		float k = (maxDepthMM - depth) * depthScale / (maxDepthMM - minDepthMM);
		return k;
    }
    
    float constrain( float val, float minVal, float maxVal )
    {
        if( val < minVal ) return minVal;
        else if( val > maxVal ) return maxVal;
        else return val;
    }
    
    float lmap(float val, float inMin, float inMax, float outMin, float outMax)
    {
        return outMin + ((outMax - outMin) * (val - inMin)) / (inMax - inMin);
    }

    void Update()
    {
        if (!KinectDepth.pollDepth())
            return;

        short[] depthImage = KinectDepth.depthImg;

        // get the heights of the terrain under this game object

        // we set each sample of the terrain in the size to the desired height
        for (int i = 0; i < depthW; i++)
        {
            for (int j = 0; j < depthH; j++)
            {
                int newi = 240 - j - 1;
                int newj = 320 - i - 1;
                float h = getHeight(depthImage, i, j);
                heights[newi, newj] = h;
                // TODO: optimize
				map[newi, newj, 0] = constrain(lmap(h, 0.0f, 0.2f, 0.3f, 0.0f), 0, 1);
				map[newi, newj, 1] = lmap(h, 0.2f, 0.7f, 1.0f, 0.5f);
				map[newi, newj, 2] = lmap(h, 0.6f, 1.0f, 0.5f, 1.0f);
            }
        }
        // set the new height
        t.terrainData.SetHeights(0, 0, heights);
		t.terrainData.SetAlphamaps(0, 0, map);
		// AssignSplatMap.Process(t);
    }
}
