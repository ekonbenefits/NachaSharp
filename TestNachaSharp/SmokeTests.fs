module Tests

open FSharp.Data.FlatFileMeta
open NachaSharp
open Xunit
open FsUnit.Xunit
open System.IO

[<Fact>]
let ``Create Blank FileHeaderRecord`` () =
    let header =  FileHeaderRecord(null)
    header.RecordTypeCode |> should equal "1"
    
[<Fact>]
let ``Create Blank FileControlRecord`` () =
    let control =  FileControlRecord(null)
    control.RecordTypeCode |> should equal "9"

[<Fact>]
let ``Create Blank BatchHeaderRecord`` () =
    let control =  BatchHeaderRecord(null)
    control.RecordTypeCode |> should equal "5"

[<Fact>]
let ``Create Blank BatchControlRecord`` () =
    let control =  BatchControlRecord(null)
    control.RecordTypeCode |> should equal "8"

[<Fact>]
let ``Create Blank EntryWildCard`` () =
    let control =  EntryWildCard("", null)
    control.RecordTypeCode |> should equal "6"

[<Fact>]
let ``Create Blank EntryPPD`` () =
    let control =  EntryPPD("", null)
    control.RecordTypeCode |> should equal "6"

[<Fact>]
let ``Create Blank EntryCCD`` () =
    let control =  EntryCCD("", null)
    control.RecordTypeCode |> should equal "6"

[<Fact>]
let ``Create Blank EntryAddenaWildCard`` () =
    let control =  EntryAddendaWildCard(null)
    control.RecordTypeCode |> should equal "7"

[<Fact>]
let ``Read Random File`` () =
    let path = Path.Combine(__SOURCE_DIRECTORY__,"Data", "web-debit.ach.txt")
    use stream = File.OpenRead(path)
    let header = NachaFile.ParseFile(stream)
    header |> MaybeRecord.isSomeRecord |> should equal true