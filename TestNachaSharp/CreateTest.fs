module CreateTest

open NachaSharp
open Xunit
open FsUnit.Xunit
open System
open System.IO
open FSharp.Data.FlatFileMeta
open FSharp.Interop.Compose.System

[<Fact>]
let ``Create Blank FileHeaderRecord`` () =
    let header =  FileHeaderRecord.Create("DummyDest","DummyOrig", "A")
                                
    header.RecordTypeCode |> should equal "1"
    
[<Fact>]
let ``Create Blank Batch`` () =
   let header =  FileHeaderRecord.Create("DummyDest","DummyOrig", "B")
   let batch = BatchHeaderRecord.Create("DUM",
                                        "Company Name DUM", 
                                        "COMP DUM",
                                        "DuM",
                                        "COMENtryd",
                                        DateTime(2018,1,1),
                                        "D",
                                        "dfi dum",
                                        10)
                      
   header.RecordTypeCode |> should equal "1"
   
let parseFile file =
    let path = Path.Combine(__SOURCE_DIRECTORY__,"Data", file)
    use stream = File.OpenRead(path)
    NachaFile.ParseFile(stream)
        |> MaybeRow.toOption 
        |> Option.defaultWith (fun ()-> raise <| Exception (sprintf "Couldn't read file %s" path))
 
let loadFile file =
    let path = Path.Combine(__SOURCE_DIRECTORY__,"Data", file)
    use stream = File.OpenRead(path)
    use reader = new StreamReader(stream)
    reader.ReadToEnd()
 
[<Fact>]  
let ``Write after parsing file web-debit compare to orig `` () =
    let filename = "web-debit.ach.txt"
    use mem = new MemoryStream()
    let parsed =  parseFile filename
    NachaFile.WriteFile(parsed, mem)
    mem.Position <- 0L
    use reader = new StreamReader(mem)
    let actual = reader.ReadToEnd() |> String.trim
    let expect = loadFile filename |> String.trim
    actual |> should equal expect
    ()   