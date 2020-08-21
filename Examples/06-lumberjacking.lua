--[[
Fully automated harvesting script.  Needs to be customized for your uses.

MoveEx()  - gets you to a spot reliably.
dropoff() - casts Gate to get you to your dropoff spot and back and NOT mess up the current "rail" of harvesting
defend()  - if attacked, it'll move away, make pets attack, and spam fireball.  Once done, it'll move back and equip hatchet again.  Customize how you need it.
doTree()  - harvests tree, uses prospect tool, and watches for mobs. Will move on after backpack weight stops increasing.
	    X, Y and Z decimal separator must be a comma and values must be sent as strings e.g. doTree("420,0", "420,1", "420,2")
]]--

gateToHouseKey = 10 -- key to cast gate to dropoff location
dropoffBoxId = <id of dropbox>
attackSpellKey = 11

function Equip(item) -- stole this from utils.lua
    FindItem (item)
	if FINDITEM == nil then
		print(item .. " not found!")
		return	
	end		
	local tool_id = FINDITEM[1].ID
	local cont_id = FINDITEM[1].CNTID
	if cont_id == CHARID then
		print(item .. " already equipped!")
		return	
	end
	print("Equipping " .. item)
	ContextMenu(tool_id, "Equip")
end

function MoveEx(x,y,z) -- starts you moving towards coordinates and waits until you're there before continuing
	count = 0
	repeat
		count = count + 1
		Move(x,y,z)
		sleep(300)
	until (CHARPOSX == x and CHARPOSZ == z) or count > 30
end

function dropoff()
	print('dropping off...')
	repeat
		Macro(gateToHouseKey) -- this is casting gate from a hotkey to my house
		sleep(4000)
		FindPanel("Moongate")
		if FINDPANEL == nil then
			panelId = nil
		else
			panelId = FINDPANEL[1].ID
		end
	until string.match(panelId, "ConfirmMoongate")
	ClickButton(FINDPANEL[1].ID, "0") -- "0" is the confirmation to travel button
	sleep(3000)
	FindItem("Logs",BACKPACKID) -- finds all logs in my backpack
	for k, v in pairs(FINDITEM) do
		Drag(v.ID)
		Dropc(dropoffBoxId) -- and drops them in a chest at my house
		sleep(500)
	end
	FindItem("Kindling",BACKPACKID) -- moves kindling
	Drag(FINDITEM[1].ID)
	Dropc(dropoffBoxId)
	sleep(500)
	FindItem("Apple",BACKPACKID) -- and apples
	Drag(FINDITEM[1].ID)
	Dropc(dropoffBoxId)
	sleep(500)
	FindItem("Portal") -- and travels back through the moongate
	UseSelected(FINDITEM[1].ID)
	sleep(1000)
	FindPanel("Moongate")
	ClickButton(FINDPANEL[1].ID, "0")
	sleep(3000)
	Equip("Hatchet") -- don't forget to re-equip your hatchet!
end
	
function defend()
	while MONSTERSNEARBY == 'True' do -- if a monster is within maybe 5 units,
		local x = CHARPOSX
		local y = CHARPOSY
		local z = CHARPOSZ
		mId_s = NEARBYMONSTERS[1].ID
		mId = mId_s
		Move(x-5,y,z) -- run away a bit
		sleep(1000)
		Say('all follow me') -- get your pets with you
		sleep(1000)
		Say('all kill') 
		TargetDynamic(mId) -- make the pets attack mob (stupid boglings!)
		for i=1,10 do
			Macro(attackSpellKey) -- fireball on my hotbar
			TargetDynamic(mId) 
		end
		print('mob dead')
		Equip("hatchet") -- re-equip yourself!
		MoveEx(x,y,z)
		FindPermanent("Tree")  -- and re-engage the tree
		closestId = FINDPERMANENT[1].ID
		Macro(28)
		sleep(500)
		TargetPermanent(closestId)
	end
end

function doTree(x,y,z)
	Equip("Hatchet")
	MoveEx(x,y,z)
	FindPermanent("Tree")  -- find all trees
	closestId = FINDPERMANENT[1].ID  -- finds the closest one
	print('At spot with x/z: ' .. x .. ',' .. z)
	Macro(28) -- the 'q' key
	sleep(500)
	TargetPermanent(closestId)
	print('whacking tree')
	sleep(1000)
	repeat
		defend() -- checks for nearby monsters and defends
		startWeight = CHARWEIGHT
		print("startWeight" .. startWeight)
		FindItem("prospector", BACKPACKID)
		prospectId = FINDITEM[1].ID
		UseSelected(prospectId)  -- uses prospecting tool from pack on tree
		TargetPermanent(closestId)
		sleep(4000)
		endWeight = CHARWEIGHT
		print("endWeight" .. endWeight)
		print('start/end weight: ' .. startWeight .. ','..endWeight)
	until startWeight == endWeight  -- will move to next tree if, within 4 seconds-ish, weight didn't change (as in the tree is gone)
	if endWeight > 300 then -- getting full, dropoff time!
		dropoff()
	end
end

while true do 
	Macro(30)
	sleep(4000)
	doTree(<x>,<y>,<z>) -- add your own tree coordinates
	doTree(<x>,<y>,<z>)
	doTree(<x>,<y>,<z>)
	doTree(<x>,<y>,<z>)
	... -- a lot of tree coordinates...
end