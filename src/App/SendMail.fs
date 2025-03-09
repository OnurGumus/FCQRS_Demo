module SendMail

open Akkling

type Mail =
    { To: string
      Subject: string
      Body: string }


let behavior (m: Actor<_>) =
    let rec loop () =
        actor {
            let! (mail: Mail) = m.Receive()
            printfn "Sending mail to %A !!" mail
            return Stop
        }

    loop ()
