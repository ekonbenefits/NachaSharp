namespace NachaSharp
open FSharp.Data.FlatFileMeta
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
    let internal matchEntryRecord constructor batchSEC =
        matchRecord (fun x-> constructor(batchSEC, x) :> EntryDetail)
    let internal (|EntryMatch|_|) batchSEC = 
        multiMatch [
                     matchEntryRecord EntryCCD batchSEC
                     matchEntryRecord EntryPPD batchSEC
                     matchEntryRecord EntryWildCard batchSEC
                   ]

    let internal matchEntryAddendaRecord constructor =
        matchRecord (fun x-> constructor(x) :> EntryAddenda)
    let internal (|EntryAddendaMatch|_|) = 
        multiMatch [
                     matchEntryAddendaRecord EntryAddendaWildCard
                   ]
                  
    let ParseLines lines = syncParseLines asyncParseLinesDef lines
    
    let ParseFile stream = syncParseFile asyncParseLinesDef stream
                   
    let AsyncParseFile stream = asyncParseFile asyncParseLinesDef stream |> Async.StartAsTask
    
    let AsyncParseLines lines = asyncParseLinesDef lines |> Async.StartAsTask

    let internal asyncParseLinesDef (lines: string AsyncSeq) = async {
        let mutable head: FileHeaderRecord MaybeRecord = NoRecord
        let enumerator = lines.GetEnumerator();
        let! current = enumerator.MoveNext()
        while isMissingRecordButHasString head current do
            match current |> Option.get with
                | FileHeaderMatch (fh) -> 
                    head <- SomeRecord(fh)
                    let! currentFH = enumerator.MoveNext()
                    while isMissingRecordButHasString fh.FileControl currentFH do
                        match currentFH |> Option.get with
                            | FileControlMatch t ->
                                fh.FileControl<- SomeRecord(t)
                            | BatchHeaderMatch bh ->
                                fh.Batches.Add(bh)
                                let! currentBH = enumerator.MoveNext()
                                while isMissingRecordButHasString bh.BatchControl currentBH do
                                    match currentBH |> Option.get with
                                        | BatchControlMatch bt -> bh.BatchControl <- SomeRecord(bt)
                                        | EntryMatch bh.StandardEntryClass ed ->
                                            bh.Entries.Add(ed)
                                            for _ in 0..ed.AddendaRecordedIndicator do
                                                let! currentAdd = enumerator.MoveNext()
                                                match currentAdd |> Option.get with
                                                | EntryAddendaMatch add ->
                                                    ed.Addenda.Add(add)
                                                | _ -> raise <| InvalidDataException("Incorrect Addenda Indicator")
                                        | _ -> ()
                            | _ -> ()
                | _ -> ()
        return head
    }