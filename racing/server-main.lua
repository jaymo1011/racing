CurrentResourceName = GetCurrentResourceName() -- Should just be left as "racing" but oh well...
FeaturesAvailable = false
MinPlayers = GetConvarInt("racing_minPlayers", 1)
PlayerLoadingTimeout = GetConvarInt("racing_playerLoadingTimeout", 10000)
CurrentMapUGC = false

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

--TODO: Create some sort of output wrapper with string.format and so on...
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

		-- Custom threading things so that we don't poll a lot of things we shouldn't be
		if RaceThreadActive then
			-- Kill the race thread if we're not racing
			if GlobalState.RacingGamemodeState ~= "racing" then
				RaceThreadActive = false
			end

			if PlayerIntents["participate"] < MinPlayers then
				GlobalState.RacingGamemodeState = "waiting"
				RaceThreadActive = false
			end

			goto continue
		end

		if GlobalState.RacingGamemodeState == "starting" then
			if GlobalState.CurrentMapMissionJSONChecked and GlobalState.CurrentMapMissionJSON then
				GlobalState.RacingGamemodeState = "waiting"
				-- This is a good time to decode the MissionJSON
				CurrentMapUGC = ParseMissionJSON(GlobalState.CurrentMapMissionJSON)
				goto continue
			end
		elseif GlobalState.RacingGamemodeState == "waiting" then
			-- If the connected players and players who can participate are both greater than the minimum amount of players then start the race
			if #Players >= MinPlayers and PlayerIntents["participate"] >= MinPlayers then
				GlobalState.RacingGamemodeState = "racing"
				goto continue
			end
		elseif GlobalState.RacingGamemodeState == "racing" then
			printf("A race will now commence...")
			CreateThread(RaceThreadFunction)
			goto continue
		end

		::continue::
	end
end)
