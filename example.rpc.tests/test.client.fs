namespace Example.Rpc.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Rpc

open Example.Serialisation 
open Example.Messaging 
open Example.Messaging.Rabbit 

                                            
type ClientShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with
                Level = LogLevel.Trace 
                OutputHelper = Some oh }
        
        Logging.CreateLogger options
           
    let serialiser = 
        Helpers.DefaultSerde 
                   
    [<Fact>]
    member this.``WorkForSimpleCaseOfLocalMethod`` () = 
    
        let simpleTestMethod (context:IRpcRequestContext,request:Mocks.EchoRequest) = 
            Mocks.EchoResult.Make( sprintf "Echo %s" request.Message )
            
        let sut =
        
            let options = 
                { ClientOptions.Default with Logger = Some logger }
                
            Client.Make( serialiser, options ) 
            
        sut.Register (Helpers.WrapMethod simpleTestMethod) 
            
        let context, request = 
            RequestContext.Make(), Mocks.EchoRequest.Make( "Hello!") 
                                
        let result = 
            sut.Call (context,request) |> Async.RunSynchronously 
          
        Assert.True( result.IsSuccess )
        