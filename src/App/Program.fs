open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Hocon.Extensions.Configuration
open System.IO
open FCQRS.Model.Data

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

let userSubs =  actorApi.CreateCommandSubscription userShard

let register  cid userName =
        let actorId: ActorId = userName |> ValueLens.CreateAsResult |> Result.value
        async {
            let! subscribe = userSubs cid actorId (User.Register(userName)) (fun (e: User.Event) -> e.IsRegisterSucceeded)

            match (subscribe: FCQRS.Common.Event<_>) with
            | { EventDetails = User.RegisterSucceeded
                Version = v } -> return Ok v
            | { EventDetails = _; Version = v } ->
                return Error [ sprintf "Registration failed for user %s" <| actorId.ToString() ]
        }


let cid : CID = System.Guid.NewGuid().ToString()  |> ValueLens.CreateAsResult |> Result.value
let userName = "test"
let result = register cid userName |> Async.RunSynchronously
printfn "%A" result