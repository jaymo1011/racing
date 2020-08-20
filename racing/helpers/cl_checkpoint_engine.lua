--[[
	Checkpoint Engine

	Handles the creation, obtaining and deletion of checkpoints as instructed by the server.
]]

-- These may be dynamic?
-- now server managed
-- local chpBaseCol = {GetHudColour(13)}
-- local chpConeCol = {GetHudColour(134)}

-- Globals
CheckpointLoopActive = false -- Will be set by the main script

-- Constants
local blipNextCHPScale = 1.0
local blipDefaultScale = 0.5
local blipColour = 66
local blipSprite = 1

-- A table for storing all checkpoints we're aware of
local checkpoints = {}

-- Turns a creation string (as supplied by the server) into a properly placed checkpoint.
local function CreateRaceCheckpoint(chpd)
	-- Create the checkpoint
	local newChp = CreateCheckpoint(chpd.type, chpd.pos.xyz, chpd.target.xyz, cgpd.radius, GetHudColour(chpd.baseHudColour), 0)

	-- Mess with the cone and icon
	SetCheckpointCylinderHeight(newChp, chpd.coneNearHeight, chpd.coneFarHeight, chpd.coneRadius)
	SetCheckpointIconRgba(newChp, GetHudColour(chpd.coneHudColour))

	-- Clip checkpoints to the surface below them where they are not round
	if not chpd.isRound then
		-- UNTESTED, hopefully this just works and I don't have to do the silly ray casting
		-- Also, the names are weird, I know
		local found, groundZ, normalX, normalY, normalZ = GetGroundZAndNormalFor_3dCoord(chpd.pos)
		if found then
			local clipPlane = vec3(chpd.pos.x - 0.05, chpd.pos.y, groundZ)
			local surfaceNormal = vec3(normalX, normalY, normalZ)
			N_0xf51d36185993515d(newChp, clipPlane.xyz, surfaceNormal.xyz) -- lol soz for not documenting yet :P
		end
	end

	-- Return the checkpoint
	return newChp
end

local function CreateCheckpointBlip(chpd)
	local blip = AddBlipForCoord(chpd.pos.xyz)
	SetBlipSprite(blip, blipSprite)
	SetBlipColour(blip, blipColour)
	SetBlipScale(blip, blipDefaultScale)

	return blip
end

local function CreateCheckpointObject(chpUpdData)
	return {
		chpHandle1 = chpUpdData.chp1 and CreateRaceCheckpoint(chpUpdData.chp1) or nil,
		blpHandle1 = chpUpdData.chp1 and CreateCheckpointBlip(chpUpdData.chp1) or nil,

		chpHandle2 = chpUpdData.chp2 and CreateRaceCheckpoint(chpUpdData.chp2) or nil,
		blpHandle2 = chpUpdData.chp2 and CreateCheckpointBlip(chpUpdData.chp2) or nil,

		-- For checking if we've touched a checkpoint
		pos1 = chpUpdData.chp1.pos or nil,
		pos2 = chpUpdData.chp2.pos or nil,
	}
end

RegisterNetEvent("racing:receiveCheckpointUpdate")
AddEventHandler("racing:receiveCheckpointUpdate", function(updateData)
	-- The update data is JSON so we just need to decode it
	local updateData = json.decode(updateData)

	-- If in this update, we cleared a checkpoint then delete it and scale the blip of the next checkpoint correctly
	if updateData.cleared then
		-- Why are we wrapping this in a do block?
		-- This is so we can return quickly and skip operations we can't do but also not return from the event as whole.
		do
			-- Get the checkpoint we just cleared, if there were no checkpoints, don't continue
			local clearedCheckpoint = table.remove(checkpoints) or false
			if not clearedCheckpoint then return end

			-- Delete the checkpoint and blip (for both in a pair)
			if clearedCheckpoint.chpHandle1 then DeleteCheckpoint(clearedCheckpoint.chpHandle1) end -- Shouldn't ever *not* exist :P
			if clearedCheckpoint.blpHandle1 then DeleteBlip(clearedCheckpoint.blpHandle1) end
			if clearedCheckpoint.chpHandle2 then DeleteCheckpoint(clearedCheckpoint.chpHandle2) end
			if clearedCheckpoint.blpHandle2 then DeleteBlip(clearedCheckpoint.blpHandle2) end

			-- Get the next checkpoint, if the just cleared checkpoint was the last checkpoint of the race, don't continue
			local currentCheckpoint = checkpoints[#checkpoints] or false
			if not currentCheckpoint then
				-- As a rule, sounds mean that the server acknowledged and accepted our action (where that makes contextual sense)
				PlaySoundFrontend(-1, "Checkpoint_Finish", "DLC_Stunt_Race_Frontend_Sounds", 0)
				return 
			end

			-- Getting here means there are checkpoints ahead, so play the ahead sound.
			PlaySoundFrontend(-1, "CHECKPOINT_AHEAD", "HUD_MINI_GAME_SOUNDSET", 1)

			-- Ensure the blip of the current checkpoint has the correct scale
			if currentCheckpoint.blpHandle1 then SetBlipScale(currentCheckpoint.blpHandle1, blipNextCHPScale) end
			if currentCheckpoint.blpHandle2 then SetBlipScale(currentCheckpoint.blpHandle2, blipNextCHPScale) end
		end
	end

	-- If in this update, we've been given new checkpoints to create, create them.
	if updateData.create and updateData.checkpoints then
		for _, checkpointData in ipairs(updateData.checkpoints) do
			table.insert(checkpoints, CreateCheckpointObject(checkpointData))
		end
	end
end)
