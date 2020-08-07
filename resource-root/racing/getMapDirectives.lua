print("now isn't this cool!!!!'")

-- This would be much harder in C# with its typed nature so, it's in here.
local currentMapUGCData = "hiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii"
exports("getCurrentMapData", function() return currentMapUGCData end)


local function addUgcData(_, ugcData)
	currentMapUGCData = ugcData
	TriggerEvent("onMapDataAvailable")
end

local function addUgcUrl(_, ugcUrl)
	
end

local function deleteUgc()
	currentMapUGCData = "[]"
end

AddEventHandler('getMapDirectives', function(add)
    -- Add the ugcData directive
	add("ugcData", addUgcData, deleteUgc)
	add("ugcUrl", addUgcUrl, deleteUgc)
	add("__isSurrogate", function(state, arg)
		if not IsDuplicityVersion() and arg == "Yes, I am the UGC surrogate and there is only one of me!" then
			local jsonFilename = GetConvar("racing_missionDataFile", "stunt-race.json")
			addUgcData(state, LoadResourceFile("racing-map-one", "stream/"..jsonFilename))
		end
	end, deleteUgc)
	--[[
    add('ugcData', function(state, model)
        -- return another callback to pass coordinates and so on (as such syntax would be [spawnpoint 'model' { options/coords }])
        return function(opts)
            local x, y, z, heading

            local s, e = pcall(function()
                -- is this a map or an array?
                if opts.x then
                    x = opts.x
                    y = opts.y
                    z = opts.z
                else
                    x = opts[1]
                    y = opts[2]
                    z = opts[3]
                end

                x = x + 0.0001
                y = y + 0.0001
                z = z + 0.0001

                -- get a heading and force it to a float, or just default to null
                heading = opts.heading and (opts.heading + 0.01) or 0

                -- add the spawnpoint
                addSpawnPoint({
                    x = x, y = y, z = z,
                    heading = heading,
                    model = model
                })

                -- recalculate the model for storage
                if not tonumber(model) then
                    model = GetHashKey(model, _r)
                end

                -- store the spawn data in the state so we can erase it later on
                state.add('xyz', { x, y, z })
                state.add('model', model)
            end)

            if not s then
                Citizen.Trace(e .. "\n")
            end
        end
        -- delete callback follows on the next line
    end, function(state, arg)
        -- loop through all spawn points to find one with our state
        for i, sp in ipairs(spawnPoints) do
            -- if it matches...
            if sp.x == state.xyz[1] and sp.y == state.xyz[2] and sp.z == state.xyz[3] and sp.model == state.model then
                -- remove it.
                table.remove(spawnPoints, i)
                return
            end
        end
    end)
	]]
end)
