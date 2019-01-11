namespace Example.Rpc.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Rpc
                                            
type InfoShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
        
        Logging.CreateLogger options
           

    [<Fact>]
    member this.``PassSimpleTest`` () = 
    
        let sut = 
            Info.Make( 123 )
            
        Assert.Equal( 123, sut.ElapsedTimeMilliseconds )
        
    [<Theory>]
    [<InlineData("json")>]
    [<InlineData("binary")>]

    member this.``CanSerialise`` (contentType:string) = 
    
        let sut = 
            Info.Make( 123 )
            
        let serialiser = 
            Helpers.DefaultSerde 
                        
        let sutRT = 
            Helpers.RoundTrip contentType serialiser sut 
            
        Assert.Equal( sutRT, sut )                