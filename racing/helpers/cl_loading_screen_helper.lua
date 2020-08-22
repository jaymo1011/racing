local loadingScreenHasShutDown = GetEntityModel(PlayerPedId()) ~= `player_zero` -- It seems like this works for the most part :D
local _ShutdownLoadingScreenNui = ShutdownLoadingScreenNui

function InvokeLoadingScreenEvent(eventName)
	if not loadingScreenHasShutDown then
		SendLoadingScreenMessage(json.encode({eventName = eventName}))
	end
end

function ShutdownLoadingScreenNui()
	_ShutdownLoadingScreenNui()
	loadingScreenHasShutDown = true
end
