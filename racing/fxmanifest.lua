fx_version 'bodacious'
game 'gta5'

resource_type 'gametype' { name = 'Racing' }

dependency 'mission-json-loader'

loadscreen_manual_shutdown 'yes'

loadscreen 'loadscreen/loadscreen.html'
file 'loadscreen/*.*'

server_scripts {
	-- Shared helpers
	"helpers/sh_*.lua",

	-- Server helpers
	"@mission-json-loader/MissionJSON.lua", -- So that we can decode the JSON on our end, not allowing clients to touch it
	"helpers/sv_*.lua",

	-- Server scripts
	"server-racing-logic.lua",
	"server-main.lua",
}

client_scripts {
	-- Shared helpers
	"helpers/sh_*.lua",
	
	-- Client helpers
	"helpers/cl_*.lua",
	--newtonmeme.json here

	-- Client scripts
	"client-main.lua",
	--"RaceUI.net.dll",
}

author 'Jaymo'
version '0.0.3'
description 'Racing gamemode for FiveM. Very much a WIP for now.'
