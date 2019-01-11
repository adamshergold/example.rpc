namespace Example.Rpc.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Rpc
                                            
type SuccessShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
        
        Logging.CreateLogger options
           

    [<Fact>]
    member this.``PassSimpleTest`` () = 
    
        let v = 
            Mocks.EchoRequest.Make( "Hello" )
            
        let success = 
            Success.Make( v ) 
        
        let v' = 
            success.Content.Value.Value
        
        Assert.Equal( v, v' :?> Mocks.EchoRequest )
        
    [<Fact>]
    member this.``CheckAllConstructors`` () = 

        let v = 
            Mocks.EchoRequest.Make( "Hello" )
    
        let s = 
            Success.Make( v, Some( Info.Make( 0 ) ) ) 
        
        Assert.True( s.Info.IsSome )        
        
    [<Theory>]
    [<InlineData("json")>]
    [<InlineData("binary")>]
    member this.``CanSerialise`` (contentType:string) = 
    
        let sut = 
            
            let content = 
                Mocks.EchoRequest.Make( "Hello" )
    
            Success.Make( content, Some( Info.Make( 123 ) ) ) 
            
        let serialiser = 
            Helpers.DefaultSerde 
                        
        let sutRT = 
            Helpers.RoundTrip contentType serialiser sut
            
        Assert.Equal( sutRT, sut )                      
                         