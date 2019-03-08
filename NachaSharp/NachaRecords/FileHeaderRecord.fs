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

open System
open FSharp.Data.FlatFileMeta
open System.Runtime.InteropServices

type FileHeaderRecord(rowInput) =
    inherit NachaRecord(rowInput, "1")
     
    static member Create(
                         immediateDest:string,
                         immediateOrigin:string,
                         fileIDModifier:string,
                         [<Optional;DefaultParameterValue("")>] immediateDestName: string,
                         [<Optional;DefaultParameterValue("")>] immediateOriginName: string,
                         [<Optional;DefaultParameterValue("")>] referenceCode:string
                         ) = 
        createRow {
            let! fh = FileHeaderRecord
            
            fh.ImmediateDestination <- immediateDest
            fh.ImmediateOrigin <- immediateOrigin
            fh.FileIDModifier <- fileIDModifier
            fh.ImmediateDestinationName <-  immediateDestName
            fh.ImmediateOriginName <-  immediateOriginName
            fh.ReferenceCode <-  referenceCode
                        
            fh.FileControl <- SomeRow <| FileControlRecord.Create()
            
            return fh
        }
        
    override this.PostSetup() =
        base.PostSetup()
        if this.IsNew() then
           this.PriorityCode <- 1
           let now = DateTime.Now
           this.FileCreationDate <- now
           this.FileCreationTime <- Nullable(now)
           this.RecordSize <- 94
           this.BlockingFactor <- 10
           this.FormatCode <- "1"
           
           
    member this.Batches 
        with get () = this.GetChildList<BatchHeaderRecord>(1)
        
    member this.FileControl 
        with get () = this.GetChild<FileControlRecord>(2)
        and set value = this.SetChild<FileControlRecord>(2,value)
        
    member this.TotalRecordCount() =
                            maybeRow {
                                let! fc = this.FileControl
                                return (fc.BatchCount * 2 //batch header and footer
                                          + fc.Entry_AddendaCount //entries and addenda
                                          + 2) // header footer
                            } |> Option.defaultValue 0
           
        
    override this.CalculateImpl () =
         base.CalculateImpl()
         maybeRow {
            let! fc = this.FileControl
            
            fc.BatchCount <- this.Batches.Count
            
            let entries = this.Batches |> Seq.collect (fun x->x.Entries)
            let addenda = entries |> Seq.collect (fun x -> x.Addenda)
            
            fc.Entry_AddendaCount <- (entries |> Seq.length) + (addenda |> Seq.length)
            
            let recordCount = this.TotalRecordCount()
            
            fc.BlockCount <- 
                (recordCount / this.BlockingFactor)
                    + if recordCount % this.BlockingFactor <> 0 then
                            1 
                      else
                            0
                
            fc.TotalCreditEntryAmount <- 
                             entries
                                 |> Seq.filter(fun x-> x.TransactionCode.ActionType = Credit)
                                 |> Seq.sumBy (fun x-> x.Amount)
                              
            fc.TotalDebitEntryAmount <- 
                             entries 
                                 |> Seq.filter(fun x-> x.TransactionCode.ActionType = Debit)
                                 |> Seq.sumBy (fun x-> x.Amount)
                                
            this.Batches |> Seq.iteri (fun i b -> 
                                            b.BatchNumber <- i + 1
                                            maybeRow {
                                                 let! bc = b.BatchControl
                                                 bc.BatchNumber <- b.BatchNumber
                                                } |> ignore
                                            )
            
            this.Batches
                      |> Seq.collect(fun b -> b.Entries |> Seq.map(fun e-> (b,e)))
                      |> Seq.iteri (fun i (b,e) -> e.TraceNumber <- sprintf "%s%s" (b.OriginatingDFIIdentification) (i.ToString("D7")) )
            
         } |> ignore
        
    override this.Setup () = setupMetaFor this {
            columns     1   this.RecordTypeCode         NachaFormat.alpha
            columns     2   this.PriorityCode           NachaFormat.numeric
            columns     10  this.ImmediateDestination   Format.leftPadString
            columns     10  this.ImmediateOrigin        Format.leftPadString
            columns     6   this.FileCreationDate       Format.reqYYMMDD
            columns     4   this.FileCreationTime       Format.optHHMM
            columns     1   this.FileIDModifier         NachaFormat.alphaUpper
            columns     3   this.RecordSize             NachaFormat.numeric
            columns     2   this.BlockingFactor         NachaFormat.numeric
            columns     1   this.FormatCode             NachaFormat.alpha
            columns     23  this.ImmediateDestinationName NachaFormat.alpha
            columns     23  this.ImmediateOriginName    NachaFormat.alpha
            columns     8   this.ReferenceCode          NachaFormat.alpha

            checkLength 94
        }
        
    member this.PriorityCode
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value
        
    member this.ImmediateDestination
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
            
    member this.ImmediateOrigin
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
              
    member this.FileCreationDate
        with get () = this.GetColumn()
        and set value = this.SetColumn<DateTime> value
             
    member this.FileCreationTime
        with get () = this.GetColumn()
        and set value = this.SetColumn<DateTime Nullable> value
        
    member this.FileIDModifier
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
        
    member this.RecordSize
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value
        
    member this.BlockingFactor
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value
        
    member this.FormatCode
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
        
    member this.ImmediateDestinationName
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value
    
    member this.ImmediateOriginName
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value

    member this.ReferenceCode
        with get () = this.GetColumn()
        and set value = this.SetColumn<string> value