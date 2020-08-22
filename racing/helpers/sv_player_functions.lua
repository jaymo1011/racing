--[[
	Player Management Helpers
]]

-- Globals
Players = {}
PlayerIntentRefreshFrequency = 500
PlayerIntents = setmetatable({}, {
	__index = function(t, k)
		-- Return 0 for intents no one has
		if type(k) == "string" then
			return 0
		end

		return nil
	end,
})

-- Gets Player object while also hydrating the player state with stuff we want to put in there
local function GetPlayerWithHydratedState(playerId)
	-- For consistency, string playerIds are enforced.
	playerId = tostring(playerId) or false; if not playerId then return end

	-- Get the special Player object with the state bag wrapper.
	local player = Player(playerId)

	-- Set the hydrated flag
	player.state._RacingStateLoaded = true

	-- Return the object given from Player
	return player
end

RegisterNetEvent("playerJoining")
AddEventHandler("playerJoining", function()
	local source = tonumber(source); if not source then return end

	-- Touching a state bag directly as this event as fired will fail
	-- We need to wait as some internal even handlers need to process first before state bags are registered
	while GetPlayerPed(source) == 0 do Wait(100) end

	table.insert(Players, GetPlayerWithHydratedState(source))
end)

AddEventHandler("playerDropped", function()
	-- For consistency, string playerIds are enforced.
	local source = tostring(source) or false; if not source then return end

	-- Find the index of the dropped player in the Players table
	local index = false
	for i, ply in ipairs(Players) do
		-- Find which index contains the player in question
		if tostring(ply.__data) == source then
			index = i
			break
		end
	end

	-- If the player is in the table, remove them and we use table.remove so there are no gaps
	if index then table.remove(Players, index) else
		-- otherwise, print an error because this should never happen!
		printf("Player with id:%s, name:%s^7 was dropped before we knew about them, did they crash on connecting?", source, GetPlayerName(source) or "unknown")
	end
end)

--[[
	Player Intents
]]

-- Player intents refresh
local function RefreshPlayerIntents()
	-- Clear all intents
	for k in pairs(PlayerIntents) do
		PlayerIntents[k] = 0
	end

	-- Loop through all players
	for _, playerObj in ipairs(Players) do
		-- Grab from this player's state bag only once
		local thisPlayerIntent = playerObj.state.RacingIntent

		-- Players can't have an intent when the current map isn't loaded for them
		if playerObj.state.MissionJSONLoaded ~= GlobalState.CurrentMap or type(thisPlayerIntent) ~= "string" then
			thisPlayerIntent = "unknown"
		end
		
		-- Add 1 to the count of players with this intent
		PlayerIntents[thisPlayerIntent] = PlayerIntents[thisPlayerIntent] + 1
	end
end

-- Player Management Thread
CreateThread(function()
	-- Add all players already connected into the Players table
	for i, plyId in ipairs(GetPlayers()) do
		Players[i] = GetPlayerWithHydratedState(plyId)
	end

	while true do
		Wait(PlayerIntentRefreshFrequency)
		RefreshPlayerIntents()
	end
end)
