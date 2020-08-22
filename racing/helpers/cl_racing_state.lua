--[[
	Racing State Helper
	Just a quick wrapper to make stuff that requires the racing state not look really stretchy and constantly nag the state bags

	!IMPORTANT!
	It seems that setting your own state bag before the server has breaks things too :(
	Just when I thought things started to work out...

	(might be renamed at some point because I don't know how I feel of the name "racing state")
]]

local stateIsAvailable = LocalPlayer.state._RacingStateLoaded
local callbackQueue = {}

if not stateIsAvailable then
	CreateThread(function()
		while not LocalPlayer.state._RacingStateLoaded do Wait(200) end
		for _,cb in ipairs(callbackQueue) do cb() end
	end)
end

function OnStateAvailable(func)
	if stateIsAvailable then
		func()
	else
		table.insert(callbackQueue, func)
	end
end
