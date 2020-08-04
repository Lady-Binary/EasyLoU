nameInFavoritesTab = "Rug"
nameToCutUp = "Rug Deed"
craftButton = "10"

function recycle()
	FindItem("Scissors", BACKPACKID)
	scissorsId = FINDITEMID[1]
	FindItem("tiny", BACKPACKID)
	craftBagId = FINDITEMID[1]
	FindItem(nameToCutUp , craftBagId)
	if FINDITEMID ~= "N/A" then
		for deed in FINDITEMID do
			SayCustom(".x use " .. scissorsId .. " Use")
			TargetDynamic(deed)
			sleep(1500)
		end
	end
end

startTime = TIME
while true do
	ClickButton("CraftingWindow", craftButton)
	if TIME - startTime > 60 then
		sleep(1000)
		recycle()
		startTime = TIME
		FindPanel("Crafting")
		if FINDPANELID == "N/A" then
			FindItem("Loom")
			SayCustom(".x use " .. FINDITEMID[1] .. " Fabrication")
			sleep(1000)
			FindButton("CraftingWindow", "Favorites")
			ClickButton("CraftingWindow", FINDBUTTONNAME[1])
			FindButton("CraftingWindow", nameInFavoritesTab)
			ClickButton("CraftingWindow", FINDBUTTONNAME[1])
		end
	end
end