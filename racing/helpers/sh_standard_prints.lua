local msgStart = IsDuplicityVersion() and "^3Racing ^0>> ^7" or "Racing: "
local msgEnd = IsDuplicityVersion() and "^7\n" or ""

function printf(msg, ...)
	Citizen.Trace(msgStart..string.format(msg or nil, ...)..msgEnd)
end
