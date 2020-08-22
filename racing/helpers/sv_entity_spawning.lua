-- Stores all entities spawned by our resource to delete on resource stop
local spawnedEntities = {}

local function OnEntityCreated(entity, callback)
	CreateThread(function()
		local timeout = GetGameTimer() + GetConvarInt("racing_entityCreationTimeout", 5000)
		while not DoesEntityExist(entity) and timeout > GetGameTimer() do Wait(50) end
		spawnedEntities[entity] = true
		callback()
	end)
end

function CreateServerVehicle(model, position, heading, netMissionEntity, onCreationCallback)
	-- Actually create the vehicle
	local handle = CreateVehicle(model, position.xyz, heading, true, netMissionEntity)
	if handle == 0 then
		printf("Failed to create vehicle with model %s", model)
		return 0
	end

	-- Freeze the entity (for now)
	FreezeEntityPosition(handle, true)

	-- Set up our functions for when it's been created
	OnEntityCreated(handle, function()
		local vehicleStateBag = Entity(handle)

		-- Set the vehicle heading because the heading in the function doesn't work properly.
		SetEntityHeading(handle, heading)

		if onCreationCallback then
			pcall(onCreationCallback, vehicleStateBag)
		end

		-- Unfreeze the entity
		FreezeEntityPosition(handle, false)
	end)

	-- Return the handle to the vehicle
	return handle
end

-- Remove entities that were removed from our entity table
AddEventHandler("entityRemoved", function(entity)
	spawnedEntities[entity] = nil
end)

-- Delete all of our own entities when the gametype stops
AddEventHandler("onGameTypeStop", function(gametype)
	-- Should literally never happen, that's while we'll warn about it!
	if gametype ~= GetCurrentResourceName() then
		printf("The current gametype should be %s but %s was just stopped?!", GetCurrentResourceName(), gametype)
	end

	for entity in pairs(spawnedEntities) do
		DeleteEntity(entity)
	end
end)
