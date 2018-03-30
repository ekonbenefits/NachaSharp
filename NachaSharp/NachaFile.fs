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
      
    type internal ParseState =
        {
            head:FileHeaderRecord MaybeRecord
            batch:BatchHeaderRecord MaybeRecord
            entry:EntryDetail MaybeRecord
            addenda:int
            finished: bool
            lineNo:int
        }
        
    let internal asyncParseLinesDef (lines: string AsyncSeq) = 

  

        let parseFromHead (state:ParseState) lineOftext =
            if state.finished then
                state
            else            
                let next = state.lineNo + 1
                let errored = {state with head = NoRecord; finished = true}
                let foundFileHeader fh = {state with head = SomeRecord(fh); lineNo = next}
                let foundBatchHeader bh = {state with batch = SomeRecord(bh); lineNo = next}
                let foundFileControl () = {state with finished = true}
                let foundEntryDetail (ed:EntryDetail) = { state with
                                                            entry = SomeRecord(ed)
                                                            addenda = ed.AddendaRecordedIndicator
                                                            lineNo = next }
                let foundBatchControl () = { state with
                                                batch = NoRecord
                                                entry = NoRecord
                                                addenda = 0
                                                lineNo = next }
                let foundEntryAddenda () = { state with addenda= 2; lineNo = next}
               
                match (state.head, state.batch) with
                    | NoRecord, _ ->
                         match lineOftext with
                             | Match.FileHeader state.lineNo (fh) -> 
                                foundFileHeader fh
                             | _ ->  
                                errored
                    | SomeRecord(fh), NoRecord ->
                         match lineOftext with
                             | Match.FileControl state.lineNo fc ->
                                fh.FileControl<- SomeRecord(fc)
                                foundFileControl ()                     
                             | Match.BatchHeader state.lineNo bh ->
                                fh.Batches.Add(bh)
                                foundBatchHeader bh
                             | _ ->  
                                errored
                    | SomeRecord(fh), SomeRecord(bh) ->
                         match state.entry,state.addenda,lineOftext with
                             | _,_,Match.BatchControl state.lineNo bc -> 
                                 bh.BatchControl <- SomeRecord(bc)
                                 foundBatchControl ()
                             | _,i,Match.EntryDetail bh.StandardEntryClass state.lineNo ed when i <> 1 ->
                                 bh.Entries.Add(ed)
                                 foundEntryDetail ed
                             | SomeRecord(ed),i,Match.EntryAddenda state.lineNo add when i > 0 ->
                                 ed.Addenda.Add(add)
                                 foundEntryAddenda ()
                             | _ ->
                                errored

        async{
            let! result = 
                        lines |> AsyncSeq.fold parseFromHead {
                                                                head = NoRecord
                                                                batch = NoRecord
                                                                entry = NoRecord
                                                                addenda = 0
                                                                finished = false
                                                                lineNo = 1
                                                             }
            return result.head
        }
        
      
    