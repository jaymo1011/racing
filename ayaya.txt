AddTextEntry("FM_NXT_RACE_RULES", "Contact/Non-Contact")
AddTextEntry("FM_NXT_RACE_CONTACT", "Contact Race")

xnRace = xnRace or {}
xnRace.DEBUG = true
xnRace.STATE = 0 -- Stolen from that other race gamemode, thanks!
--[[
	ENUM

   -1 - Invalid State 	/ Drop Player
	0 - First Join 		/ Unsynced

	1 - In Lobby		/ Voting and selecting vehicle
	2 - In Race 		/ deja vu 	i have been in this place before	 higher on the street and i know its my time to goooooo
	3 - Finished 		/ Spectating
	4 - Transition		/ In the cut scene or transitioning from it to the next voting screen
]]--
function printf(text, ...) if xnRace.DEBUG then print(string.format(tostring(text),...)) end end

local retrivedPrefs = json.decode(GetResourceKvpString("xnRace:preferences") or "[]")
if retrivedPrefs ~= "[]" then
    printf("Loaded: %s", json.encode(retrivedPrefs))
    xnRace.Prefs = retrivedPrefs
else
    xnRace.Prefs = {
        -- Put all preferences here
        defaultRadio = 16,
        colour = 1,
        playerModel = 1,
    }
    SetResourceKvp("xnRace:preferences", json.encode(xnRace.Prefs))
end

Citizen.CreateThread(function()
	while true do
		Citizen.Wait(10000)
		SetResourceKvp("xnRace:preferences", json.encode(xnRace.Prefs))
	end
end)

RegisterCommand("forceSave", function()
    printf("Saving: %s.",json.encode(xnRace.Prefs))
    SetResourceKvp("xnRace:preferences", json.encode(xnRace.Prefs))
end)

function EnterClouds(overrideFlags, overrideSwitchType)
	if not IsPlayerSwitchInProgress() then
		SwitchOutPlayer(PlayerPedId(), overrideFlags or 0, overrideSwitchType or 1)
	end
end

function LeaveClouds()
	Citizen.CreateThread(function()
		if IsPlayerSwitchInProgress() then
			--while GetPlayerSwitchState() ~= x do Citizen.Wait(0) end
			N_0xd8295af639fd9cb8(PlayerPedId()) -- Need to document the native
		end
	end)
end

Citizen.CreateThread(function()
	while true do
		Citizen.Wait(0)
		if IsPlayerSwitchInProgress() then -- Stop various things from showing up over the player switch
			HideHudAndRadarThisFrame()
		end
	end
end)

RegisterCommand("clouds", function(_, args)
	if args[1] == "enter" then EnterClouds()
	elseif args[1] == "leave" then LeaveClouds()
	elseif args[1] == "force-leave" then StopPlayerSwitch() end
end)

xnRace.radioStations = {
	"RADIO_01_CLASS_ROCK",
	"RADIO_02_POP",
	"RADIO_03_HIPHOP_NEW",
	"RADIO_04_PUNK",
	"RADIO_06_COUNTRY",
	"RADIO_07_DANCE_01",
	"RADIO_08_MEXICAN",
	"RADIO_09_HIPHOP_OLD",
	"RADIO_11_TALK_02",
	"RADIO_12_REGGAE",
	"RADIO_14_DANCE_02",
	"RADIO_13_JAZZ",
	"RADIO_15_MOTOWN",
	"RADIO_20_THELAB",
	"RADIO_16_SILVERLAKE",
	"RADIO_17_FUNK",
	"RADIO_18_90S_ROCK",
	"RADIO_21_DLC_XM17",
	"RADIO_05_TALK_01",
	"RADIO_19_USER",
	"OFF",
	"FMMC_VEH_RAND",
}

Citizen.CreateThread(function()
	local frozenVehs = {}

	Citizen.CreateThread(function()
		while true do
			Citizen.Wait(0)
			for v,frozenpos in pairs(frozenVehs) do
				if frozenpos and IsEntityAVehicle(v) then
					SetVehicleForwardSpeed(v, 0.0) -- Freeze vehicles without actually freezing them..
				end
			end
		end
	end)

	function FreezeVehicle(v)
		frozenVehs[v] = GetEntityCoords(v, true)
		local model = GetEntityModel(v)
		if IsThisModelABike(model) then
			FreezeEntityPosition(v, true)
		end
	end

	function UnfreezeVehicle(v)
		if frozenVehs[v] then
			frozenVehs[v] = nil
			local model = GetEntityModel(v)
			if IsThisModelABike(model) then
				FreezeEntityPosition(v, false)
			end
		end
	end
end)

Citizen.CreateThread(function()
	local runtimer = false
	local starttime = GetGameTimer()
	local endtime = GetGameTimer()


	function ResetRaceTimer()
		starttime = GetGameTimer()
		endtime = false
	end

	function StartRaceTimer()
		ResetRaceTimer()
	end

	function StopRaceTimer()
		endtime = GetGameTimer()
	end

	function GetRaceTimer()
		return (endtime or GetGameTimer()) - starttime --GetTimeDifference(starttime, endtime and endtime or GetGameTimer())
	end
end)

Citizen.CreateThread(function()
	local sf = scaleform.Get("COUNTDOWN")
	while not scaleform.IsLoaded(sf) do Citizen.Wait(0) end

	Citizen.CreateThread(function()
		while true do
			Citizen.Wait(0)
			DrawScaleformMovieFullscreen(sf, 255, 255, 255, 255)
		end
	end)

	function DoCountdown(veh, soundset)
        printf("Soundset: %s",soundset)

        if soundset == "stunt" then
            PlaySoundFrontend(-1, "Countdown_3","DLC_Stunt_Race_Frontend_Sounds", 1)
    		scaleform.Call(sf, "FADE_MP", "3", 255,35,35)
    		Citizen.Wait(1000)

    		PlaySoundFrontend(-1, "Countdown_2","DLC_Stunt_Race_Frontend_Sounds", 1)
    		scaleform.Call(sf, "FADE_MP", "2", 255,35,35)
    		Citizen.Wait(1000)

    		PlaySoundFrontend(-1, "Countdown_1","DLC_Stunt_Race_Frontend_Sounds", 1)
    		scaleform.Call(sf, "FADE_MP", "1", 255,35,35)
            Citizen.Wait(100)

            PlaySoundFrontend(-1, "Countdown_Go","DLC_Stunt_Race_Frontend_Sounds", 1)
    		Citizen.Wait(900)
    		scaleform.Call(sf, "SET_MESSAGE", "GO!", 35,122,221, 1)
        else
    		PlaySoundFrontend(-1, "3_2_1", "HUD_MINI_GAME_SOUNDSET", 1)
    		scaleform.Call(sf, "FADE_MP", "3", 255,35,35)
    		Citizen.Wait(1000)

    		PlaySoundFrontend(-1, "3_2_1", "HUD_MINI_GAME_SOUNDSET", 1)
    		scaleform.Call(sf, "FADE_MP", "2", 255,35,35)
    		Citizen.Wait(1000)

    		PlaySoundFrontend(-1, "3_2_1", "HUD_MINI_GAME_SOUNDSET", 1)
    		scaleform.Call(sf, "FADE_MP", "1", 255,35,35)
    		Citizen.Wait(400)

    		PlaySoundFrontend(-1, "GO", "HUD_MINI_GAME_SOUNDSET", 1)
    		Citizen.Wait(600)
    		scaleform.Call(sf, "SET_MESSAGE", "GO!", 35,122,221, 1)
        end
		UnfreezeVehicle(veh)
		SetVehicleForwardSpeed(veh, 15.0)
		SetVehicleBurnout(veh, false)
		ResetRaceTimer()
		StartRaceTimer() -- Start the timer
		xnRace.STATE = 2
	end
end)
--[[Citizen.CreateThread(function()
	local sf = scaleform.Get("RACE_POSITION")
	while not scaleform.IsLoaded(sf) do Citizen.Wait(0) end

	Citizen.CreateThread(function()
		while true do
			Citizen.Wait(0)
			DrawScaleformMovieFullscreen(sf, 255, 255, 255, 255)
		end
	end)


	scaleform.Call(sf, "debug")
end)]]

local playersReady = {}
Citizen.CreateThread(function()
	local drawanything = false

	Citizen.CreateThread(function()
		while true do
			Citizen.Wait(0)
			if drawanything then
				local x = (0.7825) + (0.2 / 2)
		        local y = 0.025 +  (0.038 / 2)

				--Top bar
				DrawRect(x, y, 0.2, 0.038, 0, 0, 0, 220)

				--Top bar title
				SetTextFont(4)
				SetTextProportional(0)
				SetTextScale(0.45, 0.45)
				SetTextColour(255, 255, 255, 255)
				SetTextDropShadow(0, 0, 0, 0, 255)
				SetTextEdge(1, 0, 0, 0, 255)
				SetTextEntry("STRING")
				AddTextComponentString("Players Ready: 1/1")
				DrawText((x-0.1)+0.005, (y/2)+0.005)

				local addedplys = 1
				for i=1,#playersInRace do
					local ply = playersInRace[i]
					local ready = playersReady[ply]

					local r
					local g
					local b

					if ready then
						r = 33
						g = 88
						b = 33
					else
						r = 88
						g = 33
						b = 33
					end

					--Row BG
					DrawRect(x, y + (addedplys * 0.038), 0.2, 0.038, r, g, b, 220)

					--Name Label
					SetTextFont(4)
					SetTextScale(0.45, 0.45)
					SetTextColour(255, 255, 255, 255)
					SetTextEntry("STRING")
					AddTextComponentString(GetPlayerName(ply))
					DrawText((x-0.1)+0.005, (y/2 + (addedplys * 0.038))+0.005)
					addedplys = addedplys + 1
				end
			end
		end
	end)

	function DrawReadyList(bool)
		if type(bool) == "boolean" then drawanything = bool end
	end

	RegisterNetEvent("xnRace:updateReadyStatus")
	AddEventHandler("xnRace:updateReadyStatus", function(PlayerID,status)
		local ply = GetPlayerFromServerId(PlayerID)
		if ply and status ~= nil then
			playersReady[ply] = (status == true and true or nil)
		end
	end)
end)

Citizen.CreateThread(function()
	local DrawHudInfoThisFrame = false
	local DrawItems = {false,false,false}

	local var_pos = 200
	local startTime = GetGameTimer()
	local var_timer = 0
	local var_laps = 200

	Citizen.CreateThread(function()
		while true do
			Citizen.Wait(0)
			if DrawHudInfoThisFrame then
				local help = ""
				if DrawItems[1] then help = help.."Position:\t"..tostring(var_pos).."/"..tostring(#playersInRace).."      \t\t ~n~" end
				if DrawItems[2] then help = help.."Lap:\t\t"..tostring(var_laps).."/"..tostring(lap_amount).."~n~" end
				if DrawItems[3] then help = help.."Time:\t"..GetTimeAsString(var_timer) end
				BeginTextCommandDisplayHelp("STRING")
					AddTextComponentSubstringPlayerName(help)
				EndTextCommandDisplayHelp(0, 0, 0, -1)
			end
		end
	end)

	Citizen.CreateThread(function()
		while true do
			if DrawItems[3] then var_timer = GetTimeDifference(GetGameTimer(), startTime) end
			Citizen.Wait(0)
		end
	end)

	function UpdateHudInfo(position,laps,resetTime)
		if position ~= nil then
			if type(position) == "number" then
				var_pos = position
			end
		end
		if laps ~= nil then
			if type(laps) == "number" then
				var_laps = laps
			end
			if type(laps) == "boolean" then
				var_laps = var_laps+1
			end
		end
		if resetTime ~= nil then
			if type(resetTime) == "boolean" then
				startTime = GetGameTimer()
			end
		end
	end

	function SetHudInfoDraw(drawanything,pos,timer,laps)
		local returnValue = nil

		if drawanything ~= nil then
			if type(drawanything) == "boolean" then
				DrawHudInfoThisFrame = drawanything
			end
		end

		if pos ~= nil then
			if type(pos) == "boolean" then
				DrawItems[1] = pos
			end
		end

		if timer ~= nil then
			if type(timer) == "boolean" then
				if timer == false then
					local oldTime = 0
					if var_timer > 0 then oldTime = var_timer end
					returnValue = oldTime
				end
				DrawItems[3] = timer
			end
		end

		if laps ~= nil then
			if type(laps) == "boolean" then
				DrawItems[2] = laps
			end
		end

		return returnValue
	end
end)

function ShowRaceInfo(raceName)
    Citizen.CreateThread(function()
        local draw = true

        local sf = scaleform.Get("mp_celebration")
    	while not scaleform.IsLoaded(sf) do Citizen.Wait(0) end

    	local sf_bg = scaleform.Get("mp_celebration_bg")
    	while not scaleform.IsLoaded(sf_bg) do Citizen.Wait(0) end

    	local sf_fg = scaleform.Get("mp_celebration_fg")
    	while not scaleform.IsLoaded(sf_fg) do Citizen.Wait(0) end

    	Citizen.CreateThread(function()
    		while draw do
    			Citizen.Wait(0)
                --DrawScaleformMovieFullscreen(sf_bg, 255, 255, 255, 255)
                DrawScaleformMovieFullscreenMasked(sf_bg, sf_fg, 255, 255, 255, 255)
                DrawScaleformMovieFullscreen(sf, 255, 255, 255, 255)
    		end
    	end)

    	scaleform.Call(sf, "CREATE_STAT_WALL", "intro")
    	scaleform.Call(sf, "ADD_INTRO_TO_WALL", "intro", "FMMC_MPM_TY2",raceName,"","","","","",false,"HUD_COLOUR_GREYLIGHT")
    	scaleform.Call(sf, "ADD_BACKGROUND_TO_WALL", "intro", 70, 3)


    	scaleform.Call(sf_bg, "CREATE_STAT_WALL", "intro")
    	scaleform.Call(sf_bg, "ADD_INTRO_TO_WALL", "intro", "FMMC_MPM_TY2",raceName,"","","","","",false,"HUD_COLOUR_GREYLIGHT")
    	scaleform.Call(sf_bg, "ADD_BACKGROUND_TO_WALL", "intro", 70, 3)


    	scaleform.Call(sf_fg, "CREATE_STAT_WALL", "intro")
    	scaleform.Call(sf_fg, "ADD_INTRO_TO_WALL", "intro", "FMMC_MPM_TY2",raceName,"","","","","",false,"HUD_COLOUR_GREYLIGHT")
    	scaleform.Call(sf_fg, "ADD_BACKGROUND_TO_WALL", "intro", 70, 3)

    	scaleform.Call(sf_fg, "SHOW_STAT_WALL", "intro")
    	scaleform.Call(sf_bg, "SHOW_STAT_WALL", "intro")
    	scaleform.Call(sf,    "SHOW_STAT_WALL", "intro")

        PushScaleformMovieFunction(sf, "GET_TOTAL_WALL_DURATION")
            local ret = EndScaleformMovieMethodReturn()
            while not GetScaleformMovieFunctionReturnBool(ret) do Citizen.Wait(0) end
        local totalWallTime = GetScaleformMovieFunctionReturnInt(ret)
        Citizen.Wait(totalWallTime+1000)
        draw = false
        SetScaleformMovieAsNoLongerNeeded(sf)
        SetScaleformMovieAsNoLongerNeeded(sf_bg)
        SetScaleformMovieAsNoLongerNeeded(sf_fg)
    end)
end

function DoRaceOverMessage(place)
    Citizen.CreateThread(function()
        StartScreenEffect("MP_Celeb_Preload_Fade", 0, true)
        local draw = true

        local sf = scaleform.Get("mp_celebration")
    	while not scaleform.IsLoaded(sf) do Citizen.Wait(0) end

    	local sf_bg = scaleform.Get("mp_celebration_bg")
    	while not scaleform.IsLoaded(sf_bg) do Citizen.Wait(0) end

    	local sf_fg = scaleform.Get("mp_celebration_fg")
    	while not scaleform.IsLoaded(sf_fg) do Citizen.Wait(0) end

    	Citizen.CreateThread(function()
    		while draw do
    			Citizen.Wait(0)
                DrawScaleformMovieFullscreenMasked(sf_bg, sf_fg, 255, 255, 255, 255)
                DrawScaleformMovieFullscreen(sf, 255, 255, 255, 255)
    		end
    	end)

        local function ExecuteOnEndingWalls(func,...)
            scaleform.Call(sf,      func,"ending",...)
            scaleform.Call(sf_fg,   func,"ending",...)
            scaleform.Call(sf_bg,   func,"ending",...)
        end

        scaleform.Call(sf, "CREATE_STAT_WALL", "ending", "HUD_COLOUR_BLUE")
        scaleform.Call(sf_fg, "CREATE_STAT_WALL", "ending", "HUD_COLOUR_RED")
        scaleform.Call(sf_bg, "CREATE_STAT_WALL", "ending", "HUD_COLOUR_BLACK")
        if place then
            ExecuteOnEndingWalls("ADD_POSITION_TO_WALL", tonumber(place))
            ExecuteOnEndingWalls("ADD_TIME_TO_WALL", tonumber(GetRaceTimer() ~= 0 and GetRaceTimer() or 1000), "CELEB_TIME")
            ExecuteOnEndingWalls("ADD_WINNER_TO_WALL", (place == 1 and "CELEB_WINNER" or "CELEB_LOSER"), GetPlayerName(PlayerId()), "", 0, false, "", false)
        else
            ExecuteOnEndingWalls("ADD_POSITION_TO_WALL", "dnf")
            ExecuteOnEndingWalls("ADD_TIME_TO_WALL", tonumber(GetRaceTimer()), "CELEB_TIME")
            ExecuteOnEndingWalls("ADD_WINNER_TO_WALL", "CELEB_LOSER", GetPlayerName(PlayerId()), "", 0, false, "", false)
        end
        ExecuteOnEndingWalls("ADD_BACKGROUND_TO_WALL", 70, 4)

        PushScaleformMovieFunction(sf, "GET_TOTAL_WALL_DURATION")
            local ret = EndScaleformMovieMethodReturn()
            while not GetScaleformMovieFunctionReturnBool(ret) do Citizen.Wait(0) end
        local totalWallTime = GetScaleformMovieFunctionReturnInt(ret)

        ExecuteOnEndingWalls("SHOW_STAT_WALL")

        Citizen.Wait(totalWallTime+700)

        draw = false
        SetScaleformMovieAsNoLongerNeeded(sf)
        SetScaleformMovieAsNoLongerNeeded(sf_bg)
        SetScaleformMovieAsNoLongerNeeded(sf_fg)
        StartScreenEffect((place and (place == 1 and "MP_Celeb_Win_Out" or "MP_Celeb_Lose_Out") or "MP_Celeb_Lose_Out"), 0, false)
		EnterClouds()
		Citizen.Wait(1000)
		RenderScriptCams(0, 1, 1000, 0, 0)
    end)
end

SafeZone = { }
SafeZone.__index = SafeZone

SafeZone.Size = function() return GetSafeZoneSize() end

SafeZone.Left = function() return (1.0 - SafeZone.Size()) * 0.5 end
SafeZone.Right = function() return 1.0 - SafeZone.Left() end

SafeZone.Top = SafeZone.Left
SafeZone.Bottom = SafeZone.Right

Text = { }
Text.__index = Text

Text.Alignment = {
	Left = 1,
	Center = 2,
	Right = 3,
}

function Text.Draw(text, position, font, color, scale, outline, shadow, alignment, width)
	SetTextFont(font or 0)

	if not color then color = { r = 255, g = 255, b = 255, a = 255 } end
	SetTextColour(color.r, color.g, color.b, color.a)

	SetTextScale(scale or 1.0, scale or 1.0)

	SetTextProportional(false)

	if outline then SetTextOutline() end

	if shadow then
		SetTextDropShadow()
		SetTextDropshadow(2, 0, 0, 0, 255)
	end

	if alignment then
		if alignment == Text.Alignment.Center then SetTextCentre(true)
		elseif alignment == Text.Alignment.Right then SetTextRightJustify(true) end
	end

	if width then SetTextWrap(position.x - width, position.x)
	else SetTextWrap(SafeZone.Left(), position.x) end

	if type(text) == "number" then
		BeginTextCommandDisplayText('STRING')
			AddTextComponentSubstringTime(text, 2055)
		EndTextCommandDisplayText(position.x, position.y)
	else
		BeginTextCommandDisplayText('STRING')
			AddTextComponentSubstringPlayerName(text)
		EndTextCommandDisplayText(position.x, position.y)
	end
end

Bar = { }
Bar.__index = Bar

Bar.Width = 0.165
Bar.Height = 0.035

Bar.Texture = 'all_black_bg'
Bar.TextureDict = 'timerbars'

function Bar.DrawTextBar(title, text, index)
	RequestStreamedTextureDict(Bar.TextureDict)
	if not HasStreamedTextureDictLoaded(Bar.TextureDict) then return end

	if not IsPlayerSwitchInProgress() then

		HideHudComponentThisFrame(6)
		HideHudComponentThisFrame(7)
		HideHudComponentThisFrame(9)

		local index = index or 1
		local x = SafeZone.Right() - Bar.Width * 0.5
		local y = SafeZone.Bottom() - Bar.Height * 0.5 - (index - 1) * (Bar.Height + 0.0038) - 0.05
		--local y = SafeZone.Top() - Bar.Height * 0.5 + (index - 1) * (Bar.Height + 0.0038) + 0.1

		DrawSprite(Bar.TextureDict, Bar.Texture, x, y, Bar.Width, Bar.Height, 0.0, 255, 255, 255, 160)

		Text.Draw(title, { x = SafeZone.Right() - Bar.Width * 0.54, y = y - 0.01 }, false, false, 0.3, false, false, Text.Alignment.Right)
		Text.Draw(text, { x = SafeZone.Right() - 0.00285, y = y - 0.0165 }, 5, false, 0.425, false, false, Text.Alignment.Right)
		--Text.Draw(text, { x = SafeZone.Right() - 0.03385, y = y - 0.0165 }, 5, false, 0.425, false, false, Text.Alignment.Right)

	end
end

function Bar.DrawRespawnBar(title, index, value, bg, fg)
	RequestStreamedTextureDict(Bar.TextureDict)
	if not HasStreamedTextureDictLoaded(Bar.TextureDict) then return end

	HideHudComponentThisFrame(6)
	HideHudComponentThisFrame(7)
	HideHudComponentThisFrame(9)

	local width = Bar.Width
	local iBW = 0.06 -- innerBarWitdh
	local iBH = 0.01
	local height = 0.02
	local index = index or 1
	local x = SafeZone.Right() - width * 0.5
	local y = SafeZone.Bottom() - height * 0.5 - (index - 1) * (Bar.Height + 0.0038) - 0.05
	--local y = SafeZone.Top() - Bar.Height * 0.5 + (index - 1) * (Bar.Height + 0.0038) + 0.1

	DrawSprite(Bar.TextureDict, Bar.Texture, x, y, width, height, 0.0, 255, 255, 255, 160)

	--Text.Draw(title, { x = SafeZone.Right() - width * 0.54, y = y - 0.009 }, false, false, 0.3, false, false, Text.Alignment.Right)
	Text.Draw(title, { x = SafeZone.Right() - width * 0.54, y = y - 0.0105 }, false, false, 0.3, false, false, Text.Alignment.Right)
	DrawRect(SafeZone.Right() - 0.03385, y, iBW, iBH, table.unpack(bg))
	DrawRect(((SafeZone.Right() - 0.03385) - iBW/2) + (value*iBW)/2, y, value*iBW, iBH, table.unpack(fg))
	--Text.Draw(text, { x = SafeZone.Right() - 0.00285, y = y - 0.0165 }, 5, false, 0.425, false, false, Text.Alignment.Right)
    --Text.Draw(text, { x = SafeZone.Right() - 0.03385, y = y - 0.0165 }, 5, false, 0.425, false, false, Text.Alignment.Right)
end

Citizen.CreateThread(function()
    while true do
        Citizen.Wait(0)
        if GetRaceTimer() > 0 then
            Bar.DrawTextBar(GetLabelText("TIMER_TIME_RCE"), tonumber(GetRaceTimer()), 1)
			Bar.DrawTextBar("SERVER RECORD", 999999, 0)
        end
    end
end)


local function AddEvent(eventName, handler)
    RegisterNetEvent("xnRace:"..eventName)
    AddEventHandler("xnRace:"..eventName, handler)
end

Citizen.CreateThread(function()
	local EventObfuscator = ""
	RegisterNetEvent("xnRace:passEventObfuscator")
	AddEventHandler("xnRace:passEventObfuscator", function(newobf)
		EventObfuscator = newobf
	end)
	TriggerServerEvent("xnRace:getEventObfuscator")

	while EventObfuscator == "" do Citizen.Wait(0) end

	function ServerEvent(eventName,...)
		TriggerServerEvent(EventObfuscator..":"..eventName, ...)
	end
end)

AddEvent("startRace", function(raceType)
	DoCountdown(xnRace.RACEVEH, raceType)
end)

local speedUpObjects = {
	[GetHashKey("stt_prop_track_speedup")] = {100,0.5}, -- Single
	[GetHashKey("stt_prop_track_speedup_t1")] = {25,0.3}, -- Quad
	[GetHashKey("stt_prop_track_speedup_t2")] = {45,0.4}, -- Triple
	[GetHashKey("stt_prop_stunt_tube_speed")] = {35,0.4},
	[GetHashKey("stt_prop_stunt_tube_speedb")] = {45,0,5},
	[GetHashKey("ar_prop_ar_speed_ring")] = {100,0.5},
	[GetHashKey("ar_prop_ar_tube_speed")] = {45,0.3},
	[GetHashKey("ar_prop_ar_tube_2x_speed")] = {45,0.4},
	[GetHashKey("ar_prop_ar_tube_4x_speed")] = {100,0.5},
}
local slowDownObjects = {
	[GetHashKey("gr_prop_gr_target_1_01a")] = 16,
	[GetHashKey("gr_prop_gr_target_2_04a")] = 16,
	[GetHashKey("gr_prop_gr_target_3_03a")] = 16,
	[GetHashKey("gr_prop_gr_target_4_01a")] = 16,
	[GetHashKey("gr_prop_gr_target_5_01a")] = 16,
	[GetHashKey("gr_prop_gr_target_small_01a")] = 16,
	[GetHashKey("gr_prop_gr_target_small_03a")] = 16,
	[GetHashKey("gr_prop_gr_target_small_02a")] = 16,
	[GetHashKey("gr_prop_gr_target_small_06a")] = 16,
	[GetHashKey("gr_prop_gr_target_small_07a")] = 16,
	[GetHashKey("gr_prop_gr_target_small_04a")] = 16,
	[GetHashKey("gr_prop_gr_target_small_05a")] = 16,
	[GetHashKey("gr_prop_gr_target_long_01a")] = 16,
	[GetHashKey("gr_prop_gr_target_large_01a")] = 16,
	[GetHashKey("gr_prop_gr_target_trap_01a")] = 16,
	[GetHashKey("gr_prop_gr_target_trap_02a")] = 16,
	[GetHashKey("stt_prop_track_slowdown")] = 46,
	[GetHashKey("stt_prop_track_slowdown_t1")] = 30,
	[GetHashKey("stt_prop_track_slowdown_t2")] = 16,
}
local LoadedMap = false
AddEvent("loadMapObjects", function(name,map)
	--DrawReadyList(true)
    --DoScreenFadeOut(500)
	Citizen.Wait(600)
    BeginTextCommandBusyString("STRING")
        AddTextComponentSubstringPlayerName("Loading Map Objects")
    EndTextCommandBusyString(2)
	printf("mapobjloading")
    if LoadedMap and LoadedMap.loadedObjects then
        for i,object in ipairs(LoadedMap.loadedObjects) do
            DeleteObject(object)
        end
    end

    LoadedMap = {mapName=name,mapData=map,loadedObjects={}}
    for i=1,#map do
        RequestModel(map[i]["hash"])
        while not HasModelLoaded(map[i]["hash"]) do
            Citizen.Wait(0)
        end

        local obj = CreateObjectNoOffset(map[i]["hash"], map[i]["x"], map[i]["y"], map[i]["z"], false, true, false)
        FreezeEntityPosition(obj, true)
        SetEntityRotation(obj, map[i]["rot"]["x"], map[i]["rot"]["y"], map[i]["rot"]["z"], 2, 0)

		-- If you have a better way to do this then let me know
		if speedUpObjects[map[i]["hash"]] then
			--AddSpeedZoneForCoord(map[i]["x"], map[i]["y"], map[i]["z"], 6.0, 0.0, true)
			N_0x96ee0eba0163df80(obj,100)
			N_0xdf6ca0330f2e737b(obj,0.5)
		end

		if slowDownObjects[map[i]["hash"]] then
		    --AddSpeedZoneForCoord(map[i]["x"], map[i]["y"], map[i]["z"], 6.0, 0.0, true)
			N_0x96ee0eba0163df80(obj,0.3)
		end


        if map[i]["prpclr"] ~= nil then
            SetObjectTextureVariant(obj, map[i]["prpclr"])
        end
        LoadedMap.loadedObjects[i] = obj
    end
	ServerEvent("clientMapLoaded")
    RemoveLoadingPrompt()
	printf("maploaded")
end)

AddEvent("spawnOnTrack", function(sd,vehicleData, raceName)
	SetFrontendActive(false)	
    --DoScreenFadeOut(500)
	Citizen.Wait(200)
    RenderScriptCams(0, 0, 10, 0, 0)
	xnRace.VEHICLEDATA = vehicleData
	while GetPlayerSwitchState() < 5 do Citizen.Wait(0) end
	TriggerEvent("raceSpawn", sd, raceName, true)
end)

-- Checkpoint Displayer
Citizen.CreateThread(function()
	local waitingForResponse = true
	xnRace.CHECKPOINTS = {}
	local currentCheckpoints = xnRace.CHECKPOINTS

	-- Shitty code for colours...
	local cpcs = {}
	local r,g,b,a = GetHudColour(13)
	cpcs[1] = {r,g,b,a}
	r,g,b,a = GetHudColour(134)
	cpcs[2] = {r,g,b,a}
	r,g,b,a = nil,nil,nil,nil

	AddEvent("checkpoints:response", function(resType, response)
		printf("Got checkpoint response with type of "..resType.." and the following response data.\n"..json.encode(response))
		printf("We "..(waitingForResponse and "were" or "were NOT").." expecting a response right now.")
		if waitingForResponse then
			if resType == "firstCPS" then
				xnRace.CHECKPOINTS = {}
				currentCheckpoints = xnRace.CHECKPOINTS
				for i,cpData in ipairs(response) do
					local posT,head,scale,isRound = table.unpack(cpData)

					if i ~= 3 then -- Only 2 checkpoints are shown at any given time but we need the position of the next checkpoint to create the previous one
						local pos = vec(table.unpack(posT))
						local nextpos = vec(table.unpack(response[i+1][1])) - vec(0,0,response[i+1][1][3]*2)
						local cpType = 5
						if isRound then cpType = 10 end

						local cpHandle = CreateCheckpoint(cpType, pos + vec(0,0,isRound and 10.5 or 5.0), nextpos, 10.0, cpcs[1][1], cpcs[1][2], cpcs[1][3], cpcs[1][4], 0)
						SetCheckpointCylinderHeight(cpHandle, 16.0, 16.0, isRound and 10.5 or 5.0)
						SetCheckpointIconRgba(cpHandle, cpcs[2][1], cpcs[2][2], cpcs[2][3], cpcs[2][4])

						local cpBlipHandle = AddBlipForCoord(pos)
						SetBlipSprite(cpBlipHandle, 1)
						SetBlipColour(cpBlipHandle, 66)
						SetBlipScale(cpBlipHandle, (i == 2 and 0.5 or 1.0))

						currentCheckpoints[i] = {cpHandle, cpBlipHandle, cpData}
					end
				end
			elseif resType == "OK" then
				-- Delete the current checkpoint
				local curCPHandle = currentCheckpoints[1][1]
				local curBlipHandle = currentCheckpoints[1][2]
				DeleteCheckpoint(curCPHandle)
				RemoveBlip(curBlipHandle)

				-- `Push` the current checkpoint out of the table
				xnRace.LASTCHECKPOINT = currentCheckpoints[1]
				table.remove(currentCheckpoints, 1)

				if currentCheckpoints[1] then
					-- Set the now current checkpoint's scale to 1
					SetBlipScale(currentCheckpoints[1][2],1.0)

					-- Create the next checkpoint
					if response[1] and response[1] ~= "FINISH" then
						if response[2] then
							local posT,head,scale,isRound = table.unpack(response[1])
							local pos = vec(table.unpack(posT))
							local nextpos = vec(table.unpack(response[2][1]))
							local cpType = 5
							if isRound then cpType = 10 end

							local cpHandle = CreateCheckpoint(cpType, pos + vec(0,0,isRound and 10.5 or 5.0), nextpos, (isRound and 21.0 or 10.0), cpcs[1][1], cpcs[1][2], cpcs[1][3], cpcs[1][4], 0)
							SetCheckpointCylinderHeight(cpHandle, 16.0, 16.0, isRound and 10.5 or 5.0)
                            --[[ Stuff possibly to do with transform races (they do not exist in the current version of fivem >:( )
                                GRAPHICS::_0xDB1EA9411C8911EC(uParam0->f_5030);
					            GRAPHICS::_0x3C788E7F6438754D(uParam0->f_5030, vVar6 + func_1549(func_1250(iParam2, bParam4), 0f, func_1550(iParam2, bParam4)));
                            ]]
							SetCheckpointIconRgba(cpHandle, cpcs[2][1], cpcs[2][2], cpcs[2][3], cpcs[2][4])

							local cpBlipHandle = AddBlipForCoord(pos)
							SetBlipSprite(cpBlipHandle, 1)
							SetBlipColour(cpBlipHandle, 66)
							SetBlipScale(cpBlipHandle, 0.5)

							currentCheckpoints[2] = {cpHandle, cpBlipHandle, response[1]}
						else
							local posT,head,scale,isRound = table.unpack(response[1])
							local pos = vec(table.unpack(posT))
							local cpType = 9

							local cpHandle = CreateCheckpoint(cpType, pos + vec(0,0,isRound and 10.5 or 5.0), vec(0,0,0), isRound and 20.0 or 10.0, cpcs[1][1], cpcs[1][2], cpcs[1][3], cpcs[1][4], 0)
							SetCheckpointCylinderHeight(cpHandle, 16.0, 16.0, isRound and 10.5 or 5.0)
							SetCheckpointIconRgba(cpHandle, cpcs[2][1], cpcs[2][2], cpcs[2][3], cpcs[2][4])

							local cpBlipHandle = AddBlipForCoord(pos)
							SetBlipSprite(cpBlipHandle, 1)
							SetBlipColour(cpBlipHandle, 66)
							SetBlipScale(cpBlipHandle, 0.5)

							currentCheckpoints[2] = {cpHandle, cpBlipHandle, response[1]}
						end
					end
				elseif response[1] == "FINISH" then
					StopRaceTimer()
					PlaySoundFrontend(-1, "Checkpoint_Finish", "DLC_Stunt_Race_Frontend_Sounds", 0)
					SetTimeout(1000, function() waitingForResponse = true end)

                    Citizen.CreateThread(function()
                        if response[2] then
                            local x,y,z = table.unpack(response[2][1])
                            local xr,yr,zr = table.unpack(response[2][2])

                            local finishCam = CreateCam("DEFAULT_SCRIPTED_CAMERA", true)
                            SetCamNearClip(finishCam, 0.5)
                            SetCamCoord(finishCam, x,y,z) -- camf
                            SetCamRot(finishCam, xr,yr,zr, 2)
                            RenderScriptCams(1, 0, 10, 0, 0)
                        end

                        SetVehicleForwardSpeed(xnRace.RACEVEH, GetVehicleMaxSpeed(xnRace.RACEVEH) / 3.0)
                        SetAirDragMultiplierForPlayersVehicle(PlayerId(), 15.0)

                        StartScreenEffect("CrossLine", 0, 1)
                        Citizen.Wait(2000)
                        StopScreenEffect("CrossLine")
                        StartScreenEffect("CrossLineOut", 0, 0)
						DestroyCam(finishCam)
						ResetRaceTimer()
                    end)
				end
			elseif resType == "1D10T_D3T3CT3D" then
				-- HEY STOP HACKING
				print("hacker.")
			elseif resType == "ERR" then
				printf("unknown server error on checkpoints")
			else
				printf("invalid checkpoint response from server!\n\t"..tostring(resType))
			end

			waitingForResponse = false
		end
	end)

	Citizen.CreateThread(function()
		while true do
			Citizen.Wait(0)
			if not waitingForResponse and currentCheckpoints[1] then
				local radius = (currentCheckpoints[1][3][4] and 17.5 or 7.5)*currentCheckpoints[1][3][3]
				if Vdist2(GetEntityCoords(xnRace.RACEVEH, true), (vec(table.unpack(currentCheckpoints[1][3][1]))) + (currentCheckpoints[1][3][4] and vec(0,0,1.5) or vec(0,0,0))) < (radius^2) then
                    local cp = currentCheckpoints[1][1]
                    ServerEvent("checkpoints:check", currentCheckpoints[1][3][5])
                    -- play the sound and make the checkpoint invisible to make it feel snappy
                    PlaySoundFrontend(-1, "CHECKPOINT_AHEAD", "HUD_MINI_GAME_SOUNDSET", 1)
                    SetCheckpointRgba(cp, cpcs[1][1], cpcs[1][2], cpcs[1][3], 0)
                    SetCheckpointIconRgba(cp, cpcs[2][1], cpcs[2][2], cpcs[2][3], 0)
                    -- make the checkpoint visible after half a second in case the server told us we did something wrong
                    SetTimeout(500, function()
                        SetCheckpointRgba(cp, cpcs[1][1], cpcs[1][2], cpcs[1][3], cpcs[1][4])
                        SetCheckpointIconRgba(cp, cpcs[2][1], cpcs[2][2], cpcs[2][3], cpcs[2][4])
                    end)
					waitingForResponse = true
				end
			end
		end
	end)
end)

AddEvent("raceOverMessage", function(place)
	DoRaceOverMessage(place)
end)

Citizen.CreateThread(function()
	local sf = scaleform.Get("MP_NEXT_JOB_SELECTION")
	while not scaleform.IsLoaded(sf) do Citizen.Wait(0) end
	local currentVoteOptions = {}
	local draw = false
	local curTotalPlayers = 0

	local rtxd = CreateRuntimeTxd("xnRaceImages")
	local currentHoverIndex = 0
	local currentVote = -1

	local function SetDetailsItem(rating, createdBy, jobType, areaLabel)
		-- Rating
		PushScaleformMovieFunction(sf, "SET_DETAILS_ITEM")
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)

			BeginTextCommandScaleformString("FM_NXT_RAT")
			EndTextCommandScaleformString()

			BeginTextCommandScaleformString("FM_NXT_RAT1")
				AddTextComponentInteger(rating)
			EndTextCommandScaleformString()
		EndScaleformMovieMethod()

		-- Creator
		PushScaleformMovieFunction(sf, "SET_DETAILS_ITEM")
			PushScaleformMovieMethodParameterInt(1)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(3)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)

			BeginTextCommandScaleformString("FM_NXT_CRE")
			EndTextCommandScaleformString()

			PushScaleformMovieMethodParameterButtonName(createdBy)

			BeginTextCommandScaleformString("")
			EndTextCommandScaleformString()

			PushScaleformMovieMethodParameterBool(true)
		EndScaleformMovieMethod()

		-- Players
		PushScaleformMovieFunction(sf, "SET_DETAILS_ITEM")
			PushScaleformMovieMethodParameterInt(2)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)

			BeginTextCommandScaleformString("FM_NXT_RACE_RULES")
			EndTextCommandScaleformString()

			if curTotalPlayers > 32 then
				BeginTextCommandScaleformString("SCTV_T_NCR")
				EndTextCommandScaleformString()
			else
				BeginTextCommandScaleformString("FM_NXT_RACE_CONTACT")
				EndTextCommandScaleformString()
			end

			PushScaleformMovieMethodParameterBool(true)
		EndScaleformMovieMethod()

		-- Job
		local jobTypes = { -- replacement for the switch statement
			[0] = {"FMMC_MPM_TY0",0},
			{"FMMC_RSTAR_TDM",4},
			{"FMMC_MPM_TY1",1},
			{"FMMC_MPM_TY2",2},
			{"FMMC_MPM_TY4",3},
		}

		PushScaleformMovieFunction(sf, "SET_DETAILS_ITEM")
			PushScaleformMovieMethodParameterInt(3)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(2)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)

			BeginTextCommandScaleformString("FM_NXT_TYP")
			EndTextCommandScaleformString()

			local jobLabel = "FMMC_MPM_TY2"
			if jobTypes[jobType[1]] then jobLabel = jobTypes[jobType[1]] end
			BeginTextCommandScaleformString(jobLabel)
			EndTextCommandScaleformString()

			PushScaleformMovieFunctionParameterInt(jobType[2])
			PushScaleformMovieFunctionParameterInt(9)
			PushScaleformMovieFunctionParameterBool(false)
		EndScaleformMovieMethod()

		-- Area
		PushScaleformMovieFunction(sf, "SET_DETAILS_ITEM")
			PushScaleformMovieMethodParameterInt(4)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(0)

			BeginTextCommandScaleformString("FM_NXT_ARA")
			EndTextCommandScaleformString()

			BeginTextCommandScaleformString(areaLabel)
			EndTextCommandScaleformString()

			PushScaleformMovieMethodParameterBool(true)
		EndScaleformMovieMethod()
	end

	local function updateSelection()
		if currentHoverIndex > 8 then currentHoverIndex = 8 end
		if currentHoverIndex < 0 then currentHoverIndex = 0 end

		if currentHoverIndex >= 0 and currentHoverIndex <= 5 then
			local ItemName,ItemDesc,_,_,_,ItemDetails = table.unpack(currentVoteOptions[currentHoverIndex+1])
			scaleform.Call(sf, "SET_SELECTION", currentHoverIndex, ItemName, {tc="FMMC_OFFLN_HD",values=ItemDesc}, 0)
			SetDetailsItem(table.unpack(ItemDetails))
			--SetDetailsItem(69, "Jaymo", 0, "FM_NXT_ARA")
		elseif currentHoverIndex == 6 then
			scaleform.Call(sf, "SET_SELECTION", currentHoverIndex, "Replay", "Replay the previous race", 0)
		elseif currentHoverIndex == 7 then
			scaleform.Call(sf, "SET_SELECTION", currentHoverIndex, "Refresh", "Don't like the choices? Show some more", 0)
		elseif currentHoverIndex == 8 then
			scaleform.Call(sf, "SET_SELECTION", currentHoverIndex, "Random", "Select a random race", 0)
		end
	end

	local function RequestTXD(txd)
		RequestStreamedTextureDict(txd)
		return HasStreamedTextureDictLoaded(txd)
	end

	local function displayVotes(voteTable, updatedIndex, curTotalPlayers)
		for i=0,8 do scaleform.Call(sf, "SET_GRID_ITEM_VOTE", i, 0, 0, false, false) end

		local numTotalVotes = 0
		for index,votes in ipairs(voteTable) do
			local flash = index == updatedIndex
			local isMyVote = false
			local numVotes = 0

			for player,didVote in pairs(votes) do
				if player == GetPlayerServerId(PlayerId()) then isMyVote = true end
				if didVote then numVotes = numVotes + 1 end
				numTotalVotes = numTotalVotes + numVotes
			end
			scaleform.Call(sf, "SET_GRID_ITEM_VOTE", index-1, numVotes, isMyVote and 18 or 0, isMyVote, flash)
		end

		scaleform.Call(sf, "SET_TITLE", "Vote for the next Race", tostring(numTotalVotes).."/"..tostring(curTotalPlayers).." Votes")
	end
	AddEvent("displayVotes", function(voteTable, updatedIndex, curTotalPlayers)
		displayVotes(voteTable, updatedIndex, curTotalPlayers)
		printf("Updated Votes")
	end)

	local function DoNextVote(voteOptions, timeout, totalPlayers)
		selType = false
		currentVoteOptions = voteOptions
		curTotalPlayers = totalPlayers
		local currentHoverIndex = 0
		local currentVote = -1
		scaleform.Call(sf, "CLEANUP_MOVIE") -- Clean Scaleform
		scaleform.Call(sf, "SET_TITLE", "Vote for the next Race", "0/"..tostring(totalPlayers).." Votes") -- Racename

		RequestStreamedTextureDict("xnraceimg")
		while not HasStreamedTextureDictLoaded("xnraceimg") do Citizen.Wait(0) end

		RequestStreamedTextureDict("CommonMenu")
		while not HasStreamedTextureDictLoaded("CommonMenu") do Citizen.Wait(0) end

		RequestStreamedTextureDict("MPLeaderboard")
		while not HasStreamedTextureDictLoaded("MPLeaderboard") do Citizen.Wait(0) end

		RequestStreamedTextureDict("MPCarHUD")
		while not HasStreamedTextureDictLoaded("MPCarHUD") do Citizen.Wait(0) end

		for i=1,6 do
			local MapName,MapDesc,ImageNo,MapType,ImgUrl = table.unpack(voteOptions[i])

			--number, sTitle, sTXD, sTXN, textureLoadType, verifiedType, ModeType, HasYouDoneThisBefore, RPMultiplier(float), CashMultiplier(float), bDisabled, iconCol
			scaleform.Call(sf, "SET_GRID_ITEM", i - 1, MapName, "xnraceimg", tostring(ImageNo), 1, 0, MapType, 0, 0, 0, 0, 0) -- Voteicon
		end

		scaleform.Call(sf, "SET_GRID_ITEM", 6, "Replay", -1, -1, -1, -1, -1, 0, -1, -1, 0, -1) -- Replay
		scaleform.Call(sf, "SET_GRID_ITEM", 7, "Refresh", -1, -1, -1, -1, -1, 0, -1, -1, 0, -1) -- Refresh Choices
		scaleform.Call(sf, "SET_GRID_ITEM", 8, "Random", -1, -1, -1, -1, -1, 0, -1, -1, 0, -1) -- Random Vote

		SetDetailsItem(69, "Jaymo", {0,0}, "FM_NXT_ARA")

		--scaleform.Call(sf, "SET_SELECTION", 0, "", "", 0)

		SetStreamedTextureDictAsNoLongerNeeded("xnraceimg")
		updateSelection()
		draw = true
		xnRace.STATE = 1
	end
	AddEvent("doMapVote", function(voteOptions, timeout, totalPlayers)
		DoNextVote(voteOptions, timeout, totalPlayers)
	end)
	AddEvent("stopMapVote", function()
		draw = false
	end)

	local function GoSelect()
		PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false)
		ServerEvent("networkVote", currentHoverIndex)
	end

	local function GoDown()
		PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false)
		local newval = currentHoverIndex + 3
		if newval > 8 then
			currentHoverIndex = newval - 9
		else
			currentHoverIndex = newval
		end
		updateSelection()
	end

	local function GoUp()
		PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false)
		local newval = currentHoverIndex - 3
		if newval < 0 then
			currentHoverIndex = 9 + newval
		else
			currentHoverIndex = newval
		end
		updateSelection()
	end

	local function GoRight()
		PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false)
		local newval = currentHoverIndex + 1
		if newval == 3 then
			currentHoverIndex = 0
		elseif newval == 6 then
			currentHoverIndex = 3
		elseif newval == 9 then
			currentHoverIndex = 6
		else
			currentHoverIndex = newval
		end
		updateSelection()
	end

	function GoLeft()
		PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false)
		local newval = currentHoverIndex - 1
		if newval == 2 then
			currentHoverIndex = 5
		elseif newval == 5 then
			currentHoverIndex = 8
		else
			currentHoverIndex = newval
		end
		updateSelection()
	end

	Citizen.CreateThread(function()
		while true do
			Citizen.Wait(0)
			if draw then
				DrawScaleformMovieFullscreen(sf, 255, 255, 255, 255)

				if IsControlJustReleased(0, 174) then
					GoLeft()
				elseif IsControlJustReleased(0, 175) then
					GoRight()
				elseif IsControlJustReleased(0, 172) then
					GoUp()
				elseif IsControlJustReleased(0, 173) then
					GoDown()
				elseif IsControlJustReleased(0, 176) then
					GoSelect()
				end
			end
		end
	end)
end)

--GetLabelText(GetDisplayNameFromVehicleModel(GetHashKey(VehicleModel)))

--[[ Get vehicle stats function
void func_2082(int iParam0, var uParam1)
{
	int iVar0;
	float fVar1;
	int iVar2;
	float fVar3;
	int iVar4;

	if (func_2085(PLAYER::PLAYER_ID()) && ENTITY::DOES_ENTITY_EXIST(Global_2512808.f_290[0]))
	{
		if (iParam0 == Global_2512808.f_290[1] || iParam0 == Global_95019)
		{
			return;
		}
	}
	if (func_2084(PLAYER::PLAYER_ID()) && ENTITY::DOES_ENTITY_EXIST(Global_2512808.f_294))
	{
		if (iParam0 == Global_2512808.f_294)
		{
			return;
		}
	}
	if (!ENTITY::IS_ENTITY_DEAD(iParam0, 0))
	{
		if (!Global_1319812.f_21)
		{
		}
		else if (!uParam1->f_5)
		{
			fVar1 = 1f;
			iVar2 = ENTITY::GET_ENTITY_MODEL(iParam0);
			if (func_2063(iVar2))
			{
				fVar1 = 0.5f;
			}
			else
			{
				fVar1 = 1f;
			}
			(*uParam1)[0] = VEHICLE::_0x53AF99BAA671CA47(iParam0);
			(*uParam1)[2] = (VEHICLE::GET_VEHICLE_MAX_BRAKING(iParam0) * fVar1);
			(*uParam1)[1] = (VEHICLE::GET_VEHICLE_ACCELERATION(iParam0) * fVar1);
			if (iVar2 == joaat("voltic"))
			{
				(*uParam1)[1] = (VEHICLE::GET_VEHICLE_ACCELERATION(iParam0) * 2f);
			}
			if (iVar2 == joaat("tezeract"))
			{
				(*uParam1)[1] = (VEHICLE::GET_VEHICLE_ACCELERATION(iParam0) * 2.6753f);
			}
			if (VEHICLE::IS_THIS_MODEL_A_HELI(iVar2) || VEHICLE::IS_THIS_MODEL_A_PLANE(iVar2))
			{
				fVar3 = (VEHICLE::_0xC6AD107DDC9054CC(iVar2) * fVar1);
			}
			else if (VEHICLE::IS_THIS_MODEL_A_BOAT(iVar2))
			{
				fVar3 = (VEHICLE::_0x5AA3F878A178C4FC(iVar2) * fVar1);
			}
			else
			{
				fVar3 = (VEHICLE::GET_VEHICLE_MAX_TRACTION(iParam0) * fVar1);
			}
			(*uParam1)[3] = fVar3;
			if (iVar2 == joaat("t20"))
			{
				(*uParam1)[1] = ((*uParam1)[1] - 0.05f);
			}
			else if (iVar2 == joaat("vindicator"))
			{
				(*uParam1)[1] = ((*uParam1)[1] - 0.02f);
			}
			iVar4 = func_2083(VEHICLE::GET_VEHICLE_CLASS(iParam0));
			iVar0 = 0;
			iVar0 = 0;
			while (iVar0 <= 3)
			{
				(*uParam1)[iVar0] = (((*uParam1)[iVar0] / Global_1319812[iVar4 /*5*/][iVar0]) * 100f);
				if ((*uParam1)[iVar0] > 100f)
				{
					(*uParam1)[iVar0] = 100f;
				}
				iVar0++;
			}
			uParam1->f_5 = 1;
		}
	}
}
]]


AddEvent("chooseAVehicle", function(instantReady, vehOptions, raceName, frontendCoords)
	if instantReady then
		printf("special race!")
		ServerEvent("clientReady")
	else
		xnRace.ShowCarSelectionUI(vehOptions, raceName, frontendCoords)
	end
end)

function GridSelector(items, useMiddleItem, callback)
	Citizen.CreateThread(function()
		local sf = scaleform.Get("MP_NEXT_JOB_SELECTION")
		while not scaleform.IsLoaded(sf) do Citizen.Wait(0) end
		local currentHoverIndex = 0
		local currentPage = 0
		local maxPage = math.ceil(#items/6)
		local draw = false

		scaleform.Call(sf, "CLEANUP_MOVIE") -- Clean Scaleform
		RequestStreamedTextureDict("CommonMenu") while not HasStreamedTextureDictLoaded("CommonMenu") do Citizen.Wait(0) end
		RequestStreamedTextureDict("MPLeaderboard") while not HasStreamedTextureDictLoaded("MPLeaderboard") do Citizen.Wait(0) end
		RequestStreamedTextureDict("MPCarHUD") while not HasStreamedTextureDictLoaded("MPCarHUD") do Citizen.Wait(0) end

		local function updateSelection()
			if currentHoverIndex > 8 then currentHoverIndex = 8 end
			if currentHoverIndex < 0 then currentHoverIndex = 0 end

			if currentHoverIndex >= 0 and currentHoverIndex <= 5 then
				local ItemName,ItemDesc = table.unpack(items[(currentPage*6)+currentHoverIndex+1] or {" "," "})
				scaleform.Call(sf, "SET_SELECTION", currentHoverIndex, ItemName, ItemDesc, 0)
			elseif currentHoverIndex >= 6 and currentHoverIndex <= 8 then
				local ItemName,ItemDesc = table.unpack(items["middle-items"][currentHoverIndex-5] or {" "," "})
				scaleform.Call(sf, "SET_SELECTION", currentHoverIndex, ItemName, ItemDesc, 0)
			end
		end
		updateSelection()

		local function displayCurrentPage()
			local i = 0
			local lastPage = false
			scaleform.Call(sf, "CLEANUP_MOVIE")
			for v=(6*currentPage)+1,(6*currentPage)+6 do
				if items[v] then
					local ItemName,ItemDesc,TXD,IMG,disabled = table.unpack(items[v])
					if TXD == "@@IMG_NOT_FOUND" or not TXD then
						RequestStreamedTextureDict("import_export_warehouse")
						while not HasStreamedTextureDictLoaded("import_export_warehouse") do Citizen.Wait(0) end
						scaleform.Call(sf, "SET_GRID_ITEM", i, ItemName, "import_export_warehouse", "asset_empty_slot", 1, 0, -1, 0, 0, 0, 0, 0)
					else
						RequestStreamedTextureDict(TXD)
						while not HasStreamedTextureDictLoaded(TXD) do Citizen.Wait(0) end
						scaleform.Call(sf, "SET_GRID_ITEM", i, ItemName, TXD, IMG, 1, 0, -1, 0, 0, 0, disabled or false, 0)
					end
				else
					scaleform.Call(sf, "SET_GRID_ITEM", i, " ", "minigame/coronaimage", nil, 2, 0, -1, 0, 0, 0, 0, 0)
				end
				i = i+1
			end
			scaleform.Call(sf, "SET_GRID_ITEM", 6, items["middle-items"][1][1], -1, -1, -1, -1, -1, 0, -1, -1, currentPage < 1 and 1 or 0, -1)
			scaleform.Call(sf, "SET_GRID_ITEM", 7, items["middle-items"][2][1], -1, -1, -1, -1, -1, 0, -1, -1, 0, -1)
			scaleform.Call(sf, "SET_GRID_ITEM", 8, items["middle-items"][3][1], -1, -1, -1, -1, -1, 0, -1, -1, currentPage == maxPage-1 and 1 or 0, -1)
		end
		displayCurrentPage()

		local function GoSelect()
			if currentHoverIndex >= 0 and currentHoverIndex <= 5 then
				local selectedItem = (currentPage)*6 + currentHoverIndex + 1
				if items[selectedItem] then
					callback((currentPage)*6 + currentHoverIndex + 1)
					draw = false
					PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false)
				end
			elseif currentHoverIndex == 6 then
				currentPage = currentPage -1
				if currentPage < 0 then currentPage = 0 else PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false) end
				displayCurrentPage()
			elseif currentHoverIndex == 7 and useMiddleItem then
				callback("middle-item")
				draw = false
				PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false)
			elseif currentHoverIndex == 8 then
				currentPage = currentPage + 1
				if currentPage > maxPage-1 then currentPage = maxPage-1 else PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false) end
				displayCurrentPage()
			end
		end

		local function GoDown()
			PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false)
			local newval = currentHoverIndex + 3
			if newval > 8 then
				currentHoverIndex = newval - 9
			else
				currentHoverIndex = newval
			end
			updateSelection()
		end

		local function GoUp()
			PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false)
			local newval = currentHoverIndex - 3
			if newval < 0 then
				currentHoverIndex = 9 + newval
			else
				currentHoverIndex = newval
			end
			updateSelection()
		end

		local function GoRight()
			PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false)
			local newval = currentHoverIndex + 1
			if newval == 3 then
				currentHoverIndex = 0
			elseif newval == 6 then
				currentHoverIndex = 3
			elseif newval == 9 then
				currentHoverIndex = 6
			else
				currentHoverIndex = newval
			end
			updateSelection()
		end

		function GoLeft()
			PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false)
			local newval = currentHoverIndex - 1
			if newval == 2 then
				currentHoverIndex = 5
			elseif newval == 5 then
				currentHoverIndex = 8
			else
				currentHoverIndex = newval
			end
			updateSelection()
		end

		draw = true

		Citizen.CreateThread(function()
			while draw do
				Citizen.Wait(0)
				DrawScaleformMovieFullscreen(sf, 255, 255, 255, 255)

				if IsControlJustReleased(0, 174) then
					GoLeft()
				elseif IsControlJustReleased(0, 175) then
					GoRight()
				elseif IsControlJustReleased(0, 172) then
					GoUp()
				elseif IsControlJustReleased(0, 173) then
					GoDown()
				elseif IsControlJustReleased(0, 176) then
					GoSelect()
				end
			end
		end)
	end)
end

function SetColumnSettings(rowIndex, leftText, rightText, rightSomething, rightDisable, HudColour)
	PushScaleformMovieFunctionN("SET_DATA_SLOT")
		PushScaleformMovieMethodParameterInt(0)
		PushScaleformMovieMethodParameterInt(rowIndex)
		PushScaleformMovieMethodParameterInt(0)
		PushScaleformMovieMethodParameterInt(0)
		PushScaleformMovieMethodParameterInt(0)
		PushScaleformMovieMethodParameterInt(HudColour)
		PushScaleformMovieMethodParameterBool(true)
		PushScaleformMovieMethodParameterString(leftText)
		PushScaleformMovieMethodParameterString(rightText)
		PushScaleformMovieMethodParameterInt(rightSomething)
		PushScaleformMovieMethodParameterString(rightText)
		PushScaleformMovieMethodParameterInt(rightSomething)
		PushScaleformMovieMethodParameterBool(rightDisabled)
	EndScaleformMovieMethod()
end

function SetColumnInfo(rowIndex, leftText, rightText, rightIconType, iconColour, iconCheck)
	PushScaleformMovieFunctionN("SET_DATA_SLOT")
	PushScaleformMovieMethodParameterInt(1)
	PushScaleformMovieMethodParameterInt(rowIndex)
	PushScaleformMovieMethodParameterInt(0)
	PushScaleformMovieMethodParameterInt(0)
	if rightIconType < 0 or rightIconType > 4 then
		PushScaleformMovieMethodParameterInt(0)
	else
		PushScaleformMovieMethodParameterInt(2)
	end
	PushScaleformMovieMethodParameterString("xnraceimg/0126") --Race Texture Image, This doesn't work. Not sure how to get it to work, Vespura couldn't get it either.
	PushScaleformMovieMethodParameterBool(false)
	PushScaleformMovieMethodParameterString(leftText)
	PushScaleformMovieMethodParameterString(rightText)
	PushScaleformMovieMethodParameterInt(rightIconType)
	PushScaleformMovieMethodParameterInt(iconColour)
	PushScaleformMovieMethodParameterBool(iconCheck)
	EndScaleformMovieMethod()
end

--[[
void func_3400(int iParam0, int iParam1, int iParam2, int iParam3, char* sParam4, int iParam5, float fParam6, bool bParam7, bool bParam8)
{
	char* sVar0;

	sVar0 = "SET_DATA_SLOT";
	if (bParam8)
	{
		sVar0 = "UPDATE_SLOT";
	}
	if (GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_N(sVar0))
	{
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT(iParam0);
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT(iParam1);
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT(iParam2);
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT(iParam3);
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT(3);
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT(-1);
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_BOOL(false);
		func_401(sParam4);
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT(0);
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT(0);
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT(-1);
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT(iParam5);
		if (fParam6 > 1f)
		{
			fParam6 = 1f;
		}
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_FLOAT(fParam6);
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_BOOL(false);
		GRAPHICS::_PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_BOOL(bParam7);
		GRAPHICS::_POP_SCALEFORM_MOVIE_FUNCTION_VOID();
	}
}
]]

function SetColumnVehInfo(colIndex, rowIndex, unki1, rowIndex2, typeLabel, unki2, value, uknb1, update)
    local sfFunc = "SET_DATA_SLOT"
    if update then
        sfFunc = "UPDATE_SLOT"
    end

	PushScaleformMovieFunctionN(sfFunc)
    	PushScaleformMovieMethodParameterInt(colIndex)
    	PushScaleformMovieMethodParameterInt(rowIndex)
        PushScaleformMovieMethodParameterInt(unki1)
        PushScaleformMovieMethodParameterInt(rowIndex2)

        PushScaleformMovieMethodParameterInt(3)
        PushScaleformMovieMethodParameterInt(-1)
        PushScaleformMovieMethodParameterBool(false)

        PushScaleformMovieFunctionParameterString(typeLabel)

        PushScaleformMovieMethodParameterInt(0)
        PushScaleformMovieMethodParameterInt(0)
        PushScaleformMovieMethodParameterInt(-1)
        PushScaleformMovieMethodParameterInt(unki2)

        local value = value + 0.0
        if value > 1.0 then
            value = 1.0
        end
        PushScaleformMovieMethodParameterFloat(value)

    	PushScaleformMovieMethodParameterBool(false)
    	PushScaleformMovieMethodParameterBool(unkb1)
	EndScaleformMovieMethod()
end

function SetColumnPlayer(rowIndex, name, rank, rowColour, reduceRowColours, rightIcon, crewLabelText, blinkKickIcon, badgeText, badgeColour)
	PushScaleformMovieFunctionN("SET_DATA_SLOT")
		PushScaleformMovieMethodParameterInt(3)
		PushScaleformMovieMethodParameterInt(rowIndex)
		PushScaleformMovieMethodParameterInt(0)
		PushScaleformMovieMethodParameterInt(0)
		PushScaleformMovieMethodParameterInt(2)
		PushScaleformMovieMethodParameterInt(rank)
		PushScaleformMovieMethodParameterBool(false)
		PushScaleformMovieMethodParameterString(name)
		PushScaleformMovieMethodParameterInt(rowColour)
		PushScaleformMovieMethodParameterBool(reduceRowColours)
		PushScaleformMovieMethodParameterString("0")
		PushScaleformMovieMethodParameterInt(rightIcon)
		PushScaleformMovieMethodParameterString("0")
		PushScaleformMovieMethodParameterString("") -- This was the badgeText, but I think we don't need to use it
		PushScaleformMovieMethodParameterBool(blinkKickIcon)
		PushScaleformMovieMethodParameterString(badgeText)
		PushScaleformMovieMethodParameterInt(badgeColour)
	EndScaleformMovieMethod()
end

function SetFrontendRow(update,col,row,menuID,uniqueID,type,rightTextFill,isSelectable,leftText,unkText,rightTextType,rightText,buttonFill,b13)
	local func = "SET_DATA_SLOT"
	if update then func = "UPDATE_SLOT" end
	PushScaleformMovieFunctionN(func)
		PushScaleformMovieFunctionParameterInt(col) -- COL
		PushScaleformMovieFunctionParameterInt(row) -- IND

		PushScaleformMovieFunctionParameterInt(menuID) -- MID
		PushScaleformMovieFunctionParameterInt(uniqueID) -- UID
		PushScaleformMovieFunctionParameterInt(type) -- Type

		PushScaleformMovieMethodParameterInt(rightTextFill) -- Selection Text Fill
		PushScaleformMovieMethodParameterBool(isSelectable or true) -- Selectable

		PushScaleformMovieMethodParameterString(leftText)
		PushScaleformMovieMethodParameterString(unkText)

		PushScaleformMovieMethodParameterInt(rightTextType) -- Right text (Disabled = -1, Selector = 0, Icon ID = int > 0)
		PushScaleformMovieMethodParameterString(rightText) -- Right Text
		PushScaleformMovieMethodParameterInt(buttonFill) -- Button Colour

		PushScaleformMovieMethodParameterBool(b13)
	EndScaleformMovieMethod()
end

local FE = {buttons = {}}
function FE.CreateButton(self,text,colour)
	if text then
		local thisRow = #self.buttons+1
		self.buttons[thisRow] = {
			col = 0,
			row = thisRow-1,
			menuID = 0,
			uniqueID = 0,
			type = 2,
			rightTextFill = 0,
			isSelectable = true,
			leftText = text,
			unkText = "",
			rightTextType = -1,
			rightText = "",
			buttonFill = (colour or 0),
			b13 = false,
		}
		local b = self.buttons[thisRow]
		SetFrontendRow(false,b.col,b.row,b.menuID,b.uniqueID,b.type,b.rightTextFill,b.isSelectable,b.leftText,b.unkText,b.rightTextType,b.rightText,b.buttonFill,b.b13)
		return thisRow
	end
end
function FE.CreateComboBox(self,text,items,hoveredItem)
	if text and items then
		local thisRow = #self.buttons+1
		self.buttons[thisRow] = {
			items = items,
			hoveredItem = hoveredItem or 1,

			col = 0,
			row = thisRow-1,
			menuID = 0,
			uniqueID = 0,
			type = 0,
			rightTextFill = 0,
			isSelectable = true,
			leftText = text or "",
			unkText = "",
			rightTextType = 0,
			rightText = items[hoveredItem or 1],
			buttonFill = 0,
			b13 = false,
		}
		local b = self.buttons[thisRow]
		SetFrontendRow(false,b.col,b.row,b.menuID,b.uniqueID,b.type,b.rightTextFill,b.isSelectable,b.leftText,b.unkText,b.rightTextType,b.rightText,b.buttonFill,b.b13)
		return thisRow
	end
end
function FE.GetObject(self,index)
	return self.buttons[index]
end
function FE.EditObject(self,buttonIndex,args)
	if self.buttons[buttonIndex] then
		local b1 = self.buttons[buttonIndex]
		self.buttons[buttonIndex] = {
			col = args.col or b1.col,
			row = b1.row, -- Never change the row
			menuID = args.menuID or b1.menuID,
			uniqueID = args.uniqueID or b1.uniqueID,
			type = args.type or b1.type,
			rightTextFill = args.rightTextFill or b1.rightTextFill,
			isSelectable = args.isSelectable or b1.isSelectable,
			leftText = args.leftText or b1.leftText,
			unkText = args.unkText or b1.unkText,
			rightTextType = args.rightTextType or b1.rightTextType,
			rightText = args.rightText or b1.rightText,
			buttonFill = args.buttonFill or b1.buttonFill,
			b13 = args.b13 or b1.b13,

			items = b1.items and (args.items or b1.items) or nil,
			hoveredItem = b1.hoveredItem and (args.hoveredItem or b1.hoveredItem) or nil,
		}
		local b = self.buttons[buttonIndex]
		if b.type == 0 and b.items then
			b.rightText = b.items[b.hoveredItem]
		end
		SetFrontendRow(true,b.col,b.row,b.menuID,b.uniqueID,b.type,b.rightTextFill,b.isSelectable,b.leftText,b.unkText,b.rightTextType,b.rightText,b.buttonFill,b.b13)
		return thisRow
	else
		printf("This frontend does not contain a button at "..(buttonIndex or "NULL").."!")
	end
end
function FE.UpdateObject(self,objectIndex)
	local b = self.buttons[objectIndex]
	if b.type == 0 and b.items then
		b.rightText = b.items[b.hoveredItem]
	end
	SetFrontendRow(true,b.col,b.row,b.menuID,b.uniqueID,b.type,b.rightTextFill,b.isSelectable,b.leftText,b.unkText,b.rightTextType,b.rightText,b.buttonFill,b.b13)
end
function FE.UpdateHelpText(self,helpText,flashIcon,flashText)
    if helpText then
        BeginScaleformMovieMethodN("SET_DESCRIPTION")
            PushScaleformMovieFunctionParameterInt(0)
            PushScaleformMovieFunctionParameterString(helpText)
            PushScaleformMovieFunctionParameterBool(flashIcon or false)
            PushScaleformMovieFunctionParameterBool(flashText or false)
        EndScaleformMovieMethod()
        self.helpText = helpText
    elseif self.helpText then
        BeginScaleformMovieMethodN("SET_DESCRIPTION")
            PushScaleformMovieFunctionParameterInt(0)
            PushScaleformMovieFunctionParameterString(self.helpText)
            PushScaleformMovieFunctionParameterBool(flashIcon or false)
            PushScaleformMovieFunctionParameterBool(flashText or false)
        EndScaleformMovieMethod()
    end
end
setmetatable(FE, {__call = function(t) -- Frontend Class!
	local ret = t

	for i,b in ipairs(ret.buttons) do
		ret.buttons[i] = nil
	end
    ret.helpText = false

	return ret
end})

function GetColSelection(col)
	Citizen.CreateThread(function()
		PushScaleformMovieFunctionN("GET_COLUMN_SELECTION")
		PushScaleformMovieMethodParameterInt(col)
		local ret = EndScaleformMovieMethodReturn()
		while not GetScaleformMovieFunctionReturnBool(ret) do Citizen.Wait(0) end
		return GetScaleformMovieFunctionReturnInt(ret)
	end)
end

RequestAdditionalText("RACES", 0)
RequestAdditionalText("FMMC", 1)
Citizen.CreateThread(function()

	local frontendActive = false

	function xnRace.ShowLobbyUI(players)
		if IsPauseMenuActive() then
			SetPauseMenuActive(false)
			SetFrontendActive(false)
			frontendActive = false
		else
			RestartFrontendMenu("FE_MENU_VERSION_CORONA", -1)
			AddFrontendMenuContext("VEHICLE_SCREEN")
			AddFrontendMenuContext("AUTOFILL_CORONA")
			AddFrontendMenuContext("CORONA_TOURNAMENT")
			AddFrontendMenuContext("AUTOFILL_CONTINUE")

			ActivateFrontendMenu("FE_MENU_VERSION_CORONA", false, -1)

			while not IsPauseMenuActive() or IsPauseMenuRestarting() do
				Citizen.Wait(0)
			end

			AddFrontendMenuContext("FM_TUTORIAL")
			AddFrontendMenuContext("AUTOFILL_CORONA")
			AddFrontendMenuContext("CORONA_TOURNAMENT")
			AddFrontendMenuContext("AUTOFILL_CONTINUE")

			frontendActive = true

			BeginScaleformMovieMethodV("SHIFT_CORONA_DESC")
			PushScaleformMovieMethodParameterBool(true)
			PushScaleformMovieMethodParameterBool(false)
			EndScaleformMovieMethod()

			BeginScaleformMovieMethodN("SET_HEADER_TITLE")
			BeginTextCommandScaleformString("STRING")
			AddTextComponentSubstringPlayerName("Race Name") -- Race name here
			EndTextCommandScaleformString()
			PushScaleformMovieMethodParameterBool(false)
			BeginTextCommandScaleformString("STRING")
			AddTextComponentSubstringPlayerName("Race Description") -- Race Descrition here
			EndTextCommandScaleformString()
			PushScaleformMovieMethodParameterBool(false)
			EndScaleformMovieMethod()

			-- This section can change the menus colour. Maybe we can do this for different event types?
			-- BeginScaleformMovieMethodV("SET_ALL_HIGHLIGHTS")
			-- PushScaleformMovieMethodParameterBool(true)
			-- PushScaleformMovieMethodParameterInt(116)
			-- EndScaleformMovieMethod()

			Citizen.Wait(500)

			SetColumnSettings(0, "Select Vehicle", "0", -1, false, 0) --Select Vehicle
			SetColumnSettings(1, "Ready", "0", -1, false, 0) --Ready Up

			PushScaleformMovieFunctionN("DISPLAY_DATA_SLOT")
			PushScaleformMovieMethodParameterInt(0)
			EndScaleformMovieMethod()

			--This is dummy info, need to get stuff from the races. You can add even more Column if need be.
			local rating = "69%"
			local createdBy = "Smallo"
			local maxPlayers = "64 lul"
			local raceType = "Stunt Race"

			SetColumnInfo(0, "Rating", rating, 5)
			SetColumnInfo(1, "Created By", createdBy, 5)
			SetColumnInfo(2, "Max Players", maxPlayers, 5)
			SetColumnInfo(3, "Type", raceType, 5, 12, 0)

			PushScaleformMovieFunctionN("DISPLAY_DATA_SLOT")
			PushScaleformMovieMethodParameterInt(1)
			EndScaleformMovieMethod()

			rowIndex = 0
			local plyName = GetPlayerName(PlayerId())
			SetColumnPlayer(rowIndex, plyName, rowIndex + 1, 28, false, 62, "", false, "", 0);
			rowIndex = rowIndex + 1

			for svId,data in pairs(players) do
				local ply = GetPlayerFromServerId(svId)
				if ply ~= PlayerId() then
					local plyName = GetPlayerName(ply)
					SetColumnPlayer(rowIndex, plyName, rowIndex + 1, 28, false, 62, "", false, "", 0);
					rowIndex = rowIndex + 1
				end
			end

			PushScaleformMovieFunctionN("DISPLAY_DATA_SLOT")
			PushScaleformMovieMethodParameterInt(3)
			EndScaleformMovieMethod()

			PushScaleformMovieFunctionN("SET_COLUMN_FOCUS")
			PushScaleformMovieMethodParameterInt(0)
			PushScaleformMovieMethodParameterInt(1)
			PushScaleformMovieMethodParameterInt(1)
			PushScaleformMovieMethodParameterInt(0)
			EndScaleformMovieMethod()

			while frontendActive do
				Citizen.Wait(10)
				if IsControlJustReleased(2, 201) or IsDisabledControlJustReleased(2, 201) then
					PushScaleformMovieFunctionN("GET_COLUMN_SELECTION")
						PushScaleformMovieMethodParameterInt(0)
					local ret = EndScaleformMovieMethodReturn()

					while not GetScaleformMovieFunctionReturnBool(ret) do Citizen.Wait(0) end

					local retInt = GetScaleformMovieFunctionReturnInt(ret)

					if retInt == 0 then
						--xnRace.ShowCarSelectionUI()
						AddFrontendMenuContext("CORONA_THIRD_COLUMN")
					elseif retInt == 1 then
						-- READY UP BOIS!!
						ServerEvent("toggleReadyState")
					end
				end
			end
		end
	end

	function xnRace.ShowCarSelectionUI(vehTable, raceName, frontendCoords)
	    --SetEntityCoords(GetPlayerPed(-1), 1496.868, 6222.999, 170.67)
		--FreezeEntityPosition(GetPlayerPed(-1), true)

        -- loading cam pos = "cam"
        -- ped pos = "start"


        -- Start? camera
        --SetCamCoord(menuCam, 746.475, 5656.176, 776.246) -- scene
        --SetCamRot(menuCam, 0.0, 0.0, 120.031, 2)

        -- Finish Camera
        --SetCamCoord(menuCam, -1032.582, 5532.714, 17.938) -- camf
        --SetCamRot(menuCam, 11.082, 0.0, -79.935, 2)

        local x,y,z = table.unpack(frontendCoords[1])
        SetEntityCoords(GetPlayerPed(-1), x,y,z) -- start
		SetEntityHeading(GetPlayerPed(-1), frontendCoords[2])
		FreezeEntityPosition(GetPlayerPed(-1), true)
		TaskAchieveHeading(GetPlayerPed(-1), frontendCoords[2], 1000)

        local initialVehicleModel = GetHashKey(vehTable[1][2])
        RequestModel(initialVehicleModel)
        while not HasModelLoaded(initialVehicleModel) do Citizen.Wait(0) end

        local ivmDim = GetModelDimensions(initialVehicleModel)
        local vehicleCoords = GetOffsetFromEntityInWorldCoords(GetPlayerPed(-1), (-ivmDim.x)+0.3, (-ivmDim.y) - 1.0, 0.0)
        ClearAreaOfEverything(x,y,z, 200.0, false, false, false, false, false)

        local selectedVehicle = CreateVehicle(initialVehicleModel, vehicleCoords, frontendCoords[2], false, false)
        SetEntityAlpha(selectedVehicle, 0)

        local menuCam = CreateCam("DEFAULT_SCRIPTED_CAMERA", true)
        SetCamNearClip(menuCam, 0.5)
        SetCamCoord(menuCam, GetOffsetFromEntityInWorldCoords(selectedVehicle, -3.1, 4.1, 0.5391))
        PointCamAtCoord(menuCam, GetOffsetFromEntityInWorldCoords(GetPlayerPed(-1), (-ivmDim.x)+0.3, (-ivmDim.y) - 1.0, 0.0))

        SetCamFov(menuCam, 49.9802)
		SetCamActive(menuCam, true)
		RenderScriptCams(1, 1, 1000, 0, 0)

		LeaveClouds()
		while IsPlayerSwitchInProgress() do Citizen.Wait(0) HideHudComponentThisFrame(0) HideHudComponentThisFrame(141) end

        local vehicleitems = {}
        for i,iteminfo in ipairs(vehTable) do
            vehicleitems[i] = GetLabelText(GetDisplayNameFromVehicleModel(GetHashKey(iteminfo[2])))
        end
		
		local radioStationNames = {}
		for i,label in ipairs(xnRace.radioStations) do 
			radioStationNames[i] = GetLabelText(label == "OFF" and "RADIO_OFF" or label) 
		end

        local vehColours = {
            {"Black", 12},
            {"Gray", 13},
            {"Light Gray", 14},
            {"Ice White", 131},
            {"Blue", 83},
            {"Dark Blue", 82},
            {"Midnight Blue", 84},
            {"Midnight Purple", 149},
            {"Schafter Purple", 148},
            {"Red", 39},
            {"Dark Red", 40},
            {"Orange", 41},
            {"Yellow", 42},
            {"Lime Green", 55},
            {"Green", 128},
            {"Frost Green", 151},
            {"Foliage Green", 155},
            {"Olive Darb", 152},
            {"Dark Earth", 153},
            {"Desert Tan", 154},
        }
        local vehicleColours = {}
        for i,item in ipairs(vehColours) do
            vehicleColours[i] = item[1]
        end

        local playerModels = {"mp_f_deadhooker","mp_f_stripperlite","mp_g_m_pros_01","mp_m_exarmy_01","mp_m_famdd_01","mp_m_fibsec_01","mp_m_shopkeep_01","mp_s_m_armoured_01","a_f_m_beach_01","a_f_m_bevhills_01","a_f_m_bevhills_02","a_f_m_bodybuild_01","a_f_m_business_02","a_f_m_downtown_01","a_f_m_eastsa_01","a_m_m_acult_01","a_m_m_afriamer_01","a_m_m_beach_01","a_m_m_beach_02","a_m_m_bevhills_01","a_m_m_bevhills_02","a_m_m_business_01","a_f_m_eastsa_02","a_f_m_fatbla_01","a_f_m_fatcult_01","a_f_m_fatwhite_01","a_f_m_ktown_01","a_f_m_ktown_02","a_f_m_prolhost_01","a_m_m_eastsa_01","a_m_m_eastsa_02","a_m_m_farmer_01","a_m_m_fatlatin_01","a_m_m_genfat_01","a_m_m_genfat_02","a_m_m_golfer_01","a_f_m_salton_01","a_f_m_skidrow_01","a_f_m_soucentmc_01","a_f_m_soucent_01","a_f_m_soucent_02","a_f_m_tourist_01","a_f_m_trampbeac_01","a_m_m_hasjew_01","a_m_m_hillbilly_01","a_m_m_hillbilly_02","a_m_m_indian_01","a_m_m_ktown_01","a_m_m_malibu_01","a_m_m_mexcntry_01","a_f_m_tramp_01","a_f_o_genstreet_01","a_f_o_indian_01","a_f_o_ktown_01","a_f_o_salton_01","a_f_o_soucent_01","a_f_o_soucent_02","a_m_m_mexlabor_01","a_m_m_og_boss_01","a_m_m_paparazzi_01","a_m_m_polynesian_01","a_m_m_prolhost_01","a_m_m_rurmeth_01","a_m_m_salton_01","a_f_y_beach_01","a_f_y_bevhills_01","a_f_y_bevhills_02","a_f_y_bevhills_03","a_f_y_bevhills_04","a_f_y_business_01","a_f_y_business_02","a_m_m_salton_02","a_m_m_salton_03","a_m_m_salton_04","a_m_m_skater_01","a_m_m_skidrow_01","a_m_m_socenlat_01","a_m_m_soucent_01","a_f_y_business_03","a_f_y_business_04","a_f_y_eastsa_01","a_f_y_eastsa_02","a_f_y_eastsa_03","a_f_y_epsilon_01","a_f_y_fitness_01","a_m_m_soucent_02","a_m_m_soucent_03","a_m_m_soucent_04","a_m_m_stlat_02","a_m_m_tennis_01","a_m_m_tourist_01","a_m_m_trampbeac_01","a_f_y_fitness_02","a_f_y_genhot_01","a_f_y_golfer_01","a_f_y_hiker_01","a_f_y_hippie_01","a_f_y_hipster_01","a_f_y_hipster_02","a_m_m_tramp_01","a_m_m_tranvest_01","a_m_m_tranvest_02","a_m_o_acult_01","a_m_o_acult_02","a_m_o_beach_01","a_m_o_genstreet_01","a_f_y_hipster_03","a_f_y_hipster_04","a_f_y_indian_01","a_f_y_juggalo_01","a_f_y_runner_01","a_f_y_rurmeth_01","a_f_y_scdressy_01","a_m_o_ktown_01","a_m_o_salton_01","a_m_o_soucent_01","a_m_o_soucent_02","a_m_o_soucent_03","a_m_o_tramp_01","a_m_y_acult_01","a_f_y_skater_01","a_f_y_soucent_01","a_f_y_soucent_02","a_f_y_soucent_03","a_f_y_tennis_01","a_f_y_topless_01","a_f_y_tourist_01","a_m_y_acult_02","a_m_y_beachvesp_01","a_m_y_beachvesp_02","a_m_y_beach_01","a_m_y_beach_02","a_m_y_beach_03","a_m_y_bevhills_01","a_f_y_tourist_02","a_f_y_vinewood_01","a_f_y_vinewood_02","a_f_y_vinewood_03","a_f_y_vinewood_04","a_f_y_yoga_01","cs_tracydisanto","a_m_y_bevhills_02","a_m_y_breakdance_01","a_m_y_busicas_01","a_m_y_business_01","a_m_y_business_02","a_m_y_business_03","a_m_y_cyclist_01","cs_tanisha", "cs_patricia", "cs_mrsphillips", "cs_mrs_thornhill", "cs_natalia", "cs_molly", "cs_movpremf_01","a_m_y_dhill_01","a_m_y_downtown_01","a_m_y_eastsa_01","a_m_y_eastsa_02","a_m_y_epsilon_01","a_m_y_epsilon_02","a_m_y_gay_01","cs_maryann", "cs_michelle", "cs_marnie", "cs_magenta", "cs_janet", "cs_jewelass", "cs_guadalope","a_m_y_gay_02","a_m_y_genstreet_01","a_m_y_genstreet_02","a_m_y_golfer_01","a_m_y_hasjew_01","a_m_y_hiker_01","a_m_y_hippy_01","cs_gurk",  "cs_debra", "cs_denise", "cs_amandatownley",  "cs_ashley", "csb_screen_writer", "csb_stripper_01","a_m_y_hipster_01","a_m_y_hipster_02","a_m_y_hipster_03","a_m_y_indian_01","a_m_y_jetski_01","a_m_y_juggalo_01","a_m_y_ktown_01","csb_stripper_02", "csb_tonya", "csb_maude", "csb_denise_friend", "csb_abigail", "csb_anita", "g_f_y_ballas_01","a_m_y_ktown_02","a_m_y_latino_01","a_m_y_methhead_01","a_m_y_mexthug_01","a_m_y_motox_01","a_m_y_motox_02","a_m_y_musclbeac_01","g_f_y_families_01","g_f_y_lost_01","g_f_y_vagos_01","s_f_m_fembarber","s_f_m_maid_01","s_f_m_shop_high","s_f_m_sweatshop_01","a_m_y_musclbeac_02","a_m_y_polynesian_01","a_m_y_roadcyc_01","a_m_y_runner_01","a_m_y_runner_02","a_m_y_salton_01","a_m_y_skater_01","s_f_y_airhostess_01","s_f_y_bartender_01","s_f_y_baywatch_01","s_f_y_factory_01","s_f_y_hooker_01","s_f_y_hooker_02","s_f_y_hooker_03","a_m_y_skater_02","a_m_y_soucent_01","a_m_y_soucent_02","a_m_y_soucent_03","a_m_y_soucent_04","a_m_y_stbla_01","a_m_y_stbla_02","s_f_y_migrant_01","s_f_y_movprem_01","s_f_y_shop_low","s_f_y_shop_mid","s_f_y_stripperlite","s_f_y_stripper_01","s_f_y_stripper_02","a_m_y_stlat_01","a_m_y_stwhi_01","a_m_y_stwhi_02","a_m_y_sunbathe_01","a_m_y_surfer_01","a_m_y_vindouche_01","a_m_y_vinewood_01","s_f_y_sweatshop_01","u_f_m_corpse_01","u_f_m_miranda","u_f_m_promourn_01","u_f_o_moviestar","u_f_o_prolhost_01","u_f_y_bikerchic","u_f_y_comjane","u_f_y_hotposh_01","u_f_y_jewelass_01","u_f_y_mistress","u_f_y_poppymich","u_f_y_princess","u_f_y_spyactress","a_m_y_vinewood_02","a_m_y_vinewood_03","a_m_y_vinewood_04","a_m_y_yoga_01","csb_anton","csb_ballasog","csb_burgerdrug","csb_car3guy1","csb_car3guy2","csb_chef","csb_chin_goon","csb_cletus", "csb_customer", "csb_fos_rep", "csb_g", "csb_groom", "csb_grove_str_dlr", "csb_hao", "csb_hugh", "csb_imran", "csb_janitor", "csb_ortega", "csb_oscar", "csb_porndudes", "csb_prologuedriver", "csb_ramp_gang",  "csb_ramp_hic", "csb_ramp_hipster", "csb_ramp_mex", "csb_reporter", "csb_roccopelosi", "csb_trafficwarden","cs_bankman", "cs_barry", "cs_beverly", "cs_brad", "cs_carbuyer", "cs_chengsr", "cs_chrisformage", "cs_clay", "cs_dale", "cs_davenorton", "cs_devin", "cs_dom", "cs_dreyfuss", "cs_drfriedlander", "cs_fabien", "cs_floyd", "cs_hunter", "cs_jimmyboston", "cs_jimmydisanto", "cs_joeminuteman", "cs_johnnyklebitz", "cs_josef", "cs_josh", "cs_lazlow", "cs_lestercrest", "cs_lifeinvad_01", "cs_manuel", "cs_martinmadrazo", "cs_milton", "cs_movpremmale", "cs_mrk", "cs_nervousron", "cs_nigel", "cs_old_man1a", "cs_old_man2", "cs_omega", "cs_orleans", "cs_paper", "cs_priest", "cs_prolsec_02", "cs_russiandrunk", "cs_siemonyetarian", "cs_solomon", "cs_stevehains", "cs_stretch", "cs_taocheng", "cs_taostranslator", "cs_tenniscoach", "cs_terry", "cs_tom", "cs_tomepsilon", "cs_wade", "cs_zimbor", "g_m_m_armboss_01","g_m_m_armgoon_01","g_m_m_armlieut_01","g_m_m_chemwork_01","g_m_m_chiboss_01","g_m_m_chicold_01","g_m_m_chigoon_01","g_m_m_chigoon_02","g_m_m_korboss_01","g_m_m_mexboss_01","g_m_m_mexboss_02","g_m_y_armgoon_02","g_m_y_azteca_01","g_m_y_ballaeast_01","g_m_y_ballaorig_01","g_m_y_ballasout_01","g_m_y_famca_01","g_m_y_famdnf_01","g_m_y_famfor_01","g_m_y_korean_01","g_m_y_korean_02","g_m_y_korlieut_01","g_m_y_lost_01","g_m_y_lost_02","g_m_y_lost_03","g_m_y_mexgang_01","g_m_y_mexgoon_01","g_m_y_mexgoon_02","g_m_y_mexgoon_03","g_m_y_pologoon_01","g_m_y_pologoon_02","g_m_y_salvaboss_01","g_m_y_salvagoon_01","g_m_y_salvagoon_02","g_m_y_salvagoon_03","g_m_y_strpunk_01","g_m_y_strpunk_02","hc_driver", "hc_gunman", "hc_hacker", "s_m_m_ammucountry","s_m_m_autoshop_01","s_m_m_autoshop_02","s_m_m_bouncer_01","s_m_m_ciasec_01","s_m_m_cntrybar_01","s_m_m_dockwork_01","s_m_m_doctor_01","s_m_m_fiboffice_02","s_m_m_gaffer_01","s_m_m_gardener_01","s_m_m_gentransport","s_m_m_hairdress_01","s_m_m_highsec_01","s_m_m_highsec_02","s_m_m_janitor","s_m_m_lathandy_01","s_m_m_lifeinvad_01","s_m_m_linecook","s_m_m_lsmetro_01","s_m_m_mariachi_01","s_m_m_migrant_01","s_m_m_movprem_01","s_m_m_movspace_01","s_m_m_pilot_01","s_m_m_pilot_02","s_m_m_postal_01","s_m_m_postal_02","s_m_m_scientist_01","s_m_m_strperf_01","s_m_m_strpreach_01","s_m_m_strvend_01","s_m_m_trucker_01","s_m_m_ups_01","s_m_m_ups_02","s_m_o_busker_01","s_m_y_airworker","s_m_y_ammucity_01","s_m_y_armymech_01","s_m_y_autopsy_01","s_m_y_barman_01","s_m_y_baywatch_01","s_m_y_busboy_01","s_m_y_chef_01","s_m_y_clown_01","s_m_y_construct_01","s_m_y_construct_02","s_m_y_dealer_01","s_m_y_devinsec_01","s_m_y_dockwork_01","s_m_y_dwservice_01","s_m_y_dwservice_02","s_m_y_factory_01","s_m_y_garbage","s_m_y_grip_01","s_m_y_mime","s_m_y_pestcont_01","s_m_y_pilot_01","s_m_y_prismuscl_01","s_m_y_prisoner_01","s_m_y_robber_01","s_m_y_shop_mask","s_m_y_strvend_01","s_m_y_uscg_01","s_m_y_valet_01","s_m_y_waiter_01","s_m_y_winclean_01","s_m_y_xmech_01","s_m_y_xmech_02","u_m_m_aldinapoli","u_m_m_bankman","u_m_m_bikehire_01","u_m_m_fibarchitect","u_m_m_filmdirector","u_m_m_glenstank_01","u_m_m_griff_01","u_m_m_jesus_01","u_m_m_jewelsec_01","u_m_m_jewelthief","u_m_m_markfost","u_m_m_partytarget","u_m_m_promourn_01","u_m_m_rivalpap","u_m_m_spyactor","u_m_m_willyfist","u_m_o_finguru_01","u_m_o_taphillbilly","u_m_o_tramp_01","u_m_y_abner","u_m_y_antonb","u_m_y_babyd","u_m_y_baygor","u_m_y_burgerdrug_01","u_m_y_chip","u_m_y_cyclist_01","u_m_y_fibmugger_01","u_m_y_guido_01","u_m_y_gunvend_01","u_m_y_hippie_01","u_m_y_imporage","u_m_y_justin","u_m_y_mani","u_m_y_militarybum","u_m_y_paparazzi","u_m_y_party_01","u_m_y_pogo_01","u_m_y_prisoner_01","u_m_y_proldriver_01","u_m_y_rsranger_01","u_m_y_sbike","u_m_y_staggrm_01","u_m_y_tattoo_01" }

		local frontend = FE()
		ActivateFrontendMenu("FE_MENU_VERSION_CORONA_RACE", false, -1)

        local function UpdateSelectedVehicle()
            local VehicleSelection = frontend:GetObject(1)
            local ColourSelection = frontend:GetObject(2)

            local newModelName = vehTable[VehicleSelection.hoveredItem][2]
            local newColour = vehColours[ColourSelection.hoveredItem][2]

            Citizen.CreateThread(function()
                local newModel = GetHashKey(newModelName)
                local changeModel = true
                if selectedVehicle then changeModel = (GetEntityModel(selectedVehicle) ~= newModel) end

                if changeModel and IsModelInCdimage(newModel) then
                    RequestModel(newModel)
                    while not HasModelLoaded(newModel) do Citizen.Wait(0) end

                    local oldSelectedVehicle = selectedVehicle

                    local md = GetModelDimensions(newModel)
                    local coords = GetOffsetFromEntityInWorldCoords(GetPlayerPed(-1), (-md.x)+0.3, (-md.y) - 1.0, 0.0)
                    local head = GetEntityHeading(oldSelectedVehicle)

                    selectedVehicle = CreateVehicle(newModel, coords, head, false, false)
                    SetEntityCoordsNoOffset(selectedVehicle, coords, 0, 0, 0)
                    SetVehicleOnGroundProperly(selectedVehicle)
                    SetEntityHeading(selectedVehicle, head)
                    FreezeEntityPosition(selectedVehicle, true)

                    -- Set veh mods to get accurate speed and shit
                	--[[ToggleVehicleMod(selectedVehicle, 22, true)
                	SetVehicleMod(selectedVehicle, 16, 5, true)
                	SetVehicleMod(selectedVehicle, 12, 2, true)
                	SetVehicleMod(selectedVehicle, 11, 3, true)
                	SetVehicleMod(selectedVehicle, 15, 3, true)
                	SetVehicleMod(selectedVehicle, 13, 2, true)
                	SetVehicleMod(selectedVehicle, 23, 19, true)]]

                    if oldSelectedVehicle and not IsEntityDead(oldSelectedVehicle) then
                        SetEntityAsMissionEntity(oldSelectedVehicle)
                        DeleteVehicle(oldSelectedVehicle)
                    end

                    -- R* is actually dumb
                    local vv = {
                        speed = 100,
                        accel = 100,
                        brake = 100,
                        handle = 100,
                    }
                    vv.speed = GetVehicleMaxSpeed(selectedVehicle)
                    vv.brake = GetVehicleMaxBraking(selectedVehicle)
                    vv.accel = GetVehicleAcceleration(selectedVehicle)
                    if GetHashKey("voltic") == newModel then
                        vv.accel = GetVehicleAcceleration(selectedVehicle) * 2.0
                    end
                    if GetHashKey("tezeract") == newModel then
                        vv.accel = GetVehicleAcceleration(selectedVehicle) * 2.6753
                    end
                    if IsThisModelAHeli(newModel) or IsThisModelAPlane(newModel) then
                        vv.handle = GetVehicleModelMaxKnots(newModel)
                    elseif IsThisModelABoat(newModel) then
                        vv.handle = GetVehicleModelMoveResistance(newModel)
                    else
                        vv.handle = GetVehicleMaxTraction(selectedVehicle)
                    end
                    if GetHashKey("t20") == newModel then
                        vv.accel = vv.accel - 0.05
                    elseif GetHashKey("t20") == newModel then
                        vv.accel = vv.accel - 0.02
                    end
                    local class = GetVehicleClass(selectedVehicle)
                    local enumsforclassestothefuckoffglobal = {
                        [14] = 3,
                        [15] = 1,
                        [16] = 2
                    }
                    for i=1,20 do
                        if not enumsforclassestothefuckoffglobal[i] then
                            enumsforclassestothefuckoffglobal[i] = 0
                        end
                    end
                    local somefuckoffglobalwhichidontknowhowtogetthecontentsof = {
                        [0] = {
                            [0] = 50,
                            [1] = 0.26753,
                            [2] = 1,
                            [3] = 2.5,
                        }
                    }
                    local enumforthefuckoffglobal = -1
                    if enumsforclassestothefuckoffglobal[class] then enumforthefuckoffglobal = enumsforclassestothefuckoffglobal[class] end
                    vv.speed = (vv.speed / somefuckoffglobalwhichidontknowhowtogetthecontentsof[enumforthefuckoffglobal][0]) * 100.0
                    vv.accel = (vv.accel / somefuckoffglobalwhichidontknowhowtogetthecontentsof[enumforthefuckoffglobal][1]) * 100.0
                    vv.brake = (vv.brake / somefuckoffglobalwhichidontknowhowtogetthecontentsof[enumforthefuckoffglobal][2]) * 100.0
                    vv.handle = (vv.handle / somefuckoffglobalwhichidontknowhowtogetthecontentsof[enumforthefuckoffglobal][3]) * 100.0
                    printf("Speed: %s Accel: %s Brake: %s Handle: %s",vv.speed,vv.accel,vv.brake,vv.handle)

                    SetColumnVehInfo(0, 5, 0, 0, GetLabelText("RH_Speed"), 0, vv.speed / 100.0, false, true)
                    SetColumnVehInfo(0, 6, 0, 1, GetLabelText("RH_Accel"), 0, vv.accel / 100.0, false, true)
                    SetColumnVehInfo(0, 7, 0, 2, GetLabelText("RH_Brake"), 0, vv.brake / 100.0, false, true)
                    SetColumnVehInfo(0, 8, 0, 3, GetLabelText("RH_Handle"), 0, vv.handle / 100.0, false, false)
                end

                SetVehicleColours(selectedVehicle, newColour, newColour)
            end)
        end

        local function UpdatePedModel()
            local ModelSelection = frontend:GetObject(3)
            local newPedModel = playerModels[ModelSelection.hoveredItem]

            Citizen.CreateThread(function()
                if IsModelInCdimage(newPedModel) then
                    RequestModel(newPedModel)
                    while not HasModelLoaded(newPedModel) do Citizen.Wait(0) end
                    SetPlayerModel(PlayerId(), newPedModel)
                end
            end)
		end

		Citizen.Wait(500)

		if IsPauseMenuActive() then
			while not IsPauseMenuActive() or IsPauseMenuRestarting() do
				Citizen.Wait(0)
			end

            AddFrontendMenuContext("*NONE*")
            AddFrontendMenuContext("VEHICLE_SCREEN")
            AddFrontendMenuContext("CORONA_SELECT")
            AddFrontendMenuContext("DISPLAY_CORONA_BUTTONS")

			N_0x77f16b447824da6c(124) -- Set page?
			--N_0xec9264727eec0f28() -- Disable frontend menu input...

			BeginScaleformMovieMethodV("LOCK_MOUSE_SUPPORT")
				PushScaleformMovieFunctionParameterBool(false)
				PushScaleformMovieFunctionParameterBool(false)
			EndScaleformMovieMethod()

			BeginScaleformMovieMethodV("SHOW_HEADING_DETAILS")
				PushScaleformMovieFunctionParameterBool(true)
			EndScaleformMovieMethod()

			BeginScaleformMovieMethodV("SET_ALL_HIGHLIGHTS")
				PushScaleformMovieFunctionParameterBool(true)
				PushScaleformMovieFunctionParameterInt(123)
			EndScaleformMovieMethod()

			PushScaleformMovieFunctionN("SET_COLUMN_TITLE")
				PushScaleformMovieMethodParameterInt(1)
				PushScaleformMovieMethodParameterButtonName("")
				PushScaleformMovieMethodParameterBool(false)
			EndScaleformMovieMethod()

			BeginScaleformMovieMethodV("SHIFT_CORONA_DESC")
				PushScaleformMovieMethodParameterBool(true)
				PushScaleformMovieMethodParameterBool(false)
			EndScaleformMovieMethod()

			--[[
			func_3556(uParam1, 0);
			]]

			BeginScaleformMovieMethodV("SET_MENU_HEADER_TEXT_BY_INDEX")
				PushScaleformMovieFunctionParameterInt(1)
				BeginTextCommandScaleformString("FM_BET_VEH")
					AddTextComponentInteger(-1)
					AddTextComponentInteger(-1)
					AddTextComponentInteger(-1)
				EndTextCommandScaleformString()
				PushScaleformMovieFunctionParameterInt(0)
				PushScaleformMovieFunctionParameterBool(true)
			EndScaleformMovieMethod()

			BeginScaleformMovieMethodV("SET_MENU_HEADER_TEXT_BY_INDEX")
				PushScaleformMovieFunctionParameterInt(2)
				BeginTextCommandScaleformString("")
					AddTextComponentInteger(-1)
					AddTextComponentInteger(-1)
					AddTextComponentInteger(-1)
				EndTextCommandScaleformString()
				PushScaleformMovieFunctionParameterInt(0)
				PushScaleformMovieFunctionParameterBool(true)
			EndScaleformMovieMethod()

			BeginScaleformMovieMethodV("WEIGHT_MENU")
				PushScaleformMovieFunctionParameterInt(((288 * 1) + (2 * (1 - 1))))
				PushScaleformMovieFunctionParameterInt(((288 * 2) + (2 * (2 - 1))))
				PushScaleformMovieFunctionParameterInt(((288 * -1) + (2 * (-1 - 1))))
			EndScaleformMovieMethod()

			BeginScaleformMovieMethodN("SET_HEADER_TITLE")
				BeginTextCommandScaleformString("STRING")
					AddTextComponentSubstringPlayerName(raceName or "Custom Race")
				EndTextCommandScaleformString()
				PushScaleformMovieMethodParameterBool(true)
				BeginTextCommandScaleformString("STRING")
					AddTextComponentSubstringPlayerName("")
				EndTextCommandScaleformString()
				PushScaleformMovieMethodParameterBool(false)
			EndScaleformMovieMethod()

			Citizen.Wait(100)

			frontend:CreateComboBox(GetLabelText("VED_BLIPN"),vehicleitems)
			frontend:CreateComboBox(GetLabelText("IB_COLOR"),vehicleColours,xnRace.Prefs.colour)
            frontend:CreateComboBox(--[[GetLabelText("PIM_TOUTF")]]"Player Model",playerModels, xnRace.Prefs.playerModel)
			frontend:CreateComboBox(GetLabelText("MO_RST"),radioStationNames,xnRace.Prefs.defaultRadio)
            frontend:CreateButton(GetLabelText("FM_BET_READY"),116)

            -- SetColumnVehInfo(colIndex, rowIndex, unki1, rowIndex2, typeLabel, unki2, value, uknb1, update)
            SetColumnVehInfo(0, 5, 0, 0, GetLabelText("RH_Speed"), 0, GetVehicleModelMaxSpeed(GetHashKey("NERO")) / 100.0, false, true)
            SetColumnVehInfo(0, 6, 0, 1, GetLabelText("RH_Accel"), 0, GetVehicleModelAcceleration(GetHashKey("NERO")), false, true)
            SetColumnVehInfo(0, 7, 0, 2, GetLabelText("RH_Brake"), 0, GetVehicleModelMaxBraking(GetHashKey("NERO")), false, true) -- plane / heli = RH_Handle_Air
            SetColumnVehInfo(0, 8, 0, 3, GetLabelText("RH_Handle"), 0, GetVehicleModelMaxTraction(GetHashKey("NERO")), false, false)

            -- SetFrontendRow(update,col,row,menuID,uniqueID,type,rightTextFill,isSelectable,leftText,unkText,rightTextType,rightText,buttonFill,b13)
            --frontend:UpdateHelpText("")
            BeginScaleformMovieMethodN("SET_DESCRIPTION")
                PushScaleformMovieFunctionParameterInt(0)
                PushScaleformMovieFunctionParameterString("Press \"Ready To Play\" when you are happy with your selections.\nPress ~INPUT_FRONTEND_ACCEPT~ to your current selection as default.")
                PushScaleformMovieMethodParameterBool(false)
                PushScaleformMovieMethodParameterBool(false)
            EndScaleformMovieMethod()

            PushScaleformMovieFunctionN("DISPLAY_DATA_SLOT")
				PushScaleformMovieMethodParameterInt(0)
			EndScaleformMovieMethod()

			PushScaleformMovieFunctionN("SET_COLUMN_FOCUS")
				PushScaleformMovieMethodParameterInt(0)
				PushScaleformMovieMethodParameterInt(1)
				PushScaleformMovieMethodParameterInt(1)
				PushScaleformMovieMethodParameterInt(0)
			EndScaleformMovieMethod()

            UpdateSelectedVehicle()
            UpdatePedModel()
			SetEntityAlpha(selectedVehicle, 255)

			Citizen.CreateThread(function()
				local ready = false
				--local items = {"item1","item2","item3","item4","item5","item6"}
				--local hovered_item = 1
				while IsPauseMenuActive() do
					Citizen.Wait(0)

                    for i = 0,127 do
                        if i ~= PlayerId() then
                            SetPlayerInvisibleLocally(i, true)
                        end
                    end

					if IsControlJustReleased(2, 201) then
						PlaySoundFrontend(-1, "Select", "HUD_FRONTEND_DEFAULT_SOUNDSET", true)

						PushScaleformMovieFunctionN("GET_COLUMN_SELECTION")
						PushScaleformMovieMethodParameterInt(0)
						local ret = EndScaleformMovieMethodReturn()
						while not GetScaleformMovieFunctionReturnBool(ret) do Citizen.Wait(0) end
						local retInt = GetScaleformMovieFunctionReturnInt(ret)
						local selectedButton = frontend:GetObject(retInt+1)

						if selectedButton.rightTextType == -1 then
							local VehicleSelection = frontend:GetObject(1)
							VehicleSelection.isSelectable = false
							frontend:UpdateObject(1)

                            local ModelSelection = frontend:GetObject(3)
							ModelSelection.isSelectable = false
							frontend:UpdateObject(3)

							local ColourSelection = frontend:GetObject(2)
							ColourSelection.isSelectable = false
							frontend:UpdateObject(2)

							ServerEvent("clientReady", {model = vehTable[VehicleSelection.hoveredItem][2],colour = vehColours[ColourSelection.hoveredItem][2], playermodel=playerModels[ModelSelection.hoveredItem]})
                        elseif retInt == 2 then
                            local ModelSelection = frontend:GetObject(3)
                            xnRace.Prefs.playerModel = ModelSelection.hoveredItem
						elseif retInt == 1 then
                            local ColourSelection = frontend:GetObject(2)
                            xnRace.Prefs.colour = ColourSelection.hoveredItem
                        elseif retInt == 3 then
                            local RadioSelection = frontend:GetObject(4)
                            xnRace.Prefs.defaultRadio = RadioSelection.hoveredItem
                        end
					elseif IsControlJustReleased(2, 190) then
						PushScaleformMovieFunctionN("GET_COLUMN_SELECTION")
						PushScaleformMovieMethodParameterInt(0)
						local ret = EndScaleformMovieMethodReturn()
						while not GetScaleformMovieFunctionReturnBool(ret) do Citizen.Wait(0) end
						local retInt = GetScaleformMovieFunctionReturnInt(ret)
						local selectedButton = frontend:GetObject(retInt+1)

						if selectedButton.rightTextType == 0 then
							PlaySoundFrontend(-1, "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET", true)
						 	selectedButton.hoveredItem = selectedButton.hoveredItem + 1
							if not (selectedButton.hoveredItem <= #(selectedButton.items)) then
								selectedButton.hoveredItem = 1
							end
							frontend:UpdateObject(retInt+1)

                            if retInt == 0 or retInt == 1 then
                                UpdateSelectedVehicle()
                            elseif retInt == 2 then
                                UpdatePedModel()
                            end
						end
					elseif IsControlJustReleased(2, 189) then
						PushScaleformMovieFunctionN("GET_COLUMN_SELECTION")
						PushScaleformMovieMethodParameterInt(0)
						local ret = EndScaleformMovieMethodReturn()
						while not GetScaleformMovieFunctionReturnBool(ret) do Citizen.Wait(0) end
						local retInt = GetScaleformMovieFunctionReturnInt(ret)
						local selectedButton = frontend:GetObject(retInt+1)

						if selectedButton.rightTextType == 0 then
							PlaySoundFrontend(-1, "NAV_LEFT_RIGHT", "HUD_FRONTEND_DEFAULT_SOUNDSET", true)
						 	selectedButton.hoveredItem = selectedButton.hoveredItem - 1
							if not (selectedButton.hoveredItem >= 1) then
								selectedButton.hoveredItem = #(selectedButton.items)
							end
							frontend:UpdateObject(retInt+1)

                            if retInt == 0 or retInt == 1 then
                                UpdateSelectedVehicle()
                            elseif retInt == 2 then
                                UpdatePedModel()
                            end
						end
					end
				end
                -- Cleanup
                DestroyCam(menuCam, false)
                SetEntityAsMissionEntity(selectedVehicle)
				DeleteVehicle(selectedVehicle)
				FreezeEntityPosition(GetPlayerPed(-1), false)
				EnterClouds(0, 2)
			end)
		end
	end
end)

local function MakeRaceVehicle(veh)
	SetEntityProofs(veh, true, true, true, true, false, false, 0, false)
	--SetEntitySomething(veh, true)
	N_0x7d6f9a3ef26136a0(veh, false, 0)
	SetVehicleAllowNoPassengersLockon(veh, false)
	SetVehicleEngineOn(veh, true, true, 0)
	N_0xe6f13851780394da(veh, 0.3) -- something to do with turning radius?
	SetHeliBladesFullSpeed(veh)
	SetVehicleStrong(veh, true)
	SetVehicleHasStrongAxles(veh, true)
	SetVehicleNumberPlateText(veh, GetPlayerName(PlayerId()))
	SetPedCanBeKnockedOffVehicle(GetPlayerPed(-1), true)
	SetVehicleDoorsLockedForAllPlayers(veh, true)
    SetAirDragMultiplierForPlayersVehicle(PlayerId(), 1.55) -- if is not a car set to 1.0 otherwise set to either 1.3, 1.55, or 2.0 depending on slipstream maybe? not sure
	SetVehicleDoorsLocked(veh, 4)
	SetEntityInvincible(veh, true)
	SetVehicleOnGroundProperly(veh)

	SetVehicleModKit(veh, 0)
	ToggleVehicleMod(veh, 18, true) -- Xenon lights
	ToggleVehicleMod(veh, 22, true)	-- Turbo
	SetVehicleMod(veh, 16, 5, true)
	SetVehicleMod(veh, 12, 2, true)
	SetVehicleMod(veh, 11, 3, true)
	SetVehicleMod(veh, 15, 3, true)
	SetVehicleMod(veh, 13, 2, true)
	SetVehicleMod(veh, 23, 19, true)
end

AddEventHandler('raceSpawn', function(sd,raceName,firstSpawn)
	if not xnRace.VEHICLEDATA then return end
    local vehData = xnRace.VEHICLEDATA
    local spawnVec = false
    if type(sd) == "vector3" then
        spawnVec = sd
    elseif type(sd) == "table" then
        spawnVec = vec(sd[1]["x"],sd[1]["y"],sd[1]["z"])
    end
	local ped = GetPlayerPed(-1)
	if spawnVec then SetEntityCoords(ped, spawnVec + vec(0,0,2)) else DoScreenFadeIn(500) return end

    printf("Veh Data: %s", json.encode(vehData))

	local modelHash = type(vehData.model) == "string" and GetHashKey(vehData.model) or vehData.model
	RequestModel(modelHash)
	while not HasModelLoaded(modelHash) do Citizen.Wait(0) end

	-- Spawn Vehicle
	xnRace.RACEVEH = CreateVehicle(modelHash, spawnVec, sd[2], true)
	--SetVehicleCustomPrimaryColour(xnRace.RACEVEH, xnRace.Prefs.colour1[1], xnRace.Prefs.colour1[2], xnRace.Prefs.colour1[3])
	--SetVehicleCustomSecondaryColour(xnRace.RACEVEH, xnRace.Prefs.colour2[1], xnRace.Prefs.colour2[2], xnRace.Prefs.colour2[3])
    SetVehicleColours(xnRace.RACEVEH, vehData.colour or 0, vehData.colour or 0)
	MakeRaceVehicle(xnRace.RACEVEH)

    if firstSpawn then
    	FreezeVehicle(xnRace.RACEVEH)
    	SetVehicleBurnout(xnRace.RACEVEH, true)
    	ActivatePhysics(xnRace.RACEVEH)
	end
	SetPedIntoVehicle(ped, xnRace.RACEVEH, -1)
	Citizen.Wait(100)
	if firstSpawn then LeaveClouds() end
	if xnRace.Prefs.defaultRadio then
		local stationName = xnRace.radioStations[xnRace.Prefs.defaultRadio]
		if stationName ~= "FMMC_VEH_RAND" then
			-- yeah nice job gta, forcing me to do this shit
			local bashTheRadio = true
			SetTimeout(2000, function()
				bashTheRadio = false
			end)
			Citizen.CreateThread(function()
				while bashTheRadio do
					Citizen.Wait(0)
					SetRadioToStationName(stationName)
				end
			end)
		end
	end
	while IsPlayerSwitchInProgress() do Citizen.Wait(0) end
	Citizen.Wait(300)
	if firstSpawn then
        ShowRaceInfo(raceName)
	    ServerEvent("clientSpawned")
	else
		SetVehicleEngineOn(xnRace.RACEVEH, false, false, 0)
		--SetVehicleNeedsToBeHotwired(xnRace.RACEVEH, true)
		NetworkFadeInEntity(xnRace.RACEVEH, false)
		--[[SetEntityCollision(xnRace.RACEVEH, false, true)
		Citizen.CreateThread(function()
			print(IsEntityVisible(xnRace.RACEVEH))
			Citizen.Wait(5000)
			SetEntityCollision(xnRace.RACEVEH, true, true)
		end)]]
	end
end)

--[[RegisterCommand("get-selection", function()
	print("Trying...")

	PushScaleformMovieFunctionN("GET_COLUMN_SELECTION")
		PushScaleformMovieMethodParameterInt(0)
	local ret = EndScaleformMovieMethodReturn()

	while not GetScaleformMovieFunctionReturnBool(ret) do Citizen.Wait(0) end

	local retInt = GetScaleformMovieFunctionReturnInt(ret)
	local retStr = SittingTv(ret) -- This native should be named "GetScaleformMovieFunctionReturnString"

	print("Got return value!\nint:"..tostring(retInt).." str:"..retStr)
end, false)]]

RegisterCommand("test-ui", function(source,args)
	local uiEl = tostring(args[1])
	if uiEl ~= "" then
		if uiEl == "vote" then
			ServerEvent("testVote")
		end
		if uiEl == "lobby" then
			local players = {}

			for i=1,32 do
				if GetPlayerPed(i) then
					players[i] = true -- other data here, needs to be moved server side
				end
			end
			xnRace.ShowLobbyUI()
		end
		if uiEl == "scoreboard" then
		end
		if uiEl == "car" then
			xnRace.ShowCarSelectionUI()
		end
	end
end, false)

RegisterCommand("readyup", function(source,args)
	ServerEvent("clientReady")
end, false)

Citizen.CreateThread(function()
	local function doRespawn()
		if xnRace.LASTCHECKPOINT then
			if xnRace.RACEVEH then SetEntityAsMissionEntity(xnRace.RACEVEH, 0, 0) DeleteEntity(xnRace.RACEVEH) end
			local x,y,z = table.unpack(xnRace.LASTCHECKPOINT[3][1])
			TriggerEvent("raceSpawn", {{["x"] = x, ["y"] = y, ["z"] = z},xnRace.LASTCHECKPOINT[3][2]}, "", false)
		end
	end

	local function drawBar(amount)
		local bgcol = {GetHudColour(8)}
		local fgcol = {GetHudColour(6)}
		Bar.DrawRespawnBar("RESPAWNING", 2, amount, bgcol, fgcol)
	end

	local pressedAt = false
	while true do
		Citizen.Wait(0)
		if xnRace.STATE == 2 then
			if not pressedAt and IsControlPressed(1, 23) then
				pressedAt = GetGameTimer()
			elseif IsControlPressed(1, 23) then
				drawBar((GetGameTimer() - pressedAt)/3000)
				if (GetGameTimer() - pressedAt) >= 3000 then
					doRespawn()
					pressedAt = false
				end
			else
				pressedAt = false
			end
		end
	end
end)

RegisterCommand("respawnCar", function(source,args)
	if xnRace.LASTCHECKPOINT then
        local x,y,z = table.unpack(xnRace.LASTCHECKPOINT[3][1])
        TriggerEvent("raceSpawn", {{["x"] = x, ["y"] = y, ["z"] = z},xnRace.LASTCHECKPOINT[3][2]}, "", false)
    end
end, false)

RegisterCommand("speedyclaims", function(source,args)
	SetVehicleHandlingFloat(GetVehiclePedIsIn(GetPlayerPed(-1), false), 'CHandlingData', args[1], args[2] + 0.0)
end, false)

RequestScriptAudioBank("DLC_STUNT/STUNT_RACE_01", false, -1)
RequestScriptAudioBank("DLC_STUNT/STUNT_RACE_02", false, -1)
RequestScriptAudioBank("DLC_STUNT/STUNT_RACE_03", false, -1)

Citizen.CreateThread(function() -- Disable Traffic
	RemoveVehiclesFromGeneratorsInArea(vec(16000,16000,2000),vec(-16000,-16000,-1000))
	while true do
		Citizen.Wait(0)
		SetVehicleDensityMultiplierThisFrame(0.0)
		SetPedDensityMultiplierThisFrame(0.0)
		SetRandomVehicleDensityMultiplierThisFrame(0.0)
		SetParkedVehicleDensityMultiplierThisFrame(0.0)
		SetScenarioPedDensityMultiplierThisFrame(0.0, 0.0)
	end
end)

SetFrontendActive(false)
DoScreenFadeIn(10)
RenderScriptCams(0, 1, 100, 0, 0)
EnterClouds(1)
xnRace.STATE = 0

Citizen.CreateThread(function() -- Debug Display
	if xnRace.DEBUG then
		while true do
			Citizen.Wait(0)
			local playerPos = GetEntityCoords(GetPlayerPed(-1))
			local playerHeading = GetEntityHeading(GetPlayerPed(-1))
			local text = "X: %s Y: %s Z: %s H: %s\tState: %s"
			local text = text:format(math.ceil(playerPos.x), math.ceil(playerPos.y), math.ceil(playerPos.z), math.ceil(playerHeading), tostring(xnRace.STATE))
			SetTextEntry("STRING")
				SetTextColour(255, 255, 255, 255)
				SetTextFont(6)
				SetTextScale(0.3, 0.3)
				SetTextWrap(0.0, 1.0)
				SetTextCentre(false)
				SetTextDropshadow(0, 0, 0, 0, 0)
				AddTextComponentString(text)
			DrawText(0.015, 0.98)
		end
	end
end)