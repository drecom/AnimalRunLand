# このコードは、YAMLのマスターデータ仕様から自動生成されたものです。
# 直接編集はしないでください。

defmodule Masterdata.Test.TestEnum do
  @spec to_integer!(atom) :: integer
  def to_integer!(e) do
    map = %{
      :blue => 1, # 青
      :yellow => 2, # 黃
      :red => 3, # 赤
    }
    case map[e] do
      nil -> raise ArgumentError, message: "Masterdata.Test.TestEnum.to_integer! : #{e} is invalid argument"
      x -> x
    end
  end

  @spec to_atom!(integer) :: atom
  def to_atom!(n) do
    map = %{
      1 => :blue, # 青
      2 => :yellow, # 黃
      3 => :red, # 赤
    }
    case map[n] do
      nil -> raise ArgumentError, message: "Masterdata.Test.TestEnum.to_atom! : #{n} is invalid argument"
      x -> x
    end
  end
end
