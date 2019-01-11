namespace Example.Rpc.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Rpc
                                            
type ErrorShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
        
        Logging.CreateLogger options
           

    [<Fact>]
    member this.``PassSimpleTest`` () = 
        
        let sut = 
            Error.Make( "the message", true )
            
        Assert.Equal( "the message", sut.Message ) 
        Assert.True( sut.Retryable )
        
    [<Fact>]
    member this.``TestAllConstructors`` () = 
        
        let sut = 
            Error.Make( "the message" )
             
        Assert.Equal( "the message", sut.Message ) 
        Assert.False( sut.Retryable )
        
    [<Theory>]
    [<InlineData("json")>]
    [<InlineData("binary")>]
    member this.``CanSerialise`` (contentType:string) = 
    
        let sut = 
            Error.Make( "the message" )
            
        let serialiser = 
            Helpers.DefaultSerde 
                        
        let sutRT = 
            Helpers.RoundTrip contentType serialiser sut 
                                
        Assert.Equal( sutRT, sut )                                  