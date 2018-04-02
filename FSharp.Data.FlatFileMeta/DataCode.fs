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
namespace rec FSharp.Data.FlatFileMeta

open System.Collections.Generic
open System.IO
open System
open System.ComponentModel
open System.Runtime.CompilerServices
open FSharp.Interop.Compose.Linq
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns

[<AbstractClass>]
type DataCode()=
    static member op_Implicit(a: DataCode) =
        a.Code
    
    member val Code:string = null with get,set
    
    abstract IsKnown: bool
        
    member this.ToRawString() = this.Code  
    
[<AbstractClass>]
type DataCode< 'T when 'T :> DataCode<'T> and  'T: ( new : unit -> 'T ) >() as self=
    inherit DataCode()

    member this.LazySetup() =
        match DataCode<'T>.Meta with 
            | None -> 
                let lazyMeta = this.Setup()
                let meta = lazyMeta.Force()
                let keyMap = meta.codes
                                        |> Seq.map (fun x->x.Key, x)
                                        |> Map.ofSeq
                let valueMap = meta.codes
                                        |> Seq.map (fun x->x.Key, x)
                                        |> Map.ofSeq          
                DataCode<'T>.Meta <- Some <| { 
                                           keyMap = keyMap
                                           valueMap= valueMap
                                        } 
             | Some(_) -> ()
        
    member this.Key = 
                this.LazySetup()
                try
                    DataCode<'T>.Meta.Value.valueMap.[this.Code].Key
                with _ -> null
      
    override this.IsKnown = this.Key |> isNull |> not
    
        
    abstract member Setup : unit -> DataCodeMeta Lazy
    
    static member val internal Meta:ProcessedDataCodeMeta option = None with get,set
        
    static member GetCode<'T>([<CallerMemberName>] ?memberName: string) : 'T =
        let key =  memberName |> Option.defaultWith Helper.raiseMissingCompilerMemberName
        
        if DataCode<'T>.Meta |> Option.isNone then
            let dummy = DataCode<'T>.Create("")
            dummy.LazySetup()
        
        match DataCode<'T>.Meta with
            | None -> invalidOp "MetaData should be initialized by this point"
            | Some(m) -> let codeMap = m.keyMap.[key]
                         DataCode<'T>.Create(codeMap.Code)
    
    static member Create(code:string):'T =
            let t = new 'T()
            t.Code <- code
            t
       
type DataCodeIdent = { Key:string; Code:string; }
           

type DataCodeMapping(key: string, code:string) =
    member __.Key = key
    member __.Code = code

type DataCodeMeta = { 
    codes: DataCodeMapping list
}

type ProcessedDataCodeMeta = { 
                                keyMap: Map<string,DataCodeMapping>
                                valueMap: Map<string, DataCodeMapping> 
                             }

[<AutoOpen>]
module DataCodeExtension =
    type DataCodeBuilder() =
        member __.Yield(x) = {codes = []; }
                
        member __.Delay(x) = lazy (x())
        
        /// Defines width of property and how to format.
        [<CustomOperation("code")>] 
        member __.Code (meta : DataCodeMeta, [<ReflectedDefinition>] name:Expr<'T> , value:string) = 
           let key = 
                match name with
                | PropertyGet(_, propOrValInfo, _) -> propOrValInfo.Name
                | ________________________________ -> invalidArg "name" "Must be a property get"
            
           { meta with codes = meta.codes @ [DataCodeMapping(key, value)]}
           
           
    let dataCodeMeta = DataCodeBuilder ()
