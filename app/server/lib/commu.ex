defmodule FirestormServer.Commu do
  @spec to_json_string(map) :: String.t
  def to_json_string(m) do
    json_encoder = Application.get_env(:phoenix, :format_encoders) |> Keyword.get(:json, Poison)
    json_encoder.encode!(m)
  end

  @spec json_to_map(String.t) :: map
  def json_to_map(s) do
    json_encoder = Application.get_env(:phoenix, :format_encoders) |> Keyword.get(:json, Poison)
    json_encoder.decode!(s)
  end

  @spec validate_request(Phoenix.Controller, map, String.t, String.t) :: any | no_return
  def validate_request(conn, params_map, schema_module, schema) do
    schema_path = "schema/request/api.net.#{schema_module}.#{schema}_schema.json"
    schema = Poison.decode!(File.read! FirestormServer.Resource.priv_path(schema_path))
      |> ExJsonSchema.Schema.resolve
    is_valid = ExJsonSchema.Validator.valid?(schema, params_map)

    if !is_valid do
      params_str = to_json_string(params_map)
      IO.puts "schema validation request error : " <> params_str <> " as "
      IO.inspect schema
      conn |> Phoenix.Controller.json(%{error: "Schema validation error. Please check request data format."})
      raise ArgumentError, message: "invalid argument"
    end
  end

  @spec extract_params_map(map) :: map
  def extract_params_map(params) do
    params["p"]
  end

  @spec response(Phoenix.Controller, map, String.t, String.t) :: Phoenix.Controller | no_return
  def response(conn, response_map, schema_module, schema) do
    stringnized_response_map = Enum.into(response_map, %{}, fn {key, value} -> {Atom.to_string(key), value} end)

    schema_path = "schema/response/api.net.#{schema_module}.#{schema}_schema.json"
    schema = Poison.decode!(File.read! FirestormServer.Resource.priv_path(schema_path))
      |> ExJsonSchema.Schema.resolve

    is_valid = case ExJsonSchema.Validator.validate(schema, stringnized_response_map) do
      :ok -> true
      {:error, error_seq} ->
        _ = IO.inspect response_map
        _ = IO.inspect error_seq
        false
    end

    response_json = to_json_string(response_map)
    if is_valid do
      response = case crypto?() do
        true -> FirestormServer.Crypto.encode(response_json)
        false -> response_json
      end
      conn |> Phoenix.Controller.text(response)
    else
      IO.puts "schema validation error : " <> response_json <> " as "
      IO.inspect schema
      conn |> Phoenix.Controller.json(%{error: "Schema validation error. Please check response data format."})
      raise ArgumentError, message: "invalid argument"
    end
  end

  @spec crypto?() :: boolean
  def crypto?() do
    Application.get_env(:firestorm_server, :crypto)
  end
end
