defmodule FirestormServerWeb.BootControllerTest do
  use FirestormServerWeb.ConnCase
  require FirestormServerWeb.ControllerTestHelper

  test "POST /api/boot", %{conn: conn} do
    # no param
    {response_map, _access_token} = FirestormServerWeb.ControllerTestHelper.test_post(conn, "api/boot/boot")
    assert Map.has_key?(response_map, "masterdata_json")
    assert Map.has_key?(response_map, "api_version")

    # param api_version in masterdata
    params = %{client_api_version: "2017040100000000"}
    {response_map, _access_token} = FirestormServerWeb.ControllerTestHelper.test_post(conn, "api/boot/boot", params)
    assert Map.has_key?(response_map, "masterdata_json")
    assert Map.has_key?(response_map, "api_version")
    assert Map.has_key?(response_map, "api_url")
    assert response_map["api_url"], "http://develop1.moequiz.techarts.co.jp:8888"

    # param api_version not in masterdata
    params = %{client_api_version: "1962082102000000"}
    {response_map, _access_token} = FirestormServerWeb.ControllerTestHelper.test_post(conn, "api/boot/boot", params)
    assert Map.has_key?(response_map, "masterdata_json")
    assert Map.has_key?(response_map, "api_version")
    assert Map.has_key?(response_map, "api_url")
    assert response_map["api_url"], ""
  end
end
