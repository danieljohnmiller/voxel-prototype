using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "voxel_test/biome_attribute")]
public class BiomeAttributes : ScriptableObject
{
    public string BiomeName;
    public int SolidGroundHeight;
    public int TerrainHeight;
    public float TerrainScale;

    public Lode[] Lodes;
}

[System.Serializable]
public class Lode 
{
    public string NodeName;
    public byte BlockId;
    public int MinHeight;
    public int MaxHeight;
    public float Scale;
    public float Threshold;
    public float NoiseOffset;
}

