module CreateTest

open FSharp.Data.FlatFileMeta
open NachaSharp
open Xunit
open FsUnit.Xunit
open System.IO
open System.Collections.Generic


[<Fact>]
let ``Create Blank File Header`` () =
        let file = FileHeaderRecord.Create()
        file.IsNew() |> should equal true

[<Fact>]
let ``Create Blank Batch`` () =
        let file = FileHeaderRecord.Create()
        let batch1 = BatchHeaderRecord.Create();
        file.Batches.Add(batch1)
        file.Batches 
                |> Seq.head 
                |> (fun x->x.IsNew()) 
                |> should equal true
                
 
[<Fact>]
let ``Check Batch Count`` () =
        let file = FileHeaderRecord.Create()
        let batch1 = BatchHeaderRecord.Create();
        let entry = EntryCCD.Create();
        file.Batches.Add(batch1)
        batch1.Entries.Add(entry)
        file.Batches 
                |> Seq.head 
                |> (fun x->x.BatchControl 
                        |> function | SomeRecord(y) -> y.Entry_AddendaCount 
                                    | NoRecord -> 0)
                |> should equal 1