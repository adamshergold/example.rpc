namespace Example.Rpc.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Rpc
                                            
type HelpersShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
        
        Logging.CreateLogger options
        
    [<Fact>]
    member this.``PassSimpleTest`` () = 
    
        let simpleTestMethod (context:IRpcRequestContext,request:Mocks.EchoRequest) = 
            Mocks.EchoResult.Make( sprintf "Echo %s" request.Message )
    
        let wrapped = 
            Helpers.WrapMethod simpleTestMethod 
            
        let context = 
            RequestContext.Make() 
                        
        let result = 
            wrapped (context,Mocks.EchoRequest.Make("Hello"))
                     
        Assert.True( result.IsSuccess )
        Assert.False( result.Success.Content.IsWrapped )    

        let v =
            result.Success.Content.Value        
                            
        Assert.True( v :? Mocks.EchoResult )
           
