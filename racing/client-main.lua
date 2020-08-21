local firstSpawn = true

AddEventHandler("onMissionJSONLoading", function()
	if not firstSpawn then return end

	InvokeLoadingScreenEvent("mapLoading")
end)

-- Add an event to handle new maps being loaded
AddEventHandler("onMissionJSONLoaded", function()
	-- This is NOT how we will be spawning players and stuff, this is just for testing!
	-- We'll do a lot more in terms of contextual things, intention based indication of loading, etc.

	if not firstSpawn then return end

	InvokeLoadingScreenEvent("mapLoaded")

	if not GlobalState.CurrentMapUGC then return end

	local loadedUGCData = GlobalState.CurrentMapUGC.data
	local spawnLocation = loadedUGCData.mission.race.scene or false; spawnLocation = spawnLocation and vec3(spawnLocation.x, spawnLocation.y, spawnLocation.z) or vec3(345, 4842, -60)
	exports["spawnmanager"]:spawnPlayer({
		x = spawnLocation.x, y = spawnLocation.y, z = spawnLocation.z,
		heading = 39.3,
		model = `a_m_y_skater_02`,
	}, function()
		OnStateAvailable(function()
			-- yes, you will participate, you get no other option :D
			LocalPlayer.state:set("RacingIntent", "participate", true)
		end)

		-- Tell the loading screen to fade out to the game view
		InvokeLoadingScreenEvent("fadeOut")
		
		-- Just a little extra to account for the message time
		Wait(600)

		-- Shutdown the loading screen frame as we no longer need it
		ShutdownLoadingScreenNui()
	end)

	firstSpawn = false
end)
