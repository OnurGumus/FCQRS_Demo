module User

open FCQRS.Model.Data
open FCQRS.Common

type Event =
    | LoginSucceeded
    | LoginFailed
    | RegisterSucceeded of string * string
    | AlreadyRegistered

type Command =
    | Login of string
    | Register of string * string


type State =
    { Username: string option
      Password: string option }

module Actor =

    let applyEvent (event: Event<_>) (_: State as state) =
        match event.EventDetails with
        | RegisterSucceeded (userName, password) -> { state with Username = Some userName ; Password = Some password}
        | _ -> state

    let handleCommand (cmd: Command<_>) (state: State) =
        match cmd.CommandDetails, state with
        | Register (userName, password), { Username = None } -> RegisterSucceeded (userName,password) |> PersistEvent
        | Register _, { Username = Some _ } -> AlreadyRegistered |> DeferEvent
        | Login password1,
          { Username = Some _
            Password = Some password2 } when password1 = password2 -> LoginSucceeded |> PersistEvent
        | Login _, _ -> LoginFailed |> PersistEvent


    let init (env: _) (actorApi: IActor)=
        let initialState = { Username = None; Password = None }

        actorApi.InitializeActor env initialState "User" handleCommand applyEvent

    let factory (env: #_) actorApi entityId =
        (init env actorApi).RefFor DEFAULT_SHARD entityId
