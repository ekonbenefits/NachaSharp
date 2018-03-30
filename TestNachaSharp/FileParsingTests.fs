module FileParsingTests

open FSharp.Data.FlatFileMeta
open NachaSharp
open Xunit
open FsUnit.Xunit
open System.IO
open System.Collections.Generic

let ``Get web-debit.ach.txt`` () =
    let path = Path.Combine(__SOURCE_DIRECTORY__,"Data", "web-debit.ach.txt")
    use stream = File.OpenRead(path)
    NachaFile.ParseFile(stream) |> MaybeRecord.toOption

[<Fact>]
let ``Parse web-debit.ach.txt`` () =
    
    ``Get web-debit.ach.txt``() |> Option.isSome |> should equal true
    
[<Fact>]
let ``Parse web-debit.ach.txt Batches`` () =
    
    ``Get web-debit.ach.txt``() 
        |> Option.map (fun x-> x.Batches)
        |> Option.defaultValue (upcast List())
        |> Seq.length
        |> should equal 3