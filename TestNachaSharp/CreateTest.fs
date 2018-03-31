module CreateTest

open FSharp.Data.FlatFileMeta
open NachaSharp
open Xunit
open FsUnit.Xunit
open System.IO
open System.Collections.Generic

[<Fact>]
let ``Create Blank FileHeaderRecord`` () =
    let header =  FileHeaderRecord.Create("DummyDest","DummyOrigin", "DummyID")
                                
    header.RecordTypeCode |> should equal "1"