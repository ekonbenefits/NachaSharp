namespace NachaSharp
open FSharp.Data.FlatFileMeta.MetaDataHelper

module NachaFile =
    let (|FileHeaderMatch|_|)=
        matchRecord(FileHeaderRecord) 
    let (|FileControlMatch|_|)=
        matchRecord(FileControlRecord) 
    let (|BatchHeaderMatch|_|) =
        matchRecord(BatchHeaderRecord) 
    let (|BatchControlMatch|_|) =
        matchRecord(BatchControlRecord) 
    let (|EntryDetailMatch|_|) =
        matchRecord(EntryDetail) 

    let Parse (lines: string seq) =
        let mutable head: FileHeaderRecord option = None
        let enumerator = lines.GetEnumerator();
        while head.IsNone && enumerator.MoveNext() do
            match enumerator.Current with
                | FileHeaderMatch (fh) -> 
                    head <- Some(fh)
                    while fh.Trailer.IsNone && enumerator.MoveNext() do
                        match enumerator.Current with
                            | FileControlMatch t ->
                                fh.Trailer <- Some(t)
                            | BatchHeaderMatch bh ->
                                fh.Children <- fh.Children @ [bh]
                                while bh.Trailer.IsNone && enumerator.MoveNext() do
                                    match enumerator.Current with
                                        | BatchControlMatch  bt -> bh.Trailer <- Some(bt)
                                        | EntryDetailMatch ed ->
                                            bh.Children <- bh.Children @ [ed]
                                        | _ -> ()
                            | _ -> ()
                | _ -> ()
        head
