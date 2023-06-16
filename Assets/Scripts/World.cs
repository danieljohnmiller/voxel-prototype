using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public int seed;
    public BiomeAttributes Biome;

    public Transform Player;
    public Vector3 PlayerSpawnPostion;


    public Material Material;
    public BlockType[] blockTypes;

    private Chunk[,] Chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    List<ChunkCoord> ActiveChunks = new List<ChunkCoord> ();
    ChunkCoord PlayerChunkCoord;
    ChunkCoord PlayerLastChunkCoord;

    private void Start()
    {
        Random.InitState(seed);

        PlayerSpawnPostion = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight + 2f, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        PlayerLastChunkCoord = GetChunkCoordFromVector3 (Player.position);
    }

    private void Update()
    {
        PlayerChunkCoord = GetChunkCoordFromVector3 (Player.position);

        if (!PlayerChunkCoord.Equals(PlayerLastChunkCoord))
        {
            CheckViewDistance();
        }
    }

    void GenerateWorld()
    {
        for(int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                CreateNewChunk(x, z);
            }
        }

        Player.position = PlayerSpawnPostion;
    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(Player.position);
        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(ActiveChunks);

        for(int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {
                    if (Chunks[x, z] == null)
                    {
                        CreateNewChunk(x, z);
                    }
                    else if (!Chunks[x, z].IsActive)
                    {
                        Chunks[x, z].IsActive = true;
                        ActiveChunks.Add(new ChunkCoord(x, z));
                    }
                    
                }

                for(int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                    {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }

        foreach(ChunkCoord leftoverCoord in previouslyActiveChunks)
        {
            Chunks[leftoverCoord.x, leftoverCoord.z].IsActive = false;
        }
    }

    public byte GetVoxel(Vector3 pos)
    {
        int yPos = Mathf.FloorToInt(pos.y);

        if (!IsVoxelInWorld(pos))
            return 0;


        // bottom block of chunk. return bedrock
        if(yPos == 0)
        {
            return 1;
        }

        // basic terrain pass

        int terrainHeight = Mathf.FloorToInt(Biome.TerrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, Biome.TerrainScale)) + Biome.SolidGroundHeight;

        byte voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = 3;
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = 6;
        else if (yPos > terrainHeight)
            return 0;
        else
            voxelValue = 2;


        // second pass

        if (voxelValue == 2)
        {
            foreach(Lode lode in Biome.Lodes)
            {
                if(yPos > lode.MinHeight && yPos < lode.MaxHeight)
                {
                    if(Noise.Get3DPerlin(pos, lode.NoiseOffset, lode.Scale, lode.Threshold))
                    {
                        voxelValue = lode.BlockId;
                    }
                }
            }
        }
        return voxelValue;


    }

    void CreateNewChunk(int x, int z)
    {
        Chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
        ActiveChunks.Add(new ChunkCoord(x, z));
    }

    bool IsChunkInWorld (ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && 
            coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
        {
            return true;
        }
        return false;
    }

    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels &&
            pos.y >= 0 && pos.y < VoxelData.ChunkHeight &&
            pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
        {
            return true;
        }
        return false;
    }
}



[System.Serializable]
public class BlockType
{
    public string BlockName;
    public bool IsSolid;


    [Header("TextureValues")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // back, front, top, bottom, left, right



    public int GetTextureId(int faceIndex)
    {
        switch (faceIndex)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3: 
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5: 
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureId; invalid face index");
                return 0;
        }
    }
}