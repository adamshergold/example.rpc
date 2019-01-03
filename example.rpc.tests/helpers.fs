namespace Example.Rpc.Tests 

open Example.Serialisation 
open Example.Serialisation.Json

open Example.Messaging 

open Example.Rpc.Tests 

module Helpers = 
    
    let Serde () =
    
        let options =   
            SerdeOptions.Default
         
        let serde = 
            Serde.Make( options )
            
        serde.TryRegisterAssembly typeof<Envelope>.Assembly |> ignore
        //serde.TryRegisterAssembly typeof<BinaryProxy>.Assembly |> ignore
        serde.TryRegisterAssembly typeof<JsonProxy>.Assembly |> ignore
        serde.TryRegisterAssembly typeof<Example.Rpc.RequestContext>.Assembly |> ignore
        serde.TryRegisterAssembly typeof<Mocks.EchoRequest>.Assembly |> ignore
        
        serde                 
        
    let DefaultSerde = 
        Serde() 
                
    let RoundTrip (contentType:string option) (serialiser:ISerde) (v:obj) = 
    
        let ts = 
            match v with 
            | :? ITypeSerialisable as ts -> ts
            | _ -> failwithf "Provided type did not implement ITypeSerialisable"
            
        let bytes = 
            Example.Serialisation.Helpers.Serialise serialiser contentType v 
        
        let typeName = 
            serialiser.TypeName contentType ts.Type 
            
        Example.Serialisation.Helpers.Deserialise serialiser contentType typeName bytes