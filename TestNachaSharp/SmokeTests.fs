module SmokeTests

open FSharp.Data.FlatFileMeta
open NachaSharp
open Xunit
open FsUnit.Xunit

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
let ``Create Non-blank EntryCCD`` () =
    let control =  EntryCCD("", "6some data")
    control.AllowMutation <- true
    control.TraceNumber <- "55"
    control.TraceNumber |> should equal "55"

[<Fact>]
let ``Create Blank EntryAddenaWildCard`` () =
    let control =  EntryAddendaWildCard(null)
    control.RecordTypeCode |> should equal "7"

[<Fact>]
let ``Create Non-blank EntryAddenda05`` () =
    let control =  EntryAddenda05("705Power of the Dog")
    control.AllowMutation <- true
    control.RecordTypeCode <- "7"
    control.EntrySeqNum <- "5"
    control.RecordTypeCode |> should equal "7"

[<Fact>]
let ``Create Non-blank EntryAddenda05 with empty string`` () =
    let control =  EntryAddenda05 null
    control.AllowMutation <- true
    control.RecordTypeCode <- "7"
    control.EntrySeqNum <- "5"
    control.RecordTypeCode |> should equal "7"



[<Fact>]
let ``Create Blank File Header`` () =
        let file = FileHeaderRecord(null)
        file.IsNew() |> should equal true

[<Fact>]
let ``Create Blank Batch`` () =
        let file = FileHeaderRecord(null)
        let batch1 = BatchHeaderRecord(null);
        file.Batches.Add(batch1)
        file.Batches 
                |> Seq.head 
                |> (fun x->x.IsNew()) 
                |> should equal true
                
 
[<Fact>]
let ``Check Batch Count`` () =
        let file = FileHeaderRecord(null)
        let batch1 = BatchHeaderRecord(null);
        batch1.BatchControl <- SomeRow <| BatchControlRecord.Create()
        let entry = EntryCCD.Create();
        file.Batches.Add(batch1)
        batch1.Entries.Add(entry)
        file.Batches 
                |> Seq.head 
                |> (fun x->x.BatchControl 
                        |> function | SomeRow(y) -> y.Entry_AddendaCount 
                                    | NoRow -> 0)
                |> should equal 1
