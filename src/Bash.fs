module Ffmpeg.FSharp.Bash

let toDoubleQuote str =
    let escape =
        String.collect (function
            | '"' -> "\\\""
            | '\\' -> @"\\\\"
            | c -> string c
        )
    $"\"{escape str}\""
