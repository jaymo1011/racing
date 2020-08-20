CurrentResourceName = GetCurrentResourceName() -- Should just be left as "racing" but oh well...
FeaturesAvailable = false
MinPlayers = GetConvarInt("racing_minPlayers", 1)
PlayerLoadingTimeout = GetConvarInt("racing_playerLoadingTimeout", 10000)

-- Feature availability checks
if not Player or not Entity or not GlobalState then
	error(CurrentResourceName.." can only run on server artifact >=2844")

	-- 2844 includes
	-- - Global, Entity and Player State Bags
	-- - Entity Lockdown
	-- - Server-side entity persistence and owning
else
	FeaturesAvailable = true
end

--[[
	As of now, all of racing's internally used state variables begin with "racing_" in after, normal camelCase follows
]]

--TODO: Create some sort of output wrapper with string.format and so on...
printf("Setting up server state...")

-- Set lockdown mode to strict
Citizen.InvokeNative(`SET_SYNC_ENTITY_LOCKDOWN_MODE`, "strict")

-- Set up the global state
GlobalState.RacingGamemodeState = "starting"

printf("Server state setup done!")

local function GetNumParticipatingPlayers(p)
	local p = p or function() end
	local num = 0

	for _, playerObj in ipairs(Players) do
		p("processing player")
		p("player intent: ", playerObj.state.RacingIntent)
		p("player map: ", playerObj.state.MissionJSONLoaded)
		if playerObj.state.MissionJSONLoaded == GlobalState.CurrentMap and playerObj.state.RacingIntent == "participate" then
			p("yay")
			num = num + 1
		end
	end

	return num
end

RegisterCommand("ply", function()
	GetNumParticipatingPlayers(print)
end)

--[[
	Main resource thread
]]
CreateThread(function()
	if not FeaturesAvailable then return end
	
	-- Add all players already connected into the Players table
	for i, plyId in ipairs(GetPlayers()) do
		Players[i] = GetPlayerWithHydratedState(plyId)
	end

	local playerLoadingMaxTimeout = false

	-- All of our "OnTick" stuff
	while true do
		Wait(500)

		RefreshPlayerIntents()

		if GlobalState.RacingGamemodeState == "starting" then
			if GlobalState.CurrentMapMissionJSONChecked and GlobalState.CurrentMapMissionJSON then
				GlobalState.RacingGamemodeState = "started"
				printf("Gamemode Started!!!")
				goto continue
			end
		elseif GlobalState.RacingGamemodeState == "started" then
			-- If the connected players > ... ugh I'll finish this comment later
			if #Players >= MinPlayers and PlayerIntents["participate"] >= MinPlayers then
				GlobalState.RacingGamemodeState = "race:lineup"
				goto continue
			end
		elseif GlobalState.RacingGamemodeState == "race:lineup" then
			printf("now we drag everyone into the race, into their vehicles and then everyone is happy")
			return
		end

		::continue::
	end
end)
