--[[
	Server-sided Racing Logic

	This file contains most of the code run by the server while a race is in progress
]]

-- Globals
RaceThreadActive = false

-- Locals
local RacePlayers = false
local RaceVehicles = false

local function SetupRacePlayerTable()
	RacePlayers = {}

	for _, player in ipairs(Players) do
		-- Only add players who intend to participate
		if player.state.RacingIntent == "participate" then 
			table.insert(RacePlayers, player)
		end
	end

	printf("Collected all players for this race %s", json.encode(RacePlayers))
end

local function SetupRaceVehicles()
	local vehSpawnLocation = CurrentMapUGC["mission"]["veh"]["loc"]
	local vehSpawnHeading = CurrentMapUGC["mission"]["veh"]["head"]
	RaceVehicles = {}

	for i, player in ipairs(RacePlayers) do
		-- DEBUG: this is nowhere near finished yet.
		local vehLocation = vehSpawnLocation[i]; vehLocation = vec3(vehLocation.x, vehLocation.y, vehLocation.z)
		local vehHeading = (tonumber(vehSpawnHeading[i]) or 0.0) + 0.0
	
		CreateServerVehicle(`sultanrs`, vehLocation, vehHeading, true, function(vehicle) -- Yes, it IS a networked mission entity!	
			local handle = vehicle.__data
			local networkId = NetworkGetNetworkIdFromEntity(handle)
			local playerPed = GetPlayerPed(player.__data)

			vehicle.state.OwningPlayer = player.__data

			vehicle.state.VehicleModProfile = {
				vehicleMods = {
					{
						type = 48,
						index = 7,
					}
				},

				-- Any guesses what the livery is? ;)
				numberPlateText = "WAIFU420"
			}

			player.state.RaceVehicleNetworkId = networkId

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
