require "math"

function round(num, numDecimalPlaces)
  local mult = 10^(numDecimalPlaces or 0)
  return math.floor(num * mult + 0.5) / mult
end

function distance2d(x1,y1,x2,y2)
	if tonumber(x1) == nil or
		tonumber(y1) == nil or
		tonumber(x2) == nil or
		tonumber(y2) == nil then
		return -1
	end

	return math.sqrt((x2 - x1) ^ 2 + (y2 - y1) ^ 2)
end

function distance(x1,y1,z1,x2,y2,z2)
	if tonumber(x1) == nil or
		tonumber(y1) == nil or
		tonumber(z1) == nil or
		tonumber(x2) == nil or
		tonumber(y2) == nil or
		tonumber(z2) == nil then
		return -1
	end
	
	return math.sqrt((x2 - x1) ^ 2 + (y2 - y1) ^ 2 + (z2 - z1) ^ 2)
end

function ExtractFirstId(ids)
    local first_id = nil
    for id in string.gmatch(ids, "([^,]+)") do
        first_id = id
    	break
    end
    return first_id
end

function Equip(item)
    FindItem (item)

	if FINDITEM == nil then
		print(item .. " not found!")
		return	
	end		
    
	local tool_id = FINDITEM[1].ID;
	local cont_id = FINDITEM[1].CNTID;
	
	if tostring(cont_id) == tostring(CHARID) then
		print(item .. " already equipped!")
		return	
	end
	
	print("Equipping " .. item)
	SayCustom(".x use " .. tool_id .. " Equip")
end
