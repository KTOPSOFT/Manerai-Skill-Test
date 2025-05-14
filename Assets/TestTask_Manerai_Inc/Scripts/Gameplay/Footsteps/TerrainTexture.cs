using UnityEngine;

using System.Collections;
using System.Collections.Generic;

// ===================================================================================================
//    https://gamedevbeginner.com/terrain-footsteps-in-unity-how-to-detect-different-textures/
// ===================================================================================================

// code was repurposed to make CheckTexture() a public static method to be called in other classes (namely Footsteps.cs)

namespace YukiOno.SkillTest
{
    public class TerrainTexture : MonoBehaviour
    {
        public static float[] CheckTexture(ColliderInfo colliderInfo, Vector3 playerPosition, out int dominantLayer)
        {
            Terrain terrain = colliderInfo.GetTerrain();

            Vector3 coordinates = ConvertPosition(playerPosition, terrain);

            int posX = (int) coordinates.x;
            int posZ = (int) coordinates.z;

            // =======================================

            float[,,] aMap = terrain.terrainData.GetAlphamaps(posX, posZ, 1, 1);

            float[] textureValues = new float[colliderInfo.materialTypes.Length];

            int arrayLength = aMap.Length;
            int materialCount = colliderInfo.materialTypes.Length;

            int dominantLayerIndex = 0;
            float dominantLayerValue = 0f;

            for (int i = 0; i < arrayLength; i ++)
            {
                if (i < materialCount)
                {
                    textureValues[i] += aMap[0, 0, i];

                    float layerValue = textureValues[i];

                    if (layerValue > dominantLayerValue)
                    {
                        dominantLayerIndex = i;
                        dominantLayerValue = layerValue;
                    }
                }
            }

            // =======================================

            textureValues[dominantLayerIndex] *= 1.5f;

            if (textureValues[dominantLayerIndex] > 1.0f)
                textureValues[dominantLayerIndex] = 1.0f;

            // =======================================

            dominantLayer = dominantLayerIndex;

            return textureValues;
        }

        private static Vector3 ConvertPosition(Vector3 playerPosition, Terrain terrain)
        {
            Vector3 terrainPosition = playerPosition - terrain.transform.position;

            float mapX = terrainPosition.x / terrain.terrainData.size.x;
            float mapZ = terrainPosition.z / terrain.terrainData.size.z;

            Vector3 mapPosition = new Vector3(mapX, 0f, mapZ);

            float xCoord = mapPosition.x * terrain.terrainData.alphamapWidth;
            float zCoord = mapPosition.z * terrain.terrainData.alphamapHeight;

            Vector3 output = new Vector3(xCoord, 0, zCoord);

            return output;
        }
    }
}










