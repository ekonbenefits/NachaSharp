namespace NachaSharp

open System
open System.Collections.Generic
open FSharp.Data.FlatFileMeta
open FSharp.Data.FlatFileMeta.MetaDataHelper

type FileControlRecord(rowInput) =
    inherit NachaRecord(rowInput, "9")
    
    static member Create() =
        MetaDataHelper.createRecord FileControlRecord 
            (fun fc ->
                
                ()
            )
    
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                    MetaColumn.Make( 1, this.RecordTypeCode, Format.leftPadString)
                                    MetaColumn.Make( 6, this.BatchCount, Format.zerodInt)
                                    MetaColumn.Make( 6, this.BlockCount, Format.zerodInt)
                                    MetaColumn.Make( 8, this.Entry_AddendaCount, Format.zerodInt)
                                    MetaColumn.Make(10, this.EntryHash, Format.zerodInt)
                                    MetaColumn.Make( 12, this.TotalDebitEntryDollar, Format.reqMoney)
                                    MetaColumn.Make( 12, this.TotalCreditEntryDollar, Format.reqMoney)
                                    MetaColumn.PlaceHolder(39)
                                  ]
                         length = 94
                     })

    member this.BatchCount 
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value

    member this.BlockCount
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value

    member this.Entry_AddendaCount
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value

    member this.EntryHash
        with get () = this.GetColumn()
        and set value = this.SetColumn<int> value
       
    member this.TotalDebitEntryDollar
        with get () = this.GetColumn()
        and set value = this.SetColumn<decimal> value

    member this.TotalCreditEntryDollar
        with get () = this.GetColumn()
        and set value = this.SetColumn<decimal> value
