﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

/// <summary>
/// Holds all loaded chunks.
/// (It's called "PCTerrain" to disambiguate from Unity's Terrain. PC as in "PowerCraft").
/// </summary>
public class PCTerrain
{
	// Singleton instance.
	private static PCTerrain instance;

	private PCTerrain()
	{
		this.chunks = new Dictionary<ChunkPosition, Chunk>();
		this.blocks = new Dictionary<Vector3, Block>();
	}

	public static PCTerrain GetInstance()
	{
		if (PCTerrain.instance == null)
			PCTerrain.instance = new PCTerrain();

		return PCTerrain.instance;
	}

	/// <summary>
	/// Contains instances of targeted blocks.
	/// </summary>
	public Dictionary<Vector3, Block> blocks;

	/// <summary>
	/// The terrain's chunks organized by (x,y) coordinates.
	/// </summary>
	public Dictionary<ChunkPosition, Chunk> chunks;

	/// <summary>
	/// Calls `BuildMesh()` on all chunks.
	/// </summary>
	public void RebuildAllChunks()
	{
		foreach(Chunk chunk in this.chunks.Values)
		{
			chunk.BuildMesh();
		}
	}

	/// <summary>
	/// Removes the block at the supplied position without breaking it.
	/// </summary>
	public void RemoveAt(int x, int y, int z)
	{
		if (this.blocks[(x,y,z).ToVector3()] == null)
			return;

		this.blocks[(x,y,z).ToVector3()] = null;
		this.RemoveBlockFromChunkMesh(x,y,z);
	}

	/// <summary>
	/// Breaks the block at the supplied position and clears the block instance.
	/// </summary>
	public void BreakAt(int x, int y, int z)
	{
		if (!this.blocks.ContainsKey((x,y,z).ToVector3()))
			return;

		this.blocks[(x,y,z).ToVector3()].Break();
		this.blocks[(x,y,z).ToVector3()] = null;
		this.RemoveBlockFromChunkMesh(x,y,z);
	}

	/// <summary>
	/// Removes a block given (x,y,z) Voxel coordinates.
	/// </summary>
	private void RemoveBlockFromChunkMesh(int x, int y, int z)
	{
		ChunkPosition chunkPosition = (
			Mathf.FloorToInt(x / 16), 
			Mathf.FloorToInt(z / 16)
		);

		this.chunks[chunkPosition].blocks[x % 16, y, z % 16] = new Air();
		this.chunks[chunkPosition].BuildMesh();
	}

	/// <summary>
	/// Places a block (given its reference) at the given position.
	/// Returns the local chunk coordinates where the block was placed.
	/// </summary>
	public Vector3 PlaceAt(Block blockRef, int x, int y, int z)
	{
		ChunkPosition chunkPosition = (
			Mathf.FloorToInt(x / 16), 
			Mathf.FloorToInt(z / 16)
		);

		this.chunks[chunkPosition].blocks[x % 16, y, z % 16] = blockRef;
		this.blocks[new Vector3(x % 16, y, z % 16)] = blockRef;

		this.chunks[chunkPosition].BuildMesh();

		return new Vector3(x % 16, y, z % 16);
	}


	/// <summary>
	/// Places a block (given its name) at the given position.
	/// </summary>
	public Vector3 PlaceAt(string blockName, int x, int y, int z)
	{
		// Create the instance
		Block b = Registry.Instantiate(blockName) as Block;
		b.coordinates = new Vector3Int(x,y,z);

		// Delegate logic
		return this.PlaceAt(b, x, y, z);
	}

	/// <summary>
	/// Places a block (given its name) at the given position.
	/// </summary>
	public Vector3 PlaceAt(string blockName, Vector3Int position)
	{
		// Create the instance
		Block b = Registry.Instantiate(blockName) as Block;
		b.coordinates = new Vector3Int(position.x, position.y, position.z);

		// Delegate logic
		return this.PlaceAt(b, position.x, position.y, position.z);
	}

	/// <summary>
	/// Places a block (given its reference) at the given position.
	/// </summary>
	public void PlaceAt(Block blockRef, Vector3Int position)
	{
		// Delegate logic
		this.PlaceAt(blockRef, position.x, position.y, position.z);
	}
}

public struct ChunkPosition
{
	/// <summary>
	/// x-position of the chunk in the world.
	/// </summary>
	public int x;

	/// <summary>
	/// z-position of the chunk in the world.
	/// </summary>
	public int z;

	public ChunkPosition(int x, int z)
	{
		this.x = x;
		this.z = z;
	}

	public static implicit operator ChunkPosition((int x, int z) t) => new ChunkPosition(t.x, t.z);

	public static bool operator ==(ChunkPosition a, ChunkPosition b)
	{
		return a.x == b.x && a.z == b.z;
	}

	public static bool operator !=(ChunkPosition a, ChunkPosition b)
	{
		return a.x != b.x || a.z != b.z;
	}

	public static ChunkPosition operator -(ChunkPosition a, ChunkPosition b)
	{
		ChunkPosition cp = new ChunkPosition();
		cp.x = a.x - b.x;
		cp.z = a.z - b.z;
		return cp;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
			return false;

		ChunkPosition cp = (ChunkPosition)obj;
		return this.x == cp.x && this.z == cp.z;
	}

	public override int GetHashCode()
	{
		int result = 17;
		
		result = result * 23 + this.x * this.z;

		return result;
	}

	public override string ToString()
	{
		return "ChunkPosition(" + this.x + ", " + this.z + ")";
	}
}