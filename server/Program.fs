module Program

open System.Net
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

module Domain = 
    open System.IO
    open Fantomas

    let private handle text = 
        let path = Path.GetTempFileName() |> sprintf "%O.fs"
        try
            try 
                CodeFormatter.FormatDocument(path, text, FormatConfig.FormatConfig.Default) 
                |> Some
            with
            | e -> printfn "ERROR: %O" e; None
        finally
            File.Delete path

    let private agent = MailboxProcessor<string * AsyncReplyChannel<string option>>.Start (fun inbox ->
        let rec messageLoop() = async {
            let! (text, reply) = inbox.Receive()
            handle text |> reply.Reply
            return! messageLoop()
        }
        messageLoop())

    let handle' code ctx = async {
        let! formated = agent.PostAndAsyncReply(fun replyChannel -> code, replyChannel)
        match formated with
        | Some t -> return! (t |> OK) ctx 
        | None -> return None
    }

[<EntryPoint>]
let main _ = 
    let cfg = { defaultConfig with 
                    bindings = [ HttpBinding.create HTTP (IPAddress.Parse "0.0.0.0") 8080us  ] }
    let app = POST 
              >=> path "/format" 
              >=> request (fun req -> req.rawForm |> UTF8.toString |> Domain.handle')
    startWebServer cfg app
    0