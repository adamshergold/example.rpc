namespace Example.Rpc

open Example.Serialisation
open Example.Serialisation.Json 
open Example.Serialisation.Binary

type RequestContext = {
    RequestId : string
    Tag : string option
    UserId : string 
}
with 
    static member Make( id, tag, userId ) = 
        { RequestId = id; Tag = tag; UserId = userId } :> IRpcRequestContext
         
    static member Make() = 
        { RequestId = System.Guid.NewGuid().ToString("N"); Tag = None; UserId = System.Environment.UserName } :> IRpcRequestContext
                  
    interface IRpcRequestContext
        with 
            member this.RequestId = this.RequestId
            
            member this.Tag = this.Tag
            
            member this.UserId = this.UserId
            
    interface ITypeSerialisable
        with 
            member this.Type with get () = typeof<RequestContext>
                  
    static member JSONSerialiser 
        with get () = 
            { new ITypeSerialiser<RequestContext>
                with
                    member this.TypeName =
                        "_r_context"
    
                    member this.Type
                        with get () = typeof<RequestContext>
    
                    member this.ContentType
                        with get () = "json"
    
                    member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) (v:RequestContext) =
    
                        use js =
                            JsonSerialiser.Make( serialiser, stream, this.ContentType )
    
                        js.WriteStartObject()
                        js.WriteProperty "@type"
                        js.WriteValue this.TypeName
    
                        js.WriteProperty "RequestId"
                        js.Serialise v.RequestId 

                        if v.Tag.IsSome then 
                            js.WriteProperty "Tag"
                            js.Serialise v.Tag.Value

                        js.WriteProperty "UserId"
                        js.Serialise v.UserId 
                        
                        js.WriteEndObject()
    
                    member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
    
                        use jds =
                            JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )
    
                        jds.Handlers.On "RequestId" ( jds.ReadString )
                        jds.Handlers.On "Tag" ( jds.ReadString )
                        jds.Handlers.On "UserId" ( jds.ReadString )                                                    
    
                        jds.Deserialise()
    
                        let result =
                            {
                                RequestId = jds.Handlers.TryItem<_>( "RequestId" ).Value
                                Tag = jds.Handlers.TryItem<_>( "Tag" )
                                UserId = jds.Handlers.TryItem<_>( "UserId" ).Value
                            }
    
                        result }
                                                    
    static member BinarySerialiser 
        with get () =   
            { new ITypeSerialiser<RequestContext> 
                with 
                    member this.TypeName =
                        "_r_context"
            
                    member this.Type 
                        with get () = typeof<RequestContext> 
                       
                    member this.ContentType = 
                        "binary" 
                                                   
                    member this.Serialise (serialiser:ISerde) (s:ISerdeStream) (v:RequestContext) =

                        use bs = 
                            BinarySerialiser.Make( serialiser, s, this.TypeName )
                            
                        bs.Write v.RequestId
                        
                        bs.Write v.Tag.IsSome
                        if v.Tag.IsSome then 
                            bs.Write v.Tag.Value 
                            
                        bs.Write v.UserId 
                                                        
    
                    member this.Deserialise (serialiser:ISerde) (s:ISerdeStream) =
                    
                        use bds = 
                            BinaryDeserialiser.Make( serialiser, s, this.TypeName )

                        let requestId = 
                            bds.ReadString()
                            
                        let tag = 
                            if bds.ReadBool() then Some( bds.ReadString() ) else None
                                
                        let userId = 
                            bds.ReadString()                                
                            
                        { RequestId = requestId; Tag = tag; UserId = userId } }                