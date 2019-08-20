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
                                        "dfi dum")
                      
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
 
 
let compareLines (expectedReader:TextReader) (actualReader:TextReader) =
    let mutable  actualNext = actualReader.ReadLine()
    let mutable  expectedNext = expectedReader.ReadLine()
    let mutable i = 1;
    while not <| isNull actualNext && not <| isNull expectedNext do
        let actual = sprintf "line %i: %s" i actualNext
        let expected = sprintf "line %i: %s" i expectedNext
        actual |> should equal expected
        actualNext <- actualReader.ReadLine()
        expectedNext <- expectedReader.ReadLine()
        i <- i + 1
        
[<Fact>]  
let ``Write after parsing file compare`` () =
    let filename = "transactions1.ach.txt"
    use mem = new MemoryStream()
    let parsed =  parseFile filename
    NachaFile.WriteFile(parsed, mem,"\r")
    mem.Position <- 0L
    use reader = new StreamReader(mem)                                 
    use expect = new StringReader(loadFile filename) 
    reader |> compareLines expect


[<Fact>]   
let ``Write after parsing file compare 2`` () =
        let filename = "transactions2.ach.txt"
        use mem = new MemoryStream()
        let parsed =  parseFile filename
        NachaFile.WriteFile(parsed,mem,"\r")
        mem.Position <- 0L
        use reader = new StreamReader(mem)                                 
        use expect = new StringReader(loadFile filename) 
        reader |> compareLines expect
        ()  
        

    
[<Fact>]  
let ``Write after parsing file all mutate`` () =
    let filename = "transactions1.ach.txt"
    use mem = new MemoryStream()
    let parsed =  parseFile filename
    parsed.AllowMutation <-true
    NachaFile.WriteFile(parsed, mem, "\r")
    mem.Position <- 0L
    use reader = new StreamReader(mem)                                 
    use expect = new StringReader(loadFile filename) 
    reader |> compareLines expect
    ()    