module User

open FCQRS.Model.Data
open FCQRS.Common

type Event =
    | LoginSucceeded
    | LoginFailed
    | RegisterSucceeded
    | AlreadyRegistered

type Command =
    | Login of string
    | Register of string
  

type State = {
    Username: string option
    Password    : string option
}

module Actor =

    let applyEvent (event: Event<_>) (_: State as state) =
        match event with
        | _ -> state

    let handleCommand (cmd: Command<_>) (state: State) =
        match cmd.CommandDetails, state with
        | Register username, { Username = None } -> RegisterSucceeded |> PersistEvent
        | _ -> UnhandledEvent


    let init (env: _) (actorApi: IActor) =
        let initialState = {
            Username = None
            Password = None
        }

        actorApi.InitializeActor env initialState "User" handleCommand applyEvent

    let factory (env: #_) actorApi entityId =
        (init env actorApi).RefFor DEFAULT_SHARD entityId

