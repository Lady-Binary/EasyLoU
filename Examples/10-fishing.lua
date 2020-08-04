function Equip(item)
    FindItem (item)
    local tool_id = FINDITEMID[1]
    local cont_id = FINDITEMCNTID[1]
    
    if tool_id == nil or
        tool_id == "N/A" then
        print(item .. " not found!")
        return    
    end        
    
    if tostring(cont_id) == tostring(CHARID) then
        print(item .. " already equipped!")
        return    
    end
    
    print("Equipping " .. item)
    ContextMenu(tool_id, "Equip")
end

function doFish(x,y,z)
    Equip("fishing rod")
    while true do
        count = 0
        Macro(28) -- cast
        print('cast activated')
        sleep(500)
        TargetLoc(x,y,z) -- target world
        sleep(500)
        Macro(20) -- hide
        print('hide activated')
        repeat
            count = count + 1
            ScanJournal(TIME)
            if string.match(SCANJOURNALMESSAGE, "seem to be any") then -- if the spot is dry, return
                sleep(1000)
                return
            end
            if count > 100 then -- somethin went wrong, wait a while and exit
                break
            end
        until string.match(SCANJOURNALMESSAGE, "caught") or string.match(SCANJOURNALMESSAGE, "fish are biting") or string.match(SCANJOURNALMESSAGE, "slippery") or string.match(SCANJOURNALMESSAGE, "You fish up a")
        sleep(1000)
    end
end

doFish(CLICKWORLDX, CLICKWORLDY, CLICKWORLDZ)
