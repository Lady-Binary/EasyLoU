-- Check this to learn more about telegram bots: https://core.telegram.org/bots

-- This is your telegram chat ID
local telegram_chat_id = "XXXXXXXXX"
-- This is your telegram bot API token
local telegram_bot_token = "botXXXXXXXXXX:XXXXXXXXXXX_XXXXXXXXXXXXXXXXXXXXXXX"

-- The actual message
local message = "Hello, World!"

local cmd = "curl -X POST -H \"Content-Type: application/json\" -d \"{\\\"chat_id\\\": \\\"" .. telegram_chat_id .. "\\\", \\\"text\\\": \\\"" .. message .. "\\\", \\\"disable_notification\\\": true}\" https://api.telegram.org/" .. telegram_bot_token .. "/sendMessage"

print (cmd)
os.execute(cmd)
