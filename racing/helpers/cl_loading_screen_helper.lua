local loadingScreenHasShutDown = not GetIsLoadingScreenActive()
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
