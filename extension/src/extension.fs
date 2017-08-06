module Extension

open System
open Fable.Import.vscode
open Fable.PowerPack

module I = Infrastructure

let private execute (editor: TextEditor) = promise {
    try 
        let version = editor.document.version
        let select = editor.selection
                     |> fun x -> if x.isEmpty then Selection(0.0, 0.0, editor.document.lineCount, 0.0) else x

        let text = editor.document.getText select
        
        let! formated = I.postText "http://212.47.229.214:8080/format" text
                        |> Promise.map (fun x -> x.TrimEnd '\n')

        if not editor.document.isClosed && editor.document.version = version then 
            do! I.replaceText' editor select formated
    with
    | e -> e.Message |> window.showErrorMessage |> ignore
}

let private formatCommand () = 
    window.activeTextEditor 
    |> Option.map execute
    |> Option.map (fun x -> I.withProgress "Format file..." x)

let activate(context : ExtensionContext) =
    commands.registerCommand("extension.fntms.format", formatCommand |> unbox<Func<_,_>>)
    |> context.subscriptions.Add