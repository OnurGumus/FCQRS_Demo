open FCQRS.Model.Data
open BootStrap

let register cid userName password =

    let actorId: ActorId = userName |> ValueLens.CreateAsResult |> Result.value

    let command = User.Register(userName, password)

    let condition = fun (e: User.Event) -> 
        e.IsRegisterSucceeded
        || e.IsAlreadyRegistered

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

let cid: CID =
    System.Guid.NewGuid().ToString() |> ValueLens.CreateAsResult |> Result.value

let userName = "test user"

let password = "password"

let result = register cid userName password |> Async.RunSynchronously
printfn "%A" result

let cid2: CID =
    System.Guid.NewGuid().ToString() |> ValueLens.CreateAsResult |> Result.value
let resultFailure = register cid2 userName password |> Async.RunSynchronously
printfn "%A" resultFailure

System.Console.ReadKey() |> ignore