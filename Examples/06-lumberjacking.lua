--[[
Fully automated harvesting script.  Needs to be customized for your uses.

MoveEx() - gets you to a spot reliably.
dropoff() - casts Gate to get you to your dropoff spot and back and NOT mess up the current "rail" of harvesting
defend() - if attacked, it'll move away, make pets attack, and spam fireball.  Once done, it'll move back and equip hatchet again.  Customize how you need it.
doTree() - harvests tree, uses prospect tool, and watches for mobs. Will move on after backpack weight stops increasing
]]--


function ExtractFirstId(ids) -- stole this from utils.lua
    local first_id = nil
    for id in string.gmatch(ids, "([^,]+)") do
        first_id = id
    	break
    end
    return first_id
end

function Equip(item) -- stole this from utils.lua
    FindItem (item)
	local tool_id = ExtractFirstId(FINDITEMID)
	local cont_id = ExtractFirstId(FINDITEMCNTID)
	if tool_id == nil or tool_id == "N/A" then
		print(item .. " not found!")
		return	
	end		
	if tostring(cont_id) == tostring(CHARID) then
		print(item .. " already equipped!")
		return	
	end
	print("Equipping " .. item)
	SayCustom(".x use " .. tool_id .. " Equip")
end

function MoveEx(x,y,z) -- starts you moving towards coordinates and waits until you're there before continuing
	count = 0
	repeat
		count = count + 1
		Move(x,y,z)
		sleep(300)
	until ((tonumber(CHARPOSX) == x) and (tonumber(CHARPOSZ) == z)) or count > 30
end

function dropoff()
	print('dropping off...')
	repeat
		Macro(3) -- this is casting gate from a hotkey to my house
		sleep(4000)
		FindPanel("Moongate")
	until string.match(FINDPANELID, "ConfirmMoongate")
	ClickButton(FINDPANELID, "0") -- "0" is the confirmation to travel button
	sleep(3000)
	FindItem("Logs",tonumber(BACKPACKID)) -- finds all logs in my backpack
	for id in string.gmatch(FINDITEMID, "([^,]+)") do
		Drag(id)
		Dropc(<container id you want to drop into>) -- and drops them in a chest at my house
		sleep(500)
	end
	FindItem("Kindling",tonumber(BACKPACKID)) -- moves kindling
	Drag(FINDITEMID)
	Dropc(<container id you want to drop into>)
	sleep(500)
	FindItem("Apple",tonumber(BACKPACKID)) -- and apples
	Drag(FINDITEMID)
	Dropc(<container id you want to drop into>)
	sleep(500)
	FindItem("Portal") -- and travels back through the moongate
	UseSelected(tonumber(FINDITEMID))
	sleep(1000)
	FindPanel("Moongate")
	ClickButton(FINDPANELID, "0")
	sleep(3000)
	Equip("Hatchet") -- don't forget to re-equip your hatchet!
end
	
function defend()
	while MONSTERSNEARBY == 'True' do -- if a monster is within maybe 5 units,
		local x = CHARPOSX
		local y = CHARPOSY
		local z = CHARPOSZ
		mId_s = ExtractFirstId(MONSTERSID)
		mId = tonumber(mId_s)
		Move(x-5,y,z) -- run away a bit
		sleep(1000)
		Say('all follow me') -- get your pets with you
		sleep(1000)
		Say('all kill') 
		TargetDynamic(mId) -- make the pets attack mob (stupid boglings!)
		for i=1,10 do
			Macro(10) -- fireball on my hotbar
			TargetDynamic(mId) 
		end
		print('mob dead')
		Equip("hatchet") -- re-equip yourself!
		MoveEx(x,y,z)
		FindPermanent("Tree")  -- and re-engage the tree
		closestId = ExtractFirstId(FINDPERMAID)
		Macro(28)
		sleep(500)
		TargetPermanent(closestId)
	end
end

function doTree(x,y,z)
	Equip("Hatchet")
	MoveEx(x,y,z)
	FindPermanent("Tree")  -- find all trees
	closestId = ExtractFirstId(FINDPERMAID)  -- finds the closest one
	print('At spot with x/z: ' .. x .. ',' .. z)
	Macro(28) -- the 'q' key
	sleep(500)
	TargetPermanent(closestId)
	print('whacking tree')
	sleep(1000)
	repeat
		defend() -- checks for nearby monsters and defends
		startWeight = CHARWEIGHT
		FindItem("prospector", BACKPACKID)
		prospectId = ExtractFirstId(FINDITEMID)
		UseSelected(prospectId)  -- uses prospecting tool from pack on tree
		TargetPermanent(closestId)
		sleep(4000)
		endWeight = CHARWEIGHT
		print('start/end weight: ' .. startWeight .. ','..endWeight)
	until startWeight == endWeight  -- will move to next tree if, within 4 seconds-ish, weight didn't change (as in the tree is gone)
	if tonumber(endWeight) > 300 then -- getting full, dropoff time!
		dropoff()
	end
end

while true do 
	Macro(30)
	sleep(4000)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
	doTree(<spotX>, <spotY>, <spotZ>)
end
