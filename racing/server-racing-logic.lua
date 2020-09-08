--[[
	Server-sided Racing Logic

	This file contains most of the code run by the server while a race is in progress
]]

-- Globals
RaceThreadActive = false

-- Locals
local RacePlayers = false
local RaceVehicles = false
local RaceMap = false
local RaceCheckpoints = false
local NumRaceCheckpoints = false

local function SetupRacePlayerTable()
	RacePlayers = {}

	for _, player in ipairs(Players) do
		-- Only add players who intend to participate
		if player.state.RacingIntent == "participate" then 
			table.insert(RacePlayers, {id = player.__data, stateBag = player})
		end
	end

	printf("Collected all players for this race %s", json.encode(RacePlayers))
end

local function SetupRaceVehicles()
	local vehSpawnLocation = RaceMap["mission"]["veh"]["loc"]
	local vehSpawnHeading = RaceMap["mission"]["veh"]["head"]
	RaceVehicles = {}

	for i, player in ipairs(RacePlayers) do
		-- DEBUG: this is nowhere near finished yet.
		local vehLocation = vehSpawnLocation[i]
		local vehHeading = (tonumber(vehSpawnHeading[i]) or 0.0) + 0.0
	
		CreateServerVehicle(`zion3`, vehLocation, vehHeading, true, function(vehicle) -- Yes, it IS a networked mission entity!	
			local handle = vehicle.__data
			local networkId = NetworkGetNetworkIdFromEntity(handle)
			local playerPed = GetPlayerPed(player.id)

			vehicle.state.OwningPlayer = player.id

			vehicle.state.VehicleModProfile = {
				vehicleMods = {
					{
						type = 48,
						index = 5,
					}
				},

				-- Any guesses what the livery is? ;)
				numberPlateText = "WAIFU420"
			}

			player.stateBag.state.RaceVehicleNetworkId = networkId
			player.raceVehicle = handle

			while GetVehiclePedIsIn(playerPed) ~= handle do
				SetEntityCoords(playerPed, vehLocation)
				SetPedIntoVehicle(playerPed, handle, -1)
				Wait(500)
			end

			EnsureRaceVehicleHasCorrectMods(vehicle)

			table.insert(RaceVehicles, handle)
		end)
	end
end

local function SetPlayerCheckpoint(player, index, playSound)
	if not RaceCheckpoints then return end

	-- TODO: lap races!!!!
	if player.checkpoints == NumRaceCheckpoints then
		print("we have a bloody winner!!", GetPlayerName(player.id))
	end

	local chp = RaceCheckpoints[index]

	if chp then
		player.checkpoint = index
		player.triggerLocation1 = chp.location
		player.triggerRadius1 = chp.radius -- We're generous :P
		player.triggerLocation2 = chp.isPair and chp.pairLocation or false
		player.triggerRadius2 = chp.isPair and chp.pairRadius or false
		TriggerClientEvent("racing:checkpoints:setIndex", player.id, index, playSound)
	end
end

local function IsPlayerInTrigger(player, location, radius)
	if player and location and radius then
		local vehicle = player.raceVehicle
		if not vehicle or not DoesEntityExist(vehicle) then return false end

		return #(GetEntityCoords(vehicle).xy - location.xy) <= radius
	end

	return false
end

function RaceThreadFunction()
	-- There can only be one race function at a time, do nothing if there is already one active
	if RaceThreadActive then return end
	RaceThreadActive = true

	-- We need to finalise who's participating in this race and who's not.
	SetupRacePlayerTable()

	-- Ensure we have map data
	if not GlobalState.CurrentMapUGC then error("some how, there was no UGC available for this race") end
	RaceMap = GlobalState.CurrentMapUGC

	-- Give everyone a vehicle and place them into it
	SetupRaceVehicles()

	-- uhh checkpoints maybe???
	RaceCheckpoints = GetRaceCheckpoints(RaceMap.mission.race)
	NumRaceCheckpoints = #RaceCheckpoints -- Why continuously calculate something that will never change?!

	--local clientCheckpointData
	--RaceCheckpoints, clientCheckpointData = GetRaceCheckpoints(RaceMap.mission.race)
	for _, player in ipairs(RacePlayers) do
		-- Checkpoint 2 is the first checkpoint as checkpoint 1 is the "starting line"
		SetPlayerCheckpoint(player, 2, false)
	end
	
	-- And now start the loop for the race
	while true do Wait(0)
		if not RaceThreadActive then return end

		for _, player in ipairs(RacePlayers) do
			if IsPlayerInTrigger(player, player.triggerLocation1, player.triggerRadius1) or IsPlayerInTrigger(player, player.triggerLocation2, player.triggerRadius2) then
				SetPlayerCheckpoint(player, player.checkpoint + 1, true)
			end
		end
	end

	-- Finally, we clean up the mess we made
	RacePlayers = false
	RaceVehicles = false -- TODO: clean up old vehicles
	RaceMap = false
	RaceCheckpoints = false
	NumRaceCheckpoints = false
end

RegisterCommand("clearChp", function(source)
	TriggerClientEvent("racing:receiveCheckpointUpdate", source, json.encode({
		cleared = true,
	}))
end)
