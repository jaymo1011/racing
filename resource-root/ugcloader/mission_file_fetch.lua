AddEventHandler("onMapStart", function(resource)
	if GetNumResourceMetadata(resource, "ugc_url") > 0 then
		local ugcUrl = GetResourceMetadata(resource, "ugc_url", 0)
		print("downloading UGC URL: "..ugcUrl)
		print("well.. it actually not implemented yet!")
	end
end)