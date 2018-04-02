namespace NachaSharp

open FSharp.Data.FlatFileMeta

[<AbstractClass>]
type EntryAddenda(typeCode, rowInput) =
    inherit NachaRecord(rowInput, "7")
    
    
    override this.IsIdentified() =
            base.IsIdentified() 
            && ( typeCode = -1 ||
                 typeCode = this.AddendaTypeCode)
            
    member this.AddendaTypeCode
            with get () = this.GetColumn ()
            and set value = this.SetColumn<int> value  
            
            

type EntryAddendaWildCard(rowInput) =
    inherit EntryAddenda(-1, rowInput)
    override this.Setup () = setupMetaFor this {
            columns      1 this.RecordTypeCode NachaFormat.alpha
            placeholder 93
            checkLength 94
        }
        
type EntryAddenda05(rowInput) =
    inherit EntryAddenda(5,rowInput)
    override this.Setup () = setupMetaFor this {
            columns      1 this.RecordTypeCode  NachaFormat.alpha
            columns      2 this.AddendaTypeCode NachaFormat.numeric
            columns     80 this.PaymentInfo     NachaFormat.alpha
            columns      4 this.AddendaSeqNum   NachaFormat.numeric
            columns      7 this.EntrySeqNum     NachaFormat.alpha
            checkLength 94
        }
    
      
    member this.PaymentInfo
            with get () = this.GetColumn ()
            and set value = this.SetColumn<string> value 
    
    member this.AddendaSeqNum
                with get () = this.GetColumn ()
                and set value = this.SetColumn<int> value
                
    member this.EntrySeqNum
                    with get () = this.GetColumn ()
                    and set value = this.SetColumn<string> value 