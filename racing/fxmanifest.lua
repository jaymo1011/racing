fx_version 'bodacious'
game 'gta5'

resource_type 'gametype' { name = 'Racing' }

dependency 'missionjsonloader'

loadscreen_manual_shutdown 'yes'

loadscreen 'loadscreen/loadscreen.html'
file 'loadscreen/*.*'

client_scripts {
	-- Helpers and stuff go here

	"client-main.lua",
}

server_scripts {
	"server-main.lua",
}

author 'Jaymo'
version '0.0.3'
description 'Racing gamemode for FiveM. Very much a WIP for now.'
