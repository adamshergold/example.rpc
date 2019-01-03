namespace Example.Rpc.Tests

open Microsoft.Extensions.Logging 

open Xunit
open Xunit.Abstractions 

open Example.Rpc
                                            
type InterfacesShould( oh: ITestOutputHelper ) = 

    let logger =
    
        let options = 
            { Logging.Options.Default with OutputHelper = Some oh }
        
        Logging.CreateLogger options
           
