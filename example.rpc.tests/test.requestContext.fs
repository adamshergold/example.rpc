namespace Example.Rpc.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Rpc
                                            
type RequestContextShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
        
        Logging.CreateLogger options
           

    [<Fact>]
    member this.``PassSimpleTest`` () = 
    
        let sut = 
            RequestContext.Make( "123", Some "tag", "me" )
            
        Assert.Equal( "123", sut.RequestId )
        Assert.Equal( Some "tag", sut.Tag ) 
        Assert.Equal( "me", sut.UserId )
        
    [<Fact>]
    member this.``CheckAllConstructors`` () = 
    
        let sut1 = 
            RequestContext.Make()
            
        Assert.True( sut1.RequestId.Length > 0 )
        Assert.True( sut1.Tag.IsNone )
        Assert.True( sut1.UserId.Length > 0 )
        
    [<Theory>]
    [<InlineData("json")>]
    [<InlineData("binary")>]
    member this.``CanSerialise`` (contentType:string) = 
    
        let sut = 
            RequestContext.Make( "123", Some "tag", "me" )
                
        let serialiser = 
            Helpers.DefaultSerde 
                        
        let sutRT = 
            Helpers.RoundTrip contentType serialiser sut
            
        Assert.Equal( sutRT, sut )              
        

