AddEventHandler("vehicleComponentControlEvent", function(sender, event)
	print(sender, json.encode(event))
end)
