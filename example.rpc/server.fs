namespace Example.Rpc

open Microsoft.Extensions.Logging  

open Example.Serialisation
open Example.Messaging 


type Server( serialiser : ISerde, options: ServerOptions ) as this =
    inherit Base( options.Logger ) 

    let receivingRecipient =
        options.Messaging.Recipient

    do        
        receivingRecipient.AddReceiver <| Receiver.Make( Some "rmi", this.OnMessage ) 
           
    static member Make( serialiser, options ) = 
        new Server( serialiser, options ) :> IRpcServer 

    member this.Dispose () =
        base.Dispose()

    member this.OnMessage (receivedMessage:IMessage) = 
        
        async {
            if this.Logger.IsSome then 
                this.Logger.Value.LogTrace( "Server::OnMessage {ClientId} {Message}", this.Id, receivedMessage )  
            
            let replyToRecipients = 
                if receivedMessage.Header.ReplyTo.IsSome then 
                    Some <| Recipients.ToOne( receivedMessage.Header.ReplyTo.Value ) 
                else 
                    None 
    
            let message = 
                try 
                    let request = 
                        match receivedMessage.Body with 
                        | Body.Content(ts) ->
                            match ts with 
                            | :? Request as req -> 
                                req
                            | _ ->
                                failwithf "Unexpected content-type for body content! Expected 'Request' but saw '%O'" (ts.GetType())
                        | Body.Error(e) ->
                            failwithf "Expecing to receive valid message but received error! %s" e.Message
                            
                    let requestType = 
                        request.Inner.GetType()
                                        
                    let result = 
                        match this.TryGetWrapper requestType with 
                        | None ->
                            failwithf "Unable to find registered handler for request-type '%O'" requestType 
                        | Some wrapper ->
                            wrapper.Invoke (request.Context,request.Inner) 
            
                    let header = 
                        Header.Make( Some "rmi", None )
                        
                    let body = 
                        Body.Content( result )
                                                    
                    Some <| Message.Make( receivedMessage.CorrelationId, header, body )                                            
                with 
                | _ as ex ->
                    if this.Logger.IsSome then 
                        this.Logger.Value.LogError( "Server::OnMessage : Exception! {Exception}", ex )
                    None 
    
            match replyToRecipients, message with 
            | Some replyTo, Some message ->                                          
                return Some ( replyTo, message )
            | _ ->
                return None                          
        }              
                            
    interface System.IDisposable 
        with    
            member this.Dispose () = 
                this.Dispose() 
                        
    interface IRpcServer 
        with 
            member this.Register method =
                this.Register method 
                
                
