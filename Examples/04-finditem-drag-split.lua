require "utils"

-- Search for bandages
FindItem("bandage")
if FINDITEMID == "N/A" then
	print("Bandages not found!")
	return
end
local bandage_id = ExtractFirstId(FINDITEMID)

-- Example 1

-- Drag them all
Drag(bandage_id)
sleep(3000)
-- And drop them in my pack
Dropc(BACKPACKID)
sleep(3000)

-- Example 2

-- Split them
SayCustom(".x use " .. bandage_id .. " Split Stack")
sleep(3000)
-- Enter 100 in the split text box
SetInput("StackSplit","TextFieldStackAmount",10)
sleep(3000)
-- Click the OK button
ClickButton("StackSplit",2)
sleep(3000)
-- And drop them in my pack
Dropc(BACKPACKID)
sleep(3000)



