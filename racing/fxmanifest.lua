fx_version 'bodacious'
game 'gta5'

resource_type 'gametype' { name = 'Racing' }

file 'Newtonsoft.Json.dll'
file 'Newtonsoft.Json.xml'

client_script 'RacingClient/*.net.dll'
server_script 'RacingServer/*.net.dll'

--dependency 'ugcloader'

author 'Jaymo'
version '0.0.1'
description 'Racing gamemode for FiveM. Very much a WIP for now.'
