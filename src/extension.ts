import * as vs from 'vscode'
import { Range, Position } from 'vscode'
import * as tmp from 'tmp'
import * as fs from "fs-promise"

const spawn = require('cross-spawn')

export function activate(context: vs.ExtensionContext) {
    let disposable = vs.commands.registerCommand('extension.fntms.format', () => {
        const editor = vs.window.activeTextEditor; if (!editor) return
        const text = editor.document.getText()

        vs.window.withProgress({
            title: "Format file...",
            location: vs.ProgressLocation.Window
        }, () => {
            return new Promise(resolver => {
                tmp.file({ postfix: ".fs" }, async (_, path) => {
                    await fs.writeFile(path, text)

                    const cmd = spawn("mono", [__dirname + "/../../fantomas/Fantomas.exe", path])
                    cmd.stdout.on('data', (data: any) => console.log(`stdout: ${data}`))
                    cmd.stderr.on('data', (data: any) => console.log(`stderr: ${data}`))
                    await waitForEnd(cmd)

                    const formated = await fs.readFile(path, "UTF-8")
                    editor.edit(edit => {
                        edit.replace(new Range(
                            new Position(0, 0),
                            new Position(editor.document.lineCount, 0)),
                            formated)
                        resolver(undefined)
                    })
                })
            })
        })
    })
    context.subscriptions.push(disposable)
}

const waitForEnd = (cmd: any) => new Promise<void>(resolve => cmd.on('close', resolve))