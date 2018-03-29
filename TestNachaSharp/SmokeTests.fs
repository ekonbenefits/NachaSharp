module Tests

open NachaSharp
open System
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
