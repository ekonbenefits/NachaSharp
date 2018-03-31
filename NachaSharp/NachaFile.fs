namespace NachaSharp
open FSharp.Data.FlatFileMeta
open FSharp.Control

module rec NachaFile =

    let ParseLines lines = FlatRowProvider.syncParseLines asyncParseLinesDef lines
    
    let ParseFile stream =  FlatRowProvider.syncParseFile asyncParseLinesDef stream
                   
    let AsyncParseFile stream =  FlatRowProvider.asyncParseFile asyncParseLinesDef stream 
                                 |> Async.StartAsTask
    
    let AsyncParseLines lines = asyncParseLinesDef lines 
                                |> Async.StartAsTask
        
    let internal asyncParseLinesDef (lines: string AsyncSeq) = async {
            let! {head = result}  =
                lines |> AsyncSeq.fold foldingParse {
                                                        head = NoRow
                                                        batch = NoRow
                                                        entry = NoRow
                                                        addenda = 0
                                                        finished = false
                                                        lineNo = 1
                                                     }
            return result
        }
        
    let internal foldingParse (state:ParseState) lineOftext =
            if state.finished then
                state
            else
                //possible actions            
                let next = state.lineNo + 1
                let errored = {state with head = NoRow; finished = true}
                let foundFileHeader fh = {state with head = SomeRow(fh); lineNo = next}
                let foundBatchHeader bh = 
                    maybeRow { let! head = state.head
                               head.Batches.Add(bh)
                             } |> ignore
                    {state with batch = SomeRow(bh); lineNo = next}
                let foundFileControl fc = 
                    maybeRow { let! head = state.head
                               head.FileControl<- SomeRow(fc)
                             } |> ignore
                    {state with finished = true}
                let foundEntryDetail (ed:EntryDetail) = 
                    maybeRow { let! batch = state.batch
                               batch.Entries.Add(ed)
                             } |>ignore
                    { state with
                        entry = SomeRow(ed)
                        addenda = ed.AddendaRecordedIndicator
                        lineNo = next }
                let foundBatchControl bc = 
                    maybeRow { let! batch = state.batch
                               batch.BatchControl <- SomeRow(bc)
                             } |> ignore
                    { state with
                            batch = NoRow
                            entry = NoRow
                            addenda = 0
                            lineNo = next }
                let foundEntryAddenda add = 
                    maybeRow { let! ed = state.entry
                               ed.Addenda.Add(add)
                             } |> ignore
                    { state with addenda= 2; lineNo = next}
                
                //Walk the file
                match (state.head, state.batch) with
                    | NoRow, _ ->
                         match lineOftext with
                             | Match.FileHeader state.lineNo (fh) -> 
                                foundFileHeader fh
                             | _ ->  
                                errored
                    | SomeRow(_), NoRow ->
                         match lineOftext with
                             | Match.FileControl state.lineNo fc ->
                                foundFileControl fc                     
                             | Match.BatchHeader state.lineNo bh ->
                                foundBatchHeader bh
                             | _ ->  
                                errored
                    | SomeRow(_), SomeRow(bh) ->
                         match state.entry,state.addenda,lineOftext with
                             | _,_,Match.BatchControl state.lineNo bc -> 
                                 foundBatchControl bc
                             | _,i,Match.EntryDetail bh.StandardEntryClass state.lineNo ed when i <> 1 ->
                                 foundEntryDetail ed
                             | SomeRow(_),i,Match.EntryAddenda state.lineNo add when i > 0 ->
                                 foundEntryAddenda add
                             | _ ->
                                errored
    module internal Match =
        let (|FileHeader|_|)=
            FlatRowProvider.matchRecord FileHeaderRecord 
        let (|FileControl|_|)=
            FlatRowProvider.matchRecord FileControlRecord
        let (|BatchHeader|_|) =
            FlatRowProvider.matchRecord BatchHeaderRecord
        let (|BatchControl|_|) =
            FlatRowProvider.matchRecord BatchControlRecord
        let matchEntryRecord constructor batchSEC =
            FlatRowProvider.matchRecord (fun x-> constructor(batchSEC, x) :> EntryDetail)
        let (|EntryDetail|_|) batchSEC = 
            FlatRowProvider.multiMatch [
                         matchEntryRecord EntryCCD batchSEC
                         matchEntryRecord EntryPPD batchSEC
                         matchEntryRecord EntryWildCard batchSEC 
                       ]
    
        let matchEntryAddendaRecord constructor  =
            FlatRowProvider.matchRecord (fun x-> constructor(x) :> EntryAddenda)
        let (|EntryAddenda|_|) = 
            FlatRowProvider.multiMatch [
                         matchEntryAddendaRecord EntryAddendaWildCard
                       ]
      
    type internal ParseState =
        {
            head:FileHeaderRecord MaybeRow
            batch:BatchHeaderRecord MaybeRow
            entry:EntryDetail MaybeRow
            addenda:int
            finished: bool
            lineNo:int
        }