namespace Ffmpeg.FSharp

// FsharpMyExtension.Primitives.Numeric.Numeric.Rational
type Fraction<'Numerator, 'Denominator> = 'Numerator * 'Denominator

type IntFraction = Fraction<int, int>

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module IntFraction =
    module Printer =
        open FsharpMyExtension.Serialization.Serializers.ShowList

        let show ((num, denum): IntFraction): ShowS =
            showByToString num
            << showChar '/'
            << showByToString denum

/// https://www.ffmpeg.org/ffmpeg-formats.html#metadata
type Chapter = {
    TimeBase: Fraction<int, int>
    /// | ч:м:cек.мс   | TimeBase | значение |
    /// |--------------|----------|----------|
    /// | 00:00:35.000 | 1/1      | 35       |
    /// | 00:00:35.123 | 1/1000   | 35123    |
    /// | 00:00:07.000 | 1/30     | 210      |
    Start: int
    End: int
    Title: string
}

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
[<RequireQualifiedAccess>]
module Chapter =
    module Printer =
        open FsharpMyExtension.Serialization.Serializers.ShowList

        let show (chapter: Chapter) : ShowS list =
            [
                showString "[CHAPTER]"
                showString "TIMEBASE" << showChar '=' << IntFraction.Printer.show chapter.TimeBase
                showString "START" << showChar '=' << showByToString chapter.Start
                showString "END" << showChar '=' << showByToString chapter.End
                showString "TITLE" << showChar '=' << showByToString chapter.Title
            ]
