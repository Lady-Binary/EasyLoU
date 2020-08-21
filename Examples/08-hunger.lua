-- Search for character window
FindPanel("Character" .. CHARID)

-- Open the character window if it's not open yet
if FINDPANEL == nil then
	SayCustom(".x ToggleCharacterWindow")
	sleep(1000)
end

-- Search for the label containing "Hunger"	
FindLabel("Character" .. CHARID, "Hunger")

-- Remove crap from the text
local Hunger = FINDLABEL[1].TEXT
Hunger = string.gsub(Hunger, 'Hunger', '')
Hunger = string.gsub(Hunger, '%b[]', '')
-- The Hunger variable will contain the hunger level (Full, Peckish, etc.)
print(Hunger)

-- Close the character window
SayCustom(".x ToggleCharacterWindow")
sleep(2000)