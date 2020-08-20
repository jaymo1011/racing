local msgStart = "^3Racing ^0>> ^7"
local msgEnd = "^7\n"

function printf(msg, ...)
	Citizen.Trace(msgStart..string.format(msg or nil, ...)..msgEnd)
end