--[[
	Checkpoint Parsing
	
	Take the checkpoint data from UGC.mission.race and turn it into our own custom type to distribute to clients on our terms.

	Data relevant to checkpoints (located at #.mission.race)
		"chh": [], 			// CHeckpoint Heading (for spawns)
		"chl": [], 			// CHeckpoint Location
		"chp": 33, 			// number of CHeckPoints
		"chs": [], 			// CHeckpoint Scale
		"chs2": [], 		// CHeckpoint Scale (for the second checkpoint)
		"cpado": [],		// CheckPoint ADO?: the x and z components make their way to y and z rotation floats of _GET_VEHICLE_SUSPENSION_BOUNDS in one case. not sure what it actually is for though
		"cpados": [],		// CheckPoint ADO? (for the Second checkpoint)
		"cpbs1": [],		// CheckPoint BitSet 1: ughhhhhhhhhhhhhhhhhh
		"cpbs2": [],		// CheckPoint BitSet 2: ughhhhhhhhhhhhhhhhhh
		"cpdss": [],		// no clue :(
		"cpgrav": [], 		// CheckPoint GRAVity?: unused
		"cpgravdura": [],	// CheckPoint GRAVity? DURAtion: unused
		"cppsst": [],		// CheckPoint S?S? Time:
		"cprst": [],		// CheckPoint ReSet Time: (assumed from other data)
		"cpwwt": [],		// CheckPoint Wrong Way Time: default is 7500 when the value is either -1 or 0, otherwise it is the value * 1000 (compared to game timer)
		"sndchk": [],		// SecoND CHecKpoint: the location of the second checkpoint in a checkpoint pair (from looking at decompiled scripts, not actually tested) 
		"sndrsp": [],		// SecoND ReSPawn heading?: a float related to the heading where you should respawn from (as decompiled scripts point towards chh or this depending on a boolean.)
		"cpair": [], 		// is Checkpoint PAIR: if this checkpoint has a pair to it
		"rndchk": [],		// Round CHecKpoint: if this is true, the checkpoint is a round one (for tubes and stuff)
		"rndchks": [],		// Round CHecKpoint (for the Second checkpoint): same as above for the second checkpoint in a pair
		/// If an index in any of the following is a zero vector, the try the next set, if all sets have zero vectors,
		/// go back one the chain and repeat until the first checkpoint in which case, start at the beginning of the race there.
		/// I believe that's how it goes anyways...
		"vspn0": [],		// Vehicle SPawN 0: The first point to try to respawn the player when they retry from checkpoint
		"vspn1": [],		// Vehicle SPawN 1: The second point to respawn at (if the first is occupied?)
		"vspn2": [],		// Vehicle SPawN 2: The third point to respawn at (if the second is occupied?)

		"vspns0": [],		// Vehicle SPawN (Second checkpoint) 0: same as above for the second checkpoint in a pair
		"vspns1": [],		// Vehicle SPawN (Second checkpoint) 1: ...
		"vspns2": [],		// Vehicle SPawN (Second checkpoint) 2: ...
]]

-- This has been separated out to be moved to a new file or directly into mission-json-loader at some stage but not right now
function CreateUGCObjectCollection(container, numberOfKey, properties)
	local collection = {}

	-- Idiot checks
	if type(container) ~= "table" or type(numberOfKey) ~= "string" or type(properties) ~= "table" then
		return false
	end

	-- Get the number of objects in the collection
	local numObject = container[numberOfKey] or false
	if not numObject then return false end

	-- Get all objects which container actually contains
	local objectProperties = {}
	
	-- Get the properties and given names for them (if they exist) and assign them to our own table
	for keyName, key in pairs(properties) do
		local property = container[key]

		if type(property) == "table" then
			objectProperties[(type(keyName) == "string" and keyName or key)] = property
		end
	end

	-- Populate the collection
	for i=1,numObject do
		local object = {}
		
		for key, valueTable in pairs(objectProperties) do
			object[key] = valueTable[i] or false
		end

		table.insert(collection, object)
	end

	-- Return the collection
	return collection
end

-- Thanks random matlab forum :D
local function GetAngleBetween2dVectors(x1, y1, x2, y2)
	return math.abs(math.deg(math.atan2(x1*y2-y1*x2, x1*x2+y1*y2)))
end

-- Just following what R* does here...
local function GetCheckpointNumberOfArrows(chpLocation, prevChpLocation, nextChpLocation)
	local prevDiff = (chpLocation - prevChpLocation)
	local nextDiff = (chpLocation - nextChpLocation)
	local calculatedAngle = GetAngleBetween2dVectors(prevDiff.x, prevDiff.y, nextDiff.x, nextDiff.y)
	calculatedAngle = calculatedAngle > 180.0 and (360.0 - calculatedAngle) or calculatedAngle

	if calculatedAngle < 80.0 then
		return 3
	elseif calculatedAngle < 140.0 then
		return 2
	elseif calculatedAngle < 180.0 then
		return 1
	end

	return 1
end

local function GetCheckpointType(isRound, numberOfArrows)
	-- TODO: 2060/1604 build compat
	return isRound and 11 + numberOfArrows or 5 + numberOfArrows
end

function GetRaceCheckpoints(race)
	-- Create a new collection and store it in a local variable, once we're done it will be assigned to GlobalState but its much more performant to have a local ref to it
	local checkpoints = CreateUGCObjectCollection(race, "chp", {
		location = "chl",
		scale = "chs",
		isRound = "rndchk",
		spawnHeading = "chh",
		spawnLocation1 = "vspn0",
		spawnLocation2 = "vspn1",
		spawnLocation3 = "vspn2",

		isCheckpointPair = "cpair",

		pairLocation = "sndchk",
		pairScale = "chs2",
		pairIsRound = "rndchks",
		pairSpawnHeading = "sndrsp",
		pairSpawnLocation1 = "vspns0",
		pairSpawnLocation2 = "vspns1",
		pairSpawnLocation3 = "vspns2",
	})

	-- Fast out if getting checkpoints failed
	if not checkpoints then return false end

	-- Find the number of checkpoints
	local numCheckpoints = #checkpoints

	-- Do some additional processing for more information
	for chpIndex, chp in ipairs(checkpoints) do
		-- If we're not the last checkpoint
		if chpIndex < numCheckpoints then
			-- Get the next checkpoint
			local nextChp = checkpoints[chpIndex + 1]
			local prevChp = checkpoints[chpIndex - 1] or checkpoints[chpIndex]

			-- Add the checkpoint's target(s)
			if chp.isPair then
				if nextChp.isPair then
					-- If BOTH the current and the next checkpoint ARE checkpoint pairs
					chp.target = nextChp.location
					chp.pairTarget = nextChp.pairLocation
				else
					-- If this checkpoint IS a checkpoint pair but the next checkpoint IS NOT
					chp.target = nextChp.location
					chp.pairTarget = nextChp.location
				end
			else
				if nextChp.isPair then
					-- If this checkpoint IS NOT a checkpoint pair but the next checkpoint IS
					chp.target = vec3((chp.location.xy + nextChp.location.xy)/2, chp.location.z) -- Midpoint between the checkpoint pairs without factoring Z
				else
					-- If BOTH the current and the next checkpoint are NOT checkpoint pairs
					chp.target = nextChp.location
				end
			end

			-- Determine the type this checkpoint should be
			chp.type = GetCheckpointType(chp.isRound, GetCheckpointNumberOfArrows(chp.location, prevChp.location, nextChp.location))
			chp.pairType = chp.type -- TODO: checkpoint pair correct types
		else
			-- This is the last checkpoint, it gets a special flag
			-- TODO: lap races, they duplicate blips!!!!
			chp.isLastCheckpoint = true
			chp.type = chp.isRound and 16 or 10
			chp.target = chp.location
		end

		-- All checkpoints get a vertical height adjustment, so add it here
		-- This is probably actually calculable from the checkpoint sprite size scaling/icon position however, I don't think those values are that easy to get.
		local locationAdjust = vec3(0,0,chp.isRound and 11.0 or 4.0)
		local targetAdjust = vec3(0,0,5.0) -- Target always gets z+5
		chp.location = chp.location + locationAdjust
		chp.target = chp.target + targetAdjust

		if chp.isPair then
			chp.pairLocation = chp.pairLocation + locationAdjust
			chp.pairTarget = chp.pairTarget + targetAdjust
		end

		-- The radius is determined independent of other checkpoints, so it's out here
		chp.radius = chp.isRound and 20.0 or 11.0
		chp.pairRadius = chp.radius -- TODO: checkpoint pair correct radius

		-- Varies based on race type I believe but 9.5 is pretty much the right value most of the time so,
		-- we'll just always use it, for now...
		chp.cylinderHeight = 9.5 

		-- Future-proofing!
		chp.baseHudColour = 13
		chp.iconHudColour = 134
		chp.subType = 0
	end

	-- Finally, assign the checkpoints to GlobalState
	GlobalState.RacingCheckpoints = checkpoints

	-- And return the object so we can save our own certified™ copy
	return checkpoints
end
