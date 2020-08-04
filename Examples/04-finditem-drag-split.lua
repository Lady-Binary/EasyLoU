-- Search for bandages
FindItem("bandage")
if FINDITEMID == "N/A" then
	print("Bandages not found!")
	return
end
local bandage_id = FINDITEMID[1]

-- Example 1
-- Drag them all
Drag(bandage_id)
sleep(3000)
-- And drop them in my pack
Dropc(BACKPACKID)
sleep(3000)


-- Example 2
-- Split them
ContextMenu(bandage_id, "Split Stack")
sleep(1000)
-- Enter 100 in the split text box
SetInput("StackSplit","TextFieldStackAmount",10)
sleep(1000)
-- Click the OK button
ClickButton("StackSplit",2)
sleep(1000)
-- And drop them in my pack
Dropc(BACKPACKID)
sleep(1000)



