module Extension

open System
open Fable.Import.vscode
open Fable.Import.JS
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fable.PowerPack.Fetch.Fetch_types

module G = Fable.Import.Node.Globals
module I = Infrastructure

let private execute text = promise {
    let! path = I.createTemp { postfix = ".fs" }
    do! I.write path text
    do! I.shell "mono" [ G.__dirname + "/../../fantomas/Fantomas.exe"; path ]
    return! I.read path
}

let private execute' text = promise {
    try 
        let! formated = I.uploadText "http://212.47.229.214:8080/format" text
        do! I.replaceText formated
    with
    | e -> e.Message |> window.showErrorMessage |> ignore
}

let private formatCommand () = 
    window.activeTextEditor 
    |> Option.map (fun x -> x.document.getText())
    |> Option.map (execute' >> I.withProgress "Format file...")

let activate(context : ExtensionContext) =
    commands.registerCommand("extension.fntms.format", formatCommand |> unbox<Func<_,_>>)
    |> context.subscriptions.Add