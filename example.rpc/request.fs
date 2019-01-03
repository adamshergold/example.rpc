namespace Example.Rpc

open Example.Serialisation
open Example.Serialisation.Json
open Example.Serialisation.Binary

open Example.Messaging 

type Request = {
    Context: IRpcRequestContext
    Inner : ITypeSerialisable
}
with
      
    static member Make( context, inner ) = 
        { Context = context; Inner = inner } 
            
    interface ITypeSerialisable
        with 
            member this.Type with get () = typeof<Request>

    static member JSONSerialiser 
        with get () = 
            { new ITypeSerialiser<Request>
                with
                    member this.TypeName =
                        "__r_request"
    
                    member this.Type
                        with get () = typeof<Request>
    
                    member this.ContentType
                        with get () = "json"
    
                    member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) (v:Request) =
    
                        use js =
                            JsonSerialiser.Make( serialiser, stream, this.ContentType )
    
                        js.WriteStartObject()
                        js.WriteProperty "@type"
                        js.WriteValue this.TypeName
    
                        js.WriteProperty "Context"
                        js.Serialise v.Context 

                        js.WriteProperty "Inner"
                        js.Serialise v.Inner 
                        
                        js.WriteEndObject()
    
                    member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
    
                        use jds =
                            JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )
    
                        jds.Handlers.On "Context" ( jds.ReadRecord "RequestContext" )
                        jds.Handlers.On "Inner" ( jds.ReadSerialisable )                                                    
    
                        jds.Deserialise()
    
                        let result =
                            {
                                Context = jds.Handlers.TryItem<_>( "Context" ).Value
                                Inner = jds.Handlers.TryItem<_>( "Inner" ).Value
                            }
    
                        result }
                        
                            
    static member BinarySerialiser 
        with get () =   
            { new ITypeSerialiser<Request> 
                with 
                    member this.TypeName =
                        "__r_request"
            
                    member this.Type 
                        with get () = typeof<Request> 
                       
                    member this.ContentType = 
                        "binary" 
                                                   
                    member this.Serialise (serialiser:ISerde) (s:ISerdeStream) (v:Request) =
    
                        use bs = 
                            BinarySerialiser.Make( serialiser, s, this.TypeName )
                            
                        bs.Write v.Context
                        bs.Write v.Inner 
                                                        
    
                    member this.Deserialise (serialiser:ISerde) (s:ISerdeStream) =
                    
                        use bds = 
                            BinaryDeserialiser.Make( serialiser, s, this.TypeName )
    
                        let context = 
                            bds.ReadRecord<_>()
                            
                        let inner = 
                            bds.ReadSerialisable()                                
                            
                        Request.Make( context, inner ) }
