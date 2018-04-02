namespace NachaSharp

open System
open FSharp.Data.FlatFileMeta

type FileHeaderRecord(rowInput) =
    inherit NachaRecord(rowInput, "1")
     
    static member Create(
                         immediateDest:string,
                         immediateOrigin:string,
                         fileIDModifier:string,
                         ?immediateDestName: string,
                         ?immediateOriginName: string,
                         ?referenceCode:string
                         ) = 
        createRow {
            let! fh = FileHeaderRecord
            
            fh.ImmediateDestination <- immediateDest
            fh.ImmediateOrigin <- immediateOrigin
            fh.FileIDModifier <- fileIDModifier
            fh.ImmediateDestinationName <- defaultArg immediateDestName ""
            fh.ImmediateOriginName <- defaultArg immediateOriginName ""
            fh.ReferenceCode <- defaultArg referenceCode ""
                        
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
        
    override this.Calculate () =
         base.Calculate()
         maybeRow {
            let! fc = this.FileControl
            
            fc.BatchCount <- this.Batches.Count
            
            let entries = this.Batches |> Seq.collect (fun x->x.Entries)
            let addenda = entries |> Seq.collect (fun x -> x.Addenda)
            
            fc.Entry_AddendaCount <- (entries |> Seq.length) + (addenda |> Seq.length)
            
            fc.BlockCount <- 
                fc.BatchCount * 2
                + fc.Entry_AddendaCount
                + 2
                
            fc.TotalCreditEntryAmount <- 
                             entries
                                 |> Seq.filter(fun x-> match x.TransactionCode with Credit(_) -> true | _-> false)
                                 |> Seq.sumBy (fun x-> x.Amount)
                              
            fc.TotalDebitEntryAmount <- 
                             entries 
                                 |> Seq.filter(fun x-> match x.TransactionCode with Debit(_) -> true | _-> false)
                                 |> Seq.sumBy (fun x-> x.Amount)
                                
            this.Batches |> Seq.iteri (fun i b -> 
                                            b.BatchNumber <- i + 1
                                            maybeRow {
                                                 let! bc = b.BatchControl
                                                 bc.BatchNumber <- b.BatchNumber
                                                } |> ignore
                                            )
            
            
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