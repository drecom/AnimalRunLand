#!/bin/bash
#MIX_ENV=prod elixir --detached -e "File.write! '.pid', :os.getpid"  -S mix phx.server
elixir --detached -e "File.write! '.pid', :os.getpid"  -S mix phx.server
