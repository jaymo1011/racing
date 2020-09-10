--[[
	Checkpoint System
]]

-- Some forward declarations for the trigger thread
local setTriggerActive
local updateTriggerParameters

-- Constant control parameters
local checkpointAlpha = 180
local checkpointIconAlpha = 180

-- Turns checkpoint data from the server into a properly placed checkpoint.
local function CreateRaceCheckpoint(chpType, location, target, radius, baseHudColour, iconHudColour, cylinderHeight, chpSubType)
	-- Create the checkpoint
	local baseR, baseG, baseB = GetHudColour(baseHudColour)
	local iconR, iconG, iconB, iconA = GetHudColour(iconHudColour)
	local newChp = CreateCheckpoint(chpType, location.xyz, target.xyz, radius, baseR, baseG, baseB, checkpointAlpha, chpSubType)
	SetCheckpointCylinderHeight(newChp, cylinderHeight, cylinderHeight, 100.0)

	-- Do stuff special for round checkpoints
	if (16 > chpType and chpType > 11) then
		-- Special colours for round checkpoints
		iconA = 150
	else
		-- Clip checkpoints to the surface below them
		-- UNTESTED, hopefully this just works and I don't have to do the silly ray casting
		-- Also, the names are weird, I know 
		local found, groundZ, surfaceNormal = GetGroundZAndNormalFor_3dCoord(location.x, location.y, location.z)
		if found then
			local clipPlane = vec3(location.x, location.y, groundZ - 0.05)
			N_0xf51d36185993515d(newChp, clipPlane.xyz, surfaceNormal.xyz) -- lol soz for not documenting yet :P
		end
		
		iconA = checkpointIconAlpha
	end

	-- Set the colouring
	SetCheckpointIconRgba(newChp, iconR, iconG, iconB, iconA)

	-- Return the checkpoint
	return newChp
end

local function CreateCheckpointBlip(location)
	local blip = AddBlipForCoord(location)

	-- All defaults pulled from scripts
	SetBlipSprite(blip, 1)
	SetBlipColour(blip, 66)
	SetBlipScale(blip, 0.5)

	return blip
end

-- Checkpoint and blip handles, there will only be a max of 2 checkpoints and 4 blips, its best if we just store them discretely
local currentCheckpointIndex = -1
local chp1 = 0
local chp2 = 0
local chp1Blip = 0
local chp2Blip = 0
local chp1NextBlip = 0
local chp2NextBlip = 0

-- The server has control over what index we show and every time our index is set, the server also receives the index,
-- allowing it to send another set event or kick the player if they are misbehaving.
-- Side-note, as an anti-stupid-people mechanism, every time the server detects a discrepancy, you lose x amount of "trust" and if your trust reaches 0, you get kicked.
-- Your trust value is persistent across identifiers and recharges by x amount every race you complete without a discrepancy. (as you may get a false positive here and there if you're on a crap connection)
RegisterNetEvent("racing:checkpoints:setIndex")
AddEventHandler("racing:checkpoints:setIndex", function(index, sound)	
	-- Save the table from GlobalState for the entire time we need to access it
	local checkpoints = false

	-- Wait until checkpoints are available before continuing (or at max 5 seconds)
	local timeout = GetGameTimer() + 5000
	repeat
		checkpoints = GlobalState.RacingCheckpoints
		Wait(100)
	until checkpoints or GetGameTimer() >= timeout

	-- If we failed to get checkpoints, don't go any further
	if not checkpoints then return end

	-- Don't try to set up a checkpoint that doesn't exist
	local chp = checkpoints[index]
	if not chp then return end

	-- Don't set up the current checkpoint again
	-- TODO: The server might send this as a possible state reset, in that case, ensure everything is how it should be and not just ignore it
	if currentCheckpointIndex == index then return end

	-- Set up the checkpoint(s) and blip(s)
	local newCheckpoint = 0
	local newBlip1 = 0
	local newCheckpoint2 = 0
	local newBlip2 = 0

	newCheckpoint = CreateRaceCheckpoint(chp.type, chp.location, chp.target, chp.radius, chp.baseHudColour, chp.iconHudColour, chp.cylinderHeight, chp.chpSubType)
	mehLocation = chp.location
	if not chp.isLastCheckpoint then
		newBlip1 = CreateCheckpointBlip(chp.target)
	end

	if chp.isPair then
		newCheckpoint2 = CreateRaceCheckpoint(chp.pairType, chp.pairLocation, chp.pairTarget, chp.pairRadius, chp.baseHudColour, chp.iconHudColour, chp.cylinderHeight, chp.chpSubType)
		if not chp.isLastCheckpoint then
			newBlip2 = CreateCheckpointBlip(chp.pairTarget)
		end
	end

	-- Update the state of the other checkpoints and blips
	if chp1 ~= 0 then
		DeleteCheckpoint(chp1)
	end

	if newCheckpoint ~= 0 then
		chp1 = newCheckpoint
	end

	if chp2 ~= 0 then
		DeleteCheckpoint(chp2)
	end

	if newCheckpoint2 ~= 0 then
		chp2 = newCheckpoint2
	end

	if chp1Blip ~= 0 then
		RemoveBlip(chp1Blip)
	else
		-- This should only happen on the first checkpoint
		chp1NextBlip = CreateCheckpointBlip(chp.location)
	end

	if chp1NextBlip ~= 0 then
		chp1Blip = chp1NextBlip
		chp1NextBlip = newBlip1
	end

	if chp1Blip ~= 0 then
		SetBlipScale(chp1Blip, 0.7)
	end

	if chp2Blip ~= 0 then
		RemoveBlip(chp2Blip)
	elseif chp.isPair then
		chp2NextBlip = CreateCheckpointBlip(chp.pairLocation)
	end

	if chp2NextBlip ~= 0 then
		chp2Blip = chp2NextBlip
		chp2NextBlip = newBlip2
	end

	if chp2Blip ~= 0 then
		SetBlipScale(chp2Blip, 0.7)
	end
	
	-- Do sound related things
	-- As a "rule", sounds mean that the server acknowledged and accepted our action (where that makes contextual sense)
	if sound then
		--TODO: race specific soundsets	or even user configurable soundsets!!!
		PlaySoundFrontend(-1, "CHECKPOINT_NORMAL", "HUD_MINI_GAME_SOUNDSET", 1)
	end

	-- Update the trigger!
	setTriggerActive(true)
	updateTriggerParameters(chp.location, chp.radius/2, chp.isPair and chp.pairLocation or false, chp.isPair and chp.pairRadius/2 or false)
end)

-- Setup a function to kill all checkpoints
local function ClearAllRaceCheckpoints()
	if chp1 ~= 0 then DeleteCheckpoint(chp1) end
	if chp2 ~= 0 then DeleteCheckpoint(chp2) end
	if chp1Blip ~= 0 then RemoveBlip(chp1Blip) end
	if chp2Blip ~= 0 then RemoveBlip(chp2Blip) end
	if chp1NextBlip ~= 0 then RemoveBlip(chp1NextBlip) end
	if chp2NextBlip ~= 0 then RemoveBlip(chp2NextBlip) end
	currentCheckpointIndex = -1
	chp1 = 0
	chp2 = 0
	chp1Blip = 0
	chp2Blip = 0
	chp1NextBlip = 0
	chp2NextBlip = 0
	setTriggerActive(false)
end

-- The server can also tell us to clear any checkpoints that we are currently controlling
RegisterNetEvent("racing:checkpoints:clear")
AddEventHandler("racing:checkpoints:clear", ClearAllRaceCheckpoints)
AddEventHandler("onClientMapStop", ClearAllRaceCheckpoints)

-- Create a thread for "reliably" triggering checkpoints
CreateThread(function()
	-- Meh sue me for "variable abuse"
	local active = false
	local vehicle = false
	local extents = false
	local location1 = false
	local location2 = false
	local radius1 = false
	local radius2 = false
	local timeout = 0

	setTriggerActive = function(toggle)
		active = toggle or false
		 
		if not active then
			vehicle = false
			extents = false
			location1 = false
			location2 = false
			radius1 = false
			radius2 = false
			timeout = 0
		end
	end

	updateTriggerParameters = function(_location1, _radius1, _location2, _radius2)
		location1 = _location1
		location2 = _location2
		radius1 = _radius1
		radius2 = _radius2
		vehicle = GetVehiclePedIsIn(PlayerPedId(), false)

		if not vehicle then setTriggerActive(false) return end

		local min,max = GetModelDimensions(GetEntityModel(vehicle))
		extents = max --+ vec(0.5,0,0) 

		timeout = 0
	end

	local function IsInTrigger(chpDiff, radius)
		if #(chpDiff - extents) <= radius then
			return true
		end

		return false
	end

	local function FireTriggered()
		-- Tell the server we triggered a checkpoint
		TriggerServerEvent("racing:checkpoints:trigger")

		-- Set the current checkpoint(s) invisible now to make it feel smooooth
		local _chp1 = chp1
		if _chp1 then
			SetCheckpointRgba(_chp1, 0,0,0,0)
			SetCheckpointIconRgba(_chp1, 0,0,0,0)
		end
		
		local _chp2 = chp2
		if _chp2 then
			SetCheckpointRgba(_chp2, 0,0,0,0)
			SetCheckpointIconRgba(_chp2, 0,0,0,0)
		end

		-- Make a sound!
		

		timeout = GetGameTimer() + 5000
		while timeout > GetGameTimer() do Wait(10) end

		-- If checkpoints weren't changed, the server probably didn't reply or think we actually hit it, set the checkpoint scale back
		if _chp1 == chp1 then
			-- Since we can't actually save the rgba of the checkpoint (easily), we'll just set it back to the default yellow :D
			local r,g,b = GetHudColour(13)
			SetCheckpointRgba(_chp1, r, g, b, checkpointAlpha)
			local r,g,b = GetHudColour(134)
			SetCheckpointIconRgba(_chp1, r, g, b, checkpointIconAlpha)
		end

		if _chp2 == chp2 then
			local r,g,b = GetHudColour(13)
			SetCheckpointRgba(_chp2, r, g, b, checkpointAlpha)
			local r,g,b = GetHudColour(134)
			SetCheckpointIconRgba(_chp2, r, g, b, checkpointIconAlpha)
		end
	end
	
	while true do
		Wait(0)
		if active then
			local vehiclePos = GetEntityCoords(vehicle)

			if location1 and radius1 then
				if IsInTrigger(math.abs(GetOffsetFromEntityGivenWorldCoords(vehicle, location1.x, location1.y, vehiclePos.z)), radius1) then
					FireTriggered()
				elseif location2 and radius2 then
					if IsInTrigger(math.abs(GetOffsetFromEntityGivenWorldCoords(vehicle, location2.x, location2.y, location2.z)), radius2) then
						FireTriggered()
					end
				end
			end
		end
	end
end)
