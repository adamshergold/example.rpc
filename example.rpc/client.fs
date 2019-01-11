namespace Example.Rpc

open Microsoft.Extensions.Logging  

open Example.Serialisation
open Example.Messaging 

[<AutoOpen>]
module ClientImpl = 
                    
    type Handler( logger: ILogger option, correlationId: string ) = 
    
        let mutable result : IRpcResult option = None 
         
        let mre = 
            new System.Threading.ManualResetEvent(false) 
            
        member val WaitHandle = mre
                 
        static member Make( logger, correlationId ) = 
            new Handler( logger, correlationId ) 

        member this.Result
            with get () = 
                lock this ( fun _ ->
                    match result with
                    | Some v -> v 
                    | None -> failwithf "Unable to access un-set result!" )  
                                                
        member this.OnMessage (message:IMessage) = 
        
            async {
                if logger.IsSome then 
                    logger.Value.LogTrace( "Handler::onMessage {Message}", message )
                    
                if message.CorrelationId.IsSome && message.CorrelationId.Value = correlationId then
                
                    let candidateResult = 
                        match message.Body with 
                        | Body.Error(error) ->
                            let error = 
                                Example.Rpc.Error.Make( error.Message, error.Retryable )
                            Result.Make( error )
                             
                        | Body.Content(ts) ->
                            match ts with 
                            | :? IRpcResult as result ->
                                result
                            | _ ->
                                let error = 
                                    Example.Rpc.Error.Make( sprintf "Unexpected message contents on RMI call - '%O'" (ts.GetType()) )
                                    
                                Result.Make( error ) 
    
                    lock this ( fun _ -> 
                        result <- Some candidateResult ) 
                                                    
                    mre.Set() |> ignore
                    
                return None
            }
                                             
type Client( serialiser : ISerde, options: ClientOptions ) =
    inherit Base( options.Logger )

    let sendingRecipient =
        if options.Messaging.IsSome then 
            Some <| options.Messaging.Value.Recipient
        else  
            None
            
    static member Make( serialiser, options ) = 
        new Client( serialiser, options ) :> IRpcClient 

    member this.Dispose () = 
        base.Dispose() 
        

    member this.CallRemote (context:IRpcRequestContext,req:'ReqT) : Async<IRpcResult> =
            
        async {                
            if sendingRecipient.IsNone then 
                failwithf "No messaging component available so cannot attempt remote call!" 
            
            if options.Messaging.IsNone then  
                failwithf "No remote label specified in options so do not know where to direct message!"
                
            let correlationId = 
                context.RequestId 
                
            let message = 
            
                let content = 
                    Request.Make( context, req ) 
                
                let header = 
                    Header.Make( Some "rmi", Some( sendingRecipient.Value.RecipientId ) )
                        
                Message.Make( Some correlationId, header, Body.Content( content ) )             
    
            if this.Logger.IsSome then 
                this.Logger.Value.LogTrace( "Client::CallRemote - {ClientId} Message {Message}", this.Id, message ) 
                
            let sendTo = 
                Recipients.ToAny( Some options.Messaging.Value.Label )
    
            let handler = 
                Handler.Make( this.Logger, correlationId ) 
                
            let receiver = 
                Receiver.Make( Some "rmi", handler.OnMessage )
                
            let isGoodResult =                
                try                
                    sendingRecipient.Value.AddReceiver <| receiver
        
                    sendingRecipient.Value.Send (sendTo,message)
                
                    Async.AwaitWaitHandle( handler.WaitHandle, options.Messaging.Value.TimeoutMilliseconds ) |> Async.RunSynchronously 
                finally 
                    sendingRecipient.Value.RemoveReceiver receiver.ReceiverId |> ignore 
                
            if isGoodResult then 
                return handler.Result 
            else 
                return Result.Make( Example.Rpc.Error.Make( "Request timed-out!" ) ) }
              
    member this.CallLocal<'ReqT when 'ReqT :> ITypeSerialisable> (wrapper:IWrapper) (context:IRpcRequestContext,req:'ReqT) : Async<IRpcResult> = 
        async { 
            let result = 
                wrapper.Invoke (context,req) 
            return result }                
                                            
    member this.Call<'ReqT when 'ReqT :> ITypeSerialisable> (context:IRpcRequestContext,req:'ReqT) : Async<IRpcResult> = 
    
        let lookupType = typeof<'ReqT> 
        
        match this.TryGetWrapper lookupType with  
        | None ->
            this.CallRemote (context,req)
        | Some wrapper ->
            this.CallLocal wrapper (context,req)
                                
    interface System.IDisposable 
        with    
            member this.Dispose () = 
                this.Dispose() 
                        
    interface IRpcClient 
        with 
            member this.Register method =
                this.Register method 
                
            member this.Call<'ReqT when 'ReqT :> ITypeSerialisable> (context,req) = 
                this.Call<'ReqT> (context,req)   
                
