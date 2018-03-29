namespace NachaSharp
open FSharp.Data.FlatFileMeta.MetaDataHelper
open FSharp.Control
open System.IO

module rec NachaFile =
    let internal (|FileHeaderMatch|_|)=
        matchRecord FileHeaderRecord 
    let internal (|FileControlMatch|_|)=
        matchRecord FileControlRecord
    let internal (|BatchHeaderMatch|_|) =
        matchRecord BatchHeaderRecord
    let internal (|BatchControlMatch|_|) =
        matchRecord BatchControlRecord
    let internal matchEntryRecord constructor =
        matchRecord (fun x-> constructor x :> EntryDetail)
    let internal (|EntryMatch|_|) = 
        multiMatch [
                     matchEntryRecord EntryExample1
                     matchEntryRecord EntryExample2 
                   ]
                   
    let ParseLines lines = syncParseLines AsyncParseLinesDef lines
                   
    let AsyncParseFile stream = asyncParseFile AsyncParseLinesDef stream |> Async.StartAsTask
    
    let AsyncParseLines lines = AsyncParseLinesDef lines |> Async.StartAsTask

    let internal AsyncParseLinesDef (lines: string AsyncSeq) = async {
        let mutable head: FileHeaderRecord option = None
        let enumerator = lines.GetEnumerator();
        let! current = enumerator.MoveNext()
        while head.IsNone && current.IsSome do
            match current.Value with
                | FileHeaderMatch (fh) -> 
                    head <- Some(fh)
                    let! currentFH = enumerator.MoveNext()
                    while fh.Trailer.IsNone && currentFH.IsSome do
                        match currentFH.Value with
                            | FileControlMatch t ->
                                fh.Trailer <- Some(t)
                            | BatchHeaderMatch bh ->
                                fh.Children <- fh.Children @ [bh]
                                let! currentBH = enumerator.MoveNext()
                                while bh.Trailer.IsNone && currentBH.IsSome do
                                    match currentBH.Value with
                                        | BatchControlMatch bt -> bh.Trailer <- Some(bt)
                                        | EntryMatch ed ->
                                            bh.Children <- bh.Children @ [ed]
                                        | _ -> ()
                            | _ -> ()
                | _ -> ()
        return head
    }