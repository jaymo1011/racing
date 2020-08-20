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
	"helpers/sv_*.lua",

	-- Server script
	"server-main.lua",
}

client_scripts {
	-- Shared helpers
	"helpers/sh_*.lua",
	
	-- Client helpers
	"helpers/cl_*.lua",

	-- Client script
	"client-main.lua",
}

author 'Jaymo'
version '0.0.3'
description 'Racing gamemode for FiveM. Very much a WIP for now.'
