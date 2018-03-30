namespace NachaSharp

open FSharp.Data.FlatFileMeta
open FSharp.Data.FlatFileMeta.MetaDataHelper


type BatchControlRecord(rowInput) =
    inherit NachaRecord(rowInput, "8")
    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })