namespace Example.Rpc

open Example.Serialisation
open Example.Serialisation.Json
open Example.Serialisation.Binary

type Error = {
    Message : string
    Retryable : bool
}
with 
    static member Make( message, retryable ) =  
        { Message = message; Retryable = retryable } :> IRpcError

    static member Make( message ) =  
        { Message = message; Retryable = false } :> IRpcError
         
    interface IRpcError
        with 
            member this.Message = this.Message
            
            member this.Retryable = this.Retryable
            
    interface ITypeSerialisable
        with 
            member this.Type with get () = typeof<Error>
                 
    static member JSONSerialiser 
        with get () = 
            { new ITypeSerialiser<Error>
                with
                    member this.TypeName =
                        "__r_error"
    
                    member this.Type
                        with get () = typeof<Error>
    
                    member this.ContentType
                        with get () = "json"
    
                    member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) (v:Error) =
    
                        use js =
                            JsonSerialiser.Make( serialiser, stream, this.ContentType )
    
                        js.WriteStartObject()
                        js.WriteProperty "@type"
                        js.WriteValue this.TypeName
    
                        js.WriteProperty "Message"
                        js.Serialise v.Message

                        js.WriteProperty "Retryable"
                        js.Serialise v.Retryable
                        
                        js.WriteEndObject()
    
                    member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
    
                        use jds =
                            JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )
    
                        jds.Handlers.On "Message" ( jds.ReadString )
                        jds.Handlers.On "Retryable" ( jds.ReadBool )                                                    
    
                        jds.Deserialise()
    
                        let result =
                            {
                                Message = jds.Handlers.TryItem<_>( "Message" ).Value
                                Retryable = jds.Handlers.TryItem<_>( "Retryable" ).Value
                            }
    
                        result }
                                                    
    static member BinarySerialiser 
        with get () =   
            { new ITypeSerialiser<Error> 
                with 
                    member this.TypeName =
                        "__r_error"
            
                    member this.Type 
                        with get () = typeof<Error> 
                       
                    member this.ContentType = 
                        "binary" 
                                                   
                    member this.Serialise (serialiser:ISerde) (s:ISerdeStream) (v:Error) =

                        use bs = 
                            BinarySerialiser.Make( serialiser, s, this.TypeName )
                            
                        bs.Write v.Message
                        bs.Write v.Retryable
                        
                    member this.Deserialise (serialiser:ISerde) (s:ISerdeStream) =
                    
                        use bds = 
                            BinaryDeserialiser.Make( serialiser, s, this.TypeName )

                        let message = 
                            bds.ReadString()
                            
                        let retryable = 
                            bds.ReadBool()
                                
                        { Message = message; Retryable = retryable } }                
