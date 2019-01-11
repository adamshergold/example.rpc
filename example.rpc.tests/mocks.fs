namespace Example.Rpc.Tests

open Example.Serialisation
open Example.Serialisation.Json
open Example.Serialisation.Binary

open Example.Messaging 

module Mocks = 

    type EchoRequest = {
        Message : string 
    }
    with 
        static member Make( message ) = 
            { Message = message } 
            
        interface ITypeSerialisable

        static member BinarySerialiser 
            with get () =   
                { new ITypeSerde<EchoRequest> 
                    with 
                        member this.TypeName =
                            "EchoRequest"
                
                        member this.ContentType = 
                            "binary" 
                                                       
                        member this.Serialise (serialiser:ISerde) (s:ISerdeStream) (v:EchoRequest) =
        
                            use bs = 
                                BinarySerialiser.Make( serialiser, s, this.TypeName )
                                
                            bs.Write v.Message
                                                            
        
                        member this.Deserialise (serialiser:ISerde) (s:ISerdeStream) =
                        
                            use bds = 
                                BinaryDeserialiser.Make( serialiser, s, this.TypeName )
        
                            let message = 
                                bds.ReadString()
                                
                            EchoRequest.Make( message ) }
                        
        static member JSONSerialiser 
            with get () = 
                { new ITypeSerde<EchoRequest>
                    with
                        member this.TypeName =
                            "EchoRequest"
        
                        member this.ContentType
                            with get () = "json"
        
                        member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) (v:EchoRequest) =
        
                            use js =
                                JsonSerialiser.Make( serialiser, stream, this.ContentType )
        
                            js.WriteStartObject()
                            js.WriteProperty "@type"
                            js.WriteValue this.TypeName
        
                            js.WriteProperty "Message"
                            js.Serialise v.Message
    
                            js.WriteEndObject()
        
                        member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
        
                            use jds =
                                JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )
        
                            jds.Handlers.On "Message" ( jds.ReadString )
                            
                            jds.Deserialise()
        
                            let result =
                                {
                                    Message = jds.Handlers.TryItem<_>( "Message" ).Value
                                }
        
                            result }

    type EchoResult = {
        Reply : string 
    }
    with 
        static member Make( reply ) = 
            { Reply = reply } 
            
        interface ITypeSerialisable

        static member JSONSerialiser 
            with get () = 
                { new ITypeSerde<EchoResult>
                    with
                        member this.TypeName =
                            "EchoResult"
        
                        member this.ContentType
                            with get () = "json"
        
                        member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =
        
                            use js =
                                JsonSerialiser.Make( serialiser, stream, this.ContentType )
        
                            js.WriteStartObject()
                            js.WriteProperty "@type"
                            js.WriteValue this.TypeName
        
                            js.WriteProperty "Message"
                            js.Serialise v.Reply
    
                            js.WriteEndObject()
        
                        member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
        
                            use jds =
                                JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )
        
                            jds.Handlers.On "Reply" ( jds.ReadString )
                            
                            jds.Deserialise()
        
                            let result =
                                {
                                    Reply = jds.Handlers.TryItem<_>( "Reply" ).Value
                                }
        
                            result }
                            
                            
    type GreetingRequest = {
        Name : string 
    }
    with 
        static member Make( name ) = 
            { Name = name } 
            
        interface ITypeSerialisable

        static member JSON_Serialiser 
            with get () = 
                { new ITypeSerde<GreetingRequest>
                    with
                        member this.TypeName =
                            "GreetingRequest"
        
                        member this.ContentType
                            with get () = "json"
        
                        member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) (v:GreetingRequest) =
        
                            use js =
                                JsonSerialiser.Make( serialiser, stream, this.ContentType )
        
                            js.WriteStartObject()
                            js.WriteProperty "@type"
                            js.WriteValue this.TypeName
        
                            js.WriteProperty "Name"
                            js.Serialise v.Name
    
                            js.WriteEndObject()
        
                        member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
        
                            use jds =
                                JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )
        
                            jds.Handlers.On "Name" ( jds.ReadString )
                            
                            jds.Deserialise()
        
                            let result =
                                {
                                    Name = jds.Handlers.TryItem<_>( "Name" ).Value
                                }
        
                            result }

        static member Binary_Serialiser 
            with get () = 
                { new ITypeSerde<GreetingRequest>
                    with
                        member this.TypeName =
                            "GreetingRequest"
        
                        member this.ContentType
                            with get () = "binary"
        
                        member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) (v:GreetingRequest) =
        
                            use bs =
                                BinarySerialiser.Make( serialiser, stream, this.TypeName )

                            bs.Write v.Name        
        
                        member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
        
                            use bds =
                                BinaryDeserialiser.Make( serialiser, stream, this.TypeName )
        
                            let name = 
                                bds.ReadString()
                            
                            let result =
                                {
                                    Name = name
                                }
        
                            result }
                            
    type GreetingResult = {
        Salutation : string 
    }
    with 
        static member Make( salutation ) = 
            { Salutation = salutation } 
            
        interface ITypeSerialisable

        static member JSONSerialiser 
            with get () = 
                { new ITypeSerde<GreetingResult>
                    with
                        member this.TypeName =
                            "GreetingResult"
        
                        member this.ContentType
                            with get () = "json"
        
                        member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) v =
        
                            use js =
                                JsonSerialiser.Make( serialiser, stream, this.ContentType )
        
                            js.WriteStartObject()
                            js.WriteProperty "@type"
                            js.WriteValue this.TypeName
        
                            js.WriteProperty "Salutation"
                            js.Serialise v.Salutation
    
                            js.WriteEndObject()
        
                        member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
        
                            use jds =
                                JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )
        
                            jds.Handlers.On "Salutation" ( jds.ReadString )
                            
                            jds.Deserialise()
        
                            let result =
                                {
                                    Salutation = jds.Handlers.TryItem<_>( "Salutation" ).Value
                                }
        
                            result }      
                            
                                     