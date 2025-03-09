module Command

open FCQRS.Model.Data
open BootStrap

let register cid userName password =

    let actorId: ActorId = userName |> ValueLens.CreateAsResult |> Result.value

    let command = User.Register(userName, password)

    let condition (e: User.Event) = 
             e.IsAlreadyRegistered || e.IsRegisterSucceeded
      
    let subscribe = userSubs cid actorId command condition

    async {
        match! subscribe with
        | { EventDetails = User.RegisterSucceeded _
            Version = v } -> return Ok v

        | { EventDetails = User.AlreadyRegistered
            Version = _ } -> return Error [ sprintf "User %s is already registered" <| actorId.ToString() ]
        | _ ->
            return Error [ sprintf "Registration failed for user %s" <| actorId.ToString() ]
    }
