defmodule FirestormServer.Redis do
  @spec setex(String.t(), integer, any) :: any
  def setex(key, expires_in, value) do
    RedisPoolex.query(["SETEX", key, expires_in, value])
  end

  @spec get(any) :: any | nil
  def get(key) do
    RedisPoolex.query(["GET", key])
  end
end
