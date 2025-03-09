﻿open FCQRS.Model.Data
open Command

let sub = BootStrap.sub Query.handleEventWrapper 0L

let cid (): CID =
    System.Guid.NewGuid().ToString() |> ValueLens.CreateAsResult |> Result.value

let userName = "testuser"

let password = "password"

let cid1 = cid()

let s = sub.Subscribe((fun e -> e.CID = cid1), 1)
let result = register cid1 userName password |> Async.RunSynchronously
(s |> Async.RunSynchronously).Dispose()
printfn "%A" result

let code = System.Console.ReadLine() 

let resultVerify = verify (cid()) userName code |> Async.RunSynchronously
printfn "%A" resultVerify

System.Console.ReadKey() |> ignore

let resultFailure = register (cid()) userName password |> Async.RunSynchronously
printfn "%A" resultFailure

System.Console.ReadKey() |> ignore

let loginResultF = login (cid()) userName "wrong pass" |> Async.RunSynchronously
printfn "%A" loginResultF

let loginResultS = login (cid()) userName password |> Async.RunSynchronously
printfn "%A" loginResultS

System.Console.ReadKey() |> ignore