--[[
	Server-sided Racing Logic

	This file contains most of the code run by the server while a race is in progress
]]

-- Globals
RaceThreadActive = false

-- Locals
local RacePlayers = false
local RaceVehicles = false
local EntitySpawnTimeout = 5000

local function SetupRacePlayerTable()
	RacePlayers = {}

	for _, playerStateBag in ipairs(Players) do
		-- Only add players who intend to participate
		if playerStateBag.RacingIntent ~= "participate" then 
			table.insert(RacePlayers, {
				-- We want to save this so that it's not expensive to index a lot
				PlayerID = playerStateBag.state.PlayerID,
				StateBag = playerStateBag,
			})
		end
	end

	printf("Collected all players for this race")
end

-- TODO: move to separate helper
local function OnEntityCreated(entity, callback)
	local timeout = GetGameTimer() + EntitySpawnTimeout

	CreateThread(function()
		while not DoesEntityExist(entity) and timeout > GetGameTimer() do Wait(50) end

		callback()
	end)
end

local function SetupRaceVehicles()
	local vehSpawnLocation = CurrentMapUGC["mission"]["veh"]["loc"]
	local vehSpawnHeading = CurrentMapUGC["mission"]["veh"]["head"]
	RaceVehicles = {}

	for i, ply in ipairs(RacePlayers) do
		-- DEBUG: this is nowhere near finished yet.
		local vehLocation = vehSpawnLocation[i]; vehLocation = vec3(vehLocation.x, vehLocation.y, vehLocation.z)
		local vehHeading = (tonumber(vehSpawnHeading[i]) or 0.0) + 0.0
		local veh = CreateVehicle(`nero`, vehLocation, vehHeading, true, true) -- Yes, it IS a networked mission entity!
		SetEntityHeading(veh, vehHeading)
		OnEntityCreated(veh, function()
			Entity(veh).state.OwningPlayer = ply.PlayerID
			ply.StateBag.state.RaceVehicleNetworkId = veh
			
			local playerPed = GetPlayerPed(tonumber(ply.PlayerID))
			SetEntityCoords(playerPed, vehLocation)
			SetPedIntoVehicle(playerPed, veh, -1)
		end)
		
		table.insert(RaceVehicles, veh)
	end
end

function RaceThreadFunction()
	-- There can only be one race function at a time, do nothing if there is already one active
	if RaceThreadActive then return end
	RaceThreadActive = true

	-- We need to finalise who's participating in this race and who's not.
	SetupRacePlayerTable()

	-- Ensure we have map data
	if not CurrentMapUGC then error("some how, there was no UGC available for this race") end

	-- Give everyone a vehicle and place them into it
	SetupRaceVehicles()
	
	-- And now start the loop for the race
	while true do Wait(0)
		if not RaceThreadActive then return end

		-- Player Checkpointy checky things
	end

	-- Finally, we clean up the mess we made
	RacePlayers = false
	RaceVehicles = false -- TODO: clean up old vehicles
end
