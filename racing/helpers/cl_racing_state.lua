--[[
	Racing State Helper
	Just a quick wrapper to make stuff that requires the racing state not look really stretchy and constantly nag the state bags

	!IMPORTANT!
	It seems that setting your own state bag before the server has breaks things too :(
	Just when I thought things started to work out...

	(might be renamed at some point because I don't know how I feel of the name "racing state")
]]

--[[
	(Local) Player
]]

PlayerStateAvailable = LocalPlayer.state._RacingStateLoaded
local playerStateCallbackQueue = {}

if not PlayerStateAvailable then
	CreateThread(function()
		while not LocalPlayer.state._RacingStateLoaded do Wait(200) end
		PlayerStateAvailable = true
		for _,cb in ipairs(playerStateCallbackQueue) do cb() end
	end)
end

function OnStateAvailable(func)
	if PlayerStateAvailable then
		func()
	else
		table.insert(playerStateCallbackQueue, func)
	end
end

--[[
	Map
]]

CurrentMapAvailable = GlobalState.CurrentMapUGC and true or false

local function resetCurrentMapHelpers()
	CurrentMapUGC = false

	CurrentMapMissionData = false

	CurrentMapRaceData = false
end

local function populateCurrentMapHelpers()
	CurrentMapUGC = GlobalState.CurrentMapUGC

	CurrentMapMissionData = CurrentMapUGC["mission"]

	if CurrentMapMissionData then
		CurrentMapRaceData = CurrentMapMissionData["race"]
	end
end

if CurrentMapAvailable then
	populateCurrentMapHelpers()
else
	resetCurrentMapHelpers()
end

AddEventHandler("onClientMapStart", function(resource)
	CurrentMapAvailable = GlobalState.CurrentMapUGC and true or false

	if not CurrentMapAvailable then
		resetCurrentMapHelpers()
	end
end)

AddEventHandler("onMissionJSONLoaded", function()
	populateCurrentMapHelpers()
	CurrentMapAvailable = true
end)
