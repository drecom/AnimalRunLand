defmodule FirestormServer.Crypto do
  # http://stackoverflow.com/questions/37629194/how-to-encrypt-and-decrypt-with-aes-cbc-128-in-elixir
  defp pad(data, block_size) do
    to_add = block_size - rem(byte_size(data), block_size)
    data <> to_string(:string.chars(to_add, to_add))
  end

  defp unpad(data) do
    to_remove = :binary.last(data)
    :binary.part(data, 0, byte_size(data) - to_remove)
  end

  defp load_keys do
    key = File.read! FirestormServer.Resource.priv_path("crypto/crypto_key.dat")
    iv = File.read! FirestormServer.Resource.priv_path("crypto/crypto_iv.dat")
    {key, iv}
  end

  @spec encode(String.t) :: String.t
  def encode(decoded_text) do
    {key, iv} = load_keys()
    crypted = :crypto.block_encrypt(:aes_cbc128, key, iv, pad(decoded_text, 16))
    Base.encode64(crypted)
  end

  @spec decode(String.t) :: String.t
  def decode(encoded_text) do
    {key, iv} = load_keys()
    {:ok, plane_crypted} = Base.decode64(encoded_text)
    decrypted = :crypto.block_decrypt(:aes_cbc128, key, iv, plane_crypted)
    unpad(decrypted)
  end
end
