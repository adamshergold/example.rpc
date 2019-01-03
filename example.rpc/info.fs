namespace Example.Rpc

open Example.Serialisation
open Example.Serialisation.Json
open Example.Serialisation.Binary

type Info = {
    ElapsedTimeMilliseconds : int
}
with 
    static member Make( elapsed ) =  
        { ElapsedTimeMilliseconds = elapsed } :> IRpcInfo
         
    interface IRpcInfo
        with 
            member this.ElapsedTimeMilliseconds = this.ElapsedTimeMilliseconds
            
    interface ITypeSerialisable
        with 
            member this.Type with get () = typeof<Info>
            
    static member JSONSerialiser 
        with get () = 
            { new ITypeSerialiser<Info>
                with
                    member this.TypeName =
                        "__r_info"
    
                    member this.Type
                        with get () = typeof<Info>
    
                    member this.ContentType
                        with get () = "json"
    
                    member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) (v:Info) =
    
                        use js =
                            JsonSerialiser.Make( serialiser, stream, this.ContentType )
    
                        js.WriteStartObject()
                        js.WriteProperty "@type"
                        js.WriteValue this.TypeName
    
                        js.WriteProperty "ElapsedTimeMilliseconds"
                        js.Serialise v.ElapsedTimeMilliseconds 
                        
                        js.WriteEndObject()
    
                    member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
    
                        use jds =
                            JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )
    
                        jds.Handlers.On "ElapsedTimeMilliseconds" ( jds.ReadInt32 )
    
                        jds.Deserialise()
    
                        let result =
                            {
                                ElapsedTimeMilliseconds = jds.Handlers.TryItem<_>( "ElapsedTimeMilliseconds" ).Value
                            }
    
                        result }            

                                                    
    static member BinarySerialiser 
        with get () =   
            { new ITypeSerialiser<Info> 
                with 
                    member this.TypeName =
                        "__r_info"
            
                    member this.Type 
                        with get () = typeof<Info> 
                       
                    member this.ContentType = 
                        "binary" 
                                                   
                    member this.Serialise (serialiser:ISerde) (s:ISerdeStream) (v:Info) =

                        use bs = 
                            BinarySerialiser.Make( serialiser, s, this.TypeName )
                            
                        bs.Write v.ElapsedTimeMilliseconds
                        
                    member this.Deserialise (serialiser:ISerde) (s:ISerdeStream) =
                    
                        use bds = 
                            BinaryDeserialiser.Make( serialiser, s, this.TypeName )

                        let elapsedTimeMilliseconds = 
                            bds.ReadInt32()
                                                        
                        { ElapsedTimeMilliseconds = elapsedTimeMilliseconds } }       