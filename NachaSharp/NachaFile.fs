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

    let internal asyncParseLinesDef (lines: string AsyncSeq) = 

        let parseFromHead (head:FileHeaderRecord MaybeRecord,
                           batch:BatchHeaderRecord MaybeRecord,
                           entry:EntryDetail MaybeRecord,
                           count:int) line =
            let errorState = (NoRecord, NoRecord, NoRecord, -1)
            match (head, batch, entry, count) with
                | _, _, _, -1 -> (head, batch, entry, count) //Finished
                | NoRecord, _, _, _ ->
                     match line with
                         | Match.FileHeader (fh) -> (SomeRecord(fh), NoRecord, NoRecord, 0)
                         | _ ->  errorState
                | SomeRecord(fh), NoRecord, _, _ ->
                     match line with
                         | Match.BatchHeader bh ->
                             fh.Batches.Add(bh)
                             (SomeRecord(fh), SomeRecord(bh), NoRecord, 0)
                         | Match.FileControl fc ->
                             fh.FileControl<- SomeRecord(fc)
                             (head, NoRecord, NoRecord, -1)
                         | _ ->  errorState
                | SomeRecord(fh), SomeRecord(bh), NoRecord, _ ->
                     match line with
                         | Match.EntryDetail bh.StandardEntryClass ed ->
                             bh.Entries.Add(ed)
                             (SomeRecord(fh), SomeRecord(bh), SomeRecord(ed), ed.AddendaRecordedIndicator)
                         | _ -> errorState
                | SomeRecord(fh), SomeRecord(bh), SomeRecord(ed), 0 ->
                     match line with
                         | Match.EntryDetail bh.StandardEntryClass ed ->
                            bh.Entries.Add(ed)
                            (SomeRecord(fh), SomeRecord(bh), SomeRecord(ed), ed.AddendaRecordedIndicator)
                         | Match.BatchControl bc -> 
                            bh.BatchControl <- SomeRecord(bc)
                            (SomeRecord(fh), NoRecord, NoRecord, 0)
                         | _ -> errorState
                | SomeRecord(fh), SomeRecord(bh), SomeRecord(ed), count ->
                     match line with
                         | Match.EntryAddenda add ->
                            ed.Addenda.Add(add)
                            (SomeRecord(fh), SomeRecord(bh), SomeRecord(ed), count - 1)
                         | _ -> errorState

        async{
            let! head,_,_,_ = 
                lines |> AsyncSeq.fold parseFromHead (NoRecord, NoRecord, NoRecord, 0)
            return head
        }
        
      
    