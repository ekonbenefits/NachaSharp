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
    
        let matchEntryAddendaRecord constructor  =
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
                           addendaStatus:int,
                           lineNo:int) line =
                           
            let next = lineNo + 1
            let errored = (NoRecord, NoRecord, NoRecord, -1, lineNo)
            let finished = (head, batch, entry, addendaStatus, lineNo)
            let foundFileHeader fh = (SomeRecord(fh), NoRecord, NoRecord, 0, next)
            let foundBatchHeader bh = (head, SomeRecord(bh), NoRecord, 0, next)
            let foundFileControl () = (head, NoRecord, NoRecord, -1, lineNo)
            let foundEntryDetail (ed:EntryDetail) = (head, batch, SomeRecord(ed), ed.AddendaRecordedIndicator, next)
            let foundBatchControl () = (head, NoRecord, NoRecord, 0, next)
            let foundEntryAddenda () = (head, batch, entry, 2, next)
           
            match (head, batch, addendaStatus) with
                | _, _, -1 -> finished
                | NoRecord, _, _ ->
                     match line with
                         | Match.FileHeader lineNo (fh) -> 
                            foundFileHeader fh
                         | _ ->  
                            errored
                | SomeRecord(fh), NoRecord, _ ->
                     match line with
                         | Match.FileControl lineNo fc ->
                            fh.FileControl<- SomeRecord(fc)
                            foundFileControl ()                     
                         | Match.BatchHeader lineNo bh ->
                            fh.Batches.Add(bh)
                            foundBatchHeader bh
                         | _ ->  
                            errored
                | SomeRecord(fh), SomeRecord(bh), _ ->
                     match entry,addendaStatus,line with
                         | _,_,Match.BatchControl lineNo bc -> 
                             bh.BatchControl <- SomeRecord(bc)
                             foundBatchControl ()
                         | _,i,Match.EntryDetail bh.StandardEntryClass lineNo ed when i <> 1 ->
                             bh.Entries.Add(ed)
                             foundEntryDetail ed
                         | SomeRecord(ed),i,Match.EntryAddenda lineNo add when i > 0 ->
                             ed.Addenda.Add(add)
                             foundEntryAddenda ()
                         | _ ->
                            errored

        async{
            let! head,_,_,_,ln = 
                lines |> AsyncSeq.fold parseFromHead (NoRecord, NoRecord, NoRecord, 0, 1)
            return head
        }
        
      
    