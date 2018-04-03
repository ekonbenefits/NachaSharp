(*
   Copyright 2018 EkonBenefits

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*)

namespace NachaSharp
open FSharp.Data.FlatFileMeta
open FSharp.Control

module rec NachaFile =
    open FSharp.Data.FlatFileMeta
    open System.Threading.Tasks
    open System.IO
    open System.Text

    let ParseLines lines = FlatRowProvider.syncParseLines asyncParseLinesDef lines
    
    let ParseFile stream =  FlatRowProvider.syncParseFile asyncParseLinesDef stream
    
    
    let AsyncWriteFile(head:FileHeaderRecord, stream) = asyncWriteNachaFile head stream |> Async.StartAsTask
        
       
    let WriteFile(head:FileHeaderRecord, stream) = asyncWriteNachaFile head stream |> Async.RunSynchronously

    let internal asyncWriteNachaFile head stream = async {
             do! FlatRowProvider.asyncWriteFile head stream
             let blocks = maybeRow {
                            let! fc = head.FileControl
                            return fc.BlockCount
                          } |> Option.defaultValue 0
             let remainder = head.BlockingFactor -
                                (head.TotalRecordCount() % head.BlockingFactor)
             
             use writer = new StreamWriter(stream, Encoding.ASCII, 1024, true)
             
             do! [0..(remainder - 1)]
                    |> AsyncSeq.ofSeq
                    |> AsyncSeq.iterAsync(fun _-> async {
                            do! Seq.init head.RecordSize (fun _ ->"9") 
                                    |> String.concat ""
                                    |> writer.WriteLineAsync 
                                    |> Async.AwaitTask
                            })
             do! writer.FlushAsync() |> Async.AwaitTask     
         }

                   
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
                         matchEntryRecord EntryCTX batchSEC
                         matchEntryRecord EntryPPD batchSEC
                         matchEntryRecord EntryWildCard batchSEC 
                       ]
    
        let matchEntryAddendaRecord constructor  =
            FlatRowProvider.matchRecord (fun x-> constructor(x) :> EntryAddenda)
        let (|EntryAddenda|_|) = 
            FlatRowProvider.multiMatch [
                         matchEntryAddendaRecord EntryAddenda05
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