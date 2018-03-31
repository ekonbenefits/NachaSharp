module CreateTest

open NachaSharp
open Xunit
open FsUnit.Xunit
open System

[<Fact>]
let ``Create Blank FileHeaderRecord`` () =
    let header =  FileHeaderRecord.Create("DummyDest","DummyOrigin", "DummyID")
                                
    header.RecordTypeCode |> should equal "1"
    
[<Fact>]
let ``Create Blank Batch`` () =
   let header =  FileHeaderRecord.Create("DummyDest","DummyOrigin", "DummyID")
   let batch = BatchHeaderRecord.Create("Code DUM",
                                        "Company Name DUM", 
                                        "COMP ident DUM",
                                        "secCode DUM",
                                        "COMPANY ENtry dum",
                                        DateTime(2018,1,1),
                                        "origin status dum",
                                        "dfi dum",
                                        10)
                      
   header.RecordTypeCode |> should equal "1"