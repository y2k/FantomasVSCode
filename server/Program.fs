module Program

open System.Net
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

module Domain = 
    open System.IO
    open System.Text
    open Fantomas
    open Microsoft.FSharp.Compiler.SourceCodeServices

    let private handle text = 
        let path = Path.GetTempFileName() |> sprintf "%O.fs"
        try 
            CodeFormatter.FormatDocument(path, text, FormatConfig.FormatConfig.Default)
        finally
            File.Delete path

    let private agent = MailboxProcessor<string * AsyncReplyChannel<string>>.Start (fun inbox ->
        let rec messageLoop() = async {
            let! (text, reply) = inbox.Receive()
            handle text |> reply.Reply
            return! messageLoop()
        }
        messageLoop())

    let handle'' (context: HttpContext) = async {
        let code = context.request.rawForm |> Encoding.UTF8.GetString
        let! formated = agent.PostAndAsyncReply(fun replyChannel -> code, replyChannel)
        return! OK formated context >>= Writers.setMimeType "plain/text"
    }

[<EntryPoint>]
let main _ = 
    let cfg = { defaultConfig with 
                    bindings = [ HttpBinding.create HTTP (System.Net.IPAddress.Parse "0.0.0.0") 8080us  ] }
    let app = choose [ POST >=> path "/format" >=> Domain.handle'' ]
    startWebServer defaultConfig app
    0