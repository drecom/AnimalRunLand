defmodule FirestormServerWeb.Router do
  use FirestormServerWeb, :router

  pipeline :browser do
    plug :accepts, ["html"]
    plug :fetch_session
    plug :fetch_flash
    plug :protect_from_forgery
    plug :put_secure_browser_headers
  end

  pipeline :api do
    plug :accepts, ["json"]
    plug FirestormServer.Platform.Plug
  end

  pipeline :authenticated do
    plug FirestormServer.TokenAuth.Plug
  end

  scope "/", FirestormServerWeb do
    pipe_through :browser # Use the default browser stack

    get "/", PageController, :index
  end

  # Other scopes may use custom stacks.
  # scope "/api", FirestormServerWeb do
  #   pipe_through :api
  # end

  scope "/api/hello", FirestormServerWeb do
    pipe_through :authenticated
    pipe_through :api

    post "/hello", PageController, :hello
  end

  scope "/api/boot", FirestormServerWeb do
    pipe_through :authenticated
    pipe_through :api

    post "/boot", BootController, :boot
  end

  scope "/api/auth", FirestormServerWeb do
    pipe_through :api

    post "/register", AuthController, :register
    post "/accesstoken", AuthController, :accesstoken
  end

  scope "/api/player", FirestormServerWeb do
    pipe_through :authenticated
    pipe_through :api

    post "/signup", PlayerController, :signup
    post "/save_score", PlayerController, :save_score
    post "/save_playerdata", PlayerController, :save_playerdata
  end

  scope "/api/ranking", FirestormServerWeb do
    pipe_through :authenticated
    pipe_through :api

    post "/retrieve", RankingController, :retrieve
  end
end
