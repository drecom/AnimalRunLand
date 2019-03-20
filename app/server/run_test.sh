#!/bin/sh
echo MIX_ENV=test mix ecto.drop
MIX_ENV=test mix ecto.drop
echo MIX_ENV=test mix ecto.create
MIX_ENV=test mix ecto.create
echo MIX_ENV=test mix ecto.migrate
MIX_ENV=test mix ecto.migrate
echo mix test
mix test
