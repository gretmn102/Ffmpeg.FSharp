module Ffmpeg.FSharp.Ffprob

let ffprobPath = "ffprobe"

let startProc args =
    printfn "%s %s" ffprobPath args
    Proc.startProcString ffprobPath args

let getTimebase inputPath =
    let args =
        String.concat " " [
            "-v error"
            "-select_streams v:0"
            "-show_entries stream=time_base"
            "-of default=noprint_wrappers=1:nokey=1"
            Bash.toDoubleQuote inputPath
        ]
    let exitCode, stdout, stderr = startProc args
    exitCode, stdout.Trim(), stderr

let getFps inputPath =
    let args =
        String.concat " " [
            "-v error"
            "-select_streams v:0"
            "-show_entries stream=r_frame_rate"
            "-of csv=p=0"
            Bash.toDoubleQuote inputPath
        ]
    let exitCode, stdout, stderr = startProc args
    exitCode, stdout.Trim(), stderr
