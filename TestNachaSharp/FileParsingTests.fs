module FileParsingTests

open FSharp.Data.FlatFileMeta
open NachaSharp
open Xunit
open FsUnit.Xunit
open System.IO
open System.Collections.Generic


let parseFile file =
        let path = Path.Combine(__SOURCE_DIRECTORY__,"Data", file)
        use stream = File.OpenRead(path)
        NachaFile.ParseFile(stream) |> MaybeRecord.toOption
        
let countBatches: FileHeaderRecord option -> int = 
            Option.map (fun x-> x.Batches)
              >> Option.defaultValue (upcast List())
              >> Seq.length
              
let countEntries: FileHeaderRecord option -> int = 
            Option.map (fun x-> x.Batches |> Seq.collect(fun y -> y.Entries))
              >> Option.defaultValue (upcast List())
              >> Seq.length
              
              
[<Fact>]
let ``Parse web-debit.ach.txt`` () =
    parseFile "web-debit.ach.txt" 
        |> Option.isSome
        |> should equal true
    
[<Fact>]
let ``Parse web-debit.ach.txt Batches`` () =
    parseFile "web-debit.ach.txt" 
        |> countBatches
        |> should equal 3
        
[<Fact>]
let ``Parse web-debit.ach.txt Entries`` () =
    parseFile "web-debit.ach.txt" 
        |> countEntries
        |> should equal 6
        
        
[<Fact>]
let ``Parse 20110805A.ach.txt`` () =
    parseFile "20110805A.ach.txt" 
        |> Option.isSome
        |> should equal true
    
[<Fact>]
let ``Parse 20110805A.ach.txt Batches`` () =
    parseFile "20110805A.ach.txt" 
        |> countBatches
        |> should equal 4
        
[<Fact>]
let ``Parse 20110805A.ach.txt Entries`` () =
    parseFile "20110805A.ach.txt" 
        |> countEntries
        |> should equal 6
        