CurrentResourceName = GetCurrentResourceName() -- Should just be left as "racing" but oh well...
FeaturesAvailable = false
MinPlayers = GetConvarInt("racing_minPlayers", 1)
PlayerLoadingTimeout = GetConvarInt("racing_playerLoadingTimeout", 10000)

-- Feature availability checks
if not Player or not Entity or not GlobalState then
	error(CurrentResourceName.." can only run on server artifact >=2844")

	-- 2844 includes
	--	Global, Entity and Player State Bags
	-- 	Entity Lockdown
	-- 	Server-side entity persistence and owning
else
	FeaturesAvailable = true
end

printf("Setting up server state...")

-- Set lockdown mode to strict
Citizen.InvokeNative(`SET_SYNC_ENTITY_LOCKDOWN_MODE`, "strict")

-- Set up the global state
GlobalState.RacingGamemodeState = "starting"

printf("Server state setup done!")

--[[
	Main resource thread
]]
CreateThread(function()
	if not FeaturesAvailable then return end

	local playerLoadingMaxTimeout = false -- TODO: add this back in...

	while true do
		Wait(500)

		-- Custom threading things so that we don't poll a lot of things we shouldn't be all the time
		if RaceThreadActive then
			-- Kill the race thread if we're not racing
			if GlobalState.RacingGamemodeState ~= "racing" then
				RaceThreadActive = false
			end

			if PlayerIntents["participate"] < MinPlayers then
				GlobalState.RacingGamemodeState = "waiting"
				RaceThreadActive = false
			end
		else
			if GlobalState.RacingGamemodeState == "starting" then
				if GlobalState.CurrentMapMissionJSONChecked and GlobalState.CurrentMapUGC then
					GlobalState.RacingGamemodeState = "waiting"
				end
			elseif GlobalState.RacingGamemodeState == "waiting" then
				-- If the connected players and players who can participate are both greater than the minimum amount of players then start the race
				if #Players >= MinPlayers and PlayerIntents["participate"] >= MinPlayers then
					GlobalState.RacingGamemodeState = "racing"
				end
			elseif GlobalState.RacingGamemodeState == "racing" then
				printf("A race will now commence...")
				CreateThread(RaceThreadFunction)
			end
		end
	end
end)

AddEventHandler("onMapStop", function()
	-- The map stopped, time to go back to the starting phase
	GlobalState.RacingGamemodeState = "starting"
end)

AddEventHandler("onGameTypeStop", function()
	-- We're being stopped, reset anything that we changed globally!
	Citizen.InvokeNative(`SET_SYNC_ENTITY_LOCKDOWN_MODE`, "inactive") -- If only there were a getter for this, we just have to assume it was inactive
	TriggerEvent("racing:shutdown")
	TriggerClientEvent("racing:shutdown", -1)
	-- TODO: clean up global and player state (I guess with some sort of registry system)
end)
