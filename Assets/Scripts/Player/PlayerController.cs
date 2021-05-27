﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

public class PlayerController : MonoBehaviour
{
	private Camera _camera;

	/// <summary>
	/// Hard reference to the block that is currently being broken.
	/// </summary>
	private Block _breakingBlockReference;

	/// <summary>
	/// Indicates whether or not a block is being broken.
	/// </summary>
	private bool _breakingBlock = false;
	
    // Start is called before the first frame update
    void Start()
    {
        this._camera = this.GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
			this.HandleMouseLeftClickDown();

		if (Input.GetMouseButton(0))
			this.HandleMouseLeftClickHeld();

		if (Input.GetMouseButtonUp(0))
			this.HandleMouseLeftClickUp();

		if (Input.GetMouseButtonDown(1))
			this.Place();
    }

	private void HandleMouseLeftClickDown()
	{

	}

	private void HandleMouseLeftClickHeld()
	{
		this.KeepBreaking();
	}

	private void HandleMouseLeftClickUp()
	{
		this.EndBreak();
	}

	/// <summary>
	/// Manages the BeginBreak logic. See KeepBreaking().
	/// </summary>
	/// <param name="target">The `Face` GameObject that was hit by a Raycast.</param>
	private void BeginBreak(Block target)
	{
		if (target != null)
		{
			this._breakingBlock = true;
			this._breakingBlockReference = target;

			target.BeginBreak();
		}
	}

	private void PrintVector(Vector3 v)
	{
		Debug.Log("[" + v.x + ", " + v.y + ", " + v.z + "]");
	}

	/// <summary>
	/// Manages block breaking (start-progress-end). 
	/// If the player is currently breaking a block and the view moves off no longer targeting the block, breaking is reset.
	/// </summary>
	private void KeepBreaking()
	{
		Block block = TargetBlock.Get();
		
		if (block == null) 
		{
			// The player is currently breaking a block and moved off the cursor to the sky.
			if (this._breakingBlock && this._breakingBlockReference != null)
				this.ResetBreaking();

			return;
		}

		if (block.broken)
			return;

		if (this._breakingBlockReference == null)
			this.BeginBreak(block);

		if ((this._breakingBlockReference.id != block.id) && this._breakingBlock)
			this.ResetBreaking();
	}

	/// <summary>
	/// If the player lifted the configured break button before the block was broken, end breaking.
	/// </summary>
	private void EndBreak()
	{
		if (!this._breakingBlock)
			return;

		Block block = TargetBlock.Get();

		if (block.broken)
			return;

		this._breakingBlock = false;
		this._breakingBlockReference = null;

		if (block == null)
			return;

		block.EndBreak();
	}

	/// <summary>
	/// Resets the breaking state of the target block.
	/// </summary>
	private void ResetBreaking()
	{
		if (this._breakingBlockReference == null || !this._breakingBlock)
			return;

		if (this._breakingBlockReference.broken)
		{
			// The block was successfully broken!
			// Block.EndBreak() was already called by the block itself. Reset only bool state and ref.
			this._breakingBlock = false;
			this._breakingBlockReference = null;

			return;
		}

		this._breakingBlockReference.EndBreak();

		this._breakingBlock = false;
		this._breakingBlockReference = null;
	}

	/// <summary>
	/// Allows the player to place a placeable block from the currently active item in the hotbar.
	/// </summary>
	void Place()
	{
		// This process would have to first get the active item in the hotbar, check whether it's placeable and onl then place it.
		string blockName = "dirt";

		RaycastHit hit;
		bool didHit = Physics.Raycast(Camera.main.ScreenPointToRay((
			Camera.main.pixelWidth / 2,
			Camera.main.pixelHeight / 2,
			0
		).ToVector3()), out hit);

		if (!didHit)
			return;
		
		Vector3Int placingBlockCoordinates = Utils.ToVectorInt(hit.point + hit.normal / 2.0f);
		Vector3Int playerPosition = Player.instance.GetVoxelPosition();

		if (
			placingBlockCoordinates == playerPosition || 
			placingBlockCoordinates == new Vector3Int(playerPosition.x, playerPosition.y + 1, playerPosition.z)
		)
			return;
		
		PCTerrain.GetInstance().PlaceAt(blockName, placingBlockCoordinates);
	}
}
