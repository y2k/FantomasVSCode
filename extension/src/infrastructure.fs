module Infrastructure

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.vscode
open Fable.Import.JS
open Fable.Import.Node.Exports

module P = Fable.PowerPack.Promise
module H = Fable.Import.Node.Http

type Options = { postfix: string }

let uploadText (url: string) (text: string): Promise<string> =
    P.create (fun resolve eHandler -> 
                   let options = createEmpty<H.RequestOptions>
                   options.method <- Some H.Methods.Post
                   options.host <- Some "212.47.229.214"
                   options.port <- Some 8080
                   options.path <- Some "format"

                   let r = Http.request(options, fun res -> 
                                                     let mutable result = ""
                                                     res.setEncoding "utf8"
                                                     res.on("data", fun data -> result <- result + data) |> ignore
                                                     res.on("end", fun _ -> resolve result) |> ignore
                                                     res.on("error", eHandler) |> ignore)

                   r.write(Buffer.Buffer.Create(text)) |> ignore)

let createTemp options = 
    let f: (Options -> (obj -> string -> unit) -> unit) = import "file" "tmp"
    P.create (fun resolve _ -> f options (fun _ path -> resolve path))
let write path text =
    P.create (fun resolve _ -> Fs.writeFile(path, text, fun _ -> resolve()))
let read path =
    P.create(fun resolve _ -> Fs.readFile(path, "utf8", (fun _ data -> resolve data)))
let shell command (args: string list) =
    P.create(fun resolve _ -> let p = ChildProcess.spawn(command, ResizeArray args)
                              p.on("close", resolve) |> ignore)
let replaceText text =
    match window.activeTextEditor with
        | Some te -> 
            let range = Range(0.0, 0.0, te.document.lineCount, 0.0)
            te.edit (fun e -> e.replace((U3.Case2 range), text)) |> unbox<Promise<_>>
        | _ -> P.lift ()
let withProgress title (promise: Promise<'a>) =
    let progressOpts = createEmpty<ProgressOptions>
    progressOpts.title <- Some title
    progressOpts.location <- ProgressLocation.Window
    window.withProgress(progressOpts, fun _ -> promise |> unbox<PromiseLike<_>>) |> ignore