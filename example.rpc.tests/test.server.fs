namespace Example.Rpc.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Rpc

open Example.Serialisation 
open Example.Messaging 
open Example.Messaging.Rabbit 

type MessagingCreator = {
    Name : string 
    Create : ISerde -> ILogger -> IMessaging
}
with 
    static member Make( name, create ) = 
        { Name = name; Create = create }

    override this.ToString() = this.Name 

                                            
type ServerShould( oh: ITestOutputHelper ) = 

    let stopWaitMilliseconds =
        250            

    let logger =
    
        let options = 
            { Logging.Options.Default with
                Level = LogLevel.Trace 
                OutputHelper = Some oh }
        
        Logging.CreateLogger options
           
    let serialiser = 
        Helpers.DefaultSerde 
                   
    static member Memory (serialiser:ISerde) (logger:ILogger) = 
    
        let options = 
            { MemoryMessagingOptions.Default with Logger = Some logger }
            
        MemoryMessaging.Make( serialiser, options ) 

    static member Rabbit (serialiser:ISerde) (logger:ILogger) = 
    
        let options = 
            { RabbitMessagingOptions.Default with 
                Logger = Some logger }
                //ContentType = "json" }
            
        RabbitMessaging.Make( serialiser, options ) 

    static member Messaging
    
        with get () = 
            seq { 
                yield [| MessagingCreator.Make( "memory", ServerShould.Memory ) |]
                //yield [| MessagingCreator.Make( "rabbit", ServerShould.Rabbit ) |]  
            }
        
        
        
    [<Theory>]
    [<MemberData("Messaging")>]
    member this.``WorkWhenMessagingIsPresent`` (v:MessagingCreator) = 
    
        use messaging = 
            v.Create serialiser logger
            
        messaging.Start()            
            
        let echo (context:IRpcRequestContext,request:Mocks.EchoRequest) = 
            Mocks.EchoResult.Make( sprintf "Echo %s" request.Message )

        let greeting (context:IRpcRequestContext,request:Mocks.GreetingRequest) = 
            Mocks.GreetingResult.Make( sprintf "Hello %s!" request.Name )
            
        use client =
            
            let recipient =
                messaging.CreateRecipient "client" "client"
                
            let messagingOptions = 
                { MessagingOptions.Default( recipient ) with Label = "server" }
                
            let options = { 
                ClientOptions.Default with 
                    Logger = Some logger
                    Messaging = Some messagingOptions 
                }
                    
            Client.Make( serialiser, options ) 
            
        client.Register (Helpers.WrapMethod echo) 
            
        use server = 

            let recipient =
                messaging.CreateRecipient "server" "server"
        
            let messagingOptions = 
                MessagingOptions.Default( recipient )
                
            let options = 
                ServerOptions.Make( Some logger, messagingOptions ) 
                    
            Server.Make( serialiser, options ) 
                                    
        server.Register (Helpers.WrapMethod greeting )
            
        let localContext, localRequest = 
            RequestContext.Make(), Mocks.EchoRequest.Make( "Hello!" ) 
                                
        let localResult = 
            client.Call (localContext,localRequest) |> Async.RunSynchronously 
          
        logger.LogInformation( "{Result}", localResult )
                  
        Assert.True( localResult.IsSuccess )        

        let remoteContext, remoteRequest = 
            RequestContext.Make(), Mocks.GreetingRequest.Make( "John Smith" ) 

        let remoteResult = 
            client.Call (remoteContext,remoteRequest) |> Async.RunSynchronously 
          
        logger.LogInformation( "{Result}", remoteResult )
                  
        Assert.True( remoteResult.IsSuccess )      
        
        let result = 
            remoteResult.Success.As<Mocks.GreetingResult>()
        
        Assert.Equal( "Hello John Smith!", result.Salutation )
        
        System.Threading.Thread.Sleep(stopWaitMilliseconds) 
        
        messaging.Stop()  
