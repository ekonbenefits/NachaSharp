namespace NachaSharp
open FSharp.Data.FlatFileMeta
open FSharp.Data.FlatFileMeta.MetaDataHelper
open FSharp.Control
open System.IO

module rec NachaFile =
    module internal Match =
        let (|FileHeader|_|)=
            matchRecord FileHeaderRecord 
        let (|FileControl|_|)=
            matchRecord FileControlRecord
        let (|BatchHeader|_|) =
            matchRecord BatchHeaderRecord
        let (|BatchControl|_|) =
            matchRecord BatchControlRecord
        let matchEntryRecord constructor batchSEC =
            matchRecord (fun x-> constructor(batchSEC, x) :> EntryDetail)
        let (|EntryDetail|_|) batchSEC = 
            multiMatch [
                         matchEntryRecord EntryCCD batchSEC
                         matchEntryRecord EntryPPD batchSEC
                         matchEntryRecord EntryWildCard batchSEC
                       ]
    
        let matchEntryAddendaRecord constructor =
            matchRecord (fun x-> constructor(x) :> EntryAddenda)
        let (|EntryAddenda|_|) = 
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
                | Match.FileHeader (fh) -> 
                    head <- SomeRecord(fh)
                    let! currentFH = enumerator.MoveNext()
                    while isMissingRecordButHasString fh.FileControl currentFH do
                        match currentFH |> Option.get with
                            | Match.FileControl t ->
                                fh.FileControl<- SomeRecord(t)
                            | Match.BatchHeader bh ->
                                fh.Batches.Add(bh)
                                let! currentBH = enumerator.MoveNext()
                                while isMissingRecordButHasString bh.BatchControl currentBH do
                                    match currentBH |> Option.get with
                                        | Match.BatchControl bt -> bh.BatchControl <- SomeRecord(bt)
                                        | Match.EntryDetail bh.StandardEntryClass ed ->
                                            bh.Entries.Add(ed)
                                            for _ in 0..ed.AddendaRecordedIndicator do
                                                let! currentAdd = enumerator.MoveNext()
                                                match currentAdd |> Option.get with
                                                | Match.EntryAddenda add ->
                                                    ed.Addenda.Add(add)
                                                | _ -> raise <| InvalidDataException("Incorrect Addenda Indicator")
                                        | _ -> ()
                            | _ -> ()
                | _ -> ()
        return head
    }