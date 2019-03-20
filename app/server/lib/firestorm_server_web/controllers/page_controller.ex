defmodule FirestormServerWeb.PageController do
  use FirestormServerWeb, :controller
  require Logger

  def index(conn, _params) do
    render conn, "index.html"
  end

  @spec hello(Phoenix.Controller, map) :: Phoenix.Controller | no_return
  def hello(conn, params) do
    _ = Logger.info("hello device : #{conn.assigns.device.device_id}")

    _ = Logger.info("hello platform : #{conn.assigns.platform}")

    params_map = FirestormServer.Commu.extract_params_map(params)
    FirestormServer.Commu.validate_request(conn, params_map, "hello", "hello")

    _ = Logger.info(params_map["key"])
    _ = Logger.info(Masterdata.Test.TestEnum.to_atom!(params_map["enum_value"]))

    test_master_list = Masterdata.Test.Test.load()
    test_master = Enum.at(test_master_list, 0)
    response_map = %{
      key: "pong",
      boolean_value: true,
      intar: [1, 2, 3],
      test_master: test_master,
      test_model: %{name: "モデル名前", description: "説明"},
      enum_value: Masterdata.Test.TestEnum.to_integer!(:red)
    }

    response_map = Map.put(response_map, :nullable_value, 3)

    FirestormServer.Commu.response(conn, response_map, "hello", "hello")
  end
end
