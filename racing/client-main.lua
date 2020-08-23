local function SpawnPlayer()
	exports["spawnmanager"]:spawnPlayer({
		x = 2270.815,
		y = 3756.9,
		z = 38.5,	
		heading = 39.3,
		model = `a_m_y_skater_02`,
	}, function()
		OnStateAvailable(function()
			-- we'll wait for the player to set their own intent now :(
			--LocalPlayer.state:set("RacingIntent", "participate", true)
		
			-- Tell the loading screen to fade out to the game view
			InvokeLoadingScreenEvent("fadeOut")
			
			-- Just a little extra to account for the message time
			Wait(600)

			-- Shutdown the loading screen frame as we no longer need it
			ShutdownLoadingScreenNui()
		end)
	end)
end


AddEventHandler("onMissionJSONLoading", function()
	InvokeLoadingScreenEvent("mapLoading")
end)

AddEventHandler("onMissionJSONLoaded", function()
	InvokeLoadingScreenEvent("mapLoaded")

	-- Only ever trigger on the first load
	if GetEntityModel(PlayerPedId()) == `player_zero` then
		SpawnPlayer()
	end
end)

-- DEBUG: just makes things easier for now without the intent picker
RegisterCommand("intent", function(_, args, raw)
	local newIntent = args[1] and tostring(args[1]) or "participate" --"unknown"  
	LocalPlayer.state:set("RacingIntent", newIntent, true)
end)
