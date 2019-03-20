defmodule PlatformType do
  @spec to_int(atom) :: integer
  def to_int(platform) do
    case platform do
      :ios -> 1
      :android -> 2
      _ -> 0
    end
  end

  @spec to_platform(integer) :: atom
  def to_platform(platform_num) do
    case platform_num do
      1 -> :ios
      2 -> :android
      _ -> :unknown
    end
  end
end
