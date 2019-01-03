namespace Example.Rpc

open Microsoft.Extensions.Logging 

open Example.Serialisation 

type IWrapper = 
    abstract Handles : System.Type with get 
    abstract Invoke : IRpcRequestContext * obj -> IRpcResult
        

type Wrapper<'ReqT,'ResT when 'ReqT :> ITypeSerialisable and 'ResT :> ITypeSerialisable > = {
    Fn : IRpcRequestContext * 'ReqT -> IRpcResult 
}
with 
    static member Make( fn ) = 
        { Fn = fn } :> IWrapper
        
    member this.Invoke (context:IRpcRequestContext,input:'ReqT) = 
        this.Fn (context,input) 

    interface IWrapper  
        with           
            member this.Handles
                with get () = typeof<'ReqT> 
                
            member this.Invoke (context:IRpcRequestContext,req:obj) = 
                match req with 
                | :? 'ReqT as input -> this.Fn (context,input)
                | _ -> failwithf "Handler invoked with wrong type - expected [%O] saw [%O]" (typeof<'ReqT>) (req.GetType())


type Base( logger: ILogger option ) =

    let id = 
        System.Guid.NewGuid().ToString("N")
       
    let wrappers = 
        new System.Collections.Generic.Dictionary<System.Type,IWrapper>() 
       
    member this.Logger = logger 
            
    member this.Id = id 
          
    member this.Register method = 

        lock this ( fun _ ->
        
            let wrapper = 
                Wrapper<_,_>.Make( method )
                
            if wrappers.ContainsKey wrapper.Handles then 
                failwithf "Cannot add caller for type that is already registered '%O'" wrapper.Handles
            else                 
                wrappers.Add( wrapper.Handles, wrapper )
                 
            if logger.IsSome then 
                logger.Value.LogTrace( "Base::Register - {ClientId} Added handler for {Type}", this.Id, wrapper.Handles ) 
                                 
            )

    member this.TryGetWrapper (st:System.Type) = 
        match wrappers.TryGetValue st with 
        | false, _ -> None
        | true, wrapper -> Some wrapper 
                    
    member this.Dispose () =
        wrappers.Clear()
        
    interface System.IDisposable
        with    
            member this.Dispose () = 
                this.Dispose()