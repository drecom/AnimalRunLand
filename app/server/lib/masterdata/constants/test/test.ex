# このコードは、YAMLのマスターデータ仕様から自動生成されたものです。
# 直接編集はしないでください。
defmodule Masterdata.Constants.Test.Test do
  @spec api_version() :: String.t
  def api_version do
    "2017.03.31.001"
  end

  @spec client_only_field() :: integer
  def client_only_field do
    314
  end

  @spec enum_value() :: atom
  def enum_value do
    :blue
  end

  @spec arr() :: list
  def arr do
    [1, 2, 3]
  end

  @spec str_arr() :: list
  def str_arr do
    ["a", "b", "c"]
  end

  @spec enum_arr() :: list
  def enum_arr do
    [:blue, :yellow, :red]
  end

end
