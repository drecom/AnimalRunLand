defmodule FirestormServer.Resource do
  def priv_path(p) do
    Application.app_dir(:firestorm_server, "priv/#{p}")
  end
end
