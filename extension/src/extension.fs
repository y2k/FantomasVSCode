module Extension

open System
open Fable.Import.vscode
open Fable.Import.JS
open Fable.PowerPack

module G = Fable.Import.Node.Globals
module I = Infrastructure

let private executeAsync text =
    promise {
        try 
            let! path = I.createTemp { postfix = ".fs" }
            do! I.write path text
            do! I.shell "mono" [ G.__dirname + "/../../fantomas/Fantomas.exe"; path ]
            let! formated = I.read path
            do! I.replaceText formated
        with
        | e -> e.Message |> window.showErrorMessage |> ignore
    }

let private formatCommand () = 
    window.activeTextEditor 
    |> Option.map (fun x -> x.document.getText())
    |> Option.map (executeAsync >> I.withProgress "Format file...")

let activate(context : ExtensionContext) =
    commands.registerCommand("extension.fntms.format", formatCommand |> unbox<Func<_,_>>)
    |> context.subscriptions.Add