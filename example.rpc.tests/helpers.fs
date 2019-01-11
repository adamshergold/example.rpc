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
        serde.TryRegisterAssembly typeof<JsonProxy>.Assembly |> ignore
        serde.TryRegisterAssembly typeof<Example.Rpc.RequestContext>.Assembly |> ignore
        serde.TryRegisterAssembly typeof<Mocks.EchoRequest>.Assembly |> ignore
        
        serde                 
        
    let DefaultSerde = 
        Serde() 
                
    let RoundTrip (contentType:string) (serde:ISerde) (v:obj) = 
    
        let ts = 
            match v with 
            | :? ITypeSerialisable as ts -> ts
            | _ -> failwithf "Provided type did not implement ITypeSerialisable"
            
        let bytes = 
            Example.Serialisation.Helpers.Serialise serde contentType v 
        
        let typeSerde =
            serde.TrySerdeBySystemType (contentType,v.GetType())
            
        if typeSerde.IsNone then
            failwithf "Unable to find type-serde for '%O'" (v.GetType())
            
        let typeName =
            typeSerde.Value.TypeName
                
        Example.Serialisation.Helpers.Deserialise serde contentType typeName bytes