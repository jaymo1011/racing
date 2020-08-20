-- Client should have access to statebags because, they just should :P
if not LocalPlayer then error("what????????") end

-- Add an event to handle new maps being loaded
AddEventHandler("onMissionJSONLoaded", function()
	-- DEBUG SPAWN MEMEING
	-- We'll do a lot more in terms of contextual things, intention based indication of loading, etc.
	if not GlobalState.CurrentMapUGC then return end
	local loadedUGCData = GlobalState.CurrentMapUGC.data
	local spawnLocation = loadedUGCData.mission.race.scene or false; spawnLocation = spawnLocation and vec3(spawnLocation.x, spawnLocation.y, spawnLocation.z) or vec3(345, 4842, -60)
	exports["spawnmanager"]:spawnPlayer({
		x = spawnLocation.x, y = spawnLocation.y, z = spawnLocation.z,
		heading = 39.3,
		model = `a_m_y_skater_02`,
	}, function()
		ShutdownLoadingScreenNui()
	end)
end)

-- Add our own handler for stuff we only want to execute when the player state is available
AddEventHandler("racing:playerStateAvailable", function()
	printf("yay lets do this!!!")

	-- yes, you will participate, you get no other option :D
	LocalPlayer.state:set("RacingIntent", "participate", true)
end)

-- Make sure that the player state is available before we do anything.
if LocalPlayer.state._RacingStateLoaded then
	TriggerEvent("racing:playerStateAvailable")
else
	CreateThread(function()
		while not LocalPlayer.state._RacingStateLoaded do Wait(200) end
		TriggerEvent("racing:playerStateAvailable")
	end)
end
