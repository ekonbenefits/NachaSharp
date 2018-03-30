namespace NachaSharp


open FSharp.Data.FlatFileMeta

[<AbstractClass>]
type NachaRecord(rowInput, recordTypeCode) =
    inherit FlatRecord(rowInput)

    
    override this.PostSetup() =
        this.RecordTypeCode <- recordTypeCode
            
    override this.IsIdentified() =
        let blockFiller = "9"
        let charBlockFiller = blockFiller |> Seq.head
        this.RecordTypeCode = recordTypeCode
            && (this.RecordTypeCode <> blockFiller
                || this.ToRawString() |> Seq.exists (fun x -> x <> charBlockFiller))
                
    member this.RecordTypeCode
        with get () = this.GetColumn ()
        and set value = this.SetColumn<string> value