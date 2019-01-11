namespace Example.Rpc

open Example.Serialisation
open Example.Serialisation.Json
open Example.Serialisation.Binary

type Success = {
    Content : IRpcContent option
    Info : IRpcInfo option 
}
with 
    static member Make( content:ITypeSerialisable, info ) = 
        let content = Example.Rpc.Content.Make(content)
        { Content = Some content; Info = info }

    static member Make( content:ITypeSerialisable ) = 
        let content = Example.Rpc.Content.Make(content)
        { Content = Some content; Info = None }

    static member Wrapped( v: ITypeWrapper ) = 
        let content = Example.Rpc.Content.Make( v )
        { Content = Some content; Info = None }

    member this.As<'T when 'T :> ITypeSerialisable> () = 
        match this.Content with 
        | Some c ->
            if c.IsWrapped then 
                failwithf "Attempted to extract Content that was wrapped"
            else 
                match c.Value with 
                | :? 'T as t -> t
                | _ -> failwithf "Attempted to extract content of type '%O' as '%O'!" (this.Content.GetType()) (typeof<'T>)
        | None ->
            failwithf "Attempted to extract Content that was not present"
                    
    interface IRpcSuccess
        with 
            member this.As<'T when 'T :> ITypeSerialisable>() = this.As<'T>()
            
            member this.Content = 
                match this.Content with 
                | Some v -> v 
                | None -> failwithf "No content available!"
            
            member this.Info = this.Info
            
    interface ITypeSerialisable
                 
    static member JSONSerialiser 
        with get () = 
            { new ITypeSerde<Success>
                with
                    member this.TypeName =
                        "__r_success"
    
                    member this.ContentType
                        with get () = "json"
    
                    member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) (v:Success) =
    
                        use js =
                            JsonSerialiser.Make( serialiser, stream, this.ContentType )
    
                        js.WriteStartObject()
                        js.WriteProperty "@type"
                        js.WriteValue this.TypeName
    
                        if v.Content.IsSome then 
                            js.WriteProperty "Content"
                            js.Serialise v.Content.Value
    
                        if v.Info.IsSome then 
                            js.WriteProperty "Info"
                            js.Serialise v.Info.Value
                        
                        js.WriteEndObject()
    
                    member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
    
                        use jds =
                            JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )
    
                        jds.Handlers.On "Content" ( jds.ReadUnion "RMI.Content" )
                        jds.Handlers.On "Info" ( jds.ReadInterface "IRpcInfo"  )                                                    
    
                        jds.Deserialise()
    
                        let result =
                            {
                                Content = jds.Handlers.TryItem<_>( "Content" )
                                Info = jds.Handlers.TryItem<_>( "Info" )
                            }
    
                        result }
                                                    
    static member BinarySerialiser 
        with get () =   
            { new ITypeSerde<Success> 
                with 
                    member this.TypeName =
                        "__r_success"
            
                    member this.ContentType = 
                        "binary" 
                                                   
                    member this.Serialise (serialiser:ISerde) (s:ISerdeStream) (v:Success) =

                        use bs = 
                            BinarySerialiser.Make( serialiser, s, this.TypeName )
                            
                        bs.Write( v.Content.IsSome )
                        if v.Content.IsSome then      
                            bs.Write v.Content.Value
                        
                        bs.Write( v.Info.IsSome )
                        if v.Info.IsSome then 
                            bs.Write v.Info.Value
                        
                    member this.Deserialise (serialiser:ISerde) (s:ISerdeStream) =
                    
                        use bds = 
                            BinaryDeserialiser.Make( serialiser, s, this.TypeName )

                        let content = 
                            if bds.ReadBool() then Some( bds.ReadUnion<_>() ) else None
                            
                        let info = 
                            if bds.ReadBool() then Some( bds.ReadInterface<_>() ) else None 
                                
                        { Content = content; Info = info } }                
       

