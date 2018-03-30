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
        
let countBatches = 
            let transform (x:FileHeaderRecord) =
                x.Batches
            Option.map transform
              >> Option.defaultValue (upcast List())
              >> Seq.length
              
let countEntries =
            let transform (x:FileHeaderRecord) =
                            x.Batches |> Seq.collect(fun y -> y.Entries)
            Option.map transform
              >> Option.defaultValue (upcast List())
              >> Seq.length
              
let countAddenda = 
             let transform (x:FileHeaderRecord) =
                 x.Batches |> Seq.collect(fun y -> y.Entries) |> Seq.collect(fun z->z.Addenda)
             Option.map transform
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
let ``Parse web-debit.ach.txt addenda`` () =
 parseFile "web-debit.ach.txt" 
     |> countAddenda
     |> should equal 0
         
                
[<Fact>]
let ``Parse 20110729A.ach.txt`` () =
    parseFile "20110729A.ach.txt" 
        |> Option.isSome
        |> should equal true
    
[<Fact>]
let ``Parse 20110729A.ach.txt Batches`` () =
    parseFile "20110729A.ach.txt" 
        |> countBatches
        |> should equal 5
        
[<Fact>]
let ``Parse 20110729A.ach.txt Entries`` () =
    parseFile "20110729A.ach.txt" 
        |> countEntries
        |> should greaterThan 100
        
[<Fact>]
let ``Parse 20110729A.ach.txt addenda`` () =
    parseFile "20110729A.ach.txt" 
        |> countAddenda
        |> should greaterThan 50       