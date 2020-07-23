function ExtractFirstId(ids)
    local first_id = nil
    for id in string.gmatch(ids, "([^,]+)") do
        first_id = id
        break
    end
    return first_id
end

startTime = TIME
while true do
    ClickButton("CraftingWindow", "11") -- craft button
    if TIME - startTime > 60 then -- every 60 seconds...
        sleep(1000)
        FindItem("Scissors", BACKPACKID)
        scissorsId = ExtractFirstId(FINDITEMID) -- get scissor id
        FindItem("tiny", BACKPACKID)
        trashId = ExtractFirstId(FINDITEMID) -- find crafting pack id
        FindItem("Rug Deed", trashId) -- find deeds in crafting pack
        for deed in string.gmatch(FINDITEMID, '([^,]+)') do
            SayCustom(".x use " .. scissorsId .. " Use") -- cut da deeds
            TargetDynamic(deed)
            sleep(1000)
        end
        
        startTime = TIME
        FindPanel("Crafting")
        if FINDPANELID == "N/A" then -- if the panel is gone (tool broke)
            FindItem("Loom")
            SayCustom(".x use " .. FINDITEMID .. " Fabrication") -- use the loom again
            sleep(1000)
            FindButton("CraftingWindow", "Favorites") 
            ClickButton("CraftingWindow", FINDBUTTONNAME) -- click favorites
            FindButton("CraftingWindow", "Ornate") 
            ClickButton("CraftingWindow", FINDBUTTONNAME) -- click item name
        end
    end
end
