--Digs with any shovel in your backpack 2 units to the west of your current character location

FindItem("shovel", BACKPACKID)
shovelId = FINDITEM[1].ID
UseSelected(shovelId)
TargetLoc(CHARPOSX - 2, CHARPOSY, CHARPOSZ)
sleep(1000)