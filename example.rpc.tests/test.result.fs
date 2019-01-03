namespace Example.Rpc.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Rpc
                                            
type ResultShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
        
        Logging.CreateLogger options
           

    [<Fact>]
    member this.``PassSimpleTest`` () = 
    
        let success = 
            Success.Make( Mocks.EchoRequest.Make( "Hello" ) )
            
        let sut = 
            Result.Make( success ) 
            
        Assert.True( sut.IsSuccess )
        
        
    [<Theory>]
    [<InlineData("json")>]
    [<InlineData("binary")>]

    member this.``CanSerialise-Success`` (contentType:string) = 
    
        let sut = 
            
            let success = 
                Success.Make( Mocks.EchoRequest.Make( "Hello" ) )
            
            Result.Make( success ) 
            
        let serialiser = 
            Helpers.DefaultSerde 
                        
        let sutRT = 
            Helpers.RoundTrip (Some contentType) serialiser sut
            
        Assert.Equal( sutRT, sut )            
        
    [<Theory>]
    [<InlineData("json")>]
    [<InlineData("binary")>]
    member this.``CanSerialise-Error`` (contentType:string) = 
    
        let sut = 
            
            let error = 
                Error.Make( "Eek!", false )
            
            Result.Make( error ) 
            
        let serialiser = 
            Helpers.DefaultSerde 
                        
        let sutRT = 
            Helpers.RoundTrip (Some contentType) serialiser sut
            
        Assert.Equal( sutRT, sut )                    