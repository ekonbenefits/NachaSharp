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
                           count:int,
                           lineNo:int) line =
                           
            let next = lineNo + 1
            let errored = (NoRecord, NoRecord, NoRecord, -1, lineNo)
            let finished = (head, batch, entry, count, lineNo)
            let foundFileHeader fh = (SomeRecord(fh), NoRecord, NoRecord, 0, next)
            let foundBatchHeader fh bh = (SomeRecord(fh), SomeRecord(bh), NoRecord, 0, next)
            let foundFileControl fh = (SomeRecord(fh), NoRecord, NoRecord, -1, lineNo)
            let foundEntryDetail fh bh (ed:EntryDetail)= (SomeRecord(fh), SomeRecord(bh), SomeRecord(ed), ed.AddendaRecordedIndicator, next)
            let foundBatchControl fh = (SomeRecord(fh), NoRecord, NoRecord, 0, next)
            let foundEntryAddenda fh bh ed count = (SomeRecord(fh), SomeRecord(bh), SomeRecord(ed), count - 1, next)
           
            match (head, batch, entry, count) with
                | _, _, _, -1 -> finished
                | NoRecord, _, _, _ ->
                     match line with
                         | Match.FileHeader lineNo (fh) -> 
                            foundFileHeader fh
                         | _ ->  
                            errored
                | SomeRecord(fh), NoRecord, _, _ ->
                     match line with
                         | Match.FileControl lineNo fc ->
                            fh.FileControl<- SomeRecord(fc)
                            foundFileControl fh                     
                         | Match.BatchHeader lineNo bh ->
                            fh.Batches.Add(bh)
                            foundBatchHeader fh bh
                         | _ ->  
                            errored
                | SomeRecord(fh), SomeRecord(bh), _, 0 ->
                     match line with
                         | Match.BatchControl lineNo bc -> 
                            bh.BatchControl <- SomeRecord(bc)
                            foundBatchControl fh
                         | Match.EntryDetail bh.StandardEntryClass lineNo ed ->
                                                     bh.Entries.Add(ed)
                                                     foundEntryDetail fh bh ed 
                         | _ ->
                            errored
                | SomeRecord(fh), SomeRecord(bh), SomeRecord(ed), count ->
                     match line with
                         | Match.EntryAddenda lineNo add ->
                            ed.Addenda.Add(add)
                            foundEntryAddenda fh bh ed count
                         | _ -> 
                            errored
                | _ -> errored

        async{
            let! head,_,_,_,ln = 
                lines |> AsyncSeq.fold parseFromHead (NoRecord, NoRecord, NoRecord, 0, 1)
            return head
        }
        
      
    