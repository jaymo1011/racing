fx_version 'bodacious'
game 'gta5'

resource_type 'gametype' { name = 'Racing' }

file 'Client/bin/Release/**/publish/*.dll'
file 'Newtonsoft.Json.dll'
file 'Newtonsoft.Json.xml'

client_script 'Client/bin/Release/**/publish/*.net.dll'
server_script 'Server/bin/Release/**/publish/*.net.dll'

author 'Jaymo'
version '0.0.1'
description 'Racing gamemode for FiveM. Very much a WIP for now.'
