using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{

	public ChunkCoord coord;

	private GameObject ChunkObject;

    private MeshRenderer MeshRenderer;
    private MeshFilter MeshFilter;

	private int VertexIndex = 0;
	private List<Vector3> Vertices = new List<Vector3>();
	private List<int> Triangles = new List<int>();
	private List<Vector2> Uvs = new List<Vector2>();

	private byte[,,] VoxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

	World World;

	public Chunk(ChunkCoord coord, World world)
    {
        World = world;
        ChunkObject = new GameObject();
        MeshFilter = ChunkObject.AddComponent<MeshFilter>();
        MeshRenderer = ChunkObject.AddComponent<MeshRenderer>();
        MeshRenderer.material = world.Material;
        ChunkObject.transform.SetParent(world.transform);
		ChunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
		ChunkObject.name = $"Chunk {coord.x}, {coord.z}";

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
        this.coord = coord;
    }

    void PopulateVoxelMap()
    {
		for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
			for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
				for (int z = 0; z < VoxelData.ChunkWidth; z++) 
				{
					VoxelMap[x, y, z] = World.GetVoxel(new Vector3(x, y, z) + Position);
				}		
			}
		}
	}

	void CreateMeshData()
    {
		for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
			for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
				for (int z = 0; z < VoxelData.ChunkWidth; z++)
				{
                    if (World.blockTypes[VoxelMap[x, y, z]].IsSolid)
                    {
						AddVoxelDataToChunk(new Vector3(x, y, z));
					}
					
				}
			}
		}	
	}


	public bool IsActive
    {
        get{ return ChunkObject.activeSelf; }
        set { ChunkObject.SetActive(value); }
    }

	public Vector3 Position
    {
		get { return ChunkObject.transform.position; }
    }


	bool IsVoxelInChunk(int x, int y, int z)
    {
		if 
		(
			x < 0 || x > VoxelData.ChunkWidth - 1 ||
			y < 0 || y > VoxelData.ChunkHeight - 1 ||
			z < 0 || z > VoxelData.ChunkWidth - 1
		)
			return false;
		return true;
	}

	bool CheckVoxel(Vector3 pos)
    {
		int x = Mathf.FloorToInt(pos.x);
		int y = Mathf.FloorToInt(pos.y);
		int z = Mathf.FloorToInt(pos.z);

		if(!IsVoxelInChunk(x, y, z))
			return World.blockTypes[World.GetVoxel(pos + Position)].IsSolid;

		return World.blockTypes[VoxelMap[x, y, z]].IsSolid;
	}

	void AddVoxelDataToChunk(Vector3 pos)
    {
		for (int p = 0; p < 6; p++)
		{
			if(!CheckVoxel(pos + VoxelData.FaceChecks[p]))
            {
				byte blockId = VoxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

				Vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
				Vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
				Vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
				Vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                AddTexture(World.blockTypes[blockId].GetTextureId(p));

				Triangles.Add(VertexIndex);
				Triangles.Add(VertexIndex + 1);
				Triangles.Add(VertexIndex + 2);
				Triangles.Add(VertexIndex + 2);
				Triangles.Add(VertexIndex + 1);
				Triangles.Add(VertexIndex + 3);

				VertexIndex += 4;
			}
		}
	}

	void CreateMesh()
    {
		Mesh mesh = new Mesh();
		mesh.vertices = Vertices.ToArray();
		mesh.triangles = Triangles.ToArray();
		mesh.uv = Uvs.ToArray();

		mesh.RecalculateNormals();

		MeshFilter.mesh = mesh;
	}

	void AddTexture(int textureId)
    {
		float y = textureId / VoxelData.TextureAtlasSizeInBlocks;
		float x = textureId - (y * VoxelData.TextureAtlasSizeInBlocks);

		x *= VoxelData.NormalisedBlockTextureSize;
		y *= VoxelData.NormalisedBlockTextureSize;

		y = 1f - y - VoxelData.NormalisedBlockTextureSize;

		Uvs.Add(new Vector2(x, y));
		Uvs.Add(new Vector2(x, y + VoxelData.NormalisedBlockTextureSize));
		Uvs.Add(new Vector2(x + VoxelData.NormalisedBlockTextureSize, y));
		Uvs.Add(new Vector2(x + VoxelData.NormalisedBlockTextureSize, y + VoxelData.NormalisedBlockTextureSize));
	}

}

public class ChunkCoord
{
	public int x;
	public int z;

	public ChunkCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

	public bool Equals(ChunkCoord other)
    {
		if (other is null)
			return false;
		else if(other.x == x && other.z == z)
        {
			return true;
        }
		return false;
    }
} 
