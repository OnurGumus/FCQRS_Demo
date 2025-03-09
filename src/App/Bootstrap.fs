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

let handleEventWrapper env (offsetValue: int64) (event:obj)=
    let log = (env :> ILoggerFactory).CreateLogger("Event")
    log.LogInformation("Event: {0}", event.ToString())

    let dataEvent =
        match event with
        | :? Event<User.Event> as  event ->
            printfn "Event: %A" event
            []
        | _ -> []

    dataEvent

let offsetCount =  0
open FCQRS.Query
let sub:ISubscribe<obj>  = FCQRS.Query.init actorApi offsetCount (handleEventWrapper env)