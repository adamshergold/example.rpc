namespace Example.Rpc.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Rpc
                                            
type RequestShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
        
        Logging.CreateLogger options
           

    [<Theory>]
    [<InlineData("json")>]
    [<InlineData("binary")>]
    member this.``CanSerialise`` (contentType:string) = 
    
        let sut = 
        
            let context = 
                RequestContext.Make( "123", Some "tag", "me" )
                
            let inner = 
                Mocks.EchoRequest.Make( "Hello" )
                                    
            Request.Make( context, inner ) 
            
        let serialiser = 
            Helpers.DefaultSerde 
                        
        let sutRT = 
            Helpers.RoundTrip (Some contentType) serialiser sut
            
        Assert.Equal( sutRT, sut )             
        