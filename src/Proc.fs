module Ffmpeg.FSharp.Proc

/// запускает процесс и ждет его завершения.
let startProc printn errorPrintn progPath args =
    let startInfo = System.Diagnostics.ProcessStartInfo()
    startInfo.FileName <- progPath
    startInfo.Arguments <- args

    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true

    use proc = new System.Diagnostics.Process()
    proc.EnableRaisingEvents <- true

    proc.OutputDataReceived.AddHandler(
        System.Diagnostics.DataReceivedEventHandler(
            fun _ args -> printn args.Data
        )
    )

    proc.ErrorDataReceived.AddHandler(
        System.Diagnostics.DataReceivedEventHandler(
            fun _ args -> errorPrintn args.Data
        )
    )

    proc.StartInfo <- startInfo
    proc.Start() |> ignore
    proc.BeginOutputReadLine()
    proc.BeginErrorReadLine()
    proc.WaitForExit()
    proc.ExitCode

/// Выводит на консоль и одновременно в `string`
let startProcString path args =
    let errorOutput = new System.Text.StringBuilder()
    let output = new System.Text.StringBuilder()
    let resultCode =
        startProc
            (fun e ->
                printfn "%s" e
                output.AppendLine(e) |> ignore
            )
            (fun e ->
                printfn "%s" e
                errorOutput.AppendLine(e) |> ignore
            )
            path
            args
    resultCode, output.ToString(), errorOutput.ToString()
