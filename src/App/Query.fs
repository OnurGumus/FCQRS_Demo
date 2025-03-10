
module Query

open Microsoft.Extensions.Logging
open FCQRS.Common

let handleEventWrapper env (offsetValue: int64) (event:obj)=
    let log = (env :> ILoggerFactory).CreateLogger "Event"
    log.LogInformation("Event: {0}", event.ToString())

    let dataEvent =
        match event with
        | :? FCQRS.Common.Event<User.Event> as  event ->
            [event:> IMessageWithCID]
        |  _ -> []

    dataEvent