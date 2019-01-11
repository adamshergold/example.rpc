namespace Example.Rpc

open Example.Serialisation
open Example.Serialisation.Json
open Example.Serialisation.Binary

type Result = { 
    Success : IRpcSuccess option 
    Error : IRpcError option
}
with 
    static member Make( success ) = 
        { Success = Some success; Error = None } :> IRpcResult

    static member Make( error ) = 
        { Success = None; Error = Some error } :> IRpcResult
        
    member this.IsSuccess = 
        this.Success.IsSome && this.Error.IsNone    
           
    interface IRpcResult
        with 
            member this.IsSuccess = this.IsSuccess 
            
            member this.Success = this.Success.Value
            
            member this.Error = this.Error.Value
            
    interface ITypeSerialisable
                 
    static member JSONSerialiser 
        with get () = 
            { new ITypeSerde<Result>
                with
                    member this.TypeName =
                        "__r_result"
    
                    member this.ContentType
                        with get () = "json"
    
                    member this.Serialise (serialiser:ISerde) (stream:ISerdeStream) (v:Result) =
    
                        use js =
                            JsonSerialiser.Make( serialiser, stream, this.ContentType )
    
                        js.WriteStartObject()
                        js.WriteProperty "@type"
                        js.WriteValue this.TypeName
    
                        if v.Success.IsSome then 
                            js.WriteProperty "Success"
                            js.Serialise v.Success.Value
    
                        if v.Error.IsSome then 
                            js.WriteProperty "Error"
                            js.Serialise v.Error.Value
                        
                        js.WriteEndObject()
    
                    member this.Deserialise (serialiser:ISerde) (stream:ISerdeStream) =
    
                        use jds =
                            JsonDeserialiser.Make( serialiser, stream, this.ContentType, this.TypeName )
    
                        jds.Handlers.On "Success" ( jds.ReadInterface "IRpcSuccess" )
                        jds.Handlers.On "Error" ( jds.ReadInterface "IRpcError"  )                                                    
    
                        jds.Deserialise()
    
                        let result =
                            {
                                Success = jds.Handlers.TryItem<_>( "Success" )
                                Error = jds.Handlers.TryItem<_>( "Error" )
                            }
    
                        result }
                                                    
    static member BinarySerialiser 
        with get () =   
            { new ITypeSerde<Result> 
                with 
                    member this.TypeName =
                        "__r_result"
            
                    member this.ContentType = 
                        "binary" 
                                                   
                    member this.Serialise (serialiser:ISerde) (s:ISerdeStream) (v:Result) =

                        use bs = 
                            BinarySerialiser.Make( serialiser, s, this.TypeName )
                            
                        bs.Write v.Success.IsSome 
                        
                        if v.Success.IsSome then 
                            bs.Write v.Success.Value 

                        bs.Write v.Error.IsSome 
                        
                        if v.Error.IsSome then 
                            bs.Write v.Error.Value 
                            
                        
                    member this.Deserialise (serialiser:ISerde) (s:ISerdeStream) =
                    
                        use bds = 
                            BinaryDeserialiser.Make( serialiser, s, this.TypeName )

                        let success = 
                            if bds.ReadBool() then Some( bds.ReadInterface<_>() ) else None

                        let error = 
                            if bds.ReadBool() then Some( bds.ReadInterface<_>() ) else None
                            
                        { Success = success; Error = error } }              
        
         
    
//type ResultT<'T when 'T :> ITypeSerialisable>( success: IRpcSuccess<'T> option, error : IRpcError option ) =
//
//    member val Success = success 
//    
//    member val Error = error
//with 
//    static member Make( success, failure ) =  
//        new ResultT<_>( success, failure ) :> IRpcResult<_>
//
//    static member Make( success ) =  
//        new ResultT<_>( Some success, None ) :> IRpcResult<_>
//
//    static member Make( error ) =  
//        new ResultT<_>( None, Some error ) :> IRpcResult<_>
//
//    interface IRpcResult 
//        with 
//            member this.IsSuccess = this.Success.IsSome 
//            
//            member this.Success = this.Success.Value :> IRpcSuccess
//            
//            member this.Error = this.Error.Value
//                     
//    interface IRpcResult<'T>
//        with 
//            member this.Success = this.Success.Value
            
            
