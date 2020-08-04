--Digs with any shovel in your backpack 2 units to the west of your current character location

FindItem("shovel", tonumber(BACKPACKID))
shovelId = FINDITEMID[1]
UseSelected(shovelId)
TargetLoc(true, tonumber(CHARPOSX) - 2, tonumber(CHARPOSY), tonumber(CHARPOSZ), 0)
sleep(1000)