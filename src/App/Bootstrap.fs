module BootStrap

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Hocon.Extensions.Configuration
open System.IO

let configBuilder =
    ConfigurationBuilder()
        .AddHoconFile(Path.Combine(__SOURCE_DIRECTORY__, "config.hocon"))

let config = configBuilder.Build()

let loggerF =
    LoggerFactory.Create(fun builder -> builder.AddConsole() |> ignore) 

let env = new Environments.AppEnv(config,loggerF)

let loggerFactory = env :> ILoggerFactory

let actorApi = FCQRS.Actor.api config loggerFactory

let sagaCheck  _ = []

actorApi.InitializeSagaStarter sagaCheck

let userShard = User.Actor.factory env actorApi

User.Actor.init env actorApi |> ignore

let userSubs cid =  actorApi.CreateCommandSubscription userShard cid
