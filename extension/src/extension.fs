module Extension

open System
open Fable.Import.vscode
open Fable.PowerPack

module I = Infrastructure

let private execute text = promise {
    try 
        let! formated = I.postText "http://212.47.229.214:8080/format" text
        do! I.replaceText formated
    with
    | e -> e.Message |> window.showErrorMessage |> ignore
}

let private formatCommand () = 
    window.activeTextEditor 
    |> Option.map (fun x -> x.document.getText())
    |> Option.map (execute >> I.withProgress "Format file...")

let activate(context : ExtensionContext) =
    commands.registerCommand("extension.fntms.format", formatCommand |> unbox<Func<_,_>>)
    |> context.subscriptions.Add