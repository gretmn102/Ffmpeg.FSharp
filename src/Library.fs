namespace Ffmpeg.FSharp

module FfMpeg =
    let ffmpegPath = "ffmpeg"

    let startProc args =
        printfn "%s %s" ffmpegPath args
        Proc.startProcString ffmpegPath args

    /// https://wiki.multimedia.cx/index.php/FFmpeg_Metadata#MP3
    type Metadata = {
        artist: string option
        title: string option
        track: int option
        trackCount: int option
        album: string option
        // album artist // `album_artist`, а еще есть `performer`
        // `comment`
    }

    module Metadata =
        open FsharpMyExtension.Primitives.Numeric

        let toArgs (metadata: Metadata) =
            let escape =
                String.collect (function
                    | '"' -> "\\\""
                    | '\\' -> "\\"
                    | c -> string c
                )
            [
                match metadata.artist with
                | Some artist ->
                    $"-metadata artist=\"{escape artist}\""
                | None -> ()

                match metadata.title with
                | Some title ->
                    $"-metadata title=\"{escape title}\""
                | None -> ()

                let track =
                    Option.map (sprintf "-metadata track=\"%s\"") (
                        match metadata.track with
                        | Some track ->
                            Some (
                                match metadata.trackCount with
                                | Some trackCount -> $"{track}/{trackCount}"
                                | None -> string track
                            )
                        | None ->
                            metadata.trackCount
                            |> Option.map (sprintf "/%d")
                    )

                match track with
                | Some track -> track
                | None -> ()

                match metadata.album with
                | Some album ->
                    $"-metadata album=\"{escape album}\""
                | None -> ()
            ]

        let toFileName (metadata: Metadata) =
            let escape =
                let invalidPathChars =
                    System.IO.Path.GetInvalidFileNameChars ()
                    |> Set.ofArray
                String.collect (function
                    | ' ' -> "-"
                    | '—' -> "" // и так разделяется с помощью '-'
                    | c ->
                        if Set.contains c invalidPathChars then
                            ""
                        else
                            string c
                )
            String.concat "-" [
                match metadata.artist with
                | None -> ()
                | Some artist ->
                    escape artist
                match metadata.album with
                | None -> ()
                | Some album ->
                    escape album
                match metadata.track with
                | None -> ()
                | Some track ->
                    match metadata.trackCount with
                    | None ->
                        string track
                    | Some trackCount ->
                        Int32.zeroPad
                            (Int32.getLength trackCount)
                            track
                match metadata.title with
                | None -> ()
                | Some title ->
                    escape title
            ]

    type Options = {
        OverwriteOutput: bool
        Start: string option
        End: string option
        Metadata: Metadata option
        CopyAudio: bool
        CopyVideo: bool
        ASync: bool
    }

    module Options =
        let toArgs (options: Options) = [
                if options.OverwriteOutput then
                    "-y"

                match options.Start with
                | Some startAt -> $"-ss \"{startAt}\""
                | None -> ()

                match options.End with
                | Some endAt -> $"-to \"{endAt}\""
                | None -> ()

                match options.Metadata with
                | Some metadata ->
                    yield! Metadata.toArgs metadata
                | None -> ()

                if options.ASync then
                    "-async 1"

                if options.CopyAudio then
                    "-c:a copy"

                if options.CopyVideo then
                    "-c:v copy"
        ]

    let start (input: string) (options: Options) (output: string) =
        startProc
            (String.concat " " [
                $"-i \"{input}\""
                yield! Options.toArgs options
                $"\"{output}\""
            ])

module Ffprobe =
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
