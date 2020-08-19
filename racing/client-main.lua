-- Client should have access to statebags because, they just should :P
if not LocalPlayer then error("what????????") end

-- I don't care about blocking the main thread, we don't want to do ANYTHING until we have a local state to do things with.
while not LocalPlayer.state._RacingStateLoaded do Wait(200) end 

print("yay lets do this!!!")

-- yes, you will participate, you get no other option :D
LocalPlayer.state:set("RacingIntent", "participate", true)