module Infrastructure

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import.vscode
open Fable.Import.JS
open Fable.Import.Node.Fs
open Fable.Import.Node.ChildProcess

module P = Fable.PowerPack.Promise

type Options = { postfix: string }

let createTemp options = 
    let f: (Options -> (obj -> string -> unit) -> unit) = import "file" "tmp"
    P.create (fun resolve _ -> f options (fun _ path -> resolve path))
let write path text =
    P.create (fun resolve _ -> fs.writeFile(path, text, fun _ -> resolve()))
let read path =
    P.create(fun resolve _ -> fs.readFile(path, "utf8", (fun _ data -> resolve data)))
let shell command (args: string list) =
    P.create(fun resolve _ -> let p = child_process.spawn(command, ResizeArray args)
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