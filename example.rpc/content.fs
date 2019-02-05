namespace Example.Rpc 

open Example.Serialisation
open Example.Serialisation.Json
open Example.Serialisation.Binary

type Content = {
    SerialisableValue : ITypeSerialisable option
    WrappedValue : ITypeWrapper option
} 
with
    static member Create( v:ITypeSerialisable option, wv: ITypeWrapper option) = 
        { SerialisableValue = v; WrappedValue = wv } 
    
    static member Make( v:ITypeSerialisable ) = 
        { SerialisableValue = Some v; WrappedValue = None } :> IRpcContent

    static member Make( wrapped:ITypeWrapper ) = 
        { SerialisableValue = None; WrappedValue = Some wrapped } :> IRpcContent

    member this.IsWrapped
        with get () = this.WrappedValue.IsSome
        
    member this.Value 
        with get () =
            this.Value
            
    member this.Wrapped 
        with get () = this.WrappedValue 
                            
    interface IRpcContent 
        with 
            member this.IsWrapped = this.IsWrapped 
            
            member this.Value = this.SerialisableValue.Value
            
            member this.Wrapped = this.WrappedValue.Value
            
    interface ITypeSerialisable
    
    static member JSON_Serialiser 
        with get () = 
            { new ITypeSerde<Content>
                with
                    member this.TypeName =
                        "__r_content"
    
                    member this.ContentType
                        with get () = "json"
    
                    member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =
    
                        use js =
                            JsonSerialiser.Make( serialiser, stream, this.ContentType )
    
                        js.WriteStartObject()
                        js.WriteProperty "@type"
                        js.WriteValue this.TypeName
    
                        if v.SerialisableValue.IsSome then 
                            js.WriteProperty "Value"
                            js.Serialise v.SerialisableValue.Value
                            
                        if v.WrappedValue.IsSome then    
                            js.WriteProperty "Wrapped"
                            
                            js.WriteStartObject()
                              
                            let wv = v.Wrapped.Value 
                              
                            js.WriteProperty "ContentType"
                            js.WriteValue wv.ContentType
                            
                            if wv.TypeName.IsSome then
                                js.WriteProperty "TypeName"
                                js.WriteValue wv.TypeName.Value
                            
                            js.WriteProperty "Body"
                            js.WriteValue wv.Body
                            
                            js.WriteEndObject()
    
                        js.WriteEndObject()
    
                    member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
    
                        use jds =
                            JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )
    
                        jds.Handlers.On "Value" ( jds.ReadITypeSerialisable )
                        jds.Handlers.On "Wrapped" ( jds.ReadMap<string>( jds.ReadString ) )
    
                        jds.Deserialise()
    
                        let result =
                            if jds.Handlers.Has "Value" then
                                Content.Create( jds.Handlers.TryItem<ITypeSerialisable>( "Value" ), None )
                            else if jds.Handlers.Has "Wrapped" then
                                let wrapper = 
                                    let v : Map<string,string> = 
                                        jds.Handlers.TryItem<_>( "Wrapped" ).Value
                                    
                                    let contentType = 
                                        v.Item("ContentType")
                                        
                                    let typeName =
                                        v.TryFind "TypeName" 
                                             
                                    let body = 
                                        v.Item("Body") |> System.Text.Encoding.UTF8.GetBytes
                                                                               
                                    TypeWrapper.Make( contentType, typeName, body )
                                    
                                Content.Create( None, Some wrapper )
                            else
                                failwithf "Unable to determine union case when deserialising [%s]" this.TypeName
    
                        result  }
                        
    static member BinarySerialiser 
        with get () =   
            { new ITypeSerde<Content> 
                with 
                    member this.TypeName =
                        "__r_content"
            
                    member this.ContentType = 
                        "binary" 
                                                   
                    member this.Serialise (serialiser:ISerde) (s:ISerdeStream) (v:Content) =

                        use bs = 
                            BinarySerialiser.Make( serialiser, s, this.TypeName )
                            
                        bs.Write v.SerialisableValue.IsSome 
                        if v.SerialisableValue.IsSome then 
                        
                            let tw = 
                                Helpers.Wrap serialiser v.SerialisableValue.Value [ "binary"; "json" ] 

                            bs.Write( tw.ContentType )
                                
                            bs.Write( tw.TypeName.IsSome )
                            if tw.TypeName.IsSome then 
                                bs.Write( tw.TypeName.Value )
                            
                            bs.Write( (int32) tw.Body.Length )
                            bs.Write( tw.Body )

                        bs.Write v.WrappedValue.IsSome 
                        if v.WrappedValue.IsSome then
                            let wv = v.WrappedValue.Value
                             
                            bs.Write wv.ContentType

                            if wv.TypeName.IsSome then                            
                                bs.Write wv.TypeName.Value
                            
                            bs.Write( (int32) wv.Body.Length )
                            bs.Write wv.Body    
                        
                    member this.Deserialise (serialiser:ISerde) (s:ISerdeStream) =
                    
                        use bds = 
                            BinaryDeserialiser.Make( serialiser, s, this.TypeName )

                        let serialisableValue = 
                            if bds.ReadBool() then
                               
                               let contentType = 
                                   bds.ReadString()
                            
                               let typeName = 
                                   if bds.ReadBool() then Some( bds.ReadString() ) else None
                                   
                               let body = 
                                   bds.ReadBytes( bds.ReadInt32() )
                                            
                               let tw = 
                                   TypeWrapper.Make( contentType, typeName, body ) 
  
                               Some( Helpers.Unwrap serialiser tw :?> ITypeSerialisable )
                               
                            else 
                                None 

                        let wrappedValue = 
                            if bds.ReadBool() then
                                
                                let contentType = 
                                    bds.ReadString()
                            
                                let typeName = 
                                    if bds.ReadBool() then Some( bds.ReadString() ) else None
                                    
                                let body =
                                    let n = bds.ReadInt32()
                                    bds.ReadBytes(n)
                                
                                Some <| TypeWrapper.Make( contentType, typeName, body )             
                            else 
                                None
                                                            
                        { SerialisableValue = serialisableValue; WrappedValue = wrappedValue } }                            
                             
                            