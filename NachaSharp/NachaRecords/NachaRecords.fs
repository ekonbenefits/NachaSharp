namespace NachaSharp


open FSharp.Data.FlatFileMeta

[<AbstractClass>]
type NachaRecord(rowInput, recordTypeCode) =
    inherit FlatRecord(rowInput)

    
    override this.PostSetup() =
        if this.IsNew() then
           this.RecordTypeCode <- recordTypeCode
            
    override this.IsIdentified() =
        let blockFiller = "9"
        let charBlockFiller = blockFiller |> Seq.head
        let matchesType = this.RecordTypeCode = recordTypeCode
        let notPossibleToBeFiller = this.RecordTypeCode <> blockFiller
        let verifyIsNotFiller = lazy (this.ToRawString() |> Seq.exists (fun x -> x <> charBlockFiller))
        
        matchesType
            && (notPossibleToBeFiller || verifyIsNotFiller.Force())
                
    member this.RecordTypeCode
        with get () = this.GetColumn ()
        and set value = this.SetColumn<string> value