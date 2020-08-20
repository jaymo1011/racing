--[[
	Player Management Helpers
]]
Players = {}

-- Gets Player object while also hydrating the player state with stuff we want to put in there
local function GetPlayerWithHydratedState(playerId)
	-- For consistency, string playerIds are enforced.
	playerId = tostring(playerId) or false; if not playerId then return end

	-- Get the special Player object with the state bag wrapper.
	local playerObject = Player(playerId)

	-- We need access to this later but playerObject has a metamethod preventing new indexes so... we bypass it! :D
	rawset(playerObject, "playerId", playerId)

	-- We don't need to re-hydrate the player if they already have the state variables
	if playerObject.state._RacingStateLoaded then return playerObject end

	-- Setup our state variables (only one right now :P)
	playerObject.state.RacingIntent = "unknown"

	-- Set the hydrated flag
	playerObject.state._RacingStateLoaded = true

	-- Return the object given from Player
	return playerObject
end

RegisterNetEvent("onPlayerJoining") -- why isn't this already an "always safe" net event???
AddEventHandler("onPlayerJoining", function()
	local source = source; if not source then return end

	-- table.insert isn't so bad here, this shouldn't happen too often
	table.insert(Players, GetPlayerWithHydratedState(source))
end)

AddEventHandler("playerDropped", function()
	-- For consistency, string playerIds are enforced.
	local source = tostring(source) or false; if not source then return end

	-- Find the index of the dropped player in the Players table
	local index = false
	for i, ply in ipairs(Players) do
		if rawget(ply, "playerId") == source then
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

-- Player intents
PlayerIntents = setmetatable({}, {
	__index = function(t, k)
		-- Return 0 for intents no one has
		if type(k) == "string" then
			return 0
		end

		return nil
	end,
})

function RefreshPlayerIntents()
	-- Clear all intents
	for k in pairs(PlayerIntents) do
		PlayerIntents[k] = 0
	end

	-- Loop through all players
	for _, playerObj in ipairs(Players) do
		-- Grab from this player's state bag only once
		local thisPlayerIntent = playerObj.state.RacingIntent

		-- Players can't have an intent when the current map isn't loaded for them
		if not playerObj.state.MissionJSONLoaded == GlobalState.CurrentMap or type(thisPlayerIntent) ~= "string" then
			thisPlayerIntent = "unknown"
		end
		
		-- Add 1 to the count of players with this intent
		PlayerIntents[thisPlayerIntent] = PlayerIntents[thisPlayerIntent] + 1
	end
end