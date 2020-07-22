-- Search for character window
FindPanel("Character" .. CHARID)

-- Open the character window if it's not open yet
if FINDPANELID == "N/A" then
	SayCustom(".x ToggleCharacterWindow")
	sleep(2000)
end

-- Search for the label containing "Hunger"	
FindLabel("Character" .. CHARID, "Hunger")
print(FINDLABELTEXT)

-- Remove crap from the text
local Hunger = FINDLABELTEXT
Hunger = Hunger:gsub('Hunger', '')::trim
Hunger = Hunger:gsub('%b[]', '')
-- The Hunger variable will contain the hunger level (Full, Peckish, etc.)
print(Hunger)

-- Close the character window
SayCustom(".x ToggleCharacterWindow")
sleep(2000)

