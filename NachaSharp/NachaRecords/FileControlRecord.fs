namespace NachaSharp

open System
open System.Collections.Generic
open FSharp.Data.FlatFileMeta
open FSharp.Data.FlatFileMeta.MetaDataHelper

type FileControlRecord(rowInput) =
    inherit NachaRecord(rowInput, "9")

    override this.Setup () = 
        setup this <|
                lazy ({ 
                         columns =[
                                  ]
                         length = 94
                     })
       